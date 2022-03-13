// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goobj -- go2cs converted at 2022 March 13 05:43:19 UTC
// import "cmd/internal/goobj" ==> using goobj = go.cmd.@internal.goobj_package
// Original source: C:\Program Files\Go\src\cmd\internal\goobj\builtin.go
namespace go.cmd.@internal;

public static partial class goobj_package {

// Builtin (compiler-generated) function references appear
// frequently. We assign special indices for them, so they
// don't need to be referenced by name.

// NBuiltin returns the number of listed builtin
// symbols.
public static nint NBuiltin() {
    return len(builtins);
}

// BuiltinName returns the name and ABI of the i-th
// builtin symbol.
public static (@string, nint) BuiltinName(nint i) {
    @string _p0 = default;
    nint _p0 = default;

    return (builtins[i].name, builtins[i].abi);
}

// BuiltinIdx returns the index of the builtin with the
// given name and abi, or -1 if it is not a builtin.
public static nint BuiltinIdx(@string name, nint abi) {
    var (i, ok) = builtinMap[name];
    if (!ok) {
        return -1;
    }
    if (builtins[i].abi != abi) {
        return -1;
    }
    return i;
}

//go:generate go run mkbuiltin.go

private static map<@string, nint> builtinMap = default;

private static void init() {
    builtinMap = make_map<@string, nint>(len(builtins));
    foreach (var (i, b) in builtins) {
        builtinMap[b.name] = i;
    }
}

} // end goobj_package
