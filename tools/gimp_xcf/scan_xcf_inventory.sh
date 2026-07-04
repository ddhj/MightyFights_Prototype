#!/usr/bin/env bash
# Non-destructive inventory scan of a folder of .xcf files: canvas size, top-level layer
# names, and a flattened PNG thumbnail per file, so art can be classified by what's actually
# inside it instead of guessed from filenames. See docs/DESIGN_DIRECTION.md "Art pipeline".
#
# Bash, not PowerShell: gimp-console reliably runs and exits to completion when launched from
# a bash/MSYS shell. The same invocation via PowerShell's Start-Process (with or without output
# redirection) OR the native `&` call operator reproducibly hung after the batch script had
# already finished its work (confirmed via step-by-step gimp-message markers: execution reaches
# gimp-quit, then the script-fu.exe helper subprocess logs a `gimp_wire_read(): error` and the
# whole process never exits) -- specific to how GIMP's plugin-wire shutdown interacts with a
# real Win32 console host, not a bug in this script. Runs one gimp-console invocation per file
# (a single malformed/huge file can't take down the whole batch), each capped by a timeout.
#
# Usage: scan_xcf_inventory.sh <source_dir> <out_dir> [gimp_console_path] [max_thumb_dim] [timeout_s]

set -u

SRC_DIR="${1:?usage: scan_xcf_inventory.sh <source_dir> <out_dir> [gimp_console_path] [max_thumb_dim] [timeout_s]}"
OUT_DIR="${2:?usage: scan_xcf_inventory.sh <source_dir> <out_dir> [gimp_console_path] [max_thumb_dim] [timeout_s]}"
GIMP="${3:-$HOME/AppData/Local/Programs/GIMP 2/bin/gimp-console-2.10.exe}"
MAX_DIM="${4:-400}"
TIMEOUT_S="${5:-60}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCAN_LIB="$(cygpath -m "$SCRIPT_DIR/scan_layers.scm" 2>/dev/null || echo "$SCRIPT_DIR/scan_layers.scm")"
DRIVER="$(cygpath -m "$SCRIPT_DIR/scan_driver.scm" 2>/dev/null || echo "$SCRIPT_DIR/scan_driver.scm")"
THUMB_DIR="$OUT_DIR/thumbs"
REPORT="$OUT_DIR/report.txt"

if [ ! -f "$GIMP" ]; then
	echo "gimp-console not found at '$GIMP'. Pass it as the 3rd argument." >&2
	exit 1
fi

mkdir -p "$THUMB_DIR"
: > "$REPORT"

export SCAN_LIB
export SCAN_THUMB_DIR="$(cygpath -m "$THUMB_DIR" 2>/dev/null || echo "$THUMB_DIR")"
export SCAN_MAX_DIM="$MAX_DIM"

count=0
failed=()

# NUL-delimited find+read: the only fully space/special-character-safe enumeration in bash.
# (A naive `for f in $(ls *.xcf)` word-splits on spaces -- broke on "camp sketch.xcf" during
# testing. This does not.)
while IFS= read -r -d '' f; do
	count=$((count + 1))
	base="$(basename "$f")"
	echo "[$count] $base"

	export SCAN_PATH="$(cygpath -m "$f" 2>/dev/null || echo "$f")"
	export SCAN_FILENAME="$base"

	if ! timeout "$TIMEOUT_S" "$GIMP" -i -d -f -b "(load \"$DRIVER\")" >> "$REPORT" 2>&1; then
		echo "  FAILED/TIMEOUT (exit $?)"
		failed+=("$base")
		echo "  -> FAILED/TIMEOUT" >> "$REPORT"
	fi
	echo "" >> "$REPORT"
done < <(find "$SRC_DIR" -maxdepth 1 -iname "*.xcf" -print0)

echo ""
echo "Scanned $count files. Report: $REPORT ; thumbnails: $THUMB_DIR"
if [ "${#failed[@]}" -gt 0 ]; then
	echo "Failed/timed out (${#failed[@]}):"
	printf '  %s\n' "${failed[@]}"
fi
