// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64 -- go2cs converted at 2022 March 06 23:14:32 UTC
// import "cmd/compile/internal/arm64" ==> using arm64 = go.cmd.compile.@internal.arm64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\arm64\ggen.go
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using buildcfg = go.@internal.buildcfg_package;

namespace go.cmd.compile.@internal;

public static partial class arm64_package {

private static var darwin = buildcfg.GOOS == "darwin" || buildcfg.GOOS == "ios";

private static long padframe(long frame) { 
    // arm64 requires that the frame size (not counting saved FP&LR)
    // be 16 bytes aligned. If not, pad it.
    if (frame % 16 != 0) {
        frame += 16 - (frame % 16);
    }
    return frame;

}

private static ptr<obj.Prog> zerorange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint _ = ref _addr__.val;

    if (cnt == 0) {
        return _addr_p!;
    }
    if (cnt < int64(4 * types.PtrSize)) {
        {
            var i = int64(0);

            while (i < cnt) {
                p = pp.Append(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0, obj.TYPE_MEM, arm64.REGSP, 8 + off + i);
                i += int64(types.PtrSize);
            }

        }

    }
    else if (cnt <= int64(128 * types.PtrSize) && !darwin) { // darwin ld64 cannot handle BR26 reloc with non-zero addend
        if (cnt % (2 * int64(types.PtrSize)) != 0) {
            p = pp.Append(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0, obj.TYPE_MEM, arm64.REGSP, 8 + off);
            off += int64(types.PtrSize);
            cnt -= int64(types.PtrSize);
        }
        p = pp.Append(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGSP, 0, obj.TYPE_REG, arm64.REG_R20, 0);
        p = pp.Append(p, arm64.AADD, obj.TYPE_CONST, 0, 8 + off, obj.TYPE_REG, arm64.REG_R20, 0);
        p.Reg = arm64.REG_R20;
        p = pp.Append(p, obj.ADUFFZERO, obj.TYPE_NONE, 0, 0, obj.TYPE_MEM, 0, 0);
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Duffzero;
        p.To.Offset = 4 * (64 - cnt / (2 * int64(types.PtrSize)));

    }
    else
 { 
        // Not using REGTMP, so this is async preemptible (async preemption clobbers REGTMP).
        // We are at the function entry, where no register is live, so it is okay to clobber
        // other registers
        const var rtmp = arm64.REG_R20;

        p = pp.Append(p, arm64.AMOVD, obj.TYPE_CONST, 0, 8 + off - 8, obj.TYPE_REG, rtmp, 0);
        p = pp.Append(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGSP, 0, obj.TYPE_REG, arm64.REGRT1, 0);
        p = pp.Append(p, arm64.AADD, obj.TYPE_REG, rtmp, 0, obj.TYPE_REG, arm64.REGRT1, 0);
        p.Reg = arm64.REGRT1;
        p = pp.Append(p, arm64.AMOVD, obj.TYPE_CONST, 0, cnt, obj.TYPE_REG, rtmp, 0);
        p = pp.Append(p, arm64.AADD, obj.TYPE_REG, rtmp, 0, obj.TYPE_REG, arm64.REGRT2, 0);
        p.Reg = arm64.REGRT1;
        p = pp.Append(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0, obj.TYPE_MEM, arm64.REGRT1, int64(types.PtrSize));
        p.Scond = arm64.C_XPRE;
        var p1 = p;
        p = pp.Append(p, arm64.ACMP, obj.TYPE_REG, arm64.REGRT1, 0, obj.TYPE_NONE, 0, 0);
        p.Reg = arm64.REGRT2;
        p = pp.Append(p, arm64.ABNE, obj.TYPE_NONE, 0, 0, obj.TYPE_BRANCH, 0, 0);
        p.To.SetTarget(p1);

    }
    return _addr_p!;

}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;

    var p = pp.Prog(arm64.AHINT);
    p.From.Type = obj.TYPE_CONST;
    return _addr_p!;
}

} // end arm64_package
