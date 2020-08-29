// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:43 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\print.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static void printFunc(ref Func f)
        {
            f.Logf("%s", f);
        }

        private static @string String(this ref Func f)
        {
            bytes.Buffer buf = default;
            stringFuncPrinter p = new stringFuncPrinter(w:&buf);
            fprintFunc(p, f);
            return buf.String();
        }

        private partial interface funcPrinter
        {
            void header(ref Func f);
            void startBlock(ref Block b, bool reachable);
            void endBlock(ref Block b);
            void value(ref Value v, bool live);
            void startDepCycle();
            void endDepCycle();
            void named(LocalSlot n, slice<ref Value> vals);
        }

        private partial struct stringFuncPrinter
        {
            public io.Writer w;
        }

        private static void header(this stringFuncPrinter p, ref Func f)
        {
            fmt.Fprint(p.w, f.Name);
            fmt.Fprint(p.w, " ");
            fmt.Fprintln(p.w, f.Type);
        }

        private static void startBlock(this stringFuncPrinter p, ref Block b, bool reachable)
        {
            fmt.Fprintf(p.w, "  b%d:", b.ID);
            if (len(b.Preds) > 0L)
            {
                io.WriteString(p.w, " <-");
                foreach (var (_, e) in b.Preds)
                {
                    var pred = e.b;
                    fmt.Fprintf(p.w, " b%d", pred.ID);
                }
            }
            if (!reachable)
            {
                fmt.Fprint(p.w, " DEAD");
            }
            io.WriteString(p.w, "\n");
        }

        private static void endBlock(this stringFuncPrinter p, ref Block b)
        {
            fmt.Fprintln(p.w, "    " + b.LongString());
        }

        private static void value(this stringFuncPrinter p, ref Value v, bool live)
        {
            fmt.Fprint(p.w, "    "); 
            //fmt.Fprint(p.w, v.Block.Func.fe.Pos(v.Pos))
            //fmt.Fprint(p.w, ": ")
            fmt.Fprint(p.w, v.LongString());
            if (!live)
            {
                fmt.Fprint(p.w, " DEAD");
            }
            fmt.Fprintln(p.w);
        }

        private static void startDepCycle(this stringFuncPrinter p)
        {
            fmt.Fprintln(p.w, "dependency cycle!");
        }

        private static void endDepCycle(this stringFuncPrinter p)
        {
        }

        private static void named(this stringFuncPrinter p, LocalSlot n, slice<ref Value> vals)
        {
            fmt.Fprintf(p.w, "name %s: %v\n", n, vals);
        }

        private static void fprintFunc(funcPrinter p, ref Func f)
        {
            var (reachable, live) = findlive(f);
            p.header(f);
            var printed = make_slice<bool>(f.NumValues());
            foreach (var (_, b) in f.Blocks)
            {
                p.startBlock(b, reachable[b.ID]);

                if (f.scheduled)
                { 
                    // Order of Values has been decided - print in that order.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            p.value(v, live[v.ID]);
                            printed[v.ID] = true;
                        }

                        v = v__prev2;
                    }

                    p.endBlock(b);
                    continue;
                } 

                // print phis first since all value cycles contain a phi
                long n = 0L;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Op != OpPhi)
                        {
                            continue;
                        }
                        p.value(v, live[v.ID]);
                        printed[v.ID] = true;
                        n++;
                    } 

                    // print rest of values in dependency order

                    v = v__prev2;
                }

                while (n < len(b.Values))
                {
                    var m = n;
outer:
                    {
                        var v__prev3 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (printed[v.ID])
                            {
                                continue;
                            }
                            foreach (var (_, w) in v.Args)
                            { 
                                // w == nil shouldn't happen, but if it does,
                                // don't panic; we'll get a better diagnosis later.
                                if (w != null && w.Block == b && !printed[w.ID])
                                {
                                    _continueouter = true;
                                    break;
                                }
                            }
                            p.value(v, live[v.ID]);
                            printed[v.ID] = true;
                            n++;
                        }

                        v = v__prev3;
                    }
                    if (m == n)
                    {
                        p.startDepCycle();
                        {
                            var v__prev3 = v;

                            foreach (var (_, __v) in b.Values)
                            {
                                v = __v;
                                if (printed[v.ID])
                                {
                                    continue;
                                }
                                p.value(v, live[v.ID]);
                                printed[v.ID] = true;
                                n++;
                            }

                            v = v__prev3;
                        }

                        p.endDepCycle();
                    }
                }


                p.endBlock(b);
            }
            foreach (var (_, name) in f.Names)
            {
                p.named(name, f.NamedValues[name]);
            }
        }
    }
}}}}
