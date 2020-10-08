// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:36 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\softfloat.go
using types = go.cmd.compile.@internal.types_package;
using math = go.math_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static void softfloat(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (!f.Config.SoftFloat)
            {
                return ;
            }
            var newInt64 = false;

            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    if (v.Type.IsFloat())
                    {

                        if (v.Op == OpPhi || v.Op == OpLoad || v.Op == OpArg) 
                            if (v.Type.Size() == 4L)
                            {
                                v.Type = f.Config.Types.UInt32;
                            }
                            else
                            {
                                v.Type = f.Config.Types.UInt64;
                            }
                        else if (v.Op == OpConst32F) 
                            v.Op = OpConst32;
                            v.Type = f.Config.Types.UInt32;
                            v.AuxInt = int64(int32(math.Float32bits(auxTo32F(v.AuxInt))));
                        else if (v.Op == OpConst64F) 
                            v.Op = OpConst64;
                            v.Type = f.Config.Types.UInt64;
                        else if (v.Op == OpNeg32F) 
                            var arg0 = v.Args[0L];
                            v.reset(OpXor32);
                            v.Type = f.Config.Types.UInt32;
                            v.AddArg(arg0);
                            var mask = v.Block.NewValue0(v.Pos, OpConst32, v.Type);
                            mask.AuxInt = -0x80000000UL;
                            v.AddArg(mask);
                        else if (v.Op == OpNeg64F) 
                            arg0 = v.Args[0L];
                            v.reset(OpXor64);
                            v.Type = f.Config.Types.UInt64;
                            v.AddArg(arg0);
                            mask = v.Block.NewValue0(v.Pos, OpConst64, v.Type);
                            mask.AuxInt = -0x8000000000000000UL;
                            v.AddArg(mask);
                        else if (v.Op == OpRound32F) 
                            v.Op = OpCopy;
                            v.Type = f.Config.Types.UInt32;
                        else if (v.Op == OpRound64F) 
                            v.Op = OpCopy;
                            v.Type = f.Config.Types.UInt64;
                                                newInt64 = newInt64 || v.Type.Size() == 8L;

                    }
                    else if ((v.Op == OpStore || v.Op == OpZero || v.Op == OpMove) && v.Aux._<ptr<types.Type>>().IsFloat())
                    {
                        {
                            ptr<types.Type> size = v.Aux._<ptr<types.Type>>().Size();

                            switch (size)
                            {
                                case 4L: 
                                    v.Aux = f.Config.Types.UInt32;
                                    break;
                                case 8L: 
                                    v.Aux = f.Config.Types.UInt64;
                                    break;
                                default: 
                                    v.Fatalf("bad float type with size %d", size);
                                    break;
                            }
                        }

                    }
                }
            }            if (newInt64 && f.Config.RegSize == 4L)
            { 
                // On 32bit arch, decompose Uint64 introduced in the switch above.
                decomposeBuiltIn(f);
                applyRewrite(f, rewriteBlockdec64, rewriteValuedec64);

            }
        }
    }
}}}}
