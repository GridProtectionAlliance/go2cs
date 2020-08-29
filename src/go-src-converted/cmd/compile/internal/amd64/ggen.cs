// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2020 August 29 09:59:11 UTC
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
        private static readonly long dzBlocks = 16L; // number of MOV/ADD blocks
        private static readonly long dzBlockLen = 4L; // number of clears per block
        private static readonly long dzBlockSize = 19L; // size of instructions in a single block
        private static readonly long dzMovSize = 4L; // size of single MOV instruction w/ offset
        private static readonly long dzLeaqSize = 4L; // size of single LEAQ instruction
        private static readonly long dzClearStep = 16L; // number of bytes cleared by each MOV instruction

        private static readonly var dzClearLen = dzClearStep * dzBlockLen; // bytes cleared by one block
        private static readonly var dzSize = dzBlocks * dzBlockSize;

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

        private static ref obj.Prog zerorange(ref gc.Progs pp, ref obj.Prog p, long off, long cnt, ref uint state)
        {
            const long ax = 1L << (int)(iota);
            const var x0 = 0;

            if (cnt == 0L)
            {
                return p;
            }
            if (cnt % int64(gc.Widthreg) != 0L)
            { 
                // should only happen with nacl
                if (cnt % int64(gc.Widthptr) != 0L)
                {
                    gc.Fatalf("zerorange count not a multiple of widthptr %d", cnt);
                }
                if (state & ax == 0L.Value)
                {
                    p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, x86.REG_AX, 0L);
                    state.Value |= ax;
                }
                p = pp.Appendpp(p, x86.AMOVL, obj.TYPE_REG, x86.REG_AX, 0L, obj.TYPE_MEM, x86.REG_SP, off);
                off += int64(gc.Widthptr);
                cnt -= int64(gc.Widthptr);
            }
            if (cnt == 8L)
            {
                if (state & ax == 0L.Value)
                {
                    p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, x86.REG_AX, 0L);
                    state.Value |= ax;
                }
                p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_AX, 0L, obj.TYPE_MEM, x86.REG_SP, off);
            }
            else if (!isPlan9 && cnt <= int64(8L * gc.Widthreg))
            {
                if (state & x0 == 0L.Value)
                {
                    p = pp.Appendpp(p, x86.AXORPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_REG, x86.REG_X0, 0L);
                    state.Value |= x0;
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
            else if (!gc.Nacl && !isPlan9 && (cnt <= int64(128L * gc.Widthreg)))
            {
                if (state & x0 == 0L.Value)
                {
                    p = pp.Appendpp(p, x86.AXORPS, obj.TYPE_REG, x86.REG_X0, 0L, obj.TYPE_REG, x86.REG_X0, 0L);
                    state.Value |= x0;
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
                if (state & ax == 0L.Value)
                {
                    p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, 0L, obj.TYPE_REG, x86.REG_AX, 0L);
                    state.Value |= ax;
                }
                p = pp.Appendpp(p, x86.AMOVQ, obj.TYPE_CONST, 0L, cnt / int64(gc.Widthreg), obj.TYPE_REG, x86.REG_CX, 0L);
                p = pp.Appendpp(p, leaptr, obj.TYPE_MEM, x86.REG_SP, off, obj.TYPE_REG, x86.REG_DI, 0L);
                p = pp.Appendpp(p, x86.AREP, obj.TYPE_NONE, 0L, 0L, obj.TYPE_NONE, 0L, 0L);
                p = pp.Appendpp(p, x86.ASTOSQ, obj.TYPE_NONE, 0L, 0L, obj.TYPE_NONE, 0L, 0L);
            }
            return p;
        }

        private static void zeroAuto(ref gc.Progs pp, ref gc.Node n)
        { 
            // Note: this code must not clobber any registers.
            var op = x86.AMOVQ;
            if (gc.Widthptr == 4L)
            {
                op = x86.AMOVL;
            }
            var sym = n.Sym.Linksym();
            var size = n.Type.Size();
            {
                var i = int64(0L);

                while (i < size)
                {
                    var p = pp.Prog(op);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = 0L;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_AUTO;
                    p.To.Reg = x86.REG_SP;
                    p.To.Offset = n.Xoffset + i;
                    p.To.Sym = sym;
                    i += int64(gc.Widthptr);
                }

            }
        }

        private static void ginsnop(ref gc.Progs pp)
        { 
            // This is actually not the x86 NOP anymore,
            // but at the point where it gets used, AX is dead
            // so it's okay if we lose the high bits.
            var p = pp.Prog(x86.AXCHGL);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = x86.REG_AX;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = x86.REG_AX;
        }
    }
}}}}
