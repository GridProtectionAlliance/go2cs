// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssagen -- go2cs converted at 2022 March 06 23:09:45 UTC
// import "cmd/compile/internal/ssagen" ==> using ssagen = go.cmd.compile.@internal.ssagen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssagen\arch.go
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ssagen_package {

public static ArchInfo Arch = default;

// interface to back end

public partial struct ArchInfo {
    public ptr<obj.LinkArch> LinkArch;
    public nint REGSP;
    public long MAXWIDTH;
    public bool SoftFloat;
    public Func<long, long> PadFrame; // ZeroRange zeroes a range of memory on stack. It is only inserted
// at function entry, and it is ok to clobber registers.
    public Func<ptr<objw.Progs>, ptr<obj.Prog>, long, long, ptr<uint>, ptr<obj.Prog>> ZeroRange;
    public Func<ptr<objw.Progs>, ptr<obj.Prog>> Ginsnop;
    public Func<ptr<objw.Progs>, ptr<obj.Prog>> Ginsnopdefer; // special ginsnop for deferreturn

// SSAMarkMoves marks any MOVXconst ops that need to avoid clobbering flags.
    public Action<ptr<State>, ptr<ssa.Block>> SSAMarkMoves; // SSAGenValue emits Prog(s) for the Value.
    public Action<ptr<State>, ptr<ssa.Value>> SSAGenValue; // SSAGenBlock emits end-of-block Progs. SSAGenValue should be called
// for all values in the block before SSAGenBlock.
    public Action<ptr<State>, ptr<ssa.Block>, ptr<ssa.Block>> SSAGenBlock; // LoadRegResults emits instructions that loads register-assigned results
// into registers. They are already in memory (PPARAMOUT nodes).
// Used in open-coded defer return path.
    public Action<ptr<State>, ptr<ssa.Func>> LoadRegResults; // SpillArgReg emits instructions that spill reg to n+off.
    public Func<ptr<objw.Progs>, ptr<obj.Prog>, ptr<ssa.Func>, ptr<types.Type>, short, ptr<ir.Name>, long, ptr<obj.Prog>> SpillArgReg;
}

} // end ssagen_package
