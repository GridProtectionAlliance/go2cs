// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package asm implements the parser and instruction generator for the assembler.
// TODO: Split apart?
// package asm -- go2cs converted at 2022 March 06 22:46:36 UTC
// import "cmd/asm/internal/asm" ==> using asm = go.cmd.asm.@internal.asm_package
// Original source: C:\Program Files\Go\src\cmd\asm\internal\asm\parse.go
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
using x86 = go.cmd.@internal.obj.x86_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;

namespace go.cmd.asm.@internal;

public static partial class asm_package {

public partial struct Parser {
    public lex.TokenReader lex;
    public nint lineNum; // Line number in source file.
    public nint errorLine; // Line number of last error.
    public nint errorCount; // Number of errors.
    public bool sawCode; // saw code in this file (as opposed to comments and blank lines)
    public long pc; // virtual PC; count of Progs; doesn't advance for GLOBL or DATA.
    public slice<lex.Token> input;
    public nint inputPos;
    public slice<@string> pendingLabels; // Labels to attach to next instruction.
    public map<@string, ptr<obj.Prog>> labels;
    public slice<Patch> toPatch;
    public slice<obj.Addr> addr;
    public ptr<arch.Arch> arch;
    public ptr<obj.Link> ctxt;
    public ptr<obj.Prog> firstProg;
    public ptr<obj.Prog> lastProg;
    public map<@string, long> dataAddr; // Most recent address for DATA for this symbol.
    public bool isJump; // Instruction being assembled is a jump.
    public bool compilingRuntime;
    public io.Writer errorWriter;
}

public partial struct Patch {
    public ptr<obj.Prog> prog;
    public @string label;
}

public static ptr<Parser> NewParser(ptr<obj.Link> _addr_ctxt, ptr<arch.Arch> _addr_ar, lex.TokenReader lexer, bool compilingRuntime) {
    ref obj.Link ctxt = ref _addr_ctxt.val;
    ref arch.Arch ar = ref _addr_ar.val;

    return addr(new Parser(ctxt:ctxt,arch:ar,lex:lexer,labels:make(map[string]*obj.Prog),dataAddr:make(map[string]int64),errorWriter:os.Stderr,compilingRuntime:compilingRuntime,));
}

// panicOnError is enabled when testing to abort execution on the first error
// and turn it into a recoverable panic.
private static bool panicOnError = default;

private static void errorf(this ptr<Parser> _addr_p, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref Parser p = ref _addr_p.val;

    if (panicOnError) {
        panic(fmt.Errorf(format, args));
    }
    if (p.lineNum == p.errorLine) { 
        // Only one error per line.
        return ;

    }
    p.errorLine = p.lineNum;
    if (p.lex != null) { 
        // Put file and line information on head of message.
        format = "%s:%d: " + format + "\n";
        args = append(args);

    }
    fmt.Fprintf(p.errorWriter, format, args);
    p.errorCount++;
    if (p.errorCount > 10 && !flags.AllErrors.val) {
        log.Fatal("too many errors");
    }
});

private static src.XPos pos(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    return p.ctxt.PosTable.XPos(src.MakePos(p.lex.Base(), uint(p.lineNum), 0));
}

private static (ptr<obj.Prog>, bool) Parse(this ptr<Parser> _addr_p) {
    ptr<obj.Prog> _p0 = default!;
    bool _p0 = default;
    ref Parser p = ref _addr_p.val;

    var scratch = make_slice<slice<lex.Token>>(0, 3);
    while (true) {
        var (word, cond, operands, ok) = p.line(scratch);
        if (!ok) {
            break;
        }
        scratch = operands;

        if (p.pseudo(word, operands)) {
            continue;
        }
        var (i, present) = p.arch.Instructions[word];
        if (present) {
            p.instruction(i, word, cond, operands);
            continue;
        }
        p.errorf("unrecognized instruction %q", word);

    }
    if (p.errorCount > 0) {
        return (_addr_null!, false);
    }
    p.patch();
    return (_addr_p.firstProg!, true);

}

// ParseSymABIs parses p's assembly code to find text symbol
// definitions and references and writes a symabis file to w.
private static bool ParseSymABIs(this ptr<Parser> _addr_p, io.Writer w) {
    ref Parser p = ref _addr_p.val;

    var operands = make_slice<slice<lex.Token>>(0, 3);
    while (true) {
        var (word, _, operands1, ok) = p.line(operands);
        if (!ok) {
            break;
        }
        operands = operands1;

        p.symDefRef(w, word, operands);

    }
    return p.errorCount == 0;

}

// nextToken returns the next non-build-comment token from the lexer.
// It reports misplaced //go:build comments but otherwise discards them.
private static lex.ScanToken nextToken(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    while (true) {
        var tok = p.lex.Next();
        if (tok == lex.BuildComment) {
            if (p.sawCode) {
                p.errorf("misplaced //go:build comment");
            }
            continue;
        }
        if (tok != '\n') {
            p.sawCode = true;
        }
        if (tok == '#') { 
            // A leftover wisp of a #include/#define/etc,
            // to let us know that p.sawCode should be true now.
            // Otherwise ignored.
            continue;

        }
        return tok;

    }

}

