// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package s390x -- go2cs converted at 2020 August 29 09:24:51 UTC
// import "cmd/compile/internal/s390x" ==> using s390x = go.cmd.compile.@internal.s390x_package
// Original source: C:\Go\src\cmd\compile\internal\s390x\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class s390x_package
    {
        // clearLoopCutOff is the (somewhat arbitrary) value above which it is better
        // to have a loop of clear instructions (e.g. XCs) rather than just generating
        // multiple instructions (i.e. loop unrolling).
        // Must be between 256 and 4096.
        private static readonly long clearLoopCutoff = 1024L;

        // zerorange clears the stack in the given range.


        // zerorange clears the stack in the given range.
        private static ref obj.Prog zerorange(ref gc.Progs pp, ref obj.Prog p, long off, long cnt, ref uint _)
        {
            if (cnt == 0L)
            {
                return p;
            } 

            // Adjust the frame to account for LR.
            off += gc.Ctxt.FixedFrameSize();
            var reg = int16(s390x.REGSP); 

            // If the off cannot fit in a 12-bit unsigned displacement then we
            // need to create a copy of the stack pointer that we can adjust.
            // We also need to do this if we are going to loop.
            if (off < 0L || off > 4096L - clearLoopCutoff || cnt > clearLoopCutoff)
            {
                p = pp.Appendpp(p, s390x.AADD, obj.TYPE_CONST, 0L, off, obj.TYPE_REG, s390x.REGRT1, 0L);
                p.Reg = int16(s390x.REGSP);
                reg = s390x.REGRT1;
                off = 0L;
            } 

            // Generate a loop of large clears.
            if (cnt > clearLoopCutoff)
            {
                var n = cnt - (cnt % 256L);
                var end = int16(s390x.REGRT2);
                p = pp.Appendpp(p, s390x.AADD, obj.TYPE_CONST, 0L, off + n, obj.TYPE_REG, end, 0L);
                p.Reg = reg;
                p = pp.Appendpp(p, s390x.ACLEAR, obj.TYPE_CONST, 0L, 256L, obj.TYPE_MEM, reg, off);
                var pl = p;
                p = pp.Appendpp(p, s390x.AADD, obj.TYPE_CONST, 0L, 256L, obj.TYPE_REG, reg, 0L);
                p = pp.Appendpp(p, s390x.ACMP, obj.TYPE_REG, reg, 0L, obj.TYPE_REG, end, 0L);
                p = pp.Appendpp(p, s390x.ABNE, obj.TYPE_NONE, 0L, 0L, obj.TYPE_BRANCH, 0L, 0L);
                gc.Patch(p, pl);

                cnt -= n;
            } 

            // Generate remaining clear instructions without a loop.
            while (cnt > 0L)
            {
                n = cnt; 

                // Can clear at most 256 bytes per instruction.
                if (n > 256L)
                {
                    n = 256L;
                }
                switch (n)
                { 
                // Handle very small clears with move instructions.
                    case 8L: 

                    case 4L: 

                    case 2L: 

                    case 1L: 
                        var ins = s390x.AMOVB;
                        switch (n)
                        {
                            case 8L: 
                                ins = s390x.AMOVD;
                                break;
                            case 4L: 
                                ins = s390x.AMOVW;
                                break;
                            case 2L: 
                                ins = s390x.AMOVH;
                                break;
                        }
                        p = pp.Appendpp(p, ins, obj.TYPE_CONST, 0L, 0L, obj.TYPE_MEM, reg, off); 

                        // Handle clears that would require multiple move instructions with CLEAR (assembled as XC).
                        break;
                    default: 
                        p = pp.Appendpp(p, s390x.ACLEAR, obj.TYPE_CONST, 0L, n, obj.TYPE_MEM, reg, off);
                        break;
                }

                cnt -= n;
                off += n;
            }


            return p;
        }

        private static void zeroAuto(ref gc.Progs pp, ref gc.Node n)
        { 
            // Note: this code must not clobber any registers or the
            // condition code.
            var sym = n.Sym.Linksym();
            var size = n.Type.Size();
            {
                var i = int64(0L);

                while (i < size)
                {
                    var p = pp.Prog(s390x.AMOVD);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = 0L;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_AUTO;
                    p.To.Reg = s390x.REGSP;
                    p.To.Offset = n.Xoffset + i;
                    p.To.Sym = sym;
                    i += int64(gc.Widthptr);
                }

            }
        }

        private static void ginsnop(ref gc.Progs pp)
        {
            var p = pp.Prog(s390x.AOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = int16(s390x.REG_R0);
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = int16(s390x.REG_R0);
        }
    }
}}}}
