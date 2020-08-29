// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testdata -- go2cs converted at 2020 August 29 10:10:39 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\unsafeptr.go
using reflect = go.reflect_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        private static void f()
        {
            unsafe.Pointer x = default;
            System.UIntPtr y = default;
            x = @unsafe.Pointer(y); // ERROR "possible misuse of unsafe.Pointer"
            y = uintptr(x); 

            // only allowed pointer arithmetic is ptr +/-/&^ num.
            // num+ptr is technically okay but still flagged: write ptr+num instead.
            x = @unsafe.Pointer(uintptr(x) + 1L);
            x = @unsafe.Pointer(1L + uintptr(x)); // ERROR "possible misuse of unsafe.Pointer"
            x = @unsafe.Pointer(uintptr(x) + uintptr(x)); // ERROR "possible misuse of unsafe.Pointer"
            x = @unsafe.Pointer(uintptr(x) - 1L);
            x = @unsafe.Pointer(1L - uintptr(x)); // ERROR "possible misuse of unsafe.Pointer"
            x = @unsafe.Pointer(uintptr(x) & ~3L);
            x = @unsafe.Pointer(1L & ~uintptr(x)); // ERROR "possible misuse of unsafe.Pointer"

            // certain uses of reflect are okay
            reflect.Value v = default;
            x = @unsafe.Pointer(v.Pointer());
            x = @unsafe.Pointer(v.UnsafeAddr());
            ref reflect.StringHeader s1 = default;
            x = @unsafe.Pointer(s1.Data);
            ref reflect.SliceHeader s2 = default;
            x = @unsafe.Pointer(s2.Data);
            reflect.StringHeader s3 = default;
            x = @unsafe.Pointer(s3.Data); // ERROR "possible misuse of unsafe.Pointer"
            reflect.SliceHeader s4 = default;
            x = @unsafe.Pointer(s4.Data); // ERROR "possible misuse of unsafe.Pointer"

            // but only in reflect
            V vv = default;
            x = @unsafe.Pointer(vv.Pointer()); // ERROR "possible misuse of unsafe.Pointer"
            x = @unsafe.Pointer(vv.UnsafeAddr()); // ERROR "possible misuse of unsafe.Pointer"
            ref StringHeader ss1 = default;
            x = @unsafe.Pointer(ss1.Data); // ERROR "possible misuse of unsafe.Pointer"
            ref SliceHeader ss2 = default;
            x = @unsafe.Pointer(ss2.Data); // ERROR "possible misuse of unsafe.Pointer"

        }

        public partial interface V
        {
            System.UIntPtr Pointer();
            System.UIntPtr UnsafeAddr();
        }

        public partial struct StringHeader
        {
            public System.UIntPtr Data;
        }

        public partial struct SliceHeader
        {
            public System.UIntPtr Data;
        }
    }
}}}
