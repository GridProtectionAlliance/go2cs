// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !darwin
// +build !darwin

// package ld -- go2cs converted at 2022 March 13 06:35:06 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\outbuf_notdarwin.go
namespace go.cmd.link.@internal;

public static partial class ld_package {

private static void purgeSignatureCache(this ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

}

} // end ld_package
