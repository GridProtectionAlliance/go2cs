// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package s390x -- go2cs converted at 2020 October 08 04:27:17 UTC
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
        private static readonly long clearLoopCutoff = (long)1024L;

        // zerorange clears the stack in the given range.


        // zerorange clears the stack in the given range.
        private static ptr<obj.Prog> zerorange(ptr<gc.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr__)
        {
            ref gc.Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;
            ref uint _ = ref _addr__.val;

            if (cnt == 0L)
            {
                return _addr_p!;
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
                var ireg = int16(s390x.REGRT2); // register holds number of remaining loop iterations
                p = pp.Appendpp(p, s390x.AMOVD, obj.TYPE_CONST, 0L, cnt / 256L, obj.TYPE_REG, ireg, 0L);
                p = pp.Appendpp(p, s390x.ACLEAR, obj.TYPE_CONST, 0L, 256L, obj.TYPE_MEM, reg, off);
                var pl = p;
                p = pp.Appendpp(p, s390x.AADD, obj.TYPE_CONST, 0L, 256L, obj.TYPE_REG, reg, 0L);
                p = pp.Appendpp(p, s390x.ABRCTG, obj.TYPE_REG, ireg, 0L, obj.TYPE_BRANCH, 0L, 0L);
                gc.Patch(p, pl);
                cnt = cnt % 256L;

            } 

            // Generate remaining clear instructions without a loop.
            while (cnt > 0L)
            {
                var n = cnt; 

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


            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;

            return _addr_pp.Prog(s390x.ANOPH)!;
        }
    }
}}}}
