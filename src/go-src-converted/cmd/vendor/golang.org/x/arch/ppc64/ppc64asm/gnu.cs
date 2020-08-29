// Copyright 2014 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64asm -- go2cs converted at 2020 August 29 10:07:53 UTC
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
        // GNUSyntax returns the GNU assembler syntax for the instruction, as defined by GNU binutils.
        // This form typically matches the syntax defined in the Power ISA Reference Manual.
        public static @string GNUSyntax(Inst inst)
        {
            bytes.Buffer buf = default;
            if (inst.Op == 0L)
            {
                return "error: unkown instruction";
            }
            buf.WriteString(inst.Op.String());
            @string sep = " ";
            foreach (var (i, arg) in inst.Args[..])
            {
                if (arg == null)
                {
                    break;
                }
                var text = gnuArg(ref inst, i, arg);
                if (text == "")
                {
                    continue;
                }
                buf.WriteString(sep);
                sep = ",";
                buf.WriteString(text);
            }            return buf.String();
        }

        // gnuArg formats arg (which is the argIndex's arg in inst) according to GNU rules.
        // NOTE: because GNUSyntax is the only caller of this func, and it receives a copy
        //       of inst, it's ok to modify inst.Args here.
        private static @string gnuArg(ref Inst _inst, long argIndex, Arg arg) => func(_inst, (ref Inst inst, Defer _, Panic panic, Recover __) =>
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
                    array<@string> bit = new array<@string>(new @string[] { "lt", "gt", "eq", "so" })[(arg - Cond0LT) % 4L];
                    if (arg <= Cond0SO)
                    {
                        return bit;
                    }
                    return fmt.Sprintf("4*cr%d+%s", int(arg - Cond0LT) / 4L, bit);
                    break;
                case Imm arg:
                    return fmt.Sprintf("%d", arg);
                    break;
                case SpReg arg:
                    return fmt.Sprintf("%d", int(arg));
                    break;
                case PCRel arg:
                    return fmt.Sprintf(".%+#x", int(arg));
                    break;
                case Label arg:
                    return fmt.Sprintf("%#x", uint32(arg));
                    break;
                case Offset arg:
                    Reg reg = inst.Args[argIndex + 1L]._<Reg>();
                    removeArg(inst, argIndex + 1L);
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
        private static void removeArg(ref Inst inst, long index)
        {
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
                        return false;
        }
    }
}}}}}}}
