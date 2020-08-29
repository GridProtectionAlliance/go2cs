// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements scanner, a lexical tokenizer for
// Go source. After initialization, consecutive calls of
// next advance the scanner one token at a time.
//
// This file, source.go, and tokens.go are self-contained
// (go tool compile scanner.go source.go tokens.go compiles)
// and thus could be made into its own package.

// package syntax -- go2cs converted at 2020 August 29 09:26:26 UTC
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
        private partial struct scanner
        {
            public ref source source => ref source_val;
            public Action<ulong, ulong, @string> pragh;
            public bool nlsemi; // if set '\n' and EOF translate to ';'

// current token, valid after calling next()
            public ulong line;
            public ulong col;
            public token tok;
            public @string lit; // valid if tok is _Name, _Literal, or _Semi ("semicolon", "newline", or "EOF")
            public LitKind kind; // valid if tok is _Literal
            public Operator op; // valid if tok is _Operator, _AssignOp, or _IncOp
            public long prec; // valid if tok is _Operator, _AssignOp, or _IncOp
        }

        private static void init(this ref scanner s, io.Reader src, Action<ulong, ulong, @string> errh, Action<ulong, ulong, @string> pragh)
        {
            s.source.init(src, errh);
            s.pragh = pragh;
            s.nlsemi = false;
        }

        // next advances the scanner by reading the next token.
        //
        // If a read, source encoding, or lexical error occurs, next
        // calls the error handler installed with init. The handler
        // must exist.
        //
        // If a //line or //go: directive is encountered at the start
        // of a line, next calls the directive handler pragh installed
        // with init, if not nil.
        //
        // The (line, col) position passed to the error and directive
        // handler is always at or after the current source reading
        // position.
        private static void next(this ref scanner s)
        {
            var nlsemi = s.nlsemi;
            s.nlsemi = false;

redo:
            var c = s.getr();
            while (c == ' ' || c == '\t' || c == '\n' && !nlsemi || c == '\r')
            {
                c = s.getr();
            } 

            // token start
 

            // token start
            s.line = s.source.line0;
            s.col = s.source.col0;

            if (isLetter(c) || c >= utf8.RuneSelf && s.isIdentRune(c, true))
            {
                s.ident();
                return;
            }

            if (c == -1L)
            {
                if (nlsemi)
                {
                    s.lit = "EOF";
                    s.tok = _Semi;
                    break;
                }
                s.tok = _EOF;
                goto __switch_break0;
            }
            if (c == '\n')
            {
                s.lit = "newline";
                s.tok = _Semi;
                goto __switch_break0;
            }
            if (c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9')
            {
                s.number(c);
                goto __switch_break0;
            }
            if (c == '"')
            {
                s.stdString();
                goto __switch_break0;
            }
            if (c == '`')
            {
                s.rawString();
                goto __switch_break0;
            }
            if (c == '\'')
            {
                s.rune();
                goto __switch_break0;
            }
            if (c == '(')
            {
                s.tok = _Lparen;
                goto __switch_break0;
            }
            if (c == '[')
            {
                s.tok = _Lbrack;
                goto __switch_break0;
            }
            if (c == '{')
            {
                s.tok = _Lbrace;
                goto __switch_break0;
            }
            if (c == ',')
            {
                s.tok = _Comma;
                goto __switch_break0;
            }
            if (c == ';')
            {
                s.lit = "semicolon";
                s.tok = _Semi;
                goto __switch_break0;
            }
            if (c == ')')
            {
                s.nlsemi = true;
                s.tok = _Rparen;
                goto __switch_break0;
            }
            if (c == ']')
            {
                s.nlsemi = true;
                s.tok = _Rbrack;
                goto __switch_break0;
            }
            if (c == '}')
            {
                s.nlsemi = true;
                s.tok = _Rbrace;
                goto __switch_break0;
            }
            if (c == ':')
            {
                if (s.getr() == '=')
                {
                    s.tok = _Define;
                    break;
                }
                s.ungetr();
                s.tok = _Colon;
                goto __switch_break0;
            }
            if (c == '.')
            {
                c = s.getr();
                if (isDigit(c))
                {
                    s.ungetr2();
                    s.number('.');
                    break;
                }
                if (c == '.')
                {
                    c = s.getr();
                    if (c == '.')
                    {
                        s.tok = _DotDotDot;
                        break;
                    }
                    s.ungetr2();
                }
                s.ungetr();
                s.tok = _Dot;
                goto __switch_break0;
            }
            if (c == '+')
            {
                s.op = Add;
                s.prec = precAdd;
                c = s.getr();
                if (c != '+')
                {
                    goto assignop;
                }
                s.nlsemi = true;
                s.tok = _IncOp;
                goto __switch_break0;
            }
            if (c == '-')
            {
                s.op = Sub;
                s.prec = precAdd;
                c = s.getr();
                if (c != '-')
                {
                    goto assignop;
                }
                s.nlsemi = true;
                s.tok = _IncOp;
                goto __switch_break0;
            }
            if (c == '*')
            {
                s.op = Mul;
                s.prec = precMul; 
                // don't goto assignop - want _Star token
                if (s.getr() == '=')
                {
                    s.tok = _AssignOp;
                    break;
                }
                s.ungetr();
                s.tok = _Star;
                goto __switch_break0;
            }
            if (c == '/')
            {
                c = s.getr();
                if (c == '/')
                {
                    s.lineComment();
                    goto redo;
                }
                if (c == '*')
                {
                    s.fullComment();
                    if (s.source.line > s.line && nlsemi)
                    { 
                        // A multi-line comment acts like a newline;
                        // it translates to a ';' if nlsemi is set.
                        s.lit = "newline";
                        s.tok = _Semi;
                        break;
                    }
                    goto redo;
                }
                s.op = Div;
                s.prec = precMul;
                goto assignop;
                goto __switch_break0;
            }
            if (c == '%')
            {
                s.op = Rem;
                s.prec = precMul;
                c = s.getr();
                goto assignop;
                goto __switch_break0;
            }
            if (c == '&')
            {
                c = s.getr();
                if (c == '&')
                {
                    s.op = AndAnd;
                    s.prec = precAndAnd;
                    s.tok = _Operator;
                    break;
                }
                s.op = And;
                s.prec = precMul;
                if (c == '^')
                {
                    s.op = AndNot;
                    c = s.getr();
                }
                goto assignop;
                goto __switch_break0;
            }
            if (c == '|')
            {
                c = s.getr();
                if (c == '|')
                {
                    s.op = OrOr;
                    s.prec = precOrOr;
                    s.tok = _Operator;
                    break;
                }
                s.op = Or;
                s.prec = precAdd;
                goto assignop;
                goto __switch_break0;
            }
            if (c == '~')
            {
                s.error("bitwise complement operator is ^");
                fallthrough = true;

            }
            if (fallthrough || c == '^')
            {
                s.op = Xor;
                s.prec = precAdd;
                c = s.getr();
                goto assignop;
                goto __switch_break0;
            }
            if (c == '<')
            {
                c = s.getr();
                if (c == '=')
                {
                    s.op = Leq;
                    s.prec = precCmp;
                    s.tok = _Operator;
                    break;
                }
                if (c == '<')
                {
                    s.op = Shl;
                    s.prec = precMul;
                    c = s.getr();
                    goto assignop;
                }
                if (c == '-')
                {
                    s.tok = _Arrow;
                    break;
                }
                s.ungetr();
                s.op = Lss;
                s.prec = precCmp;
                s.tok = _Operator;
                goto __switch_break0;
            }
            if (c == '>')
            {
                c = s.getr();
                if (c == '=')
                {
                    s.op = Geq;
                    s.prec = precCmp;
                    s.tok = _Operator;
                    break;
                }
                if (c == '>')
                {
                    s.op = Shr;
                    s.prec = precMul;
                    c = s.getr();
                    goto assignop;
                }
                s.ungetr();
                s.op = Gtr;
                s.prec = precCmp;
                s.tok = _Operator;
                goto __switch_break0;
            }
            if (c == '=')
            {
                if (s.getr() == '=')
                {
                    s.op = Eql;
                    s.prec = precCmp;
                    s.tok = _Operator;
                    break;
                }
                s.ungetr();
                s.tok = _Assign;
                goto __switch_break0;
            }
            if (c == '!')
            {
                if (s.getr() == '=')
                {
                    s.op = Neq;
                    s.prec = precCmp;
                    s.tok = _Operator;
                    break;
                }
                s.ungetr();
                s.op = Not;
                s.prec = 0L;
                s.tok = _Operator;
                goto __switch_break0;
            }
            // default: 
                s.tok = 0L;
                s.error(fmt.Sprintf("invalid character %#U", c));
                goto redo;

            __switch_break0:;

            return;

assignop:
            if (c == '=')
            {
                s.tok = _AssignOp;
                return;
            }
            s.ungetr();
            s.tok = _Operator;
        }

        private static bool isLetter(int c)
        {
            return 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || c == '_';
        }

        private static bool isDigit(int c)
        {
            return '0' <= c && c <= '9';
        }

        private static void ident(this ref scanner s)
        {
            s.startLit(); 

            // accelerate common case (7bit ASCII)
            var c = s.getr();
            while (isLetter(c) || isDigit(c))
            {
                c = s.getr();
            } 

            // general case
 

            // general case
            if (c >= utf8.RuneSelf)
            {
                while (s.isIdentRune(c, false))
                {
                    c = s.getr();
                }

            }
            s.ungetr();

            var lit = s.stopLit(); 

            // possibly a keyword
            if (len(lit) >= 2L)
            {
                {
                    var tok = keywordMap[hash(lit)];

                    if (tok != 0L && tokstrings[tok] == string(lit))
                    {
                        s.nlsemi = contains(1L << (int)(_Break) | 1L << (int)(_Continue) | 1L << (int)(_Fallthrough) | 1L << (int)(_Return), tok);
                        s.tok = tok;
                        return;
                    }

                }
            }
            s.nlsemi = true;
            s.lit = string(lit);
            s.tok = _Name;
        }

        private static bool isIdentRune(this ref scanner s, int c, bool first)
        {

            if (unicode.IsLetter(c) || c == '_')             else if (unicode.IsDigit(c)) 
                if (first)
                {
                    s.error(fmt.Sprintf("identifier cannot begin with digit %#U", c));
                }
            else if (c >= utf8.RuneSelf) 
                s.error(fmt.Sprintf("invalid identifier character %#U", c));
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
                var h = hash((slice<byte>)tokstrings[tok]);
                if (keywordMap[h] != 0L)
                {
                    panic("imperfect hash");
                }
                keywordMap[h] = tok;
            }

        });

        private static void number(this ref scanner s, int c)
        {
            s.startLit();

            if (c != '.')
            {
                s.kind = IntLit; // until proven otherwise
                if (c == '0')
                {
                    c = s.getr();
                    if (c == 'x' || c == 'X')
                    { 
                        // hex
                        c = s.getr();
                        var hasDigit = false;
                        while (isDigit(c) || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F')
                        {
                            c = s.getr();
                            hasDigit = true;
                        }

                        if (!hasDigit)
                        {
                            s.error("malformed hex constant");
                        }
                        goto done;
                    }
                else
 

                    // decimal 0, octal, or float
                    var has8or9 = false;
                    while (isDigit(c))
                    {
                        if (c > '7')
                        {
                            has8or9 = true;
                        }
                        c = s.getr();
                    }

                    if (c != '.' && c != 'e' && c != 'E' && c != 'i')
                    { 
                        // octal
                        if (has8or9)
                        {
                            s.error("malformed octal constant");
                        }
                        goto done;
                    }
                }                { 
                    // decimal or float
                    while (isDigit(c))
                    {
                        c = s.getr();
                    }

                }
            } 

            // float
            if (c == '.')
            {
                s.kind = FloatLit;
                c = s.getr();
                while (isDigit(c))
                {
                    c = s.getr();
                }

            } 

            // exponent
            if (c == 'e' || c == 'E')
            {
                s.kind = FloatLit;
                c = s.getr();
                if (c == '-' || c == '+')
                {
                    c = s.getr();
                }
                if (!isDigit(c))
                {
                    s.error("malformed floating-point constant exponent");
                }
                while (isDigit(c))
                {
                    c = s.getr();
                }

            } 

            // complex
            if (c == 'i')
            {
                s.kind = ImagLit;
                s.getr();
            }
done:
            s.ungetr();
            s.nlsemi = true;
            s.lit = string(s.stopLit());
            s.tok = _Literal;
        }

        private static void rune(this ref scanner s)
        {
            s.startLit();

            var ok = true; // only report errors if we're ok so far
            long n = 0L;
            while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
            {
                var r = s.getr();
                if (r == '\'')
                {
                    break;
                n++;
                }
                if (r == '\\')
                {
                    if (!s.escape('\''))
                    {
                        ok = false;
                    }
                    continue;
                }
                if (r == '\n')
                {
                    s.ungetr(); // assume newline is not part of literal
                    if (ok)
                    {
                        s.error("newline in character literal");
                        ok = false;
                    }
                    break;
                }
                if (r < 0L)
                {
                    if (ok)
                    {
                        s.errh(s.line, s.col, "invalid character literal (missing closing ')");
                        ok = false;
                    }
                    break;
                }
            }


            if (ok)
            {
                if (n == 0L)
                {
                    s.error("empty character literal or unescaped ' in character literal");
                }
                else if (n != 1L)
                {
                    s.errh(s.line, s.col, "invalid character literal (more than one character)");
                }
            }
            s.nlsemi = true;
            s.lit = string(s.stopLit());
            s.kind = RuneLit;
            s.tok = _Literal;
        }

        private static void stdString(this ref scanner s)
        {
            s.startLit();

            while (true)
            {
                var r = s.getr();
                if (r == '"')
                {
                    break;
                }
                if (r == '\\')
                {
                    s.escape('"');
                    continue;
                }
                if (r == '\n')
                {
                    s.ungetr(); // assume newline is not part of literal
                    s.error("newline in string");
                    break;
                }
                if (r < 0L)
                {
                    s.errh(s.line, s.col, "string not terminated");
                    break;
                }
            }


            s.nlsemi = true;
            s.lit = string(s.stopLit());
            s.kind = StringLit;
            s.tok = _Literal;
        }

        private static void rawString(this ref scanner s)
        {
            s.startLit();

            while (true)
            {
                var r = s.getr();
                if (r == '`')
                {
                    break;
                }
                if (r < 0L)
                {
                    s.errh(s.line, s.col, "string not terminated");
                    break;
                }
            } 
            // We leave CRs in the string since they are part of the
            // literal (even though they are not part of the literal
            // value).
 
            // We leave CRs in the string since they are part of the
            // literal (even though they are not part of the literal
            // value).

            s.nlsemi = true;
            s.lit = string(s.stopLit());
            s.kind = StringLit;
            s.tok = _Literal;
        }

        private static void skipLine(this ref scanner s, int r)
        {
            while (r >= 0L)
            {
                if (r == '\n')
                {
                    s.ungetr(); // don't consume '\n' - needed for nlsemi logic
                    break;
                }
                r = s.getr();
            }

        }

        private static void lineComment(this ref scanner s)
        {
            var r = s.getr(); 
            // directives must start at the beginning of the line (s.col == colbase)
            if (s.col != colbase || s.pragh == null || (r != 'g' && r != 'l'))
            {
                s.skipLine(r);
                return;
            } 
            // s.col == colbase && s.pragh != nil && (r == 'g' || r == 'l')

            // recognize directives
            @string prefix = "go:";
            if (r == 'l')
            {
                prefix = "line ";
            }
            foreach (var (_, m) in prefix)
            {
                if (r != m)
                {
                    s.skipLine(r);
                    return;
                }
                r = s.getr();
            } 

            // directive text without line ending (which may be "\r\n" if Windows),
            s.startLit();
            s.skipLine(r);
            var text = s.stopLit();
            {
                var i = len(text) - 1L;

                if (i >= 0L && text[i] == '\r')
                {
                    text = text[..i];
                }

            }

            s.pragh(s.line, s.col + 2L, prefix + string(text)); // +2 since directive text starts after //
        }

        private static void fullComment(this ref scanner s)
        {
            while (true)
            {
                var r = s.getr();
                while (r == '*')
                {
                    r = s.getr();
                    if (r == '/')
                    {
                        return;
                    }
                }

                if (r < 0L)
                {
                    s.errh(s.line, s.col, "comment not terminated");
                    return;
                }
            }

        }

        private static bool escape(this ref scanner s, int quote)
        {
            long n = default;
            uint @base = default;            uint max = default;



            var c = s.getr();

            if (c == 'a' || c == 'b' || c == 'f' || c == 'n' || c == 'r' || c == 't' || c == 'v' || c == '\\' || c == quote) 
                return true;
            else if (c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7') 
                n = 3L;
                base = 8L;
                max = 255L;
            else if (c == 'x') 
                c = s.getr();
                n = 2L;
                base = 16L;
                max = 255L;
            else if (c == 'u') 
                c = s.getr();
                n = 4L;
                base = 16L;
                max = unicode.MaxRune;
            else if (c == 'U') 
                c = s.getr();
                n = 8L;
                base = 16L;
                max = unicode.MaxRune;
            else 
                if (c < 0L)
                {
                    return true; // complain in caller about EOF
                }
                s.error("unknown escape sequence");
                return false;
                        uint x = default;
            for (var i = n; i > 0L; i--)
            {
                var d = base;

                if (isDigit(c)) 
                    d = uint32(c) - '0';
                else if ('a' <= c && c <= 'f') 
                    d = uint32(c) - ('a' - 10L);
                else if ('A' <= c && c <= 'F') 
                    d = uint32(c) - ('A' - 10L);
                                if (d >= base)
                {
                    if (c < 0L)
                    {
                        return true; // complain in caller about EOF
                    }
                    @string kind = "hex";
                    if (base == 8L)
                    {
                        kind = "octal";
                    }
                    s.error(fmt.Sprintf("non-%s character in escape sequence: %c", kind, c));
                    s.ungetr();
                    return false;
                } 
                // d < base
                x = x * base + d;
                c = s.getr();
            }

            s.ungetr();

            if (x > max && base == 8L)
            {
                s.error(fmt.Sprintf("octal escape value > 255: %d", x));
                return false;
            }
            if (x > max || 0xD800UL <= x && x < 0xE000UL)
            {
                s.error("escape sequence is invalid Unicode code point");
                return false;
            }
            return true;
        }
    }
}}}}
