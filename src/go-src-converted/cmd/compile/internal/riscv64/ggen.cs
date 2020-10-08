// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2020 October 08 04:27:24 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Go\src\cmd\compile\internal\riscv64\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using riscv = go.cmd.@internal.obj.riscv_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class riscv64_package
    {
        private static ptr<obj.Prog> zeroRange(ptr<gc.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__)
        {
            ref gc.Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;
            ref uint _ = ref _addr__.val;

            if (cnt == 0L)
            {
                return _addr_p!;
            }
            off += gc.Ctxt.FixedFrameSize();

            if (cnt < int64(4L * gc.Widthptr))
            {
                {
                    var i = int64(0L);

                    while (i < cnt)
                    {
                        p = pp.Appendpp(p, riscv.AMOV, obj.TYPE_REG, riscv.REG_ZERO, 0L, obj.TYPE_MEM, riscv.REG_SP, off + i);
                        i += int64(gc.Widthptr);
                    }
                }
                return _addr_p!;

            }
            p = pp.Appendpp(p, riscv.AADD, obj.TYPE_CONST, 0L, off, obj.TYPE_REG, riscv.REG_T0, 0L);
            p.Reg = riscv.REG_SP;
            p = pp.Appendpp(p, riscv.AADD, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, riscv.REG_T1, 0L);
            p.Reg = riscv.REG_T0;
            p = pp.Appendpp(p, riscv.AMOV, obj.TYPE_REG, riscv.REG_ZERO, 0L, obj.TYPE_MEM, riscv.REG_T0, 0L);
            var loop = p;
            p = pp.Appendpp(p, riscv.AADD, obj.TYPE_CONST, 0L, int64(gc.Widthptr), obj.TYPE_REG, riscv.REG_T0, 0L);
            p = pp.Appendpp(p, riscv.ABNE, obj.TYPE_REG, riscv.REG_T0, 0L, obj.TYPE_BRANCH, 0L, 0L);
            p.Reg = riscv.REG_T1;
            gc.Patch(p, loop);
            return _addr_p!;

        }
    }
}}}}
