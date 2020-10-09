// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:25:30 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\print.go
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
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
        private static void printFunc(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            f.Logf("%s", f);
        }

        private static slice<byte> hashFunc(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var h = sha256.New();
            stringFuncPrinter p = new stringFuncPrinter(w:h);
            fprintFunc(p, _addr_f);
            return h.Sum(null);
        }

        private static @string String(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            stringFuncPrinter p = new stringFuncPrinter(w:&buf);
            fprintFunc(p, _addr_f);
            return buf.String();
        }

        private partial interface funcPrinter
        {
            void header(ptr<Func> f);
            void startBlock(ptr<Block> b, bool reachable);
            void endBlock(ptr<Block> b);
            void value(ptr<Value> v, bool live);
            void startDepCycle();
            void endDepCycle();
            void named(LocalSlot n, slice<ptr<Value>> vals);
        }

        private partial struct stringFuncPrinter
        {
            public io.Writer w;
        }

        private static void header(this stringFuncPrinter p, ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            fmt.Fprint(p.w, f.Name);
            fmt.Fprint(p.w, " ");
            fmt.Fprintln(p.w, f.Type);
        }

        private static void startBlock(this stringFuncPrinter p, ptr<Block> _addr_b, bool reachable)
        {
            ref Block b = ref _addr_b.val;

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

        private static void endBlock(this stringFuncPrinter p, ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            fmt.Fprintln(p.w, "    " + b.LongString());
        }

        private static void value(this stringFuncPrinter p, ptr<Value> _addr_v, bool live)
        {
            ref Value v = ref _addr_v.val;

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

        private static void named(this stringFuncPrinter p, LocalSlot n, slice<ptr<Value>> vals)
        {
            fmt.Fprintf(p.w, "name %s: %v\n", n, vals);
        }

        private static void fprintFunc(funcPrinter p, ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;

            var (reachable, live) = findlive(f);
            defer(f.retDeadcodeLive(live));
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

        });
    }
}}}}
