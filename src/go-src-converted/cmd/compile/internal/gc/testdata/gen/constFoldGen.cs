// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program generates a test to verify that the standard arithmetic
// operators properly handle constant folding. The test file should be
// generated with a known working version of go.
// launch with `go run constFoldGen.go` a file called constFold_test.go
// will be written into the grandparent directory containing the tests.

// package main -- go2cs converted at 2020 August 29 09:58:49 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\gen\constFoldGen.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using format = go.go.format_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private partial struct op
        {
            public @string name;
            public @string symbol;
        }
        private partial struct szD
        {
            public @string name;
            public @string sn;
            public slice<ulong> u;
            public slice<long> i;
        }

        private static slice<szD> szs = new slice<szD>(new szD[] { szD{name:"uint64",sn:"64",u:[]uint64{0,1,4294967296,0xffffFFFFffffFFFF}}, szD{name:"int64",sn:"64",i:[]int64{-0x8000000000000000,-0x7FFFFFFFFFFFFFFF,-4294967296,-1,0,1,4294967296,0x7FFFFFFFFFFFFFFE,0x7FFFFFFFFFFFFFFF}}, szD{name:"uint32",sn:"32",u:[]uint64{0,1,4294967295}}, szD{name:"int32",sn:"32",i:[]int64{-0x80000000,-0x7FFFFFFF,-1,0,1,0x7FFFFFFF}}, szD{name:"uint16",sn:"16",u:[]uint64{0,1,65535}}, szD{name:"int16",sn:"16",i:[]int64{-32768,-32767,-1,0,1,32766,32767}}, szD{name:"uint8",sn:"8",u:[]uint64{0,1,255}}, szD{name:"int8",sn:"8",i:[]int64{-128,-127,-1,0,1,126,127}} });

        private static op ops = new slice<op>(new op[] { op{"add","+"}, op{"sub","-"}, op{"div","/"}, op{"mul","*"}, op{"lsh","<<"}, op{"rsh",">>"}, op{"mod","%"} });

        // compute the result of i op j, cast as type t.
        private static @string ansU(ulong i, ulong j, @string t, @string op)
        {
            ulong ans = default;
            switch (op)
            {
                case "+": 
                    ans = i + j;
                    break;
                case "-": 
                    ans = i - j;
                    break;
                case "*": 
                    ans = i * j;
                    break;
                case "/": 
                    if (j != 0L)
                    {
                        ans = i / j;
                    }
                    break;
                case "%": 
                    if (j != 0L)
                    {
                        ans = i % j;
                    }
                    break;
                case "<<": 
                    ans = i << (int)(j);
                    break;
                case ">>": 
                    ans = i >> (int)(j);
                    break;
            }
            switch (t)
            {
                case "uint32": 
                    ans = uint64(uint32(ans));
                    break;
                case "uint16": 
                    ans = uint64(uint16(ans));
                    break;
                case "uint8": 
                    ans = uint64(uint8(ans));
                    break;
            }
            return fmt.Sprintf("%d", ans);
        }

        // compute the result of i op j, cast as type t.
        private static @string ansS(long i, long j, @string t, @string op)
        {
            long ans = default;
            switch (op)
            {
                case "+": 
                    ans = i + j;
                    break;
                case "-": 
                    ans = i - j;
                    break;
                case "*": 
                    ans = i * j;
                    break;
                case "/": 
                    if (j != 0L)
                    {
                        ans = i / j;
                    }
                    break;
                case "%": 
                    if (j != 0L)
                    {
                        ans = i % j;
                    }
                    break;
                case "<<": 
                    ans = i << (int)(uint64(j));
                    break;
                case ">>": 
                    ans = i >> (int)(uint64(j));
                    break;
            }
            switch (t)
            {
                case "int32": 
                    ans = int64(int32(ans));
                    break;
                case "int16": 
                    ans = int64(int16(ans));
                    break;
                case "int8": 
                    ans = int64(int8(ans));
                    break;
            }
            return fmt.Sprintf("%d", ans);
        }

        private static void Main() => func((_, panic, __) =>
        {
            ptr<object> w = @new<bytes.Buffer>();
            fmt.Fprintf(w, "// run\n");
            fmt.Fprintf(w, "// Code generated by gen/constFoldGen.go. DO NOT EDIT.\n\n");
            fmt.Fprintf(w, "package gc\n");
            fmt.Fprintf(w, "import \"testing\"\n");

            {
                var s__prev1 = s;

                foreach (var (_, __s) in szs)
                {
                    s = __s;
                    {
                        var o__prev2 = o;

                        foreach (var (_, __o) in ops)
                        {
                            o = __o;
                            if (o.symbol == "<<" || o.symbol == ">>")
                            { 
                                // shifts handled separately below, as they can have
                                // different types on the LHS and RHS.
                                continue;
                            }
                            fmt.Fprintf(w, "func TestConstFold%s%s(t *testing.T) {\n", s.name, o.name);
                            fmt.Fprintf(w, "\tvar x, y, r %s\n", s.name); 
                            // unsigned test cases
                            {
                                var c__prev3 = c;

                                foreach (var (_, __c) in s.u)
                                {
                                    c = __c;
                                    fmt.Fprintf(w, "\tx = %d\n", c);
                                    {
                                        var d__prev4 = d;

                                        foreach (var (_, __d) in s.u)
                                        {
                                            d = __d;
                                            if (d == 0L && (o.symbol == "/" || o.symbol == "%"))
                                            {
                                                continue;
                                            }
                                            fmt.Fprintf(w, "\ty = %d\n", d);
                                            fmt.Fprintf(w, "\tr = x %s y\n", o.symbol);
                                            var want = ansU(c, d, s.name, o.symbol);
                                            fmt.Fprintf(w, "\tif r != %s {\n", want);
                                            fmt.Fprintf(w, "\t\tt.Errorf(\"%d %%s %d = %%d, want %s\", %q, r)\n", c, d, want, o.symbol);
                                            fmt.Fprintf(w, "\t}\n");
                                        }

                                        d = d__prev4;
                                    }

                                } 
                                // signed test cases

                                c = c__prev3;
                            }

                            {
                                var c__prev3 = c;

                                foreach (var (_, __c) in s.i)
                                {
                                    c = __c;
                                    fmt.Fprintf(w, "\tx = %d\n", c);
                                    {
                                        var d__prev4 = d;

                                        foreach (var (_, __d) in s.i)
                                        {
                                            d = __d;
                                            if (d == 0L && (o.symbol == "/" || o.symbol == "%"))
                                            {
                                                continue;
                                            }
                                            fmt.Fprintf(w, "\ty = %d\n", d);
                                            fmt.Fprintf(w, "\tr = x %s y\n", o.symbol);
                                            want = ansS(c, d, s.name, o.symbol);
                                            fmt.Fprintf(w, "\tif r != %s {\n", want);
                                            fmt.Fprintf(w, "\t\tt.Errorf(\"%d %%s %d = %%d, want %s\", %q, r)\n", c, d, want, o.symbol);
                                            fmt.Fprintf(w, "\t}\n");
                                        }

                                        d = d__prev4;
                                    }

                                }

                                c = c__prev3;
                            }

                            fmt.Fprintf(w, "}\n");
                        }

                        o = o__prev2;
                    }

                } 

                // Special signed/unsigned cases for shifts

                s = s__prev1;
            }

            foreach (var (_, ls) in szs)
            {
                foreach (var (_, rs) in szs)
                {
                    if (rs.name[0L] != 'u')
                    {
                        continue;
                    }
                    {
                        var o__prev3 = o;

                        foreach (var (_, __o) in ops)
                        {
                            o = __o;
                            if (o.symbol != "<<" && o.symbol != ">>")
                            {
                                continue;
                            }
                            fmt.Fprintf(w, "func TestConstFold%s%s%s(t *testing.T) {\n", ls.name, rs.name, o.name);
                            fmt.Fprintf(w, "\tvar x, r %s\n", ls.name);
                            fmt.Fprintf(w, "\tvar y %s\n", rs.name); 
                            // unsigned LHS
                            {
                                var c__prev4 = c;

                                foreach (var (_, __c) in ls.u)
                                {
                                    c = __c;
                                    fmt.Fprintf(w, "\tx = %d\n", c);
                                    {
                                        var d__prev5 = d;

                                        foreach (var (_, __d) in rs.u)
                                        {
                                            d = __d;
                                            fmt.Fprintf(w, "\ty = %d\n", d);
                                            fmt.Fprintf(w, "\tr = x %s y\n", o.symbol);
                                            want = ansU(c, d, ls.name, o.symbol);
                                            fmt.Fprintf(w, "\tif r != %s {\n", want);
                                            fmt.Fprintf(w, "\t\tt.Errorf(\"%d %%s %d = %%d, want %s\", %q, r)\n", c, d, want, o.symbol);
                                            fmt.Fprintf(w, "\t}\n");
                                        }

                                        d = d__prev5;
                                    }

                                } 
                                // signed LHS

                                c = c__prev4;
                            }

                            {
                                var c__prev4 = c;

                                foreach (var (_, __c) in ls.i)
                                {
                                    c = __c;
                                    fmt.Fprintf(w, "\tx = %d\n", c);
                                    {
                                        var d__prev5 = d;

                                        foreach (var (_, __d) in rs.u)
                                        {
                                            d = __d;
                                            fmt.Fprintf(w, "\ty = %d\n", d);
                                            fmt.Fprintf(w, "\tr = x %s y\n", o.symbol);
                                            want = ansS(c, int64(d), ls.name, o.symbol);
                                            fmt.Fprintf(w, "\tif r != %s {\n", want);
                                            fmt.Fprintf(w, "\t\tt.Errorf(\"%d %%s %d = %%d, want %s\", %q, r)\n", c, d, want, o.symbol);
                                            fmt.Fprintf(w, "\t}\n");
                                        }

                                        d = d__prev5;
                                    }

                                }

                                c = c__prev4;
                            }

                            fmt.Fprintf(w, "}\n");
                        }

                        o = o__prev3;
                    }

                }
            } 

            // Constant folding for comparisons
            {
                var s__prev1 = s;

                foreach (var (_, __s) in szs)
                {
                    s = __s;
                    fmt.Fprintf(w, "func TestConstFoldCompare%s(t *testing.T) {\n", s.name);
                    {
                        var x__prev2 = x;

                        foreach (var (_, __x) in s.i)
                        {
                            x = __x;
                            {
                                var y__prev3 = y;

                                foreach (var (_, __y) in s.i)
                                {
                                    y = __y;
                                    fmt.Fprintf(w, "\t{\n");
                                    fmt.Fprintf(w, "\t\tvar x %s = %d\n", s.name, x);
                                    fmt.Fprintf(w, "\t\tvar y %s = %d\n", s.name, y);
                                    if (x == y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x == y) { t.Errorf(\"!(%%d == %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x == y { t.Errorf(\"%%d == %%d\", x, y) }\n");
                                    }
                                    if (x != y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x != y) { t.Errorf(\"!(%%d != %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x != y { t.Errorf(\"%%d != %%d\", x, y) }\n");
                                    }
                                    if (x < y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x < y) { t.Errorf(\"!(%%d < %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x < y { t.Errorf(\"%%d < %%d\", x, y) }\n");
                                    }
                                    if (x > y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x > y) { t.Errorf(\"!(%%d > %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x > y { t.Errorf(\"%%d > %%d\", x, y) }\n");
                                    }
                                    if (x <= y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x <= y) { t.Errorf(\"!(%%d <= %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x <= y { t.Errorf(\"%%d <= %%d\", x, y) }\n");
                                    }
                                    if (x >= y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x >= y) { t.Errorf(\"!(%%d >= %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x >= y { t.Errorf(\"%%d >= %%d\", x, y) }\n");
                                    }
                                    fmt.Fprintf(w, "\t}\n");
                                }

                                y = y__prev3;
                            }

                        }

                        x = x__prev2;
                    }

                    {
                        var x__prev2 = x;

                        foreach (var (_, __x) in s.u)
                        {
                            x = __x;
                            {
                                var y__prev3 = y;

                                foreach (var (_, __y) in s.u)
                                {
                                    y = __y;
                                    fmt.Fprintf(w, "\t{\n");
                                    fmt.Fprintf(w, "\t\tvar x %s = %d\n", s.name, x);
                                    fmt.Fprintf(w, "\t\tvar y %s = %d\n", s.name, y);
                                    if (x == y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x == y) { t.Errorf(\"!(%%d == %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x == y { t.Errorf(\"%%d == %%d\", x, y) }\n");
                                    }
                                    if (x != y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x != y) { t.Errorf(\"!(%%d != %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x != y { t.Errorf(\"%%d != %%d\", x, y) }\n");
                                    }
                                    if (x < y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x < y) { t.Errorf(\"!(%%d < %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x < y { t.Errorf(\"%%d < %%d\", x, y) }\n");
                                    }
                                    if (x > y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x > y) { t.Errorf(\"!(%%d > %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x > y { t.Errorf(\"%%d > %%d\", x, y) }\n");
                                    }
                                    if (x <= y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x <= y) { t.Errorf(\"!(%%d <= %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x <= y { t.Errorf(\"%%d <= %%d\", x, y) }\n");
                                    }
                                    if (x >= y)
                                    {
                                        fmt.Fprintf(w, "\t\tif !(x >= y) { t.Errorf(\"!(%%d >= %%d)\", x, y) }\n");
                                    }
                                    else
                                    {
                                        fmt.Fprintf(w, "\t\tif x >= y { t.Errorf(\"%%d >= %%d\", x, y) }\n");
                                    }
                                    fmt.Fprintf(w, "\t}\n");
                                }

                                y = y__prev3;
                            }

                        }

                        x = x__prev2;
                    }

                    fmt.Fprintf(w, "}\n");
                } 

                // gofmt result

                s = s__prev1;
            }

            var b = w.Bytes();
            var (src, err) = format.Source(b);
            if (err != null)
            {
                fmt.Printf("%s\n", b);
                panic(err);
            } 

            // write to file
            err = ioutil.WriteFile("../../constFold_test.go", src, 0666L);
            if (err != null)
            {
                log.Fatalf("can't write output: %v\n", err);
            }
        });
    }
}
