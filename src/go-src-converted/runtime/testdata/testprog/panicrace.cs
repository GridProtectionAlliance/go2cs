// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:49 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\panicrace.go
using runtime = go.runtime_package;
using sync = go.sync_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("PanicRace", PanicRace);
        }

        public static void PanicRace() => func((defer, panic, _) =>
        {
            sync.WaitGroup wg = default;
            wg.Add(1L);
            go_(() => () =>
            {
                defer(() =>
                {
                    wg.Done();
                    runtime.Gosched();
                }());
                panic("crash");

            }());
            wg.Wait();

        });
    }
}
