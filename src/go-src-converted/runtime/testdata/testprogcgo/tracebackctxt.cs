// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The __attribute__((weak)) used below doesn't seem to work on Windows.

// package main -- go2cs converted at 2020 August 29 08:25:03 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\tracebackctxt.go
// Test the context argument to SetCgoTraceback.
// Use fake context, traceback, and symbolizer functions.

/*
// Defined in tracebackctxt_c.c.
extern void C1(void);
extern void C2(void);
extern void tcContext(void*);
extern void tcTraceback(void*);
extern void tcSymbolizer(void*);
extern int getContextCount(void);
*/
using C = go.C_package;// Test the context argument to SetCgoTraceback.
// Use fake context, traceback, and symbolizer functions.

/*
// Defined in tracebackctxt_c.c.
extern void C1(void);
extern void C2(void);
extern void tcContext(void*);
extern void tcTraceback(void*);
extern void tcSymbolizer(void*);
extern int getContextCount(void);
*/


using fmt = go.fmt_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("TracebackContext", TracebackContext);
        }

        private static bool tracebackOK = default;

        public static void TracebackContext()
        {
            runtime.SetCgoTraceback(0L, @unsafe.Pointer(C.tcTraceback), @unsafe.Pointer(C.tcContext), @unsafe.Pointer(C.tcSymbolizer));
            C.C1();
            {
                var got = C.getContextCount();

                if (got != 0L)
                {
                    fmt.Printf("at end contextCount == %d, expected 0\n", got);
                    tracebackOK = false;
                }

            }
            if (tracebackOK)
            {
                fmt.Println("OK");
            }
        }

        //export G1
        public static void G1()
        {
            C.C2();
        }

        //export G2
        public static void G2()
        {
            var pc = make_slice<System.UIntPtr>(32L);
            var n = runtime.Callers(0L, pc);
            var cf = runtime.CallersFrames(pc[..n]);
            slice<runtime.Frame> frames = default;
            while (true)
            {
                var (frame, more) = cf.Next();
                frames = append(frames, frame);
                if (!more)
                {
                    break;
                }
            }


            var ok = true;
            long i = 0L;
wantLoop:
            foreach (var (_, w) in want)
            {
                while (i < len(frames))
                {
                    if (w.function == frames[i].Function)
                    {
                        if (w.line != 0L && w.line != frames[i].Line)
                        {
                            fmt.Printf("found function %s at wrong line %#x (expected %#x)\n", w.function, frames[i].Line, w.line);
                            ok = false;
                    i++;
                        }
                        i++;
                        _continuewantLoop = true;
                        break;
                    }
                }

                fmt.Printf("did not find function %s in\n", w.function);
                foreach (var (_, f) in frames)
                {
                    fmt.Println(f);
                }
                ok = false;
                break;
            }
            tracebackOK = ok;
            {
                var got = C.getContextCount();

                if (got != 2L)
                {
                    fmt.Printf("at bottom contextCount == %d, expected 2\n", got);
                    tracebackOK = false;
                }

            }
        }
    }
}
