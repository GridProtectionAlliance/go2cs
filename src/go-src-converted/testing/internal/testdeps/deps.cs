// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testdeps provides access to dependencies needed by test execution.
//
// This package is imported by the generated main package, which passes
// TestDeps into testing.Main. This allows tests to use packages at run time
// without making those packages direct dependencies of package testing.
// Direct dependencies of package testing are harder to write tests for.
// package testdeps -- go2cs converted at 2020 October 08 04:36:39 UTC
// import "testing/internal/testdeps" ==> using testdeps = go.testing.@internal.testdeps_package
// Original source: C:\Go\src\testing\internal\testdeps\deps.go
using bufio = go.bufio_package;
using testlog = go.@internal.testlog_package;
using io = go.io_package;
using regexp = go.regexp_package;
using pprof = go.runtime.pprof_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace testing {
namespace @internal
{
    public static partial class testdeps_package
    {
        // TestDeps is an implementation of the testing.testDeps interface,
        // suitable for passing to testing.MainStart.
        public partial struct TestDeps
        {
        }

        private static @string matchPat = default;
        private static ptr<regexp.Regexp> matchRe;

        public static (bool, error) MatchString(this TestDeps _p0, @string pat, @string str)
        {
            bool result = default;
            error err = default!;

            if (matchRe == null || matchPat != pat)
            {
                matchPat = pat;
                matchRe, err = regexp.Compile(matchPat);
                if (err != null)
                {
                    return ;
                }

            }

            return (matchRe.MatchString(str), error.As(null!)!);

        }

        public static error StartCPUProfile(this TestDeps _p0, io.Writer w)
        {
            return error.As(pprof.StartCPUProfile(w))!;
        }

        public static void StopCPUProfile(this TestDeps _p0)
        {
            pprof.StopCPUProfile();
        }

        public static error WriteProfileTo(this TestDeps _p0, @string name, io.Writer w, long debug)
        {
            return error.As(pprof.Lookup(name).WriteTo(w, debug))!;
        }

        // ImportPath is the import path of the testing binary, set by the generated main function.
        public static @string ImportPath = default;

        public static @string ImportPath(this TestDeps _p0)
        {
            return ImportPath;
        }

        // testLog implements testlog.Interface, logging actions by package os.
        private partial struct testLog
        {
            public sync.Mutex mu;
            public ptr<bufio.Writer> w;
            public bool set;
        }

        private static void Getenv(this ptr<testLog> _addr_l, @string key)
        {
            ref testLog l = ref _addr_l.val;

            l.add("getenv", key);
        }

        private static void Open(this ptr<testLog> _addr_l, @string name)
        {
            ref testLog l = ref _addr_l.val;

            l.add("open", name);
        }

        private static void Stat(this ptr<testLog> _addr_l, @string name)
        {
            ref testLog l = ref _addr_l.val;

            l.add("stat", name);
        }

        private static void Chdir(this ptr<testLog> _addr_l, @string name)
        {
            ref testLog l = ref _addr_l.val;

            l.add("chdir", name);
        }

        // add adds the (op, name) pair to the test log.
        private static void add(this ptr<testLog> _addr_l, @string op, @string name) => func((defer, _, __) =>
        {
            ref testLog l = ref _addr_l.val;

            if (strings.Contains(name, "\n") || name == "")
            {
                return ;
            }

            l.mu.Lock();
            defer(l.mu.Unlock());
            if (l.w == null)
            {
                return ;
            }

            l.w.WriteString(op);
            l.w.WriteByte(' ');
            l.w.WriteString(name);
            l.w.WriteByte('\n');

        });

        private static testLog log = default;

        public static void StartTestLog(this TestDeps _p0, io.Writer w)
        {
            log.mu.Lock();
            log.w = bufio.NewWriter(w);
            if (!log.set)
            { 
                // Tests that define TestMain and then run m.Run multiple times
                // will call StartTestLog/StopTestLog multiple times.
                // Checking log.set avoids calling testlog.SetLogger multiple times
                // (which will panic) and also avoids writing the header multiple times.
                log.set = true;
                testlog.SetLogger(_addr_log);
                log.w.WriteString("# test log\n"); // known to cmd/go/internal/test/test.go
            }

            log.mu.Unlock();

        }

        public static error StopTestLog(this TestDeps _p0) => func((defer, _, __) =>
        {
            log.mu.Lock();
            defer(log.mu.Unlock());
            var err = log.w.Flush();
            log.w = null;
            return error.As(err)!;
        });
    }
}}}
