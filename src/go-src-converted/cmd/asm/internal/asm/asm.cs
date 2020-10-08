// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package asm -- go2cs converted at 2020 October 08 04:04:36 UTC
// import "cmd/asm/internal/asm" ==> using asm = go.cmd.asm.@internal.asm_package
// Original source: C:\Go\src\cmd\asm\internal\asm\asm.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using scanner = go.text.scanner_package;

using arch = go.cmd.asm.@internal.arch_package;
using flags = go.cmd.asm.@internal.flags_package;
using lex = go.cmd.asm.@internal.lex_package;
using obj = go.cmd.@internal.obj_package;
using x86 = go.cmd.@internal.obj.x86_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class asm_package
    {
        // TODO: configure the architecture
        private static ptr<bytes.Buffer> testOut; // Gathers output when testing.

        // append adds the Prog to the end of the program-thus-far.
        // If doLabel is set, it also defines the labels collect for this Prog.
        private static void append(this ptr<Parser> _addr_p, ptr<obj.Prog> _addr_prog, @string cond, bool doLabel)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Prog prog = ref _addr_prog.val;

            if (cond != "")
            {

                if (p.arch.Family == sys.ARM) 
                    if (!arch.ARMConditionCodes(prog, cond))
                    {
                        p.errorf("unrecognized condition code .%q", cond);
                        return ;
                    }

                else if (p.arch.Family == sys.ARM64) 
                    if (!arch.ARM64Suffix(prog, cond))
                    {
                        p.errorf("unrecognized suffix .%q", cond);
                        return ;
                    }

                else if (p.arch.Family == sys.AMD64 || p.arch.Family == sys.I386) 
                    {
                        var err = x86.ParseSuffix(prog, cond);

                        if (err != null)
                        {
                            p.errorf("%v", err);
                            return ;
                        }

                    }


                else 
                    p.errorf("unrecognized suffix .%q", cond);
                    return ;
                
            }

            if (p.firstProg == null)
            {
                p.firstProg = prog;
            }
            else
            {
                p.lastProg.Link = prog;
            }

            p.lastProg = prog;
            if (doLabel)
            {
                p.pc++;
                foreach (var (_, label) in p.pendingLabels)
                {
                    if (p.labels[label] != null)
                    {
                        p.errorf("label %q multiply defined", label);
                        return ;
                    }

                    p.labels[label] = prog;

                }
                p.pendingLabels = p.pendingLabels[0L..0L];

            }

            prog.Pc = p.pc;
            if (flags.Debug.val)
            {
                fmt.Println(p.lineNum, prog);
            }

            if (testOut != null)
            {
                fmt.Fprintln(testOut, prog);
            }

        }

        // validSymbol checks that addr represents a valid name for a pseudo-op.
        private static bool validSymbol(this ptr<Parser> _addr_p, @string pseudo, ptr<obj.Addr> _addr_addr, bool offsetOk)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Sym == null || addr.Name != obj.NAME_EXTERN && addr.Name != obj.NAME_STATIC || addr.Scale != 0L || addr.Reg != 0L)
            {
                p.errorf("%s symbol %q must be a symbol(SB)", pseudo, symbolName(_addr_addr));
                return false;
            }

            if (!offsetOk && addr.Offset != 0L)
            {
                p.errorf("%s symbol %q must not be offset from SB", pseudo, symbolName(_addr_addr));
                return false;
            }

            return true;

        }

        // evalInteger evaluates an integer constant for a pseudo-op.
        private static long evalInteger(this ptr<Parser> _addr_p, @string pseudo, slice<lex.Token> operands)
        {
            ref Parser p = ref _addr_p.val;

            ref var addr = ref heap(p.address(operands), out ptr<var> _addr_addr);
            return p.getConstantPseudo(pseudo, _addr_addr);
        }

        // validImmediate checks that addr represents an immediate constant.
        private static bool validImmediate(this ptr<Parser> _addr_p, @string pseudo, ptr<obj.Addr> _addr_addr)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Type != obj.TYPE_CONST || addr.Name != 0L || addr.Reg != 0L || addr.Index != 0L)
            {
                p.errorf("%s: expected immediate constant; found %s", pseudo, obj.Dconv(_addr_emptyProg, addr));
                return false;
            }

            return true;

        }

        // asmText assembles a TEXT pseudo-op.
        // TEXT runtime·sigtramp(SB),4,$0-0
        private static void asmText(this ptr<Parser> _addr_p, slice<slice<lex.Token>> operands)
        {
            ref Parser p = ref _addr_p.val;

            if (len(operands) != 2L && len(operands) != 3L)
            {
                p.errorf("expect two or three operands for TEXT");
                return ;
            } 

            // Labels are function scoped. Patch existing labels and
            // create a new label space for this TEXT.
            p.patch();
            p.labels = make_map<@string, ptr<obj.Prog>>(); 

            // Operand 0 is the symbol name in the form foo(SB).
            // That means symbol plus indirect on SB and no offset.
            ref var nameAddr = ref heap(p.address(operands[0L]), out ptr<var> _addr_nameAddr);
            if (!p.validSymbol("TEXT", _addr_nameAddr, false))
            {
                return ;
            }

            var name = symbolName(_addr_nameAddr);
            long next = 1L; 

            // Next operand is the optional text flag, a literal integer.
            var flag = int64(0L);
            if (len(operands) == 3L)
            {
                flag = p.evalInteger("TEXT", operands[1L]);
                next++;
            } 

            // Next operand is the frame and arg size.
            // Bizarre syntax: $frameSize-argSize is two words, not subtraction.
            // Both frameSize and argSize must be simple integers; only frameSize
            // can be negative.
            // The "-argSize" may be missing; if so, set it to objabi.ArgsSizeUnknown.
            // Parse left to right.
            var op = operands[next];
            if (len(op) < 2L || op[0L].ScanToken != '$')
            {
                p.errorf("TEXT %s: frame size must be an immediate constant", name);
                return ;
            }

            op = op[1L..];
            var negative = false;
            if (op[0L].ScanToken == '-')
            {
                negative = true;
                op = op[1L..];
            }

            if (len(op) == 0L || op[0L].ScanToken != scanner.Int)
            {
                p.errorf("TEXT %s: frame size must be an immediate constant", name);
                return ;
            }

            var frameSize = p.positiveAtoi(op[0L].String());
            if (negative)
            {
                frameSize = -frameSize;
            }

            op = op[1L..];
            var argSize = int64(objabi.ArgsSizeUnknown);
            if (len(op) > 0L)
            { 
                // There is an argument size. It must be a minus sign followed by a non-negative integer literal.
                if (len(op) != 2L || op[0L].ScanToken != '-' || op[1L].ScanToken != scanner.Int)
                {
                    p.errorf("TEXT %s: argument size must be of form -integer", name);
                    return ;
                }

                argSize = p.positiveAtoi(op[1L].String());

            }

            p.ctxt.InitTextSym(nameAddr.Sym, int(flag));
            ptr<obj.Prog> prog = addr(new obj.Prog(Ctxt:p.ctxt,As:obj.ATEXT,Pos:p.pos(),From:nameAddr,To:obj.Addr{Type:obj.TYPE_TEXTSIZE,Offset:frameSize,},));
            nameAddr.Sym.Func.Text = prog;
            prog.To.Val = int32(argSize);
            p.append(prog, "", true);

        }

        // asmData assembles a DATA pseudo-op.
        // DATA masks<>+0x00(SB)/4, $0x00000000
        private static void asmData(this ptr<Parser> _addr_p, slice<slice<lex.Token>> operands)
        {
            ref Parser p = ref _addr_p.val;

            if (len(operands) != 2L)
            {
                p.errorf("expect two operands for DATA");
                return ;
            } 

            // Operand 0 has the general form foo<>+0x04(SB)/4.
            var op = operands[0L];
            var n = len(op);
            if (n < 3L || op[n - 2L].ScanToken != '/' || op[n - 1L].ScanToken != scanner.Int)
            {
                p.errorf("expect /size for DATA argument");
                return ;
            }

            var szop = op[n - 1L].String();
            var (sz, err) = strconv.Atoi(szop);
            if (err != null)
            {
                p.errorf("bad size for DATA argument: %q", szop);
            }

            op = op[..n - 2L];
            ref var nameAddr = ref heap(p.address(op), out ptr<var> _addr_nameAddr);
            if (!p.validSymbol("DATA", _addr_nameAddr, true))
            {
                return ;
            }

            var name = symbolName(_addr_nameAddr); 

            // Operand 1 is an immediate constant or address.
            var valueAddr = p.address(operands[1L]);

            if (valueAddr.Type == obj.TYPE_CONST || valueAddr.Type == obj.TYPE_FCONST || valueAddr.Type == obj.TYPE_SCONST || valueAddr.Type == obj.TYPE_ADDR)             else 
                p.errorf("DATA value must be an immediate constant or address");
                return ;
            // The addresses must not overlap. Easiest test: require monotonicity.
            {
                var (lastAddr, ok) = p.dataAddr[name];

                if (ok && nameAddr.Offset < lastAddr)
                {
                    p.errorf("overlapping DATA entry for %s", name);
                    return ;
                }

            }

            p.dataAddr[name] = nameAddr.Offset + int64(sz);


            if (valueAddr.Type == obj.TYPE_CONST) 
                switch (sz)
                {
                    case 1L: 

                    case 2L: 

                    case 4L: 

                    case 8L: 
                        nameAddr.Sym.WriteInt(p.ctxt, nameAddr.Offset, int(sz), valueAddr.Offset);
                        break;
                    default: 
                        p.errorf("bad int size for DATA argument: %d", sz);
                        break;
                }
            else if (valueAddr.Type == obj.TYPE_FCONST) 
                switch (sz)
                {
                    case 4L: 
                        nameAddr.Sym.WriteFloat32(p.ctxt, nameAddr.Offset, float32(valueAddr.Val._<double>()));
                        break;
                    case 8L: 
                        nameAddr.Sym.WriteFloat64(p.ctxt, nameAddr.Offset, valueAddr.Val._<double>());
                        break;
                    default: 
                        p.errorf("bad float size for DATA argument: %d", sz);
                        break;
                }
            else if (valueAddr.Type == obj.TYPE_SCONST) 
                nameAddr.Sym.WriteString(p.ctxt, nameAddr.Offset, int(sz), valueAddr.Val._<@string>());
            else if (valueAddr.Type == obj.TYPE_ADDR) 
                if (sz == p.arch.PtrSize)
                {
                    nameAddr.Sym.WriteAddr(p.ctxt, nameAddr.Offset, int(sz), valueAddr.Sym, valueAddr.Offset);
                }
                else
                {
                    p.errorf("bad addr size for DATA argument: %d", sz);
                }

                    }

        // asmGlobl assembles a GLOBL pseudo-op.
        // GLOBL shifts<>(SB),8,$256
        // GLOBL shifts<>(SB),$256
        private static void asmGlobl(this ptr<Parser> _addr_p, slice<slice<lex.Token>> operands)
        {
            ref Parser p = ref _addr_p.val;

            if (len(operands) != 2L && len(operands) != 3L)
            {
                p.errorf("expect two or three operands for GLOBL");
                return ;
            } 

            // Operand 0 has the general form foo<>+0x04(SB).
            ref var nameAddr = ref heap(p.address(operands[0L]), out ptr<var> _addr_nameAddr);
            if (!p.validSymbol("GLOBL", _addr_nameAddr, false))
            {
                return ;
            }

            long next = 1L; 

            // Next operand is the optional flag, a literal integer.
            var flag = int64(0L);
            if (len(operands) == 3L)
            {
                flag = p.evalInteger("GLOBL", operands[1L]);
                next++;
            } 

            // Final operand is an immediate constant.
            ref var addr = ref heap(p.address(operands[next]), out ptr<var> _addr_addr);
            if (!p.validImmediate("GLOBL", _addr_addr))
            {
                return ;
            } 

            // log.Printf("GLOBL %s %d, $%d", name, flag, size)
            p.ctxt.Globl(nameAddr.Sym, addr.Offset, int(flag));

        }

        // asmPCData assembles a PCDATA pseudo-op.
        // PCDATA $2, $705
        private static void asmPCData(this ptr<Parser> _addr_p, slice<slice<lex.Token>> operands)
        {
            ref Parser p = ref _addr_p.val;

            if (len(operands) != 2L)
            {
                p.errorf("expect two operands for PCDATA");
                return ;
            } 

            // Operand 0 must be an immediate constant.
            ref var key = ref heap(p.address(operands[0L]), out ptr<var> _addr_key);
            if (!p.validImmediate("PCDATA", _addr_key))
            {
                return ;
            } 

            // Operand 1 must be an immediate constant.
            ref var value = ref heap(p.address(operands[1L]), out ptr<var> _addr_value);
            if (!p.validImmediate("PCDATA", _addr_value))
            {
                return ;
            } 

            // log.Printf("PCDATA $%d, $%d", key.Offset, value.Offset)
            ptr<obj.Prog> prog = addr(new obj.Prog(Ctxt:p.ctxt,As:obj.APCDATA,Pos:p.pos(),From:key,To:value,));
            p.append(prog, "", true);

        }

        // asmPCAlign assembles a PCALIGN pseudo-op.
        // PCALIGN $16
        private static void asmPCAlign(this ptr<Parser> _addr_p, slice<slice<lex.Token>> operands)
        {
            ref Parser p = ref _addr_p.val;

            if (len(operands) != 1L)
            {
                p.errorf("expect one operand for PCALIGN");
                return ;
            } 

            // Operand 0 must be an immediate constant.
            ref var key = ref heap(p.address(operands[0L]), out ptr<var> _addr_key);
            if (!p.validImmediate("PCALIGN", _addr_key))
            {
                return ;
            }

            ptr<obj.Prog> prog = addr(new obj.Prog(Ctxt:p.ctxt,As:obj.APCALIGN,From:key,));
            p.append(prog, "", true);

        }

        // asmFuncData assembles a FUNCDATA pseudo-op.
        // FUNCDATA $1, funcdata<>+4(SB)
        private static void asmFuncData(this ptr<Parser> _addr_p, slice<slice<lex.Token>> operands)
        {
            ref Parser p = ref _addr_p.val;

            if (len(operands) != 2L)
            {
                p.errorf("expect two operands for FUNCDATA");
                return ;
            } 

            // Operand 0 must be an immediate constant.
            ref var valueAddr = ref heap(p.address(operands[0L]), out ptr<var> _addr_valueAddr);
            if (!p.validImmediate("FUNCDATA", _addr_valueAddr))
            {
                return ;
            } 

            // Operand 1 is a symbol name in the form foo(SB).
            ref var nameAddr = ref heap(p.address(operands[1L]), out ptr<var> _addr_nameAddr);
            if (!p.validSymbol("FUNCDATA", _addr_nameAddr, true))
            {
                return ;
            }

            ptr<obj.Prog> prog = addr(new obj.Prog(Ctxt:p.ctxt,As:obj.AFUNCDATA,Pos:p.pos(),From:valueAddr,To:nameAddr,));
            p.append(prog, "", true);

        }

        // asmJump assembles a jump instruction.
        // JMP    R1
        // JMP    exit
        // JMP    3(PC)
        private static void asmJump(this ptr<Parser> _addr_p, obj.As op, @string cond, slice<obj.Addr> a)
        {
            ref Parser p = ref _addr_p.val;

            ptr<obj.Addr> target;
            ptr<obj.Prog> prog = addr(new obj.Prog(Ctxt:p.ctxt,Pos:p.pos(),As:op,));
            switch (len(a))
            {
                case 0L: 
                    if (p.arch.Family == sys.Wasm)
                    {
                        target = addr(new obj.Addr(Type:obj.TYPE_NONE));
                        break;
                    }

                    p.errorf("wrong number of arguments to %s instruction", op);
                    return ;
                    break;
                case 1L: 
                    target = _addr_a[0L];
                    break;
                case 2L: 
                    // Special 2-operand jumps.
                    target = _addr_a[1L];
                    prog.From = a[0L];
                    break;
                case 3L: 
                    if (p.arch.Family == sys.PPC64)
                    { 
                        // Special 3-operand jumps.
                        // First two must be constants; a[1] is a register number.
                        target = _addr_a[2L];
                        prog.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:p.getConstant(prog,op,&a[0]),);
                        var reg = int16(p.getConstant(prog, op, _addr_a[1L]));
                        var (reg, ok) = p.arch.RegisterNumber("R", reg);
                        if (!ok)
                        {
                            p.errorf("bad register number %d", reg);
                            return ;
                        }

                        prog.Reg = reg;
                        break;

                    }

                    if (p.arch.Family == sys.MIPS || p.arch.Family == sys.MIPS64 || p.arch.Family == sys.RISCV64)
                    { 
                        // 3-operand jumps.
                        // First two must be registers
                        target = _addr_a[2L];
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        break;

                    }

                    if (p.arch.Family == sys.S390X)
                    { 
                        // 3-operand jumps.
                        target = _addr_a[2L];
                        prog.From = a[0L];
                        if (a[1L].Reg != 0L)
                        { 
                            // Compare two registers and jump.
                            prog.Reg = p.getRegister(prog, op, _addr_a[1L]);

                        }
                        else
                        { 
                            // Compare register with immediate and jump.
                            prog.SetFrom3(a[1L]);

                        }

                        break;

                    }

                    if (p.arch.Family == sys.ARM64)
                    { 
                        // Special 3-operand jumps.
                        // a[0] must be immediate constant; a[1] is a register.
                        if (a[0L].Type != obj.TYPE_CONST)
                        {
                            p.errorf("%s: expected immediate constant; found %s", op, obj.Dconv(prog, _addr_a[0L]));
                            return ;
                        }

                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        target = _addr_a[2L];
                        break;

                    }

                    p.errorf("wrong number of arguments to %s instruction", op);
                    return ;
                    break;
                case 4L: 
                    if (p.arch.Family == sys.S390X)
                    { 
                        // 4-operand compare-and-branch.
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        prog.SetFrom3(a[2L]);
                        target = _addr_a[3L];
                        break;

                    }

                    p.errorf("wrong number of arguments to %s instruction", op);
                    return ;
                    break;
                default: 
                    p.errorf("wrong number of arguments to %s instruction", op);
                    return ;
                    break;
            }

            if (target.Type == obj.TYPE_BRANCH) 
                // JMP 4(PC)
                prog.To = new obj.Addr(Type:obj.TYPE_BRANCH,Offset:p.pc+1+target.Offset,);
            else if (target.Type == obj.TYPE_REG) 
                // JMP R1
                prog.To = target.val;
            else if (target.Type == obj.TYPE_MEM && (target.Name == obj.NAME_EXTERN || target.Name == obj.NAME_STATIC)) 
                // JMP main·morestack(SB)
                prog.To = target.val;
            else if (target.Type == obj.TYPE_INDIR && (target.Name == obj.NAME_EXTERN || target.Name == obj.NAME_STATIC)) 
                // JMP *main·morestack(SB)
                prog.To = target.val;
                prog.To.Type = obj.TYPE_INDIR;
            else if (target.Type == obj.TYPE_MEM && target.Reg == 0L && target.Offset == 0L) 
                // JMP exit
                if (target.Sym == null)
                { 
                    // Parse error left name unset.
                    return ;

                }

                var targetProg = p.labels[target.Sym.Name];
                if (targetProg == null)
                {
                    p.toPatch = append(p.toPatch, new Patch(prog,target.Sym.Name));
                }
                else
                {
                    p.branch(prog, targetProg);
                }

            else if (target.Type == obj.TYPE_MEM && target.Name == obj.NAME_NONE) 
                // JMP 4(R0)
                prog.To = target.val; 
                // On the ppc64, 9a encodes BR (CTR) as BR CTR. We do the same.
                if (p.arch.Family == sys.PPC64 && target.Offset == 0L)
                {
                    prog.To.Type = obj.TYPE_REG;
                }

            else if (target.Type == obj.TYPE_CONST) 
                // JMP $4
                prog.To = a[0L];
            else if (target.Type == obj.TYPE_NONE)             else 
                p.errorf("cannot assemble jump %+v", target);
                return ;
                        p.append(prog, cond, true);

        }

        private static void patch(this ptr<Parser> _addr_p)
        {
            ref Parser p = ref _addr_p.val;

            foreach (var (_, patch) in p.toPatch)
            {
                var targetProg = p.labels[patch.label];
                if (targetProg == null)
                {
                    p.errorf("undefined label %s", patch.label);
                    return ;
                }

                p.branch(patch.prog, targetProg);

            }
            p.toPatch = p.toPatch[..0L];

        }

        private static void branch(this ptr<Parser> _addr_p, ptr<obj.Prog> _addr_jmp, ptr<obj.Prog> _addr_target)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Prog jmp = ref _addr_jmp.val;
            ref obj.Prog target = ref _addr_target.val;

            jmp.To = new obj.Addr(Type:obj.TYPE_BRANCH,Index:0,);
            jmp.To.Val = target;
        }

        // asmInstruction assembles an instruction.
        // MOVW R9, (R10)
        private static void asmInstruction(this ptr<Parser> _addr_p, obj.As op, @string cond, slice<obj.Addr> a)
        {
            ref Parser p = ref _addr_p.val;
 
            // fmt.Printf("%s %+v\n", op, a)
            ptr<obj.Prog> prog = addr(new obj.Prog(Ctxt:p.ctxt,Pos:p.pos(),As:op,));

            if (len(a) == 0L)
            {
                goto __switch_break0;
            }
            if (len(a) == 1L)
            {
                if (p.arch.UnaryDst[op] || op == obj.ARET || op == obj.AGETCALLERPC)
                { 
                    // prog.From is no address.
                    prog.To = a[0L];

                }
                else
                {
                    prog.From = a[0L]; 
                    // prog.To is no address.
                }

                if (p.arch.Family == sys.PPC64 && arch.IsPPC64NEG(op))
                { 
                    // NEG: From and To are both a[0].
                    prog.To = a[0L];
                    prog.From = a[0L];
                    break;

                }

                goto __switch_break0;
            }
            if (len(a) == 2L)
            {
                if (p.arch.Family == sys.ARM)
                {
                    if (arch.IsARMCMP(op))
                    {
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        break;
                    } 
                    // Strange special cases.
                    if (arch.IsARMFloatCmp(op))
                    {
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        break;
                    }

                }
                else if (p.arch.Family == sys.ARM64 && arch.IsARM64CMP(op))
                {
                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    break;
                }
                else if (p.arch.Family == sys.MIPS || p.arch.Family == sys.MIPS64)
                {
                    if (arch.IsMIPSCMP(op) || arch.IsMIPSMUL(op))
                    {
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        break;
                    }

                }

                prog.From = a[0L];
                prog.To = a[1L];
                goto __switch_break0;
            }
            if (len(a) == 3L)
            {

                if (p.arch.Family == sys.MIPS || p.arch.Family == sys.MIPS64) 
                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    prog.To = a[2L];
                else if (p.arch.Family == sys.ARM) 
                    // Special cases.
                    if (arch.IsARMSTREX(op))
                    {
                        /*
                                            STREX x, (y), z
                                                from=(y) reg=x to=z
                                        */
                        prog.From = a[1L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[0L]);
                        prog.To = a[2L];
                        break;

                    }

                    if (arch.IsARMBFX(op))
                    { 
                        // a[0] and a[1] must be constants, a[2] must be a register
                        prog.From = a[0L];
                        prog.SetFrom3(a[1L]);
                        prog.To = a[2L];
                        break;

                    } 
                    // Otherwise the 2nd operand (a[1]) must be a register.
                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    prog.To = a[2L];
                else if (p.arch.Family == sys.AMD64) 
                    prog.From = a[0L];
                    prog.SetFrom3(a[1L]);
                    prog.To = a[2L];
                else if (p.arch.Family == sys.ARM64) 
                    // ARM64 instructions with one input and two outputs.
                    if (arch.IsARM64STLXR(op))
                    {
                        prog.From = a[0L];
                        prog.To = a[1L];
                        if (a[2L].Type != obj.TYPE_REG)
                        {
                            p.errorf("invalid addressing modes for third operand to %s instruction, must be register", op);
                            return ;
                        }

                        prog.RegTo2 = a[2L].Reg;
                        break;

                    }

                    if (arch.IsARM64TBL(op))
                    {
                        prog.From = a[0L];
                        if (a[1L].Type != obj.TYPE_REGLIST)
                        {
                            p.errorf("%s: expected list; found %s", op, obj.Dconv(prog, _addr_a[1L]));
                        }

                        prog.SetFrom3(a[1L]);
                        prog.To = a[2L];
                        break;

                    }

                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    prog.To = a[2L];
                else if (p.arch.Family == sys.I386) 
                    prog.From = a[0L];
                    prog.SetFrom3(a[1L]);
                    prog.To = a[2L];
                else if (p.arch.Family == sys.PPC64) 
                    if (arch.IsPPC64CMP(op))
                    { 
                        // CMPW etc.; third argument is a CR register that goes into prog.Reg.
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[2L]);
                        prog.To = a[1L];
                        break;

                    } 
                    // Arithmetic. Choices are:
                    // reg reg reg
                    // imm reg reg
                    // reg imm reg
                    // If the immediate is the middle argument, use From3.

                    if (a[1L].Type == obj.TYPE_REG) 
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        prog.To = a[2L];
                    else if (a[1L].Type == obj.TYPE_CONST) 
                        prog.From = a[0L];
                        prog.SetFrom3(a[1L]);
                        prog.To = a[2L];
                    else 
                        p.errorf("invalid addressing modes for %s instruction", op);
                        return ;
                                    else if (p.arch.Family == sys.RISCV64) 
                    // RISCV64 instructions with one input and two outputs.
                    if (arch.IsRISCV64AMO(op))
                    {
                        prog.From = a[0L];
                        prog.To = a[1L];
                        if (a[2L].Type != obj.TYPE_REG)
                        {
                            p.errorf("invalid addressing modes for third operand to %s instruction, must be register", op);
                            return ;
                        }

                        prog.RegTo2 = a[2L].Reg;
                        break;

                    }

                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    prog.To = a[2L];
                else if (p.arch.Family == sys.S390X) 
                    prog.From = a[0L];
                    if (a[1L].Type == obj.TYPE_REG)
                    {
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    }
                    else
                    {
                        prog.SetFrom3(a[1L]);
                    }

                    prog.To = a[2L];
                else 
                    p.errorf("TODO: implement three-operand instructions for this architecture");
                    return ;
                                goto __switch_break0;
            }
            if (len(a) == 4L)
            {
                if (p.arch.Family == sys.ARM)
                {
                    if (arch.IsARMBFX(op))
                    { 
                        // a[0] and a[1] must be constants, a[2] and a[3] must be registers
                        prog.From = a[0L];
                        prog.SetFrom3(a[1L]);
                        prog.Reg = p.getRegister(prog, op, _addr_a[2L]);
                        prog.To = a[3L];
                        break;

                    }

                    if (arch.IsARMMULA(op))
                    { 
                        // All must be registers.
                        p.getRegister(prog, op, _addr_a[0L]);
                        var r1 = p.getRegister(prog, op, _addr_a[1L]);
                        var r2 = p.getRegister(prog, op, _addr_a[2L]);
                        p.getRegister(prog, op, _addr_a[3L]);
                        prog.From = a[0L];
                        prog.To = a[3L];
                        prog.To.Type = obj.TYPE_REGREG2;
                        prog.To.Offset = int64(r2);
                        prog.Reg = r1;
                        break;

                    }

                }

                if (p.arch.Family == sys.AMD64)
                {
                    prog.From = a[0L];
                    prog.RestArgs = new slice<obj.Addr>(new obj.Addr[] { a[1], a[2] });
                    prog.To = a[3L];
                    break;
                }

                if (p.arch.Family == sys.ARM64)
                {
                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    prog.SetFrom3(a[2L]);
                    prog.To = a[3L];
                    break;
                }

                if (p.arch.Family == sys.PPC64)
                {
                    if (arch.IsPPC64RLD(op))
                    {
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        prog.SetFrom3(a[2L]);
                        prog.To = a[3L];
                        break;
                    }
                    else if (arch.IsPPC64ISEL(op))
                    { 
                        // ISEL BC,RB,RA,RT becomes isel rt,ra,rb,bc
                        prog.SetFrom3(a[2L]); // ra
                        prog.From = a[0L]; // bc
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]); // rb
                        prog.To = a[3L]; // rt
                        break;

                    } 
                    // Else, it is a VA-form instruction
                    // reg reg reg reg
                    // imm reg reg reg
                    // Or a VX-form instruction
                    // imm imm reg reg
                    if (a[1L].Type == obj.TYPE_REG)
                    {
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                        prog.SetFrom3(a[2L]);
                        prog.To = a[3L];
                        break;
                    }
                    else if (a[1L].Type == obj.TYPE_CONST)
                    {
                        prog.From = a[0L];
                        prog.Reg = p.getRegister(prog, op, _addr_a[2L]);
                        prog.SetFrom3(a[1L]);
                        prog.To = a[3L];
                        break;
                    }
                    else
                    {
                        p.errorf("invalid addressing modes for %s instruction", op);
                        return ;
                    }

                }

                if (p.arch.Family == sys.S390X)
                {
                    if (a[1L].Type != obj.TYPE_REG)
                    {
                        p.errorf("second operand must be a register in %s instruction", op);
                        return ;
                    }

                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    prog.SetFrom3(a[2L]);
                    prog.To = a[3L];
                    break;

                }

                p.errorf("can't handle %s instruction with 4 operands", op);
                return ;
                goto __switch_break0;
            }
            if (len(a) == 5L)
            {
                if (p.arch.Family == sys.PPC64 && arch.IsPPC64RLD(op))
                { 
                    // Always reg, reg, con, con, reg.  (con, con is a 'mask').
                    prog.From = a[0L];
                    prog.Reg = p.getRegister(prog, op, _addr_a[1L]);
                    var mask1 = p.getConstant(prog, op, _addr_a[2L]);
                    var mask2 = p.getConstant(prog, op, _addr_a[3L]);
                    uint mask = default;
                    if (mask1 < mask2)
                    {
                        mask = (~uint32(0L) >> (int)(uint(mask1))) & (~uint32(0L) << (int)(uint(31L - mask2)));
                    }
                    else
                    {
                        mask = (~uint32(0L) >> (int)(uint(mask2 + 1L))) & (~uint32(0L) << (int)(uint(31L - (mask1 - 1L))));
                    }

                    prog.SetFrom3(new obj.Addr(Type:obj.TYPE_CONST,Offset:int64(mask),));
                    prog.To = a[4L];
                    break;

                }

                if (p.arch.Family == sys.AMD64)
                {
                    prog.From = a[0L];
                    prog.RestArgs = new slice<obj.Addr>(new obj.Addr[] { a[1], a[2], a[3] });
                    prog.To = a[4L];
                    break;
                }

                if (p.arch.Family == sys.S390X)
                {
                    prog.From = a[0L];
                    prog.RestArgs = new slice<obj.Addr>(new obj.Addr[] { a[1], a[2], a[3] });
                    prog.To = a[4L];
                    break;
                }

                p.errorf("can't handle %s instruction with 5 operands", op);
                return ;
                goto __switch_break0;
            }
            if (len(a) == 6L)
            {
                if (p.arch.Family == sys.ARM && arch.IsARMMRC(op))
                { 
                    // Strange special case: MCR, MRC.
                    prog.To.Type = obj.TYPE_CONST;
                    var x0 = p.getConstant(prog, op, _addr_a[0L]);
                    var x1 = p.getConstant(prog, op, _addr_a[1L]);
                    var x2 = int64(p.getRegister(prog, op, _addr_a[2L]));
                    var x3 = int64(p.getRegister(prog, op, _addr_a[3L]));
                    var x4 = int64(p.getRegister(prog, op, _addr_a[4L]));
                    var x5 = p.getConstant(prog, op, _addr_a[5L]); 
                    // Cond is handled specially for this instruction.
                    var (offset, MRC, ok) = arch.ARMMRCOffset(op, cond, x0, x1, x2, x3, x4, x5);
                    if (!ok)
                    {
                        p.errorf("unrecognized condition code .%q", cond);
                    }

                    prog.To.Offset = offset;
                    cond = "";
                    prog.As = MRC; // Both instructions are coded as MRC.
                    break;

                }

            }
            // default: 
                p.errorf("can't handle %s instruction with %d operands", op, len(a));
                return ;

            __switch_break0:;

            p.append(prog, cond, true);

        }

        // newAddr returns a new(Addr) initialized to x.
        private static ptr<obj.Addr> newAddr(obj.Addr x)
        {
            ptr<obj.Addr> p = @new<obj.Addr>();
            p.val = x;
            return _addr_p!;
        }

        // symbolName returns the symbol name, or an error string if none if available.
        private static @string symbolName(ptr<obj.Addr> _addr_addr)
        {
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Sym != null)
            {
                return addr.Sym.Name;
            }

            return "<erroneous symbol>";

        }

        private static obj.Prog emptyProg = default;

        // getConstantPseudo checks that addr represents a plain constant and returns its value.
        private static long getConstantPseudo(this ptr<Parser> _addr_p, @string pseudo, ptr<obj.Addr> _addr_addr)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Type != obj.TYPE_MEM || addr.Name != 0L || addr.Reg != 0L || addr.Index != 0L)
            {
                p.errorf("%s: expected integer constant; found %s", pseudo, obj.Dconv(_addr_emptyProg, addr));
            }

            return addr.Offset;

        }

        // getConstant checks that addr represents a plain constant and returns its value.
        private static long getConstant(this ptr<Parser> _addr_p, ptr<obj.Prog> _addr_prog, obj.As op, ptr<obj.Addr> _addr_addr)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Prog prog = ref _addr_prog.val;
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Type != obj.TYPE_MEM || addr.Name != 0L || addr.Reg != 0L || addr.Index != 0L)
            {
                p.errorf("%s: expected integer constant; found %s", op, obj.Dconv(prog, addr));
            }

            return addr.Offset;

        }

        // getImmediate checks that addr represents an immediate constant and returns its value.
        private static long getImmediate(this ptr<Parser> _addr_p, ptr<obj.Prog> _addr_prog, obj.As op, ptr<obj.Addr> _addr_addr)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Prog prog = ref _addr_prog.val;
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Type != obj.TYPE_CONST || addr.Name != 0L || addr.Reg != 0L || addr.Index != 0L)
            {
                p.errorf("%s: expected immediate constant; found %s", op, obj.Dconv(prog, addr));
            }

            return addr.Offset;

        }

        // getRegister checks that addr represents a register and returns its value.
        private static short getRegister(this ptr<Parser> _addr_p, ptr<obj.Prog> _addr_prog, obj.As op, ptr<obj.Addr> _addr_addr)
        {
            ref Parser p = ref _addr_p.val;
            ref obj.Prog prog = ref _addr_prog.val;
            ref obj.Addr addr = ref _addr_addr.val;

            if (addr.Type != obj.TYPE_REG || addr.Offset != 0L || addr.Name != 0L || addr.Index != 0L)
            {
                p.errorf("%s: expected register; found %s", op, obj.Dconv(prog, addr));
            }

            return addr.Reg;

        }
    }
}}}}
