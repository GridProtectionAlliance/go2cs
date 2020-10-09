// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:25:25 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\phiopt.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // phiopt eliminates boolean Phis based on the previous if.
        //
        // Main use case is to transform:
        //   x := false
        //   if b {
        //     x = true
        //   }
        // into x = b.
        //
        // In SSA code this appears as
        //
        // b0
        //   If b -> b1 b2
        // b1
        //   Plain -> b2
        // b2
        //   x = (OpPhi (ConstBool [true]) (ConstBool [false]))
        //
        // In this case we can replace x with a copy of b.
        private static void phiopt(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var sdom = f.Sdom();
            foreach (var (_, b) in f.Blocks)
            {
                if (len(b.Preds) != 2L || len(b.Values) == 0L)
                { 
                    // TODO: handle more than 2 predecessors, e.g. a || b || c.
                    continue;

                }
                var pb0 = b;
                var b0 = b.Preds[0L].b;
                while (len(b0.Succs) == 1L && len(b0.Preds) == 1L)
                {
                    pb0 = b0;
                    b0 = b0.Preds[0L].b;

                }
                if (b0.Kind != BlockIf)
                {
                    continue;
                }
                var pb1 = b;
                var b1 = b.Preds[1L].b;
                while (len(b1.Succs) == 1L && len(b1.Preds) == 1L)
                {
                    pb1 = b1;
                    b1 = b1.Preds[0L].b;

                }
                if (b1 != b0)
                {
                    continue;
                }
                long reverse = default;
                if (b0.Succs[0L].b == pb0 && b0.Succs[1L].b == pb1)
                {
                    reverse = 0L;
                }
                else if (b0.Succs[0L].b == pb1 && b0.Succs[1L].b == pb0)
                {
                    reverse = 1L;
                }
                else
                {
                    b.Fatalf("invalid predecessors\n");
                }
                foreach (var (_, v) in b.Values)
                {
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }
                    if (v.Type.IsInteger())
                    {
                        phioptint(_addr_v, _addr_b0, reverse);
                    }
                    if (!v.Type.IsBoolean())
                    {
                        continue;
                    }
                    if (v.Args[0L].Op == OpConstBool && v.Args[1L].Op == OpConstBool)
                    {
                        if (v.Args[reverse].AuxInt != v.Args[1L - reverse].AuxInt)
                        {
                            array<Op> ops = new array<Op>(new Op[] { OpNot, OpCopy });
                            v.reset(ops[v.Args[reverse].AuxInt]);
                            v.AddArg(b0.Controls[0L]);
                            if (f.pass.debug > 0L)
                            {
                                f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
                            }
                            continue;

                        }
                    }
                    if (v.Args[reverse].Op == OpConstBool && v.Args[reverse].AuxInt == 1L)
                    {
                        {
                            var tmp__prev2 = tmp;

                            var tmp = v.Args[1L - reverse];

                            if (sdom.IsAncestorEq(tmp.Block, b))
                            {
                                v.reset(OpOrB);
                                v.SetArgs2(b0.Controls[0L], tmp);
                                if (f.pass.debug > 0L)
                                {
                                    f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
                                }
                                continue;

                            }
                            tmp = tmp__prev2;

                        }

                    }
                    if (v.Args[1L - reverse].Op == OpConstBool && v.Args[1L - reverse].AuxInt == 0L)
                    {
                        {
                            var tmp__prev2 = tmp;

                            tmp = v.Args[reverse];

                            if (sdom.IsAncestorEq(tmp.Block, b))
                            {
                                v.reset(OpAndB);
                                v.SetArgs2(b0.Controls[0L], tmp);
                                if (f.pass.debug > 0L)
                                {
                                    f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
                                }
                                continue;

                            }
                            tmp = tmp__prev2;

                        }

                    }
                }
            }
        }

        private static void phioptint(ptr<Value> _addr_v, ptr<Block> _addr_b0, long reverse)
        {
            ref Value v = ref _addr_v.val;
            ref Block b0 = ref _addr_b0.val;

            var a0 = v.Args[0L];
            var a1 = v.Args[1L];
            if (a0.Op != a1.Op)
            {
                return ;
            }


            if (a0.Op == OpConst8 || a0.Op == OpConst16 || a0.Op == OpConst32 || a0.Op == OpConst64)             else 
                return ;
                        var negate = false;

            if (a0.AuxInt == 0L && a1.AuxInt == 1L) 
                negate = true;
            else if (a0.AuxInt == 1L && a1.AuxInt == 0L)             else 
                return ;
                        if (reverse == 1L)
            {
                negate = !negate;
            }

            var a = b0.Controls[0L];
            if (negate)
            {
                a = v.Block.NewValue1(v.Pos, OpNot, a.Type, a);
            }

            v.AddArg(a);

            var cvt = v.Block.NewValue1(v.Pos, OpCvtBoolToUint8, v.Block.Func.Config.Types.UInt8, a);
            switch (v.Type.Size())
            {
                case 1L: 
                    v.reset(OpCopy);
                    break;
                case 2L: 
                    v.reset(OpZeroExt8to16);
                    break;
                case 4L: 
                    v.reset(OpZeroExt8to32);
                    break;
                case 8L: 
                    v.reset(OpZeroExt8to64);
                    break;
                default: 
                    v.Fatalf("bad int size %d", v.Type.Size());
                    break;
            }
            v.AddArg(cvt);

            var f = b0.Func;
            if (f.pass.debug > 0L)
            {
                f.Warnl(v.Block.Pos, "converted OpPhi bool -> int%d", v.Type.Size() * 8L);
            }

        }
    }
}}}}
