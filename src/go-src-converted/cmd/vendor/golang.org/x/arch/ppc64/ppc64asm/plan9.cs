// Copyright 2015 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2020 August 29 10:07:55 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\plan9.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace ppc64
{
    public static partial class ppc64asm_package
    {
        // GoSyntax returns the Go assembler syntax for the instruction.
        // The pc is the program counter of the first instruction, used for expanding
        // PC-relative addresses into absolute ones.
        // The symname function queries the symbol table for the program
        // being disassembled. It returns the name and base address of the symbol
        // containing the target, if any; otherwise it returns "", 0.
        public static @string GoSyntax(Inst inst, ulong pc, Func<ulong, (@string, ulong)> symname)
        {
            if (symname == null)
            {
                symname = _p0 => ("", 0L);
            }
            if (inst.Op == 0L)
            {
                return "?";
            }
            slice<@string> args = default;
            foreach (var (i, a) in inst.Args[..])
            {
                if (a == null)
                {
                    break;
                }
                {
                    var s = plan9Arg(ref inst, i, pc, a, symname);

                    if (s != "")
                    {
                        args = append(args, s);
                    }
                }
            }            @string op = default;
            op = plan9OpMap[inst.Op];
            if (op == "")
            {
                op = strings.ToUpper(inst.Op.String());
            }

            if (inst.Op == STB || inst.Op == STBU || inst.Op == STBX || inst.Op == STBUX || inst.Op == STH || inst.Op == STHU || inst.Op == STHX || inst.Op == STHUX || inst.Op == STW || inst.Op == STWU || inst.Op == STWX || inst.Op == STWUX || inst.Op == STD || inst.Op == STDU || inst.Op == STDX || inst.Op == STDUX || inst.Op == STQ || inst.Op == STHBRX || inst.Op == STWBRX) 
                return op + " " + strings.Join(args, ", "); 
                // branch instructions needs additional handling
            else if (inst.Op == BCLR) 
                if (int(inst.Args[0L]._<Imm>()) & 20L == 20L)
                { // unconditional
                    return "RET";
                }
                return op + " " + strings.Join(args, ", ");
            else if (inst.Op == BC) 
                if (int(inst.Args[0L]._<Imm>()) & 0x1cUL == 12L)
                { // jump on cond bit set
                    return fmt.Sprintf("B%s %s", args[1L], args[2L]);
                }
                else if (int(inst.Args[0L]._<Imm>()) & 0x1cUL == 4L && revCondMap[args[1L]] != "")
                { // jump on cond bit not set
                    return fmt.Sprintf("B%s %s", revCondMap[args[1L]], args[2L]);
                }
                return op + " " + strings.Join(args, ", ");
            else if (inst.Op == BCCTR) 
                if (int(inst.Args[0L]._<Imm>()) & 20L == 20L)
                { // unconditional
                    return "BR (CTR)";
                }
                return op + " " + strings.Join(args, ", ");
            else if (inst.Op == BCCTRL) 
                if (int(inst.Args[0L]._<Imm>()) & 20L == 20L)
                { // unconditional
                    return "BL (CTR)";
                }
                return op + " " + strings.Join(args, ", ");
            else if (inst.Op == BCA || inst.Op == BCL || inst.Op == BCLA || inst.Op == BCLRL || inst.Op == BCTAR || inst.Op == BCTARL) 
                return op + " " + strings.Join(args, ", ");
            else // dst, sA, sB, ...
                if (len(args) == 0L)
                {
                    return op;
                }
                else if (len(args) == 1L)
                {
                    return fmt.Sprintf("%s %s", op, args[0L]);
                }
                args = append(args, args[0L]);
                return op + " " + strings.Join(args[1L..], ", "); 
                // store instructions always have the memory operand at the end, no need to reorder
                    }

        // plan9Arg formats arg (which is the argIndex's arg in inst) according to Plan 9 rules.
        // NOTE: because Plan9Syntax is the only caller of this func, and it receives a copy
        //       of inst, it's ok to modify inst.Args here.
        private static @string plan9Arg(ref Inst _inst, long argIndex, ulong pc, Arg arg, Func<ulong, (@string, ulong)> symname) => func(_inst, (ref Inst inst, Defer _, Panic panic, Recover __) =>
        { 
            // special cases for load/store instructions
            {
                Offset (_, ok) = arg._<Offset>();

                if (ok)
                {
                    if (argIndex + 1L == len(inst.Args) || inst.Args[argIndex + 1L] == null)
                    {
                        panic(fmt.Errorf("wrong table: offset not followed by register"));
                    }
                }

            }
            switch (arg.type())
            {
                case Reg arg:
                    if (isLoadStoreOp(inst.Op) && argIndex == 1L && arg == R0)
                    {
                        return "0";
                    }
                    if (arg == R30)
                    {
                        return "g";
                    }
                    return strings.ToUpper(arg.String());
                    break;
                case CondReg arg:
                    if (arg == CR0 && strings.HasPrefix(inst.Op.String(), "cmp"))
                    {
                        return ""; // don't show cr0 for cmp instructions
                    }
                    else if (arg >= CR0)
                    {
                        return fmt.Sprintf("CR%d", int(arg - CR0));
                    }
                    array<@string> bit = new array<@string>(new @string[] { "LT", "GT", "EQ", "SO" })[(arg - Cond0LT) % 4L];
                    if (arg <= Cond0SO)
                    {
                        return bit;
                    }
                    return fmt.Sprintf("4*CR%d+%s", int(arg - Cond0LT) / 4L, bit);
                    break;
                case Imm arg:
                    return fmt.Sprintf("$%d", arg);
                    break;
                case SpReg arg:
                    switch (arg)
                    {
                        case 8L: 
                            return "LR";
                            break;
                        case 9L: 
                            return "CTR";
                            break;
                    }
                    return fmt.Sprintf("SPR(%d)", int(arg));
                    break;
                case PCRel arg:
                    var addr = pc + uint64(int64(arg));
                    {
                        var (s, base) = symname(addr);

                        if (s != "" && base == addr)
                        {
                            return fmt.Sprintf("%s(SB)", s);
                        }

                    }
                    return fmt.Sprintf("%#x", addr);
                    break;
                case Label arg:
                    return fmt.Sprintf("%#x", int(arg));
                    break;
                case Offset arg:
                    Reg reg = inst.Args[argIndex + 1L]._<Reg>();
                    removeArg(inst, argIndex + 1L);
                    if (reg == R0)
                    {
                        return fmt.Sprintf("%d(0)", int(arg));
                    }
                    return fmt.Sprintf("%d(R%d)", int(arg), reg - R0);
                    break;
            }
            return fmt.Sprintf("???(%v)", arg);
        });

        // revCondMap maps a conditional register bit to its inverse, if possible.
        private static map revCondMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"LT":"GE","GT":"LE","EQ":"NE",};

        // plan9OpMap maps an Op to its Plan 9 mnemonics, if different than its GNU mnemonics.
        private static map plan9OpMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, @string>{LWARX:"LWAR",STWCX_:"STWCCC",LDARX:"LDAR",STDCX_:"STDCCC",LHARX:"LHAR",STHCX_:"STHCCC",LBARX:"LBAR",STBCX_:"STBCCC",ADDI:"ADD",ADD_:"ADDCC",LBZ:"MOVBZ",STB:"MOVB",LBZU:"MOVBZU",STBU:"MOVBU",LHZ:"MOVHZ",LHA:"MOVH",STH:"MOVH",LHZU:"MOVHZU",STHU:"MOVHU",LI:"MOVD",LIS:"ADDIS",LWZ:"MOVWZ",LWA:"MOVW",STW:"MOVW",LWZU:"MOVWZU",STWU:"MOVWU",LD:"MOVD",STD:"MOVD",LDU:"MOVDU",STDU:"MOVDU",MTSPR:"MOVD",MFSPR:"MOVD",B:"BR",BL:"CALL",CMPLD:"CMPU",CMPLW:"CMPWU",CMPD:"CMP",CMPW:"CMPW",};
    }
}}}}}}}
