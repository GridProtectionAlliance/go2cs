// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64 -- go2cs converted at 2020 October 09 05:43:59 UTC
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
            // arm64 requires that the frame size (not counting saved FP&LR)
            // be 16 bytes aligned. If not, pad it.
            if (frame % 16L != 0L)
            {
                frame += 16L - (frame % 16L);
            }

            return frame;

        }

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

                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGSP, 0L, obj.TYPE_REG, arm64.REG_R20, 0L);
                p = pp.Appendpp(p, arm64.AADD, obj.TYPE_CONST, 0L, 8L + off, obj.TYPE_REG, arm64.REG_R20, 0L);
                p.Reg = arm64.REG_R20;
                p = pp.Appendpp(p, obj.ADUFFZERO, obj.TYPE_NONE, 0L, 0L, obj.TYPE_MEM, 0L, 0L);
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = 4L * (64L - cnt / (2L * int64(gc.Widthptr)));

            }
            else
            { 
                // Not using REGTMP, so this is async preemptible (async preemption clobbers REGTMP).
                // We are at the function entry, where no register is live, so it is okay to clobber
                // other registers
                const var rtmp = arm64.REG_R20;

                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_CONST, 0L, 8L + off - 8L, obj.TYPE_REG, rtmp, 0L);
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGSP, 0L, obj.TYPE_REG, arm64.REGRT1, 0L);
                p = pp.Appendpp(p, arm64.AADD, obj.TYPE_REG, rtmp, 0L, obj.TYPE_REG, arm64.REGRT1, 0L);
                p.Reg = arm64.REGRT1;
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_CONST, 0L, cnt, obj.TYPE_REG, rtmp, 0L);
                p = pp.Appendpp(p, arm64.AADD, obj.TYPE_REG, rtmp, 0L, obj.TYPE_REG, arm64.REGRT2, 0L);
                p.Reg = arm64.REGRT1;
                p = pp.Appendpp(p, arm64.AMOVD, obj.TYPE_REG, arm64.REGZERO, 0L, obj.TYPE_MEM, arm64.REGRT1, int64(gc.Widthptr));
                p.Scond = arm64.C_XPRE;
                var p1 = p;
                p = pp.Appendpp(p, arm64.ACMP, obj.TYPE_REG, arm64.REGRT1, 0L, obj.TYPE_NONE, 0L, 0L);
                p.Reg = arm64.REGRT2;
                p = pp.Appendpp(p, arm64.ABNE, obj.TYPE_NONE, 0L, 0L, obj.TYPE_BRANCH, 0L, 0L);
                gc.Patch(p, p1);

            }

            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;

            var p = pp.Prog(arm64.AHINT);
            p.From.Type = obj.TYPE_CONST;
            return _addr_p!;
        }
    }
}}}}
