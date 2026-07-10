// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!linux && !freebsd && !darwin) || !cgo
namespace go;

using errors = errors_package;

partial class plugin_package {

internal static (Symbol, error) lookup(ж<Plugin> Ꮡp, @string symName) {
    ref var p = ref Ꮡp.Value;

    return (default!, errors.New("plugin: not implemented"u8));
}

internal static (ж<Plugin>, error) open(@string name) {
    return (default!, errors.New("plugin: not implemented"u8));
}

} // end plugin_package
