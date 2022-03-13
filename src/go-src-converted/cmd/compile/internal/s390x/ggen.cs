// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package s390x -- go2cs converted at 2022 March 13 06:24:02 UTC
// import "cmd/compile/internal/s390x" ==> using s390x = go.cmd.compile.@internal.s390x_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\s390x\ggen.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using objw = cmd.compile.@internal.objw_package;
using obj = cmd.@internal.obj_package;
using s390x = cmd.@internal.obj.s390x_package;


// clearLoopCutOff is the (somewhat arbitrary) value above which it is better
// to have a loop of clear instructions (e.g. XCs) rather than just generating
// multiple instructions (i.e. loop unrolling).
// Must be between 256 and 4096.

public static partial class s390x_package {

private static readonly nint clearLoopCutoff = 1024;

// zerorange clears the stack in the given range.


// zerorange clears the stack in the given range.
private static ptr<obj.Prog> zerorange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint _ = ref _addr__.val;

    if (cnt == 0) {
        return _addr_p!;
    }
    off += @base.Ctxt.FixedFrameSize();
    var reg = int16(s390x.REGSP); 

    // If the off cannot fit in a 12-bit unsigned displacement then we
    // need to create a copy of the stack pointer that we can adjust.
    // We also need to do this if we are going to loop.
    if (off < 0 || off > 4096 - clearLoopCutoff || cnt > clearLoopCutoff) {
        p = pp.Append(p, s390x.AADD, obj.TYPE_CONST, 0, off, obj.TYPE_REG, s390x.REGRT1, 0);
        p.Reg = int16(s390x.REGSP);
        reg = s390x.REGRT1;
        off = 0;
    }
    if (cnt > clearLoopCutoff) {
        var ireg = int16(s390x.REGRT2); // register holds number of remaining loop iterations
        p = pp.Append(p, s390x.AMOVD, obj.TYPE_CONST, 0, cnt / 256, obj.TYPE_REG, ireg, 0);
        p = pp.Append(p, s390x.ACLEAR, obj.TYPE_CONST, 0, 256, obj.TYPE_MEM, reg, off);
        var pl = p;
        p = pp.Append(p, s390x.AADD, obj.TYPE_CONST, 0, 256, obj.TYPE_REG, reg, 0);
        p = pp.Append(p, s390x.ABRCTG, obj.TYPE_REG, ireg, 0, obj.TYPE_BRANCH, 0, 0);
        p.To.SetTarget(pl);
        cnt = cnt % 256;
    }
    while (cnt > 0) {
        var n = cnt; 

        // Can clear at most 256 bytes per instruction.
        if (n > 256) {
            n = 256;
        }
        switch (n) { 
        // Handle very small clears with move instructions.
            case 8: 

            case 4: 

            case 2: 

            case 1: 
                var ins = s390x.AMOVB;
                switch (n) {
                    case 8: 
                        ins = s390x.AMOVD;
                        break;
                    case 4: 
                        ins = s390x.AMOVW;
                        break;
                    case 2: 
                        ins = s390x.AMOVH;
                        break;
                }
                p = pp.Append(p, ins, obj.TYPE_CONST, 0, 0, obj.TYPE_MEM, reg, off); 

                // Handle clears that would require multiple move instructions with CLEAR (assembled as XC).
                break;
            default: 
                p = pp.Append(p, s390x.ACLEAR, obj.TYPE_CONST, 0, n, obj.TYPE_MEM, reg, off);
                break;
        }

        cnt -= n;
        off += n;
    }

    return _addr_p!;
}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;

    return _addr_pp.Prog(s390x.ANOPH)!;
}

} // end s390x_package
