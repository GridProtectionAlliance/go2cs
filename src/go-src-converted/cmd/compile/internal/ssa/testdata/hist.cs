// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This is the input program for an end-to-end test of the DWARF produced
// by the compiler. It is compiled with various flags, then the resulting
// binary is "debugged" under the control of a harness.  Because the compile+debug
// step is time-consuming, the tests for different bugs are all accumulated here
// so that their cost is only the time to "n" through the additional code.

// package main -- go2cs converted at 2022 March 06 23:09:37 UTC
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\testdata\hist.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

namespace go;

public static partial class main_package {

private partial struct point {
    public nint x;
    public nint y;
}

private partial struct line {
    public point begin;
    public point end;
}

private static nint zero = default;
private static nint sink = default;

//go:noinline
private static void tinycall() {
}

private static slice<nint> ensure(nint n, slice<nint> sl) {
    while (len(sl) <= n) {
        sl = append(sl, 0);
    }
    return sl;
}

private static @string cannedInput = "1\n1\n1\n2\n2\n2\n4\n4\n5\n";

private static void test() { 
    // For #19868
    line l = new line(point{1+zero,2+zero},point{3+zero,4+zero});
    tinycall(); // this forces l etc to stack
    var dx = l.end.x - l.begin.x; //gdb-dbg=(l.begin.x,l.end.y)//gdb-opt=(l,dx/O,dy/O)
    var dy = l.end.y - l.begin.y; //gdb-opt=(dx,dy/O)
    sink = dx + dy; //gdb-opt=(dx,dy)
    // For #21098
    var hist = make_slice<nint>(7); //gdb-opt=(dx/O,dy/O) // TODO sink is missing if this code is in 'test' instead of 'main'
    io.Reader reader = strings.NewReader(cannedInput); //gdb-dbg=(hist/A) // TODO cannedInput/A is missing if this code is in 'test' instead of 'main'
    if (len(os.Args) > 1) {
        error err = default!;
        reader, err = os.Open(os.Args[1]);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "There was an error opening %s: %v\n", os.Args[1], err);
            return ;
        }
    }
    var scanner = bufio.NewScanner(reader);
    while (scanner.Scan()) { //gdb-opt=(scanner/A)
        var s = scanner.Text();
        var (i, err) = strconv.ParseInt(s, 10, 64);
        if (err != null) { //gdb-dbg=(i) //gdb-opt=(err,hist,i)
            fmt.Fprintf(os.Stderr, "There was an error: %v\n", err);
            return ;

        }
        hist = ensure(int(i), hist);
        hist[int(i)]++;

    }
    nint t = 0;
    nint n = 0;
    {
        var i__prev1 = i;

        foreach (var (__i, __a) in hist) {
            i = __i;
            a = __a;
            if (a == 0) { //gdb-opt=(a,n,t)
                continue;

            }

            t += i * a;
            n += a;
            fmt.Fprintf(os.Stderr, "%d\t%d\t%d\t%d\t%d\n", i, a, n, i * a, t); //gdb-dbg=(n,i,t)
        }
        i = i__prev1;
    }
}

private static void Main() {
    growstack(); // Use stack early to prevent growth during test, which confuses gdb
    test();

}

private static @string snk = default;

//go:noinline
private static void growstack() {
    snk = fmt.Sprintf("%#v,%#v,%#v", 1, true, "cat");
}

} // end main_package
