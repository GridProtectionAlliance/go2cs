// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !aix && !darwin && !freebsd && !openbsd && !plan9 && !solaris && !wasip1
namespace go;

partial class runtime_package {

//go:wasmimport gojs runtime.nanotime1
internal static partial int64 nanotime1();

} // end runtime_package
