// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2020 October 08 04:44:11 UTC
// import "cmd/vendor/golang.org/x/arch/arm/armasm" ==> using armasm = go.cmd.vendor.golang.org.x.arch.arm.armasm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\arm\armasm\plan9x.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using strings = go.strings_package;
using static go.builtin;
using System;

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
        // GoSyntax returns the Go assembler syntax for the instruction.
        // The syntax was originally defined by Plan 9.
        // The pc is the program counter of the instruction, used for expanding
        // PC-relative addresses into absolute ones.
        // The symname function queries the symbol table for the program
        // being disassembled. Given a target address it returns the name and base
        // address of the symbol containing the target, if any; otherwise it returns "", 0.
        // The reader r should read from the text segment using text addresses
        // as offsets; it is used to display pc-relative loads as constant loads.
        public static @string GoSyntax(Inst inst, ulong pc, Func<ulong, (@string, ulong)> symname, io.ReaderAt text) => func((_, panic, __) =>
        {
            if (symname == null)
            {
                symname = _p0 => ("", 0L);
            }
            slice<@string> args = default;
            foreach (var (_, a) in inst.Args)
            {
                if (a == null)
                {
                    break;
                }
                args = append(args, plan9Arg(_addr_inst, pc, symname, a));

            }            var op = inst.Op.String();


            if (inst.Op & ~15L == LDR_EQ || inst.Op & ~15L == LDRB_EQ || inst.Op & ~15L == LDRH_EQ || inst.Op & ~15L == LDRSB_EQ || inst.Op & ~15L == LDRSH_EQ || inst.Op & ~15L == VLDR_EQ) 
                // Check for RET
                Reg (reg, _) = inst.Args[0L]._<Reg>();
                Mem (mem, _) = inst.Args[1L]._<Mem>();
                if (inst.Op & ~15L == LDR_EQ && reg == R15 && mem.Base == SP && mem.Sign == 0L && mem.Mode == AddrPostIndex)
                {
                    return fmt.Sprintf("RET%s #%d", op[3L..], mem.Offset);
                }
                if (mem.Base == PC && mem.Sign == 0L && mem.Mode == AddrOffset && text != null)
                {
                    var addr = uint32(pc) + 8L + uint32(mem.Offset);
                    var buf = make_slice<byte>(8L);

                    if (inst.Op & ~15L == LDRB_EQ || inst.Op & ~15L == LDRSB_EQ) 
                        {
                            var (_, err) = text.ReadAt(buf[..1L], int64(addr));

                            if (err != null)
                            {
                                break;
                            }
                        }

                        args[1L] = fmt.Sprintf("$%#x", buf[0L]);
                    else if (inst.Op & ~15L == LDRH_EQ || inst.Op & ~15L == LDRSH_EQ) 
                        {
                            (_, err) = text.ReadAt(buf[..2L], int64(addr));

                            if (err != null)
                            {
                                break;
                            }
                        }

                        args[1L] = fmt.Sprintf("$%#x", binary.LittleEndian.Uint16(buf));
                    else if (inst.Op & ~15L == LDR_EQ) 
                        {
                            (_, err) = text.ReadAt(buf[..4L], int64(addr));

                            if (err != null)
                            {
                                break;
                            }
                        }

                        var x = binary.LittleEndian.Uint32(buf);
                        {
                            var (s, base) = symname(uint64(x));

                            if (s != "" && uint64(x) == base)
                            {
                                args[1L] = fmt.Sprintf("$%s(SB)", s);
                            }
                            else
                            {
                                args[1L] = fmt.Sprintf("$%#x", x);
                            }
                        }


                    else if (inst.Op & ~15L == VLDR_EQ) 

                        if (strings.HasPrefix(args[0L], "D")) // VLDR.F64
                            {
                                (_, err) = text.ReadAt(buf, int64(addr));

                                if (err != null)
                                {
                                    break;
                                }
                            }

                            args[1L] = fmt.Sprintf("$%f", math.Float64frombits(binary.LittleEndian.Uint64(buf)));
                        else if (strings.HasPrefix(args[0L], "S")) // VLDR.F32
                            {
                                (_, err) = text.ReadAt(buf[..4L], int64(addr));

                                if (err != null)
                                {
                                    break;
                                }
                            }

                            args[1L] = fmt.Sprintf("$%f", math.Float32frombits(binary.LittleEndian.Uint32(buf)));
                        else 
                            panic(fmt.Sprintf("wrong FP register: %v", inst));
                                            
                }
            // Move addressing mode into opcode suffix.
            @string suffix = "";

            if (inst.Op & ~15L == PLD || inst.Op & ~15L == PLI || inst.Op & ~15L == PLD_W) 
                {
                    Mem mem__prev1 = mem;

                    Mem (mem, ok) = inst.Args[0L]._<Mem>();

                    if (ok)
                    {
                        args[0L], suffix = memOpTrans(mem);
                    }
                    else
                    {
                        panic(fmt.Sprintf("illegal instruction: %v", inst));
                    }
                    mem = mem__prev1;

                }

            else if (inst.Op & ~15L == LDR_EQ || inst.Op & ~15L == LDRB_EQ || inst.Op & ~15L == LDRSB_EQ || inst.Op & ~15L == LDRH_EQ || inst.Op & ~15L == LDRSH_EQ || inst.Op & ~15L == STR_EQ || inst.Op & ~15L == STRB_EQ || inst.Op & ~15L == STRH_EQ || inst.Op & ~15L == VLDR_EQ || inst.Op & ~15L == VSTR_EQ || inst.Op & ~15L == LDREX_EQ || inst.Op & ~15L == LDREXH_EQ || inst.Op & ~15L == LDREXB_EQ) 
                {
                    Mem mem__prev1 = mem;

                    (mem, ok) = inst.Args[1L]._<Mem>();

                    if (ok)
                    {
                        args[1L], suffix = memOpTrans(mem);
                    }
                    else
                    {
                        panic(fmt.Sprintf("illegal instruction: %v", inst));
                    }
                    mem = mem__prev1;

                }

            else if (inst.Op & ~15L == SWP_EQ || inst.Op & ~15L == SWP_B_EQ || inst.Op & ~15L == STREX_EQ || inst.Op & ~15L == STREXB_EQ || inst.Op & ~15L == STREXH_EQ) 
                {
                    Mem mem__prev1 = mem;

                    (mem, ok) = inst.Args[2L]._<Mem>();

                    if (ok)
                    {
                        args[2L], suffix = memOpTrans(mem);
                    }
                    else
                    {
                        panic(fmt.Sprintf("illegal instruction: %v", inst));
                    }
                    mem = mem__prev1;

                }

            // Reverse args, placing dest last.
            {
                long i = 0L;
                var j = len(args) - 1L;

                while (i < j)
                {
                    args[i] = args[j];
                    args[j] = args[i];
                    i = i + 1L;
                j = j - 1L;
                }
            } 
            // For MLA-like instructions, the addend is the third operand.

            if (inst.Op & ~15L == SMLAWT_EQ || inst.Op & ~15L == SMLAWB_EQ || inst.Op & ~15L == MLA_EQ || inst.Op & ~15L == MLA_S_EQ || inst.Op & ~15L == MLS_EQ || inst.Op & ~15L == SMMLA_EQ || inst.Op & ~15L == SMMLS_EQ || inst.Op & ~15L == SMLABB_EQ || inst.Op & ~15L == SMLATB_EQ || inst.Op & ~15L == SMLABT_EQ || inst.Op & ~15L == SMLATT_EQ || inst.Op & ~15L == SMLAD_EQ || inst.Op & ~15L == SMLAD_X_EQ || inst.Op & ~15L == SMLSD_EQ || inst.Op & ~15L == SMLSD_X_EQ) 
                args = new slice<@string>(new @string[] { args[1], args[2], args[0], args[3] });
            // For STREX like instructions, the memory operands comes first.

            if (inst.Op & ~15L == STREX_EQ || inst.Op & ~15L == STREXB_EQ || inst.Op & ~15L == STREXH_EQ || inst.Op & ~15L == SWP_EQ || inst.Op & ~15L == SWP_B_EQ) 
                args = new slice<@string>(new @string[] { args[1], args[0], args[2] });
            // special process for FP instructions
            op, args = fpTrans(_addr_inst, op, args); 

            // LDR/STR like instructions -> MOV like

            if (inst.Op & ~15L == MOV_EQ) 
                op = "MOVW" + op[3L..];
            else if (inst.Op & ~15L == LDR_EQ || inst.Op & ~15L == MSR_EQ || inst.Op & ~15L == MRS_EQ) 
                op = "MOVW" + op[3L..] + suffix;
            else if (inst.Op & ~15L == VMRS_EQ || inst.Op & ~15L == VMSR_EQ) 
                op = "MOVW" + op[4L..] + suffix;
            else if (inst.Op & ~15L == LDRB_EQ || inst.Op & ~15L == UXTB_EQ) 
                op = "MOVBU" + op[4L..] + suffix;
            else if (inst.Op & ~15L == LDRSB_EQ) 
                op = "MOVBS" + op[5L..] + suffix;
            else if (inst.Op & ~15L == SXTB_EQ) 
                op = "MOVBS" + op[4L..] + suffix;
            else if (inst.Op & ~15L == LDRH_EQ || inst.Op & ~15L == UXTH_EQ) 
                op = "MOVHU" + op[4L..] + suffix;
            else if (inst.Op & ~15L == LDRSH_EQ) 
                op = "MOVHS" + op[5L..] + suffix;
            else if (inst.Op & ~15L == SXTH_EQ) 
                op = "MOVHS" + op[4L..] + suffix;
            else if (inst.Op & ~15L == STR_EQ) 
                op = "MOVW" + op[3L..] + suffix;
                args[0L] = args[1L];
                args[1L] = args[0L];
            else if (inst.Op & ~15L == STRB_EQ) 
                op = "MOVB" + op[4L..] + suffix;
                args[0L] = args[1L];
                args[1L] = args[0L];
            else if (inst.Op & ~15L == STRH_EQ) 
                op = "MOVH" + op[4L..] + suffix;
                args[0L] = args[1L];
                args[1L] = args[0L];
            else if (inst.Op & ~15L == VSTR_EQ) 
                args[0L] = args[1L];
                args[1L] = args[0L];
            else 
                op = op + suffix;
                        if (args != null)
            {
                op += " " + strings.Join(args, ", ");
            }
            return op;

        });

        // assembler syntax for the various shifts.
        // @x> is a lie; the assembler uses @> 0
        // instead of @x> 1, but i wanted to be clear that it
        // was a different operation (rotate right extended, not rotate right).
        private static @string plan9Shift = new slice<@string>(new @string[] { "<<", ">>", "->", "@>", "@x>" });

        private static @string plan9Arg(ptr<Inst> _addr_inst, ulong pc, Func<ulong, (@string, ulong)> symname, Arg arg)
        {
            ref Inst inst = ref _addr_inst.val;

            switch (arg.type())
            {
                case Endian a:
                    break;
                case Imm a:
                    return fmt.Sprintf("$%d", uint32(a));
                    break;
                case Mem a:
                    break;
                case PCRel a:
                    var addr = uint32(pc) + 8L + uint32(a);
                    {
                        var (s, base) = symname(uint64(addr));

                        if (s != "" && uint64(addr) == base)
                        {
                            return fmt.Sprintf("%s(SB)", s);
                        }

                    }

                    return fmt.Sprintf("%#x", addr);
                    break;
                case Reg a:
                    if (a < 16L)
                    {
                        return fmt.Sprintf("R%d", int(a));
                    }

                    break;
                case RegList a:
                    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
                    long start = -2L;
                    long end = -2L;
                    fmt.Fprintf(_addr_buf, "[");
                    Action flush = () =>
                    {
                        if (start >= 0L)
                        {
                            if (buf.Len() > 1L)
                            {
                                fmt.Fprintf(_addr_buf, ",");
                            }

                            if (start == end)
                            {
                                fmt.Fprintf(_addr_buf, "R%d", start);
                            }
                            else
                            {
                                fmt.Fprintf(_addr_buf, "R%d-R%d", start, end);
                            }

                            start = -2L;
                            end = -2L;

                        }

                    }
;
                    for (long i = 0L; i < 16L; i++)
                    {
                        if (a & (1L << (int)(uint(i))) != 0L)
                        {
                            if (i == end + 1L)
                            {
                                end++;
                                continue;
                            }

                            start = i;
                            end = i;

                        }
                        else
                        {
                            flush();
                        }

                    }

                    flush();
                    fmt.Fprintf(_addr_buf, "]");
                    return buf.String();
                    break;
                case RegShift a:
                    return fmt.Sprintf("R%d%s$%d", int(a.Reg), plan9Shift[a.Shift], int(a.Count));
                    break;
                case RegShiftReg a:
                    return fmt.Sprintf("R%d%sR%d", int(a.Reg), plan9Shift[a.Shift], int(a.RegCount));
                    break;
            }
            return strings.ToUpper(arg.String());

        }

        // convert memory operand from GNU syntax to Plan 9 syntax, for example,
        // [r5] -> (R5)
        // [r6, #4080] -> 0xff0(R6)
        // [r2, r0, ror #1] -> (R2)(R0@>1)
        // inst [r2, -r0, ror #1] -> INST.U (R2)(R0@>1)
        // input:
        //   a memory operand
        // return values:
        //   corresponding memory operand in Plan 9 syntax
        //   .W/.P/.U suffix
        private static (@string, @string) memOpTrans(Mem mem)
        {
            @string _p0 = default;
            @string _p0 = default;

            @string suffix = "";

            if (mem.Mode == AddrOffset || mem.Mode == AddrLDM)             else if (mem.Mode == AddrPreIndex || mem.Mode == AddrLDM_WB) 
                suffix = ".W";
            else if (mem.Mode == AddrPostIndex) 
                suffix = ".P";
                        @string off = "";
            if (mem.Offset != 0L)
            {
                off = fmt.Sprintf("%#x", mem.Offset);
            }

            var @base = fmt.Sprintf("(R%d)", int(mem.Base));
            @string index = "";
            if (mem.Sign != 0L)
            {
                @string sign = "";
                if (mem.Sign < 0L)
                {
                    suffix += ".U";
                }

                @string shift = "";
                if (mem.Count != 0L)
                {
                    shift = fmt.Sprintf("%s%d", plan9Shift[mem.Shift], mem.Count);
                }

                index = fmt.Sprintf("(%sR%d%s)", sign, int(mem.Index), shift);

            }

            return (off + base + index, suffix);

        }

        private partial struct goFPInfo
        {
            public Op op;
            public slice<long> transArgs; // indexes of arguments which need transformation
            public @string gnuName; // instruction name in GNU syntax
            public @string goName; // instruction name in Plan 9 syntax
        }

        private static slice<goFPInfo> fpInst = new slice<goFPInfo>(new goFPInfo[] { {VADD_EQ_F32,[]int{2,1,0},"VADD","ADDF"}, {VADD_EQ_F64,[]int{2,1,0},"VADD","ADDD"}, {VSUB_EQ_F32,[]int{2,1,0},"VSUB","SUBF"}, {VSUB_EQ_F64,[]int{2,1,0},"VSUB","SUBD"}, {VMUL_EQ_F32,[]int{2,1,0},"VMUL","MULF"}, {VMUL_EQ_F64,[]int{2,1,0},"VMUL","MULD"}, {VNMUL_EQ_F32,[]int{2,1,0},"VNMUL","NMULF"}, {VNMUL_EQ_F64,[]int{2,1,0},"VNMUL","NMULD"}, {VMLA_EQ_F32,[]int{2,1,0},"VMLA","MULAF"}, {VMLA_EQ_F64,[]int{2,1,0},"VMLA","MULAD"}, {VMLS_EQ_F32,[]int{2,1,0},"VMLS","MULSF"}, {VMLS_EQ_F64,[]int{2,1,0},"VMLS","MULSD"}, {VNMLA_EQ_F32,[]int{2,1,0},"VNMLA","NMULAF"}, {VNMLA_EQ_F64,[]int{2,1,0},"VNMLA","NMULAD"}, {VNMLS_EQ_F32,[]int{2,1,0},"VNMLS","NMULSF"}, {VNMLS_EQ_F64,[]int{2,1,0},"VNMLS","NMULSD"}, {VDIV_EQ_F32,[]int{2,1,0},"VDIV","DIVF"}, {VDIV_EQ_F64,[]int{2,1,0},"VDIV","DIVD"}, {VNEG_EQ_F32,[]int{1,0},"VNEG","NEGF"}, {VNEG_EQ_F64,[]int{1,0},"VNEG","NEGD"}, {VABS_EQ_F32,[]int{1,0},"VABS","ABSF"}, {VABS_EQ_F64,[]int{1,0},"VABS","ABSD"}, {VSQRT_EQ_F32,[]int{1,0},"VSQRT","SQRTF"}, {VSQRT_EQ_F64,[]int{1,0},"VSQRT","SQRTD"}, {VCMP_EQ_F32,[]int{1,0},"VCMP","CMPF"}, {VCMP_EQ_F64,[]int{1,0},"VCMP","CMPD"}, {VCMP_E_EQ_F32,[]int{1,0},"VCMP.E","CMPF"}, {VCMP_E_EQ_F64,[]int{1,0},"VCMP.E","CMPD"}, {VLDR_EQ,[]int{1},"VLDR","MOV"}, {VSTR_EQ,[]int{1},"VSTR","MOV"}, {VMOV_EQ_F32,[]int{1,0},"VMOV","MOVF"}, {VMOV_EQ_F64,[]int{1,0},"VMOV","MOVD"}, {VMOV_EQ_32,[]int{1,0},"VMOV","MOVW"}, {VMOV_EQ,[]int{1,0},"VMOV","MOVW"}, {VCVT_EQ_F64_F32,[]int{1,0},"VCVT","MOVFD"}, {VCVT_EQ_F32_F64,[]int{1,0},"VCVT","MOVDF"}, {VCVT_EQ_F32_U32,[]int{1,0},"VCVT","MOVWF.U"}, {VCVT_EQ_F32_S32,[]int{1,0},"VCVT","MOVWF"}, {VCVT_EQ_S32_F32,[]int{1,0},"VCVT","MOVFW"}, {VCVT_EQ_U32_F32,[]int{1,0},"VCVT","MOVFW.U"}, {VCVT_EQ_F64_U32,[]int{1,0},"VCVT","MOVWD.U"}, {VCVT_EQ_F64_S32,[]int{1,0},"VCVT","MOVWD"}, {VCVT_EQ_S32_F64,[]int{1,0},"VCVT","MOVDW"}, {VCVT_EQ_U32_F64,[]int{1,0},"VCVT","MOVDW.U"} });

        // convert FP instructions from GNU syntax to Plan 9 syntax, for example,
        // vadd.f32 s0, s3, s4 -> ADDF F0, S3, F2
        // vsub.f64 d0, d2, d4 -> SUBD F0, F2, F4
        // vldr s2, [r11] -> MOVF (R11), F1
        // inputs: instruction name and arguments in GNU syntax
        // return values: corresponding instruction name and arguments in Plan 9 syntax
        private static (@string, slice<@string>) fpTrans(ptr<Inst> _addr_inst, @string op, slice<@string> args) => func((_, panic, __) =>
        {
            @string _p0 = default;
            slice<@string> _p0 = default;
            ref Inst inst = ref _addr_inst.val;

            foreach (var (_, fp) in fpInst)
            {
                if (inst.Op & ~15L == fp.op)
                { 
                    // remove gnu syntax suffixes
                    op = strings.Replace(op, ".F32", "", -1L);
                    op = strings.Replace(op, ".F64", "", -1L);
                    op = strings.Replace(op, ".S32", "", -1L);
                    op = strings.Replace(op, ".U32", "", -1L);
                    op = strings.Replace(op, ".32", "", -1L); 
                    // compose op name
                    if (fp.op == VLDR_EQ || fp.op == VSTR_EQ)
                    {

                        if (strings.HasPrefix(args[fp.transArgs[0L]], "D")) 
                            op = "MOVD" + op[len(fp.gnuName)..];
                        else if (strings.HasPrefix(args[fp.transArgs[0L]], "S")) 
                            op = "MOVF" + op[len(fp.gnuName)..];
                        else 
                            panic(fmt.Sprintf("wrong FP register: %v", inst));
                        
                    }
                    else
                    {
                        op = fp.goName + op[len(fp.gnuName)..];
                    } 
                    // transform registers
                    foreach (var (ix, ri) in fp.transArgs)
                    {

                        if (strings.HasSuffix(args[ri], "[1]")) // MOVW Rx, Dy[1]
                        {
                            break;
                            goto __switch_break0;
                        }
                        if (strings.HasSuffix(args[ri], "[0]")) // Dx[0] -> Fx
                        {
                            args[ri] = strings.Replace(args[ri], "[0]", "", -1L);
                            fallthrough = true;
                        }
                        if (fallthrough || strings.HasPrefix(args[ri], "D")) // Dx -> Fx
                        {
                            args[ri] = "F" + args[ri][1L..];
                            goto __switch_break0;
                        }
                        if (strings.HasPrefix(args[ri], "S"))
                        {
                            if (inst.Args[ix]._<Reg>() & 1L == 0L)
                            { // Sx -> Fy, y = x/2, if x is even
                                args[ri] = fmt.Sprintf("F%d", (inst.Args[ix]._<Reg>() - S0) / 2L);

                            }

                            goto __switch_break0;
                        }
                        if (strings.HasPrefix(args[ri], "$")) // CMPF/CMPD $0, Fx
                        {
                            break;
                            goto __switch_break0;
                        }
                        if (strings.HasPrefix(args[ri], "R")) // MOVW Rx, Dy[1]
                        {
                            break;
                            goto __switch_break0;
                        }
                        // default: 
                            panic(fmt.Sprintf("wrong FP register: %v", inst));

                        __switch_break0:;

                    }
                    break;

                }

            }
            return (op, args);

        });
    }
}}}}}}}
