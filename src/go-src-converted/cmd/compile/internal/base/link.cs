// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 06 23:14:30 UTC
// import "cmd/compile/internal/base" ==> using @base = go.cmd.compile.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\base\link.go
using obj = go.cmd.@internal.obj_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class @base_package {

public static ptr<obj.Link> Ctxt;

// TODO(mdempsky): These should probably be obj.Link methods.

// PkgLinksym returns the linker symbol for name within the given
// package prefix. For user packages, prefix should be the package
// path encoded with objabi.PathToPrefix.
public static ptr<obj.LSym> PkgLinksym(@string prefix, @string name, obj.ABI abi) {
    if (name == "_") { 
        // TODO(mdempsky): Cleanup callers and Fatalf instead.
        return _addr_linksym(prefix, "_", abi)!;

    }
    return _addr_linksym(prefix, prefix + "." + name, abi)!;

}

// Linkname returns the linker symbol for the given name as it might
// appear within a //go:linkname directive.
public static ptr<obj.LSym> Linkname(@string name, obj.ABI abi) {
    return _addr_linksym("_", name, abi)!;
}

// linksym is an internal helper function for implementing the above
// exported APIs.
private static ptr<obj.LSym> linksym(@string pkg, @string name, obj.ABI abi) {
    return _addr_Ctxt.LookupABIInit(name, abi, r => {
        r.Pkg = pkg;
    })!;

}

} // end @base_package
