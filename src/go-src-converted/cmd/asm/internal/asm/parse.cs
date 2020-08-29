// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asm implements the parser and instruction generator for the assembler.
// TODO: Split apart?
// package asm -- go2cs converted at 2020 August 29 08:51:52 UTC
// import "cmd/asm/internal/asm" ==> using asm = go.cmd.asm.@internal.asm_package
// Original source: C:\Go\src\cmd\asm\internal\asm\parse.go
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;
using scanner = go.text.scanner_package;
using utf8 = go.unicode.utf8_package;

using arch = go.cmd.asm.@internal.arch_package;
using flags = go.cmd.asm.@internal.flags_package;
using lex = go.cmd.asm.@internal.lex_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class asm_package
    {
        public partial struct Parser
        {
            public lex.TokenReader lex;
            public long lineNum; // Line number in source file.
            public long errorLine; // Line number of last error.
            public long errorCount; // Number of errors.
            public long pc; // virtual PC; count of Progs; doesn't advance for GLOBL or DATA.
            public slice<lex.Token> input;
            public long inputPos;
            public slice<@string> pendingLabels; // Labels to attach to next instruction.
            public map<@string, ref obj.Prog> labels;
            public slice<Patch> toPatch;
            public slice<obj.Addr> addr;
            public ptr<arch.Arch> arch;
            public ptr<obj.Link> ctxt;
            public ptr<obj.Prog> firstProg;
            public ptr<obj.Prog> lastProg;
            public map<@string, long> dataAddr; // Most recent address for DATA for this symbol.
            public bool isJump; // Instruction being assembled is a jump.
            public io.Writer errorWriter;
        }

        public partial struct Patch
        {
            public ptr<obj.Prog> prog;
            public @string label;
        }

        public static ref Parser NewParser(ref obj.Link ctxt, ref arch.Arch ar, lex.TokenReader lexer)
        {
            return ref new Parser(ctxt:ctxt,arch:ar,lex:lexer,labels:make(map[string]*obj.Prog),dataAddr:make(map[string]int64),errorWriter:os.Stderr,);
        }

        // panicOnError is enabled when testing to abort execution on the first error
        // and turn it into a recoverable panic.
        private static bool panicOnError = default;

        private static void errorf(this ref Parser _p, @string format, params object[] args) => func(_p, (ref Parser p, Defer _, Panic panic, Recover __) =>
        {
            if (panicOnError)
            {
                panic(fmt.Errorf(format, args));
            }
            if (p.lineNum == p.errorLine)
            { 
                // Only one error per line.
                return;
            }
            p.errorLine = p.lineNum;
            if (p.lex != null)
            { 
                // Put file and line information on head of message.
                format = "%s:%d: " + format + "\n";
                args = append(args);
            }
            fmt.Fprintf(p.errorWriter, format, args);
            p.errorCount++;
            if (p.errorCount > 10L && !flags.AllErrors.Value)
            {
                log.Fatal("too many errors");
            }
        });

        private static src.XPos pos(this ref Parser p)
        {
            return p.ctxt.PosTable.XPos(src.MakePos(p.lex.Base(), uint(p.lineNum), 0L));
        }

        private static (ref obj.Prog, bool) Parse(this ref Parser p)
        {
            while (p.line())
            {
            }

            if (p.errorCount > 0L)
            {
                return (null, false);
            }
            p.patch();
            return (p.firstProg, true);
        }

        // WORD [ arg {, arg} ] (';' | '\n')
        private static bool line(this ref Parser p)
        { 
            // Skip newlines.
            lex.ScanToken tok = default;
            while (true)
            {
                tok = p.lex.Next(); 
                // We save the line number here so error messages from this instruction
                // are labeled with this line. Otherwise we complain after we've absorbed
                // the terminating newline and the line numbers are off by one in errors.
                p.lineNum = p.lex.Line();

                if (tok == '\n' || tok == ';') 
                    continue;
                else if (tok == scanner.EOF) 
                    return false;
                                break;
            } 
            // First item must be an identifier.
 
            // First item must be an identifier.
            if (tok != scanner.Ident)
            {
                p.errorf("expected identifier, found %q", p.lex.Text());
                return false; // Might as well stop now.
            }
            var word = p.lex.Text();
            @string cond = default;
            var operands = make_slice<slice<lex.Token>>(0L, 3L); 
            // Zero or more comma-separated operands, one per loop.
            long nesting = 0L;
            long colon = -1L;
            while (tok != '\n' && tok != ';')
            { 
                // Process one operand.
                var items = make_slice<lex.Token>(0L, 3L);
                while (true)
                {
                    tok = p.lex.Next();
                    if (len(operands) == 0L && len(items) == 0L)
                    {
                        if (p.arch.InFamily(sys.ARM, sys.ARM64) && tok == '.')
                        { 
                            // ARM conditionals.
                            tok = p.lex.Next();
                            var str = p.lex.Text();
                            if (tok != scanner.Ident)
                            {
                                p.errorf("ARM condition expected identifier, found %s", str);
                            }
                            cond = cond + "." + str;
                            continue;
                        }
                        if (tok == ':')
                        { 
                            // Labels.
                            p.pendingLabels = append(p.pendingLabels, word);
                            return true;
                        }
                    }
                    if (tok == scanner.EOF)
                    {
                        p.errorf("unexpected EOF");
                        return false;
                    } 
                    // Split operands on comma. Also, the old syntax on x86 for a "register pair"
                    // was AX:DX, for which the new syntax is DX, AX. Note the reordering.
                    if (tok == '\n' || tok == ';' || (nesting == 0L && (tok == ',' || tok == ':')))
                    {
                        if (tok == ':')
                        { 
                            // Remember this location so we can swap the operands below.
                            if (colon >= 0L)
                            {
                                p.errorf("invalid ':' in operand");
                                return true;
                            }
                            colon = len(operands);
                        }
                        break;
                    }
                    if (tok == '(' || tok == '[')
                    {
                        nesting++;
                    }
                    if (tok == ')' || tok == ']')
                    {
                        nesting--;
                    }
                    items = append(items, lex.Make(tok, p.lex.Text()));
                }

                if (len(items) > 0L)
                {
                    operands = append(operands, items);
                    if (colon >= 0L && len(operands) == colon + 2L)
                    { 
                        // AX:DX becomes DX, AX.
                        operands[colon] = operands[colon + 1L];
                        operands[colon + 1L] = operands[colon];
                        colon = -1L;
                    }
                }
                else if (len(operands) > 0L || tok == ',' || colon >= 0L)
                { 
                    // Had a separator with nothing after.
                    p.errorf("missing operand");
                }
            }

            if (p.pseudo(word, operands))
            {
                return true;
            }
            var (i, present) = p.arch.Instructions[word];
            if (present)
            {
                p.instruction(i, word, cond, operands);
                return true;
            }
            p.errorf("unrecognized instruction %q", word);
            return true;
        }

        private static void instruction(this ref Parser p, obj.As op, @string word, @string cond, slice<slice<lex.Token>> operands)
        {
            p.addr = p.addr[0L..0L];
            p.isJump = p.arch.IsJump(word);
            foreach (var (_, op) in operands)
            {
                var addr = p.address(op);
                if (!p.isJump && addr.Reg < 0L)
                { // Jumps refer to PC, a pseudo.
                    p.errorf("illegal use of pseudo-register in %s", word);
                }
                p.addr = append(p.addr, addr);
            }
            if (p.isJump)
            {
                p.asmJump(op, cond, p.addr);
                return;
            }
            p.asmInstruction(op, cond, p.addr);
        }

        private static bool pseudo(this ref Parser p, @string word, slice<slice<lex.Token>> operands)
        {
            switch (word)
            {
                case "DATA": 
                    p.asmData(word, operands);
                    break;
                case "FUNCDATA": 
                    p.asmFuncData(word, operands);
                    break;
                case "GLOBL": 
                    p.asmGlobl(word, operands);
                    break;
                case "PCDATA": 
                    p.asmPCData(word, operands);
                    break;
                case "TEXT": 
                    p.asmText(word, operands);
                    break;
                default: 
                    return false;
                    break;
            }
            return true;
        }

        private static void start(this ref Parser p, slice<lex.Token> operand)
        {
            p.input = operand;
            p.inputPos = 0L;
        }

        // address parses the operand into a link address structure.
        private static obj.Addr address(this ref Parser p, slice<lex.Token> operand)
        {
            p.start(operand);
            obj.Addr addr = new obj.Addr();
            p.operand(ref addr);
            return addr;
        }

        // parseScale converts a decimal string into a valid scale factor.
        private static sbyte parseScale(this ref Parser p, @string s)
        {
            switch (s)
            {
                case "1": 

                case "2": 

                case "4": 

                case "8": 
                    return int8(s[0L] - '0');
                    break;
            }
            p.errorf("bad scale: %s", s);
            return 0L;
        }

        // operand parses a general operand and stores the result in *a.
        private static void operand(this ref Parser _p, ref obj.Addr _a) => func(_p, _a, (ref Parser p, ref obj.Addr a, Defer _, Panic panic, Recover __) =>
        { 
            //fmt.Printf("Operand: %v\n", p.input)
            if (len(p.input) == 0L)
            {
                p.errorf("empty operand: cannot happen");
                return;
            } 
            // General address (with a few exceptions) looks like
            //    $sym±offset(SB)(reg)(index*scale)
            // Exceptions are:
            //
            //    R1
            //    offset
            //    $offset
            // Every piece is optional, so we scan left to right and what
            // we discover tells us where we are.

            // Prefix: $.
            int prefix = default;
            {
                var tok__prev1 = tok;

                var tok = p.peek();

                switch (tok)
                {
                    case '$': 

                    case '*': 
                        prefix = rune(tok);
                        p.next();
                        break;
                }

                tok = tok__prev1;
            } 

            // Symbol: sym±offset(SB)
            tok = p.next();
            var name = tok.String();
            if (tok.ScanToken == scanner.Ident && !p.atStartOfRegister(name))
            { 
                // We have a symbol. Parse $sym±offset(symkind)
                p.symbolReference(a, name, prefix); 
                // fmt.Printf("SYM %s\n", obj.Dconv(&emptyProg, 0, a))
                if (p.peek() == scanner.EOF)
                {
                    return;
                }
            } 

            // Special register list syntax for arm: [R1,R3-R7]
            if (tok.ScanToken == '[')
            {
                if (prefix != 0L)
                {
                    p.errorf("illegal use of register list");
                }
                p.registerList(a);
                p.expectOperandEnd();
                return;
            } 

            // Register: R1
            if (tok.ScanToken == scanner.Ident && p.atStartOfRegister(name))
            {
                if (p.atRegisterShift())
                { 
                    // ARM shifted register such as R1<<R2 or R1>>2.
                    a.Type = obj.TYPE_SHIFT;
                    a.Offset = p.registerShift(tok.String(), prefix);
                    if (p.peek() == '(')
                    { 
                        // Can only be a literal register here.
                        p.next();
                        tok = p.next();
                        name = tok.String();
                        if (!p.atStartOfRegister(name))
                        {
                            p.errorf("expected register; found %s", name);
                        }
                        a.Reg, _ = p.registerReference(name);
                        p.get(')');
                    }
                }
                else if (p.atRegisterExtension())
                {
                    p.registerExtension(a, tok.String(), prefix);
                    p.expectOperandEnd();
                    return;
                }                {
                    var (r1, r2, scale, ok) = p.register(tok.String(), prefix);


                    else if (ok)
                    {
                        if (scale != 0L)
                        {
                            p.errorf("expected simple register reference");
                        }
                        a.Type = obj.TYPE_REG;
                        a.Reg = r1;
                        if (r2 != 0L)
                        { 
                            // Form is R1:R2. It is on RHS and the second register
                            // needs to go into the LHS.
                            panic("cannot happen (Addr.Reg2)");
                        }
                    } 
                    // fmt.Printf("REG %s\n", obj.Dconv(&emptyProg, 0, a))

                } 
                // fmt.Printf("REG %s\n", obj.Dconv(&emptyProg, 0, a))
                p.expectOperandEnd();
                return;
            } 

            // Constant.
            var haveConstant = false;

            if (tok.ScanToken == scanner.Int || tok.ScanToken == scanner.Float || tok.ScanToken == scanner.String || tok.ScanToken == scanner.Char || tok.ScanToken == '+' || tok.ScanToken == '-' || tok.ScanToken == '~') 
                haveConstant = true;
            else if (tok.ScanToken == '(') 
                // Could be parenthesized expression or (R). Must be something, though.
                tok = p.next();
                if (tok.ScanToken == scanner.EOF)
                {
                    p.errorf("missing right parenthesis");
                    return;
                }
                var rname = tok.String();
                p.back();
                haveConstant = !p.atStartOfRegister(rname);
                if (!haveConstant)
                {
                    p.back(); // Put back the '('.
                }
                        if (haveConstant)
            {
                p.back();
                if (p.have(scanner.Float))
                {
                    if (prefix != '$')
                    {
                        p.errorf("floating-point constant must be an immediate");
                    }
                    a.Type = obj.TYPE_FCONST;
                    a.Val = p.floatExpr(); 
                    // fmt.Printf("FCONST %s\n", obj.Dconv(&emptyProg, 0, a))
                    p.expectOperandEnd();
                    return;
                }
                if (p.have(scanner.String))
                {
                    if (prefix != '$')
                    {
                        p.errorf("string constant must be an immediate");
                        return;
                    }
                    var (str, err) = strconv.Unquote(p.get(scanner.String).String());
                    if (err != null)
                    {
                        p.errorf("string parse error: %s", err);
                    }
                    a.Type = obj.TYPE_SCONST;
                    a.Val = str; 
                    // fmt.Printf("SCONST %s\n", obj.Dconv(&emptyProg, 0, a))
                    p.expectOperandEnd();
                    return;
                }
                a.Offset = int64(p.expr());
                if (p.peek() != '(')
                {
                    switch (prefix)
                    {
                        case '$': 
                            a.Type = obj.TYPE_CONST;
                            break;
                        case '*': 
                            a.Type = obj.TYPE_INDIR; // Can appear but is illegal, will be rejected by the linker.
                            break;
                        default: 
                            a.Type = obj.TYPE_MEM;
                            break;
                    } 
                    // fmt.Printf("CONST %d %s\n", a.Offset, obj.Dconv(&emptyProg, 0, a))
                    p.expectOperandEnd();
                    return;
                } 
                // fmt.Printf("offset %d \n", a.Offset)
            } 

            // Register indirection: (reg) or (index*scale). We are on the opening paren.
            p.registerIndirect(a, prefix); 
            // fmt.Printf("DONE %s\n", p.arch.Dconv(&emptyProg, 0, a))

            p.expectOperandEnd();
            return;
        });

        // atStartOfRegister reports whether the parser is at the start of a register definition.
        private static bool atStartOfRegister(this ref Parser p, @string name)
        { 
            // Simple register: R10.
            var (_, present) = p.arch.Register[name];
            if (present)
            {
                return true;
            } 
            // Parenthesized register: R(10).
            return p.arch.RegisterPrefix[name] && p.peek() == '(';
        }

        // atRegisterShift reports whether we are at the start of an ARM shifted register.
        // We have consumed the register or R prefix.
        private static bool atRegisterShift(this ref Parser p)
        { 
            // ARM only.
            if (!p.arch.InFamily(sys.ARM, sys.ARM64))
            {
                return false;
            } 
            // R1<<...
            if (lex.IsRegisterShift(p.peek()))
            {
                return true;
            } 
            // R(1)<<...   Ugly check. TODO: Rethink how we handle ARM register shifts to be
            // less special.
            if (p.peek() != '(' || len(p.input) - p.inputPos < 4L)
            {
                return false;
            }
            return p.at('(', scanner.Int, ')') && lex.IsRegisterShift(p.input[p.inputPos + 3L].ScanToken);
        }

        // atRegisterExtension reports whether we are at the start of an ARM64 extended register.
        // We have consumed the register or R prefix.
        private static bool atRegisterExtension(this ref Parser p)
        { 
            // ARM64 only.
            if (p.arch.Family != sys.ARM64)
            {
                return false;
            } 
            // R1.xxx
            if (p.peek() == '.')
            {
                return true;
            }
            return false;
        }

        // registerReference parses a register given either the name, R10, or a parenthesized form, SPR(10).
        private static (short, bool) registerReference(this ref Parser p, @string name)
        {
            var (r, present) = p.arch.Register[name];
            if (present)
            {
                return (r, true);
            }
            if (!p.arch.RegisterPrefix[name])
            {
                p.errorf("expected register; found %s", name);
                return (0L, false);
            }
            p.get('(');
            var tok = p.get(scanner.Int);
            var (num, err) = strconv.ParseInt(tok.String(), 10L, 16L);
            p.get(')');
            if (err != null)
            {
                p.errorf("parsing register list: %s", err);
                return (0L, false);
            }
            var (r, ok) = p.arch.RegisterNumber(name, int16(num));
            if (!ok)
            {
                p.errorf("illegal register %s(%d)", name, r);
                return (0L, false);
            }
            return (r, true);
        }

        // register parses a full register reference where there is no symbol present (as in 4(R0) or R(10) but not sym(SB))
        // including forms involving multiple registers such as R1:R2.
        private static (short, short, sbyte, bool) register(this ref Parser p, @string name, int prefix)
        { 
            // R1 or R(1) R1:R2 R1,R2 R1+R2, or R1*scale.
            r1, ok = p.registerReference(name);
            if (!ok)
            {
                return;
            }
            if (prefix != 0L && prefix != '*')
            { // *AX is OK.
                p.errorf("prefix %c not allowed for register: %c%s", prefix, prefix, name);
            }
            var c = p.peek();
            if (c == ':' || c == ',' || c == '+')
            { 
                // 2nd register; syntax (R1+R2) etc. No two architectures agree.
                // Check the architectures match the syntax.
                switch (p.next().ScanToken)
                {
                    case ',': 
                        if (!p.arch.InFamily(sys.ARM, sys.ARM64))
                        {
                            p.errorf("(register,register) not supported on this architecture");
                            return;
                        }
                        break;
                    case '+': 
                        if (p.arch.Family != sys.PPC64)
                        {
                            p.errorf("(register+register) not supported on this architecture");
                            return;
                        }
                        break;
                }
                var name = p.next().String();
                r2, ok = p.registerReference(name);
                if (!ok)
                {
                    return;
                }
            }
            if (p.peek() == '*')
            { 
                // Scale
                p.next();
                scale = p.parseScale(p.next().String());
            }
            return (r1, r2, scale, true);
        }

        // registerShift parses an ARM/ARM64 shifted register reference and returns the encoded representation.
        // There is known to be a register (current token) and a shift operator (peeked token).
        private static long registerShift(this ref Parser p, @string name, int prefix)
        {
            if (prefix != 0L)
            {
                p.errorf("prefix %c not allowed for shifted register: $%s", prefix, name);
            } 
            // R1 op R2 or r1 op constant.
            // op is:
            //    "<<" == 0
            //    ">>" == 1
            //    "->" == 2
            //    "@>" == 3
            var (r1, ok) = p.registerReference(name);
            if (!ok)
            {
                return 0L;
            }
            short op = default;

            if (p.next().ScanToken == lex.LSH) 
                op = 0L;
            else if (p.next().ScanToken == lex.RSH) 
                op = 1L;
            else if (p.next().ScanToken == lex.ARR) 
                op = 2L;
            else if (p.next().ScanToken == lex.ROT) 
                // following instructions on ARM64 support rotate right
                // AND, ANDS, TST, BIC, BICS, EON, EOR, ORR, MVN, ORN
                op = 3L;
                        var tok = p.next();
            var str = tok.String();
            short count = default;

            if (tok.ScanToken == scanner.Ident) 
                if (p.arch.Family == sys.ARM64)
                {
                    p.errorf("rhs of shift must be integer: %s", str);
                }
                else
                {
                    var (r2, ok) = p.registerReference(str);
                    if (!ok)
                    {
                        p.errorf("rhs of shift must be register or integer: %s", str);
                    }
                    count = (r2 & 15L) << (int)(8L) | 1L << (int)(4L);
                }
            else if (tok.ScanToken == scanner.Int || tok.ScanToken == '(') 
                p.back();
                var x = int64(p.expr());
                if (p.arch.Family == sys.ARM64)
                {
                    if (x >= 64L)
                    {
                        p.errorf("register shift count too large: %s", str);
                    }
                    count = int16((x & 63L) << (int)(10L));
                }
                else
                {
                    if (x >= 32L)
                    {
                        p.errorf("register shift count too large: %s", str);
                    }
                    count = int16((x & 31L) << (int)(7L));
                }
            else 
                p.errorf("unexpected %s in register shift", tok.String());
                        if (p.arch.Family == sys.ARM64)
            {
                return int64(int64(r1 & 31L) << (int)(16L) | int64(op) << (int)(22L) | int64(uint16(count)));
            }
            else
            {
                return int64((r1 & 15L) | op << (int)(5L) | count);
            }
        }

        // registerExtension parses a register with extension or arrangment.
        // There is known to be a register (current token) and an extension operator (peeked token).
        private static void registerExtension(this ref Parser p, ref obj.Addr a, @string name, int prefix)
        {
            if (prefix != 0L)
            {
                p.errorf("prefix %c not allowed for shifted register: $%s", prefix, name);
            }
            var (reg, ok) = p.registerReference(name);
            if (!ok)
            {
                p.errorf("unexpected %s in register extension", name);
                return;
            }
            p.get('.');
            var tok = p.next();
            var ext = tok.String();
            var isIndex = false;
            var num = int16(0L);
            var isAmount = true; // Amount is zero by default
            if (p.peek() == lex.LSH)
            { 
                // parses left shift amount applied after extension: <<Amount
                p.get(lex.LSH);
                tok = p.get(scanner.Int);
                var (amount, err) = strconv.ParseInt(tok.String(), 10L, 16L);
                if (err != null)
                {
                    p.errorf("parsing left shift amount: %s", err);
                }
                num = int16(amount);
            }
            else if (p.peek() == '[')
            { 
                // parses an element: [Index]
                p.get('[');
                tok = p.get(scanner.Int);
                var (index, err) = strconv.ParseInt(tok.String(), 10L, 16L);
                p.get(']');
                if (err != null)
                {
                    p.errorf("parsing element index: %s", err);
                }
                isIndex = true;
                isAmount = false;
                num = int16(index);
            }

            if (p.arch.Family == sys.ARM64) 
                var err = arch.ARM64RegisterExtension(a, ext, reg, num, isAmount, isIndex);
                if (err != null)
                {
                    p.errorf(err.Error());
                }
            else 
                p.errorf("register extension not supported on this architecture");
                    }

        // symbolReference parses a symbol that is known not to be a register.
        private static void symbolReference(this ref Parser p, ref obj.Addr a, @string name, int prefix)
        { 
            // Identifier is a name.
            switch (prefix)
            {
                case 0L: 
                    a.Type = obj.TYPE_MEM;
                    break;
                case '$': 
                    a.Type = obj.TYPE_ADDR;
                    break;
                case '*': 
                    a.Type = obj.TYPE_INDIR;
                    break;
            } 
            // Weirdness with statics: Might now have "<>".
            var isStatic = false;
            if (p.peek() == '<')
            {
                isStatic = true;
                p.next();
                p.get('>');
            }
            if (p.peek() == '+' || p.peek() == '-')
            {
                a.Offset = int64(p.expr());
            }
            if (isStatic)
            {
                a.Sym = p.ctxt.LookupStatic(name);
            }
            else
            {
                a.Sym = p.ctxt.Lookup(name);
            }
            if (p.peek() == scanner.EOF)
            {
                if (prefix == 0L && p.isJump)
                { 
                    // Symbols without prefix or suffix are jump labels.
                    return;
                }
                p.errorf("illegal or missing addressing mode for symbol %s", name);
                return;
            } 
            // Expect (SB), (FP), (PC), or (SP)
            p.get('(');
            var reg = p.get(scanner.Ident).String();
            p.get(')');
            p.setPseudoRegister(a, reg, isStatic, prefix);
        }

        // setPseudoRegister sets the NAME field of addr for a pseudo-register reference such as (SB).
        private static void setPseudoRegister(this ref Parser p, ref obj.Addr addr, @string reg, bool isStatic, int prefix)
        {
            if (addr.Reg != 0L)
            {
                p.errorf("internal error: reg %s already set in pseudo", reg);
            }
            switch (reg)
            {
                case "FP": 
                    addr.Name = obj.NAME_PARAM;
                    break;
                case "PC": 
                    if (prefix != 0L)
                    {
                        p.errorf("illegal addressing mode for PC");
                    }
                    addr.Type = obj.TYPE_BRANCH; // We set the type and leave NAME untouched. See asmJump.
                    break;
                case "SB": 
                    addr.Name = obj.NAME_EXTERN;
                    if (isStatic)
                    {
                        addr.Name = obj.NAME_STATIC;
                    }
                    break;
                case "SP": 
                    addr.Name = obj.NAME_AUTO; // The pseudo-stack.
                    break;
                default: 
                    p.errorf("expected pseudo-register; found %s", reg);
                    break;
            }
            if (prefix == '$')
            {
                addr.Type = obj.TYPE_ADDR;
            }
        }

        // registerIndirect parses the general form of a register indirection.
        // It is can be (R1), (R2*scale), or (R1)(R2*scale) where R1 may be a simple
        // register or register pair R:R or (R, R) or (R+R).
        // Or it might be a pseudo-indirection like (FP).
        // We are sitting on the opening parenthesis.
        private static void registerIndirect(this ref Parser p, ref obj.Addr a, int prefix)
        {
            p.get('(');
            var tok = p.next();
            var name = tok.String();
            var (r1, r2, scale, ok) = p.register(name, 0L);
            if (!ok)
            {
                p.errorf("indirect through non-register %s", tok);
            }
            p.get(')');
            a.Type = obj.TYPE_MEM;
            if (r1 < 0L)
            { 
                // Pseudo-register reference.
                if (r2 != 0L)
                {
                    p.errorf("cannot use pseudo-register in pair");
                    return;
                } 
                // For SB, SP, and FP, there must be a name here. 0(FP) is not legal.
                if (name != "PC" && a.Name == obj.NAME_NONE)
                {
                    p.errorf("cannot reference %s without a symbol", name);
                }
                p.setPseudoRegister(a, name, false, prefix);
                return;
            }
            a.Reg = r1;
            if (r2 != 0L)
            { 
                // TODO: Consistency in the encoding would be nice here.
                if (p.arch.InFamily(sys.ARM, sys.ARM64))
                { 
                    // Special form
                    // ARM: destination register pair (R1, R2).
                    // ARM64: register pair (R1, R2) for LDP/STP.
                    if (prefix != 0L || scale != 0L)
                    {
                        p.errorf("illegal address mode for register pair");
                        return;
                    }
                    a.Type = obj.TYPE_REGREG;
                    a.Offset = int64(r2); 
                    // Nothing may follow
                    return;
                }
                if (p.arch.Family == sys.PPC64)
                { 
                    // Special form for PPC64: (R1+R2); alias for (R1)(R2*1).
                    if (prefix != 0L || scale != 0L)
                    {
                        p.errorf("illegal address mode for register+register");
                        return;
                    }
                    a.Type = obj.TYPE_MEM;
                    a.Scale = 1L;
                    a.Index = r2; 
                    // Nothing may follow.
                    return;
                }
            }
            if (r2 != 0L)
            {
                p.errorf("indirect through register pair");
            }
            if (prefix == '$')
            {
                a.Type = obj.TYPE_ADDR;
            }
            if (r1 == arch.RPC && prefix != 0L)
            {
                p.errorf("illegal addressing mode for PC");
            }
            if (scale == 0L && p.peek() == '(')
            { 
                // General form (R)(R*scale).
                p.next();
                tok = p.next();
                r1, r2, scale, ok = p.register(tok.String(), 0L);
                if (!ok)
                {
                    p.errorf("indirect through non-register %s", tok);
                }
                if (r2 != 0L)
                {
                    p.errorf("unimplemented two-register form");
                }
                a.Index = r1;
                if (scale == 0L && p.arch.Family == sys.ARM64)
                { 
                    // scale is 1 by default for ARM64
                    a.Scale = 1L;
                }
                else
                {
                    a.Scale = int16(scale);
                }
                p.get(')');
            }
            else if (scale != 0L)
            { 
                // First (R) was missing, all we have is (R*scale).
                a.Reg = 0L;
                a.Index = r1;
                a.Scale = int16(scale);
            }
        }

        // registerList parses an ARM or ARM64 register list expression, a list of
        // registers in []. There may be comma-separated ranges or individual
        // registers, as in [R1,R3-R5] or [V1.S4, V2.S4, V3.S4, V4.S4].
        // For ARM, only R0 through R15 may appear.
        // For ARM64, V0 through V31 with arrangement may appear.
        // The opening bracket has been consumed.
        private static void registerList(this ref Parser p, ref obj.Addr a)
        { 
            // One range per loop.
            long maxReg = default;
            ushort bits = default;
            long arrangement = default;

            if (p.arch.Family == sys.ARM) 
                maxReg = 16L;
            else if (p.arch.Family == sys.ARM64) 
                maxReg = 32L;
            else 
                p.errorf("unexpected register list");
                        long firstReg = -1L;
            long nextReg = -1L;
            long regCnt = 0L;
ListLoop:
            while (true)
            {
                var tok = p.next();

                if (tok.ScanToken == ']') 
                    _breakListLoop = true;
                    break;
                else if (tok.ScanToken == scanner.EOF) 
                    p.errorf("missing ']' in register list");
                    return;
                
                if (p.arch.Family == sys.ARM64) 
                    // Vn.T
                    var name = tok.String();
                    var (r, ok) = p.registerReference(name);
                    if (!ok)
                    {
                        p.errorf("invalid register: %s", name);
                    }
                    var reg = r - p.arch.Register["V0"];
                    p.get('.');
                    tok = p.next();
                    var ext = tok.String();
                    var (curArrangement, err) = arch.ARM64RegisterArrangement(reg, name, ext);
                    if (err != null)
                    {
                        p.errorf(err.Error());
                    }
                    if (firstReg == -1L)
                    { 
                        // only record the first register and arrangement
                        firstReg = int(reg);
                        nextReg = firstReg;
                        arrangement = curArrangement;
                    }
                    else if (curArrangement != arrangement)
                    {
                        p.errorf("inconsistent arrangement in ARM64 register list");
                    }
                    else if (nextReg != int(reg))
                    {
                        p.errorf("incontiguous register in ARM64 register list: %s", name);
                    }
                    regCnt++;
                    nextReg = (nextReg + 1L) % 32L;
                else if (p.arch.Family == sys.ARM) 
                    // Parse the upper and lower bounds.
                    var lo = p.registerNumber(tok.String());
                    var hi = lo;
                    if (p.peek() == '-')
                    {
                        p.next();
                        hi = p.registerNumber(p.next().String());
                    }
                    if (hi < lo)
                    {
                        lo = hi;
                        hi = lo;
                    } 
                    // Check there are no duplicates in the register list.
                    for (long i = 0L; lo <= hi && i < maxReg; i++)
                    {
                        if (bits & (1L << (int)(lo)) != 0L)
                        {
                            p.errorf("register R%d already in list", lo);
                        }
                        bits |= 1L << (int)(lo);
                        lo++;
                    }
                else 
                    p.errorf("unexpected register list");
                                if (p.peek() != ']')
                {
                    p.get(',');
                }
            }
            a.Type = obj.TYPE_REGLIST;

            if (p.arch.Family == sys.ARM) 
                a.Offset = int64(bits);
            else if (p.arch.Family == sys.ARM64) 
                var (offset, err) = arch.ARM64RegisterListOffset(firstReg, regCnt, arrangement);
                if (err != null)
                {
                    p.errorf(err.Error());
                }
                a.Offset = offset;
            else 
                p.errorf("register list not supported on this architecuture");
                    }

        // register number is ARM-specific. It returns the number of the specified register.
        private static ushort registerNumber(this ref Parser p, @string name)
        {
            if (p.arch.Family == sys.ARM && name == "g")
            {
                return 10L;
            }
            if (name[0L] != 'R')
            {
                p.errorf("expected g or R0 through R15; found %s", name);
                return 0L;
            }
            var (r, ok) = p.registerReference(name);
            if (!ok)
            {
                return 0L;
            }
            var reg = r - p.arch.Register["R0"];
            if (reg < 0L)
            { 
                // Could happen for an architecture having other registers prefixed by R
                p.errorf("expected g or R0 through R15; found %s", name);
                return 0L;
            }
            return uint16(reg);
        }

        // Note: There are two changes in the expression handling here
        // compared to the old yacc/C implementations. Neither has
        // much practical consequence because the expressions we
        // see in assembly code are simple, but for the record:
        //
        // 1) Evaluation uses uint64; the old one used int64.
        // 2) Precedence uses Go rules not C rules.

        // expr = term | term ('+' | '-' | '|' | '^') term.
        private static ulong expr(this ref Parser p)
        {
            var value = p.term();
            while (true)
            {
                switch (p.peek())
                {
                    case '+': 
                        p.next();
                        value += p.term();
                        break;
                    case '-': 
                        p.next();
                        value -= p.term();
                        break;
                    case '|': 
                        p.next();
                        value |= p.term();
                        break;
                    case '^': 
                        p.next();
                        value ^= p.term();
                        break;
                    default: 
                        return value;
                        break;
                }
            }

        }

        // floatExpr = fconst | '-' floatExpr | '+' floatExpr | '(' floatExpr ')'
        private static double floatExpr(this ref Parser p)
        {
            var tok = p.next();

            if (tok.ScanToken == '(') 
                var v = p.floatExpr();
                if (p.next().ScanToken != ')')
                {
                    p.errorf("missing closing paren");
                }
                return v;
            else if (tok.ScanToken == '+') 
                return +p.floatExpr();
            else if (tok.ScanToken == '-') 
                return -p.floatExpr();
            else if (tok.ScanToken == scanner.Float) 
                return p.atof(tok.String());
                        p.errorf("unexpected %s evaluating float expression", tok);
            return 0L;
        }

        // term = factor | factor ('*' | '/' | '%' | '>>' | '<<' | '&') factor
        private static ulong term(this ref Parser p)
        {
            var value = p.factor();
            while (true)
            {

                if (p.peek() == '*') 
                    p.next();
                    value *= p.factor();
                else if (p.peek() == '/') 
                    p.next();
                    if (int64(value) < 0L)
                    {
                        p.errorf("divide of value with high bit set");
                    }
                    var divisor = p.factor();
                    if (divisor == 0L)
                    {
                        p.errorf("division by zero");
                    }
                    else
                    {
                        value /= divisor;
                    }
                else if (p.peek() == '%') 
                    p.next();
                    divisor = p.factor();
                    if (int64(value) < 0L)
                    {
                        p.errorf("modulo of value with high bit set");
                    }
                    if (divisor == 0L)
                    {
                        p.errorf("modulo by zero");
                    }
                    else
                    {
                        value %= divisor;
                    }
                else if (p.peek() == lex.LSH) 
                    p.next();
                    var shift = p.factor();
                    if (int64(shift) < 0L)
                    {
                        p.errorf("negative left shift count");
                    }
                    return value << (int)(shift);
                else if (p.peek() == lex.RSH) 
                    p.next();
                    shift = p.term();
                    if (int64(shift) < 0L)
                    {
                        p.errorf("negative right shift count");
                    }
                    if (int64(value) < 0L)
                    {
                        p.errorf("right shift of value with high bit set");
                    }
                    value >>= shift;
                else if (p.peek() == '&') 
                    p.next();
                    value &= p.factor();
                else 
                    return value;
                            }

        }

        // factor = const | '+' factor | '-' factor | '~' factor | '(' expr ')'
        private static ulong factor(this ref Parser p)
        {
            var tok = p.next();

            if (tok.ScanToken == scanner.Int) 
                return p.atoi(tok.String());
            else if (tok.ScanToken == scanner.Char) 
                var (str, err) = strconv.Unquote(tok.String());
                if (err != null)
                {
                    p.errorf("%s", err);
                }
                var (r, w) = utf8.DecodeRuneInString(str);
                if (w == 1L && r == utf8.RuneError)
                {
                    p.errorf("illegal UTF-8 encoding for character constant");
                }
                return uint64(r);
            else if (tok.ScanToken == '+') 
                return +p.factor();
            else if (tok.ScanToken == '-') 
                return -p.factor();
            else if (tok.ScanToken == '~') 
                return ~p.factor();
            else if (tok.ScanToken == '(') 
                var v = p.expr();
                if (p.next().ScanToken != ')')
                {
                    p.errorf("missing closing paren");
                }
                return v;
                        p.errorf("unexpected %s evaluating expression", tok);
            return 0L;
        }

        // positiveAtoi returns an int64 that must be >= 0.
        private static long positiveAtoi(this ref Parser p, @string str)
        {
            var (value, err) = strconv.ParseInt(str, 0L, 64L);
            if (err != null)
            {
                p.errorf("%s", err);
            }
            if (value < 0L)
            {
                p.errorf("%s overflows int64", str);
            }
            return value;
        }

        private static ulong atoi(this ref Parser p, @string str)
        {
            var (value, err) = strconv.ParseUint(str, 0L, 64L);
            if (err != null)
            {
                p.errorf("%s", err);
            }
            return value;
        }

        private static double atof(this ref Parser p, @string str)
        {
            var (value, err) = strconv.ParseFloat(str, 64L);
            if (err != null)
            {
                p.errorf("%s", err);
            }
            return value;
        }

        // EOF represents the end of input.
        public static var EOF = lex.Make(scanner.EOF, "EOF");

        private static lex.Token next(this ref Parser p)
        {
            if (!p.more())
            {
                return EOF;
            }
            var tok = p.input[p.inputPos];
            p.inputPos++;
            return tok;
        }

        private static void back(this ref Parser p)
        {
            if (p.inputPos == 0L)
            {
                p.errorf("internal error: backing up before BOL");
            }
            else
            {
                p.inputPos--;
            }
        }

        private static lex.ScanToken peek(this ref Parser p)
        {
            if (p.more())
            {
                return p.input[p.inputPos].ScanToken;
            }
            return scanner.EOF;
        }

        private static bool more(this ref Parser p)
        {
            return p.inputPos < len(p.input);
        }

        // get verifies that the next item has the expected type and returns it.
        private static lex.Token get(this ref Parser p, lex.ScanToken expected)
        {
            p.expect(expected, expected.String());
            return p.next();
        }

        // expectOperandEnd verifies that the parsing state is properly at the end of an operand.
        private static void expectOperandEnd(this ref Parser p)
        {
            p.expect(scanner.EOF, "end of operand");
        }

        // expect verifies that the next item has the expected type. It does not consume it.
        private static void expect(this ref Parser p, lex.ScanToken expectedToken, @string expectedMessage)
        {
            if (p.peek() != expectedToken)
            {
                p.errorf("expected %s, found %s", expectedMessage, p.next());
            }
        }

        // have reports whether the remaining tokens (including the current one) contain the specified token.
        private static bool have(this ref Parser p, lex.ScanToken token)
        {
            for (var i = p.inputPos; i < len(p.input); i++)
            {
                if (p.input[i].ScanToken == token)
                {
                    return true;
                }
            }

            return false;
        }

        // at reports whether the next tokens are as requested.
        private static bool at(this ref Parser p, params lex.ScanToken[] next)
        {
            if (len(p.input) - p.inputPos < len(next))
            {
                return false;
            }
            foreach (var (i, r) in next)
            {
                if (p.input[p.inputPos + i].ScanToken != r)
                {
                    return false;
                }
            }
            return true;
        }
    }
}}}}
