// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:13 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\lower.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // convert to machine-dependent ops
private static void lower(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // repeat rewrites until we find no more rewrites
    applyRewrite(f, f.Config.lowerBlock, f.Config.lowerValue, removeDeadValues);

}

// checkLower checks for unlowered opcodes and fails if we find one.
private static void checkLower(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Needs to be a separate phase because it must run after both
    // lowering and a subsequent dead code elimination (because lowering
    // rules may leave dead generic ops behind).
    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            if (!opcodeTable[v.Op].generic) {
                continue; // lowered
            }


            if (v.Op == OpSP || v.Op == OpSB || v.Op == OpInitMem || v.Op == OpArg || v.Op == OpArgIntReg || v.Op == OpArgFloatReg || v.Op == OpPhi || v.Op == OpVarDef || v.Op == OpVarKill || v.Op == OpVarLive || v.Op == OpKeepAlive || v.Op == OpSelect0 || v.Op == OpSelect1 || v.Op == OpSelectN || v.Op == OpConvert || v.Op == OpInlMark) 
                continue; // ok not to lower
            else if (v.Op == OpMakeResult) 
                if (b.Controls[0] == v) {
                    continue;
                }
            else if (v.Op == OpGetG) 
                if (f.Config.hasGReg) { 
                    // has hardware g register, regalloc takes care of it
                    continue; // ok not to lower
                }

                        @string s = "not lowered: " + v.String() + ", " + v.Op.String() + " " + v.Type.SimpleString();

            foreach (var (_, a) in v.Args) {
                s += " " + a.Type.SimpleString();
            }
            f.Fatalf("%s", s);

        }
    }
}

} // end ssa_package
