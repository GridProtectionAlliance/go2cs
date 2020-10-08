// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program generates a test to verify that the standard arithmetic
// operators properly handle const cases. The test file should be
// generated with a known working version of go.
// launch with `go run arithConstGen.go` a file called arithConst.go
// will be written into the parent directory containing the tests

// package main -- go2cs converted at 2020 October 08 04:32:00 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\gen\arithConstGen.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using format = go.go.format_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using strings = go.strings_package;
using template = go.text.template_package;
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
            public @string oponly;
        }

        private static szD szs = new slice<szD>(new szD[] { {name:"uint64",sn:"64",u:[]uint64{0,1,4294967296,0x8000000000000000,0xffffFFFFffffFFFF}}, {name:"uint64",sn:"64",u:[]uint64{3,5,7,9,10,11,13,19,21,25,27,37,41,45,73,81},oponly:"mul"}, {name:"int64",sn:"64",i:[]int64{-0x8000000000000000,-0x7FFFFFFFFFFFFFFF,-4294967296,-1,0,1,4294967296,0x7FFFFFFFFFFFFFFE,0x7FFFFFFFFFFFFFFF}}, {name:"int64",sn:"64",i:[]int64{-9,-5,-3,3,5,7,9,10,11,13,19,21,25,27,37,41,45,73,81},oponly:"mul"}, {name:"uint32",sn:"32",u:[]uint64{0,1,4294967295}}, {name:"uint32",sn:"32",u:[]uint64{3,5,7,9,10,11,13,19,21,25,27,37,41,45,73,81},oponly:"mul"}, {name:"int32",sn:"32",i:[]int64{-0x80000000,-0x7FFFFFFF,-1,0,1,0x7FFFFFFF}}, {name:"int32",sn:"32",i:[]int64{-9,-5,-3,3,5,7,9,10,11,13,19,21,25,27,37,41,45,73,81},oponly:"mul"}, {name:"uint16",sn:"16",u:[]uint64{0,1,65535}}, {name:"int16",sn:"16",i:[]int64{-32768,-32767,-1,0,1,32766,32767}}, {name:"uint8",sn:"8",u:[]uint64{0,1,255}}, {name:"int8",sn:"8",i:[]int64{-128,-127,-1,0,1,126,127}} });

        private static op ops = new slice<op>(new op[] { {"add","+"}, {"sub","-"}, {"div","/"}, {"mul","*"}, {"lsh","<<"}, {"rsh",">>"}, {"mod","%"}, {"and","&"}, {"or","|"}, {"xor","^"} });

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
                case "&": 
                    ans = i & j;
                    break;
                case "|": 
                    ans = i | j;
                    break;
                case "^": 
                    ans = i ^ j;
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
                case "&": 
                    ans = i & j;
                    break;
                case "|": 
                    ans = i | j;
                    break;
                case "^": 
                    ans = i ^ j;
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
            fmt.Fprintf(w, "// Code generated by gen/arithConstGen.go. DO NOT EDIT.\n\n");
            fmt.Fprintf(w, "package main;\n");
            fmt.Fprintf(w, "import \"testing\"\n");

            var fncCnst1 = template.Must(template.New("fnc").Parse("//go:noinline\nfunc {{.Name}}_{{.Type_}}_{{.FNumber}}(a {{.Type_}}) {{.Type_}} { r" +
    "eturn a {{.Symbol}} {{.Number}} }\n"));
            var fncCnst2 = template.Must(template.New("fnc").Parse("//go:noinline\nfunc {{.Name}}_{{.FNumber}}_{{.Type_}}(a {{.Type_}}) {{.Type_}} { r" +
    "eturn {{.Number}} {{.Symbol}} a }\n"));

            private partial struct fncData
            {
                public @string Name;
                public @string Type_;
                public @string Symbol;
                public @string FNumber;
                public @string Number;
            }

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
                            if (s.oponly != "" && s.oponly != o.name)
                            {
                                continue;
                            }

                            fncData fd = new fncData(o.name,s.name,o.symbol,"",""); 

                            // unsigned test cases
                            if (len(s.u) > 0L)
                            {
                                {
                                    var i__prev3 = i;

                                    foreach (var (_, __i) in s.u)
                                    {
                                        i = __i;
                                        fd.Number = fmt.Sprintf("%d", i);
                                        fd.FNumber = strings.Replace(fd.Number, "-", "Neg", -1L); 

                                        // avoid division by zero
                                        if (o.name != "mod" && o.name != "div" || i != 0L)
                                        { 
                                            // introduce uint64 cast for rhs shift operands
                                            // if they are too large for default uint type
                                            var number = fd.Number;
                                            if ((o.name == "lsh" || o.name == "rsh") && uint64(uint32(i)) != i)
                                            {
                                                fd.Number = fmt.Sprintf("uint64(%s)", number);
                                            }

                                            fncCnst1.Execute(w, fd);
                                            fd.Number = number;

                                        }

                                        fncCnst2.Execute(w, fd);

                                    }

                                    i = i__prev3;
                                }
                            } 

                            // signed test cases
                            if (len(s.i) > 0L)
                            { 
                                // don't generate tests for shifts by signed integers
                                if (o.name == "lsh" || o.name == "rsh")
                                {
                                    continue;
                                }

                                {
                                    var i__prev3 = i;

                                    foreach (var (_, __i) in s.i)
                                    {
                                        i = __i;
                                        fd.Number = fmt.Sprintf("%d", i);
                                        fd.FNumber = strings.Replace(fd.Number, "-", "Neg", -1L); 

                                        // avoid division by zero
                                        if (o.name != "mod" && o.name != "div" || i != 0L)
                                        {
                                            fncCnst1.Execute(w, fd);
                                        }

                                        fncCnst2.Execute(w, fd);

                                    }

                                    i = i__prev3;
                                }
                            }

                        }

                        o = o__prev2;
                    }
                }

                s = s__prev1;
            }

            var vrf1 = template.Must(template.New("vrf1").Parse("\n\t\ttest_{{.Size}}{fn: {{.Name}}_{{.FNumber}}_{{.Type_}}, fnname: \"{{.Name}}_{{.FN" +
    "umber}}_{{.Type_}}\", in: {{.Input}}, want: {{.Ans}}},"));

            var vrf2 = template.Must(template.New("vrf2").Parse("\n\t\ttest_{{.Size}}{fn: {{.Name}}_{{.Type_}}_{{.FNumber}}, fnname: \"{{.Name}}_{{.Ty" +
    "pe_}}_{{.FNumber}}\", in: {{.Input}}, want: {{.Ans}}},"));

            private partial struct cfncData
            {
                public @string Size;
                public @string Name;
                public @string Type_;
                public @string Symbol;
                public @string FNumber;
                public @string Number;
                public @string Ans;
                public @string Input;
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in szs)
                {
                    s = __s;
                    fmt.Fprintf(w, "\ntype test_%[1]s%[2]s struct {\n\tfn func (%[1]s) %[1]s\n\tfnname string\n\tin %[1]s\n\tw" +
    "ant %[1]s\n}\n", s.name, s.oponly);
                    fmt.Fprintf(w, "var tests_%[1]s%[2]s =[]test_%[1]s {\n\n", s.name, s.oponly);

                    if (len(s.u) > 0L)
                    {
                        {
                            var o__prev2 = o;

                            foreach (var (_, __o) in ops)
                            {
                                o = __o;
                                if (s.oponly != "" && s.oponly != o.name)
                                {
                                    continue;
                                }

                                fd = new cfncData(s.name,o.name,s.name,o.symbol,"","","","");
                                {
                                    var i__prev3 = i;

                                    foreach (var (_, __i) in s.u)
                                    {
                                        i = __i;
                                        fd.Number = fmt.Sprintf("%d", i);
                                        fd.FNumber = strings.Replace(fd.Number, "-", "Neg", -1L); 

                                        // unsigned
                                        {
                                            var j__prev4 = j;

                                            foreach (var (_, __j) in s.u)
                                            {
                                                j = __j;
                                                if (o.name != "mod" && o.name != "div" || j != 0L)
                                                {
                                                    fd.Ans = ansU(i, j, s.name, o.symbol);
                                                    fd.Input = fmt.Sprintf("%d", j);
                                                    {
                                                        var err__prev3 = err;

                                                        var err = vrf1.Execute(w, fd);

                                                        if (err != null)
                                                        {
                                                            panic(err);
                                                        }

                                                        err = err__prev3;

                                                    }

                                                }

                                                if (o.name != "mod" && o.name != "div" || i != 0L)
                                                {
                                                    fd.Ans = ansU(j, i, s.name, o.symbol);
                                                    fd.Input = fmt.Sprintf("%d", j);
                                                    {
                                                        var err__prev3 = err;

                                                        err = vrf2.Execute(w, fd);

                                                        if (err != null)
                                                        {
                                                            panic(err);
                                                        }

                                                        err = err__prev3;

                                                    }

                                                }

                                            }

                                            j = j__prev4;
                                        }
                                    }

                                    i = i__prev3;
                                }
                            }

                            o = o__prev2;
                        }
                    } 

                    // signed
                    if (len(s.i) > 0L)
                    {
                        {
                            var o__prev2 = o;

                            foreach (var (_, __o) in ops)
                            {
                                o = __o;
                                if (s.oponly != "" && s.oponly != o.name)
                                {
                                    continue;
                                } 
                                // don't generate tests for shifts by signed integers
                                if (o.name == "lsh" || o.name == "rsh")
                                {
                                    continue;
                                }

                                fd = new cfncData(s.name,o.name,s.name,o.symbol,"","","","");
                                {
                                    var i__prev3 = i;

                                    foreach (var (_, __i) in s.i)
                                    {
                                        i = __i;
                                        fd.Number = fmt.Sprintf("%d", i);
                                        fd.FNumber = strings.Replace(fd.Number, "-", "Neg", -1L);
                                        {
                                            var j__prev4 = j;

                                            foreach (var (_, __j) in s.i)
                                            {
                                                j = __j;
                                                if (o.name != "mod" && o.name != "div" || j != 0L)
                                                {
                                                    fd.Ans = ansS(i, j, s.name, o.symbol);
                                                    fd.Input = fmt.Sprintf("%d", j);
                                                    {
                                                        var err__prev3 = err;

                                                        err = vrf1.Execute(w, fd);

                                                        if (err != null)
                                                        {
                                                            panic(err);
                                                        }

                                                        err = err__prev3;

                                                    }

                                                }

                                                if (o.name != "mod" && o.name != "div" || i != 0L)
                                                {
                                                    fd.Ans = ansS(j, i, s.name, o.symbol);
                                                    fd.Input = fmt.Sprintf("%d", j);
                                                    {
                                                        var err__prev3 = err;

                                                        err = vrf2.Execute(w, fd);

                                                        if (err != null)
                                                        {
                                                            panic(err);
                                                        }

                                                        err = err__prev3;

                                                    }

                                                }

                                            }

                                            j = j__prev4;
                                        }
                                    }

                                    i = i__prev3;
                                }
                            }

                            o = o__prev2;
                        }
                    }

                    fmt.Fprintf(w, "}\n\n");

                }

                s = s__prev1;
            }

            fmt.Fprint(w, "\n\n// TestArithmeticConst tests results for arithmetic operations against constant" +
    "s.\nfunc TestArithmeticConst(t *testing.T) {\n");

            {
                var s__prev1 = s;

                foreach (var (_, __s) in szs)
                {
                    s = __s;
                    fmt.Fprintf(w, "for _, test := range tests_%s%s {", s.name, s.oponly); 
                    // Use WriteString here to avoid a vet warning about formatting directives.
                    w.WriteString("if got := test.fn(test.in); got != test.want {\n\t\t\tt.Errorf(\"%s(%d) = %d, want %d\\" +
    "n\", test.fnname, test.in, got, test.want)\n\t\t}\n\t}\n");

                }

                s = s__prev1;
            }

            fmt.Fprint(w, "\n}\n"); 

            // gofmt result
            var b = w.Bytes();
            var (src, err) = format.Source(b);
            if (err != null)
            {
                fmt.Printf("%s\n", b);
                panic(err);
            } 

            // write to file
            err = ioutil.WriteFile("../arithConst_test.go", src, 0666L);
            if (err != null)
            {
                log.Fatalf("can't write output: %v\n", err);
            }

        });
    }
}
