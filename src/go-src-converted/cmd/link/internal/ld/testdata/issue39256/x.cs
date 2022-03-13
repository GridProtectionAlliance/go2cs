// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:35:38 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue39256\x.go
namespace go;

using _@unsafe_ = @unsafe_package;


//go:cgo_import_dynamic libc_getpid getpid "libc.so"
//go:cgo_import_dynamic libc_kill kill "libc.so"
//go:cgo_import_dynamic libc_close close "libc.so"
//go:cgo_import_dynamic libc_open open "libc.so"

//go:cgo_import_dynamic _ _ "libc.so"

public static partial class main_package {

private static void trampoline();

private static void Main() {
    trampoline();
}

} // end main_package
