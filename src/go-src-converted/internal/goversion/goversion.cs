// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goversion -- go2cs converted at 2020 October 08 04:02:34 UTC
// import "internal/goversion" ==> using goversion = go.@internal.goversion_package
// Original source: C:\Go\src\internal\goversion\goversion.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class goversion_package
    {
        // Version is the current Go 1.x version. During development cycles on
        // the master branch it changes to be the version of the next Go 1.x
        // release.
        //
        // When incrementing this, also add to the list at src/go/build/doc.go
        // (search for "onward").
        public static readonly long Version = (long)15L;

    }
}}
