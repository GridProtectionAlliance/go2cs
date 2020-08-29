// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:14 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\softfloat.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static void softfloat(ref Func f)
        {
            if (!f.Config.SoftFloat)
            {
                return;
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
                            v.AuxInt = int64(int32(math.Float32bits(i2f32(v.AuxInt))));
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
