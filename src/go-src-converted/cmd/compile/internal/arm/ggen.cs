// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm -- go2cs converted at 2020 October 09 05:44:06 UTC
// import "cmd/compile/internal/arm" ==> using arm = go.cmd.compile.@internal.arm_package
// Original source: C:\Go\src\cmd\compile\internal\arm\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using arm = go.cmd.@internal.obj.arm_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class arm_package
    {
        private static ptr<obj.Prog> zerorange(ptr<gc.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr_r0)
        {
            ref gc.Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;
            ref uint r0 = ref _addr_r0.val;

            if (cnt == 0L)
            {
                return _addr_p!;
            }
            if (r0 == 0L.val)
            {
                p = pp.Appendpp(p, arm.AMOVW, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, arm.REG_R0, 0L);
                r0 = 1L;
            }
            if (cnt < int64(4L * gc.Widthptr))
            {
                {
                    var i = int64(0L);

                    while (i < cnt)
                    {
                        p = pp.Appendpp(p, arm.AMOVW, obj.TYPE_REG, arm.REG_R0, 0L, obj.TYPE_MEM, arm.REGSP, 4L + off + i);
                        i += int64(gc.Widthptr);
                    }
                }

            }
            else if (cnt <= int64(128L * gc.Widthptr))
            {
                p = pp.Appendpp(p, arm.AADD, obj.TYPE_CONST, 0L, 4L + off, obj.TYPE_REG, arm.REG_R1, 0L);
                p.Reg = arm.REGSP;
                p = pp.Appendpp(p, obj.ADUFFZERO, obj.TYPE_NONE, 0L, 0L, obj.TYPE_MEM, 0L, 0L);
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = 4L * (128L - cnt / int64(gc.Widthptr));
            }
            else
            {
                p = pp.Appendpp(p, arm.AADD, obj.TYPE_CONST, 0L, 4L + off, obj.TYPE_REG, arm.REG_R1, 0L);
                p.Reg = arm.REGSP;
                p = pp.Appendpp(p, arm.AADD, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, arm.REG_R2, 0L);
                p.Reg = arm.REG_R1;
                p = pp.Appendpp(p, arm.AMOVW, obj.TYPE_REG, arm.REG_R0, 0L, obj.TYPE_MEM, arm.REG_R1, 4L);
                var p1 = p;
                p.Scond |= arm.C_PBIT;
                p = pp.Appendpp(p, arm.ACMP, obj.TYPE_REG, arm.REG_R1, 0L, obj.TYPE_NONE, 0L, 0L);
                p.Reg = arm.REG_R2;
                p = pp.Appendpp(p, arm.ABNE, obj.TYPE_NONE, 0L, 0L, obj.TYPE_BRANCH, 0L, 0L);
                gc.Patch(p, p1);
            }
            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;

            var p = pp.Prog(arm.AAND);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = arm.REG_R0;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = arm.REG_R0;
            p.Scond = arm.C_SCOND_EQ;
            return _addr_p!;
        }
    }
}}}}
