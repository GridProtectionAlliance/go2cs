// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips || mipsle
// +build mips mipsle

// package runtime -- go2cs converted at 2022 March 13 05:27:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_mipsx.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

// adjust Gobuf as if it executed a call to fn with context ctxt
// and then did an immediate Gosave.
private static void gostartcall(ptr<gobuf> _addr_buf, unsafe.Pointer fn, unsafe.Pointer ctxt) {
    ref gobuf buf = ref _addr_buf.val;

    if (buf.lr != 0) {
        throw("invalid use of gostartcall");
    }
    buf.lr = buf.pc;
    buf.pc = uintptr(fn);
    buf.ctxt = ctxt;
}

} // end runtime_package
