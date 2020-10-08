// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.11

// package gcimporter -- go2cs converted at 2020 October 08 04:55:37 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\newInterface11.go
using types = go.go.types_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class gcimporter_package
    {
        private static ptr<types.Interface> newInterface(slice<ptr<types.Func>> methods, slice<types.Type> embeddeds)
        {
            return _addr_types.NewInterfaceType(methods, embeddeds)!;
        }
    }
}}}}}}
