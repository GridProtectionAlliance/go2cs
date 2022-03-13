// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:21:59 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\softfloat.go
namespace go.cmd.compile.@internal;

using types = cmd.compile.@internal.types_package;
using math = math_package;

public static partial class ssa_package {

private static void softfloat(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (!f.Config.SoftFloat) {
        return ;
    }
    var newInt64 = false;

    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            if (v.Type.IsFloat()) {
                f.unCache(v);

                if (v.Op == OpPhi || v.Op == OpLoad || v.Op == OpArg) 
                    if (v.Type.Size() == 4) {
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
                    var arg0 = v.Args[0];
                    v.reset(OpXor32);
                    v.Type = f.Config.Types.UInt32;
                    v.AddArg(arg0);
                    var mask = v.Block.NewValue0(v.Pos, OpConst32, v.Type);
                    mask.AuxInt = -0x80000000;
                    v.AddArg(mask);
                else if (v.Op == OpNeg64F) 
                    arg0 = v.Args[0];
                    v.reset(OpXor64);
                    v.Type = f.Config.Types.UInt64;
                    v.AddArg(arg0);
                    mask = v.Block.NewValue0(v.Pos, OpConst64, v.Type);
                    mask.AuxInt = -0x8000000000000000;
                    v.AddArg(mask);
                else if (v.Op == OpRound32F) 
                    v.Op = OpCopy;
                    v.Type = f.Config.Types.UInt32;
                else if (v.Op == OpRound64F) 
                    v.Op = OpCopy;
                    v.Type = f.Config.Types.UInt64;
                                newInt64 = newInt64 || v.Type.Size() == 8;
            }
            else if ((v.Op == OpStore || v.Op == OpZero || v.Op == OpMove) && v.Aux._<ptr<types.Type>>().IsFloat()) {
                {
                    ptr<types.Type> size = v.Aux._<ptr<types.Type>>().Size();

                    switch (size) {
                        case 4: 
                            v.Aux = f.Config.Types.UInt32;
                            break;
                        case 8: 
                            v.Aux = f.Config.Types.UInt64;
                            break;
                        default: 
                            v.Fatalf("bad float type with size %d", size);
                            break;
                    }
                }
            }
        }
    }    if (newInt64 && f.Config.RegSize == 4) { 
        // On 32bit arch, decompose Uint64 introduced in the switch above.
        decomposeBuiltIn(f);
        applyRewrite(f, rewriteBlockdec64, rewriteValuedec64, removeDeadValues);
    }
}

} // end ssa_package
