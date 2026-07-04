; Thin driver invoked via `gimp-console -b`. Reads its parameters from environment
; variables rather than the -b argument string itself -- Windows PowerShell's native-exe
; argument quoting mangles embedded double-quotes badly enough that a Scheme string literal
; built by interpolation is not reliable; env vars sidestep it entirely. See
; Scan-XcfInventory.ps1 for the invocation.
(load (getenv "SCAN_LIB"))
(scan-one (getenv "SCAN_PATH") (getenv "SCAN_FILENAME") (getenv "SCAN_THUMB_DIR")
          (string->number (getenv "SCAN_MAX_DIM")))
(gimp-quit 0)
