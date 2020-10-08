// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This is the input program for an end-to-end test of the DWARF produced
// by the compiler. It is compiled with various flags, then the resulting
// binary is "debugged" under the control of a harness.  Because the compile+debug
// step is time-consuming, the tests for different bugs are all accumulated here
// so that their cost is only the time to "n" through the additional code.

// package main -- go2cs converted at 2020 October 08 04:27:09 UTC
// Original source: C:\Go\src\cmd\compile\internal\ssa\testdata\hist.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private partial struct point
        {
            public long x;
            public long y;
        }

        private partial struct line
        {
            public point begin;
            public point end;
        }

        private static long zero = default;
        private static long sink = default;

        //go:noinline
        private static void tinycall()
        {
        }

        private static slice<long> ensure(long n, slice<long> sl)
        {
            while (len(sl) <= n)
            {
                sl = append(sl, 0L);
            }

            return sl;

        }

        private static @string cannedInput = "1\n1\n1\n2\n2\n2\n4\n4\n5\n";

        private static void test()
        { 
            // For #19868
            line l = new line(point{1+zero,2+zero},point{3+zero,4+zero});
            tinycall(); // this forces l etc to stack
            var dx = l.end.x - l.begin.x; //gdb-dbg=(l.begin.x,l.end.y)//gdb-opt=(l,dx/O,dy/O)
            var dy = l.end.y - l.begin.y; //gdb-opt=(dx,dy/O)
            sink = dx + dy; //gdb-opt=(dx,dy)
            // For #21098
            var hist = make_slice<long>(7L); //gdb-opt=(dx/O,dy/O) // TODO sink is missing if this code is in 'test' instead of 'main'
            io.Reader reader = strings.NewReader(cannedInput); //gdb-dbg=(hist/A) // TODO cannedInput/A is missing if this code is in 'test' instead of 'main'
            if (len(os.Args) > 1L)
            {
                error err = default!;
                reader, err = os.Open(os.Args[1L]);
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "There was an error opening %s: %v\n", os.Args[1L], err);
                    return ;
                }

            }

            var scanner = bufio.NewScanner(reader);
            while (scanner.Scan())
            { //gdb-opt=(scanner/A)
                var s = scanner.Text();
                var (i, err) = strconv.ParseInt(s, 10L, 64L);
                if (err != null)
                { //gdb-dbg=(i) //gdb-opt=(err,hist,i)
                    fmt.Fprintf(os.Stderr, "There was an error: %v\n", err);
                    return ;

                }

                hist = ensure(int(i), hist);
                hist[int(i)]++;

            }

            long t = 0L;
            long n = 0L;
            {
                var i__prev1 = i;

                foreach (var (__i, __a) in hist)
                {
                    i = __i;
                    a = __a;
                    if (a == 0L)
                    { //gdb-opt=(a,n,t)
                        continue;

                    }

                    t += i * a;
                    n += a;
                    fmt.Fprintf(os.Stderr, "%d\t%d\t%d\t%d\t%d\n", i, a, n, i * a, t); //gdb-dbg=(n,i,t)
                }

                i = i__prev1;
            }
        }

        private static void Main()
        {
            growstack(); // Use stack early to prevent growth during test, which confuses gdb
            test();

        }

        private static @string snk = default;

        //go:noinline
        private static void growstack()
        {
            snk = fmt.Sprintf("%#v,%#v,%#v", 1L, true, "cat");
        }
    }
}
