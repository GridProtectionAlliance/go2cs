// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 03:44:09 UTC
// Original source: C:\Go\src\runtime\testdata\testprognet\net.go
using fmt = go.fmt_package;
using net = go.net_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            registerInit("NetpollDeadlock", NetpollDeadlockInit);
            register("NetpollDeadlock", NetpollDeadlock);
        }

        public static void NetpollDeadlockInit()
        {
            fmt.Println("dialing");
            var (c, err) = net.Dial("tcp", "localhost:14356");
            if (err == null)
            {
                c.Close();
            }
            else
            {
                fmt.Println("error: ", err);
            }

        }

        public static void NetpollDeadlock()
        {
            fmt.Println("done");
        }
    }
}
