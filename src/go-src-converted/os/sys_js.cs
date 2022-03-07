// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package os -- go2cs converted at 2022 March 06 22:13:52 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sys_js.go


namespace go;

public static partial class os_package {

    // supportsCloseOnExec reports whether the platform supports the
    // O_CLOEXEC flag.
private static readonly var supportsCloseOnExec = false;


} // end os_package
