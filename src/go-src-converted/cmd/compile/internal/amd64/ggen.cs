// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2020 October 08 04:32:20 UTC
// import "cmd/compile/internal/amd64" ==> using amd64 = go.cmd.compile.@internal.amd64_package
// Original source: C:\Go\src\cmd\compile\internal\amd64\ggen.go
using gc = go.cmd.compile.@internal.gc_package;
using obj = go.cmd.@internal.obj_package;
using x86 = go.cmd.@internal.obj.x86_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class amd64_package
    {
        // no floating point in note handlers on Plan 9
        private static var isPlan9 = objabi.GOOS == "plan9";

        // DUFFZERO consists of repeated blocks of 4 MOVUPSs + LEAQ,
        // See runtime/mkduff.go.
        private static readonly long dzBlocks = (long)16L; // number of MOV/ADD blocks
        private static readonly long dzBlockLen = (long)4L; // number of clears per block
        private static readonly long dzBlockSize = (long)19L; // size of instructions in a single block
        private static readonly long dzMovSize = (long)4L; // size of single MOV instruction w/ offset
        private static readonly long dzLeaqSize = (long)4L; // size of single LEAQ instruction
        private static readonly long dzClearStep = (long)16L; // number of bytes cleared by each MOV instruction

        private static readonly var dzClearLen = (var)dzClearStep * dzBlockLen; // bytes cleared by one block
        private static readonly var dzSize = (var)dzBlocks * dzBlockSize;


        // dzOff returns the offset for a jump into DUFFZERO.
        // b is the number of bytes to zero.
        private static long dzOff(long b)
        {
            var off = int64(dzSize);
            off -= b / dzClearLen * dzBlockSize;
            var tailLen = b % dzClearLen;
            if (tailLen >= dzClearStep)
            {
                off -= dzLeaqSize + dzMovSize * (tailLen / dzClearStep);
            }

            return off;

        }

        // duffzeroDI returns the pre-adjustment to DI for a call to DUFFZERO.
        // b is the number of bytes to zero.
        private static long dzDI(long b)
        {
            var tailLen = b % dzClearLen;
            if (tailLen < dzClearStep)
            {
                return 0L;
            }

            var tailSteps = tailLen / dzClearStep;
            return -dzClearStep * (dzBlockLen - tailSteps);

        }

        private static ptr<obj.Prog> zerorange(ptr<gc.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr_state)
        {
            ref gc.Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;
            ref uint state = ref _addr_state.val;

            const long ax = (long)1L << (int)(iota);
            const var x0 = (var)0;

            if (cnt == 0L)
            {
                return _addr_p!;
            }

            if (cnt % int64(gc.Widthreg) != 0L)
            { 
                // should only happen with nacl
                if (cnt % int64(gc.Widthptr) != 0L)
                {
                    gc.Fatalf("zerorange count not a multiple of widthptr %d", cnt);
                }

                if (state & ax == 0L.val)
                {
                    p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, x86.REG_AX, 0L);
                    state |= ax;
                }

                p = pp.Appendpp(p, x86.AMOVL, obj.TYPE_REG, x86.REG_AX, 0L, obj.TYPE_MEM, x86.REG_SP, off);
                off += int64(gc.Widthptr);
                cnt -= int64(gc.Widthptr);

            }

            if (cnt == 8L)
            {
                if (state & ax == 0L.val)
                {
                    p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, x86.REG_AX, 0L);
                    state |= ax;
                }

                p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_AX, 0L, obj.TYPE_MEM, x86.REG_SP, off);

            }
            else if (!isPlan9 && cnt <= int64(8L * gc.Widthreg))
            {
                if (state & x0 == 0L.val)
                {
                    p = pp.Appendpp(p, x86.AXORPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_REG, x86.REG_X0, 0L);
                    state |= x0;
                }

                for (var i = int64(0L); i < cnt / 16L; i++)
                {
                    p = pp.Appendpp(p, x86.AMOVUPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_MEM, x86.REG_SP, off + i * 16L);
                }


                if (cnt % 16L != 0L)
                {
                    p = pp.Appendpp(p, x86.AMOVUPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_MEM, x86.REG_SP, off + cnt - int64(16L));
                }

            }
            else if (!isPlan9 && (cnt <= int64(128L * gc.Widthreg)))
            {
                if (state & x0 == 0L.val)
                {
                    p = pp.Appendpp(p, x86.AXORPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_REG, x86.REG_X0, 0L);
                    state |= x0;
                }

                p = pp.Appendpp(p, leaptr, obj.TYPE_MEM, x86.REG_SP, off + dzDI(cnt), obj.TYPE_REG, x86.REG_DI, 0L);
                p = pp.Appendpp(p, obj.ADUFFZERO, obj.TYPE_NONE, 0L, 0L, obj.TYPE_ADDR, 0L, dzOff(cnt));
                p.To.Sym = gc.Duffzero;

                if (cnt % 16L != 0L)
                {
                    p = pp.Appendpp(p, x86.AMOVUPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_MEM, x86.REG_DI, -int64(8L));
                }

            }
            else
            {
                if (state & ax == 0L.val)
                {
                    p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, x86.REG_AX, 0L);
                    state |= ax;
                }

                p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, cnt / int64(gc.Widthreg), obj.TYPE_REG, x86.REG_CX, 0L);
                p = pp.Appendpp(p, leaptr, obj.TYPE_MEM, x86.REG_SP, off, obj.TYPE_REG, x86.REG_DI, 0L);
                p = pp.Appendpp(p, x86.AREP, obj.TYPE_NONE, 0L, 0L, obj.TYPE_NONE, 0L, 0L);
                p = pp.Appendpp(p, x86.ASTOSQ, obj.TYPE_NONE, 0L, 0L, obj.TYPE_NONE, 0L, 0L);

            }

            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;
 
            // This is a hardware nop (1-byte 0x90) instruction,
            // even though we describe it as an explicit XCHGL here.
            // Particularly, this does not zero the high 32 bits
            // like typical *L opcodes.
            // (gas assembles "xchg %eax,%eax" to 0x87 0xc0, which
            // does zero the high 32 bits.)
            var p = pp.Prog(x86.AXCHGL);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = x86.REG_AX;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = x86.REG_AX;
            return _addr_p!;

        }
    }
}}}}
