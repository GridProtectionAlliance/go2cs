// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:25:04 UTC
// Original source: C:\Go\src\runtime\testdata\testprognet\main.go
using os = go.os_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static map cmds = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Action>{};

        private static void register(@string name, Action f) => func((_, panic, __) =>
        {
            if (cmds[name] != null)
            {
                panic("duplicate registration: " + name);
            }
            cmds[name] = f;
        });

        private static void registerInit(@string name, Action f)
        {
            if (len(os.Args) >= 2L && os.Args[1L] == name)
            {
                f();
            }
        }

        private static void Main()
        {
            if (len(os.Args) < 2L)
            {
                println("usage: " + os.Args[0L] + " name-of-test");
                return;
            }
            var f = cmds[os.Args[1L]];
            if (f == null)
            {
                println("unknown function: " + os.Args[1L]);
                return;
            }
            f();
        }
    }
}
