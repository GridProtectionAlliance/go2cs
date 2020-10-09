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

// package syntax -- go2cs converted at 2020 October 09 05:41:06 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\scanner.go
using fmt = go.fmt_package;
using io = go.io_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        // The mode flags below control which comments are reported
        // by calling the error handler. If no flag is set, comments
        // are ignored.
        private static readonly ulong comments = (ulong)1L << (int)(iota); // call handler for all comments
        private static readonly var directives = 0; // call handler for directives only

        private partial struct scanner
        {
            public ref source source => ref source_val;
            public ulong mode;
            public bool nlsemi; // if set '\n' and EOF translate to ';'

// current token, valid after calling next()
            public ulong line;
            public ulong col;
            public bool blank; // line is blank up to col
            public token tok;
            public @string lit; // valid if tok is _Name, _Literal, or _Semi ("semicolon", "newline", or "EOF"); may be malformed if bad is true
            public bool bad; // valid if tok is _Literal, true if a syntax error occurred, lit may be malformed
            public LitKind kind; // valid if tok is _Literal
            public Operator op; // valid if tok is _Operator, _AssignOp, or _IncOp
            public long prec; // valid if tok is _Operator, _AssignOp, or _IncOp
        }

        private static void init(this ptr<scanner> _addr_s, io.Reader src, Action<ulong, ulong, @string> errh, ulong mode)
        {
            ref scanner s = ref _addr_s.val;

            s.source.init(src, errh);
            s.mode = mode;
            s.nlsemi = false;
        }

        // errorf reports an error at the most recently read character position.
        private static void errorf(this ptr<scanner> _addr_s, @string format, params object[] args)
        {
            args = args.Clone();
            ref scanner s = ref _addr_s.val;

            s.error(fmt.Sprintf(format, args));
        }

        // errorAtf reports an error at a byte column offset relative to the current token start.
        private static void errorAtf(this ptr<scanner> _addr_s, long offset, @string format, params object[] args)
        {
            args = args.Clone();
            ref scanner s = ref _addr_s.val;

            s.errh(s.line, s.col + uint(offset), fmt.Sprintf(format, args));
        }

        // setLit sets the scanner state for a recognized _Literal token.
        private static void setLit(this ptr<scanner> _addr_s, LitKind kind, bool ok)
        {
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
        private static void next(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            var nlsemi = s.nlsemi;
            s.nlsemi = false;

redo:
            s.stop();
            var (startLine, startCol) = s.pos();
            while (s.ch == ' ' || s.ch == '\t' || s.ch == '\n' && !nlsemi || s.ch == '\r')
            {
                s.nextch();
            } 

            // token start
 

            // token start
            s.line, s.col = s.pos();
            s.blank = s.line > startLine || startCol == colbase;
            s.start();
            if (isLetter(s.ch) || s.ch >= utf8.RuneSelf && s.atIdentChar(true))
            {
                s.nextch();
                s.ident();
                return ;
            }

            switch (s.ch)
            {
                case -1L: 
                    if (nlsemi)
                    {
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
                    if (s.ch == '=')
                    {
                        s.nextch();
                        s.tok = _Define;
                        break;
                    }

                    s.tok = _Colon;
                    break;
                case '.': 
                    s.nextch();
                    if (isDecimal(s.ch))
                    {
                        s.number(true);
                        break;
                    }

                    if (s.ch == '.')
                    {
                        s.nextch();
                        if (s.ch == '.')
                        {
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
                    s.op = Add;
                    s.prec = precAdd;
                    if (s.ch != '+')
                    {
                        goto assignop;
                    }

                    s.nextch();
                    s.nlsemi = true;
                    s.tok = _IncOp;
                    break;
                case '-': 
                    s.nextch();
                    s.op = Sub;
                    s.prec = precAdd;
                    if (s.ch != '-')
                    {
                        goto assignop;
                    }

                    s.nextch();
                    s.nlsemi = true;
                    s.tok = _IncOp;
                    break;
                case '*': 
                    s.nextch();
                    s.op = Mul;
                    s.prec = precMul; 
                    // don't goto assignop - want _Star token
                    if (s.ch == '=')
                    {
                        s.nextch();
                        s.tok = _AssignOp;
                        break;
                    }

                    s.tok = _Star;
                    break;
                case '/': 
                    s.nextch();
                    if (s.ch == '/')
                    {
                        s.nextch();
                        s.lineComment();
                        goto redo;
                    }

                    if (s.ch == '*')
                    {
                        s.nextch();
                        s.fullComment();
                        {
                            var (line, _) = s.pos();

                            if (line > s.line && nlsemi)
                            { 
                                // A multi-line comment acts like a newline;
                                // it translates to a ';' if nlsemi is set.
                                s.lit = "newline";
                                s.tok = _Semi;
                                break;

                            }

                        }

                        goto redo;

                    }

                    s.op = Div;
                    s.prec = precMul;
                    goto assignop;
                    break;
                case '%': 
                    s.nextch();
                    s.op = Rem;
                    s.prec = precMul;
                    goto assignop;
                    break;
                case '&': 
                    s.nextch();
                    if (s.ch == '&')
                    {
                        s.nextch();
                        s.op = AndAnd;
                        s.prec = precAndAnd;
                        s.tok = _Operator;
                        break;

                    }

                    s.op = And;
                    s.prec = precMul;
                    if (s.ch == '^')
                    {
                        s.nextch();
                        s.op = AndNot;
                    }

                    goto assignop;
                    break;
                case '|': 
                    s.nextch();
                    if (s.ch == '|')
                    {
                        s.nextch();
                        s.op = OrOr;
                        s.prec = precOrOr;
                        s.tok = _Operator;
                        break;

                    }

                    s.op = Or;
                    s.prec = precAdd;
                    goto assignop;
                    break;
                case '^': 
                    s.nextch();
                    s.op = Xor;
                    s.prec = precAdd;
                    goto assignop;
                    break;
                case '<': 
                    s.nextch();
                    if (s.ch == '=')
                    {
                        s.nextch();
                        s.op = Leq;
                        s.prec = precCmp;
                        s.tok = _Operator;
                        break;

                    }

                    if (s.ch == '<')
                    {
                        s.nextch();
                        s.op = Shl;
                        s.prec = precMul;
                        goto assignop;

                    }

                    if (s.ch == '-')
                    {
                        s.nextch();
                        s.tok = _Arrow;
                        break;
                    }

                    s.op = Lss;
                    s.prec = precCmp;
                    s.tok = _Operator;
                    break;
                case '>': 
                    s.nextch();
                    if (s.ch == '=')
                    {
                        s.nextch();
                        s.op = Geq;
                        s.prec = precCmp;
                        s.tok = _Operator;
                        break;

                    }

                    if (s.ch == '>')
                    {
                        s.nextch();
                        s.op = Shr;
                        s.prec = precMul;
                        goto assignop;

                    }

                    s.op = Gtr;
                    s.prec = precCmp;
                    s.tok = _Operator;
                    break;
                case '=': 
                    s.nextch();
                    if (s.ch == '=')
                    {
                        s.nextch();
                        s.op = Eql;
                        s.prec = precCmp;
                        s.tok = _Operator;
                        break;

                    }

                    s.tok = _Assign;
                    break;
                case '!': 
                    s.nextch();
                    if (s.ch == '=')
                    {
                        s.nextch();
                        s.op = Neq;
                        s.prec = precCmp;
                        s.tok = _Operator;
                        break;

                    }

                    s.op = Not;
                    s.prec = 0L;
                    s.tok = _Operator;
                    break;
                default: 
                    s.errorf("invalid character %#U", s.ch);
                    s.nextch();
                    goto redo;
                    break;
            }

            return ;

assignop:
            if (s.ch == '=')
            {
                s.nextch();
                s.tok = _AssignOp;
                return ;
            }

            s.tok = _Operator;

        }

        private static void ident(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;
 
            // accelerate common case (7bit ASCII)
            while (isLetter(s.ch) || isDecimal(s.ch))
            {
                s.nextch();
            } 

            // general case
 

            // general case
            if (s.ch >= utf8.RuneSelf)
            {
                while (s.atIdentChar(false))
                {
                    s.nextch();
                }


            } 

            // possibly a keyword
            var lit = s.segment();
            if (len(lit) >= 2L)
            {
                {
                    var tok = keywordMap[hash(lit)];

                    if (tok != 0L && tokStrFast(tok) == string(lit))
                    {
                        s.nlsemi = contains(1L << (int)(_Break) | 1L << (int)(_Continue) | 1L << (int)(_Fallthrough) | 1L << (int)(_Return), tok);
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
        private static @string tokStrFast(token tok)
        {
            return _token_name[_token_index[tok - 1L].._token_index[tok]];
        }

        private static bool atIdentChar(this ptr<scanner> _addr_s, bool first)
        {
            ref scanner s = ref _addr_s.val;


            if (unicode.IsLetter(s.ch) || s.ch == '_')             else if (unicode.IsDigit(s.ch)) 
                if (first)
                {
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
        private static ulong hash(slice<byte> s)
        {
            return (uint(s[0L]) << (int)(4L) ^ uint(s[1L]) + uint(len(s))) & uint(len(keywordMap) - 1L);
        }

        private static array<token> keywordMap = new array<token>(1L << (int)(6L)); // size must be power of two

        private static void init() => func((_, panic, __) =>
        { 
            // populate keywordMap
            for (var tok = _Break; tok <= _Var; tok++)
            {
                var h = hash((slice<byte>)tok.String());
                if (keywordMap[h] != 0L)
                {
                    panic("imperfect hash");
                }

                keywordMap[h] = tok;

            }


        });

        private static int lower(int ch)
        {
            return ('a' - 'A') | ch;
        } // returns lower-case ch iff ch is ASCII letter
        private static bool isLetter(int ch)
        {
            return 'a' <= lower(ch) && lower(ch) <= 'z' || ch == '_';
        }
        private static bool isDecimal(int ch)
        {
            return '0' <= ch && ch <= '9';
        }
        private static bool isHex(int ch)
        {
            return '0' <= ch && ch <= '9' || 'a' <= lower(ch) && lower(ch) <= 'f';
        }

        // digits accepts the sequence { digit | '_' }.
        // If base <= 10, digits accepts any decimal digit but records
        // the index (relative to the literal start) of a digit >= base
        // in *invalid, if *invalid < 0.
        // digits returns a bitset describing whether the sequence contained
        // digits (bit 0 is set), or separators '_' (bit 1 is set).
        private static long digits(this ptr<scanner> _addr_s, long @base, ptr<long> _addr_invalid)
        {
            long digsep = default;
            ref scanner s = ref _addr_s.val;
            ref long invalid = ref _addr_invalid.val;

            if (base <= 10L)
            {
                var max = rune('0' + base);
                while (isDecimal(s.ch) || s.ch == '_')
                {
                    long ds = 1L;
                    if (s.ch == '_')
                    {
                        ds = 2L;
                    }
                    else if (s.ch >= max && invalid < 0L.val)
                    {
                        var (_, col) = s.pos();
                        invalid = int(col - s.col); // record invalid rune index
                    }

                    digsep |= ds;
                    s.nextch();

                }
            else


            }            {
                while (isHex(s.ch) || s.ch == '_')
                {
                    ds = 1L;
                    if (s.ch == '_')
                    {
                        ds = 2L;
                    }

                    digsep |= ds;
                    s.nextch();

                }


            }

            return ;

        }

        private static void number(this ptr<scanner> _addr_s, bool seenPoint)
        {
            ref scanner s = ref _addr_s.val;

            var ok = true;
            var kind = IntLit;
            long @base = 10L; // number base
            var prefix = rune(0L); // one of 0 (decimal), '0' (0-octal), 'x', 'o', or 'b'
            long digsep = 0L; // bit 0: digit present, bit 1: '_' present
            ref long invalid = ref heap(-1L, out ptr<long> _addr_invalid); // index of invalid digit in literal, or < 0

            // integer part
            if (!seenPoint)
            {
                if (s.ch == '0')
                {
                    s.nextch();
                    switch (lower(s.ch))
                    {
                        case 'x': 
                            s.nextch();
                            base = 16L;
                            prefix = 'x';
                            break;
                        case 'o': 
                            s.nextch();
                            base = 8L;
                            prefix = 'o';
                            break;
                        case 'b': 
                            s.nextch();
                            base = 2L;
                            prefix = 'b';
                            break;
                        default: 
                            base = 8L;
                            prefix = '0';
                            digsep = 1L; // leading 0
                            break;
                    }

                }

                digsep |= s.digits(base, _addr_invalid);
                if (s.ch == '.')
                {
                    if (prefix == 'o' || prefix == 'b')
                    {
                        s.errorf("invalid radix point in %s literal", baseName(base));
                        ok = false;
                    }

                    s.nextch();
                    seenPoint = true;

                }

            } 

            // fractional part
            if (seenPoint)
            {
                kind = FloatLit;
                digsep |= s.digits(base, _addr_invalid);
            }

            if (digsep & 1L == 0L && ok)
            {
                s.errorf("%s literal has no digits", baseName(base));
                ok = false;
            } 

            // exponent
            {
                var e = lower(s.ch);

                if (e == 'e' || e == 'p')
                {
                    if (ok)
                    {

                        if (e == 'e' && prefix != 0L && prefix != '0') 
                            s.errorf("%q exponent requires decimal mantissa", s.ch);
                            ok = false;
                        else if (e == 'p' && prefix != 'x') 
                            s.errorf("%q exponent requires hexadecimal mantissa", s.ch);
                            ok = false;
                        
                    }

                    s.nextch();
                    kind = FloatLit;
                    if (s.ch == '+' || s.ch == '-')
                    {
                        s.nextch();
                    }

                    digsep = s.digits(10L, null) | digsep & 2L; // don't lose sep bit
                    if (digsep & 1L == 0L && ok)
                    {
                        s.errorf("exponent has no digits");
                        ok = false;
                    }

                }
                else if (prefix == 'x' && kind == FloatLit && ok)
                {
                    s.errorf("hexadecimal mantissa requires a 'p' exponent");
                    ok = false;
                } 

                // suffix 'i'

            } 

            // suffix 'i'
            if (s.ch == 'i')
            {
                kind = ImagLit;
                s.nextch();
            }

            s.setLit(kind, ok); // do this now so we can use s.lit below

            if (kind == IntLit && invalid >= 0L && ok)
            {
                s.errorAtf(invalid, "invalid digit %q in %s literal", s.lit[invalid], baseName(base));
                ok = false;
            }

            if (digsep & 2L != 0L && ok)
            {
                {
                    var i = invalidSep(s.lit);

                    if (i >= 0L)
                    {
                        s.errorAtf(i, "'_' must separate successive digits");
                        ok = false;
                    }

                }

            }

            s.bad = !ok; // correct s.bad
        }

        private static @string baseName(long @base) => func((_, panic, __) =>
        {
            switch (base)
            {
                case 2L: 
                    return "binary";
                    break;
                case 8L: 
                    return "octal";
                    break;
                case 10L: 
                    return "decimal";
                    break;
                case 16L: 
                    return "hexadecimal";
                    break;
            }
            panic("invalid base");

        });

        // invalidSep returns the index of the first invalid separator in x, or -1.
        private static long invalidSep(@string x)
        {
            char x1 = ' '; // prefix char, we only care if it's 'x'
            char d = '.'; // digit, one of '_', '0' (a digit), or '.' (anything else)
            long i = 0L; 

            // a prefix counts as a digit
            if (len(x) >= 2L && x[0L] == '0')
            {
                x1 = lower(rune(x[1L]));
                if (x1 == 'x' || x1 == 'o' || x1 == 'b')
                {
                    d = '0';
                    i = 2L;
                }

            } 

            // mantissa and exponent
            while (i < len(x))
            {
                var p = d; // previous digit
                d = rune(x[i]);

                if (d == '_') 
                    if (p != '0')
                    {
                        return i;
                i++;
                    }

                else if (isDecimal(d) || x1 == 'x' && isHex(d)) 
                    d = '0';
                else 
                    if (p == '_')
                    {
                        return i - 1L;
                    }

                    d = '.';
                
            }

            if (d == '_')
            {
                return len(x) - 1L;
            }

            return -1L;

        }

        private static void rune(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            var ok = true;
            s.nextch();

            long n = 0L;
            while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
            {
                if (s.ch == '\'')
                {
                    if (ok)
                    {
                        if (n == 0L)
                        {
                            s.errorf("empty rune literal or unescaped '");
                            ok = false;
                n++;
                        }
                        else if (n != 1L)
                        {
                            s.errorAtf(0L, "more than one character in rune literal");
                            ok = false;
                        }

                    }

                    s.nextch();
                    break;

                }

                if (s.ch == '\\')
                {
                    s.nextch();
                    if (!s.escape('\''))
                    {
                        ok = false;
                    }

                    continue;

                }

                if (s.ch == '\n')
                {
                    if (ok)
                    {
                        s.errorf("newline in rune literal");
                        ok = false;
                    }

                    break;

                }

                if (s.ch < 0L)
                {
                    if (ok)
                    {
                        s.errorAtf(0L, "rune literal not terminated");
                        ok = false;
                    }

                    break;

                }

                s.nextch();

            }


            s.setLit(RuneLit, ok);

        }

        private static void stdString(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            var ok = true;
            s.nextch();

            while (true)
            {
                if (s.ch == '"')
                {
                    s.nextch();
                    break;
                }

                if (s.ch == '\\')
                {
                    s.nextch();
                    if (!s.escape('"'))
                    {
                        ok = false;
                    }

                    continue;

                }

                if (s.ch == '\n')
                {
                    s.errorf("newline in string");
                    ok = false;
                    break;
                }

                if (s.ch < 0L)
                {
                    s.errorAtf(0L, "string not terminated");
                    ok = false;
                    break;
                }

                s.nextch();

            }


            s.setLit(StringLit, ok);

        }

        private static void rawString(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            var ok = true;
            s.nextch();

            while (true)
            {
                if (s.ch == '`')
                {
                    s.nextch();
                    break;
                }

                if (s.ch < 0L)
                {
                    s.errorAtf(0L, "string not terminated");
                    ok = false;
                    break;
                }

                s.nextch();

            } 
            // We leave CRs in the string since they are part of the
            // literal (even though they are not part of the literal
            // value).
 
            // We leave CRs in the string since they are part of the
            // literal (even though they are not part of the literal
            // value).

            s.setLit(StringLit, ok);

        }

        private static void comment(this ptr<scanner> _addr_s, @string text)
        {
            ref scanner s = ref _addr_s.val;

            s.errorAtf(0L, "%s", text);
        }

        private static void skipLine(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;
 
            // don't consume '\n' - needed for nlsemi logic
            while (s.ch >= 0L && s.ch != '\n')
            {
                s.nextch();
            }


        }

        private static void lineComment(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;
 
            // opening has already been consumed

            if (s.mode & comments != 0L)
            {
                s.skipLine();
                s.comment(string(s.segment()));
                return ;
            } 

            // are we saving directives? or is this definitely not a directive?
            if (s.mode & directives == 0L || (s.ch != 'g' && s.ch != 'l'))
            {
                s.stop();
                s.skipLine();
                return ;
            } 

            // recognize go: or line directives
            @string prefix = "go:";
            if (s.ch == 'l')
            {
                prefix = "line ";
            }

            foreach (var (_, m) in prefix)
            {
                if (s.ch != m)
                {
                    s.stop();
                    s.skipLine();
                    return ;
                }

                s.nextch();

            } 

            // directive text
            s.skipLine();
            s.comment(string(s.segment()));

        }

        private static bool skipComment(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            while (s.ch >= 0L)
            {
                while (s.ch == '*')
                {
                    s.nextch();
                    if (s.ch == '/')
                    {
                        s.nextch();
                        return true;
                    }

                }

                s.nextch();

            }

            s.errorAtf(0L, "comment not terminated");
            return false;

        }

        private static void fullComment(this ptr<scanner> _addr_s)
        {
            ref scanner s = ref _addr_s.val;

            /* opening has already been consumed */

            if (s.mode & comments != 0L)
            {
                if (s.skipComment())
                {
                    s.comment(string(s.segment()));
                }

                return ;

            }

            if (s.mode & directives == 0L || s.ch != 'l')
            {
                s.stop();
                s.skipComment();
                return ;
            } 

            // recognize line directive
            const @string prefix = (@string)"line ";

            foreach (var (_, m) in prefix)
            {
                if (s.ch != m)
                {
                    s.stop();
                    s.skipComment();
                    return ;
                }

                s.nextch();

            } 

            // directive text
            if (s.skipComment())
            {
                s.comment(string(s.segment()));
            }

        }

        private static bool escape(this ptr<scanner> _addr_s, int quote)
        {
            ref scanner s = ref _addr_s.val;

            long n = default;
            uint @base = default;            uint max = default;




            if (s.ch == quote || s.ch == 'a' || s.ch == 'b' || s.ch == 'f' || s.ch == 'n' || s.ch == 'r' || s.ch == 't' || s.ch == 'v' || s.ch == '\\') 
                s.nextch();
                return true;
            else if (s.ch == '0' || s.ch == '1' || s.ch == '2' || s.ch == '3' || s.ch == '4' || s.ch == '5' || s.ch == '6' || s.ch == '7') 
                n = 3L;
                base = 8L;
                max = 255L;
            else if (s.ch == 'x') 
                s.nextch();
                n = 2L;
                base = 16L;
                max = 255L;
            else if (s.ch == 'u') 
                s.nextch();
                n = 4L;
                base = 16L;
                max = unicode.MaxRune;
            else if (s.ch == 'U') 
                s.nextch();
                n = 8L;
                base = 16L;
                max = unicode.MaxRune;
            else 
                if (s.ch < 0L)
                {
                    return true; // complain in caller about EOF
                }

                s.errorf("unknown escape");
                return false;
                        uint x = default;
            for (var i = n; i > 0L; i--)
            {
                if (s.ch < 0L)
                {
                    return true; // complain in caller about EOF
                }

                var d = base;
                if (isDecimal(s.ch))
                {
                    d = uint32(s.ch) - '0';
                }
                else if ('a' <= lower(s.ch) && lower(s.ch) <= 'f')
                {
                    d = uint32(lower(s.ch)) - 'a' + 10L;
                }

                if (d >= base)
                {
                    s.errorf("invalid character %q in %s escape", s.ch, baseName(int(base)));
                    return false;
                } 
                // d < base
                x = x * base + d;
                s.nextch();

            }


            if (x > max && base == 8L)
            {
                s.errorf("octal escape value %d > 255", x);
                return false;
            }

            if (x > max || 0xD800UL <= x && x < 0xE000UL)
            {
                s.errorf("escape is invalid Unicode code point %#U", x);
                return false;
            }

            return true;

        }
    }
}}}}
