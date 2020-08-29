// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips64 -- go2cs converted at 2020 August 29 09:25:10 UTC
// import "cmd/compile/internal/mips64" ==> using mips64 = go.cmd.compile.@internal.mips64_package
// Original source: C:\Go\src\cmd\compile\internal\mips64\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using mips = go.cmd.@internal.obj.mips_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class mips64_package
    {
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
                        p = pp.Appendpp(p, mips.AMOVV, obj.TYPE_REG, mips.REGZERO, 0L, obj.TYPE_MEM, mips.REGSP, 8L + off + i);
                        i += int64(gc.Widthptr);
                    }
                }
            }
            else if (cnt <= int64(128L * gc.Widthptr))
            {
                p = pp.Appendpp(p, mips.AADDV, obj.TYPE_CONST, 0L, 8L + off - 8L, obj.TYPE_REG, mips.REGRT1, 0L);
                p.Reg = mips.REGSP;
                p = pp.Appendpp(p, obj.ADUFFZERO, obj.TYPE_NONE, 0L, 0L, obj.TYPE_MEM, 0L, 0L);
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = 8L * (128L - cnt / int64(gc.Widthptr));
            }
            else
            { 
                //    ADDV    $(8+frame+lo-8), SP, r1
                //    ADDV    $cnt, r1, r2
                // loop:
                //    MOVV    R0, (Widthptr)r1
                //    ADDV    $Widthptr, r1
                //    BNE        r1, r2, loop
                p = pp.Appendpp(p, mips.AADDV, obj.TYPE_CONST, 0L, 8L + off - 8L, obj.TYPE_REG, mips.REGRT1, 0L);
                p.Reg = mips.REGSP;
                p = pp.Appendpp(p, mips.AADDV, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, mips.REGRT2, 0L);
                p.Reg = mips.REGRT1;
                p = pp.Appendpp(p, mips.AMOVV, obj.TYPE_REG, mips.REGZERO, 0L, obj.TYPE_MEM, mips.REGRT1, int64(gc.Widthptr));
                var p1 = p;
                p = pp.Appendpp(p, mips.AADDV, obj.TYPE_CONST, 0L, int64(gc.Widthptr), obj.TYPE_REG, mips.REGRT1, 0L);
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
                    var p = pp.Prog(mips.AMOVV);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = mips.REGZERO;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_AUTO;
                    p.To.Reg = mips.REGSP;
                    p.To.Offset = n.Xoffset + i;
                    p.To.Sym = sym;
                    i += 8L;
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
