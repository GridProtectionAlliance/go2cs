// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gccgo
// +build gccgo

// package goroot -- go2cs converted at 2022 March 06 22:41:26 UTC
// import "internal/goroot" ==> using goroot = go.@internal.goroot_package
// Original source: C:\Program Files\Go\src\internal\goroot\gccgo.go
using os = go.os_package;
using filepath = go.path.filepath_package;

namespace go.@internal;

public static partial class goroot_package {

    // IsStandardPackage reports whether path is a standard package,
    // given goroot and compiler.
public static bool IsStandardPackage(@string goroot, @string compiler, @string path) => func((_, panic, _) => {
    switch (compiler) {
        case "gc": 
            var dir = filepath.Join(goroot, "src", path);
            var (_, err) = os.Stat(dir);
            return err == null;
            break;
        case "gccgo": 
            return stdpkg[path];
            break;
        default: 
            panic("unknown compiler " + compiler);
            break;
    }

});

} // end goroot_package
