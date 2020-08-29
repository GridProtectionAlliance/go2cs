// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package armasm -- go2cs converted at 2020 August 29 10:07:12 UTC
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
                args = append(args, plan9Arg(ref inst, pc, symname, a));
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

            if (inst.Op & ~15L == LDR_EQ || inst.Op & ~15L == LDRB_EQ || inst.Op & ~15L == LDRSB_EQ || inst.Op & ~15L == LDRH_EQ || inst.Op & ~15L == LDRSH_EQ || inst.Op & ~15L == STR_EQ || inst.Op & ~15L == STRB_EQ || inst.Op & ~15L == STRH_EQ || inst.Op & ~15L == VLDR_EQ || inst.Op & ~15L == VSTR_EQ) 
                (mem, _) = inst.Args[1L]._<Mem>();

                if (mem.Mode == AddrOffset || mem.Mode == AddrLDM)                 else if (mem.Mode == AddrPreIndex || mem.Mode == AddrLDM_WB) 
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
                args[1L] = off + base + index;
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
            
            if (inst.Op & ~15L == MOV_EQ) 
                op = "MOVW" + op[3L..];
            else if (inst.Op & ~15L == LDR_EQ) 
                op = "MOVW" + op[3L..] + suffix;
            else if (inst.Op & ~15L == LDRB_EQ) 
                op = "MOVBU" + op[4L..] + suffix;
            else if (inst.Op & ~15L == LDRSB_EQ) 
                op = "MOVBS" + op[5L..] + suffix;
            else if (inst.Op & ~15L == LDRH_EQ) 
                op = "MOVHU" + op[4L..] + suffix;
            else if (inst.Op & ~15L == LDRSH_EQ) 
                op = "MOVHS" + op[5L..] + suffix;
            else if (inst.Op & ~15L == VLDR_EQ) 

                if (strings.HasPrefix(args[1L], "D")) // VLDR.F64
                    op = "MOVD" + op[4L..] + suffix;
                    args[1L] = "F" + args[1L][1L..]; // Dx -> Fx
                else if (strings.HasPrefix(args[1L], "S")) // VLDR.F32
                    op = "MOVF" + op[4L..] + suffix;
                    if (inst.Args[0L]._<Reg>() & 1L == 0L)
                    { // Sx -> Fy, y = x/2, if x is even
                        args[1L] = fmt.Sprintf("F%d", (inst.Args[0L]._<Reg>() - S0) / 2L);
                    }
                else 
                    panic(fmt.Sprintf("wrong FP register: %v", inst));
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

                if (strings.HasPrefix(args[1L], "D")) // VSTR.F64
                    op = "MOVD" + op[4L..] + suffix;
                    args[1L] = "F" + args[1L][1L..]; // Dx -> Fx
                else if (strings.HasPrefix(args[1L], "S")) // VSTR.F32
                    op = "MOVF" + op[4L..] + suffix;
                    if (inst.Args[0L]._<Reg>() & 1L == 0L)
                    { // Sx -> Fy, y = x/2, if x is even
                        args[1L] = fmt.Sprintf("F%d", (inst.Args[0L]._<Reg>() - S0) / 2L);
                    }
                else 
                    panic(fmt.Sprintf("wrong FP register: %v", inst));
                                args[0L] = args[1L];
                args[1L] = args[0L];
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

        private static @string plan9Arg(ref Inst inst, ulong pc, Func<ulong, (@string, ulong)> symname, Arg arg)
        {
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
                    bytes.Buffer buf = default;
                    long start = -2L;
                    long end = -2L;
                    fmt.Fprintf(ref buf, "[");
                    Action flush = () =>
                    {
                        if (start >= 0L)
                        {
                            if (buf.Len() > 1L)
                            {
                                fmt.Fprintf(ref buf, ",");
                            }
                            if (start == end)
                            {
                                fmt.Fprintf(ref buf, "R%d", start);
                            }
                            else
                            {
                                fmt.Fprintf(ref buf, "R%d-R%d", start, end);
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
                    fmt.Fprintf(ref buf, "]");
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
    }
}}}}}}}
