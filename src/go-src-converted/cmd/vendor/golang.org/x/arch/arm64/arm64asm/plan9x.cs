// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2020 August 29 10:07:44 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\plan9x.go
using fmt = go.fmt_package;
using io = go.io_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm64
{
    public static partial class arm64asm_package
    {
        // GoSyntax returns the Go assembler syntax for the instruction.
        // The syntax was originally defined by Plan 9.
        // The pc is the program counter of the instruction, used for
        // expanding PC-relative addresses into absolute ones.
        // The symname function queries the symbol table for the program
        // being disassembled. Given a target address it returns the name
        // and base address of the symbol containing the target, if any;
        // otherwise it returns "", 0.
        // The reader text should read from the text segment using text addresses
        // as offsets; it is used to display pc-relative loads as constant loads.
        public static @string GoSyntax(Inst inst, ulong pc, Func<ulong, (@string, ulong)> symname, io.ReaderAt text)
        {
            if (symname == null)
            {
                symname = _p0 => ("", 0L);
            }
            slice<@string> args = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in inst.Args)
                {
                    a = __a;
                    if (a == null)
                    {
                        break;
                    }
                    args = append(args, plan9Arg(ref inst, pc, symname, a));
                }
                a = a__prev1;
            }

            var op = inst.Op.String();


            if (inst.Op == LDR || inst.Op == LDRB || inst.Op == LDRH || inst.Op == LDRSB || inst.Op == LDRSH || inst.Op == LDRSW) 
                // Check for PC-relative load.
                {
                    PCRel (offset, ok) = inst.Args[1L]._<PCRel>();

                    if (ok)
                    {
                        var addr = pc + uint64(offset);
                        {
                            Reg (_, ok) = inst.Args[0L]._<Reg>();

                            if (!ok)
                            {
                                break;
                            }
                        }
                        {
                            var (s, base) = symname(addr);

                            if (s != "" && addr == base)
                            {
                                args[1L] = fmt.Sprintf("$%s(SB)", s);
                            }
                        }
                    }
                }
            // Move addressing mode into opcode suffix.
            @string suffix = "";

            if (inst.Op == LDR || inst.Op == LDRB || inst.Op == LDRH || inst.Op == LDRSB || inst.Op == LDRSH || inst.Op == LDRSW || inst.Op == STR || inst.Op == STRB || inst.Op == STRH || inst.Op == STUR || inst.Op == STURB || inst.Op == STURH || inst.Op == LD1) 
                switch (inst.Args[1L].type())
                {
                    case MemImmediate mem:

                        if (mem.Mode == AddrOffset)                         else if (mem.Mode == AddrPreIndex) 
                            suffix = ".W";
                        else if (mem.Mode == AddrPostIndex || mem.Mode == AddrPostReg) 
                            suffix = ".P";
                                                break;

                }
            else if (inst.Op == STP || inst.Op == LDP) 
                switch (inst.Args[2L].type())
                {
                    case MemImmediate mem:

                        if (mem.Mode == AddrOffset)                         else if (mem.Mode == AddrPreIndex) 
                            suffix = ".W";
                        else if (mem.Mode == AddrPostIndex) 
                            suffix = ".P";
                                                break;
                }
            
            if (inst.Op == BL)
            {
                return "CALL " + args[0L];
                goto __switch_break0;
            }
            if (inst.Op == BLR)
            {
                Reg r = inst.Args[0L]._<Reg>();
                var regno = uint16(r) & 31L;
                return fmt.Sprintf("CALL (R%d)", regno);
                goto __switch_break0;
            }
            if (inst.Op == RET)
            {
                {
                    Reg r__prev1 = r;

                    Reg (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok && r == X30)
                    {
                        return "RET";
                    }
                    r = r__prev1;

                }
                goto __switch_break0;
            }
            if (inst.Op == B)
            {
                {
                    Cond (cond, ok) = inst.Args[0L]._<Cond>();

                    if (ok)
                    {
                        return "B" + cond.String() + " " + args[1L];
                    }
                }
                return "JMP" + " " + args[0L];
                goto __switch_break0;
            }
            if (inst.Op == BR)
            {
                r = inst.Args[0L]._<Reg>();
                regno = uint16(r) & 31L;
                return fmt.Sprintf("JMP (R%d)", regno);
                goto __switch_break0;
            }
            if (inst.Op == MOV)
            {
                long rno = -1L;
                switch (inst.Args[0L].type())
                {
                    case Reg a:
                        rno = int(a);
                        break;
                    case RegSP a:
                        rno = int(a);
                        break;
                    case RegisterWithArrangementAndIndex a:
                        op = "VMOV";
                        break;
                }
                if (rno >= 0L && rno <= int(WZR))
                {
                    op = "MOVW";
                }
                else if (rno >= int(X0) && rno <= int(XZR))
                {
                    op = "MOVD";
                }
                {
                    (_, ok) = inst.Args[1L]._<RegisterWithArrangementAndIndex>();

                    if (ok)
                    {
                        op = "VMOV";
                    }
                }
                goto __switch_break0;
            }
            if (inst.Op == LDR)
            {
                rno = default;
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                    }
                    else
                    {
                        rno = uint16(inst.Args[0L]._<RegSP>());
                    }
                    r = r__prev1;

                }
                if (rno <= uint16(WZR))
                {
                    op = "MOVWU" + suffix;
                }
                else
                {
                    op = "MOVD" + suffix;
                }
                goto __switch_break0;
            }
            if (inst.Op == LDRB)
            {
                op = "MOVBU" + suffix;
                goto __switch_break0;
            }
            if (inst.Op == LDRH)
            {
                op = "MOVHU" + suffix;
                goto __switch_break0;
            }
            if (inst.Op == LDRSW)
            {
                op = "MOVW" + suffix;
                goto __switch_break0;
            }
            if (inst.Op == LDRSB)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op = "MOVBW" + suffix;
                        }
                        else
                        {
                            op = "MOVB" + suffix;
                        }
                    }
                    r = r__prev1;

                }
                goto __switch_break0;
            }
            if (inst.Op == LDRSH)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op = "MOVHW" + suffix;
                        }
                        else
                        {
                            op = "MOVH" + suffix;
                        }
                    }
                    r = r__prev1;

                }
                goto __switch_break0;
            }
            if (inst.Op == STR || inst.Op == STUR)
            {
                rno = default;
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                    }
                    else
                    {
                        rno = uint16(inst.Args[0L]._<RegSP>());
                    }
                    r = r__prev1;

                }
                if (rno <= uint16(WZR))
                {
                    op = "MOVW" + suffix;
                }
                else
                {
                    op = "MOVD" + suffix;
                }
                args[0L] = args[1L];
                args[1L] = args[0L];
                goto __switch_break0;
            }
            if (inst.Op == STRB || inst.Op == STURB)
            {
                op = "MOVB" + suffix;
                args[0L] = args[1L];
                args[1L] = args[0L];
                goto __switch_break0;
            }
            if (inst.Op == STRH || inst.Op == STURH)
            {
                op = "MOVH" + suffix;
                args[0L] = args[1L];
                args[1L] = args[0L];
                goto __switch_break0;
            }
            if (inst.Op == TBNZ || inst.Op == TBZ)
            {
                args[0L] = args[2L];
                args[1L] = args[0L];
                args[2L] = args[1L];
                goto __switch_break0;
            }
            if (inst.Op == MADD || inst.Op == MSUB || inst.Op == SMADDL || inst.Op == SMSUBL || inst.Op == UMADDL || inst.Op == UMSUBL)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op += "W";
                        }
                    }
                    r = r__prev1;

                }
                args[2L] = args[3L];
                args[3L] = args[2L];
                goto __switch_break0;
            }
            if (inst.Op == STLR)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op += "W";
                        }
                    }
                    r = r__prev1;

                }
                args[0L] = args[1L];
                args[1L] = args[0L];
                goto __switch_break0;
            }
            if (inst.Op == STLRB || inst.Op == STLRH)
            {
                args[0L] = args[1L];
                args[1L] = args[0L];
                goto __switch_break0;
            }
            if (inst.Op == STLXR || inst.Op == STXR)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[1L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op += "W";
                        }
                    }
                    r = r__prev1;

                }
                args[1L] = args[2L];
                args[2L] = args[1L];
                goto __switch_break0;
            }
            if (inst.Op == STLXRB || inst.Op == STLXRH || inst.Op == STXRB || inst.Op == STXRH)
            {
                args[1L] = args[2L];
                args[2L] = args[1L];
                goto __switch_break0;
            }
            if (inst.Op == BFI || inst.Op == BFXIL || inst.Op == SBFIZ || inst.Op == SBFX || inst.Op == UBFIZ || inst.Op == UBFX)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op += "W";
                        }
                    }
                    r = r__prev1;

                }
                args[1L] = args[3L];
                args[2L] = args[1L];
                args[3L] = args[2L];
                goto __switch_break0;
            }
            if (inst.Op == STP || inst.Op == LDP)
            {
                args[0L] = fmt.Sprintf("(%s, %s)", args[0L], args[1L]);
                args[1L] = args[2L];
                if (op == "STP")
                {
                    op = op + suffix;
                    return op + " " + args[0L] + ", " + args[1L];
                }
                else if (op == "LDP")
                {
                    op = op + suffix;
                    return op + " " + args[1L] + ", " + args[0L];
                }
                goto __switch_break0;
            }
            if (inst.Op == FCCMP || inst.Op == FCCMPE)
            {
                args[0L] = args[1L];
                args[1L] = args[0L];
                fallthrough = true;

            }
            if (fallthrough || inst.Op == FCMP || inst.Op == FCMPE)
            {
                {
                    (_, ok) = inst.Args[1L]._<Imm>();

                    if (ok)
                    {
                        args[1L] = "$(0.0)";
                    }
                }
                fallthrough = true;

            }
            if (fallthrough || inst.Op == FADD || inst.Op == FSUB || inst.Op == FMUL || inst.Op == FNMUL || inst.Op == FDIV || inst.Op == FMAX || inst.Op == FMIN || inst.Op == FMAXNM || inst.Op == FMINNM || inst.Op == FCSEL)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno >= uint16(S0) && rno <= uint16(S31))
                        {
                            op = fmt.Sprintf("%sS", op);
                        }
                        else if (rno >= uint16(D0) && rno <= uint16(D31))
                        {
                            op = fmt.Sprintf("%sD", op);
                        }
                    }
                    r = r__prev1;

                }
                goto __switch_break0;
            }
            if (inst.Op == FCVT)
            {
                {
                    long i__prev1 = i;

                    for (long i = 1L; i >= 0L; i--)
                    {
                        {
                            Reg r__prev1 = r;

                            (r, ok) = inst.Args[i]._<Reg>();

                            if (ok)
                            {
                                rno = uint16(r);
                                if (rno >= uint16(H0) && rno <= uint16(H31))
                                {
                                    op = fmt.Sprintf("%sH", op);
                                }
                                else if (rno >= uint16(S0) && rno <= uint16(S31))
                                {
                                    op = fmt.Sprintf("%sS", op);
                                }
                                else if (rno >= uint16(D0) && rno <= uint16(D31))
                                {
                                    op = fmt.Sprintf("%sD", op);
                                }
                            }
                            r = r__prev1;

                        }
                    }

                    i = i__prev1;
                }
                goto __switch_break0;
            }
            if (inst.Op == FABS || inst.Op == FNEG || inst.Op == FSQRT || inst.Op == FRINTN || inst.Op == FRINTP || inst.Op == FRINTM || inst.Op == FRINTZ || inst.Op == FRINTA || inst.Op == FRINTX || inst.Op == FRINTI)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[1L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno >= uint16(S0) && rno <= uint16(S31))
                        {
                            op = fmt.Sprintf("%sS", op);
                        }
                        else if (rno >= uint16(D0) && rno <= uint16(D31))
                        {
                            op = fmt.Sprintf("%sD", op);
                        }
                    }
                    r = r__prev1;

                }
                goto __switch_break0;
            }
            if (inst.Op == FCVTZS || inst.Op == FCVTZU || inst.Op == SCVTF || inst.Op == UCVTF)
            {
                {
                    (_, ok) = inst.Args[2L]._<Imm>();

                    if (!ok)
                    {
                        {
                            long i__prev1 = i;

                            for (i = 1L; i >= 0L; i--)
                            {
                                {
                                    Reg r__prev2 = r;

                                    (r, ok) = inst.Args[i]._<Reg>();

                                    if (ok)
                                    {
                                        rno = uint16(r);
                                        if (rno >= uint16(S0) && rno <= uint16(S31))
                                        {
                                            op = fmt.Sprintf("%sS", op);
                                        }
                                        else if (rno >= uint16(D0) && rno <= uint16(D31))
                                        {
                                            op = fmt.Sprintf("%sD", op);
                                        }
                                        else if (rno <= uint16(WZR))
                                        {
                                            op += "W";
                                        }
                                    }
                                    r = r__prev2;

                                }
                            }

                            i = i__prev1;
                        }
                    }
                }
                goto __switch_break0;
            }
            if (inst.Op == FMOV)
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i <= 1L; i++)
                    {
                        {
                            Reg r__prev1 = r;

                            (r, ok) = inst.Args[i]._<Reg>();

                            if (ok)
                            {
                                rno = uint16(r);
                                if (rno >= uint16(S0) && rno <= uint16(S31))
                                {
                                    op = fmt.Sprintf("%sS", op);
                                    break;
                                }
                                else if (rno >= uint16(D0) && rno <= uint16(D31))
                                {
                                    op = fmt.Sprintf("%sD", op);
                                    break;
                                }
                            }
                            r = r__prev1;

                        }
                    }

                    i = i__prev1;
                }
                goto __switch_break0;
            }
            if (inst.Op == SYSL)
            {
                var op1 = int(inst.Args[1L]._<Imm>().Imm);
                var cn = int(inst.Args[2L]._<Imm_c>());
                var cm = int(inst.Args[3L]._<Imm_c>());
                var op2 = int(inst.Args[4L]._<Imm>().Imm);
                var sysregno = int32(op1 << (int)(16L) | cn << (int)(12L) | cm << (int)(8L) | op2 << (int)(5L));
                args[1L] = fmt.Sprintf("$%d", sysregno);
                return op + " " + args[1L] + ", " + args[0L];
                goto __switch_break0;
            }
            if (inst.Op == CBNZ || inst.Op == CBZ)
            {
                {
                    Reg r__prev1 = r;

                    (r, ok) = inst.Args[0L]._<Reg>();

                    if (ok)
                    {
                        rno = uint16(r);
                        if (rno <= uint16(WZR))
                        {
                            op += "W";
                        }
                    }
                    r = r__prev1;

                }
                args[0L] = args[1L];
                args[1L] = args[0L];
                goto __switch_break0;
            }
            if (inst.Op == ADR || inst.Op == ADRP)
            {
                addr = int64(inst.Args[1L]._<PCRel>());
                args[1L] = fmt.Sprintf("%d(PC)", addr);
                goto __switch_break0;
            }
            // default: 
                var index = sort.SearchStrings(noSuffixOpSet, op);
                if (!(index < len(noSuffixOpSet) && noSuffixOpSet[index] == op))
                {
                    rno = -1L;
                    switch (inst.Args[0L].type())
                    {
                        case Reg a:
                            rno = int(a);
                            break;
                        case RegSP a:
                            rno = int(a);
                            break;
                        case RegisterWithArrangement a:
                            op = fmt.Sprintf("V%s", op);
                            break;

                    }

                    if (rno >= 0L && rno <= int(WZR))
                    { 
                        // Add "w" to opcode suffix.
                        op += "W";
                    }
                }
                op = op + suffix;

            __switch_break0:; 

            // conditional instructions, replace args.
            {
                (_, ok) = inst.Args[3L]._<Cond>();

                if (ok)
                {
                    {
                        (_, ok) = inst.Args[2L]._<Reg>();

                        if (ok)
                        {
                            args[1L] = args[2L];
                            args[2L] = args[1L];
                        }
                        else
                        {
                            args[0L] = args[2L];
                            args[2L] = args[0L];
                        }
                    }
                }
            } 
            // Reverse args, placing dest last.
            {
                long i__prev1 = i;

                i = 0L;
                var j = len(args) - 1L;

                while (i < j)
                {
                    args[i] = args[j];
                    args[j] = args[i];
                    i = i + 1L;
                j = j - 1L;
                }

                i = i__prev1;
            }

            if (args != null)
            {
                op += " " + strings.Join(args, ", ");
            }
            return op;
        }

        // No need add "W" to opcode suffix.
        // Opcode must be inserted in ascending order.
        private static var noSuffixOpSet = strings.Fields("\nCRC32B\nCRC32CB\nCRC32CH\nCRC32CW\nCRC32CX\nCRC32H\nCRC32W\nCRC32X\nLDARB\nLDARH\nLDAXRB\nL" +
    "DAXRH\nLDXRB\nLDXRH\n");

        private static @string plan9Arg(ref Inst inst, ulong pc, Func<ulong, (@string, ulong)> symname, Arg arg)
        {
            switch (arg.type())
            {
                case Imm a:
                    return fmt.Sprintf("$%d", uint32(a.Imm));
                    break;
                case Imm64 a:
                    return fmt.Sprintf("$%d", int64(a.Imm));
                    break;
                case ImmShift a:
                    if (a.shift == 0L)
                    {
                        return fmt.Sprintf("$%d", a.imm);
                    }
                    return fmt.Sprintf("$(%d<<%d)", a.imm, a.shift);
                    break;
                case PCRel a:
                    var addr = int64(pc) + int64(a);
                    {
                        var s__prev1 = s;
                        @string base__prev1 = base;

                        var (s, base) = symname(uint64(addr));

                        if (s != "" && uint64(addr) == base)
                        {
                            return fmt.Sprintf("%s(SB)", s);
                        }

                        s = s__prev1;
                        base = base__prev1;

                    }
                    return fmt.Sprintf("%d(PC)", a / 4L);
                    break;
                case Reg a:
                    var regenum = uint16(a);
                    var regno = uint16(a) & 31L;

                    if (regenum >= uint16(B0) && regenum <= uint16(D31))
                    { 
                        // FP registers are the same ones as SIMD registers
                        // Print Fn for scalar variant to align with assembler (e.g., FCVT)
                        return fmt.Sprintf("F%d", regno);
                    }
                    else if (regenum >= uint16(Q0) && regenum <= uint16(Q31))
                    { 
                        // Print Vn to align with assembler (e.g., SHA256H)
                        return fmt.Sprintf("V%d", regno);
                    }
                    if (regno == 31L)
                    {
                        return "ZR";
                    }
                    return fmt.Sprintf("R%d", regno);
                    break;
                case RegSP a:
                    regno = uint16(a) & 31L;
                    if (regno == 31L)
                    {
                        return "RSP";
                    }
                    return fmt.Sprintf("R%d", regno);
                    break;
                case RegExtshiftAmount a:
                    @string reg = "";
                    regno = uint16(a.reg) & 31L;
                    if (regno == 31L)
                    {
                        reg = "ZR";
                    }
                    else
                    {
                        reg = fmt.Sprintf("R%d", uint16(a.reg) & 31L);
                    }
                    @string extshift = "";
                    @string amount = "";
                    if (a.extShift != ExtShift(0L))
                    {

                        if (a.extShift == lsl) 
                            extshift = "<<";
                            amount = fmt.Sprintf("%d", a.amount);
                            return reg + extshift + amount;
                        else if (a.extShift == lsr) 
                            extshift = ">>";
                            amount = fmt.Sprintf("%d", a.amount);
                            return reg + extshift + amount;
                        else if (a.extShift == asr) 
                            extshift = "->";
                            amount = fmt.Sprintf("%d", a.amount);
                            return reg + extshift + amount;
                        else if (a.extShift == ror) 
                            extshift = "@>";
                            amount = fmt.Sprintf("%d", a.amount);
                            return reg + extshift + amount;
                        else 
                            extshift = "." + a.extShift.String();
                                                if (a.amount != 0L)
                        {
                            amount = fmt.Sprintf("<<%d", a.amount);
                        }
                    }
                    return reg + extshift + amount;
                    break;
                case MemImmediate a:
                    @string off = "";
                    @string @base = "";
                    regno = uint16(a.Base) & 31L;
                    if (regno == 31L)
                    {
                        base = "(RSP)";
                    }
                    else
                    {
                        base = fmt.Sprintf("(R%d)", regno);
                    }
                    if (a.imm != 0L && a.Mode != AddrPostReg)
                    {
                        off = fmt.Sprintf("%d", a.imm);
                    }
                    else if (a.Mode == AddrPostReg)
                    {
                        var postR = fmt.Sprintf("(R%d)", a.imm);
                        return base + postR;
                    }
                    return off + base;
                    break;
                case MemExtend a:
                    @base = "";
                    @string index = "";
                    @string extend = "";
                    @string indexreg = "";
                    regno = uint16(a.Base) & 31L;
                    if (regno == 31L)
                    {
                        base = "(RSP)";
                    }
                    else
                    {
                        base = fmt.Sprintf("(R%d)", regno);
                    }
                    regno = uint16(a.Index) & 31L;
                    if (regno == 31L)
                    {
                        indexreg = "ZR";
                    }
                    else
                    {
                        indexreg = fmt.Sprintf("R%d", regno);
                    }
                    if (a.Extend == lsl)
                    {
                        if (a.Amount != 0L)
                        {
                            extend = fmt.Sprintf("<<%d", a.Amount);
                        }
                    }
                    else
                    {
                        extend = "unimplemented!";
                    }
                    index = indexreg + extend;
                    return index + base;
                    break;
                case Cond a:
                    switch (arg.String())
                    {
                        case "CS": 
                            return "HS";
                            break;
                        case "CC": 
                            return "LO";
                            break;
                    }
                    break;
                case Imm_clrex a:
                    return fmt.Sprintf("$%d", uint32(a));
                    break;
                case Imm_dcps a:
                    return fmt.Sprintf("$%d", uint32(a));
                    break;
                case Imm_option a:
                    return fmt.Sprintf("$%d", uint8(a));
                    break;
                case Imm_hint a:
                    return fmt.Sprintf("$%d", uint8(a));
                    break;
                case Imm_fp a:
                    short s = default;                    short pre = default;                    short numerator = default;                    short denominator = default;

                    double result = default;
                    if (a.s == 0L)
                    {
                        s = 1L;
                    }
                    else
                    {
                        s = -1L;
                    }
                    pre = s * int16(16L + a.pre);
                    if (a.exp > 0L)
                    {
                        numerator = (pre << (int)(uint8(a.exp)));
                        denominator = 16L;
                    }
                    else
                    {
                        numerator = pre;
                        denominator = (16L << (int)(uint8(-1L * a.exp)));
                    }
                    result = float64(numerator) / float64(denominator);
                    return strings.TrimRight(fmt.Sprintf("$%f", result), "0");
                    break;
                case RegisterWithArrangement a:
                    result = a.r.String();
                    var arrange = a.a.String();
                    slice<int> c = (slice<int>)arrange;
                    switch (len(c))
                    {
                        case 3L: 
                            c[1L] = c[2L];
                            c[2L] = c[1L]; // .8B -> .B8
                            break;
                        case 4L: 
                            c[1L] = c[3L];
                            c[2L] = c[1L];
                            c[3L] = c[2L]; // 16B -> B16
                            break;
                    }
                    arrange = string(c);
                    result += arrange;
                    if (a.cnt > 0L)
                    {
                        result = "[" + result;
                        {
                            long i__prev1 = i;

                            for (long i = 1L; i < int(a.cnt); i++)
                            {
                                var cur = V0 + Reg((uint16(a.r) - uint16(V0) + uint16(i)) & 31L);
                                result += ", " + cur.String() + arrange;
                            }


                            i = i__prev1;
                        }
                        result += "]";
                    }
                    return result;
                    break;
                case RegisterWithArrangementAndIndex a:
                    result = a.r.String();
                    arrange = a.a.String();
                    result += arrange;
                    if (a.cnt > 0L)
                    {
                        result = "[" + result;
                        {
                            long i__prev1 = i;

                            for (i = 1L; i < int(a.cnt); i++)
                            {
                                cur = V0 + Reg((uint16(a.r) - uint16(V0) + uint16(i)) & 31L);
                                result += ", " + cur.String() + arrange;
                            }


                            i = i__prev1;
                        }
                        result += "]";
                    }
                    return fmt.Sprintf("%s[%d]", result, a.index);
                    break;
                case Systemreg a:
                    return fmt.Sprintf("$%d", uint32(a.op0 & 1L) << (int)(14L) | uint32(a.op1 & 7L) << (int)(11L) | uint32(a.cn & 15L) << (int)(7L) | uint32(a.cm & 15L) << (int)(3L) | uint32(a.op2) & 7L);
                    break;

            }

            return strings.ToUpper(arg.String());
        }
    }
}}}}}}}
