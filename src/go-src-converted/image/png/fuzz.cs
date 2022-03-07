// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gofuzz
// +build gofuzz

// package png -- go2cs converted at 2022 March 06 23:36:15 UTC
// import "image/png" ==> using png = go.image.png_package
// Original source: C:\Program Files\Go\src\image\png\fuzz.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;

namespace go.image;

public static partial class png_package {

public static nint Fuzz(slice<byte> data) => func((_, panic, _) => {
    var (cfg, err) = DecodeConfig(bytes.NewReader(data));
    if (err != null) {
        return 0;
    }
    if (cfg.Width * cfg.Height > 1e6F) {
        return 0;
    }
    var (img, err) = Decode(bytes.NewReader(data));
    if (err != null) {
        return 0;
    }
    CompressionLevel levels = new slice<CompressionLevel>(new CompressionLevel[] { DefaultCompression, NoCompression, BestSpeed, BestCompression });
    foreach (var (_, l) in levels) {
        ref bytes.Buffer w = ref heap(out ptr<bytes.Buffer> _addr_w);
        ptr<Encoder> e = addr(new Encoder(CompressionLevel:l));
        err = e.Encode(_addr_w, img);
        if (err != null) {
            panic(err);
        }
        var (img1, err) = Decode(_addr_w);
        if (err != null) {
            panic(err);
        }
        var got = img1.Bounds();
        var want = img.Bounds();
        if (!got.Eq(want)) {
            fmt.Printf("bounds0: %#v\n", want);
            fmt.Printf("bounds1: %#v\n", got);
            panic("bounds have changed");
        }
    }    return 1;

});

} // end png_package
