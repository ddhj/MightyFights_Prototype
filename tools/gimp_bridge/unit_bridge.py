#!/usr/bin/env python3
"""UnitBridge: the GIMP bridge as ONE program, for the packaged UnitBridge.exe (build_bridge.py).

    UnitBridge.exe                               a menu (what a double-click gets)
    UnitBridge.exe list                          the units
    UnitBridge.exe export --unit Bandit [--xcf]  -> work\\ beside the exe
    UnitBridge.exe new --new Goblin --like Peasant
    UnitBridge.exe import work\\ [--dry-run]     -> out\\ beside the exe
    UnitBridge.exe <file.ora> [<file.xcf> ...]   drag files onto the exe: imports them

One exe with subcommands (as SettlementARPG's DollBridge.exe): export and import share unit_sheet and the
whole Pillow/numpy runtime, and the artist gets one thing to double-click. The subcommands hand straight to
`export_unit.main()` / `import_unit.main()` -- the same code the developer runs from source.

Run from source it works too: `python tools/gimp_bridge/unit_bridge.py list`.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unit_sheet as us  # noqa: E402
import export_unit  # noqa: E402
import import_unit  # noqa: E402

USAGE = __doc__.split("\n\n")[1]


def call(entry, prog, argv):
    """Run one CLI's main() as if it were invoked with argv. Returns its exit code."""
    saved = sys.argv
    sys.argv = [prog] + list(argv)
    try:
        entry()
        return 0
    except SystemExit as e:
        if e.code in (None, 0):
            return 0
        if not isinstance(e.code, int):
            print(e.code)
        return e.code if isinstance(e.code, int) else 1
    finally:
        sys.argv = saved


def pause():
    try:
        input("\npress Enter to close ")
    except (EOFError, KeyboardInterrupt):
        pass


def ask(prompt, default=""):
    try:
        got = input(prompt).strip()
    except (EOFError, KeyboardInterrupt):
        return None
    return got or default


def pick_unit(prompt, default=None):
    units = list(us.find_units())
    for n, u in enumerate(units, 1):
        print(f"  {n}  {u}")
    got = ask(prompt)
    if not got and default:
        return default
    if not got or not got.isdigit() or not 1 <= int(got) <= len(units):
        print("no such unit")
        return None
    return units[int(got) - 1]


def xcf_flag(gimp):
    if not gimp:
        print("(GIMP not found, so .ora only -- GIMP opens .ora directly)")
        return []
    yes = (ask(f"also save .xcf (GIMP found: {gimp})? [Y/n] ", "y") or "y").lower().startswith("y")
    return ["--xcf"] if yes else []


def menu():
    work = os.path.join(us.HERE, "work")
    gimp = us.find_gimp()
    while True:
        print("\nUNIT BRIDGE -- paint MightyFights units in GIMP   (new here? open ARTIST_GUIDE.html first)")
        print("  1  Export a unit to a GIMP file            (writes into work\\)")
        print("  2  Start a NEW unit                        (writes a blank template into work\\)")
        print("  3  Check my painted files                  (reads work\\, writes out\\)")
        print("  4  List the units")
        print("  q  Quit")
        pick = ask("> ")
        if pick is None or pick.lower() in ("q", "quit", "exit"):
            return
        if pick == "1":
            unit = pick_unit("which unit (number)? ")
            if unit:
                call(export_unit.main, "export", ["--unit", unit] + xcf_flag(gimp))
        elif pick == "2":
            name = ask("name for the new unit (letters and digits, e.g. Goblin)? ")
            if not name:
                continue
            print("which existing unit should it copy the animation list, timings and size from?")
            like = pick_unit("(number, Enter for Peasant)? ", "Peasant")
            if not like:
                continue
            argv = ["--new", name, "--like", like]
            size = ask("frame size WxH (Enter to use that unit's size)? ")
            if size:
                argv += ["--canvas", size]
            call(export_unit.main, "export", argv + xcf_flag(gimp))
        elif pick == "3":
            if not os.path.isdir(work) or not any(f.lower().endswith((".ora", ".xcf")) for f in os.listdir(work)):
                print("nothing in work\\ yet -- export a unit first (1 or 2)")
                continue
            call(import_unit.main, "import", [work])
        elif pick == "4":
            call(export_unit.main, "export", ["--list"])


def main(argv):
    if not argv:
        try:
            menu()
        except Exception as e:                      # a double-clicked window must not vanish on a crash
            print(f"\nUnitBridge stopped: {type(e).__name__}: {e}")
            pause()
            return 1
        return 0
    cmd, rest = argv[0].lower(), argv[1:]
    if cmd in ("-h", "--help", "help", "/?"):
        print(USAGE)
        return 0
    if cmd == "list":
        return call(export_unit.main, "export", ["--list"] + rest)
    if cmd in ("export", "new"):
        return call(export_unit.main, "export", rest)
    if cmd == "import":
        return call(import_unit.main, "import", rest)
    #### files dropped onto the exe arrive as bare paths
    if all(os.path.exists(a) for a in argv):
        code = call(import_unit.main, "import", argv)
        pause()
        return code
    print(f"unknown command '{argv[0]}'\n\n{USAGE}")
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
