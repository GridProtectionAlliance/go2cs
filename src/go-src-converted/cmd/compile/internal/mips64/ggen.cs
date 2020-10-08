// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mips64 -- go2cs converted at 2020 October 08 04:27:42 UTC
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
