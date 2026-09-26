#!/usr/bin/env python3
"""Build the SENDABLE Unit Bridge: one folder with UnitBridge.exe and every unit sheet it reads.

    python tools/gimp_bridge/build_bridge.py --out publish/UnitBridge

Built the way SettlementARPG's DollBridge is (its tools/gimp_bridge/build_bridge.py): a FROZEN .exe
(no Python on the artist's machine) and exactly the files it reads, laid out as the repo in miniature so
every path in unit_sheet means the same thing (`unit_sheet.FROZEN`). The Desktop project's publish runs
this (the PublishUnitBridge target in MightyFights.Desktop.csproj).

    UnitBridge.exe, _internal/        the frozen bridge (PyInstaller --onedir, console)
    content/Sprite Data/.../<Unit>/   every single-texture unit's sheet (.png + Array.json)
    work/                             where exports land, and where the artist paints
    out/                              where an import writes, and the ONLY place it may
    README.txt                        for whoever receives it

IT IS NOT DONE UNTIL THE .EXE HAS ROUND-TRIPPED SOMETHING. Through the FROZEN exe, reading the PACKAGED
sheets: every unit exported and imported unchanged comes back pixel-equal with the same frames, taxonomy
and keyframes; one painted pixel changes exactly one frame; a frame painted into a spare column adds
exactly that frame; a hit mark becomes exactly one Collision keyframe; a new unit painted from the blank
template comes out as a complete unit; --out outside the package is refused; and, when GIMP is on this
machine, the unchanged round trip also passes through a real .xcf.
"""
import argparse
import json
import os
import shutil
import subprocess
import sys
import tempfile

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
TOOLS = os.path.dirname(HERE)
sys.path.insert(0, HERE)
import unit_sheet as us  # noqa: E402

PROBE = "Bandit"             # a stitched unit: its unchanged import must be JSON-identical

README = """UNIT BRIDGE -- paint MightyFights units in GIMP, and check they go back into the game.

WHAT YOU NEED
  GIMP 2.10 or 3.0 (gimp.org). Nothing else -- no Python, nothing to install for UnitBridge itself.
  Unzip this folder anywhere and keep it together: UnitBridge.exe reads the unit sheets in content\\.

RUN IT
  Double-click UnitBridge.exe. A window opens with a menu:
    1  Export a unit        -> writes a GIMP file into the work\\ folder here
    2  Start a NEW unit     -> writes a blank template (with a faint existing unit behind it, for scale)
    3  Check my painted files -> reads work\\, writes the rebuilt game sheet + animated previews into out\\
    4  List the units
  Or drag .ora / .xcf files onto UnitBridge.exe to check just those.

  From a command prompt in this folder, the same things:
    UnitBridge.exe list
    UnitBridge.exe export --unit Bandit --xcf
    UnitBridge.exe new --new Goblin --like Peasant                (--canvas 140x96 for a bigger unit)
    UnitBridge.exe import work\\ --dry-run                         (say what changed, write nothing)
    UnitBridge.exe import work\\

THE FILE
  One file per unit: work\\<Unit>.ora (and .xcf if you chose it; .ora opens in GIMP either way).
  Every ROW is one animation (idle, low, stab, ..., walk, run, flee), every COLUMN a frame. Each cell is
  the unit's frame size (Peasant 100x64, Bandit 100x70, ...). The magenta labels say the animation, its
  type and its speed (ms per frame); the numbers in each cell's corner are the frame numbers.

PAINT
  - Paint on the "art" layer, or add your own layers: every VISIBLE layer whose name does not start
    with # goes into the game, merged top to bottom. Hidden layers are left out.
  - ADD FRAMES by painting into the empty cells to the right of a row. REMOVE FRAMES by erasing a row's
    last cells completely. A row can't be empty: every animation needs at least one frame.
  - Draw the unit FACING LEFT. The game mirrors it to face right.
  - KEEP THE BODY ON THE DOTTED BLUE LINE (the middle of each cell). The game mirrors each frame across
    that line when the unit turns around, so a body that sits off it jumps sideways every turn.
  - HIT FRAMES: on the "#hit frames" layer, a red dot in a cell means "the attack lands on this frame".
    Move the dot to move the hit. Each attack row needs one, or that attack never deals damage.
  - Save as .xcf (File > Save) or .ora (File > Export As...), keeping the file name.
  Please do NOT: resize or crop the canvas, move whole rows up or down, or delete the "#guides" layer
  (it tells the bridge which unit the file is; if it's gone, the file name has to be the unit name).
  Layer opacity is kept; blend modes other than Normal are not. Layers starting with # are helpers and
  never go into the game, so add your own "#notes" layer freely.

CHECK IT
  Menu 3 (or "UnitBridge.exe import work\\") reads everything in work\\, says what changed row by row, warns
  about anything the game will get wrong (a missing hit frame, a body off the midline), and writes the
  rebuilt sheet into out\\ and one looping preview GIF per animation into out\\_preview\\<Unit>\\, at the
  game's own speed. It only ever writes into out\\ inside this folder. If it says ERROR, fix what it says,
  or tell the developer what it said.

  .xcf files are read through GIMP itself, so checking a .xcf needs GIMP installed. UnitBridge looks for
  gimp-console in %LOCALAPPDATA%\\Programs\\GIMP 2 (or GIMP 3), C:\\Program Files\\GIMP 2 (or 3) and the PATH;
  set the GIMP_CONSOLE environment variable to the full path of gimp-console-*.exe if it is elsewhere.
  .ora files need nothing but UnitBridge.

SEND BACK
  The .ora / .xcf files you painted (from work\\). Those are the source of the edit; the developer imports
  them. If you use GIMP 3, send a .ora as well -- GIMP 2.10 may not open a GIMP 3 .xcf.
  The out\\ sheets and previews are only for your own check; you do not need to send them.

WHAT ISN'T HERE
  The Halberd footman: its one layout is shared by 14 recoloured textures, so it can't be repainted
  one texture at a time yet. Ask the developer if you want to work on it.

Built: {stamp}
"""


