; Exports every top-level layer of a .xcf as its own PNG, named "<prefix><LayerName>.png",
; cropped to the layer's own bounds. Matches the loose "draw00.png"/"0.png" per-frame
; convention the sprite stitcher expects (tools/sprite_stitcher). Used on hand-animated
; multi-frame .xcf sources identified by scan_xcf_inventory.sh (e.g. bandit.xcf's draw00..
; draw10, sword_hero.xcf's 0..5) -- NOT intended for single-pose concept art, which this would
; just export as one big frame per composition layer.
;
; Each layer is copied into a FRESH, otherwise-empty image via gimp-layer-new-from-drawable
; before saving -- NOT duplicated into the source image and flattened there. The first version
; of this script did the latter, and every export came out byte-identical: gimp-image-flatten
; merges ALL visible layers in whatever image it's called on, so "isolating" one layer by
; duplicating it into the same multi-layer image and flattening just re-composited everything
; underneath it too. A brand-new single-layer image has nothing else to flatten in.
;
; Layers are read top-level only (see scan_layers.scm's header for why: gimp-item-get-children
; crashes this GIMP build on real legacy .xcf group layers). If a "frame" layer is itself a
; group, gimp-layer-new-from-drawable copies its flattened appearance, which is normally what
; you want for a single animation frame anyway.

(define (export-one-layer path filename layer-index out-dir prefix)
  (let* ((src-image (car (gimp-file-load RUN-NONINTERACTIVE path filename)))
         (src-layers (gimp-image-get-layers src-image))
         (src-layer (vector-ref (cadr src-layers) layer-index))
         (name (car (gimp-item-get-name src-layer)))
         (w (car (gimp-drawable-width src-layer)))
         (h (car (gimp-drawable-height src-layer)))
         (dest-image (car (gimp-image-new w h RGB)))
         (new-layer (car (gimp-layer-new-from-drawable src-layer dest-image))))
    (gimp-image-insert-layer dest-image new-layer 0 -1)
    (gimp-layer-set-offsets new-layer 0 0)
    ; the source layer's visibility carries over onto the copy -- many of these frame layers
    ; were saved hidden (the artist toggles one visible at a time while drawing), which made
    ; gimp-image-flatten fail outright with "no visible layer"
    (gimp-item-set-visible new-layer TRUE)
    (gimp-image-flatten dest-image)
    (file-png-save RUN-NONINTERACTIVE dest-image (car (gimp-image-get-active-drawable dest-image))
                    (string-append out-dir "/" prefix name ".png") name 0 9 1 1 1 1 1)
    (gimp-image-delete dest-image)
    (gimp-image-delete src-image)))

(define (export-frames path filename out-dir prefix)
  (let* ((probe-image (car (gimp-file-load RUN-NONINTERACTIVE path filename)))
         (n (car (gimp-image-get-layers probe-image))))
    (gimp-image-delete probe-image)
    (let loop ((i 0))
      (if (< i n)
          (begin
            (export-one-layer path filename i out-dir prefix)
            (loop (+ i 1)))))))
