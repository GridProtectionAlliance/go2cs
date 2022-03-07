// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux
// +build linux

// package runtime -- go2cs converted at 2022 March 06 22:11:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs_linux.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static System.UIntPtr sbrk0();

// Called from write_err_android.go only, but defined in sys_linux_*.s;
// declared here (instead of in write_err_android.go) for go vet on non-android builds.
// The return value is the raw syscall result, which may encode an error number.
//go:noescape
private static int access(ptr<byte> name, int mode);
private static int connect(int fd, unsafe.Pointer addr, int len);
private static int socket(int domain, int typ, int prot);

} // end runtime_package
