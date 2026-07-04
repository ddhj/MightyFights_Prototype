; Non-destructive .xcf inventory scanner (Script-Fu, GIMP 2.10 batch mode). Reports canvas
; size and every top-level layer name via gimp-message (reliably flushed under redirected
; stdio in batch mode, unlike display/newline -- that silently produced no output at all in
; testing), and writes a flattened, size-capped PNG thumbnail so a human can classify contents
; visually. Used to sort found/recovered .xcf art by what's actually inside instead of
; filename guesses -- see docs/DESIGN_DIRECTION.md "Art pipeline".
;
; Deliberately does NOT recurse into layer groups: gimp-item-get-children crashes the
; script-fu plugin on this GIMP 2.10.38 Windows build for real legacy-format .xcf group
; layers (confirmed reproducible; a synthetic freshly-built group layer does not crash, so
; this is specific to how old .xcf files serialize groups, not a general API bug). The
; thumbnail substitutes for recursion -- a flattened composite shows what a group contains
; without needing to enumerate it.
;
; Driven one file per GIMP invocation (see Scan-XcfInventory.ps1) so a single bad file can't
; take down a whole batch run.

(define (report-layer layer depth)
  (let* ((name (car (gimp-item-get-name layer)))
         (is-group (car (gimp-item-is-group layer)))
         (w (car (gimp-drawable-width layer)))
         (h (car (gimp-drawable-height layer))))
    (gimp-message (string-append "  - " name
                                  " [" (number->string w) "x" (number->string h) "]"
                                  (if is-group " (group)" "")))))

(define (scan-one path filename thumb-dir max-dim)
  (let* ((image (car (gimp-file-load RUN-NONINTERACTIVE path filename)))
         (w (car (gimp-image-width image)))
         (h (car (gimp-image-height image)))
         (layers (gimp-image-get-layers image))
         (n (car layers))
         (ids (cadr layers))
         (flat-image (car (gimp-image-duplicate image)))
         (scale (min 1.0 (/ max-dim (max w h))))
         (thumb-w (max 1 (round (* w scale))))
         (thumb-h (max 1 (round (* h scale))))
         (thumb-path (string-append thumb-dir "/" filename ".png")))
    (gimp-message (string-append "=== " filename " ==="))
    (gimp-message (string-append "canvas " (number->string w) "x" (number->string h)
                                  ", " (number->string n) " top-level layers:"))
    (let loop ((i 0))
      (if (< i n)
          (begin (report-layer (vector-ref ids i) 1)
                 (loop (+ i 1)))))
    ; flatten a working duplicate (leaves the original untouched) and export a thumbnail
    (gimp-image-flatten flat-image)
    (gimp-image-scale flat-image thumb-w thumb-h)
    (file-png-save RUN-NONINTERACTIVE flat-image (car (gimp-image-get-active-drawable flat-image))
                    thumb-path filename 0 9 1 1 1 1 1)
    (gimp-image-delete flat-image)
    (gimp-image-delete image)))
