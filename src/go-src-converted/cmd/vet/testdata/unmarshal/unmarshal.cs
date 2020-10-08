// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the unmarshal checker.

// package unmarshal -- go2cs converted at 2020 October 08 04:58:39 UTC
// import "cmd/vet/testdata/unmarshal" ==> using unmarshal = go.cmd.vet.testdata.unmarshal_package
// Original source: C:\Go\src\cmd\vet\testdata\unmarshal\unmarshal.go
using json = go.encoding.json_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class unmarshal_package
    {
        private static void _()
        {
            private partial struct t
            {
                public long a;
            }
            t v = default;

            json.Unmarshal(new slice<byte>(new byte[] {  }), v); // ERROR "call of Unmarshal passes non-pointer as second argument"
        }
    }
}}}}
