// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements scanner, a lexical tokenizer for
// Go source. After initialization, consecutive calls of
// next advance the scanner one token at a time.
//
// This file, source.go, tokens.go, and token_string.go are self-contained
// (`go tool compile scanner.go source.go tokens.go token_string.go` compiles)
// and thus could be made into their own package.

// package syntax -- go2cs converted at 2022 March 13 06:27:06 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\scanner.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using io = io_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// The mode flags below control which comments are reported
// by calling the error handler. If no flag is set, comments
// are ignored.

using System;
public static partial class syntax_package {

private static readonly nuint comments = 1 << (int)(iota); // call handler for all comments
private static readonly var directives = 0; // call handler for directives only

private partial struct scanner {
    public ref source source => ref source_val;
    public nuint mode;
    public bool nlsemi; // if set '\n' and EOF translate to ';'

// current token, valid after calling next()
    public nuint line;
    public nuint col;
    public bool blank; // line is blank up to col
    public token tok;
    public @string lit; // valid if tok is _Name, _Literal, or _Semi ("semicolon", "newline", or "EOF"); may be malformed if bad is true
    public bool bad; // valid if tok is _Literal, true if a syntax error occurred, lit may be malformed
    public LitKind kind; // valid if tok is _Literal
    public Operator op; // valid if tok is _Operator, _AssignOp, or _IncOp
    public nint prec; // valid if tok is _Operator, _AssignOp, or _IncOp
}

private static void init(this ptr<scanner> _addr_s, io.Reader src, Action<nuint, nuint, @string> errh, nuint mode) {
    ref scanner s = ref _addr_s.val;

    s.source.init(src, errh);
    s.mode = mode;
    s.nlsemi = false;
}

// errorf reports an error at the most recently read character position.
private static void errorf(this ptr<scanner> _addr_s, @string format, params object[] args) {
    args = args.Clone();
    ref scanner s = ref _addr_s.val;

    s.error(fmt.Sprintf(format, args));
}

// errorAtf reports an error at a byte column offset relative to the current token start.
private static void errorAtf(this ptr<scanner> _addr_s, nint offset, @string format, params object[] args) {
    args = args.Clone();
    ref scanner s = ref _addr_s.val;

    s.errh(s.line, s.col + uint(offset), fmt.Sprintf(format, args));
}

// setLit sets the scanner state for a recognized _Literal token.
private static void setLit(this ptr<scanner> _addr_s, LitKind kind, bool ok) {
    ref scanner s = ref _addr_s.val;

    s.nlsemi = true;
    s.tok = _Literal;
    s.lit = string(s.segment());
    s.bad = !ok;
    s.kind = kind;
}

// next advances the scanner by reading the next token.
//
// If a read, source encoding, or lexical error occurs, next calls
// the installed error handler with the respective error position
// and message. The error message is guaranteed to be non-empty and
// never starts with a '/'. The error handler must exist.
//
// If the scanner mode includes the comments flag and a comment
// (including comments containing directives) is encountered, the
// error handler is also called with each comment position and text
// (including opening /* or // and closing */, but without a newline
// at the end of line comments). Comment text always starts with a /
// which can be used to distinguish these handler calls from errors.
//
// If the scanner mode includes the directives (but not the comments)
// flag, only comments containing a //line, /*line, or //go: directive
// are reported, in the same way as regular comments.
private static void next(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;

    var nlsemi = s.nlsemi;
    s.nlsemi = false;

redo:
    s.stop();
    var (startLine, startCol) = s.pos();
    while (s.ch == ' ' || s.ch == '\t' || s.ch == '\n' && !nlsemi || s.ch == '\r') {
        s.nextch();
    } 

    // token start
    s.line, s.col = s.pos();
    s.blank = s.line > startLine || startCol == colbase;
    s.start();
    if (isLetter(s.ch) || s.ch >= utf8.RuneSelf && s.atIdentChar(true)) {
        s.nextch();
        s.ident();
        return ;
    }
    switch (s.ch) {
        case -1: 
            if (nlsemi) {
                s.lit = "EOF";
                s.tok = _Semi;
                break;
            }
            s.tok = _EOF;
            break;
        case '\n': 
            s.nextch();
            s.lit = "newline";
            s.tok = _Semi;
            break;
        case '0': 

        case '1': 

        case '2': 

        case '3': 

        case '4': 

        case '5': 

        case '6': 

        case '7': 

        case '8': 

        case '9': 
            s.number(false);
            break;
        case '"': 
            s.stdString();
            break;
        case '`': 
            s.rawString();
            break;
        case '\'': 
            s.rune();
            break;
        case '(': 
            s.nextch();
            s.tok = _Lparen;
            break;
        case '[': 
            s.nextch();
            s.tok = _Lbrack;
            break;
        case '{': 
            s.nextch();
            s.tok = _Lbrace;
            break;
        case ',': 
            s.nextch();
            s.tok = _Comma;
            break;
        case ';': 
            s.nextch();
            s.lit = "semicolon";
            s.tok = _Semi;
            break;
        case ')': 
            s.nextch();
            s.nlsemi = true;
            s.tok = _Rparen;
            break;
        case ']': 
            s.nextch();
            s.nlsemi = true;
            s.tok = _Rbrack;
            break;
        case '}': 
            s.nextch();
            s.nlsemi = true;
            s.tok = _Rbrace;
            break;
        case ':': 
            s.nextch();
            if (s.ch == '=') {
                s.nextch();
                s.tok = _Define;
                break;
            }
            s.tok = _Colon;
            break;
        case '.': 
            s.nextch();
            if (isDecimal(s.ch)) {
                s.number(true);
                break;
            }
            if (s.ch == '.') {
                s.nextch();
                if (s.ch == '.') {
                    s.nextch();
                    s.tok = _DotDotDot;
                    break;
                }
                s.rewind(); // now s.ch holds 1st '.'
                s.nextch(); // consume 1st '.' again
            }
            s.tok = _Dot;
            break;
        case '+': 
            s.nextch();
            (s.op, s.prec) = (Add, precAdd);        if (s.ch != '+') {
                goto assignop;
            }
            s.nextch();
            s.nlsemi = true;
            s.tok = _IncOp;
            break;
        case '-': 
            s.nextch();
            (s.op, s.prec) = (Sub, precAdd);        if (s.ch != '-') {
                goto assignop;
            }
            s.nextch();
            s.nlsemi = true;
            s.tok = _IncOp;
            break;
        case '*': 
            s.nextch();
            (s.op, s.prec) = (Mul, precMul);        if (s.ch == '=') {
                s.nextch();
                s.tok = _AssignOp;
                break;
            }
            s.tok = _Star;
            break;
        case '/': 
            s.nextch();
            if (s.ch == '/') {
                s.nextch();
                s.lineComment();
                goto redo;
            }
            if (s.ch == '*') {
                s.nextch();
                s.fullComment();
                {
                    var (line, _) = s.pos();

                    if (line > s.line && nlsemi) { 
                        // A multi-line comment acts like a newline;
                        // it translates to a ';' if nlsemi is set.
                        s.lit = "newline";
                        s.tok = _Semi;
                        break;
                    }

                }
                goto redo;
            }
            (s.op, s.prec) = (Div, precMul);        goto assignop;
            break;
        case '%': 
            s.nextch();
            (s.op, s.prec) = (Rem, precMul);        goto assignop;
            break;
        case '&': 
            s.nextch();
            if (s.ch == '&') {
                s.nextch();
                (s.op, s.prec) = (AndAnd, precAndAnd);            s.tok = _Operator;
                break;
            }
            (s.op, s.prec) = (And, precMul);        if (s.ch == '^') {
                s.nextch();
                s.op = AndNot;
            }
            goto assignop;
            break;
        case '|': 
            s.nextch();
            if (s.ch == '|') {
                s.nextch();
                (s.op, s.prec) = (OrOr, precOrOr);            s.tok = _Operator;
                break;
            }
            (s.op, s.prec) = (Or, precAdd);        goto assignop;
            break;
        case '^': 
            s.nextch();
            (s.op, s.prec) = (Xor, precAdd);        goto assignop;
            break;
        case '<': 
            s.nextch();
            if (s.ch == '=') {
                s.nextch();
                (s.op, s.prec) = (Leq, precCmp);            s.tok = _Operator;
                break;
            }
            if (s.ch == '<') {
                s.nextch();
                (s.op, s.prec) = (Shl, precMul);            goto assignop;
            }
            if (s.ch == '-') {
                s.nextch();
                s.tok = _Arrow;
                break;
            }
            (s.op, s.prec) = (Lss, precCmp);        s.tok = _Operator;
            break;
        case '>': 
            s.nextch();
            if (s.ch == '=') {
                s.nextch();
                (s.op, s.prec) = (Geq, precCmp);            s.tok = _Operator;
                break;
            }
            if (s.ch == '>') {
                s.nextch();
                (s.op, s.prec) = (Shr, precMul);            goto assignop;
            }
            (s.op, s.prec) = (Gtr, precCmp);        s.tok = _Operator;
            break;
        case '=': 
            s.nextch();
            if (s.ch == '=') {
                s.nextch();
                (s.op, s.prec) = (Eql, precCmp);            s.tok = _Operator;
                break;
            }
            s.tok = _Assign;
            break;
        case '!': 
            s.nextch();
            if (s.ch == '=') {
                s.nextch();
                (s.op, s.prec) = (Neq, precCmp);            s.tok = _Operator;
                break;
            }
            (s.op, s.prec) = (Not, 0);        s.tok = _Operator;
            break;
        case '~': 
            s.nextch();
            (s.op, s.prec) = (Tilde, 0);        s.tok = _Operator;
            break;
        default: 
            s.errorf("invalid character %#U", s.ch);
            s.nextch();
            goto redo;
            break;
    }

    return ;

assignop:
    if (s.ch == '=') {
        s.nextch();
        s.tok = _AssignOp;
        return ;
    }
    s.tok = _Operator;
}

private static void ident(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;
 
    // accelerate common case (7bit ASCII)
    while (isLetter(s.ch) || isDecimal(s.ch)) {
        s.nextch();
    } 

    // general case
    if (s.ch >= utf8.RuneSelf) {
        while (s.atIdentChar(false)) {
            s.nextch();
        }
    }
    var lit = s.segment();
    if (len(lit) >= 2) {
        {
            var tok = keywordMap[hash(lit)];

            if (tok != 0 && tokStrFast(tok) == string(lit)) {
                s.nlsemi = contains(1 << (int)(_Break) | 1 << (int)(_Continue) | 1 << (int)(_Fallthrough) | 1 << (int)(_Return), tok);
                s.tok = tok;
                return ;
            }

        }
    }
    s.nlsemi = true;
    s.lit = string(lit);
    s.tok = _Name;
}

// tokStrFast is a faster version of token.String, which assumes that tok
// is one of the valid tokens - and can thus skip bounds checks.
private static @string tokStrFast(token tok) {
    return _token_name[(int)_token_index[tok - 1]..(int)_token_index[tok]];
}

private static bool atIdentChar(this ptr<scanner> _addr_s, bool first) {
    ref scanner s = ref _addr_s.val;


    if (unicode.IsLetter(s.ch) || s.ch == '_')     else if (unicode.IsDigit(s.ch)) 
        if (first) {
            s.errorf("identifier cannot begin with digit %#U", s.ch);
        }
    else if (s.ch >= utf8.RuneSelf) 
        s.errorf("invalid character %#U in identifier", s.ch);
    else 
        return false;
        return true;
}

// hash is a perfect hash function for keywords.
// It assumes that s has at least length 2.
private static nuint hash(slice<byte> s) {
    return (uint(s[0]) << 4 ^ uint(s[1]) + uint(len(s))) & uint(len(keywordMap) - 1);
}

private static array<token> keywordMap = new array<token>(1 << 6); // size must be power of two

private static void init() => func((_, panic, _) => { 
    // populate keywordMap
    for (var tok = _Break; tok <= _Var; tok++) {
        var h = hash((slice<byte>)tok.String());
        if (keywordMap[h] != 0) {
            panic("imperfect hash");
        }
        keywordMap[h] = tok;
    }
});

private static int lower(int ch) {
    return ('a' - 'A') | ch;
} // returns lower-case ch iff ch is ASCII letter
private static bool isLetter(int ch) {
    return 'a' <= lower(ch) && lower(ch) <= 'z' || ch == '_';
}
private static bool isDecimal(int ch) {
    return '0' <= ch && ch <= '9';
}
private static bool isHex(int ch) {
    return '0' <= ch && ch <= '9' || 'a' <= lower(ch) && lower(ch) <= 'f';
}

// digits accepts the sequence { digit | '_' }.
// If base <= 10, digits accepts any decimal digit but records
// the index (relative to the literal start) of a digit >= base
// in *invalid, if *invalid < 0.
// digits returns a bitset describing whether the sequence contained
// digits (bit 0 is set), or separators '_' (bit 1 is set).
private static nint digits(this ptr<scanner> _addr_s, nint @base, ptr<nint> _addr_invalid) {
    nint digsep = default;
    ref scanner s = ref _addr_s.val;
    ref nint invalid = ref _addr_invalid.val;

    if (base <= 10) {
        var max = rune('0' + base);
        while (isDecimal(s.ch) || s.ch == '_') {
            nint ds = 1;
            if (s.ch == '_') {
                ds = 2;
            }
            else if (s.ch >= max && invalid < 0.val) {
                var (_, col) = s.pos();
                invalid = int(col - s.col); // record invalid rune index
            }
            digsep |= ds;
            s.nextch();
        }
    else
    } {
        while (isHex(s.ch) || s.ch == '_') {
            ds = 1;
            if (s.ch == '_') {
                ds = 2;
            }
            digsep |= ds;
            s.nextch();
        }
    }
    return ;
}

private static void number(this ptr<scanner> _addr_s, bool seenPoint) {
    ref scanner s = ref _addr_s.val;

    var ok = true;
    var kind = IntLit;
    nint @base = 10; // number base
    var prefix = rune(0); // one of 0 (decimal), '0' (0-octal), 'x', 'o', or 'b'
    nint digsep = 0; // bit 0: digit present, bit 1: '_' present
    ref nint invalid = ref heap(-1, out ptr<nint> _addr_invalid); // index of invalid digit in literal, or < 0

    // integer part
    if (!seenPoint) {
        if (s.ch == '0') {
            s.nextch();
            switch (lower(s.ch)) {
                case 'x': 
                    s.nextch();
                    (base, prefix) = (16, 'x');
                    break;
                case 'o': 
                    s.nextch();
                    (base, prefix) = (8, 'o');
                    break;
                case 'b': 
                    s.nextch();
                    (base, prefix) = (2, 'b');
                    break;
                default: 
                    (base, prefix) = (8, '0');                digsep = 1; // leading 0
                    break;
            }
        }
        digsep |= s.digits(base, _addr_invalid);
        if (s.ch == '.') {
            if (prefix == 'o' || prefix == 'b') {
                s.errorf("invalid radix point in %s literal", baseName(base));
                ok = false;
            }
            s.nextch();
            seenPoint = true;
        }
    }
    if (seenPoint) {
        kind = FloatLit;
        digsep |= s.digits(base, _addr_invalid);
    }
    if (digsep & 1 == 0 && ok) {
        s.errorf("%s literal has no digits", baseName(base));
        ok = false;
    }
    {
        var e = lower(s.ch);

        if (e == 'e' || e == 'p') {
            if (ok) {

                if (e == 'e' && prefix != 0 && prefix != '0') 
                    s.errorf("%q exponent requires decimal mantissa", s.ch);
                    ok = false;
                else if (e == 'p' && prefix != 'x') 
                    s.errorf("%q exponent requires hexadecimal mantissa", s.ch);
                    ok = false;
                            }
            s.nextch();
            kind = FloatLit;
            if (s.ch == '+' || s.ch == '-') {
                s.nextch();
            }
            digsep = s.digits(10, null) | digsep & 2; // don't lose sep bit
            if (digsep & 1 == 0 && ok) {
                s.errorf("exponent has no digits");
                ok = false;
            }
        }
        else if (prefix == 'x' && kind == FloatLit && ok) {
            s.errorf("hexadecimal mantissa requires a 'p' exponent");
            ok = false;
        }

    } 

    // suffix 'i'
    if (s.ch == 'i') {
        kind = ImagLit;
        s.nextch();
    }
    s.setLit(kind, ok); // do this now so we can use s.lit below

    if (kind == IntLit && invalid >= 0 && ok) {
        s.errorAtf(invalid, "invalid digit %q in %s literal", s.lit[invalid], baseName(base));
        ok = false;
    }
    if (digsep & 2 != 0 && ok) {
        {
            var i = invalidSep(s.lit);

            if (i >= 0) {
                s.errorAtf(i, "'_' must separate successive digits");
                ok = false;
            }

        }
    }
    s.bad = !ok; // correct s.bad
}

private static @string baseName(nint @base) => func((_, panic, _) => {
    switch (base) {
        case 2: 
            return "binary";
            break;
        case 8: 
            return "octal";
            break;
        case 10: 
            return "decimal";
            break;
        case 16: 
            return "hexadecimal";
            break;
    }
    panic("invalid base");
});

// invalidSep returns the index of the first invalid separator in x, or -1.
private static nint invalidSep(@string x) {
    char x1 = ' '; // prefix char, we only care if it's 'x'
    char d = '.'; // digit, one of '_', '0' (a digit), or '.' (anything else)
    nint i = 0; 

    // a prefix counts as a digit
    if (len(x) >= 2 && x[0] == '0') {
        x1 = lower(rune(x[1]));
        if (x1 == 'x' || x1 == 'o' || x1 == 'b') {
            d = '0';
            i = 2;
        }
    }
    while (i < len(x)) {
        var p = d; // previous digit
        d = rune(x[i]);

        if (d == '_') 
            if (p != '0') {
                return i;
        i++;
            }
        else if (isDecimal(d) || x1 == 'x' && isHex(d)) 
            d = '0';
        else 
            if (p == '_') {
                return i - 1;
            }
            d = '.';
            }
    if (d == '_') {
        return len(x) - 1;
    }
    return -1;
}

private static void rune(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;

    var ok = true;
    s.nextch();

    nint n = 0;
    while () {
        if (s.ch == '\'') {
            if (ok) {
                if (n == 0) {
                    s.errorf("empty rune literal or unescaped '");
                    ok = false;
        n++;
                }
                else if (n != 1) {
                    s.errorAtf(0, "more than one character in rune literal");
                    ok = false;
                }
            }
            s.nextch();
            break;
        }
        if (s.ch == '\\') {
            s.nextch();
            if (!s.escape('\'')) {
                ok = false;
            }
            continue;
        }
        if (s.ch == '\n') {
            if (ok) {
                s.errorf("newline in rune literal");
                ok = false;
            }
            break;
        }
        if (s.ch < 0) {
            if (ok) {
                s.errorAtf(0, "rune literal not terminated");
                ok = false;
            }
            break;
        }
        s.nextch();
    }

    s.setLit(RuneLit, ok);
}

private static void stdString(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;

    var ok = true;
    s.nextch();

    while (true) {
        if (s.ch == '"') {
            s.nextch();
            break;
        }
        if (s.ch == '\\') {
            s.nextch();
            if (!s.escape('"')) {
                ok = false;
            }
            continue;
        }
        if (s.ch == '\n') {
            s.errorf("newline in string");
            ok = false;
            break;
        }
        if (s.ch < 0) {
            s.errorAtf(0, "string not terminated");
            ok = false;
            break;
        }
        s.nextch();
    }

    s.setLit(StringLit, ok);
}

private static void rawString(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;

    var ok = true;
    s.nextch();

    while (true) {
        if (s.ch == '`') {
            s.nextch();
            break;
        }
        if (s.ch < 0) {
            s.errorAtf(0, "string not terminated");
            ok = false;
            break;
        }
        s.nextch();
    } 
    // We leave CRs in the string since they are part of the
    // literal (even though they are not part of the literal
    // value).

    s.setLit(StringLit, ok);
}

private static void comment(this ptr<scanner> _addr_s, @string text) {
    ref scanner s = ref _addr_s.val;

    s.errorAtf(0, "%s", text);
}

private static void skipLine(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;
 
    // don't consume '\n' - needed for nlsemi logic
    while (s.ch >= 0 && s.ch != '\n') {
        s.nextch();
    }
}

private static void lineComment(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;
 
    // opening has already been consumed

    if (s.mode & comments != 0) {
        s.skipLine();
        s.comment(string(s.segment()));
        return ;
    }
    if (s.mode & directives == 0 || (s.ch != 'g' && s.ch != 'l')) {
        s.stop();
        s.skipLine();
        return ;
    }
    @string prefix = "go:";
    if (s.ch == 'l') {
        prefix = "line ";
    }
    foreach (var (_, m) in prefix) {
        if (s.ch != m) {
            s.stop();
            s.skipLine();
            return ;
        }
        s.nextch();
    }    s.skipLine();
    s.comment(string(s.segment()));
}

private static bool skipComment(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;

    while (s.ch >= 0) {
        while (s.ch == '*') {
            s.nextch();
            if (s.ch == '/') {
                s.nextch();
                return true;
            }
        }
        s.nextch();
    }
    s.errorAtf(0, "comment not terminated");
    return false;
}

private static void fullComment(this ptr<scanner> _addr_s) {
    ref scanner s = ref _addr_s.val;

    /* opening has already been consumed */

    if (s.mode & comments != 0) {
        if (s.skipComment()) {
            s.comment(string(s.segment()));
        }
        return ;
    }
    if (s.mode & directives == 0 || s.ch != 'l') {
        s.stop();
        s.skipComment();
        return ;
    }
    const @string prefix = "line ";

    foreach (var (_, m) in prefix) {
        if (s.ch != m) {
            s.stop();
            s.skipComment();
            return ;
        }
        s.nextch();
    }    if (s.skipComment()) {
        s.comment(string(s.segment()));
    }
}

private static bool escape(this ptr<scanner> _addr_s, int quote) {
    ref scanner s = ref _addr_s.val;

    nint n = default;
    uint @base = default;    uint max = default;




    if (s.ch == quote || s.ch == 'a' || s.ch == 'b' || s.ch == 'f' || s.ch == 'n' || s.ch == 'r' || s.ch == 't' || s.ch == 'v' || s.ch == '\\') 
        s.nextch();
        return true;
    else if (s.ch == '0' || s.ch == '1' || s.ch == '2' || s.ch == '3' || s.ch == '4' || s.ch == '5' || s.ch == '6' || s.ch == '7') 
        (n, base, max) = (3, 8, 255);    else if (s.ch == 'x') 
        s.nextch();
        (n, base, max) = (2, 16, 255);    else if (s.ch == 'u') 
        s.nextch();
        (n, base, max) = (4, 16, unicode.MaxRune);    else if (s.ch == 'U') 
        s.nextch();
        (n, base, max) = (8, 16, unicode.MaxRune);    else 
        if (s.ch < 0) {
            return true; // complain in caller about EOF
        }
        s.errorf("unknown escape");
        return false;
        uint x = default;
    for (var i = n; i > 0; i--) {
        if (s.ch < 0) {
            return true; // complain in caller about EOF
        }
        var d = base;
        if (isDecimal(s.ch)) {
            d = uint32(s.ch) - '0';
        }
        else if ('a' <= lower(s.ch) && lower(s.ch) <= 'f') {
            d = uint32(lower(s.ch)) - 'a' + 10;
        }
        if (d >= base) {
            s.errorf("invalid character %q in %s escape", s.ch, baseName(int(base)));
            return false;
        }
        x = x * base + d;
        s.nextch();
    }

    if (x > max && base == 8) {
        s.errorf("octal escape value %d > 255", x);
        return false;
    }
    if (x > max || 0xD800 <= x && x < 0xE000) {
        s.errorf("escape is invalid Unicode code point %#U", x);
        return false;
    }
    return true;
}

} // end syntax_package
