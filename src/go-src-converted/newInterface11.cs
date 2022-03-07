// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.11

// package gcimporter -- go2cs converted at 2022 March 06 23:32:14 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\newInterface11.go
using types = go.go.types_package;

namespace go.golang.org.x.tools.go.@internal;

public static partial class gcimporter_package {

private static ptr<types.Interface> newInterface(slice<ptr<types.Func>> methods, slice<types.Type> embeddeds) {
    return _addr_types.NewInterfaceType(methods, embeddeds)!;
}

} // end gcimporter_package
