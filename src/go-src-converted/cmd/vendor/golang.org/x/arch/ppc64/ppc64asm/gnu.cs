// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2022 March 13 06:38:20 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\gnu.go
namespace go.cmd.vendor.golang.org.x.arch.ppc64;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;

public static partial class ppc64asm_package {

 
// bit 3 of index is a negated check.
private static array<@string> condBit = new array<@string>(new @string[] { "lt", "gt", "eq", "so", "ge", "le", "ne", "ns" });

// GNUSyntax returns the GNU assembler syntax for the instruction, as defined by GNU binutils.
// This form typically matches the syntax defined in the Power ISA Reference Manual.
public static @string GNUSyntax(Inst inst, ulong pc) {
    bytes.Buffer buf = default; 
    // When there are all 0s, identify them as the disassembler
    // in binutils would.
    if (inst.Enc == 0) {
        return ".long 0x0";
    }
    else if (inst.Op == 0) {
        return "error: unknown instruction";
    }
    var PC = pc; 
    // Special handling for some ops
    nint startArg = 0;
    @string sep = " ";
    var opName = inst.Op.String();
    var argList = inst.Args[..];

    switch (opName) {
        case "bc": 

        case "bcl": 

        case "bca": 

        case "bcla": 

        case "bclr": 

        case "bclrl": 

        case "bcctr": 

        case "bcctrl": 

        case "bctar": 

        case "bctarl": 
                   var sfx = inst.Op.String()[(int)2..];
                   var bo = int(inst.Args[0]._<Imm>());
                   CondReg bi = inst.Args[1]._<CondReg>();
                   array<@string> atsfx = new array<@string>(new @string[] { "", "?", "-", "+" });
                   array<@string> decsfx = new array<@string>(new @string[] { "dnz", "dz" }); 

                   //BO field is... complicated (z == ignored bit, at == prediction hint)
                   //Paraphrased from ISA 3.1 Book I Section 2.4:
                   //
                   //0000z -> decrement ctr, b if ctr != 0 and CRbi == 0
                   //0001z -> decrement ctr, b if ctr == 0 and CRbi == 0
                   //001at -> b if CRbi == 0
                   //0100z -> decrement ctr, b if ctr != 0 and CRbi == 1
                   //0101z -> decrement ctr, b if ctr == 0 and CRbi == 1
                   //011at -> b if CRbi == 1
                   //1a00t -> decrement ctr, b if ctr != 0
                   //1a01t -> decrement ctr, b if ctr == 0
                   //1z1zz -> b always

                   // Decoding (in this order) we get
                   // BO & 0b00100 == 0b00000 -> dz if BO[1], else dnz (not simplified for bcctrl forms)
                   // BO & 0b10000 == 0b10000 -> (bc and bca forms not simplified), at = B[4]B[0] if B[2] != 0, done
                   // BO & 0b10000 == 0b00000 -> t if BO[3], else f
                   // BO & 0b10100 == 0b00100 -> at = B[0:1]

                   // BI fields rename as follows:
                   // less than            : lt BI%4==0 && test == t
                   // less than or equal   : le BI%4==1 && test == f
                   // equal         : eq BI%4==2 && test == t
                   // greater than or equal: ge BI%4==0 && test == f
                   // greater than        : gt BI%4==1 && test == t
                   // not less than    : nl BI%4==0 && test == f
                   // not equal        : ne BI%4==2 && test == f
                   // not greater than    : ng BI%4==1 && test == f
                   // summary overflow    : so BI%4==3 && test == t
                   // not summary overflow : ns BI%4==3 && test == f
                   // unordered        : un BI%4==3 && test == t
                   // not unordered    : nu BI%4==3 && test == f
                   //
                   // Note, there are only 8 possible tests, but quite a few more
                   // ways to name fields.  For simplicity, we choose those in condBit.

                   nint at = 0; // 0 == no hint, 1 == reserved, 2 == not likely, 3 == likely
                   nint form = 1; // 1 == n/a,  0 == cr bit not set, 4 == cr bit set
                   var cr = (bi - Cond0LT) / 4;
                   nint bh = -1; // Only for lr/tar/ctr variants.
                   switch (opName) {
                       case "bclr": 

                       case "bclrl": 

                       case "bcctr": 

                       case "bcctrl": 

                       case "bctar": 

                       case "bctarl": 
                           bh = int(inst.Args[2]._<Imm>());
                           break;
                   }

                   if (bo & 0x14 == 0x14) {
                       if (bo == 0x14 && bi == Cond0LT) { // preferred form of unconditional branch
                           // Likewise, avoid printing fake b/ba/bl/bla
                           if (opName != "bc" && opName != "bca" && opName != "bcl" && opName != "bcla") {
                               startArg = 2;
                           }
                       }
                   }
                   else if (bo & 0x04 == 0) { // ctr is decremented
                       if (opName != "bcctr" && opName != "bcctrl") {
                           startArg = 1;
                           @string tf = "";
                           if (bo & 0x10 == 0x00) {
                               tf = "f";
                               if (bo & 0x08 == 0x08) {
                                   tf = "t";
                               }
                           }
                           sfx = decsfx[(bo >> 1) & 1] + tf + sfx;
                       }
                       if (bo & 0x10 == 0x10) {
                           if (opName != "bcctr" && opName != "bcctrl") {
                               startArg = 2;
                           }
                           if (bi != Cond0LT) { 
                               // A non-zero BI bit was encoded, but ignored by BO
                               startArg = 0;
                           }
                           at = ((bo & 0x8) >> 2) | (bo & 0x1);
                       }
                       else if (bo & 0x4 == 0x4) {
                           at = bo & 0x3;
                       }
                   }
                   else if (bo & 0x10 == 0x10) { // BI field is not used
                       if (opName != "bca" && opName != "bc") {
                           at = ((bo & 0x8) >> 2) | (bo & 0x1);
                           startArg = 2;
                       } 
                       // If BI is encoded as a bit other than 0, no mnemonic.
                       if (bo & 0x14 == 0x14) {
                           startArg = 0;
                       }
                   }
                   else
            {
                       form = (bo & 0x8) >> 1;
                       startArg = 2;
                       if (bo & 0x14 == 0x04) {
                           at = bo & 0x3;
                       }
                   }
                   sfx += atsfx[at];

                   if (form != 1) {
                       var bit = int((bi - Cond0LT) % 4) | (~form) & 0x4;
                       sfx = condBit[bit] + sfx;
                   }
                   if (at != 1 && startArg > 0 && bh <= 0) {
                       var str = fmt.Sprintf("b%s", sfx);
                       if (startArg > 1 && (cr != 0 || bh > 0)) {
                           str += fmt.Sprintf(" cr%d", cr);
                           sep = ",";
                       }
                       buf.WriteString(str);
                       if (startArg < 2 && bh == 0) {
                           str = fmt.Sprintf(" %s", gnuArg(_addr_inst, 1, inst.Args[1], PC));
                           buf.WriteString(str);
                           startArg = 3;
                       }
                       else if (bh == 0) {
                           startArg = 3;
                       }
                   }
                   else
            {
                       if (startArg == 0 || bh > 0 || at == 1) {
                           buf.WriteString(inst.Op.String());
                           buf.WriteString(atsfx[at]);
                           startArg = 0;
                       }
                       else
            {
                           buf.WriteString("b" + sfx);
                       }
                       if (bh == 0) {
                           str = fmt.Sprintf(" %d,%s", bo, gnuArg(_addr_inst, 1, inst.Args[1], PC));
                           buf.WriteString(str);
                           startArg = 3;
                       }
                   }
            break;
        case "mtspr": 
            var opcode = inst.Op.String();
            buf.WriteString(opcode[(int)0..(int)2]);
            switch (inst.Args[0].type()) {
                case SpReg spr:
                    switch (spr) {
                        case 1: 
                            buf.WriteString("xer");
                            startArg = 1;
                            break;
                        case 8: 
                            buf.WriteString("lr");
                            startArg = 1;
                            break;
                        case 9: 
                            buf.WriteString("ctr");
                            startArg = 1;
                            break;
                        default: 
                            buf.WriteString("spr");
                            break;
                    }
                    break;
                default:
                {
                    var spr = inst.Args[0].type();
                    buf.WriteString("spr");
                    break;
                }

            }
            break;
        case "mfspr": 
            opcode = inst.Op.String();
            buf.WriteString(opcode[(int)0..(int)2]);
            var arg = inst.Args[0];
            switch (inst.Args[1].type()) {
                case SpReg spr:
                    switch (spr) {
                        case 1: 
                            buf.WriteString("xer ");
                            buf.WriteString(gnuArg(_addr_inst, 0, arg, PC));
                            startArg = 2;
                            break;
                        case 8: 
                            buf.WriteString("lr ");
                            buf.WriteString(gnuArg(_addr_inst, 0, arg, PC));
                            startArg = 2;
                            break;
                        case 9: 
                            buf.WriteString("ctr ");
                            buf.WriteString(gnuArg(_addr_inst, 0, arg, PC));
                            startArg = 2;
                            break;
                        case 268: 
                            buf.WriteString("tb ");
                            buf.WriteString(gnuArg(_addr_inst, 0, arg, PC));
                            startArg = 2;
                            break;
                        default: 
                            buf.WriteString("spr");
                            break;
                    }
                    break;
                default:
                {
                    var spr = inst.Args[1].type();
                    buf.WriteString("spr");
                    break;
                }

            }
            break;
        case "mtfsfi": 

        case "mtfsfi.": 
            buf.WriteString(opName);
            Imm l = inst.Args[2]._<Imm>();
            if (l == 0) { 
                // L == 0 is an extended mnemonic for the same.
                var asm = fmt.Sprintf(" %s,%s", gnuArg(_addr_inst, 0, inst.Args[0], PC), gnuArg(_addr_inst, 1, inst.Args[1], PC));
                buf.WriteString(asm);
                startArg = 3;
            }
            break;
        case "paste.": 
            buf.WriteString(opName);
            l = inst.Args[2]._<Imm>();
            if (l == 1) { 
                // L == 1 is an extended mnemonic for the same.
                asm = fmt.Sprintf(" %s,%s", gnuArg(_addr_inst, 0, inst.Args[0], PC), gnuArg(_addr_inst, 1, inst.Args[1], PC));
                buf.WriteString(asm);
                startArg = 3;
            }
            break;
        case "mtfsf": 

        case "mtfsf.": 
            buf.WriteString(opName);
            l = inst.Args[3]._<Imm>();
            if (l == 0) { 
                // L == 0 is an extended mnemonic for the same.
                asm = fmt.Sprintf(" %s,%s,%s", gnuArg(_addr_inst, 0, inst.Args[0], PC), gnuArg(_addr_inst, 1, inst.Args[1], PC), gnuArg(_addr_inst, 2, inst.Args[2], PC));
                buf.WriteString(asm);
                startArg = 4;
            }
            break;
        case "sync": 
            Imm lsc = inst.Args[0]._<Imm>() << 4 | inst.Args[1]._<Imm>();
            switch (lsc) {
                case 0x00: 
                    buf.WriteString("hwsync");
                    startArg = 2;
                    break;
                case 0x10: 
                    buf.WriteString("lwsync");
                    startArg = 2;
                    break;
                default: 
                    buf.WriteString(opName);
                    break;
            }
            break;
        case "lbarx": 
            // If EH == 0, omit printing EH.

        case "lharx": 
            // If EH == 0, omit printing EH.

        case "lwarx": 
            // If EH == 0, omit printing EH.

        case "ldarx": 
            // If EH == 0, omit printing EH.
            Imm eh = inst.Args[3]._<Imm>();
            if (eh == 0) {
                argList = inst.Args[..(int)3];
            }
            buf.WriteString(inst.Op.String());
            break;
        case "paddi": 
            // There are several extended mnemonics.  Notably, "pla" is
            // the only valid mnemonic for paddi (R=1), In this case, RA must
            // always be 0.  Otherwise it is invalid.
            Imm r = inst.Args[3]._<Imm>();
            Reg ra = inst.Args[1]._<Reg>();
            str = opName;
            if (ra == R0) {
                @string name = new slice<@string>(new @string[] { "pli", "pla" });
                str = fmt.Sprintf("%s %s,%s", name[r & 1], gnuArg(_addr_inst, 0, inst.Args[0], PC), gnuArg(_addr_inst, 2, inst.Args[2], PC));
                startArg = 4;
            }
            else if (r == 0) {
                str = fmt.Sprintf("%s %s,%s,%s", opName, gnuArg(_addr_inst, 0, inst.Args[0], PC), gnuArg(_addr_inst, 1, inst.Args[1], PC), gnuArg(_addr_inst, 2, inst.Args[2], PC));
                startArg = 4;
            }
            buf.WriteString(str);
            break;
        default: 
            // Prefixed load/stores do not print the displacement register when R==1 (they are PCrel).
            // This also implies RA should be 0.  Likewise, when R==0, printing of R can be omitted.
                   if (strings.HasPrefix(opName, "pl") || strings.HasPrefix(opName, "pst")) {
                       r = inst.Args[3]._<Imm>();
                       ra = inst.Args[2]._<Reg>();
                       Offset d = inst.Args[1]._<Offset>();
                       if (r == 1 && ra == R0) {
                           str = fmt.Sprintf("%s %s,%d", opName, gnuArg(_addr_inst, 0, inst.Args[0], PC), d);
                           buf.WriteString(str);
                           startArg = 4;
                       }
                       else if (r == 0) {
                           str = fmt.Sprintf("%s %s,%d(%s)", opName, gnuArg(_addr_inst, 0, inst.Args[0], PC), d, gnuArg(_addr_inst, 2, inst.Args[2], PC));
                           buf.WriteString(str);
                           startArg = 4;
                       }
                   }
                   else
            {
                       buf.WriteString(opName);
                   }
            break;
    }
    {
        var arg__prev1 = arg;

        foreach (var (__i, __arg) in argList) {
            i = __i;
            arg = __arg;
            if (arg == null) {
                break;
            }
            if (i < startArg) {
                continue;
            }
            var text = gnuArg(_addr_inst, i, arg, PC);
            if (text == "") {
                continue;
            }
            buf.WriteString(sep);
            sep = ",";
            buf.WriteString(text);
        }
        arg = arg__prev1;
    }

    return buf.String();
}

// gnuArg formats arg (which is the argIndex's arg in inst) according to GNU rules.
// NOTE: because GNUSyntax is the only caller of this func, and it receives a copy
//       of inst, it's ok to modify inst.Args here.
private static @string gnuArg(ptr<Inst> _addr_inst, nint argIndex, Arg arg, ulong pc) => func((_, panic, _) => {
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
            return arg.String();
            break;
        case CondReg arg:
            if (arg == CR0 && strings.HasPrefix(inst.Op.String(), "cmp")) {
                return ""; // don't show cr0 for cmp instructions
            }
            else if (arg >= CR0) {
                return fmt.Sprintf("cr%d", int(arg - CR0));
            }
            var bit = condBit[(arg - Cond0LT) % 4];
            if (arg <= Cond0SO) {
                return bit;
            }
            return fmt.Sprintf("4*cr%d+%s", int(arg - Cond0LT) / 4, bit);
            break;
        case Imm arg:
            return fmt.Sprintf("%d", arg);
            break;
        case SpReg arg:
            switch (int(arg)) {
                case 1: 
                    return "xer";
                    break;
                case 8: 
                    return "lr";
                    break;
                case 9: 
                    return "ctr";
                    break;
                case 268: 
                    return "tb";
                    break;
                default: 
                    return fmt.Sprintf("%d", int(arg));
                    break;
            }
            break;
        case PCRel arg:
            if (int(arg) == 0) {
                return fmt.Sprintf(".%+#x", int(arg));
            }
            var addr = pc + uint64(int64(arg));
            return fmt.Sprintf("%#x", addr);
            break;
        case Label arg:
            return fmt.Sprintf("%#x", uint32(arg));
            break;
        case Offset arg:
            Reg reg = inst.Args[argIndex + 1]._<Reg>();
            removeArg(_addr_inst, argIndex + 1);
            if (reg == R0) {
                return fmt.Sprintf("%d(0)", int(arg));
            }
            return fmt.Sprintf("%d(r%d)", int(arg), reg - R0);
            break;
    }
    return fmt.Sprintf("???(%v)", arg);
});

