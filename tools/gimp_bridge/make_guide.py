"""Builds ARTIST_GUIDE.html -- the Unit Bridge field guide -- as ONE self-contained file.

    python tools/gimp_bridge/make_guide.py

It re-renders the pictures from the game's own unit sheets (guide/guide_art.py), embeds them as data
URIs into guide/guide.src.html, and fills the animation and unit tables from the sheets themselves.
build_bridge.py runs it and ships the result in the UnitBridge package. Same shape as SettlementARPG's
DollBridge guide (its tools/gimp_bridge/make_guide.py), whose stylesheet this page shares.

The "when the game plays it" column is prose, read off Trooper_Huristics.cs / Trooper_Actions.cs
(SetAnimationCriteria calls): check it against those when the trooper AI changes.
"""
import base64
import datetime
import html
import os
import re
import subprocess
import sys

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
GUIDE = os.path.join(HERE, "guide")
ART = os.path.join(GUIDE, "art")
OUT = os.path.join(HERE, "ARTIST_GUIDE.html")
sys.path.insert(0, HERE)
import unit_sheet as us  # noqa: E402

#### row -> (when the game plays it, plays once?)
WHEN = {
    "idle":    ("standing around, out of battle", False),
    "low":     ("a normal attack: one of these five, picked at random", True),
    "stab":    ("a normal attack (random pick)", True),
    "stick":   ("a normal attack (random pick)", True),
    "chop":    ("a normal attack (random pick)", True),
    "chopb":   ("a normal attack (random pick)", True),
    "punch":   ("in the sheet, but the game never asks for it at the moment", True),
    "bigchop": ("a critical hit, triple damage: this or lunge, at random", True),
    "lunge":   ("a critical hit, triple damage (random pick)", True),
    "parry":   ("blocking an incoming attack", True),
    "stance":  ("waiting in battle between actions", False),
    "ready":   ("standing ready for the fight to start", False),
    "pant":    ("catching its breath (the Halberd heals a little on one pant frame)", False),
    "victory": ("the battle is won", False),
    "death":   ("killed: this or deathb", True),
    "deathb":  ("killed (the other death)", True),
    "walk":    ("walking into position", False),
    "run":     ("charging into the fight", False),
    "flee":    ("running away", False),
}


def animations():
    p = us.UnitSheet("Peasant")
    rows = []
    for act, _ in p.actions:
        main, sub, inc = p.taxonomy[act]
        when, once = WHEN.get(act, ("", False))
        rows.append("    <tr><td><code>%s</code></td><td>%s/%s</td><td class=\"num\">%dms</td><td>%s</td><td>%s</td></tr>"
                    % (act, main, sub, inc, html.escape(when), "once" if once else "loops"))
    return "\n".join(rows)


def units():
    rows = []
    for name in us.find_units():
        s = us.UnitSheet(name)
        hits = sum(1 for fr in s.frames if (fr.get("KeyFrame") or {}).get("Type") == us.HIT_KEY)
        rows.append("    <tr><td><code>%s</code></td><td class=\"num\">%d×%d</td><td class=\"num\">%d</td>"
                    "<td class=\"num\">%d</td><td class=\"num\">%d</td></tr>"
                    % (name, s.cell[0], s.cell[1], len(s.actions), len(s.frames), hits))
    return "\n".join(rows)


def main():
    subprocess.run([sys.executable, os.path.join(GUIDE, "guide_art.py")], check=True, capture_output=True)
    page = open(os.path.join(GUIDE, "guide.src.html"), encoding="utf-8").read()

    def img(m):
        data = open(os.path.join(ART, m.group(1) + ".png"), "rb").read()
        return "data:image/png;base64," + base64.b64encode(data).decode("ascii")

    def size(m):
        w, h = Image.open(os.path.join(ART, m.group(2) + ".png")).size
        return str(w if m.group(1) == "W" else h)

    page = page.replace("{{ANIMATIONS}}", animations())
    page = page.replace("{{UNITS}}", units())
    page = page.replace("{{STAMP}}", datetime.date.today().isoformat())
    page = re.sub(r"\{\{(W|H):(\w+)\}\}", size, page)
    page = re.sub(r"\{\{IMG:(\w+)\}\}", img, page)
    left = re.findall(r"\{\{[^}]*\}\}", page)
    if left:
        raise SystemExit("unfilled placeholders: %s" % sorted(set(left)))
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        fh.write("<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n"
                 "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n")
        #### the source is a fragment (title + style first); a plain file needs its own head
        head_end = page.index("</style>") + len("</style>")
        fh.write(page[:head_end] + "\n<style>body{margin:0}img{max-width:100%}</style>\n</head>\n<body>\n")
        fh.write(page[head_end:] + "\n</body>\n</html>\n")
    print("wrote %s (%d KB)" % (OUT, os.path.getsize(OUT) // 1024))


if __name__ == "__main__":
    main()
