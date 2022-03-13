// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:10 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\sort.go
namespace go.cmd.compile.@internal;

public static partial class types_package {

// MethodsByName sorts methods by symbol.
public partial struct MethodsByName { // : slice<ptr<Field>>
}

public static nint Len(this MethodsByName x) {
    return len(x);
}

public static void Swap(this MethodsByName x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

public static bool Less(this MethodsByName x, nint i, nint j) {
    return x[i].Sym.Less(x[j].Sym);
}

} // end types_package
