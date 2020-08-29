// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the copylock checker's
// range statement analysis.

// package testdata -- go2cs converted at 2020 August 29 10:10:31 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\copylock_range.go
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        private static void rangeMutex()
        {
            sync.Mutex mu = default;
            long i = default;

            slice<sync.Mutex> s = default;
            foreach (var (__i) in s)
            {
                i = __i;
            }            foreach (var (__i) in s)
            {
                i = __i;
            }
            {
                long i__prev1 = i;

                foreach (var (__i) in s)
                {
                    i = __i;
                }
                i = i__prev1;
            }

            foreach (var (__i, _) in s)
            {
                i = __i;
            }
            {
                long i__prev1 = i;

                foreach (var (__i, _) in s)
                {
                    i = __i;
                }
                i = i__prev1;
            }

            foreach (var (_, __mu) in s)
            {
                mu = __mu; // ERROR "range var mu copies lock: sync.Mutex"
            }
            {
                var m__prev1 = m;

                foreach (var (_, __m) in s)
                {
                    m = __m; // ERROR "range var m copies lock: sync.Mutex"
                }
                m = m__prev1;
            }

            foreach (var (__i, __mu) in s)
            {
                i = __i;
                mu = __mu; // ERROR "range var mu copies lock: sync.Mutex"
            }
            {
                long i__prev1 = i;
                var m__prev1 = m;

                foreach (var (__i, __m) in s)
                {
                    i = __i;
                    m = __m; // ERROR "range var m copies lock: sync.Mutex"
                }
                i = i__prev1;
                m = m__prev1;
            }

            array<sync.Mutex> a = new array<sync.Mutex>(3L);
            {
                var m__prev1 = m;

                foreach (var (_, __m) in a)
                {
                    m = __m; // ERROR "range var m copies lock: sync.Mutex"
                }
                m = m__prev1;
            }

            map<sync.Mutex, sync.Mutex> m = default;
            {
                var k__prev1 = k;

                foreach (var (__k) in m)
                {
                    k = __k; // ERROR "range var k copies lock: sync.Mutex"
                }
                k = k__prev1;
            }

            foreach (var (__mu, _) in m)
            {
                mu = __mu; // ERROR "range var mu copies lock: sync.Mutex"
            }
            {
                var k__prev1 = k;

                foreach (var (__k, _) in m)
                {
                    k = __k; // ERROR "range var k copies lock: sync.Mutex"
                }
                k = k__prev1;
            }

            foreach (var (_, __mu) in m)
            {
                mu = __mu; // ERROR "range var mu copies lock: sync.Mutex"
            }
            {
                var v__prev1 = v;

                foreach (var (_, __v) in m)
                {
                    v = __v; // ERROR "range var v copies lock: sync.Mutex"
                }
                v = v__prev1;
            }

            channel<sync.Mutex> c = default;
            foreach (var (__mu) in c)
            {
                mu = __mu;
            }            foreach (var (__mu) in c)
            {
                mu = __mu; // ERROR "range var mu copies lock: sync.Mutex"
            }
            {
                var v__prev1 = v;

                foreach (var (__v) in c)
                {
                    v = __v; // ERROR "range var v copies lock: sync.Mutex"
                }
                v = v__prev1;
            }

            var t = default;
            foreach (var (__t.i, __t.mu) in s)
            {
                t.i = __t.i;
                t.mu = __t.mu; // ERROR "range var t.mu copies lock: sync.Mutex"
            }
        }
    }
}}}
