// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package trace contains facilities for programs to generate traces
// for the Go execution tracer.
//
// Tracing runtime activities
//
// The execution trace captures a wide range of execution events such as
// goroutine creation/blocking/unblocking, syscall enter/exit/block,
// GC-related events, changes of heap size, processor start/stop, etc.
// A precise nanosecond-precision timestamp and a stack trace is
// captured for most events. The generated trace can be interpreted
// using `go tool trace`.
//
// Support for tracing tests and benchmarks built with the standard
// testing package is built into `go test`. For example, the following
// command runs the test in the current directory and writes the trace
// file (trace.out).
//
//    go test -trace=trace.out
//
// This runtime/trace package provides APIs to add equivalent tracing
// support to a standalone program. See the Example that demonstrates
// how to use this API to enable tracing.
//
// There is also a standard HTTP interface to trace data. Adding the
// following line will install a handler under the /debug/pprof/trace URL
// to download a live trace:
//
//     import _ "net/http/pprof"
//
// See the net/http/pprof package for more details about all of the
// debug endpoints installed by this import.
//
// User annotation
//
// Package trace provides user annotation APIs that can be used to
// log interesting events during execution.
//
// There are three types of user annotations: log messages, regions,
// and tasks.
//
// Log emits a timestamped message to the execution trace along with
// additional information such as the category of the message and
// which goroutine called Log. The execution tracer provides UIs to filter
// and group goroutines using the log category and the message supplied
// in Log.
//
// A region is for logging a time interval during a goroutine's execution.
// By definition, a region starts and ends in the same goroutine.
// Regions can be nested to represent subintervals.
// For example, the following code records four regions in the execution
// trace to trace the durations of sequential steps in a cappuccino making
// operation.
//
//   trace.WithRegion(ctx, "makeCappuccino", func() {
//
//      // orderID allows to identify a specific order
//      // among many cappuccino order region records.
//      trace.Log(ctx, "orderID", orderID)
//
//      trace.WithRegion(ctx, "steamMilk", steamMilk)
//      trace.WithRegion(ctx, "extractCoffee", extractCoffee)
//      trace.WithRegion(ctx, "mixMilkCoffee", mixMilkCoffee)
//   })
//
// A task is a higher-level component that aids tracing of logical
// operations such as an RPC request, an HTTP request, or an
// interesting local operation which may require multiple goroutines
// working together. Since tasks can involve multiple goroutines,
// they are tracked via a context.Context object. NewTask creates
// a new task and embeds it in the returned context.Context object.
// Log messages and regions are attached to the task, if any, in the
// Context passed to Log and WithRegion.
//
// For example, assume that we decided to froth milk, extract coffee,
// and mix milk and coffee in separate goroutines. With a task,
// the trace tool can identify the goroutines involved in a specific
// cappuccino order.
//
//      ctx, task := trace.NewTask(ctx, "makeCappuccino")
//      trace.Log(ctx, "orderID", orderID)
//
//      milk := make(chan bool)
//      espresso := make(chan bool)
//
//      go func() {
//              trace.WithRegion(ctx, "steamMilk", steamMilk)
//              milk <- true
//      }()
//      go func() {
//              trace.WithRegion(ctx, "extractCoffee", extractCoffee)
//              espresso <- true
//      }()
//      go func() {
//              defer task.End() // When assemble is done, the order is complete.
//              <-espresso
//              <-milk
//              trace.WithRegion(ctx, "mixMilkCoffee", mixMilkCoffee)
//      }()
//
//
// The trace tool computes the latency of a task by measuring the
// time between the task creation and the task end and provides
// latency distributions for each task type found in the trace.

// package trace -- go2cs converted at 2022 March 13 05:38:38 UTC
// import "runtime/trace" ==> using trace = go.runtime.trace_package
// Original source: C:\Program Files\Go\src\runtime\trace\trace.go
namespace go.runtime;

using io = io_package;
using runtime = runtime_package;
using sync = sync_package;
using atomic = sync.atomic_package;


// Start enables tracing for the current program.
// While tracing, the trace will be buffered and written to w.
// Start returns an error if tracing is already enabled.

using System;
using System.Threading;
public static partial class trace_package {

public static error Start(io.Writer w) => func((defer, _, _) => {
    tracing.Lock();
    defer(tracing.Unlock());

    {
        var err = runtime.StartTrace();

        if (err != null) {
            return error.As(err)!;
        }
    }
    go_(() => () => {
        while (true) {
            var data = runtime.ReadTrace();
            if (data == null) {
                break;
            }
            w.Write(data);
        }
    }());
    atomic.StoreInt32(_addr_tracing.enabled, 1);
    return error.As(null!)!;
});

// Stop stops the current tracing, if any.
// Stop only returns after all the writes for the trace have completed.
public static void Stop() => func((defer, _, _) => {
    tracing.Lock();
    defer(tracing.Unlock());
    atomic.StoreInt32(_addr_tracing.enabled, 0);

    runtime.StopTrace();
});

private static var tracing = default;

} // end trace_package
