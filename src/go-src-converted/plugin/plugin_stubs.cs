// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux,!darwin !cgo

// package plugin -- go2cs converted at 2020 August 29 10:11:11 UTC
// import "plugin" ==> using plugin = go.plugin_package
// Original source: C:\Go\src\plugin\plugin_stubs.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class plugin_package
    {
        private static (object, error) lookup(ref Plugin p, @string symName)
        {
            return (null, errors.New("plugin: not implemented"));
        }

        private static (ref Plugin, error) open(@string name)
        {
            return (null, errors.New("plugin: not implemented"));
        }
    }
}