// line consumes a single assembly line from p.lex of the form
//
//   {label:} WORD[.cond] [ arg {, arg} ] (';' | '\n')
//
// It adds any labels to p.pendingLabels and returns the word, cond,
// operand list, and true. If there is an error or EOF, it returns
// ok=false.
//
// line may reuse the memory from scratch.
private static (@string, @string, slice<slice<lex.Token>>, bool) line(this ptr<Parser> _addr_p, slice<slice<lex.Token>> scratch) {
    @string word = default;
    @string cond = default;
    slice<slice<lex.Token>> operands = default;
    bool ok = default;
    ref Parser p = ref _addr_p.val;

next:
    lex.ScanToken tok = default;
    while (true) {
        tok = p.nextToken(); 
        // We save the line number here so error messages from this instruction
        // are labeled with this line. Otherwise we complain after we've absorbed
        // the terminating newline and the line numbers are off by one in errors.
        p.lineNum = p.lex.Line();

        if (tok == '\n' || tok == ';') 
            continue;
        else if (tok == scanner.EOF) 
            return ("", "", null, false);
                break;

    } 
    // First item must be an identifier.
    if (tok != scanner.Ident) {
        p.errorf("expected identifier, found %q", p.lex.Text());
        return ("", "", null, false); // Might as well stop now.
    }
    (word, cond) = (p.lex.Text(), "");    operands = scratch[..(int)0]; 
    // Zero or more comma-separated operands, one per loop.
    nint nesting = 0;
    nint colon = -1;
    while (tok != '\n' && tok != ';') { 
        // Process one operand.
        slice<lex.Token> items = default;
        if (cap(operands) > len(operands)) { 
            // Reuse scratch items slice.
            items = operands[..(int)cap(operands)][len(operands)][..(int)0];

        }
        else
 {
            items = make_slice<lex.Token>(0, 3);
        }
        while (true) {
            tok = p.nextToken();
            if (len(operands) == 0 && len(items) == 0) {
                if (p.arch.InFamily(sys.ARM, sys.ARM64, sys.AMD64, sys.I386) && tok == '.') { 
                    // Suffixes: ARM conditionals or x86 modifiers.
                    tok = p.nextToken();
                    var str = p.lex.Text();
                    if (tok != scanner.Ident) {
                        p.errorf("instruction suffix expected identifier, found %s", str);
                    }

                    cond = cond + "." + str;
                    continue;

                }

                if (tok == ':') { 
                    // Labels.
                    p.pendingLabels = append(p.pendingLabels, word);
                    goto next;

                }

            }

            if (tok == scanner.EOF) {
                p.errorf("unexpected EOF");
                return ("", "", null, false);
            } 
            // Split operands on comma. Also, the old syntax on x86 for a "register pair"
            // was AX:DX, for which the new syntax is DX, AX. Note the reordering.
            if (tok == '\n' || tok == ';' || (nesting == 0 && (tok == ',' || tok == ':'))) {
                if (tok == ':') { 
                    // Remember this location so we can swap the operands below.
                    if (colon >= 0) {
                        p.errorf("invalid ':' in operand");
                        return (word, cond, operands, true);
                    }

                    colon = len(operands);

                }

                break;

            }

            if (tok == '(' || tok == '[') {
                nesting++;
            }

            if (tok == ')' || tok == ']') {
                nesting--;
            }

            items = append(items, lex.Make(tok, p.lex.Text()));

        }
        if (len(items) > 0) {
            operands = append(operands, items);
            if (colon >= 0 && len(operands) == colon + 2) { 
                // AX:DX becomes DX, AX.
                (operands[colon], operands[colon + 1]) = (operands[colon + 1], operands[colon]);                colon = -1;

            }

        }
        else if (len(operands) > 0 || tok == ',' || colon >= 0) { 
            // Had a separator with nothing after.
            p.errorf("missing operand");

        }
    }
    return (word, cond, operands, true);

}

private static void instruction(this ptr<Parser> _addr_p, obj.As op, @string word, @string cond, slice<slice<lex.Token>> operands) {
    ref Parser p = ref _addr_p.val;

    p.addr = p.addr[(int)0..(int)0];
    p.isJump = p.arch.IsJump(word);
    foreach (var (_, op) in operands) {
        var addr = p.address(op);
        if (!p.isJump && addr.Reg < 0) { // Jumps refer to PC, a pseudo.
            p.errorf("illegal use of pseudo-register in %s", word);

        }
        p.addr = append(p.addr, addr);

    }    if (p.isJump) {
        p.asmJump(op, cond, p.addr);
        return ;
    }
    p.asmInstruction(op, cond, p.addr);

}

private static bool pseudo(this ptr<Parser> _addr_p, @string word, slice<slice<lex.Token>> operands) {
    ref Parser p = ref _addr_p.val;

    switch (word) {
        case "DATA": 
            p.asmData(operands);
            break;
        case "FUNCDATA": 
            p.asmFuncData(operands);
            break;
        case "GLOBL": 
            p.asmGlobl(operands);
            break;
        case "PCDATA": 
            p.asmPCData(operands);
            break;
        case "PCALIGN": 
            p.asmPCAlign(operands);
            break;
        case "TEXT": 
            p.asmText(operands);
            break;
        default: 
            return false;
            break;
    }
    return true;

}

// symDefRef scans a line for potential text symbol definitions and
// references and writes symabis information to w.
//
// The symabis format is documented at
// cmd/compile/internal/ssagen.ReadSymABIs.
private static void symDefRef(this ptr<Parser> _addr_p, io.Writer w, @string word, slice<slice<lex.Token>> operands) {
    ref Parser p = ref _addr_p.val;

    switch (word) {
        case "TEXT": 
            // Defines text symbol in operands[0].
            if (len(operands) > 0) {
                p.start(operands[0]);
                {
                    var name__prev2 = name;

                    var (name, abi, ok) = p.funcAddress();

                    if (ok) {
                        fmt.Fprintf(w, "def %s %s\n", name, abi);
                    }

                    name = name__prev2;

                }

            }
            return ;

            break;
        case "GLOBL": 

        case "PCDATA": 

            break;
        case "DATA": 
            // For DATA, operands[0] is defined symbol.
            // For FUNCDATA, operands[0] is an immediate constant.
            // Remaining operands may have references.

        case "FUNCDATA": 
            // For DATA, operands[0] is defined symbol.
            // For FUNCDATA, operands[0] is an immediate constant.
            // Remaining operands may have references.
            if (len(operands) < 2) {
                return ;
            }
            operands = operands[(int)1..];

            break;
    } 
    // Search for symbol references.
    foreach (var (_, op) in operands) {
        p.start(op);
        {
            var name__prev1 = name;

            (name, abi, ok) = p.funcAddress();

            if (ok) {
                fmt.Fprintf(w, "ref %s %s\n", name, abi);
            }

            name = name__prev1;

        }

    }
}

