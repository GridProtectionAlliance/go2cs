// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2020 August 29 09:25:00 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Go\src\cmd\compile\internal\ppc64\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ppc64_package
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
                        p = pp.Appendpp(p, ppc64.AMOVD, obj.TYPE_REG, ppc64.REGZERO, 0L, obj.TYPE_MEM, ppc64.REGSP, gc.Ctxt.FixedFrameSize() + off + i);
                        i += int64(gc.Widthptr);
                    }
                }
            }
            else if (cnt <= int64(128L * gc.Widthptr))
            {
                p = pp.Appendpp(p, ppc64.AADD, obj.TYPE_CONST, 0L, gc.Ctxt.FixedFrameSize() + off - 8L, obj.TYPE_REG, ppc64.REGRT1, 0L);
                p.Reg = ppc64.REGSP;
                p = pp.Appendpp(p, obj.ADUFFZERO, obj.TYPE_NONE, 0L, 0L, obj.TYPE_MEM, 0L, 0L);
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = 4L * (128L - cnt / int64(gc.Widthptr));
            }
            else
            {
                p = pp.Appendpp(p, ppc64.AMOVD, obj.TYPE_CONST, 0L, gc.Ctxt.FixedFrameSize() + off - 8L, obj.TYPE_REG, ppc64.REGTMP, 0L);
                p = pp.Appendpp(p, ppc64.AADD, obj.TYPE_REG, ppc64.REGTMP, 0L, obj.TYPE_REG, ppc64.REGRT1, 0L);
                p.Reg = ppc64.REGSP;
                p = pp.Appendpp(p, ppc64.AMOVD, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, ppc64.REGTMP, 0L);
                p = pp.Appendpp(p, ppc64.AADD, obj.TYPE_REG, ppc64.REGTMP, 0L, obj.TYPE_REG, ppc64.REGRT2, 0L);
                p.Reg = ppc64.REGRT1;
                p = pp.Appendpp(p, ppc64.AMOVDU, obj.TYPE_REG, ppc64.REGZERO, 0L, obj.TYPE_MEM, ppc64.REGRT1, int64(gc.Widthptr));
                var p1 = p;
                p = pp.Appendpp(p, ppc64.ACMP, obj.TYPE_REG, ppc64.REGRT1, 0L, obj.TYPE_REG, ppc64.REGRT2, 0L);
                p = pp.Appendpp(p, ppc64.ABNE, obj.TYPE_NONE, 0L, 0L, obj.TYPE_BRANCH, 0L, 0L);
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
                    var p = pp.Prog(ppc64.AMOVD);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = ppc64.REGZERO;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_AUTO;
                    p.To.Reg = ppc64.REGSP;
                    p.To.Offset = n.Xoffset + i;
                    p.To.Sym = sym;
                    i += 8L;
                }

            }
        }

        private static void ginsnop(ref gc.Progs pp)
        {
            var p = pp.Prog(ppc64.AOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_R0;
        }

        private static void ginsnop2(ref gc.Progs pp)
        { 
            // PPC64 is unusual because TWO nops are required
            // (see gc/cgen.go, gc/plive.go -- copy of comment below)
            //
            // On ppc64, when compiling Go into position
            // independent code on ppc64le we insert an
            // instruction to reload the TOC pointer from the
            // stack as well. See the long comment near
            // jmpdefer in runtime/asm_ppc64.s for why.
            // If the MOVD is not needed, insert a hardware NOP
            // so that the same number of instructions are used
            // on ppc64 in both shared and non-shared modes.

            ginsnop(pp);
            if (gc.Ctxt.Flag_shared)
            {
                var p = pp.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_MEM;
                p.From.Offset = 24L;
                p.From.Reg = ppc64.REGSP;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REG_R2;
            }
            else
            {
                ginsnop(pp);
            }
        }
    }
}}}}
