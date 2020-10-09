// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:52 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\traceback_ancestors.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using static go.builtin;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("TracebackAncestors", TracebackAncestors);
        }

        private static readonly long numGoroutines = (long)3L;

        private static readonly long numFrames = (long)2L;



        public static void TracebackAncestors()
        {
            var w = make_channel<object>();
            recurseThenCallGo(w, numGoroutines, numFrames, true).Send(w);
            printStack();
            close(w);
        }

        private static var ignoreGoroutines = make_map<@string, bool>();

        private static void printStack()
        {
            var buf = make_slice<byte>(1024L);
            while (true)
            {
                var n = runtime.Stack(buf, true);
                if (n < len(buf))
                {
                    var tb = string(buf[..n]); 

                    // Delete any ignored goroutines, if present.
                    long pos = 0L;
                    while (pos < len(tb))
                    {
                        var next = pos + strings.Index(tb[pos..], "\n\n");
                        if (next < pos)
                        {
                            next = len(tb);
                        }
                        else
                        {
                            next += len("\n\n");
                        }

                        if (strings.HasPrefix(tb[pos..], "goroutine "))
                        {
                            var id = tb[pos + len("goroutine ")..];
                            id = id[..strings.IndexByte(id, ' ')];
                            if (ignoreGoroutines[id])
                            {
                                tb = tb[..pos] + tb[next..];
                                next = pos;
                            }

                        }

                        pos = next;

                    }


                    fmt.Print(tb);
                    return ;

                }

                buf = make_slice<byte>(2L * len(buf));

            }


        }

        private static void recurseThenCallGo(channel<object> w, long frames, long goroutines, bool main)
        {
            if (frames == 0L)
            { 
                // Signal to TracebackAncestors that we are done recursing and starting goroutines.
                w.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                w.Receive();
                return ;

            }

            if (goroutines == 0L)
            { 
                // Record which goroutine this is so we can ignore it
                // in the traceback if it hasn't finished exiting by
                // the time we printStack.
                if (!main)
                {
                    ignoreGoroutines[goroutineID()] = true;
                } 

                // Start the next goroutine now that there are no more recursions left
                // for this current goroutine.
                go_(() => recurseThenCallGo(w, frames - 1L, numFrames, false));
                return ;

            }

            recurseThenCallGo(w, frames, goroutines - 1L, main);

        }

        private static @string goroutineID() => func((_, panic, __) =>
        {
            var buf = make_slice<byte>(128L);
            runtime.Stack(buf, false);
            const @string prefix = (@string)"goroutine ";

            if (!bytes.HasPrefix(buf, (slice<byte>)prefix))
            {
                panic(fmt.Sprintf("expected %q at beginning of traceback:\n%s", prefix, buf));
            }

            buf = buf[len(prefix)..];
            var n = bytes.IndexByte(buf, ' ');
            return string(buf[..n]);

        });
    }
}
