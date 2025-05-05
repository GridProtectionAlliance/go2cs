// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !noopt
namespace go.@internal;

partial class testenv_package {

// OptimizationOff reports whether optimization is disabled.
public static bool OptimizationOff() {
    return false;
}

} // end testenv_package
