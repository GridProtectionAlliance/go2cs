// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2020 August 29 08:36:42 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Go\src\sync\once.go
using atomic = go.sync.atomic_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class sync_package
    {
        // Once is an object that will perform exactly one action.
        public partial struct Once
        {
            public Mutex m;
            public uint done;
        }

        // Do calls the function f if and only if Do is being called for the
        // first time for this instance of Once. In other words, given
        //     var once Once
        // if once.Do(f) is called multiple times, only the first call will invoke f,
        // even if f has a different value in each invocation. A new instance of
        // Once is required for each function to execute.
        //
        // Do is intended for initialization that must be run exactly once. Since f
        // is niladic, it may be necessary to use a function literal to capture the
        // arguments to a function to be invoked by Do:
        //     config.once.Do(func() { config.init(filename) })
        //
        // Because no call to Do returns until the one call to f returns, if f causes
        // Do to be called, it will deadlock.
        //
        // If f panics, Do considers it to have returned; future calls of Do return
        // without calling f.
        //
        private static void Do(this ref Once _o, Action f) => func(_o, (ref Once o, Defer defer, Panic _, Recover __) =>
        {
            if (atomic.LoadUint32(ref o.done) == 1L)
            {
                return;
            } 
            // Slow-path.
            o.m.Lock();
            defer(o.m.Unlock());
            if (o.done == 0L)
            {
                defer(atomic.StoreUint32(ref o.done, 1L));
                f();
            }
        });
    }
}
