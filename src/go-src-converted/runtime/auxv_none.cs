// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux && !darwin && !dragonfly && !freebsd && !netbsd && !solaris
namespace go;

partial class runtime_package {

internal static void sysargs(int32 argc, ж<ж<byte>> Ꮡargv) {
}

} // end runtime_package
