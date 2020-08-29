// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is used to generate an object file which
// serves as test file for gcimporter_test.go.

// package exports -- go2cs converted at 2020 August 29 10:09:13 UTC
// import "go/internal/gcimporter.exports" ==> using exports = go.go.@internal.gcimporter.exports_package
// Original source: C:\Go\src\go\internal\gcimporter\testdata\exports.go
using ast = go.go.ast_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace go {
namespace @internal
{
    public static partial class exports_package
    {
        // Issue 3682: Correctly read dotted identifiers from export data.
        private static readonly long init1 = 0L;



        private static void init()
        {
        }

        public static readonly long C0 = 0L;
        public static readonly float C1 = 3.14159265F;
        public static readonly ulong C2 = 2.718281828iUL;
        public static readonly float C3 = -123.456e-789F;
        public static readonly float C4 = +123.456E+789F;
        public static readonly ulong C5 = 1234iUL;
        public static readonly @string C6 = "foo\n";
        public static readonly @string C7 = "bar\\n";

        public partial struct T1 // : long
        {
        }
        public partial struct T2 // : array<long>
        {
        }
        public partial struct T3 // : slice<long>
        {
        }
        public partial struct T4 // : ref long
        {
        }
        public partial struct T5 // : channel<long>
        {
        }
        public partial struct T6a // : channel<long>
        {
        }
        public partial struct T6b // : channel<channel<long>>
        {
        }
        public partial struct T6c // : channel<channel<long>>
        {
        }
        public partial struct T7 // : channel<ref ast.File>
        {
        }
        public partial struct T8
        {
        }
        public partial struct T9
        {
            public long a;
            public float b;
            public float c;
            [Description("go:\"tag\"")]
            public slice<@string> d;
        }
        public partial struct T10
        {
            public ref T8 T8 => ref T8_val;
            public ref T9 T9 => ref T9_val;
            public ptr<T10> _;
        }
        public partial struct T11 // : map<long, @string>
        {
        }
        public partial interface T12
        {
        }
        public partial interface T13
        {
            float m1();
            float m2(long _p0);
        }
        public partial interface T14 : T12, T13
        {
            slice<T9> m3(params object[] x);
        }
        public delegate void T15();
        public delegate void T16(long);
        public delegate void T17(long);
        public delegate float T18();
        public delegate float T19();
        public delegate void T20(object);
        public partial struct T21
        {
            public ptr<T21> next;
        }
        public partial struct T22
        {
            public ptr<T23> link;
        }
        public partial struct T23
        {
            public ptr<T22> link;
        }
        public partial struct T24 // : ref T24
        {
        }
        public partial struct T25 // : ref T26
        {
        }
        public partial struct T26 // : ref T27
        {
        }
        public partial struct T27 // : ref T25
        {
        }
        public delegate  T28 T28(T28);        public static long V0 = default;        public static float V1 = -991.0F;        public static float V2 = 1.2F;

        public static void F1()
        {
        }
        public static void F2(long x)
        {
        }
        public static long F3()
        {
            return 0L;
        }
        public static float F4()
        {
            return 0L;
        }
        public static (channel<T10>, channel<T10>, channel<T10>) F5(long a, long b, long c, object u, object v, object w, params object[] more)
;

        private static void M1(this ref T1 p)

    }
}}}
