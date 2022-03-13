// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package oserror defines errors values used in the os package.
//
// These types are defined here to permit the syscall package to reference them.

// package oserror -- go2cs converted at 2022 March 13 05:23:51 UTC
// import "internal/oserror" ==> using oserror = go.@internal.oserror_package
// Original source: C:\Program Files\Go\src\internal\oserror\errors.go
namespace go.@internal;

using errors = errors_package;

public static partial class oserror_package {

public static var ErrInvalid = errors.New("invalid argument");public static var ErrPermission = errors.New("permission denied");public static var ErrExist = errors.New("file already exists");public static var ErrNotExist = errors.New("file does not exist");public static var ErrClosed = errors.New("file already closed");

} // end oserror_package
