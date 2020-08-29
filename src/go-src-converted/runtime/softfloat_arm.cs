// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Software floating point interpretation of ARM 7500 FP instructions.
// The interpretation is not bit compatible with the 7500.
// It uses true little-endian doubles, while the 7500 used mixed-endian.

// package runtime -- go2cs converted at 2020 August 29 08:20:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\softfloat_arm.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly long _CPSR = 14L;
        private static readonly long _FLAGS_N = 1L << (int)(31L);
        private static readonly long _FLAGS_Z = 1L << (int)(30L);
        private static readonly long _FLAGS_C = 1L << (int)(29L);
        private static readonly long _FLAGS_V = 1L << (int)(28L);

        private static long fptrace = 0L;

        private static void fabort()
        {
            throw("unsupported floating point instruction");
        }

        private static void fputf(uint reg, uint val)
        {
            var _g_ = getg();
            _g_.m.freglo[reg] = val;
        }

        private static void fputd(uint reg, ulong val)
        {
            var _g_ = getg();
            _g_.m.freglo[reg] = uint32(val);
            _g_.m.freghi[reg] = uint32(val >> (int)(32L));
        }

        private static ulong fgetd(uint reg)
        {
            var _g_ = getg();
            return uint64(_g_.m.freglo[reg]) | uint64(_g_.m.freghi[reg]) << (int)(32L);
        }

        private static void fprintregs()
        {
            var _g_ = getg();
            foreach (var (i) in _g_.m.freglo)
            {
                print("\tf", i, ":\t", hex(_g_.m.freghi[i]), " ", hex(_g_.m.freglo[i]), "\n");
            }
        }

        private static uint fstatus(bool nan, int cmp)
        {
            if (nan)
            {
                return _FLAGS_C | _FLAGS_V;
            }
            if (cmp == 0L)
            {
                return _FLAGS_Z | _FLAGS_C;
            }
            if (cmp < 0L)
            {
                return _FLAGS_N;
            }
            return _FLAGS_C;
        }

        // conditions array record the required CPSR cond field for the
        // first 5 pairs of conditional execution opcodes
        // higher 4 bits are must set, lower 4 bits are must clear
        private static array<uint> conditions = new array<uint>(InitKeyedValues<uint>(10/2, (0/2, _FLAGS_Z>>24|0), (2/2, _FLAGS_C>>24|0), (4/2, _FLAGS_N>>24|0), (6/2, _FLAGS_V>>24|0), (8/2, _FLAGS_C>>24|_FLAGS_Z>>28)));

        private static readonly ulong _FAULT = 0x80000000UL; // impossible PC offset

        // returns number of words that the fp instruction
        // is occupying, 0 if next instruction isn't float.
 // impossible PC offset

        // returns number of words that the fp instruction
        // is occupying, 0 if next instruction isn't float.
        private static uint stepflt(ref uint pc, ref array<uint> regs)
        {
            uint i = default;            uint opc = default;            uint regd = default;            uint regm = default;            uint regn = default;            uint cpsr = default; 

            // m is locked in vlop_arm.s, so g.m cannot change during this function call,
            // so caching it in a local variable is safe.
 

            // m is locked in vlop_arm.s, so g.m cannot change during this function call,
            // so caching it in a local variable is safe.
            var m = getg().m;
            i = pc.Value;

            if (fptrace > 0L)
            {
                print("stepflt ", pc, " ", hex(i), " (cpsr ", hex(regs[_CPSR] >> (int)(28L)), ")\n");
            }
            opc = i >> (int)(28L);
            if (opc == 14L)
            { // common case first
                goto execute;
            }
            cpsr = regs[_CPSR] >> (int)(28L);
            switch (opc)
            {
                case 0L: 

                case 1L: 

                case 2L: 

                case 3L: 

                case 4L: 

                case 5L: 

                case 6L: 

                case 7L: 

                case 8L: 

                case 9L: 
                    if (cpsr & (conditions[opc / 2L] >> (int)(4L)) == conditions[opc / 2L] >> (int)(4L) && cpsr & (conditions[opc / 2L] & 0xfUL) == 0L)
                    {
                        if (opc & 1L != 0L)
                        {
                            return 1L;
                        }
                    }
                    else
                    {
                        if (opc & 1L == 0L)
                        {
                            return 1L;
                        }
                    }
                    break;
                case 10L: // GE (N == V), LT (N != V)

                case 11L: // GE (N == V), LT (N != V)
                    if (cpsr & (_FLAGS_N >> (int)(28L)) == cpsr & (_FLAGS_V >> (int)(28L)))
                    {
                        if (opc & 1L != 0L)
                        {
                            return 1L;
                        }
                    }
                    else
                    {
                        if (opc & 1L == 0L)
                        {
                            return 1L;
                        }
                    }
                    break;
                case 12L: // GT (N == V and Z == 0), LE (N != V or Z == 1)

                case 13L: // GT (N == V and Z == 0), LE (N != V or Z == 1)
                    if (cpsr & (_FLAGS_N >> (int)(28L)) == cpsr & (_FLAGS_V >> (int)(28L)) && cpsr & (_FLAGS_Z >> (int)(28L)) == 0L)
                    {
                        if (opc & 1L != 0L)
                        {
                            return 1L;
                        }
                    }
                    else
                    {
                        if (opc & 1L == 0L)
                        {
                            return 1L;
                        }
                    }
                    break;
                case 14L: 
                    break;
                case 15L: // shouldn't happen
                    return 0L;
                    break;
            }

            if (fptrace > 0L)
            {
                print("conditional ", hex(opc), " (cpsr ", hex(cpsr), ") pass\n");
            }
            i = 0xeUL << (int)(28L) | i & (1L << (int)(28L) - 1L);

execute:
            if (i & 0xfffff000UL == 0xe59fb000UL)
            { 
                // load r11 from pc-relative address.
                // might be part of a floating point move
                // (or might not, but no harm in simulating
                // one instruction too many).
                ref array<uint> addr = new ptr<ref array<uint>>(add(@unsafe.Pointer(pc), uintptr(i & 0xfffUL + 8L)));
                regs[11L] = addr[0L];

                if (fptrace > 0L)
                {
                    print("*** cpu R[11] = *(", addr, ") ", hex(regs[11L]), "\n");
                }
                return 1L;
            }
            if (i == 0xe08fb00bUL)
            { 
                // add pc to r11
                // might be part of a PIC floating point move
                // (or might not, but again no harm done).
                regs[11L] += uint32(uintptr(@unsafe.Pointer(pc))) + 8L;

                if (fptrace > 0L)
                {
                    print("*** cpu R[11] += pc ", hex(regs[11L]), "\n");
                }
                return 1L;
            }
            if (i & 0xfffffff0UL == 0xe08bb000UL)
            {
                var r = i & 0xfUL; 
                // add r to r11.
                // might be part of a large offset address calculation
                // (or might not, but again no harm done).
                regs[11L] += regs[r];

                if (fptrace > 0L)
                {
                    print("*** cpu R[11] += R[", r, "] ", hex(regs[11L]), "\n");
                }
                return 1L;
            }
            if (i == 0xeef1fa10UL)
            {
                regs[_CPSR] = regs[_CPSR] & 0x0fffffffUL | m.fflag;

                if (fptrace > 0L)
                {
                    print("*** fpsr R[CPSR] = F[CPSR] ", hex(regs[_CPSR]), "\n");
                }
                return 1L;
            }
            if (i & 0xff000000UL == 0xea000000UL)
            { 
                // unconditional branch
                // can happen in the middle of floating point
                // if the linker decides it is time to lay down
                // a sequence of instruction stream constants.
                var delta = int32(i & 0xffffffUL) << (int)(8L) >> (int)(8L); // sign extend

                if (fptrace > 0L)
                {
                    print("*** cpu PC += ", hex((delta + 2L) * 4L), "\n");
                }
                return uint32(delta + 2L);
            } 

            // load/store regn is cpureg, regm is 8bit offset
            regd = i >> (int)(12L) & 0xfUL;
            regn = i >> (int)(16L) & 0xfUL;
            regm = i & 0xffUL << (int)(2L); // PLUS or MINUS ??

            switch (i & 0xfff00f00UL)
            {
                case 0xed900a00UL: // single load
                    var uaddr = uintptr(regs[regn] + regm);
                    if (uaddr < 4096L)
                    {
                        if (fptrace > 0L)
                        {
                            print("*** load @", hex(uaddr), " => fault\n");
                        }
                        return _FAULT;
                    }
                    addr = new ptr<ref array<uint>>(@unsafe.Pointer(uaddr));
                    m.freglo[regd] = addr[0L];

                    if (fptrace > 0L)
                    {
                        print("*** load F[", regd, "] = ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xed900b00UL: // double load
                    uaddr = uintptr(regs[regn] + regm);
                    if (uaddr < 4096L)
                    {
                        if (fptrace > 0L)
                        {
                            print("*** double load @", hex(uaddr), " => fault\n");
                        }
                        return _FAULT;
                    }
                    addr = new ptr<ref array<uint>>(@unsafe.Pointer(uaddr));
                    m.freglo[regd] = addr[0L];
                    m.freghi[regd] = addr[1L];

                    if (fptrace > 0L)
                    {
                        print("*** load D[", regd, "] = ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xed800a00UL: // single store
                    uaddr = uintptr(regs[regn] + regm);
                    if (uaddr < 4096L)
                    {
                        if (fptrace > 0L)
                        {
                            print("*** store @", hex(uaddr), " => fault\n");
                        }
                        return _FAULT;
                    }
                    addr = new ptr<ref array<uint>>(@unsafe.Pointer(uaddr));
                    addr[0L] = m.freglo[regd];

                    if (fptrace > 0L)
                    {
                        print("*** *(", addr, ") = ", hex(addr[0L]), "\n");
                    }
                    return 1L;
                    break;
                case 0xed800b00UL: // double store
                    uaddr = uintptr(regs[regn] + regm);
                    if (uaddr < 4096L)
                    {
                        if (fptrace > 0L)
                        {
                            print("*** double store @", hex(uaddr), " => fault\n");
                        }
                        return _FAULT;
                    }
                    addr = new ptr<ref array<uint>>(@unsafe.Pointer(uaddr));
                    addr[0L] = m.freglo[regd];
                    addr[1L] = m.freghi[regd];

                    if (fptrace > 0L)
                    {
                        print("*** *(", addr, ") = ", hex(addr[1L]), "-", hex(addr[0L]), "\n");
                    }
                    return 1L;
                    break;
            } 

            // regd, regm, regn are 4bit variables
            regm = i >> (int)(0L) & 0xfUL;
            switch (i & 0xfff00ff0UL)
            {
                case 0xf3000110UL: // veor
                    m.freglo[regd] = m.freglo[regm] ^ m.freglo[regn];
                    m.freghi[regd] = m.freghi[regm] ^ m.freghi[regn];

                    if (fptrace > 0L)
                    {
                        print("*** veor D[", regd, "] = ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb00b00UL: // D[regd] = const(regn,regm)
                    regn = regn << (int)(4L) | regm;
                    regm = 0x40000000UL;
                    if (regn & 0x80UL != 0L)
                    {
                        regm |= 0x80000000UL;
                    }
                    if (regn & 0x40UL != 0L)
                    {
                        regm ^= 0x7fc00000UL;
                    }
                    regm |= regn & 0x3fUL << (int)(16L);
                    m.freglo[regd] = 0L;
                    m.freghi[regd] = regm;

                    if (fptrace > 0L)
                    {
                        print("*** immed D[", regd, "] = ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb00a00UL: // F[regd] = const(regn,regm)
                    regn = regn << (int)(4L) | regm;
                    regm = 0x40000000UL;
                    if (regn & 0x80UL != 0L)
                    {
                        regm |= 0x80000000UL;
                    }
                    if (regn & 0x40UL != 0L)
                    {
                        regm ^= 0x7e000000UL;
                    }
                    regm |= regn & 0x3fUL << (int)(19L);
                    m.freglo[regd] = regm;

                    if (fptrace > 0L)
                    {
                        print("*** immed D[", regd, "] = ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee300b00UL: // D[regd] = D[regn]+D[regm]
                    fputd(regd, fadd64(fgetd(regn), fgetd(regm)));

                    if (fptrace > 0L)
                    {
                        print("*** add D[", regd, "] = D[", regn, "]+D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee300a00UL: // F[regd] = F[regn]+F[regm]
                    m.freglo[regd] = f64to32(fadd64(f32to64(m.freglo[regn]), f32to64(m.freglo[regm])));

                    if (fptrace > 0L)
                    {
                        print("*** add F[", regd, "] = F[", regn, "]+F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee300b40UL: // D[regd] = D[regn]-D[regm]
                    fputd(regd, fsub64(fgetd(regn), fgetd(regm)));

                    if (fptrace > 0L)
                    {
                        print("*** sub D[", regd, "] = D[", regn, "]-D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee300a40UL: // F[regd] = F[regn]-F[regm]
                    m.freglo[regd] = f64to32(fsub64(f32to64(m.freglo[regn]), f32to64(m.freglo[regm])));

                    if (fptrace > 0L)
                    {
                        print("*** sub F[", regd, "] = F[", regn, "]-F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee200b00UL: // D[regd] = D[regn]*D[regm]
                    fputd(regd, fmul64(fgetd(regn), fgetd(regm)));

                    if (fptrace > 0L)
                    {
                        print("*** mul D[", regd, "] = D[", regn, "]*D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee200a00UL: // F[regd] = F[regn]*F[regm]
                    m.freglo[regd] = f64to32(fmul64(f32to64(m.freglo[regn]), f32to64(m.freglo[regm])));

                    if (fptrace > 0L)
                    {
                        print("*** mul F[", regd, "] = F[", regn, "]*F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee800b00UL: // D[regd] = D[regn]/D[regm]
                    fputd(regd, fdiv64(fgetd(regn), fgetd(regm)));

                    if (fptrace > 0L)
                    {
                        print("*** div D[", regd, "] = D[", regn, "]/D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee800a00UL: // F[regd] = F[regn]/F[regm]
                    m.freglo[regd] = f64to32(fdiv64(f32to64(m.freglo[regn]), f32to64(m.freglo[regm])));

                    if (fptrace > 0L)
                    {
                        print("*** div F[", regd, "] = F[", regn, "]/F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee000b10UL: // S[regn] = R[regd] (MOVW) (regm ignored)
                    m.freglo[regn] = regs[regd];

                    if (fptrace > 0L)
                    {
                        print("*** cpy S[", regn, "] = R[", regd, "] ", hex(m.freglo[regn]), "\n");
                    }
                    return 1L;
                    break;
                case 0xee100b10UL: // R[regd] = S[regn] (MOVW) (regm ignored)
                    regs[regd] = m.freglo[regn];

                    if (fptrace > 0L)
                    {
                        print("*** cpy R[", regd, "] = S[", regn, "] ", hex(regs[regd]), "\n");
                    }
                    return 1L;
                    break;
            } 

            // regd, regm are 4bit variables
            switch (i & 0xffff0ff0UL)
            {
                case 0xeeb00a40UL: // F[regd] = F[regm] (MOVF)
                    m.freglo[regd] = m.freglo[regm];

                    if (fptrace > 0L)
                    {
                        print("*** F[", regd, "] = F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb00b40UL: // D[regd] = D[regm] (MOVD)
                    m.freglo[regd] = m.freglo[regm];
                    m.freghi[regd] = m.freghi[regm];

                    if (fptrace > 0L)
                    {
                        print("*** D[", regd, "] = D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb10bc0UL: // D[regd] = sqrt D[regm]
                    fputd(regd, sqrt(fgetd(regm)));

                    if (fptrace > 0L)
                    {
                        print("*** D[", regd, "] = sqrt D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb00bc0UL: // D[regd] = abs D[regm]
                    m.freglo[regd] = m.freglo[regm];
                    m.freghi[regd] = m.freghi[regm] & (1L << (int)(31L) - 1L);

                    if (fptrace > 0L)
                    {
                        print("*** D[", regd, "] = abs D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb00ac0UL: // F[regd] = abs F[regm]
                    m.freglo[regd] = m.freglo[regm] & (1L << (int)(31L) - 1L);

                    if (fptrace > 0L)
                    {
                        print("*** F[", regd, "] = abs F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb10b40UL: // D[regd] = neg D[regm]
                    m.freglo[regd] = m.freglo[regm];
                    m.freghi[regd] = m.freghi[regm] ^ 1L << (int)(31L);

                    if (fptrace > 0L)
                    {
                        print("*** D[", regd, "] = neg D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb10a40UL: // F[regd] = neg F[regm]
                    m.freglo[regd] = m.freglo[regm] ^ 1L << (int)(31L);

                    if (fptrace > 0L)
                    {
                        print("*** F[", regd, "] = neg F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb40bc0UL: // D[regd] :: D[regm] (CMPD)
                    var (cmp, nan) = fcmp64(fgetd(regd), fgetd(regm));
                    m.fflag = fstatus(nan, cmp);

                    if (fptrace > 0L)
                    {
                        print("*** cmp D[", regd, "]::D[", regm, "] ", hex(m.fflag), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb40ac0UL: // F[regd] :: F[regm] (CMPF)
                    (cmp, nan) = fcmp64(f32to64(m.freglo[regd]), f32to64(m.freglo[regm]));
                    m.fflag = fstatus(nan, cmp);

                    if (fptrace > 0L)
                    {
                        print("*** cmp F[", regd, "]::F[", regm, "] ", hex(m.fflag), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb50bc0UL: // D[regd] :: 0 (CMPD)
                    (cmp, nan) = fcmp64(fgetd(regd), 0L);
                    m.fflag = fstatus(nan, cmp);

                    if (fptrace > 0L)
                    {
                        print("*** cmp D[", regd, "]::0 ", hex(m.fflag), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb50ac0UL: // F[regd] :: 0 (CMPF)
                    (cmp, nan) = fcmp64(f32to64(m.freglo[regd]), 0L);
                    m.fflag = fstatus(nan, cmp);

                    if (fptrace > 0L)
                    {
                        print("*** cmp F[", regd, "]::0 ", hex(m.fflag), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb70ac0UL: // D[regd] = F[regm] (MOVFD)
                    fputd(regd, f32to64(m.freglo[regm]));

                    if (fptrace > 0L)
                    {
                        print("*** f2d D[", regd, "]=F[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb70bc0UL: // F[regd] = D[regm] (MOVDF)
                    m.freglo[regd] = f64to32(fgetd(regm));

                    if (fptrace > 0L)
                    {
                        print("*** d2f F[", regd, "]=D[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeebd0ac0UL: // S[regd] = F[regm] (MOVFW)
                    var (sval, ok) = f64toint(f32to64(m.freglo[regm]));
                    if (!ok || int64(int32(sval)) != sval)
                    {
                        sval = 0L;
                    }
                    m.freglo[regd] = uint32(sval);
                    if (fptrace > 0L)
                    {
                        print("*** fix S[", regd, "]=F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeebc0ac0UL: // S[regd] = F[regm] (MOVFW.U)
                    (sval, ok) = f64toint(f32to64(m.freglo[regm]));
                    if (!ok || int64(uint32(sval)) != sval)
                    {
                        sval = 0L;
                    }
                    m.freglo[regd] = uint32(sval);

                    if (fptrace > 0L)
                    {
                        print("*** fix unsigned S[", regd, "]=F[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeebd0bc0UL: // S[regd] = D[regm] (MOVDW)
                    (sval, ok) = f64toint(fgetd(regm));
                    if (!ok || int64(int32(sval)) != sval)
                    {
                        sval = 0L;
                    }
                    m.freglo[regd] = uint32(sval);

                    if (fptrace > 0L)
                    {
                        print("*** fix S[", regd, "]=D[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeebc0bc0UL: // S[regd] = D[regm] (MOVDW.U)
                    (sval, ok) = f64toint(fgetd(regm));
                    if (!ok || int64(uint32(sval)) != sval)
                    {
                        sval = 0L;
                    }
                    m.freglo[regd] = uint32(sval);

                    if (fptrace > 0L)
                    {
                        print("*** fix unsigned S[", regd, "]=D[", regm, "] ", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb80ac0UL: // D[regd] = S[regm] (MOVWF)
                    var cmp = int32(m.freglo[regm]);
                    if (cmp < 0L)
                    {
                        fputf(regd, f64to32(fintto64(-int64(cmp))));
                        m.freglo[regd] ^= 0x80000000UL;
                    }
                    else
                    {
                        fputf(regd, f64to32(fintto64(int64(cmp))));
                    }
                    if (fptrace > 0L)
                    {
                        print("*** float D[", regd, "]=S[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb80a40UL: // D[regd] = S[regm] (MOVWF.U)
                    fputf(regd, f64to32(fintto64(int64(m.freglo[regm]))));

                    if (fptrace > 0L)
                    {
                        print("*** float unsigned D[", regd, "]=S[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb80bc0UL: // D[regd] = S[regm] (MOVWD)
                    cmp = int32(m.freglo[regm]);
                    if (cmp < 0L)
                    {
                        fputd(regd, fintto64(-int64(cmp)));
                        m.freghi[regd] ^= 0x80000000UL;
                    }
                    else
                    {
                        fputd(regd, fintto64(int64(cmp)));
                    }
                    if (fptrace > 0L)
                    {
                        print("*** float D[", regd, "]=S[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
                case 0xeeb80b40UL: // D[regd] = S[regm] (MOVWD.U)
                    fputd(regd, fintto64(int64(m.freglo[regm])));

                    if (fptrace > 0L)
                    {
                        print("*** float unsigned D[", regd, "]=S[", regm, "] ", hex(m.freghi[regd]), "-", hex(m.freglo[regd]), "\n");
                    }
                    return 1L;
                    break;
            }

            if (i & 0xff000000UL == 0xee000000UL || i & 0xff000000UL == 0xed000000UL)
            {
                print("stepflt ", pc, " ", hex(i), "\n");
                fabort();
            }
            return 0L;
        }

        //go:nosplit
        private static uint _sfloat2(uint pc, array<uint> regs)
        {
            regs = regs.Clone();

            systemstack(() =>
            {
                newpc = sfloat2(pc, ref regs);
            });
            return;
        }

        private static void _sfloatpanic()
;

        private static uint sfloat2(uint pc, ref array<uint> regs)
        {
            var first = true;
            while (true)
            {>>MARKER:FUNCTION__sfloatpanic_BLOCK_PREFIX<<
                var skip = stepflt((uint32.Value)(@unsafe.Pointer(uintptr(pc))), regs);
                if (skip == 0L)
                {
                    break;
                }
                first = false;
                if (skip == _FAULT)
                { 
                    // Encountered bad address in store/load.
                    // Record signal information and return to assembly
                    // trampoline that fakes the call.
                    const long SIGSEGV = 11L;

                    var curg = getg().m.curg;
                    curg.sig = SIGSEGV;
                    curg.sigcode0 = 0L;
                    curg.sigcode1 = 0L;
                    curg.sigpc = uintptr(pc);
                    pc = uint32(funcPC(_sfloatpanic));
                    break;
                }
                pc += 4L * skip;
            }

            if (first)
            {
                print("sfloat2 ", pc, " ", hex(@unsafe.Pointer(uintptr(pc)).Value), "\n");
                fabort(); // not ok to fail first instruction
            }
            return pc;
        }

        // Stubs to pacify vet. Not safe to call from Go.
        // Calls to these functions are inserted by the compiler or assembler.
        private static void _sfloat()
;
        private static void udiv()
;
        private static void _div()
;
        private static void _divu()
;
        private static void _mod()
;
        private static void _modu()
;
    }
}
