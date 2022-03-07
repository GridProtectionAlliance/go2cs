// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2022 March 06 23:24:33 UTC
// import "cmd/vendor/golang.org/x/arch/arm/armasm" ==> using armasm = go.cmd.vendor.golang.org.x.arch.arm.armasm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\arm\armasm\gnu.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strings = go.strings_package;

namespace go.cmd.vendor.golang.org.x.arch.arm;

public static partial class armasm_package {

private static var saveDot = strings.NewReplacer(".F16", "_dot_F16", ".F32", "_dot_F32", ".F64", "_dot_F64", ".S32", "_dot_S32", ".U32", "_dot_U32", ".FXS", "_dot_S", ".FXU", "_dot_U", ".32", "_dot_32");

// GNUSyntax returns the GNU assembler syntax for the instruction, as defined by GNU binutils.
// This form typically matches the syntax defined in the ARM Reference Manual.
public static @string GNUSyntax(Inst inst) {
    bytes.Buffer buf = default;
    var op = inst.Op.String();
    op = saveDot.Replace(op);
    op = strings.Replace(op, ".", "", -1);
    op = strings.Replace(op, "_dot_", ".", -1);
    op = strings.ToLower(op);
    buf.WriteString(op);
    @string sep = " ";
    foreach (var (i, arg) in inst.Args) {
        if (arg == null) {
            break;
        }
        var text = gnuArg(_addr_inst, i, arg);
        if (text == "") {
            continue;
        }
        buf.WriteString(sep);
        sep = ", ";
        buf.WriteString(text);

    }    return buf.String();

}

private static @string gnuArg(ptr<Inst> _addr_inst, nint argIndex, Arg arg) {
    ref Inst inst = ref _addr_inst.val;


    if (inst.Op & ~15 == LDRD_EQ || inst.Op & ~15 == LDREXD_EQ || inst.Op & ~15 == STRD_EQ) 
        if (argIndex == 1) { 
            // second argument in consecutive pair not printed
            return "";

        }
    else if (inst.Op & ~15 == STREXD_EQ) 
        if (argIndex == 2) { 
            // second argument in consecutive pair not printed
            return "";

        }
        switch (arg.type()) {
        case Imm arg:

            if (inst.Op & ~15 == BKPT_EQ) 
                return fmt.Sprintf("%#04x", uint32(arg));
            else if (inst.Op & ~15 == SVC_EQ) 
                return fmt.Sprintf("%#08x", uint32(arg));
                        return fmt.Sprintf("#%d", int32(arg));
            break;
        case ImmAlt arg:
            return fmt.Sprintf("#%d, %d", arg.Val, arg.Rot);
            break;
        case Mem arg:
            var R = gnuArg(_addr_inst, -1, arg.Base);
            @string X = "";
            if (arg.Sign != 0) {
                X = "";
                if (arg.Sign < 0) {
                    X = "-";
                }
                X += gnuArg(_addr_inst, -1, arg.Index);
                if (arg.Shift == ShiftLeft && arg.Count == 0) { 
                    // nothing
                }
                else if (arg.Shift == RotateRightExt) {
                    X += ", rrx";
                }
                else
 {
                    X += fmt.Sprintf(", %s #%d", strings.ToLower(arg.Shift.String()), arg.Count);
                }

            }
            else
 {
                X = fmt.Sprintf("#%d", arg.Offset);
            }


            if (arg.Mode == AddrOffset) 
                if (X == "#0") {
                    return fmt.Sprintf("[%s]", R);
                }
                return fmt.Sprintf("[%s, %s]", R, X);
            else if (arg.Mode == AddrPreIndex) 
                return fmt.Sprintf("[%s, %s]!", R, X);
            else if (arg.Mode == AddrPostIndex) 
                return fmt.Sprintf("[%s], %s", R, X);
            else if (arg.Mode == AddrLDM) 
                if (X == "#0") {
                    return R;
                }
            else if (arg.Mode == AddrLDM_WB) 
                if (X == "#0") {
                    return R + "!";
                }
                        return fmt.Sprintf("[%s Mode(%d) %s]", R, int(arg.Mode), X);
            break;
        case PCRel arg:
            return fmt.Sprintf(".%+#x", int32(arg) + 4);
            break;
        case Reg arg:

            if (inst.Op & ~15 == LDREX_EQ) 
                if (argIndex == 0) {
                    return fmt.Sprintf("r%d", int32(arg));
                }
            
            if (arg == R10) 
                return "sl";
            else if (arg == R11) 
                return "fp";
            else if (arg == R12) 
                return "ip";
                        break;
        case RegList arg:
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            fmt.Fprintf(_addr_buf, "{");
            @string sep = "";
            for (nint i = 0; i < 16; i++) {
                if (arg & (1 << (int)(uint(i))) != 0) {
                    fmt.Fprintf(_addr_buf, "%s%s", sep, gnuArg(_addr_inst, -1, Reg(i)));
                    sep = ", ";
                }
            }

            fmt.Fprintf(_addr_buf, "}");
            return buf.String();
            break;
        case RegShift arg:
            if (arg.Shift == ShiftLeft && arg.Count == 0) {
                return gnuArg(_addr_inst, -1, arg.Reg);
            }
            if (arg.Shift == RotateRightExt) {
                return gnuArg(_addr_inst, -1, arg.Reg) + ", rrx";
            }
            return fmt.Sprintf("%s, %s #%d", gnuArg(_addr_inst, -1, arg.Reg), strings.ToLower(arg.Shift.String()), arg.Count);
            break;
        case RegShiftReg arg:
            return fmt.Sprintf("%s, %s %s", gnuArg(_addr_inst, -1, arg.Reg), strings.ToLower(arg.Shift.String()), gnuArg(_addr_inst, -1, arg.RegCount));
            break;
    }
    return strings.ToLower(arg.String());

}

} // end armasm_package
