// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:17 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\print.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        // The compiler knows that a print of a value of this type
        // should use printhex instead of printuint (decimal).
        private partial struct hex // : ulong
        {
        }

        private static slice<byte> bytes(@string s)
        {
            var rp = (slice.Value)(@unsafe.Pointer(ref ret));
            var sp = stringStructOf(ref s);
            rp.array = sp.str;
            rp.len = sp.len;
            rp.cap = sp.len;
            return;
        }

 
        // printBacklog is a circular buffer of messages written with the builtin
        // print* functions, for use in postmortem analysis of core dumps.
        private static array<byte> printBacklog = new array<byte>(512L);        private static long printBacklogIndex = default;

        // recordForPanic maintains a circular buffer of messages written by the
        // runtime leading up to a process crash, allowing the messages to be
        // extracted from a core dump.
        //
        // The text written during a process crash (following "panic" or "fatal
        // error") is not saved, since the goroutine stacks will generally be readable
        // from the runtime datastructures in the core file.
        private static void recordForPanic(slice<byte> b)
        {
            printlock();

            if (atomic.Load(ref panicking) == 0L)
            { 
                // Not actively crashing: maintain circular buffer of print output.
                {
                    long i = 0L;

                    while (i < len(b))
                    {
                        var n = copy(printBacklog[printBacklogIndex..], b[i..]);
                        i += n;
                        printBacklogIndex += n;
                        printBacklogIndex %= len(printBacklog);
                    }

                }
            }
            printunlock();
        }

        private static mutex debuglock = default;

        // The compiler emits calls to printlock and printunlock around
        // the multiple calls that implement a single Go print or println
        // statement. Some of the print helpers (printslice, for example)
        // call print recursively. There is also the problem of a crash
        // happening during the print routines and needing to acquire
        // the print lock to print information about the crash.
        // For both these reasons, let a thread acquire the printlock 'recursively'.

        private static void printlock()
        {
            var mp = getg().m;
            mp.locks++; // do not reschedule between printlock++ and lock(&debuglock).
            mp.printlock++;
            if (mp.printlock == 1L)
            {
                lock(ref debuglock);
            }
            mp.locks--; // now we know debuglock is held and holding up mp.locks for us.
        }

        private static void printunlock()
        {
            var mp = getg().m;
            mp.printlock--;
            if (mp.printlock == 0L)
            {
                unlock(ref debuglock);
            }
        }

        // write to goroutine-local buffer if diverting output,
        // or else standard error.
        private static void gwrite(slice<byte> b)
        {
            if (len(b) == 0L)
            {
                return;
            }
            recordForPanic(b);
            var gp = getg();
            if (gp == null || gp.writebuf == null)
            {
                writeErr(b);
                return;
            }
            var n = copy(gp.writebuf[len(gp.writebuf)..cap(gp.writebuf)], b);
            gp.writebuf = gp.writebuf[..len(gp.writebuf) + n];
        }

        private static void printsp()
        {
            printstring(" ");
        }

        private static void printnl()
        {
            printstring("\n");
        }

        private static void printbool(bool v)
        {
            if (v)
            {
                printstring("true");
            }
            else
            {
                printstring("false");
            }
        }

        private static void printfloat(double v)
        {

            if (v != v) 
                printstring("NaN");
                return;
            else if (v + v == v && v > 0L) 
                printstring("+Inf");
                return;
            else if (v + v == v && v < 0L) 
                printstring("-Inf");
                return;
                        const long n = 7L; // digits printed
 // digits printed
            array<byte> buf = new array<byte>(n + 7L);
            buf[0L] = '+';
            long e = 0L; // exp
            if (v == 0L)
            {
                if (1L / v < 0L)
                {
                    buf[0L] = '-';
                }
            }
            else
            {
                if (v < 0L)
                {
                    v = -v;
                    buf[0L] = '-';
                } 

                // normalize
                while (v >= 10L)
                {
                    e++;
                    v /= 10L;
                }

                while (v < 1L)
                {
                    e--;
                    v *= 10L;
                } 

                // round
 

                // round
                float h = 5.0F;
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < n; i++)
                    {
                        h /= 10L;
                    }


                    i = i__prev1;
                }
                v += h;
                if (v >= 10L)
                {
                    e++;
                    v /= 10L;
                }
            } 

            // format +d.dddd+edd
            {
                long i__prev1 = i;

                for (i = 0L; i < n; i++)
                {
                    var s = int(v);
                    buf[i + 2L] = byte(s + '0');
                    v -= float64(s);
                    v *= 10L;
                }


                i = i__prev1;
            }
            buf[1L] = buf[2L];
            buf[2L] = '.';

            buf[n + 2L] = 'e';
            buf[n + 3L] = '+';
            if (e < 0L)
            {
                e = -e;
                buf[n + 3L] = '-';
            }
            buf[n + 4L] = byte(e / 100L) + '0';
            buf[n + 5L] = byte(e / 10L) % 10L + '0';
            buf[n + 6L] = byte(e % 10L) + '0';
            gwrite(buf[..]);
        }

        private static void printcomplex(System.Numerics.Complex128 c)
        {
            print("(", real(c), imag(c), "i)");
        }

        private static void printuint(ulong v)
        {
            array<byte> buf = new array<byte>(100L);
            var i = len(buf);
            i--;

            while (i > 0L)
            {
                buf[i] = byte(v % 10L + '0');
                if (v < 10L)
                {
                    break;
                i--;
                }
                v /= 10L;
            }

            gwrite(buf[i..]);
        }

        private static void printint(long v)
        {
            if (v < 0L)
            {
                printstring("-");
                v = -v;
            }
            printuint(uint64(v));
        }

        private static void printhex(ulong v)
        {
            const @string dig = "0123456789abcdef";

            array<byte> buf = new array<byte>(100L);
            var i = len(buf);
            i--;

            while (i > 0L)
            {
                buf[i] = dig[v % 16L];
                if (v < 16L)
                {
                    break;
                i--;
                }
                v /= 16L;
            }

            i--;
            buf[i] = 'x';
            i--;
            buf[i] = '0';
            gwrite(buf[i..]);
        }

        private static void printpointer(unsafe.Pointer p)
        {
            printhex(uint64(uintptr(p)));
        }

        private static void printstring(@string s)
        {
            gwrite(bytes(s));
        }

        private static void printslice(slice<byte> s)
        {
            var sp = (slice.Value)(@unsafe.Pointer(ref s));
            print("[", len(s), "/", cap(s), "]");
            printpointer(sp.array);
        }

        private static void printeface(eface e)
        {
            print("(", e._type, ",", e.data, ")");
        }

        private static void printiface(iface i)
        {
            print("(", i.tab, ",", i.data, ")");
        }

        // hexdumpWords prints a word-oriented hex dump of [p, end).
        //
        // If mark != nil, it will be called with each printed word's address
        // and should return a character mark to appear just before that
        // word's value. It can return 0 to indicate no mark.
        private static byte hexdumpWords(System.UIntPtr p, System.UIntPtr end, Func<System.UIntPtr, byte> mark)
        {
            Action<System.UIntPtr> p1 = x =>
            {
                array<byte> buf = new array<byte>(2L * sys.PtrSize);
                {
                    var i__prev1 = i;

                    for (var i = len(buf) - 1L; i >= 0L; i--)
                    {
                        if (x & 0xFUL < 10L)
                        {
                            buf[i] = byte(x & 0xFUL) + '0';
                        }
                        else
                        {
                            buf[i] = byte(x & 0xFUL) - 10L + 'a';
                        }
                        x >>= 4L;
                    }


                    i = i__prev1;
                }
                gwrite(buf[..]);
            }
;

            printlock();
            array<byte> markbuf = new array<byte>(1L);
            markbuf[0L] = ' ';
            {
                var i__prev1 = i;

                i = uintptr(0L);

                while (p + i < end)
                {
                    if (i % 16L == 0L)
                    {
                        if (i != 0L)
                        {
                            println();
                    i += sys.PtrSize;
                        }
                        p1(p + i);
                        print(": ");
                    }
                    if (mark != null)
                    {
                        markbuf[0L] = mark(p + i);
                        if (markbuf[0L] == 0L)
                        {
                            markbuf[0L] = ' ';
                        }
                    }
                    gwrite(markbuf[..]);
                    *(*System.UIntPtr) val = @unsafe.Pointer(p + i).Value;
                    p1(val);
                    print(" "); 

                    // Can we symbolize val?
                    var fn = findfunc(val);
                    if (fn.valid())
                    {
                        print("<", funcname(fn), "+", val - fn.entry, "> ");
                    }
                }


                i = i__prev1;
            }
            println();
            printunlock();
        }
    }
}
