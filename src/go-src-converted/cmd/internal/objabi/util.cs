// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2022 March 13 05:43:23 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Program Files\Go\src\cmd\internal\objabi\util.go
namespace go.cmd.@internal;

using fmt = fmt_package;
using strings = strings_package;

using buildcfg = @internal.buildcfg_package;

public static partial class objabi_package {

public static readonly nint ElfRelocOffset = 256;
public static readonly nint MachoRelocOffset = 2048; // reserve enough space for ELF relocations

// HeaderString returns the toolchain configuration string written in
// Go object headers. This string ensures we don't attempt to import
// or link object files that are incompatible with each other. This
// string always starts with "go object ".
public static @string HeaderString() {
    return fmt.Sprintf("go object %s %s %s X:%s\n", buildcfg.GOOS, buildcfg.GOARCH, buildcfg.Version, strings.Join(buildcfg.EnabledExperiments(), ","));
}

} // end objabi_package
