// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64 amd64p32 arm64 ppc64 ppc64le s390x

// package runtime -- go2cs converted at 2020 August 29 08:21:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\unaligned1.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static uint readUnaligned32(unsafe.Pointer p)
        {
            return p.Value;
        }

        private static ulong readUnaligned64(unsafe.Pointer p)
        {
            return p.Value;
        }
    }
}
