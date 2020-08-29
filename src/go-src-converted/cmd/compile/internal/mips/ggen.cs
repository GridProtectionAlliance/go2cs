// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips -- go2cs converted at 2020 August 29 09:25:20 UTC
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
        private static ref obj.Prog zerorange(ref gc.Progs pp, ref obj.Prog p, long off, long cnt, ref uint _)
        {
            if (cnt == 0L)
            {
                return p;
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
            return p;
        }

        private static void zeroAuto(ref gc.Progs pp, ref gc.Node n)
        { 
            // Note: this code must not clobber any registers.
            var sym = n.Sym.Linksym();
            var size = n.Type.Size();
            {
                var i = int64(0L);

                while (i < size)
                {
                    var p = pp.Prog(mips.AMOVW);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGZERO;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_AUTO;
                    p.To.Reg = mips.REGSP;
                    p.To.Offset = n.Xoffset + i;
                    p.To.Sym = sym;
                    i += 4L;
                }

            }
        }

        private static void ginsnop(ref gc.Progs pp)
        {
            var p = pp.Prog(mips.ANOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = mips.REG_R0;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = mips.REG_R0;
        }
    }
}}}}
