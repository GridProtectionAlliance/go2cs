// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
global using syscallErrorType = go.syscall_package.Errno;

namespace go;

using syscall = syscall_package;

partial class os_package {

} // end os_package
