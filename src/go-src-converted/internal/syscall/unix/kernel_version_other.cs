// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux && !solaris
namespace go.@internal.syscall;

partial class unix_package {

public static (nint major, nint minor) KernelVersion() {
    nint major = default!;
    nint minor = default!;

    return (0, 0);
}

} // end unix_package
