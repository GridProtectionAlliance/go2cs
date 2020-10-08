// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.10

// package bidirule -- go2cs converted at 2020 October 08 05:01:52 UTC
// import "vendor/golang.org/x/text/secure/bidirule" ==> using bidirule = go.vendor.golang.org.x.text.secure.bidirule_package
// Original source: C:\Go\src\vendor\golang.org\x\text\secure\bidirule\bidirule9.0.0.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace secure
{
    public static partial class bidirule_package
    {
        private static bool isFinal(this ptr<Transformer> _addr_t)
        {
            ref Transformer t = ref _addr_t.val;

            if (!t.isRTL())
            {
                return true;
            }
            return t.state == ruleLTRFinal || t.state == ruleRTLFinal || t.state == ruleInitial;

        }
    }
}}}}}}
