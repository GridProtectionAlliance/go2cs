// Copyright 2015 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2022 March 13 06:38:21 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\plan9.go
namespace go.cmd.vendor.golang.org.x.arch.ppc64;

using fmt = fmt_package;
using strings = strings_package;


// GoSyntax returns the Go assembler syntax for the instruction.
// The pc is the program counter of the first instruction, used for expanding
// PC-relative addresses into absolute ones.
// The symname function queries the symbol table for the program
// being disassembled. It returns the name and base address of the symbol
// containing the target, if any; otherwise it returns "", 0.

using System;
public static partial class ppc64asm_package {

public static @string GoSyntax(Inst inst, ulong pc, Func<ulong, (@string, ulong)> symname) {
    if (symname == null) {
        symname = _p0 => ("", 0);
    }
    if (inst.Op == 0 && inst.Enc == 0) {
        return "WORD $0";
    }
    else if (inst.Op == 0) {
        return "?";
    }
    slice<@string> args = default;
    foreach (var (i, a) in inst.Args[..]) {
        if (a == null) {
            break;
        }
        {
            var s = plan9Arg(_addr_inst, i, pc, a, symname);

            if (s != "") { 
                // In the case for some BC instructions, a CondReg arg has
                // both the CR and the branch condition encoded in its value.
                // plan9Arg will return a string with the string representation
                // of these values separated by a blank that will be treated
                // as 2 args from this point on.
                if (strings.IndexByte(s, ' ') > 0) {
                    var t = strings.Split(s, " ");
                    args = append(args, t[0]);
                    args = append(args, t[1]);
                }
                else
 {
                    args = append(args, s);
                }
            }
        }
    }    @string op = default;
    op = plan9OpMap[inst.Op];
    if (op == "") {
        op = strings.ToUpper(inst.Op.String());
        if (op[len(op) - 1] == '.') {
            op = op[..(int)len(op) - 1] + "CC";
        }
    }

    if (inst.Op == PASTECC) 
        // paste. has two input registers, and an L field, unlike other 3 operand instructions.
        return op + " " + args[0] + "," + args[1] + "," + args[2];
    else if (inst.Op == SYNC) 
        if (args[0] == "$1") {
            return "LWSYNC";
        }
        return "HWSYNC";
    else if (inst.Op == ISEL) 
        return "ISEL " + args[3] + "," + args[1] + "," + args[2] + "," + args[0]; 

        // store instructions always have the memory operand at the end, no need to reorder
        // indexed stores handled separately
    else if (inst.Op == STB || inst.Op == STBU || inst.Op == STH || inst.Op == STHU || inst.Op == STW || inst.Op == STWU || inst.Op == STD || inst.Op == STDU || inst.Op == STQ || inst.Op == STFD || inst.Op == STFDU || inst.Op == STFS || inst.Op == STFSU) 
        return op + " " + strings.Join(args, ",");
    else if (inst.Op == CMPD || inst.Op == CMPDI || inst.Op == CMPLD || inst.Op == CMPLDI || inst.Op == CMPW || inst.Op == CMPWI || inst.Op == CMPLW || inst.Op == CMPLWI) 
        if (len(args) == 2) {
            return op + " " + args[0] + "," + args[1];
        }
        else if (len(args) == 3) {
            return op + " " + args[0] + "," + args[1] + "," + args[2];
        }
        return op + " " + args[0] + " ??";
    else if (inst.Op == LIS) 
        return "ADDIS $0," + args[1] + "," + args[0]; 
        // store instructions with index registers
    else if (inst.Op == STBX || inst.Op == STBUX || inst.Op == STHX || inst.Op == STHUX || inst.Op == STWX || inst.Op == STWUX || inst.Op == STDX || inst.Op == STDUX || inst.Op == STHBRX || inst.Op == STWBRX || inst.Op == STDBRX || inst.Op == STSWX || inst.Op == STFIWX) 
        return "MOV" + op[(int)2..(int)len(op) - 1] + " " + args[0] + ",(" + args[2] + ")(" + args[1] + ")";
    else if (inst.Op == STDCXCC || inst.Op == STWCXCC || inst.Op == STHCXCC || inst.Op == STBCXCC) 
        return op + " " + args[0] + ",(" + args[2] + ")(" + args[1] + ")";
    else if (inst.Op == STXVX || inst.Op == STXVD2X || inst.Op == STXVW4X || inst.Op == STXVH8X || inst.Op == STXVB16X || inst.Op == STXSDX || inst.Op == STVX || inst.Op == STVXL || inst.Op == STVEBX || inst.Op == STVEHX || inst.Op == STVEWX || inst.Op == STXSIWX || inst.Op == STFDX || inst.Op == STFDUX || inst.Op == STFDPX || inst.Op == STFSX || inst.Op == STFSUX) 
        return op + " " + args[0] + ",(" + args[2] + ")(" + args[1] + ")";
    else if (inst.Op == STXV) 
        return op + " " + args[0] + "," + args[1];
    else if (inst.Op == STXVL || inst.Op == STXVLL) 
        return op + " " + args[0] + "," + args[1] + "," + args[2];
    else if (inst.Op == LWAX || inst.Op == LWAUX || inst.Op == LWZX || inst.Op == LHZX || inst.Op == LBZX || inst.Op == LDX || inst.Op == LHAX || inst.Op == LHAUX || inst.Op == LDARX || inst.Op == LWARX || inst.Op == LHARX || inst.Op == LBARX || inst.Op == LFDX || inst.Op == LFDUX || inst.Op == LFSX || inst.Op == LFSUX || inst.Op == LDBRX || inst.Op == LWBRX || inst.Op == LHBRX || inst.Op == LDUX || inst.Op == LWZUX || inst.Op == LHZUX || inst.Op == LBZUX) 
        if (args[1] == "0") {
            return op + " (" + args[2] + ")," + args[0];
        }
        return op + " (" + args[2] + ")(" + args[1] + ")," + args[0];
    else if (inst.Op == LXVX || inst.Op == LXVD2X || inst.Op == LXVW4X || inst.Op == LXVH8X || inst.Op == LXVB16X || inst.Op == LVX || inst.Op == LVXL || inst.Op == LVSR || inst.Op == LVSL || inst.Op == LVEBX || inst.Op == LVEHX || inst.Op == LVEWX || inst.Op == LXSDX || inst.Op == LXSIWAX) 
        return op + " (" + args[2] + ")(" + args[1] + ")," + args[0];
    else if (inst.Op == LXV) 
        return op + " " + args[1] + "," + args[0];
    else if (inst.Op == LXVL || inst.Op == LXVLL) 
        return op + " " + args[1] + "," + args[2] + "," + args[0];
    else if (inst.Op == DCBT || inst.Op == DCBTST || inst.Op == DCBZ || inst.Op == DCBST || inst.Op == ICBI) 
        if (args[0] == "0" || args[0] == "R0") {
            return op + " (" + args[1] + ")";
        }
        return op + " (" + args[1] + ")(" + args[0] + ")"; 

        // branch instructions needs additional handling
    else if (inst.Op == BCLR) 
        if (int(inst.Args[0]._<Imm>()) & 20 == 20) { // unconditional
            return "RET";
        }
        return op + " " + strings.Join(args, ", ");
    else if (inst.Op == BC) 
        if (int(inst.Args[0]._<Imm>()) & 0x1c == 12) { // jump on cond bit set
            if (len(args) == 4) {
                return fmt.Sprintf("B%s %s,%s", args[1], args[2], args[3]);
            }
            return fmt.Sprintf("B%s %s", args[1], args[2]);
        }
        else if (int(inst.Args[0]._<Imm>()) & 0x1c == 4 && revCondMap[args[1]] != "") { // jump on cond bit not set
            if (len(args) == 4) {
                return fmt.Sprintf("B%s %s,%s", revCondMap[args[1]], args[2], args[3]);
            }
            return fmt.Sprintf("B%s %s", revCondMap[args[1]], args[2]);
        }
        return op + " " + strings.Join(args, ",");
    else if (inst.Op == BCCTR) 
        if (int(inst.Args[0]._<Imm>()) & 20 == 20) { // unconditional
            return "BR (CTR)";
        }
        return op + " " + strings.Join(args, ", ");
    else if (inst.Op == BCCTRL) 
        if (int(inst.Args[0]._<Imm>()) & 20 == 20) { // unconditional
            return "BL (CTR)";
        }
        return op + " " + strings.Join(args, ",");
    else if (inst.Op == BCA || inst.Op == BCL || inst.Op == BCLA || inst.Op == BCLRL || inst.Op == BCTAR || inst.Op == BCTARL) 
        return op + " " + strings.Join(args, ",");
    else // dst, sA, sB, ...
        switch (len(args)) {
            case 0: 
                return op;
                break;
            case 1: 
                return fmt.Sprintf("%s %s", op, args[0]);
                break;
            case 2: 
                if (inst.Op == COPY || inst.Op == PASTECC || inst.Op == FCMPO || inst.Op == FCMPU) {
                    return op + " " + args[0] + "," + args[1];
                }
                return op + " " + args[1] + "," + args[0];
                break;
            case 3: 
                if (reverseOperandOrder(inst.Op)) {
                    return op + " " + args[2] + "," + args[1] + "," + args[0];
                }
                break;
            case 4: 
                if (reverseMiddleOps(inst.Op)) {
                    return op + " " + args[1] + "," + args[3] + "," + args[2] + "," + args[0];
                }
                break;
        }
        args = append(args, args[0]);
        return op + " " + strings.Join(args[(int)1..], ",");
    }

// plan9Arg formats arg (which is the argIndex's arg in inst) according to Plan 9 rules.
// NOTE: because Plan9Syntax is the only caller of this func, and it receives a copy
//       of inst, it's ok to modify inst.Args here.
private static @string plan9Arg(ptr<Inst> _addr_inst, nint argIndex, ulong pc, Arg arg, Func<ulong, (@string, ulong)> symname) => func((_, panic, _) => {
    ref Inst inst = ref _addr_inst.val;
 
    // special cases for load/store instructions
    {
        Offset (_, ok) = arg._<Offset>();

        if (ok) {
            if (argIndex + 1 == len(inst.Args) || inst.Args[argIndex + 1] == null) {
                panic(fmt.Errorf("wrong table: offset not followed by register"));
            }
        }
    }
    switch (arg.type()) {
        case Reg arg:
            if (isLoadStoreOp(inst.Op) && argIndex == 1 && arg == R0) {
                return "0";
            }
            if (arg == R30) {
                return "g";
            }
            return strings.ToUpper(arg.String());
            break;
        case CondReg arg:
            if (inst.Op == ISEL) {
                return fmt.Sprintf("$%d", (arg - Cond0LT));
            }
            if (arg == CR0 && (strings.HasPrefix(inst.Op.String(), "cmp") || strings.HasPrefix(inst.Op.String(), "fcmp"))) {
                return ""; // don't show cr0 for cmp instructions
            }
            else if (arg >= CR0) {
                return fmt.Sprintf("CR%d", int(arg - CR0));
            }
            array<@string> bit = new array<@string>(new @string[] { "LT", "GT", "EQ", "SO" })[(arg - Cond0LT) % 4];
            if (strings.HasPrefix(inst.Op.String(), "cr")) {
                return fmt.Sprintf("CR%d%s", int(arg - Cond0LT) / 4, bit);
            }
            if (arg <= Cond0SO) {
                return bit;
            }
            return fmt.Sprintf("%s CR%d", bit, int(arg - Cond0LT) / 4);
            break;
        case Imm arg:
            return fmt.Sprintf("$%d", arg);
            break;
        case SpReg arg:
            switch (arg) {
                case 8: 
                    return "LR";
                    break;
                case 9: 
                    return "CTR";
                    break;
            }
            return fmt.Sprintf("SPR(%d)", int(arg));
            break;
        case PCRel arg:
            var addr = pc + uint64(int64(arg));
            {
                var (s, base) = symname(addr);

                if (s != "" && base == addr) {
                    return fmt.Sprintf("%s(SB)", s);
                }

            }
            return fmt.Sprintf("%#x", addr);
            break;
        case Label arg:
            return fmt.Sprintf("%#x", int(arg));
            break;
        case Offset arg:
            Reg reg = inst.Args[argIndex + 1]._<Reg>();
            removeArg(inst, argIndex + 1);
            if (reg == R0) {
                return fmt.Sprintf("%d(0)", int(arg));
            }
            return fmt.Sprintf("%d(R%d)", int(arg), reg - R0);
            break;
    }
    return fmt.Sprintf("???(%v)", arg);
});

private static bool reverseMiddleOps(Op op) {

    if (op == FMADD || op == FMADDCC || op == FMADDS || op == FMADDSCC || op == FMSUB || op == FMSUBCC || op == FMSUBS || op == FMSUBSCC || op == FNMADD || op == FNMADDCC || op == FNMADDS || op == FNMADDSCC || op == FNMSUB || op == FNMSUBCC || op == FNMSUBS || op == FNMSUBSCC || op == FSEL || op == FSELCC) 
        return true;
        return false;
}

private static bool reverseOperandOrder(Op op) {

    // Special case for SUBF, SUBFC: not reversed
    if (op == ADD || op == ADDC || op == ADDE || op == ADDCC || op == ADDCCC) 
        return true;
    else if (op == MULLW || op == MULLWCC || op == MULHW || op == MULHWCC || op == MULLD || op == MULLDCC || op == MULHD || op == MULHDCC || op == MULLWO || op == MULLWOCC || op == MULHWU || op == MULHWUCC || op == MULLDO || op == MULLDOCC) 
        return true;
    else if (op == DIVD || op == DIVDCC || op == DIVDU || op == DIVDUCC || op == DIVDE || op == DIVDECC || op == DIVDEU || op == DIVDEUCC || op == DIVDO || op == DIVDOCC || op == DIVDUO || op == DIVDUOCC) 
        return true;
    else if (op == MODUD || op == MODSD || op == MODUW || op == MODSW) 
        return true;
    else if (op == FADD || op == FADDS || op == FSUB || op == FSUBS || op == FMUL || op == FMULS || op == FDIV || op == FDIVS || op == FMADD || op == FMADDS || op == FMSUB || op == FMSUBS || op == FNMADD || op == FNMADDS || op == FNMSUB || op == FNMSUBS || op == FMULSCC) 
        return true;
    else if (op == FADDCC || op == FADDSCC || op == FSUBCC || op == FMULCC || op == FDIVCC || op == FDIVSCC) 
        return true;
    else if (op == OR || op == ORC || op == AND || op == ANDC || op == XOR || op == NAND || op == EQV || op == NOR || op == ANDCC || op == ORCC || op == XORCC || op == EQVCC || op == NORCC || op == NANDCC) 
        return true;
    else if (op == SLW || op == SLWCC || op == SLD || op == SLDCC || op == SRW || op == SRAW || op == SRWCC || op == SRAWCC || op == SRD || op == SRDCC || op == SRAD || op == SRADCC) 
        return true;
        return false;
}

// revCondMap maps a conditional register bit to its inverse, if possible.
private static map revCondMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"LT":"GE","GT":"LE","EQ":"NE",};

