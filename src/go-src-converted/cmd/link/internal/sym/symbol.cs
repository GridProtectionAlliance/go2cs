// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2022 March 13 06:33:29 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\sym\symbol.go
namespace go.cmd.link.@internal;

using obj = cmd.@internal.obj_package;

public static partial class sym_package {

public static readonly nint SymVerABI0 = 0;
public static readonly nint SymVerABIInternal = 1;
public static readonly nint SymVerStatic = 10; // Minimum version used by static (file-local) syms

public static nint ABIToVersion(obj.ABI abi) {

    if (abi == obj.ABI0) 
        return SymVerABI0;
    else if (abi == obj.ABIInternal) 
        return SymVerABIInternal;
        return -1;
}

public static (obj.ABI, bool) VersionToABI(nint v) {
    obj.ABI _p0 = default;
    bool _p0 = default;


    if (v == SymVerABI0) 
        return (obj.ABI0, true);
    else if (v == SymVerABIInternal) 
        return (obj.ABIInternal, true);
        return (~obj.ABI(0), false);
}

} // end sym_package
