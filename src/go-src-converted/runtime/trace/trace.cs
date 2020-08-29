// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package trace contains facilities for programs to generate trace
// for Go execution tracer.
//
// The execution trace captures a wide range of execution events such as
// goroutine creation/blocking/unblocking, syscall enter/exit/block,
// GC-related events, changes of heap size, processor start/stop, etc.
// A precise nanosecond-precision timestamp and a stack trace is
// captured for most events. The generated trace can be interpreted
// using `go tool trace`.
//
// Tracing a Go program
//
// Support for tracing tests and benchmarks built with the standard
// testing package is built into `go test`. For example, the following
// command runs the test in the current directory and writes the trace
// file (trace.out).
//
//    go test -trace=test.out
//
// This runtime/trace package provides APIs to add equivalent tracing
// support to a standalone program. See the Example that demonstrates
// how to use this API to enable tracing.
//
// There is also a standard HTTP interface to profiling data. Adding the
// following line will install handlers under the /debug/pprof/trace URL
// to download live profiles:
//
//     import _ "net/http/pprof"
//
// See the net/http/pprof package for more details.
// package trace -- go2cs converted at 2020 August 29 08:34:29 UTC
// import "runtime/trace" ==> using trace = go.runtime.trace_package
// Original source: C:\Go\src\runtime\trace\trace.go
using io = go.io_package;
using runtime = go.runtime_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace runtime
{
    public static partial class trace_package
    {
        // Start enables tracing for the current program.
        // While tracing, the trace will be buffered and written to w.
        // Start returns an error if tracing is already enabled.
        public static error Start(io.Writer w)
        {
            {
                var err = runtime.StartTrace();

                if (err != null)
                {
                    return error.As(err);
                }
            }
            go_(() => () =>
            {
                while (true)
                {
                    var data = runtime.ReadTrace();
                    if (data == null)
                    {
                        break;
                    }
                    w.Write(data);
                }
            }());
            return error.As(null);
        }

        // Stop stops the current tracing, if any.
        // Stop only returns after all the writes for the trace have completed.
        public static void Stop()
        {
            runtime.StopTrace();
        }
    }
}}
