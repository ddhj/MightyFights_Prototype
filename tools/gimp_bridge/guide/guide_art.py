"""Pictures for the Unit Bridge artist guide (make_guide.py), made from the game's own unit sheets and
the bridge's own export code, so the guide shows exactly what the artist gets.

Re-run whenever the art changes; make_guide.py calls it."""
import os
import sys
import tempfile

from PIL import Image, ImageDraw

GUIDE = os.path.dirname(os.path.abspath(__file__))
BRIDGE = os.path.dirname(GUIDE)
sys.path.insert(0, BRIDGE)
import unit_sheet as us  # noqa: E402
import export_unit  # noqa: E402

OUT = os.path.join(GUIDE, 'art')
os.makedirs(OUT, exist_ok=True)
BG = (228, 232, 238, 255)
GRASS = (150, 168, 120, 255)
INK = (40, 46, 60, 255)
MID = (0, 160, 230, 255)
HALBERD = (os.path.join(us.SPRITE_DATA, 'Troopers', 'Halberd', 'Textures', 'fsteel.png'),
           os.path.join(us.SPRITE_DATA, 'Troopers', 'Halberd', 'HalberdArray.json'))


def save(name, im, scale=1):
    if scale != 1:
        im = im.resize((im.width * scale, im.height * scale), Image.NEAREST)
    im.save(os.path.join(OUT, name + '.png'), optimize=True)