private static void start(this ptr<Parser> _addr_p, slice<lex.Token> operand) {
    ref Parser p = ref _addr_p.val;

    p.input = operand;
    p.inputPos = 0;
}

// address parses the operand into a link address structure.
private static obj.Addr address(this ptr<Parser> _addr_p, slice<lex.Token> operand) {
    ref Parser p = ref _addr_p.val;

    p.start(operand);
    ref obj.Addr addr = ref heap(new obj.Addr(), out ptr<obj.Addr> _addr_addr);
    p.operand(_addr_addr);
    return addr;
}

// parseScale converts a decimal string into a valid scale factor.
private static sbyte parseScale(this ptr<Parser> _addr_p, @string s) {
    ref Parser p = ref _addr_p.val;

    switch (s) {
        case "1": 

        case "2": 

        case "4": 

        case "8": 
            return int8(s[0] - '0');
            break;
    }
    p.errorf("bad scale: %s", s);
    return 0;

}

// operand parses a general operand and stores the result in *a.
private static void operand(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a) => func((_, panic, _) => {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;
 
    //fmt.Printf("Operand: %v\n", p.input)
    if (len(p.input) == 0) {
        p.errorf("empty operand: cannot happen");
        return ;
    }
    int prefix = default;
    {
        var tok__prev1 = tok;

        var tok = p.peek();

        switch (tok) {
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
    if (tok.ScanToken == scanner.Ident && !p.atStartOfRegister(name)) { 
        // We have a symbol. Parse $sym±offset(symkind)
        p.symbolReference(a, name, prefix); 
        // fmt.Printf("SYM %s\n", obj.Dconv(&emptyProg, 0, a))
        if (p.peek() == scanner.EOF) {
            return ;
        }
    }
    if (tok.ScanToken == '[') {
        if (prefix != 0) {
            p.errorf("illegal use of register list");
        }
        p.registerList(a);
        p.expectOperandEnd();
        return ;

    }
    if (tok.ScanToken == scanner.Ident && p.atStartOfRegister(name)) {
        if (p.atRegisterShift()) { 
            // ARM shifted register such as R1<<R2 or R1>>2.
            a.Type = obj.TYPE_SHIFT;
            a.Offset = p.registerShift(tok.String(), prefix);
            if (p.peek() == '(') { 
                // Can only be a literal register here.
                p.next();
                tok = p.next();
                name = tok.String();
                if (!p.atStartOfRegister(name)) {
                    p.errorf("expected register; found %s", name);
                }

                a.Reg, _ = p.registerReference(name);
                p.get(')');

            }

        }
        else if (p.atRegisterExtension()) {
            a.Type = obj.TYPE_REG;
            p.registerExtension(a, tok.String(), prefix);
            p.expectOperandEnd();
            return ;
        }        {
            var (r1, r2, scale, ok) = p.register(tok.String(), prefix);


            else if (ok) {
                if (scale != 0) {
                    p.errorf("expected simple register reference");
                }
                a.Type = obj.TYPE_REG;
                a.Reg = r1;
                if (r2 != 0) { 
                    // Form is R1:R2. It is on RHS and the second register
                    // needs to go into the LHS.
                    panic("cannot happen (Addr.Reg2)");

                }

            } 
            // fmt.Printf("REG %s\n", obj.Dconv(&emptyProg, 0, a))

        } 
        // fmt.Printf("REG %s\n", obj.Dconv(&emptyProg, 0, a))
        p.expectOperandEnd();
        return ;

    }
    var haveConstant = false;

    if (tok.ScanToken == scanner.Int || tok.ScanToken == scanner.Float || tok.ScanToken == scanner.String || tok.ScanToken == scanner.Char || tok.ScanToken == '+' || tok.ScanToken == '-' || tok.ScanToken == '~') 
        haveConstant = true;
    else if (tok.ScanToken == '(') 
        // Could be parenthesized expression or (R). Must be something, though.
        tok = p.next();
        if (tok.ScanToken == scanner.EOF) {
            p.errorf("missing right parenthesis");
            return ;
        }
        var rname = tok.String();
        p.back();
        haveConstant = !p.atStartOfRegister(rname);
        if (!haveConstant) {
            p.back(); // Put back the '('.
        }
        if (haveConstant) {
        p.back();
        if (p.have(scanner.Float)) {
            if (prefix != '$') {
                p.errorf("floating-point constant must be an immediate");
            }
            a.Type = obj.TYPE_FCONST;
            a.Val = p.floatExpr(); 
            // fmt.Printf("FCONST %s\n", obj.Dconv(&emptyProg, 0, a))
            p.expectOperandEnd();
            return ;

        }
        if (p.have(scanner.String)) {
            if (prefix != '$') {
                p.errorf("string constant must be an immediate");
                return ;
            }
            var (str, err) = strconv.Unquote(p.get(scanner.String).String());
            if (err != null) {
                p.errorf("string parse error: %s", err);
            }
            a.Type = obj.TYPE_SCONST;
            a.Val = str; 
            // fmt.Printf("SCONST %s\n", obj.Dconv(&emptyProg, 0, a))
            p.expectOperandEnd();
            return ;

        }
        a.Offset = int64(p.expr());
        if (p.peek() != '(') {
            switch (prefix) {
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
            return ;

        }
    }
    p.registerIndirect(a, prefix); 
    // fmt.Printf("DONE %s\n", p.arch.Dconv(&emptyProg, 0, a))

    p.expectOperandEnd();
    return ;

});

// atStartOfRegister reports whether the parser is at the start of a register definition.
private static bool atStartOfRegister(this ptr<Parser> _addr_p, @string name) {
    ref Parser p = ref _addr_p.val;
 
    // Simple register: R10.
    var (_, present) = p.arch.Register[name];
    if (present) {
        return true;
    }
    return p.arch.RegisterPrefix[name] && p.peek() == '(';

}

// atRegisterShift reports whether we are at the start of an ARM shifted register.
// We have consumed the register or R prefix.
private static bool atRegisterShift(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;
 
    // ARM only.
    if (!p.arch.InFamily(sys.ARM, sys.ARM64)) {
        return false;
    }
    if (lex.IsRegisterShift(p.peek())) {
        return true;
    }
    if (p.peek() != '(' || len(p.input) - p.inputPos < 4) {
        return false;
    }
    return p.at('(', scanner.Int, ')') && lex.IsRegisterShift(p.input[p.inputPos + 3].ScanToken);

}

// atRegisterExtension reports whether we are at the start of an ARM64 extended register.
// We have consumed the register or R prefix.
private static bool atRegisterExtension(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;
 
    // ARM64 only.
    if (p.arch.Family != sys.ARM64) {
        return false;
    }
    if (p.peek() == '.') {
        return true;
    }
    return false;

}

// registerReference parses a register given either the name, R10, or a parenthesized form, SPR(10).
private static (short, bool) registerReference(this ptr<Parser> _addr_p, @string name) {
    short _p0 = default;
    bool _p0 = default;
    ref Parser p = ref _addr_p.val;

    var (r, present) = p.arch.Register[name];
    if (present) {
        return (r, true);
    }
    if (!p.arch.RegisterPrefix[name]) {
        p.errorf("expected register; found %s", name);
        return (0, false);
    }
    p.get('(');
    var tok = p.get(scanner.Int);
    var (num, err) = strconv.ParseInt(tok.String(), 10, 16);
    p.get(')');
    if (err != null) {
        p.errorf("parsing register list: %s", err);
        return (0, false);
    }
    var (r, ok) = p.arch.RegisterNumber(name, int16(num));
    if (!ok) {
        p.errorf("illegal register %s(%d)", name, r);
        return (0, false);
    }
    return (r, true);

}

// register parses a full register reference where there is no symbol present (as in 4(R0) or R(10) but not sym(SB))
// including forms involving multiple registers such as R1:R2.
private static (short, short, sbyte, bool) register(this ptr<Parser> _addr_p, @string name, int prefix) {
    short r1 = default;
    short r2 = default;
    sbyte scale = default;
    bool ok = default;
    ref Parser p = ref _addr_p.val;
 
    // R1 or R(1) R1:R2 R1,R2 R1+R2, or R1*scale.
    r1, ok = p.registerReference(name);
    if (!ok) {
        return ;
    }
    if (prefix != 0 && prefix != '*') { // *AX is OK.
        p.errorf("prefix %c not allowed for register: %c%s", prefix, prefix, name);

    }
    var c = p.peek();
    if (c == ':' || c == ',' || c == '+') { 
        // 2nd register; syntax (R1+R2) etc. No two architectures agree.
        // Check the architectures match the syntax.
        switch (p.next().ScanToken) {
            case ',': 
                if (!p.arch.InFamily(sys.ARM, sys.ARM64)) {
                    p.errorf("(register,register) not supported on this architecture");
                    return ;
                }
                break;
            case '+': 
                if (p.arch.Family != sys.PPC64) {
                    p.errorf("(register+register) not supported on this architecture");
                    return ;
                }
                break;
        }
        var name = p.next().String();
        r2, ok = p.registerReference(name);
        if (!ok) {
            return ;
        }
    }
    if (p.peek() == '*') { 
        // Scale
        p.next();
        scale = p.parseScale(p.next().String());

    }
    return (r1, r2, scale, true);

}

// registerShift parses an ARM/ARM64 shifted register reference and returns the encoded representation.
// There is known to be a register (current token) and a shift operator (peeked token).
private static long registerShift(this ptr<Parser> _addr_p, @string name, int prefix) {
    ref Parser p = ref _addr_p.val;

    if (prefix != 0) {
        p.errorf("prefix %c not allowed for shifted register: $%s", prefix, name);
    }
    var (r1, ok) = p.registerReference(name);
    if (!ok) {
        return 0;
    }
    short op = default;

    if (p.next().ScanToken == lex.LSH) 
        op = 0;
    else if (p.next().ScanToken == lex.RSH) 
        op = 1;
    else if (p.next().ScanToken == lex.ARR) 
        op = 2;
    else if (p.next().ScanToken == lex.ROT) 
        // following instructions on ARM64 support rotate right
        // AND, ANDS, TST, BIC, BICS, EON, EOR, ORR, MVN, ORN
        op = 3;
        var tok = p.next();
    var str = tok.String();
    short count = default;

    if (tok.ScanToken == scanner.Ident) 
        if (p.arch.Family == sys.ARM64) {
            p.errorf("rhs of shift must be integer: %s", str);
        }
        else
 {
            var (r2, ok) = p.registerReference(str);
            if (!ok) {
                p.errorf("rhs of shift must be register or integer: %s", str);
            }
            count = (r2 & 15) << 8 | 1 << 4;
        }
    else if (tok.ScanToken == scanner.Int || tok.ScanToken == '(') 
        p.back();
        var x = int64(p.expr());
        if (p.arch.Family == sys.ARM64) {
            if (x >= 64) {
                p.errorf("register shift count too large: %s", str);
            }
            count = int16((x & 63) << 10);
        }
        else
 {
            if (x >= 32) {
                p.errorf("register shift count too large: %s", str);
            }
            count = int16((x & 31) << 7);
        }
    else 
        p.errorf("unexpected %s in register shift", tok.String());
        if (p.arch.Family == sys.ARM64) {
        var (off, err) = arch.ARM64RegisterShift(r1, op, count);
        if (err != null) {
            p.errorf(err.Error());
        }
        return off;

    }
    else
 {
        return int64((r1 & 15) | op << 5 | count);
    }
}

// registerExtension parses a register with extension or arrangement.
// There is known to be a register (current token) and an extension operator (peeked token).
private static void registerExtension(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a, @string name, int prefix) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;

    if (prefix != 0) {
        p.errorf("prefix %c not allowed for shifted register: $%s", prefix, name);
    }
    var (reg, ok) = p.registerReference(name);
    if (!ok) {
        p.errorf("unexpected %s in register extension", name);
        return ;
    }
    var isIndex = false;
    var num = int16(0);
    var isAmount = true; // Amount is zero by default
    @string ext = "";
    if (p.peek() == lex.LSH) { 
        // (Rn)(Rm<<2), the shifted offset register.
        ext = "LSL";

    }
    else
 { 
        // (Rn)(Rm.UXTW<1), the extended offset register.
        // Rm.UXTW<<3, the extended register.
        p.get('.');
        var tok = p.next();
        ext = tok.String();

    }
    if (p.peek() == lex.LSH) { 
        // parses left shift amount applied after extension: <<Amount
        p.get(lex.LSH);
        tok = p.get(scanner.Int);
        var (amount, err) = strconv.ParseInt(tok.String(), 10, 16);
        if (err != null) {
            p.errorf("parsing left shift amount: %s", err);
        }
        num = int16(amount);

    }
    else if (p.peek() == '[') { 
        // parses an element: [Index]
        p.get('[');
        tok = p.get(scanner.Int);
        var (index, err) = strconv.ParseInt(tok.String(), 10, 16);
        p.get(']');
        if (err != null) {
            p.errorf("parsing element index: %s", err);
        }
        isIndex = true;
        isAmount = false;
        num = int16(index);

    }

    if (p.arch.Family == sys.ARM64) 
        var err = arch.ARM64RegisterExtension(a, ext, reg, num, isAmount, isIndex);
        if (err != null) {
            p.errorf(err.Error());
        }
    else 
        p.errorf("register extension not supported on this architecture");
    
}

// symbolReference parses a symbol that is known not to be a register.
private static void symbolReference(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a, @string name, int prefix) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;
 
    // Identifier is a name.
    switch (prefix) {
        case 0: 
            a.Type = obj.TYPE_MEM;
            break;
        case '$': 
            a.Type = obj.TYPE_ADDR;
            break;
        case '*': 
            a.Type = obj.TYPE_INDIR;
            break;
    } 

    // Parse optional <> (indicates a static symbol) or
    // <ABIxxx> (selecting text symbol with specific ABI).
    var doIssueError = true;
    var (isStatic, abi) = p.symRefAttrs(name, doIssueError);

    if (p.peek() == '+' || p.peek() == '-') {
        a.Offset = int64(p.expr());
    }
    if (isStatic) {
        a.Sym = p.ctxt.LookupStatic(name);
    }
    else
 {
        a.Sym = p.ctxt.LookupABI(name, abi);
    }
    if (p.peek() == scanner.EOF) {
        if (prefix == 0 && p.isJump) { 
            // Symbols without prefix or suffix are jump labels.
            return ;

        }
        p.errorf("illegal or missing addressing mode for symbol %s", name);
        return ;

    }
    p.get('(');
    var reg = p.get(scanner.Ident).String();
    p.get(')');
    p.setPseudoRegister(a, reg, isStatic, prefix);

}

// setPseudoRegister sets the NAME field of addr for a pseudo-register reference such as (SB).
private static void setPseudoRegister(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_addr, @string reg, bool isStatic, int prefix) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr addr = ref _addr_addr.val;

    if (addr.Reg != 0) {
        p.errorf("internal error: reg %s already set in pseudo", reg);
    }
    switch (reg) {
        case "FP": 
            addr.Name = obj.NAME_PARAM;
            break;
        case "PC": 
            if (prefix != 0) {
                p.errorf("illegal addressing mode for PC");
            }
            addr.Type = obj.TYPE_BRANCH; // We set the type and leave NAME untouched. See asmJump.
            break;
        case "SB": 
            addr.Name = obj.NAME_EXTERN;
            if (isStatic) {
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
    if (prefix == '$') {
        addr.Type = obj.TYPE_ADDR;
    }
}

// symRefAttrs parses an optional function symbol attribute clause for
// the function symbol 'name', logging an error for a malformed
// attribute clause if 'issueError' is true. The return value is a
// (boolean, ABI) pair indicating that the named symbol is either
// static or a particular ABI specification.
//
// The expected form of the attribute clause is:
//
// empty,           yielding (false, obj.ABI0)
// "<>",            yielding (true,  obj.ABI0)
// "<ABI0>"         yielding (false, obj.ABI0)
// "<ABIInternal>"  yielding (false, obj.ABIInternal)
//
// Anything else beginning with "<" logs an error if issueError is
// true, otherwise returns (false, obj.ABI0).
//
private static (bool, obj.ABI) symRefAttrs(this ptr<Parser> _addr_p, @string name, bool issueError) {
    bool _p0 = default;
    obj.ABI _p0 = default;
    ref Parser p = ref _addr_p.val;

    var abi = obj.ABI0;
    var isStatic = false;
    if (p.peek() != '<') {
        return (isStatic, abi);
    }
    p.next();
    var tok = p.peek();
    if (tok == '>') {
        isStatic = true;
    }
    else if (tok == scanner.Ident) {
        var abistr = p.get(scanner.Ident).String();
        if (!p.compilingRuntime) {
            if (issueError) {
                p.errorf("ABI selector only permitted when compiling runtime, reference was to %q", name);
            }
        }
        else
 {
            var (theabi, valid) = obj.ParseABI(abistr);
            if (!valid) {
                if (issueError) {
                    p.errorf("malformed ABI selector %q in reference to %q", abistr, name);
                }
            }
            else
 {
                abi = theabi;
            }

        }
    }
    p.get('>');
    return (isStatic, abi);

}

// funcAddress parses an external function address. This is a
// constrained form of the operand syntax that's always SB-based,
// non-static, and has at most a simple integer offset:
//
//    [$|*]sym[<abi>][+Int](SB)
private static (@string, obj.ABI, bool) funcAddress(this ptr<Parser> _addr_p) {
    @string _p0 = default;
    obj.ABI _p0 = default;
    bool _p0 = default;
    ref Parser p = ref _addr_p.val;

    switch (p.peek()) {
        case '$': 
            // Skip prefix.

        case '*': 
            // Skip prefix.
            p.next();
            break;
    }

    var tok = p.next();
    var name = tok.String();
    if (tok.ScanToken != scanner.Ident || p.atStartOfRegister(name)) {
        return ("", obj.ABI0, false);
    }
    var noErrMsg = false;
    var (isStatic, abi) = p.symRefAttrs(name, noErrMsg);
    if (isStatic) {
        return ("", obj.ABI0, false); // This function rejects static symbols.
    }
    tok = p.next();
    if (tok.ScanToken == '+') {
        if (p.next().ScanToken != scanner.Int) {
            return ("", obj.ABI0, false);
        }
        tok = p.next();

    }
    if (tok.ScanToken != '(') {
        return ("", obj.ABI0, false);
    }
    {
        var reg = p.next();

        if (reg.ScanToken != scanner.Ident || reg.String() != "SB") {
            return ("", obj.ABI0, false);
        }
    }

    if (p.next().ScanToken != ')' || p.peek() != scanner.EOF) {
        return ("", obj.ABI0, false);
    }
    return (name, abi, true);

}

// registerIndirect parses the general form of a register indirection.
// It is can be (R1), (R2*scale), (R1)(R2*scale), (R1)(R2.SXTX<<3) or (R1)(R2<<3)
// where R1 may be a simple register or register pair R:R or (R, R) or (R+R).
// Or it might be a pseudo-indirection like (FP).
// We are sitting on the opening parenthesis.
private static void registerIndirect(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a, int prefix) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;

    p.get('(');
    var tok = p.next();
    var name = tok.String();
    var (r1, r2, scale, ok) = p.register(name, 0);
    if (!ok) {
        p.errorf("indirect through non-register %s", tok);
    }
    p.get(')');
    a.Type = obj.TYPE_MEM;
    if (r1 < 0) { 
        // Pseudo-register reference.
        if (r2 != 0) {
            p.errorf("cannot use pseudo-register in pair");
            return ;
        }
        if (name != "PC" && a.Name == obj.NAME_NONE) {
            p.errorf("cannot reference %s without a symbol", name);
        }
        p.setPseudoRegister(a, name, false, prefix);
        return ;

    }
    a.Reg = r1;
    if (r2 != 0) { 
        // TODO: Consistency in the encoding would be nice here.
        if (p.arch.InFamily(sys.ARM, sys.ARM64)) { 
            // Special form
            // ARM: destination register pair (R1, R2).
            // ARM64: register pair (R1, R2) for LDP/STP.
            if (prefix != 0 || scale != 0) {
                p.errorf("illegal address mode for register pair");
                return ;
            }

            a.Type = obj.TYPE_REGREG;
            a.Offset = int64(r2); 
            // Nothing may follow
            return ;

        }
        if (p.arch.Family == sys.PPC64) { 
            // Special form for PPC64: (R1+R2); alias for (R1)(R2*1).
            if (prefix != 0 || scale != 0) {
                p.errorf("illegal address mode for register+register");
                return ;
            }

            a.Type = obj.TYPE_MEM;
            a.Scale = 1;
            a.Index = r2; 
            // Nothing may follow.
            return ;

        }
    }
    if (r2 != 0) {
        p.errorf("indirect through register pair");
    }
    if (prefix == '$') {
        a.Type = obj.TYPE_ADDR;
    }
    if (r1 == arch.RPC && prefix != 0) {
        p.errorf("illegal addressing mode for PC");
    }
    if (scale == 0 && p.peek() == '(') { 
        // General form (R)(R*scale).
        p.next();
        tok = p.next();
        if (p.atRegisterExtension()) {
            p.registerExtension(a, tok.String(), prefix);
        }
        else if (p.atRegisterShift()) { 
            // (R1)(R2<<3)
            p.registerExtension(a, tok.String(), prefix);

        }
        else
 {
            r1, r2, scale, ok = p.register(tok.String(), 0);
            if (!ok) {
                p.errorf("indirect through non-register %s", tok);
            }
            if (r2 != 0) {
                p.errorf("unimplemented two-register form");
            }
            a.Index = r1;
            if (scale != 0 && scale != 1 && p.arch.Family == sys.ARM64) { 
                // Support (R1)(R2) (no scaling) and (R1)(R2*1).
                p.errorf("arm64 doesn't support scaled register format");

            }
            else
 {
                a.Scale = int16(scale);
            }

        }
        p.get(')');

    }
    else if (scale != 0) {
        if (p.arch.Family == sys.ARM64) {
            p.errorf("arm64 doesn't support scaled register format");
        }
        a.Reg = 0;
        a.Index = r1;
        a.Scale = int16(scale);

    }
}

// registerList parses an ARM or ARM64 register list expression, a list of
// registers in []. There may be comma-separated ranges or individual
// registers, as in [R1,R3-R5] or [V1.S4, V2.S4, V3.S4, V4.S4].
// For ARM, only R0 through R15 may appear.
// For ARM64, V0 through V31 with arrangement may appear.
//
// For 386/AMD64 register list specifies 4VNNIW-style multi-source operand.
// For range of 4 elements, Intel manual uses "+3" notation, for example:
//    VP4DPWSSDS zmm1{k1}{z}, zmm2+3, m128
// Given asm line:
//    VP4DPWSSDS Z5, [Z10-Z13], (AX)
// zmm2 is Z10, and Z13 is the only valid value for it (Z10+3).
// Only simple ranges are accepted, like [Z0-Z3].
//
// The opening bracket has been consumed.
private static void registerList(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;

    if (p.arch.InFamily(sys.I386, sys.AMD64)) {
        p.registerListX86(a);
    }
    else
 {
        p.registerListARM(a);
    }
}

private static void registerListARM(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;
 
    // One range per loop.
    nint maxReg = default;
    ushort bits = default;
    long arrangement = default;

    if (p.arch.Family == sys.ARM) 
        maxReg = 16;
    else if (p.arch.Family == sys.ARM64) 
        maxReg = 32;
    else 
        p.errorf("unexpected register list");
        nint firstReg = -1;
    nint nextReg = -1;
    nint regCnt = 0;
ListLoop:
    while (true) {
        var tok = p.next();

        if (tok.ScanToken == ']') 
            _breakListLoop = true;
            break;
        else if (tok.ScanToken == scanner.EOF) 
            p.errorf("missing ']' in register list");
            return ;
        
        if (p.arch.Family == sys.ARM64) 
            // Vn.T
            var name = tok.String();
            var (r, ok) = p.registerReference(name);
            if (!ok) {
                p.errorf("invalid register: %s", name);
            }
            var reg = r - p.arch.Register["V0"];
            p.get('.');
            tok = p.next();
            var ext = tok.String();
            var (curArrangement, err) = arch.ARM64RegisterArrangement(reg, name, ext);
            if (err != null) {
                p.errorf(err.Error());
            }
            if (firstReg == -1) { 
                // only record the first register and arrangement
                firstReg = int(reg);
                nextReg = firstReg;
                arrangement = curArrangement;

            }
            else if (curArrangement != arrangement) {
                p.errorf("inconsistent arrangement in ARM64 register list");
            }
            else if (nextReg != int(reg)) {
                p.errorf("incontiguous register in ARM64 register list: %s", name);
            }

            regCnt++;
            nextReg = (nextReg + 1) % 32;
        else if (p.arch.Family == sys.ARM) 
            // Parse the upper and lower bounds.
            var lo = p.registerNumber(tok.String());
            var hi = lo;
            if (p.peek() == '-') {
                p.next();
                hi = p.registerNumber(p.next().String());
            }
            if (hi < lo) {
                (lo, hi) = (hi, lo);
            } 
            // Check there are no duplicates in the register list.
            for (nint i = 0; lo <= hi && i < maxReg; i++) {
                if (bits & (1 << (int)(lo)) != 0) {
                    p.errorf("register R%d already in list", lo);
                }
                bits |= 1 << (int)(lo);
                lo++;
            }
        else 
            p.errorf("unexpected register list");
                if (p.peek() != ']') {
            p.get(',');
        }
    }
    a.Type = obj.TYPE_REGLIST;

    if (p.arch.Family == sys.ARM) 
        a.Offset = int64(bits);
    else if (p.arch.Family == sys.ARM64) 
        var (offset, err) = arch.ARM64RegisterListOffset(firstReg, regCnt, arrangement);
        if (err != null) {
            p.errorf(err.Error());
        }
        a.Offset = offset;
    else 
        p.errorf("register list not supported on this architecuture");
    
}

private static void registerListX86(this ptr<Parser> _addr_p, ptr<obj.Addr> _addr_a) {
    ref Parser p = ref _addr_p.val;
    ref obj.Addr a = ref _addr_a.val;
 
    // Accept only [RegA-RegB] syntax.
    // Don't use p.get() to provide better error messages.

    var loName = p.next().String();
    var (lo, ok) = p.arch.Register[loName];
    if (!ok) {
        if (loName == "EOF") {
            p.errorf("register list: expected ']', found EOF");
        }
        else
 {
            p.errorf("register list: bad low register in `[%s`", loName);
        }
        return ;

    }
    {
        var tok__prev1 = tok;

        var tok = p.next().ScanToken;

        if (tok != '-') {
            p.errorf("register list: expected '-' after `[%s`, found %s", loName, tok);
            return ;
        }
        tok = tok__prev1;

    }

    var hiName = p.next().String();
    var (hi, ok) = p.arch.Register[hiName];
    if (!ok) {
        p.errorf("register list: bad high register in `[%s-%s`", loName, hiName);
        return ;
    }
    {
        var tok__prev1 = tok;

        tok = p.next().ScanToken;

        if (tok != ']') {
            p.errorf("register list: expected ']' after `[%s-%s`, found %s", loName, hiName, tok);
        }
        tok = tok__prev1;

    }


    a.Type = obj.TYPE_REGLIST;
    a.Reg = lo;
    a.Offset = x86.EncodeRegisterRange(lo, hi);

}

// register number is ARM-specific. It returns the number of the specified register.
private static ushort registerNumber(this ptr<Parser> _addr_p, @string name) {
    ref Parser p = ref _addr_p.val;

    if (p.arch.Family == sys.ARM && name == "g") {
        return 10;
    }
    if (name[0] != 'R') {
        p.errorf("expected g or R0 through R15; found %s", name);
        return 0;
    }
    var (r, ok) = p.registerReference(name);
    if (!ok) {
        return 0;
    }
    var reg = r - p.arch.Register["R0"];
    if (reg < 0) { 
        // Could happen for an architecture having other registers prefixed by R
        p.errorf("expected g or R0 through R15; found %s", name);
        return 0;

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
private static ulong expr(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    var value = p.term();
    while (true) {
        switch (p.peek()) {
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
private static double floatExpr(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    var tok = p.next();

    if (tok.ScanToken == '(') 
        var v = p.floatExpr();
        if (p.next().ScanToken != ')') {
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
    return 0;

}

// term = factor | factor ('*' | '/' | '%' | '>>' | '<<' | '&') factor
private static ulong term(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    var value = p.factor();
    while (true) {

        if (p.peek() == '*') 
            p.next();
            value *= p.factor();
        else if (p.peek() == '/') 
            p.next();
            if (int64(value) < 0) {
                p.errorf("divide of value with high bit set");
            }
            var divisor = p.factor();
            if (divisor == 0) {
                p.errorf("division by zero");
            }
            else
 {
                value /= divisor;
            }

        else if (p.peek() == '%') 
            p.next();
            divisor = p.factor();
            if (int64(value) < 0) {
                p.errorf("modulo of value with high bit set");
            }
            if (divisor == 0) {
                p.errorf("modulo by zero");
            }
            else
 {
                value %= divisor;
            }

        else if (p.peek() == lex.LSH) 
            p.next();
            var shift = p.factor();
            if (int64(shift) < 0) {
                p.errorf("negative left shift count");
            }
            return value << (int)(shift);
        else if (p.peek() == lex.RSH) 
            p.next();
            shift = p.term();
            if (int64(shift) < 0) {
                p.errorf("negative right shift count");
            }
            if (int64(value) < 0) {
                p.errorf("right shift of value with high bit set");
            }
            value>>=shift;
        else if (p.peek() == '&') 
            p.next();
            value &= p.factor();
        else 
            return value;
        
    }

}

// factor = const | '+' factor | '-' factor | '~' factor | '(' expr ')'
private static ulong factor(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    var tok = p.next();

    if (tok.ScanToken == scanner.Int) 
        return p.atoi(tok.String());
    else if (tok.ScanToken == scanner.Char) 
        var (str, err) = strconv.Unquote(tok.String());
        if (err != null) {
            p.errorf("%s", err);
        }
        var (r, w) = utf8.DecodeRuneInString(str);
        if (w == 1 && r == utf8.RuneError) {
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
        if (p.next().ScanToken != ')') {
            p.errorf("missing closing paren");
        }
        return v;
        p.errorf("unexpected %s evaluating expression", tok);
    return 0;

}

// positiveAtoi returns an int64 that must be >= 0.
private static long positiveAtoi(this ptr<Parser> _addr_p, @string str) {
    ref Parser p = ref _addr_p.val;

    var (value, err) = strconv.ParseInt(str, 0, 64);
    if (err != null) {
        p.errorf("%s", err);
    }
    if (value < 0) {
        p.errorf("%s overflows int64", str);
    }
    return value;

}

private static ulong atoi(this ptr<Parser> _addr_p, @string str) {
    ref Parser p = ref _addr_p.val;

    var (value, err) = strconv.ParseUint(str, 0, 64);
    if (err != null) {
        p.errorf("%s", err);
    }
    return value;

}

private static double atof(this ptr<Parser> _addr_p, @string str) {
    ref Parser p = ref _addr_p.val;

    var (value, err) = strconv.ParseFloat(str, 64);
    if (err != null) {
        p.errorf("%s", err);
    }
    return value;

}

// EOF represents the end of input.
public static var EOF = lex.Make(scanner.EOF, "EOF");

private static lex.Token next(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    if (!p.more()) {
        return EOF;
    }
    var tok = p.input[p.inputPos];
    p.inputPos++;
    return tok;

}

private static void back(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    if (p.inputPos == 0) {
        p.errorf("internal error: backing up before BOL");
    }
    else
 {
        p.inputPos--;
    }
}

private static lex.ScanToken peek(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    if (p.more()) {
        return p.input[p.inputPos].ScanToken;
    }
    return scanner.EOF;

}

private static bool more(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    return p.inputPos < len(p.input);
}

// get verifies that the next item has the expected type and returns it.
private static lex.Token get(this ptr<Parser> _addr_p, lex.ScanToken expected) {
    ref Parser p = ref _addr_p.val;

    p.expect(expected, expected.String());
    return p.next();
}

// expectOperandEnd verifies that the parsing state is properly at the end of an operand.
private static void expectOperandEnd(this ptr<Parser> _addr_p) {
    ref Parser p = ref _addr_p.val;

    p.expect(scanner.EOF, "end of operand");
}

// expect verifies that the next item has the expected type. It does not consume it.
private static void expect(this ptr<Parser> _addr_p, lex.ScanToken expectedToken, @string expectedMessage) {
    ref Parser p = ref _addr_p.val;

    if (p.peek() != expectedToken) {
        p.errorf("expected %s, found %s", expectedMessage, p.next());
    }
}

// have reports whether the remaining tokens (including the current one) contain the specified token.
private static bool have(this ptr<Parser> _addr_p, lex.ScanToken token) {
    ref Parser p = ref _addr_p.val;

    for (var i = p.inputPos; i < len(p.input); i++) {
        if (p.input[i].ScanToken == token) {
            return true;
        }
    }
    return false;

}

// at reports whether the next tokens are as requested.
private static bool at(this ptr<Parser> _addr_p, params lex.ScanToken[] next) {
    next = next.Clone();
    ref Parser p = ref _addr_p.val;

    if (len(p.input) - p.inputPos < len(next)) {
        return false;
    }
    foreach (var (i, r) in next) {
        if (p.input[p.inputPos + i].ScanToken != r) {
            return false;
        }
    }    return true;

}

} // end asm_package
