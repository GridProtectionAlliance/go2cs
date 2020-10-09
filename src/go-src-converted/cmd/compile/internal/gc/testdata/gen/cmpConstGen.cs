// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program generates a test to verify that the standard comparison
// operators properly handle one const operand. The test file should be
// generated with a known working version of go.
// launch with `go run cmpConstGen.go` a file called cmpConst.go
// will be written into the parent directory containing the tests

// package main -- go2cs converted at 2020 October 09 05:43:55 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\gen\cmpConstGen.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using format = go.go.format_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using big = go.math.big_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static readonly long maxU64 = (long)(1L << (int)(64L)) - 1L;
        private static readonly long maxU32 = (long)(1L << (int)(32L)) - 1L;
        private static readonly long maxU16 = (long)(1L << (int)(16L)) - 1L;
        private static readonly long maxU8 = (long)(1L << (int)(8L)) - 1L;

        private static readonly long maxI64 = (long)(1L << (int)(63L)) - 1L;
        private static readonly long maxI32 = (long)(1L << (int)(31L)) - 1L;
        private static readonly long maxI16 = (long)(1L << (int)(15L)) - 1L;
        private static readonly long maxI8 = (long)(1L << (int)(7L)) - 1L;

        private static readonly long minI64 = (long)-(1L << (int)(63L));
        private static readonly long minI32 = (long)-(1L << (int)(31L));
        private static readonly long minI16 = (long)-(1L << (int)(15L));
        private static readonly long minI8 = (long)-(1L << (int)(7L));


        private static bool cmp(ptr<big.Int> _addr_left, @string op, ptr<big.Int> _addr_right) => func((_, panic, __) =>
        {
            ref big.Int left = ref _addr_left.val;
            ref big.Int right = ref _addr_right.val;

            switch (left.Cmp(right))
            {
                case -1L: // less than
                    return op == "<" || op == "<=" || op == "!=";
                    break;
                case 0L: // equal
                    return op == "==" || op == "<=" || op == ">=";
                    break;
                case 1L: // greater than
                    return op == ">" || op == ">=" || op == "!=";
                    break;
            }
            panic("unexpected comparison value");

        });

        private static bool inRange(@string typ, ptr<big.Int> _addr_val) => func((_, panic, __) =>
        {
            ref big.Int val = ref _addr_val.val;

            ptr<big.Int> min = addr(new big.Int());
            ptr<big.Int> max = addr(new big.Int());
            switch (typ)
            {
                case "uint64": 
                    max = max.SetUint64(maxU64);
                    break;
                case "uint32": 
                    max = max.SetUint64(maxU32);
                    break;
                case "uint16": 
                    max = max.SetUint64(maxU16);
                    break;
                case "uint8": 
                    max = max.SetUint64(maxU8);
                    break;
                case "int64": 
                    min = min.SetInt64(minI64);
                    max = max.SetInt64(maxI64);
                    break;
                case "int32": 
                    min = min.SetInt64(minI32);
                    max = max.SetInt64(maxI32);
                    break;
                case "int16": 
                    min = min.SetInt64(minI16);
                    max = max.SetInt64(maxI16);
                    break;
                case "int8": 
                    min = min.SetInt64(minI8);
                    max = max.SetInt64(maxI8);
                    break;
                default: 
                    panic("unexpected type");
                    break;
            }
            return cmp(min, "<=", _addr_val) && cmp(_addr_val, "<=", max);

        });

        private static slice<ptr<big.Int>> getValues(@string typ)
        {
            Func<ulong, ptr<big.Int>> Uint = v => big.NewInt(0L).SetUint64(v);
            Func<long, ptr<big.Int>> Int = v => big.NewInt(0L).SetInt64(v);
            ptr<big.Int> values = new slice<ptr<big.Int>>(new ptr<big.Int>[] { Uint(maxU64), Uint(maxU64-1), Uint(maxI64+1), Uint(maxI64), Uint(maxI64-1), Uint(maxU32+1), Uint(maxU32), Uint(maxU32-1), Uint(maxI32+1), Uint(maxI32), Uint(maxI32-1), Uint(maxU16+1), Uint(maxU16), Uint(maxU16-1), Uint(maxI16+1), Uint(maxI16), Uint(maxI16-1), Uint(maxU8+1), Uint(maxU8), Uint(maxU8-1), Uint(maxI8+1), Uint(maxI8), Uint(maxI8-1), Uint(0), Int(minI8+1), Int(minI8), Int(minI8-1), Int(minI16+1), Int(minI16), Int(minI16-1), Int(minI32+1), Int(minI32), Int(minI32-1), Int(minI64+1), Int(minI64), Uint(1), Int(-1), Uint(0xff<<56), Uint(0xff<<32), Uint(0xff<<24) });
            sort.Slice(values, (i, j) => values[i].Cmp(values[j]) == -1L);
            slice<ptr<big.Int>> ret = default;
            foreach (var (_, val) in values)
            {
                if (!inRange(typ, _addr_val))
                {
                    continue;
                }

                ret = append(ret, val);

            }
            return ret;

        }

        private static @string sigString(ptr<big.Int> _addr_v)
        {
            ref big.Int v = ref _addr_v.val;

            big.Int t = default;
            t.Abs(v);
            if (v.Sign() == -1L)
            {
                return "neg" + t.String();
            }

            return t.String();

        }

        private static void Main() => func((_, panic, __) =>
        {
            @string types = new slice<@string>(new @string[] { "uint64", "uint32", "uint16", "uint8", "int64", "int32", "int16", "int8" });

            ptr<object> w = @new<bytes.Buffer>();
            fmt.Fprintf(w, "// Code generated by gen/cmpConstGen.go. DO NOT EDIT.\n\n");
            fmt.Fprintf(w, "package main;\n");
            fmt.Fprintf(w, "import (\"testing\"; \"reflect\"; \"runtime\";)\n");
            fmt.Fprintf(w, "// results show the expected result for the elements left of, equal to and right of the index.\n");
            fmt.Fprintf(w, "type result struct{l, e, r bool}\n");
            fmt.Fprintf(w, "var (\n");
            fmt.Fprintf(w, "	eq = result{l: false, e: true, r: false}\n");
            fmt.Fprintf(w, "	ne = result{l: true, e: false, r: true}\n");
            fmt.Fprintf(w, "	lt = result{l: true, e: false, r: false}\n");
            fmt.Fprintf(w, "	le = result{l: true, e: true, r: false}\n");
            fmt.Fprintf(w, "	gt = result{l: false, e: false, r: true}\n");
            fmt.Fprintf(w, "	ge = result{l: false, e: true, r: true}\n");
            fmt.Fprintf(w, ")\n");

            {
                var typ__prev1 = typ;

                foreach (var (_, __typ) in types)
                {
                    typ = __typ; 
                    // generate a slice containing valid values for this type
                    fmt.Fprintf(w, "\n// %v tests\n", typ);
                    var values = getValues(typ);
                    fmt.Fprintf(w, "var %v_vals = []%v{\n", typ, typ);
                    foreach (var (_, val) in values)
                    {
                        fmt.Fprintf(w, "%v,\n", val.String());
                    }
                    fmt.Fprintf(w, "}\n"); 

                    // generate test functions
                    {
                        var r__prev2 = r;

                        foreach (var (_, __r) in values)
                        {
                            r = __r; 
                            // TODO: could also test constant on lhs.
                            var sig = sigString(_addr_r);
                            {
                                var op__prev3 = op;

                                foreach (var (_, __op) in operators)
                                {
                                    op = __op; 
                                    // no need for go:noinline because the function is called indirectly
                                    fmt.Fprintf(w, "func %v_%v_%v(x %v) bool { return x %v %v; }\n", op.name, sig, typ, typ, op.op, r.String());

                                }

                                op = op__prev3;
                            }
                        } 

                        // generate a table of test cases

                        r = r__prev2;
                    }

                    fmt.Fprintf(w, "var %v_tests = []struct{\n", typ);
                    fmt.Fprintf(w, "	idx int // index of the constant used\n");
                    fmt.Fprintf(w, "	exp result // expected results\n");
                    fmt.Fprintf(w, "	fn  func(%v) bool\n", typ);
                    fmt.Fprintf(w, "}{\n");
                    {
                        var r__prev2 = r;

                        foreach (var (__i, __r) in values)
                        {
                            i = __i;
                            r = __r;
                            sig = sigString(_addr_r);
                            {
                                var op__prev3 = op;

                                foreach (var (_, __op) in operators)
                                {
                                    op = __op;
                                    fmt.Fprintf(w, "{idx: %v,", i);
                                    fmt.Fprintf(w, "exp: %v,", op.name);
                                    fmt.Fprintf(w, "fn:  %v_%v_%v},\n", op.name, sig, typ);
                                }

                                op = op__prev3;
                            }
                        }

                        r = r__prev2;
                    }

                    fmt.Fprintf(w, "}\n");

                } 

                // emit the main function, looping over all test cases

                typ = typ__prev1;
            }

            fmt.Fprintf(w, "// TestComparisonsConst tests results for comparison operations against constants.\n");
            fmt.Fprintf(w, "func TestComparisonsConst(t *testing.T) {\n");
            {
                var typ__prev1 = typ;

                foreach (var (_, __typ) in types)
                {
                    typ = __typ;
                    fmt.Fprintf(w, "for i, test := range %v_tests {\n", typ);
                    fmt.Fprintf(w, "	for j, x := range %v_vals {\n", typ);
                    fmt.Fprintf(w, "		want := test.exp.l\n");
                    fmt.Fprintf(w, "		if j == test.idx {\nwant = test.exp.e\n}");
                    fmt.Fprintf(w, "		else if j > test.idx {\nwant = test.exp.r\n}\n");
                    fmt.Fprintf(w, "		if test.fn(x) != want {\n");
                    fmt.Fprintf(w, "			fn := runtime.FuncForPC(reflect.ValueOf(test.fn).Pointer()).Name()\n");
                    fmt.Fprintf(w, "			t.Errorf(\"test failed: %%v(%%v) != %%v [type=%v i=%%v j=%%v idx=%%v]\", fn, x, want, i, j, test.idx)\n", typ);
                    fmt.Fprintf(w, "		}\n");
                    fmt.Fprintf(w, "	}\n");
                    fmt.Fprintf(w, "}\n");
                }

                typ = typ__prev1;
            }

            fmt.Fprintf(w, "}\n"); 

            // gofmt result
            var b = w.Bytes();
            var (src, err) = format.Source(b);
            if (err != null)
            {
                fmt.Printf("%s\n", b);
                panic(err);
            } 

            // write to file
            err = ioutil.WriteFile("../cmpConst_test.go", src, 0666L);
            if (err != null)
            {
                log.Fatalf("can't write output: %v\n", err);
            }

        });
    }
}
