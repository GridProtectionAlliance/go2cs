// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64 -- go2cs converted at 2020 August 29 09:58:53 UTC
// import "cmd/compile/internal/arm64" ==> using arm64 = go.cmd.compile.@internal.arm64_package
// Original source: C:\Go\src\cmd\compile\internal\arm64\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using arm64 = go.cmd.@internal.obj.arm64_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class arm64_package
    {
        private static var darwin = objabi.GOOS == "darwin";

        private static long padframe(long frame)
        { 
            // arm64 requires that the frame size (not counting saved LR)
            // be empty or be 8 mod 16. If not, pad it.
            if (frame != 0L && frame % 16L != 8L)
            {
                frame += 8L;
            }
            return frame;
        }

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
                        p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0L, obj.TYPE_MEM, arm64.REGSP, 8L + off + i);
                        i += int64(gc.Widthptr);
                    }

                }
            }
            else if (cnt <= int64(128L * gc.Widthptr) && !darwin)
            { // darwin ld64 cannot handle BR26 reloc with non-zero addend
                if (cnt % (2L * int64(gc.Widthptr)) != 0L)
                {
                    p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0L, obj.TYPE_MEM, arm64.REGSP, 8L + off);
                    off += int64(gc.Widthptr);
                    cnt -= int64(gc.Widthptr);
                }
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGSP, 0L, obj.TYPE_REG, arm64.REGRT1, 0L);
                p = pp.Appendpp(p, arm64.AADD, obj.TYPE_CONST, 0L, 8L + off, obj.TYPE_REG, arm64.REGRT1, 0L);
                p.Reg = arm64.REGRT1;
                p = pp.Appendpp(p, obj.ADUFFZERO, obj.TYPE_NONE, 0L, 0L, obj.TYPE_MEM, 0L, 0L);
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = 4L * (64L - cnt / (2L * int64(gc.Widthptr)));
            }
            else
            {
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_CONST, 0L, 8L + off - 8L, obj.TYPE_REG, arm64.REGTMP, 0L);
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGSP, 0L, obj.TYPE_REG, arm64.REGRT1, 0L);
                p = pp.Appendpp(p, arm64.AADD, obj.TYPE_REG, arm64.REGTMP, 0L, obj.TYPE_REG, arm64.REGRT1, 0L);
                p.Reg = arm64.REGRT1;
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, arm64.REGTMP, 0L);
                p = pp.Appendpp(p, arm64.AADD, obj.TYPE_REG, arm64.REGTMP, 0L, obj.TYPE_REG, arm64.REGRT2, 0L);
                p.Reg = arm64.REGRT1;
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0L, obj.TYPE_MEM, arm64.REGRT1, int64(gc.Widthptr));
                p.Scond = arm64.C_XPRE;
                var p1 = p;
                p = pp.Appendpp(p, arm64.ACMP, obj.TYPE_REG, arm64.REGRT1, 0L, obj.TYPE_NONE, 0L, 0L);
                p.Reg = arm64.REGRT2;
                p = pp.Appendpp(p, arm64.ABNE, obj.TYPE_NONE, 0L, 0L, obj.TYPE_BRANCH, 0L, 0L);
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
                    var p = pp.Prog(arm64.AMOVD);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = arm64.REGZERO;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_AUTO;
                    p.To.Reg = arm64.REGSP;
                    p.To.Offset = n.Xoffset + i;
                    p.To.Sym = sym;
                    i += 8L;
                }

            }
        }

        private static void ginsnop(ref gc.Progs pp)
        {
            var p = pp.Prog(arm64.AHINT);
            p.From.Type = obj.TYPE_CONST;
        }
    }
}}}}
