// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (!linux && !freebsd && !darwin) || !cgo
// +build !linux,!freebsd,!darwin !cgo

// package plugin -- go2cs converted at 2022 March 06 23:36:30 UTC
// import "plugin" ==> using plugin = go.plugin_package
// Original source: C:\Program Files\Go\src\plugin\plugin_stubs.go
using errors = go.errors_package;

namespace go;

public static partial class plugin_package {

private static (Symbol, error) lookup(ptr<Plugin> _addr_p, @string symName) {
    Symbol _p0 = default;
    error _p0 = default!;
    ref Plugin p = ref _addr_p.val;

    return (null, error.As(errors.New("plugin: not implemented"))!);
}

private static (ptr<Plugin>, error) open(@string name) {
    ptr<Plugin> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(errors.New("plugin: not implemented"))!);
}

} // end plugin_package