def row_strip(sheet, act, bg=BG, highlight=None, numbers=True):
    """One action's frames side by side on the unit's canvas; highlight = {frame: colour}."""
    ix = dict(sheet.actions)[act]
    cw, ch = sheet.cell
    pad = 14 if numbers else 0
    im = Image.new('RGBA', (cw * len(ix), ch + pad), bg)
    d = ImageDraw.Draw(im)
    for k, i in enumerate(ix):
        if highlight and k in highlight:
            d.rectangle([k * cw, 0, k * cw + cw - 1, ch - 1], fill=highlight[k])
        im.alpha_composite(sheet.cell_image(i), (k * cw, 0))
        d.line([(k * cw, 0), (k * cw, ch + pad)], fill=(190, 198, 210, 255))
        if numbers:
            d.text((k * cw + cw // 2 - 6, ch + 1), '%02d' % k, fill=INK)
    return im


def flatten_ora(path, box=None):
    w, h, layers = us.read_ora(path)
    out = Image.new('RGBA', (w, h), BG)
    for L in reversed(layers):
        if not L['visible']:
            continue
        img = L['img']
        if L['opacity'] < 1:
            img = img.copy()
            img.putalpha(img.getchannel('A').point(lambda v, o=L['opacity']: int(v * o)))
        out.alpha_composite(img, (L['x'], L['y']))
    return out.crop(box) if box else out


peasant = us.UnitSheet('Peasant')
bandit = us.UnitSheet('Bandit')
halberd = us.UnitSheet('Halberd', paths=HALBERD)

# 1. hero: the Peasant's big critical chop, every other frame
full = row_strip(peasant, 'bigchop', bg=GRASS, numbers=False)
pw, ph = peasant.cell
picks = list(range(0, len(dict(peasant.actions)['bigchop']), 2))
hero = Image.new('RGBA', (pw * len(picks), ph), GRASS)
for n, k in enumerate(picks):
    hero.alpha_composite(full.crop((k * pw, 0, (k + 1) * pw, ph)), (n * pw, 0))
save('hero', hero, 2)

# 2. the file as GIMP shows it: the Bandit, first six rows, through the real exporter
tmp = tempfile.mkdtemp(prefix='unit_guide_')
export_unit.export_unit(bandit, export_unit.DEFAULT_SPARE, 'Bandit', tmp, False)
cw, ch = bandit.cell
save('canvas', flatten_ora(os.path.join(tmp, 'Bandit.ora'), (0, 0, 9 * cw, 6 * ch)))

# 3. the midline, as an ILLUSTRATION: one Peasant idle frame centred on the midline, and the same frame
#    shifted off it, each beside its mirror (what the game draws when the unit turns round)
def centred(img, dx=0):
    x0, _, x1, _ = img.getbbox()
    out = Image.new('RGBA', img.size, (0, 0, 0, 0))
    out.paste(img, (round(img.width / 2 - (x0 + x1) / 2) + dx, 0))
    return out

def mirrored_pair(cell):
    cw, ch = cell.size
    pair = Image.new('RGBA', (cw * 2 + 8, ch), (0, 0, 0, 0))
    for n, img in enumerate((cell, cell.transpose(Image.FLIP_LEFT_RIGHT))):
        panel = Image.new('RGBA', (cw, ch), GRASS)
        d = ImageDraw.Draw(panel)
        for y in range(0, ch, 4):
            d.point((cw // 2, y), fill=MID)
            d.point((cw // 2, y + 1), fill=MID)
        panel.alpha_composite(img)
        pair.alpha_composite(panel, (n * (cw + 8), 0))
    return pair

idle0 = peasant.cell_image(dict(peasant.actions)['idle'][0])
good, bad = mirrored_pair(centred(idle0)), mirrored_pair(centred(idle0, -24))
mid = Image.new('RGBA', (good.width * 2 + 24, good.height), BG)
mid.alpha_composite(good, (0, 0))
mid.alpha_composite(bad, (good.width + 24, 0))
save('midline', mid, 3)

# 4. hit frame: the Halberd's chop, whose Collision keyframe is on frame 04
hits = {k: (255, 214, 196, 255) for k, kf in halberd.keyframes('chop').items() if kf.get('Type') == us.HIT_KEY}
strip = row_strip(halberd, 'chop', highlight=hits)
d = ImageDraw.Draw(strip)
for k in hits:
    x = k * halberd.cell[0] + halberd.cell[0] - 10
    d.ellipse([x, 4, x + 6, 10], fill=us.HIT_COLOUR)
save('hit', strip, 2)

# 5. adding a frame: the Bandit's walk row with its spare cells, then with a new frame painted in
n = len(dict(bandit.actions)['walk'])
cols = n + 3
cw, ch = bandit.cell

def walk_row(extra):
    im = Image.new('RGBA', (cols * cw, ch + 14), BG)
    d = ImageDraw.Draw(im)
    for c in range(cols):
        spare = c >= n + extra
        d.rectangle([c * cw, 0, c * cw + cw - 1, ch - 1],
                    fill=(242, 244, 247, 255) if spare else ((210, 240, 205, 255) if c >= n else GRASS),
                    outline=(230, 120, 230, 255))
        d.text((c * cw + cw // 2 - 6, ch + 1), '%02d' % c, fill=INK if not spare else (160, 166, 178, 255))
    ix = dict(bandit.actions)['walk']
    for k, i in enumerate(ix):
        im.alpha_composite(bandit.cell_image(i), (k * cw, 0))
    if extra:
        im.alpha_composite(bandit.cell_image(ix[1]), (n * cw, 0))
    return im

before, after = walk_row(0), walk_row(1)
both = Image.new('RGBA', (before.width, before.height * 2 + 16), BG)
both.alpha_composite(before, (0, 0))
both.alpha_composite(after, (0, before.height + 16))
save('addframe', both, 2)

# 6. a new unit's blank template, with the Peasant faintly behind
export_unit.export_new('Goblin', peasant, None, export_unit.NEW_FRAMES, tmp, False)
pw, ph = peasant.cell
save('newunit', flatten_ora(os.path.join(tmp, 'Goblin.ora'), (0, 0, 8 * pw, 4 * ph)))

sizes = {f: os.path.getsize(os.path.join(OUT, f)) for f in sorted(os.listdir(OUT)) if f.endswith('.png')}
print(sizes)
