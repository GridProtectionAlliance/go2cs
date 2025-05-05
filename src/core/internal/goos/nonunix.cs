// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix
namespace go.@internal;

partial class goos_package {

public const bool IsUnix = false;

} // end goos_package
