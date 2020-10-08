// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue31540 -- go2cs converted at 2020 October 08 04:56:22 UTC
// import "go/internal/gccgoimporter.issue31540" ==> using issue31540 = go.go.@internal.gccgoimporter.issue31540_package
// Original source: C:\Go\src\go\internal\gccgoimporter\testdata\issue31540.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class issue31540_package
    {
        public partial struct Y
        {
            public long q;
        }

        public partial struct Z // : map<long, long>
        {
        }

        public partial struct X // : map<Y, Z>
        {
        }

        public partial struct A1 // : X
        {
        }

        public partial struct A2 // : A1
        {
        }

        public partial struct S
        {
            public long b;
            public ref A2 A2 => ref A2_val;
        }

        public static S Hallo()
        {
            return new S();
        }
    }
}}}
