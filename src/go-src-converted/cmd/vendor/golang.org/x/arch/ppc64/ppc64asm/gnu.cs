// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2020 October 09 05:54:55 UTC
// import "cmd/vendor/golang.org/x/arch/ppc64/ppc64asm" ==> using ppc64asm = go.cmd.vendor.golang.org.x.arch.ppc64.ppc64asm_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\arch\ppc64\ppc64asm\gnu.go
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
namespace ppc64
{
    public static partial class ppc64asm_package
    {
        private static array<@string> condBit = new array<@string>(new @string[] { "lt", "gt", "eq", "so" });        private static array<@string> condBitNeg = new array<@string>(new @string[] { "ge", "le", "ne", "so" });

        // GNUSyntax returns the GNU assembler syntax for the instruction, as defined by GNU binutils.
        // This form typically matches the syntax defined in the Power ISA Reference Manual.
        public static @string GNUSyntax(Inst inst, ulong pc)
        {
            bytes.Buffer buf = default; 
            // When there are all 0s, identify them as the disassembler
            // in binutils would.
            if (inst.Enc == 0L)
            {
                return ".long 0x0";
            }
            else if (inst.Op == 0L)
            {
                return "error: unknown instruction";
            }

            var PC = pc; 
            // Special handling for some ops
            long startArg = 0L;
            @string sep = " ";
            switch (inst.Op.String())
            {
                case "bc": 
                    var bo = gnuArg(_addr_inst, 0L, inst.Args[0L], PC);
                    var bi = inst.Args[1L];
                    switch (bi.type())
                    {
                        case CondReg bi:
                            if (bi >= CR0)
                            {
                                if (bi == CR0 && bo == "16")
                                {
                                    buf.WriteString("bdnz");
                                }

                                buf.WriteString(fmt.Sprintf("bc cr%d", bi - CR0));

                            }

                            var cr = bi / 4L;
                            switch (bo)
                            {
                                case "4": 
                                    var bit = condBitNeg[(bi - Cond0LT) % 4L];
                                    if (cr == 0L)
                                    {
                                        buf.WriteString(fmt.Sprintf("b%s", bit));
                                    }
                                    else
                                    {
                                        buf.WriteString(fmt.Sprintf("b%s cr%d,", bit, cr));
                                        sep = "";
                                    }

                                    break;
                                case "12": 
                                    bit = condBit[(bi - Cond0LT) % 4L];
                                    if (cr == 0L)
                                    {
                                        buf.WriteString(fmt.Sprintf("b%s", bit));
                                    }
                                    else
                                    {
                                        buf.WriteString(fmt.Sprintf("b%s cr%d,", bit, cr));
                                        sep = "";
                                    }

                                    break;
                                case "8": 
                                    bit = condBit[(bi - Cond0LT) % 4L];
                                    sep = "";
                                    if (cr == 0L)
                                    {
                                        buf.WriteString(fmt.Sprintf("bdnzt %s,", bit));
                                    }
                                    else
                                    {
                                        buf.WriteString(fmt.Sprintf("bdnzt cr%d,%s,", cr, bit));
                                    }

                                    break;
                                case "16": 
                                    if (cr == 0L && bi == Cond0LT)
                                    {
                                        buf.WriteString("bdnz");
                                    }
                                    else
                                    {
                                        buf.WriteString(fmt.Sprintf("bdnz cr%d,", cr));
                                        sep = "";
                                    }

                                    break;
                            }
                            startArg = 2L;
                            break;
                        default:
                        {
                            var bi = bi.type();
                            fmt.Printf("Unexpected bi: %d for bc with bo: %s\n", bi, bo);
                            break;
                        }
                    }
                    startArg = 2L;
                    break;
                case "mtspr": 
                    var opcode = inst.Op.String();
                    buf.WriteString(opcode[0L..2L]);
                    switch (inst.Args[0L].type())
                    {
                        case SpReg spr:
                            switch (spr)
                            {
                                case 1L: 
                                    buf.WriteString("xer");
                                    startArg = 1L;
                                    break;
                                case 8L: 
                                    buf.WriteString("lr");
                                    startArg = 1L;
                                    break;
                                case 9L: 
                                    buf.WriteString("ctr");
                                    startArg = 1L;
                                    break;
                                default: 
                                    buf.WriteString("spr");
                                    break;
                            }
                            break;
                        default:
                        {
                            var spr = inst.Args[0L].type();
                            buf.WriteString("spr");
                            break;
                        }

                    }
                    break;
                case "mfspr": 
                    opcode = inst.Op.String();
                    buf.WriteString(opcode[0L..2L]);
                    var arg = inst.Args[0L];
                    switch (inst.Args[1L].type())
                    {
                        case SpReg spr:
                            switch (spr)
                            {
                                case 1L: 
                                    buf.WriteString("xer ");
                                    buf.WriteString(gnuArg(_addr_inst, 0L, arg, PC));
                                    startArg = 2L;
                                    break;
                                case 8L: 
                                    buf.WriteString("lr ");
                                    buf.WriteString(gnuArg(_addr_inst, 0L, arg, PC));
                                    startArg = 2L;
                                    break;
                                case 9L: 
                                    buf.WriteString("ctr ");
                                    buf.WriteString(gnuArg(_addr_inst, 0L, arg, PC));
                                    startArg = 2L;
                                    break;
                                case 268L: 
                                    buf.WriteString("tb ");
                                    buf.WriteString(gnuArg(_addr_inst, 0L, arg, PC));
                                    startArg = 2L;
                                    break;
                                default: 
                                    buf.WriteString("spr");
                                    break;
                            }
                            break;
                        default:
                        {
                            var spr = inst.Args[1L].type();
                            buf.WriteString("spr");
                            break;
                        }

                    }
                    break;
                case "sync": 
                    switch (inst.Args[0L].type())
                    {
                        case Imm arg:
                            switch (arg)
                            {
                                case 0L: 
                                    buf.WriteString("hwsync");
                                    break;
                                case 1L: 
                                    buf.WriteString("lwsync");
                                    break;
                                case 2L: 
                                    buf.WriteString("ptesync");
                                    break;
                            }
                            break;
                    }
                    startArg = 2L;
                    break;
                default: 
                    buf.WriteString(inst.Op.String());
                    break;
            }
            {
                var arg__prev1 = arg;

                foreach (var (__i, __arg) in inst.Args[..])
                {
                    i = __i;
                    arg = __arg;
                    if (arg == null)
                    {
                        break;
                    }

                    if (i < startArg)
                    {
                        continue;
                    }

                    var text = gnuArg(_addr_inst, i, arg, PC);
                    if (text == "")
                    {
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
        private static @string gnuArg(ptr<Inst> _addr_inst, long argIndex, Arg arg, ulong pc) => func((_, panic, __) =>
        {
            ref Inst inst = ref _addr_inst.val;
 
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

                    return arg.String();
                    break;
                case CondReg arg:
                    if (arg == CR0 && strings.HasPrefix(inst.Op.String(), "cmp"))
                    {
                        return ""; // don't show cr0 for cmp instructions
                    }
                    else if (arg >= CR0)
                    {
                        return fmt.Sprintf("cr%d", int(arg - CR0));
                    }

                    var bit = condBit[(arg - Cond0LT) % 4L];
                    if (arg <= Cond0SO)
                    {
                        return bit;
                    }

                    return fmt.Sprintf("%s cr%d", bit, int(arg - Cond0LT) / 4L);
                    break;
                case Imm arg:
                    return fmt.Sprintf("%d", arg);
                    break;
                case SpReg arg:
                    switch (int(arg))
                    {
                        case 1L: 
                            return "xer";
                            break;
                        case 8L: 
                            return "lr";
                            break;
                        case 9L: 
                            return "ctr";
                            break;
                        case 268L: 
                            return "tb";
                            break;
                        default: 
                            return fmt.Sprintf("%d", int(arg));
                            break;
                    }
                    break;
                case PCRel arg:
                    if (int(arg) == 0L)
                    {
                        return fmt.Sprintf(".%+#x", int(arg));
                    }

                    var addr = pc + uint64(int64(arg));
                    return fmt.Sprintf("%#x", addr);
                    break;
                case Label arg:
                    return fmt.Sprintf("%#x", uint32(arg));
                    break;
                case Offset arg:
                    Reg reg = inst.Args[argIndex + 1L]._<Reg>();
                    removeArg(_addr_inst, argIndex + 1L);
                    if (reg == R0)
                    {
                        return fmt.Sprintf("%d(0)", int(arg));
                    }

                    return fmt.Sprintf("%d(r%d)", int(arg), reg - R0);
                    break;
            }
            return fmt.Sprintf("???(%v)", arg);

        });

        // removeArg removes the arg in inst.Args[index].
        private static void removeArg(ptr<Inst> _addr_inst, long index)
        {
            ref Inst inst = ref _addr_inst.val;

            for (var i = index; i < len(inst.Args); i++)
            {
                if (i + 1L < len(inst.Args))
                {
                    inst.Args[i] = inst.Args[i + 1L];
                }
                else
                {
                    inst.Args[i] = null;
                }

            }


        }

        // isLoadStoreOp returns true if op is a load or store instruction
        private static bool isLoadStoreOp(Op op)
        {

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
    }
}}}}}}}