def run(cmd, ok=True, **kw):
    print('  $ ' + ' '.join(str(c) for c in cmd))
    got = subprocess.run(cmd, capture_output=True, text=True, stdin=subprocess.DEVNULL, **kw)
    if ok and got.returncode != 0:
        print(got.stdout)
        print(got.stderr)
        sys.exit('!! failed (%d): %s' % (got.returncode, ' '.join(str(c) for c in cmd)))
    return got


def files_under(root):
    return sorted(os.path.relpath(os.path.join(d, f), root) for d, _, fs in os.walk(root) for f in fs)


def empty(folder):
    shutil.rmtree(folder, ignore_errors=True)
    os.makedirs(folder)


def load_doc(path):
    with open(path, 'r', encoding='utf-8-sig') as f:
        return json.load(f)


def compare(unit, a_root, b_root):
    """(frames differing, doc a, doc b) between a unit in two roots."""
    a, b = us.UnitSheet(unit, a_root), us.UnitSheet(unit, b_root)
    names_a = [f['filename'] for f in a.frames]
    names_b = [f['filename'] for f in b.frames]
    diff = [n for n in sorted(set(names_a) | set(names_b))
            if n not in names_a or n not in names_b
            or not us.same_pixels(a.cell_image(names_a.index(n)), b.cell_image(names_b.index(n)))]
    return diff, a.doc, b.doc


def edit_ora(src, dst, fn):
    """Rewrite an .ora with fn(name, img) -> img applied to every layer."""
    w, h, layers = us.read_ora(src)
    stack = [(L['name'], fn(L['name'], L['img']), L['visible'], L['opacity']) for L in reversed(layers)]
    us.write_ora(dst, (w, h), stack, stack[0][1])


