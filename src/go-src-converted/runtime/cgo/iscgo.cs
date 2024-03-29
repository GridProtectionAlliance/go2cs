// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The runtime package contains an uninitialized definition
// for runtime·iscgo. Override it to tell the runtime we're here.
// There are various function pointers that should be set too,
// but those depend on dynamic linker magic to get initialized
// correctly, and sometimes they break. This variable is a
// backup: it depends only on old C style static linking rules.

// package cgo -- go2cs converted at 2022 March 13 05:27:33 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\iscgo.go
namespace go.runtime;

using _@unsafe_ = @unsafe_package;

public static partial class cgo_package { // for go:linkname

//go:linkname _iscgo runtime.iscgo
private static bool _iscgo = true;

} // end cgo_package
