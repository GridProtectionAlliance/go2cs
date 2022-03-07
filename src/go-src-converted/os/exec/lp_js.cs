// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package exec -- go2cs converted at 2022 March 06 22:14:24 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Program Files\Go\src\os\exec\lp_js.go
using errors = go.errors_package;

namespace go.os;

public static partial class exec_package {

    // ErrNotFound is the error resulting if a path search failed to find an executable file.
public static var ErrNotFound = errors.New("executable file not found in $PATH");

// LookPath searches for an executable named file in the
// directories named by the PATH environment variable.
// If file contains a slash, it is tried directly and the PATH is not consulted.
// The result may be an absolute path or a path relative to the current directory.
public static (@string, error) LookPath(@string file) {
    @string _p0 = default;
    error _p0 = default!;
 
    // Wasm can not execute processes, so act as if there are no executables at all.
    return ("", error.As(addr(new Error(file,ErrNotFound))!)!);

}

} // end exec_package