def build(out, stamp):
    out = os.path.abspath(out)
    if os.path.exists(out):
        shutil.rmtree(out)
    work = tempfile.mkdtemp(prefix='unit_bridge_build_')

    print('1. freeze')
    run([sys.executable, '-m', 'PyInstaller', '--noconfirm', '--onedir', '--console', '--name', 'UnitBridge',
         '--paths', HERE, '--paths', os.path.join(TOOLS, 'sprite_stitcher'),
         '--distpath', work, '--workpath', os.path.join(work, 'build'),
         '--specpath', work, '--log-level', 'WARN', os.path.join(HERE, 'unit_bridge.py')])
    shutil.copytree(os.path.join(work, 'UnitBridge'), out)
    exe = os.path.join(out, 'UnitBridge.exe')

    print('2. the unit sheets, in the repo\'s layout')
    pkg_sprites = os.path.join(out, 'content', 'Sprite Data')
    units = us.find_units()
    for name, rel in units.items():
        png, js, _ = us.unit_paths(name, rel=rel)
        dest = os.path.join(pkg_sprites, rel)
        os.makedirs(dest)
        shutil.copy2(png, dest)
        shutil.copy2(js, dest)
    os.makedirs(os.path.join(out, 'work'))
    os.makedirs(os.path.join(out, 'out'))
    with open(os.path.join(out, 'README.txt'), 'w', encoding='utf-8') as fh:
        fh.write(README.format(stamp=stamp))
    print('     %d units: %s' % (len(units), ', '.join(units)))
    before = files_under(out)
    pkg_work, pkg_out = os.path.join(out, 'work'), os.path.join(out, 'out')

    print('3. the EXE round-trips, reading only the package')
    got = run([exe, 'list'], cwd=out)
    listed = [line.split()[0] for line in got.stdout.splitlines() if line.strip()]
    if listed != list(units):
        sys.exit('!! the frozen exe lists %s, the repo has %s' % (listed, list(units)))
    print('     list: all %d units' % len(units))

    #### every unit, unchanged: same frames, same taxonomy, same keyframes, pixel-equal
    for name in units:
        run([exe, 'export', '--unit', name], cwd=out)
    empty(pkg_out)
    run([exe, 'import', 'work', '--all', '--no-preview'], cwd=out)
    for name in units:
        diff, a, b = compare(name, pkg_sprites, pkg_out)
        if diff:
            sys.exit('!! unchanged %s came back with %d frame(s) different, e.g. %s' % (name, len(diff), diff[:3]))
        if a['ActionTypes'] != b['ActionTypes'] or \
                [f.get('KeyFrame') for f in a['frames']] != [f.get('KeyFrame') for f in b['frames']]:
            sys.exit('!! unchanged %s came back with a different taxonomy or keyframes' % name)
        rotated = any(f.get('rotated') for f in a['frames'])
        if not rotated and a != b:
            sys.exit('!! unchanged %s (a stitched sheet) came back with a different JSON' % name)
    print('     unchanged: %d units pixel-equal, taxonomy and keyframes intact, stitched JSON identical' % len(units))

    ora = os.path.join(pkg_work, PROBE + '.ora')
    probe = us.UnitSheet(PROBE, pkg_sprites)
    cw, ch = probe.cell
    rows = [a for a, _ in probe.actions]
    counts = {a: len(ix) for a, ix in probe.actions}

    def probe_import(label, fn, expect_diff):
        edited = os.path.join(work, label, PROBE + '.ora')
        os.makedirs(os.path.dirname(edited))
        edit_ora(ora, edited, fn)
        empty(pkg_out)
        run([exe, 'import', edited], cwd=out)
        written = files_under(pkg_out)
        want = [os.path.join(probe.rel, PROBE + '.png'), os.path.join(probe.rel, PROBE + 'Array.json')]
        if sorted(f for f in written if not f.startswith('_preview')) != sorted(want):
            sys.exit('!! %s wrote %s, expected only %s' % (label, written, want))
        diff, _, doc = compare(PROBE, pkg_sprites, pkg_out)
        if diff != expect_diff:
            sys.exit('!! %s changed %s, expected exactly %s' % (label, diff[:6], expect_diff))
        return doc

    #### ONE PIXEL on walk frame 01
    r, c, px = rows.index('walk'), 1, (50, 40)
    cell = probe.cell_image(probe.actions[r][1][c])
    colour = (201, 33, 77, 255) if cell.getpixel(px) != (201, 33, 77, 255) else (202, 33, 77, 255)

    def one_pixel(name, img):
        if name.startswith('art:'):
            img = img.copy()
            img.putpixel((c * cw + px[0], r * ch + px[1]), colour)
        return img
    doc = probe_import('pixel', one_pixel, ['walk01.png'])
    back = us.UnitSheet(PROBE, pkg_out)
    if back.cell_image(back.frames.index(next(f for f in back.frames if f['filename'] == 'walk01.png'))).getpixel(px) != colour:
        sys.exit('!! the painted pixel did not come back on walk01')
    if not os.path.isfile(os.path.join(pkg_out, '_preview', PROBE, 'walk.gif')):
        sys.exit('!! no preview GIF for walk')
    print('     one pixel: only walk01 changed (1 of %d frames); previews written' % len(probe.frames))

    #### ADD A FRAME: the walk row's last frame copied into its first spare column
    n = counts['walk']
    added = 'walk%02d.png' % n

    def add_frame(name, img):
        if name.startswith('art:'):
            img = img.copy()
            img.paste(img.crop(((n - 1) * cw, r * ch, n * cw, (r + 1) * ch)), (n * cw, r * ch))
        return img
    doc = probe_import('addframe', add_frame, [added])
    got_n = sum(1 for f in doc['frames'] if us.action_of(f['filename']) == 'walk')
    if got_n != n + 1:
        sys.exit('!! adding a frame gave walk %d frames, expected %d' % (got_n, n + 1))
    print('     add a frame: walk %d -> %d frames, nothing else changed' % (n, n + 1))

    #### A HIT MARK on chop frame 02 -> exactly one Collision keyframe
    hr, hc = rows.index('chop'), 2

    def hit(name, img):
        if name.lower().startswith(us.HIT_TAG):
            img = img.copy()
            img.paste(us.HIT_COLOUR, (hc * cw + cw - 10, hr * ch + 4, hc * cw + cw - 4, hr * ch + 10))
        return img
    doc = probe_import('hit', hit, [])
    kfs = [(f['filename'], f['KeyFrame']) for f in doc['frames'] if 'KeyFrame' in f]
    if kfs != [('chop02.png', {'Type': us.HIT_KEY, 'oData': ''})]:
        sys.exit('!! one hit mark gave keyframes %s' % kfs)
    print('     hit mark: exactly one Collision keyframe, on chop02')

    #### A NEW UNIT, painted from its template: two frames per row, copied from the reference
    run([exe, 'new', '--new', 'ProofUnit', '--like', 'Peasant', '--frames', '4'], cwd=out)
    tmpl = os.path.join(pkg_work, 'ProofUnit.ora')
    w, h, layers = us.read_ora(tmpl)
    ref = next(L['img'] for L in layers if L['name'].startswith('#reference'))
    pw, ph = us.UnitSheet('Peasant', pkg_sprites).cell

    def paint_new(name, img):
        if name.startswith('art:'):
            img = img.copy()
            for rr in range(h // ph):
                img.paste(ref.crop((0, rr * ph, 2 * pw, (rr + 1) * ph)), (0, rr * ph))
        return img
    newfile = os.path.join(work, 'new', 'ProofUnit.ora')
    os.makedirs(os.path.dirname(newfile))
    edit_ora(tmpl, newfile, paint_new)
    empty(pkg_out)
    run([exe, 'import', newfile], cwd=out)
    peasant = us.UnitSheet('Peasant', pkg_sprites)
    made = us.UnitSheet('ProofUnit', pkg_out)
    if made.taxonomy != peasant.taxonomy or [a for a, _ in made.actions] != [a for a, _ in peasant.actions]:
        sys.exit('!! the new unit does not carry the Peasant\'s actions and timings')
    for act, ix in made.actions:
        pix = dict(peasant.actions)[act]
        want = min(2, len(pix))
        if len(ix) != want or not all(us.same_pixels(made.cell_image(ix[k]), peasant.cell_image(pix[k])) for k in range(want)):
            sys.exit('!! new unit row %s: %d frames or pixels wrong' % (act, len(ix)))
    print('     new unit: %d actions, frames equal to what was painted, loads as a unit' % len(made.actions))

    #### the package's one promise about writing: out\ inside the folder, never content\, never elsewhere
    for bad in (['--out', os.path.join(work, 'escape')], ['--out', 'content\\Sprite Data'], ['--write-content']):
        res = run([exe, 'import', ora] + bad, ok=False, cwd=out)
        if res.returncode == 0:
            sys.exit('!! the frozen import accepted %s -- it must write inside out\\ only' % ' '.join(bad))
    if os.path.exists(os.path.join(work, 'escape')):
        sys.exit('!! the frozen import wrote outside the package')
    print('     refused: --out outside the package, --out into content\\, --write-content')

    if us.find_gimp():
        empty(pkg_work)
        run([exe, 'export', '--unit', PROBE, '--xcf'], cwd=out)
        os.remove(ora)                      # so the import reads the .xcf, not the .ora beside it
        empty(pkg_out)
        run([exe, 'import', os.path.join('work', PROBE + '.xcf'), '--all', '--no-preview'], cwd=out)
        diff, a, b = compare(PROBE, pkg_sprites, pkg_out)
        if diff or a != b:
            sys.exit('!! unchanged .xcf via GIMP came back different: %s' % diff[:5])
        print('     .xcf via GIMP: unchanged round trip, JSON identical')
    else:
        print('     .xcf: SKIPPED -- no GIMP on this machine, so the .xcf path is unproven in this build')

    #### and it leaves the package as it found it
    empty(pkg_work)
    empty(pkg_out)
    for d, subs, _ in os.walk(out):
        if '__pycache__' in subs:
            shutil.rmtree(os.path.join(d, '__pycache__'))
    if files_under(out) != before:
        extra = sorted(set(files_under(out)) ^ set(before))
        sys.exit('!! the proof left the package different: %s' % extra[:10])
    shutil.rmtree(work, ignore_errors=True)

    size = sum(os.path.getsize(os.path.join(d, f)) for d, _, fs in os.walk(out) for f in fs) / 1e6
    print('OK  %s  (%.0f MB)' % (out, size))


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('--out', required=True)
    ap.add_argument('--stamp', default='')
    a = ap.parse_args()
    build(a.out, a.stamp)
