// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2020 October 09 05:54:24 UTC
// import "cmd/vendor/golang.org/x/arch/arm/armasm" ==> using armasm = go.cmd.vendor.golang.org.x.arch.arm.armasm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm\armasm\gnu.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm
{
    public static partial class armasm_package
    {
        private static var saveDot = strings.NewReplacer(".F16", "_dot_F16", ".F32", "_dot_F32", ".F64", "_dot_F64", ".S32", "_dot_S32", ".U32", "_dot_U32", ".FXS", "_dot_S", ".FXU", "_dot_U", ".32", "_dot_32");

        // GNUSyntax returns the GNU assembler syntax for the instruction, as defined by GNU binutils.
        // This form typically matches the syntax defined in the ARM Reference Manual.
        public static @string GNUSyntax(Inst inst)
        {
            bytes.Buffer buf = default;
            var op = inst.Op.String();
            op = saveDot.Replace(op);
            op = strings.Replace(op, ".", "", -1L);
            op = strings.Replace(op, "_dot_", ".", -1L);
            op = strings.ToLower(op);
            buf.WriteString(op);
            @string sep = " ";
            foreach (var (i, arg) in inst.Args)
            {
                if (arg == null)
                {
                    break;
                }

                var text = gnuArg(_addr_inst, i, arg);
                if (text == "")
                {
                    continue;
                }

                buf.WriteString(sep);
                sep = ", ";
                buf.WriteString(text);

            }
            return buf.String();

        }

        private static @string gnuArg(ptr<Inst> _addr_inst, long argIndex, Arg arg)
        {
            ref Inst inst = ref _addr_inst.val;


            if (inst.Op & ~15L == LDRD_EQ || inst.Op & ~15L == LDREXD_EQ || inst.Op & ~15L == STRD_EQ) 
                if (argIndex == 1L)
                { 
                    // second argument in consecutive pair not printed
                    return "";

                }

            else if (inst.Op & ~15L == STREXD_EQ) 
                if (argIndex == 2L)
                { 
                    // second argument in consecutive pair not printed
                    return "";

                }

                        switch (arg.type())
            {
                case Imm arg:

                    if (inst.Op & ~15L == BKPT_EQ) 
                        return fmt.Sprintf("%#04x", uint32(arg));
                    else if (inst.Op & ~15L == SVC_EQ) 
                        return fmt.Sprintf("%#08x", uint32(arg));
                                        return fmt.Sprintf("#%d", int32(arg));
                    break;
                case ImmAlt arg:
                    return fmt.Sprintf("#%d, %d", arg.Val, arg.Rot);
                    break;
                case Mem arg:
                    var R = gnuArg(_addr_inst, -1L, arg.Base);
                    @string X = "";
                    if (arg.Sign != 0L)
                    {
                        X = "";
                        if (arg.Sign < 0L)
                        {
                            X = "-";
                        }

                        X += gnuArg(_addr_inst, -1L, arg.Index);
                        if (arg.Shift == ShiftLeft && arg.Count == 0L)
                        { 
                            // nothing
                        }
                        else if (arg.Shift == RotateRightExt)
                        {
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
                        if (X == "#0")
                        {
                            return fmt.Sprintf("[%s]", R);
                        }

                        return fmt.Sprintf("[%s, %s]", R, X);
                    else if (arg.Mode == AddrPreIndex) 
                        return fmt.Sprintf("[%s, %s]!", R, X);
                    else if (arg.Mode == AddrPostIndex) 
                        return fmt.Sprintf("[%s], %s", R, X);
                    else if (arg.Mode == AddrLDM) 
                        if (X == "#0")
                        {
                            return R;
                        }

                    else if (arg.Mode == AddrLDM_WB) 
                        if (X == "#0")
                        {
                            return R + "!";
                        }

                                        return fmt.Sprintf("[%s Mode(%d) %s]", R, int(arg.Mode), X);
                    break;
                case PCRel arg:
                    return fmt.Sprintf(".%+#x", int32(arg) + 4L);
                    break;
                case Reg arg:

                    if (inst.Op & ~15L == LDREX_EQ) 
                        if (argIndex == 0L)
                        {
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
                    for (long i = 0L; i < 16L; i++)
                    {
                        if (arg & (1L << (int)(uint(i))) != 0L)
                        {
                            fmt.Fprintf(_addr_buf, "%s%s", sep, gnuArg(_addr_inst, -1L, Reg(i)));
                            sep = ", ";
                        }

                    }

                    fmt.Fprintf(_addr_buf, "}");
                    return buf.String();
                    break;
                case RegShift arg:
                    if (arg.Shift == ShiftLeft && arg.Count == 0L)
                    {
                        return gnuArg(_addr_inst, -1L, arg.Reg);
                    }

                    if (arg.Shift == RotateRightExt)
                    {
                        return gnuArg(_addr_inst, -1L, arg.Reg) + ", rrx";
                    }

                    return fmt.Sprintf("%s, %s #%d", gnuArg(_addr_inst, -1L, arg.Reg), strings.ToLower(arg.Shift.String()), arg.Count);
                    break;
                case RegShiftReg arg:
                    return fmt.Sprintf("%s, %s %s", gnuArg(_addr_inst, -1L, arg.Reg), strings.ToLower(arg.Shift.String()), gnuArg(_addr_inst, -1L, arg.RegCount));
                    break;
            }
            return strings.ToLower(arg.String());

        }
    }
}}}}}}}
