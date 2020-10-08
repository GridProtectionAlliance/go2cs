// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unsafeheader contains header declarations for the Go runtime's
// slice and string implementations.
//
// This package allows x/sys to use types equivalent to
// reflect.SliceHeader and reflect.StringHeader without introducing
// a dependency on the (relatively heavy) "reflect" package.
// package unsafeheader -- go2cs converted at 2020 October 08 04:46:07 UTC
// import "cmd/vendor/golang.org/x/sys/internal/unsafeheader" ==> using unsafeheader = go.cmd.vendor.golang.org.x.sys.@internal.unsafeheader_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\internal\unsafeheader\unsafeheader.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys {
namespace @internal
{
    public static partial class unsafeheader_package
    {
        // Slice is the runtime representation of a slice.
        // It cannot be used safely or portably and its representation may change in a later release.
        public partial struct Slice
        {
            public unsafe.Pointer Data;
            public long Len;
            public long Cap;
        }

        // String is the runtime representation of a string.
        // It cannot be used safely or portably and its representation may change in a later release.
        public partial struct String
        {
            public unsafe.Pointer Data;
            public long Len;
        }
    }
}}}}}}}
