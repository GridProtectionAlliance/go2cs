// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2020 October 09 05:40:14 UTC
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
            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;

            var p = pp.Prog(ppc64.AOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_R0;
            return _addr_p!;
        }

        private static ptr<obj.Prog> ginsnopdefer(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;
 
            // On PPC64 two nops are required in the defer case.
            //
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

            ginsnop(_addr_pp);
            if (gc.Ctxt.Flag_shared)
            {
                var p = pp.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_MEM;
                p.From.Offset = 24L;
                p.From.Reg = ppc64.REGSP;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REG_R2;
                return _addr_p!;
            }

            return _addr_ginsnop(_addr_pp)!;

        }
    }
}}}}