// removeArg removes the arg in inst.Args[index].
private static void removeArg(ptr<Inst> _addr_inst, nint index) {
    ref Inst inst = ref _addr_inst.val;

    for (var i = index; i < len(inst.Args); i++) {
        if (i + 1 < len(inst.Args)) {
            inst.Args[i] = inst.Args[i + 1];
        }
        else
 {
            inst.Args[i] = null;
        }
    }
}

// isLoadStoreOp returns true if op is a load or store instruction
private static bool isLoadStoreOp(Op op) {

    if (op == LBZ || op == LBZU || op == LBZX || op == LBZUX) 
        return true;
    else if (op == LHZ || op == LHZU || op == LHZX || op == LHZUX) 
        return true;
    else if (op == LHA || op == LHAU || op == LHAX || op == LHAUX) 
        return true;
    else if (op == LWZ || op == LWZU || op == LWZX || op == LWZUX) 
        return true;
    else if (op == LWA || op == LWAX || op == LWAUX) 
        return true;
    else if (op == LD || op == LDU || op == LDX || op == LDUX) 
        return true;
    else if (op == LQ) 
        return true;
    else if (op == STB || op == STBU || op == STBX || op == STBUX) 
        return true;
    else if (op == STH || op == STHU || op == STHX || op == STHUX) 
        return true;
    else if (op == STW || op == STWU || op == STWX || op == STWUX) 
        return true;
    else if (op == STD || op == STDU || op == STDX || op == STDUX) 
        return true;
    else if (op == STQ) 
        return true;
    else if (op == LHBRX || op == LWBRX || op == STHBRX || op == STWBRX) 
        return true;
    else if (op == LBARX || op == LWARX || op == LHARX || op == LDARX) 
        return true;
        return false;
}

} // end ppc64asm_package
