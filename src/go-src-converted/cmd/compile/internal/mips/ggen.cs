// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips -- go2cs converted at 2020 October 09 05:40:30 UTC
// import "cmd/compile/internal/mips" ==> using mips = go.cmd.compile.@internal.mips_package
// Original source: C:\Go\src\cmd\compile\internal\mips\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using mips = go.cmd.@internal.obj.mips_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class mips_package
    {
        // TODO(mips): implement DUFFZERO
        private static ptr<obj.Prog> zerorange(ptr<gc.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__)
        {
            ref gc.Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;
            ref uint _ = ref _addr__.val;

            if (cnt == 0L)
            {
                return _addr_p!;
            }
            if (cnt < int64(4L * gc.Widthptr))
            {
                {
                    var i = int64(0L);

                    while (i < cnt)
                    {
                        p = pp.Appendpp(p, mips.AMOVW, obj.TYPE_REG, mips.REGZERO, 0L, obj.TYPE_MEM, mips.REGSP, gc.Ctxt.FixedFrameSize() + off + i);
                        i += int64(gc.Widthptr);
                    }
            else

                }

            }            { 
                //fmt.Printf("zerorange frame:%v, lo: %v, hi:%v \n", frame ,lo, hi)
                //    ADD     $(FIXED_FRAME+frame+lo-4), SP, r1
                //    ADD     $cnt, r1, r2
                // loop:
                //    MOVW    R0, (Widthptr)r1
                //    ADD     $Widthptr, r1
                //    BNE        r1, r2, loop
                p = pp.Appendpp(p, mips.AADD, obj.TYPE_CONST, 0L, gc.Ctxt.FixedFrameSize() + off - 4L, obj.TYPE_REG, mips.REGRT1, 0L);
                p.Reg = mips.REGSP;
                p = pp.Appendpp(p, mips.AADD, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, mips.REGRT2, 0L);
                p.Reg = mips.REGRT1;
                p = pp.Appendpp(p, mips.AMOVW, obj.TYPE_REG, mips.REGZERO, 0L, obj.TYPE_MEM, mips.REGRT1, int64(gc.Widthptr));
                var p1 = p;
                p = pp.Appendpp(p, mips.AADD, obj.TYPE_CONST, 0L, int64(gc.Widthptr), obj.TYPE_REG, mips.REGRT1, 0L);
                p = pp.Appendpp(p, mips.ABNE, obj.TYPE_REG, mips.REGRT1, 0L, obj.TYPE_BRANCH, 0L, 0L);
                p.Reg = mips.REGRT2;
                gc.Patch(p, p1);

            }
            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;

            var p = pp.Prog(mips.ANOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = mips.REG_R0;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = mips.REG_R0;
            return _addr_p!;
        }
    }
}}}}
