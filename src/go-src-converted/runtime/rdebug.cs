// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:26:44 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\rdebug.go
namespace go;

using _@unsafe_ = @unsafe_package;

public static partial class runtime_package { // for go:linkname

//go:linkname setMaxStack runtime/debug.setMaxStack
private static nint setMaxStack(nint @in) {
    nint @out = default;

    out = int(maxstacksize);
    maxstacksize = uintptr(in);
    return out;
}

//go:linkname setPanicOnFault runtime/debug.setPanicOnFault
private static bool setPanicOnFault(bool @new) {
    bool old = default;

    var _g_ = getg();
    old = _g_.paniconfault;
    _g_.paniconfault = new;
    return old;
}

} // end runtime_package
