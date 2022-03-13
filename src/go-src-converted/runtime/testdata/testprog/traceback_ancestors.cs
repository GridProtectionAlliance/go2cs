// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:27 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\traceback_ancestors.go
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;
using System.Threading;

public static partial class main_package {

private static void init() {
    register("TracebackAncestors", TracebackAncestors);
}

private static readonly nint numGoroutines = 3;

private static readonly nint numFrames = 2;



public static void TracebackAncestors() {
    var w = make_channel<object>();
    recurseThenCallGo(w, numGoroutines, numFrames, true).Send(w);
    printStack();
    close(w);
}

private static var ignoreGoroutines = make_map<@string, bool>();

private static void printStack() {
    var buf = make_slice<byte>(1024);
    while (true) {
        var n = runtime.Stack(buf, true);
        if (n < len(buf)) {
            var tb = string(buf[..(int)n]); 

            // Delete any ignored goroutines, if present.
            nint pos = 0;
            while (pos < len(tb)) {
                var next = pos + strings.Index(tb[(int)pos..], "\n\n");
                if (next < pos) {
                    next = len(tb);
                }
                else
 {
                    next += len("\n\n");
                }
                if (strings.HasPrefix(tb[(int)pos..], "goroutine ")) {
                    var id = tb[(int)pos + len("goroutine ")..];
                    id = id[..(int)strings.IndexByte(id, ' ')];
                    if (ignoreGoroutines[id]) {
                        tb = tb[..(int)pos] + tb[(int)next..];
                        next = pos;
                    }
                }
                pos = next;
            }


            fmt.Print(tb);
            return ;
        }
        buf = make_slice<byte>(2 * len(buf));
    }
}

private static void recurseThenCallGo(channel<object> w, nint frames, nint goroutines, bool main) {
    if (frames == 0) { 
        // Signal to TracebackAncestors that we are done recursing and starting goroutines.
        w.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
        w.Receive();
        return ;
    }
    if (goroutines == 0) { 
        // Record which goroutine this is so we can ignore it
        // in the traceback if it hasn't finished exiting by
        // the time we printStack.
        if (!main) {
            ignoreGoroutines[goroutineID()] = true;
        }
        go_(() => recurseThenCallGo(w, frames - 1, numFrames, false));
        return ;
    }
    recurseThenCallGo(w, frames, goroutines - 1, main);
}

private static @string goroutineID() => func((_, panic, _) => {
    var buf = make_slice<byte>(128);
    runtime.Stack(buf, false);
    const @string prefix = "goroutine ";

    if (!bytes.HasPrefix(buf, (slice<byte>)prefix)) {
        panic(fmt.Sprintf("expected %q at beginning of traceback:\n%s", prefix, buf));
    }
    buf = buf[(int)len(prefix)..];
    var n = bytes.IndexByte(buf, ' ');
    return string(buf[..(int)n]);
});

} // end main_package
