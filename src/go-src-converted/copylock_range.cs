// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the copylock checker's
// range statement analysis.

// package a -- go2cs converted at 2020 October 09 06:03:58 UTC
// import "golang.org/x/tools/go/analysis/passes/copylock/testdata/src/a" ==> using a = go.golang.org.x.tools.go.analysis.passes.copylock.testdata.src.a_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\copylock\testdata\src\a\copylock_range.go
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace copylock {
namespace testdata {
namespace src
{
    public static partial class a_package
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
                mu = __mu; // want "range var mu copies lock: sync.Mutex"
            }
            {
                var m__prev1 = m;

                foreach (var (_, __m) in s)
                {
                    m = __m; // want "range var m copies lock: sync.Mutex"
                }
                m = m__prev1;
            }

            foreach (var (__i, __mu) in s)
            {
                i = __i;
                mu = __mu; // want "range var mu copies lock: sync.Mutex"
            }
            {
                long i__prev1 = i;
                var m__prev1 = m;

                foreach (var (__i, __m) in s)
                {
                    i = __i;
                    m = __m; // want "range var m copies lock: sync.Mutex"
                }
                i = i__prev1;
                m = m__prev1;
            }

            array<sync.Mutex> a = new array<sync.Mutex>(3L);
            {
                var m__prev1 = m;

                foreach (var (_, __m) in a)
                {
                    m = __m; // want "range var m copies lock: sync.Mutex"
                }
                m = m__prev1;
            }

            map<sync.Mutex, sync.Mutex> m = default;
            {
                var k__prev1 = k;

                foreach (var (__k) in m)
                {
                    k = __k; // want "range var k copies lock: sync.Mutex"
                }
                k = k__prev1;
            }

            foreach (var (__mu, _) in m)
            {
                mu = __mu; // want "range var mu copies lock: sync.Mutex"
            }
            {
                var k__prev1 = k;

                foreach (var (__k, _) in m)
                {
                    k = __k; // want "range var k copies lock: sync.Mutex"
                }
                k = k__prev1;
            }

            foreach (var (_, __mu) in m)
            {
                mu = __mu; // want "range var mu copies lock: sync.Mutex"
            }
            {
                var v__prev1 = v;

                foreach (var (_, __v) in m)
                {
                    v = __v; // want "range var v copies lock: sync.Mutex"
                }
                v = v__prev1;
            }

            channel<sync.Mutex> c = default;
            foreach (var (__mu) in c)
            {
                mu = __mu;
            }            foreach (var (__mu) in c)
            {
                mu = __mu; // want "range var mu copies lock: sync.Mutex"
            }
            {
                var v__prev1 = v;

                foreach (var (__v) in c)
                {
                    v = __v; // want "range var v copies lock: sync.Mutex"
                }
                v = v__prev1;
            }

            var t = default;
            foreach (var (__t.i, __t.mu) in s)
            {
                t.i = __t.i;
                t.mu = __t.mu; // want "range var t.mu copies lock: sync.Mutex"
            }
        }
    }
}}}}}}}}}}
