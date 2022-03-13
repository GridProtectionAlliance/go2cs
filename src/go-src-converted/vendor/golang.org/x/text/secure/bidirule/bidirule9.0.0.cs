// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !go1.10
// +build !go1.10

// package bidirule -- go2cs converted at 2022 March 13 06:46:36 UTC
// import "vendor/golang.org/x/text/secure/bidirule" ==> using bidirule = go.vendor.golang.org.x.text.secure.bidirule_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\secure\bidirule\bidirule9.0.0.go
namespace go.vendor.golang.org.x.text.secure;

public static partial class bidirule_package {

private static bool isFinal(this ptr<Transformer> _addr_t) {
    ref Transformer t = ref _addr_t.val;

    if (!t.isRTL()) {
        return true;
    }
    return t.state == ruleLTRFinal || t.state == ruleRTLFinal || t.state == ruleInitial;
}

} // end bidirule_package
