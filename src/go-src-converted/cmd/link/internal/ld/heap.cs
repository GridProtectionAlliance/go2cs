// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:21:26 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\heap.go
using loader = go.cmd.link.@internal.loader_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

    // Min-heap implementation, for the deadcode pass.
    // Specialized for loader.Sym elements.
private partial struct heap { // : slice<loader.Sym>
}

private static void push(this ptr<heap> _addr_h, loader.Sym s) {
    ref heap h = ref _addr_h.val;

    h.val = append(h.val, s); 
    // sift up
    var n = len(h.val) - 1;
    while (n > 0) {
        var p = (n - 1) / 2; // parent
        if ((h.val)[p] <= (h.val)[n]) {
            break;
        }
        ((h.val)[n], (h.val)[p]) = ((h.val)[p], (h.val)[n]);        n = p;

    }

}

private static loader.Sym pop(this ptr<heap> _addr_h) {
    ref heap h = ref _addr_h.val;

    var r = (h.val)[0];
    var n = len(h.val) - 1;
    (h.val)[0] = (h.val)[n];
    h.val = (h.val)[..(int)n]; 

    // sift down
    nint i = 0;
    while (true) {
        nint c = 2 * i + 1; // left child
        if (c >= n) {
            break;
        }
        {
            var c1 = c + 1;

            if (c1 < n && (h.val)[c1] < (h.val)[c]) {
                c = c1; // right child
            }

        }

        if ((h.val)[i] <= (h.val)[c]) {
            break;
        }
        ((h.val)[i], (h.val)[c]) = ((h.val)[c], (h.val)[i]);        i = c;

    }

    return r;

}

private static bool empty(this ptr<heap> _addr_h) {
    ref heap h = ref _addr_h.val;

    return len(h.val) == 0;
}

} // end ld_package