// plan9OpMap maps an Op to its Plan 9 mnemonics, if different than its GNU mnemonics.
private static map plan9OpMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, @string>{LWARX:"LWAR",LDARX:"LDAR",LHARX:"LHAR",LBARX:"LBAR",LWAX:"MOVW",LHAX:"MOVH",LWAUX:"MOVWU",LHAU:"MOVHU",LHAUX:"MOVHU",LDX:"MOVD",LDUX:"MOVDU",LWZX:"MOVWZ",LWZUX:"MOVWZU",LHZX:"MOVHZ",LHZUX:"MOVHZU",LBZX:"MOVBZ",LBZUX:"MOVBZU",LDBRX:"MOVDBR",LWBRX:"MOVWBR",LHBRX:"MOVHBR",MCRF:"MOVFL",XORI:"XOR",ORI:"OR",ANDICC:"ANDCC",ANDC:"ANDN",ADDEO:"ADDEV",ADDEOCC:"ADDEVCC",ADDO:"ADDV",ADDOCC:"ADDVCC",ADDMEO:"ADDMEV",ADDMEOCC:"ADDMEVCC",ADDCO:"ADDCV",ADDCOCC:"ADDCVCC",ADDZEO:"ADDZEV",ADDZEOCC:"ADDZEVCC",SUBFME:"SUBME",SUBFMECC:"SUBMECC",SUBFZE:"SUBZE",SUBFZECC:"SUBZECC",SUBFZEO:"SUBZEV",SUBFZEOCC:"SUBZEVCC",SUBFC:"SUBC",ORC:"ORN",MULLWO:"MULLWV",MULLWOCC:"MULLWVCC",MULLDO:"MULLDV",MULLDOCC:"MULLDVCC",DIVDO:"DIVDV",DIVDOCC:"DIVDVCC",DIVDUO:"DIVDUV",DIVDUOCC:"DIVDUVCC",ADDI:"ADD",MULLI:"MULLD",SRADI:"SRAD",SUBF:"SUB",STBCXCC:"STBCCC",STWCXCC:"STWCCC",STDCXCC:"STDCCC",LI:"MOVD",LBZ:"MOVBZ",STB:"MOVB",LBZU:"MOVBZU",STBU:"MOVBU",LHZ:"MOVHZ",LHA:"MOVH",STH:"MOVH",LHZU:"MOVHZU",STHU:"MOVHU",LWZ:"MOVWZ",LWA:"MOVW",STW:"MOVW",LWZU:"MOVWZU",STWU:"MOVWU",LD:"MOVD",STD:"MOVD",LDU:"MOVDU",STDU:"MOVDU",LFD:"FMOVD",STFD:"FMOVD",LFS:"FMOVS",STFS:"FMOVS",LFDX:"FMOVD",STFDX:"FMOVD",LFDU:"FMOVDU",STFDU:"FMOVDU",LFDUX:"FMOVDU",STFDUX:"FMOVDU",LFSX:"FMOVS",STFSX:"FMOVS",LFSU:"FMOVSU",STFSU:"FMOVSU",LFSUX:"FMOVSU",STFSUX:"FMOVSU",CMPD:"CMP",CMPDI:"CMP",CMPW:"CMPW",CMPWI:"CMPW",CMPLD:"CMPU",CMPLDI:"CMPU",CMPLW:"CMPWU",CMPLWI:"CMPWU",MTSPR:"MOVD",MFSPR:"MOVD",B:"BR",BL:"CALL",};

} // end ppc64asm_package
