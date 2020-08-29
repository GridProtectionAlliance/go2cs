// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package scanner implements a scanner for Go source text.
// It takes a []byte as source which can then be tokenized
// through repeated calls to the Scan method.
//
// package scanner -- go2cs converted at 2020 August 29 08:48:26 UTC
// import "go/scanner" ==> using scanner = go.go.scanner_package
// Original source: C:\Go\src\go\scanner\scanner.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class scanner_package
    {
        // An ErrorHandler may be provided to Scanner.Init. If a syntax error is
        // encountered and a handler was installed, the handler is called with a
        // position and an error message. The position points to the beginning of
        // the offending token.
        //
        public delegate void ErrorHandler(token.Position, @string);

        // A Scanner holds the scanner's internal state while processing
        // a given text. It can be allocated as part of another data
        // structure but must be initialized via Init before use.
        //
        public partial struct Scanner
        {
            public ptr<token.File> file; // source file handle
            public @string dir; // directory portion of file.Name()
            public slice<byte> src; // source
            public ErrorHandler err; // error reporting; or nil
            public Mode mode; // scanning mode

// scanning state
            public int ch; // current character
            public long offset; // character offset
            public long rdOffset; // reading offset (position after current character)
            public long lineOffset; // current line offset
            public bool insertSemi; // insert a semicolon before next newline

// public state - ok to modify
            public long ErrorCount; // number of errors encountered
        }

        private static readonly ulong bom = 0xFEFFUL; // byte order mark, only permitted as very first character

        // Read the next Unicode char into s.ch.
        // s.ch < 0 means end-of-file.
        //
 // byte order mark, only permitted as very first character

        // Read the next Unicode char into s.ch.
        // s.ch < 0 means end-of-file.
        //
        private static void next(this ref Scanner s)
        {
            if (s.rdOffset < len(s.src))
            {
                s.offset = s.rdOffset;
                if (s.ch == '\n')
                {
                    s.lineOffset = s.offset;
                    s.file.AddLine(s.offset);
                }
                var r = rune(s.src[s.rdOffset]);
                long w = 1L;

                if (r == 0L) 
                    s.error(s.offset, "illegal character NUL");
                else if (r >= utf8.RuneSelf) 
                    // not ASCII
                    r, w = utf8.DecodeRune(s.src[s.rdOffset..]);
                    if (r == utf8.RuneError && w == 1L)
                    {
                        s.error(s.offset, "illegal UTF-8 encoding");
                    }
                    else if (r == bom && s.offset > 0L)
                    {
                        s.error(s.offset, "illegal byte order mark");
                    }
                                s.rdOffset += w;
                s.ch = r;
            }
            else
            {
                s.offset = len(s.src);
                if (s.ch == '\n')
                {
                    s.lineOffset = s.offset;
                    s.file.AddLine(s.offset);
                }
                s.ch = -1L; // eof
            }
        }

        // A mode value is a set of flags (or 0).
        // They control scanner behavior.
        //
        public partial struct Mode // : ulong
        {
        }

        public static readonly Mode ScanComments = 1L << (int)(iota); // return comments as COMMENT tokens
        private static readonly var dontInsertSemis = 0; // do not automatically insert semicolons - for testing only

        // Init prepares the scanner s to tokenize the text src by setting the
        // scanner at the beginning of src. The scanner uses the file set file
        // for position information and it adds line information for each line.
        // It is ok to re-use the same file when re-scanning the same file as
        // line information which is already present is ignored. Init causes a
        // panic if the file size does not match the src size.
        //
        // Calls to Scan will invoke the error handler err if they encounter a
        // syntax error and err is not nil. Also, for each error encountered,
        // the Scanner field ErrorCount is incremented by one. The mode parameter
        // determines how comments are handled.
        //
        // Note that Init may call err if there is an error in the first character
        // of the file.
        //
        private static void Init(this ref Scanner _s, ref token.File _file, slice<byte> src, ErrorHandler err, Mode mode) => func(_s, _file, (ref Scanner s, ref token.File file, Defer _, Panic panic, Recover __) =>
        { 
            // Explicitly initialize all fields since a scanner may be reused.
            if (file.Size() != len(src))
            {
                panic(fmt.Sprintf("file size (%d) does not match src len (%d)", file.Size(), len(src)));
            }
            s.file = file;
            s.dir, _ = filepath.Split(file.Name());
            s.src = src;
            s.err = err;
            s.mode = mode;

            s.ch = ' ';
            s.offset = 0L;
            s.rdOffset = 0L;
            s.lineOffset = 0L;
            s.insertSemi = false;
            s.ErrorCount = 0L;

            s.next();
            if (s.ch == bom)
            {
                s.next(); // ignore BOM at file beginning
            }
        });

        private static void error(this ref Scanner s, long offs, @string msg)
        {
            if (s.err != null)
            {
                s.err(s.file.Position(s.file.Pos(offs)), msg);
            }
            s.ErrorCount++;
        }

        private static slice<byte> prefix = (slice<byte>)"//line ";

        private static void interpretLineComment(this ref Scanner s, slice<byte> text)
        {
            if (bytes.HasPrefix(text, prefix))
            { 
                // get filename and line number, if any
                {
                    var i = bytes.LastIndex(text, new slice<byte>(new byte[] { ':' }));

                    if (i > 0L)
                    {
                        {
                            var (line, err) = strconv.Atoi(string(text[i + 1L..]));

                            if (err == null && line > 0L)
                            { 
                                // valid //line filename:line comment
                                var filename = string(bytes.TrimSpace(text[len(prefix)..i]));
                                if (filename != "")
                                {
                                    filename = filepath.Clean(filename);
                                    if (!filepath.IsAbs(filename))
                                    { 
                                        // make filename relative to current directory
                                        filename = filepath.Join(s.dir, filename);
                                    }
                                } 
                                // update scanner position
                                s.file.AddLineInfo(s.lineOffset + len(text) + 1L, filename, line); // +len(text)+1 since comment applies to next line
                            }

                        }
                    }

                }
            }
        }

        private static @string scanComment(this ref Scanner s)
        { 
            // initial '/' already consumed; s.ch == '/' || s.ch == '*'
            var offs = s.offset - 1L; // position of initial '/'
            var hasCR = false;

            if (s.ch == '/')
            { 
                //-style comment
                s.next();
                while (s.ch != '\n' && s.ch >= 0L)
                {
                    if (s.ch == '\r')
                    {
                        hasCR = true;
                    }
                    s.next();
                }

                if (offs == s.lineOffset)
                { 
                    // comment starts at the beginning of the current line
                    s.interpretLineComment(s.src[offs..s.offset]);
                }
                goto exit;
            } 

            /*-style comment */
            s.next();
            while (s.ch >= 0L)
            {
                var ch = s.ch;
                if (ch == '\r')
                {
                    hasCR = true;
                }
                s.next();
                if (ch == '*' && s.ch == '/')
                {
                    s.next();
                    goto exit;
                }
            }


            s.error(offs, "comment not terminated");

exit:
            var lit = s.src[offs..s.offset];
            if (hasCR)
            {
                lit = stripCR(lit);
            }
            return string(lit);
        }

        private static bool findLineEnd(this ref Scanner _s) => func(_s, (ref Scanner s, Defer defer, Panic _, Recover __) =>
        { 
            // initial '/' already consumed

            defer(offs =>
            { 
                // reset scanner state to where it was upon calling findLineEnd
                s.ch = '/';
                s.offset = offs;
                s.rdOffset = offs + 1L;
                s.next(); // consume initial '/' again
            }(s.offset - 1L)); 

            // read ahead until a newline, EOF, or non-comment token is found
            while (s.ch == '/' || s.ch == '*')
            {
                if (s.ch == '/')
                { 
                    //-style comment always contains a newline
                    return true;
                } 
                /*-style comment: look for newline */
                s.next();
                while (s.ch >= 0L)
                {
                    var ch = s.ch;
                    if (ch == '\n')
                    {
                        return true;
                    }
                    s.next();
                    if (ch == '*' && s.ch == '/')
                    {
                        s.next();
                        break;
                    }
                }

                s.skipWhitespace(); // s.insertSemi is set
                if (s.ch < 0L || s.ch == '\n')
                {
                    return true;
                }
                if (s.ch != '/')
                { 
                    // non-comment token
                    return false;
                }
                s.next(); // consume '/'
            }


            return false;
        });

        private static bool isLetter(int ch)
        {
            return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_' || ch >= utf8.RuneSelf && unicode.IsLetter(ch);
        }

        private static bool isDigit(int ch)
        {
            return '0' <= ch && ch <= '9' || ch >= utf8.RuneSelf && unicode.IsDigit(ch);
        }

        private static @string scanIdentifier(this ref Scanner s)
        {
            var offs = s.offset;
            while (isLetter(s.ch) || isDigit(s.ch))
            {
                s.next();
            }

            return string(s.src[offs..s.offset]);
        }

        private static long digitVal(int ch)
        {

            if ('0' <= ch && ch <= '9') 
                return int(ch - '0');
            else if ('a' <= ch && ch <= 'f') 
                return int(ch - 'a' + 10L);
            else if ('A' <= ch && ch <= 'F') 
                return int(ch - 'A' + 10L);
                        return 16L; // larger than any legal digit val
        }

        private static void scanMantissa(this ref Scanner s, long @base)
        {
            while (digitVal(s.ch) < base)
            {
                s.next();
            }

        }

        private static (token.Token, @string) scanNumber(this ref Scanner s, bool seenDecimalPoint)
        { 
            // digitVal(s.ch) < 10
            var offs = s.offset;
            var tok = token.INT;

            if (seenDecimalPoint)
            {
                offs--;
                tok = token.FLOAT;
                s.scanMantissa(10L);
                goto exponent;
            }
            if (s.ch == '0')
            { 
                // int or float
                offs = s.offset;
                s.next();
                if (s.ch == 'x' || s.ch == 'X')
                { 
                    // hexadecimal int
                    s.next();
                    s.scanMantissa(16L);
                    if (s.offset - offs <= 2L)
                    { 
                        // only scanned "0x" or "0X"
                        s.error(offs, "illegal hexadecimal number");
                    }
                }
                else
                { 
                    // octal int or float
                    var seenDecimalDigit = false;
                    s.scanMantissa(8L);
                    if (s.ch == '8' || s.ch == '9')
                    { 
                        // illegal octal int or float
                        seenDecimalDigit = true;
                        s.scanMantissa(10L);
                    }
                    if (s.ch == '.' || s.ch == 'e' || s.ch == 'E' || s.ch == 'i')
                    {
                        goto fraction;
                    } 
                    // octal int
                    if (seenDecimalDigit)
                    {
                        s.error(offs, "illegal octal number");
                    }
                }
                goto exit;
            } 

            // decimal int or float
            s.scanMantissa(10L);

fraction:

            if (s.ch == '.')
            {
                tok = token.FLOAT;
                s.next();
                s.scanMantissa(10L);
            }
exponent:

            if (s.ch == 'e' || s.ch == 'E')
            {
                tok = token.FLOAT;
                s.next();
                if (s.ch == '-' || s.ch == '+')
                {
                    s.next();
                }
                if (digitVal(s.ch) < 10L)
                {
                    s.scanMantissa(10L);
                }
                else
                {
                    s.error(offs, "illegal floating-point exponent");
                }
            }
            if (s.ch == 'i')
            {
                tok = token.IMAG;
                s.next();
            }
exit:
            return (tok, string(s.src[offs..s.offset]));
        }

        // scanEscape parses an escape sequence where rune is the accepted
        // escaped quote. In case of a syntax error, it stops at the offending
        // character (without consuming it) and returns false. Otherwise
        // it returns true.
        private static bool scanEscape(this ref Scanner s, int quote)
        {
            var offs = s.offset;

            long n = default;
            uint @base = default;            uint max = default;


            if (s.ch == 'a' || s.ch == 'b' || s.ch == 'f' || s.ch == 'n' || s.ch == 'r' || s.ch == 't' || s.ch == 'v' || s.ch == '\\' || s.ch == quote) 
                s.next();
                return true;
            else if (s.ch == '0' || s.ch == '1' || s.ch == '2' || s.ch == '3' || s.ch == '4' || s.ch == '5' || s.ch == '6' || s.ch == '7') 
                n = 3L;
                base = 8L;
                max = 255L;
            else if (s.ch == 'x') 
                s.next();
                n = 2L;
                base = 16L;
                max = 255L;
            else if (s.ch == 'u') 
                s.next();
                n = 4L;
                base = 16L;
                max = unicode.MaxRune;
            else if (s.ch == 'U') 
                s.next();
                n = 8L;
                base = 16L;
                max = unicode.MaxRune;
            else 
                @string msg = "unknown escape sequence";
                if (s.ch < 0L)
                {
                    msg = "escape sequence not terminated";
                }
                s.error(offs, msg);
                return false;
                        uint x = default;
            while (n > 0L)
            {
                var d = uint32(digitVal(s.ch));
                if (d >= base)
                {
                    msg = fmt.Sprintf("illegal character %#U in escape sequence", s.ch);
                    if (s.ch < 0L)
                    {
                        msg = "escape sequence not terminated";
                    }
                    s.error(s.offset, msg);
                    return false;
                }
                x = x * base + d;
                s.next();
                n--;
            }


            if (x > max || 0xD800UL <= x && x < 0xE000UL)
            {
                s.error(offs, "escape sequence is invalid Unicode code point");
                return false;
            }
            return true;
        }

        private static @string scanRune(this ref Scanner s)
        { 
            // '\'' opening already consumed
            var offs = s.offset - 1L;

            var valid = true;
            long n = 0L;
            while (true)
            {
                var ch = s.ch;
                if (ch == '\n' || ch < 0L)
                { 
                    // only report error if we don't have one already
                    if (valid)
                    {
                        s.error(offs, "rune literal not terminated");
                        valid = false;
                    }
                    break;
                }
                s.next();
                if (ch == '\'')
                {
                    break;
                }
                n++;
                if (ch == '\\')
                {
                    if (!s.scanEscape('\''))
                    {
                        valid = false;
                    } 
                    // continue to read to closing quote
                }
            }


            if (valid && n != 1L)
            {
                s.error(offs, "illegal rune literal");
            }
            return string(s.src[offs..s.offset]);
        }

        private static @string scanString(this ref Scanner s)
        { 
            // '"' opening already consumed
            var offs = s.offset - 1L;

            while (true)
            {
                var ch = s.ch;
                if (ch == '\n' || ch < 0L)
                {
                    s.error(offs, "string literal not terminated");
                    break;
                }
                s.next();
                if (ch == '"')
                {
                    break;
                }
                if (ch == '\\')
                {
                    s.scanEscape('"');
                }
            }


            return string(s.src[offs..s.offset]);
        }

        private static slice<byte> stripCR(slice<byte> b)
        {
            var c = make_slice<byte>(len(b));
            long i = 0L;
            foreach (var (_, ch) in b)
            {
                if (ch != '\r')
                {
                    c[i] = ch;
                    i++;
                }
            }
            return c[..i];
        }

        private static @string scanRawString(this ref Scanner s)
        { 
            // '`' opening already consumed
            var offs = s.offset - 1L;

            var hasCR = false;
            while (true)
            {
                var ch = s.ch;
                if (ch < 0L)
                {
                    s.error(offs, "raw string literal not terminated");
                    break;
                }
                s.next();
                if (ch == '`')
                {
                    break;
                }
                if (ch == '\r')
                {
                    hasCR = true;
                }
            }


            var lit = s.src[offs..s.offset];
            if (hasCR)
            {
                lit = stripCR(lit);
            }
            return string(lit);
        }

        private static void skipWhitespace(this ref Scanner s)
        {
            while (s.ch == ' ' || s.ch == '\t' || s.ch == '\n' && !s.insertSemi || s.ch == '\r')
            {
                s.next();
            }

        }

        // Helper functions for scanning multi-byte tokens such as >> += >>= .
        // Different routines recognize different length tok_i based on matches
        // of ch_i. If a token ends in '=', the result is tok1 or tok3
        // respectively. Otherwise, the result is tok0 if there was no other
        // matching character, or tok2 if the matching character was ch2.

        private static token.Token switch2(this ref Scanner s, token.Token tok0, token.Token tok1)
        {
            if (s.ch == '=')
            {
                s.next();
                return tok1;
            }
            return tok0;
        }

        private static token.Token switch3(this ref Scanner s, token.Token tok0, token.Token tok1, int ch2, token.Token tok2)
        {
            if (s.ch == '=')
            {
                s.next();
                return tok1;
            }
            if (s.ch == ch2)
            {
                s.next();
                return tok2;
            }
            return tok0;
        }

        private static token.Token switch4(this ref Scanner s, token.Token tok0, token.Token tok1, int ch2, token.Token tok2, token.Token tok3)
        {
            if (s.ch == '=')
            {
                s.next();
                return tok1;
            }
            if (s.ch == ch2)
            {
                s.next();
                if (s.ch == '=')
                {
                    s.next();
                    return tok3;
                }
                return tok2;
            }
            return tok0;
        }

        // Scan scans the next token and returns the token position, the token,
        // and its literal string if applicable. The source end is indicated by
        // token.EOF.
        //
        // If the returned token is a literal (token.IDENT, token.INT, token.FLOAT,
        // token.IMAG, token.CHAR, token.STRING) or token.COMMENT, the literal string
        // has the corresponding value.
        //
        // If the returned token is a keyword, the literal string is the keyword.
        //
        // If the returned token is token.SEMICOLON, the corresponding
        // literal string is ";" if the semicolon was present in the source,
        // and "\n" if the semicolon was inserted because of a newline or
        // at EOF.
        //
        // If the returned token is token.ILLEGAL, the literal string is the
        // offending character.
        //
        // In all other cases, Scan returns an empty literal string.
        //
        // For more tolerant parsing, Scan will return a valid token if
        // possible even if a syntax error was encountered. Thus, even
        // if the resulting token sequence contains no illegal tokens,
        // a client may not assume that no error occurred. Instead it
        // must check the scanner's ErrorCount or the number of calls
        // of the error handler, if there was one installed.
        //
        // Scan adds line information to the file added to the file
        // set with Init. Token positions are relative to that file
        // and thus relative to the file set.
        //
        private static (token.Pos, token.Token, @string) Scan(this ref Scanner s)
        {
scanAgain: 

            // current token start
            s.skipWhitespace(); 

            // current token start
            pos = s.file.Pos(s.offset); 

            // determine token value
            var insertSemi = false;
            {
                var ch = s.ch;


                if (isLetter(ch)) 
                    lit = s.scanIdentifier();
                    if (len(lit) > 1L)
                    { 
                        // keywords are longer than one letter - avoid lookup otherwise
                        tok = token.Lookup(lit);

                        if (tok == token.IDENT || tok == token.BREAK || tok == token.CONTINUE || tok == token.FALLTHROUGH || tok == token.RETURN) 
                            insertSemi = true;
                                            }
                    else
                    {
                        insertSemi = true;
                        tok = token.IDENT;
                    }
                else if ('0' <= ch && ch <= '9') 
                    insertSemi = true;
                    tok, lit = s.scanNumber(false);
                else 
                    s.next(); // always make progress
                    switch (ch)
                    {
                        case -1L: 
                            if (s.insertSemi)
                            {
                                s.insertSemi = false; // EOF consumed
                                return (pos, token.SEMICOLON, "\n");
                            }
                            tok = token.EOF;
                            break;
                        case '\n': 
                            // we only reach here if s.insertSemi was
                            // set in the first place and exited early
                            // from s.skipWhitespace()
                            s.insertSemi = false; // newline consumed
                            return (pos, token.SEMICOLON, "\n");
                            break;
                        case '"': 
                            insertSemi = true;
                            tok = token.STRING;
                            lit = s.scanString();
                            break;
                        case '\'': 
                            insertSemi = true;
                            tok = token.CHAR;
                            lit = s.scanRune();
                            break;
                        case '`': 
                            insertSemi = true;
                            tok = token.STRING;
                            lit = s.scanRawString();
                            break;
                        case ':': 
                            tok = s.switch2(token.COLON, token.DEFINE);
                            break;
                        case '.': 
                            if ('0' <= s.ch && s.ch <= '9')
                            {
                                insertSemi = true;
                                tok, lit = s.scanNumber(true);
                            }
                            else if (s.ch == '.')
                            {
                                s.next();
                                if (s.ch == '.')
                                {
                                    s.next();
                                    tok = token.ELLIPSIS;
                                }
                            }
                            else
                            {
                                tok = token.PERIOD;
                            }
                            break;
                        case ',': 
                            tok = token.COMMA;
                            break;
                        case ';': 
                            tok = token.SEMICOLON;
                            lit = ";";
                            break;
                        case '(': 
                            tok = token.LPAREN;
                            break;
                        case ')': 
                            insertSemi = true;
                            tok = token.RPAREN;
                            break;
                        case '[': 
                            tok = token.LBRACK;
                            break;
                        case ']': 
                            insertSemi = true;
                            tok = token.RBRACK;
                            break;
                        case '{': 
                            tok = token.LBRACE;
                            break;
                        case '}': 
                            insertSemi = true;
                            tok = token.RBRACE;
                            break;
                        case '+': 
                            tok = s.switch3(token.ADD, token.ADD_ASSIGN, '+', token.INC);
                            if (tok == token.INC)
                            {
                                insertSemi = true;
                            }
                            break;
                        case '-': 
                            tok = s.switch3(token.SUB, token.SUB_ASSIGN, '-', token.DEC);
                            if (tok == token.DEC)
                            {
                                insertSemi = true;
                            }
                            break;
                        case '*': 
                            tok = s.switch2(token.MUL, token.MUL_ASSIGN);
                            break;
                        case '/': 
                            if (s.ch == '/' || s.ch == '*')
                            { 
                                // comment
                                if (s.insertSemi && s.findLineEnd())
                                { 
                                    // reset position to the beginning of the comment
                                    s.ch = '/';
                                    s.offset = s.file.Offset(pos);
                                    s.rdOffset = s.offset + 1L;
                                    s.insertSemi = false; // newline consumed
                                    return (pos, token.SEMICOLON, "\n");
                                }
                                var comment = s.scanComment();
                                if (s.mode & ScanComments == 0L)
                                { 
                                    // skip comment
                                    s.insertSemi = false; // newline consumed
                                    goto scanAgain;
                                }
                                tok = token.COMMENT;
                                lit = comment;
                            }
                            else
                            {
                                tok = s.switch2(token.QUO, token.QUO_ASSIGN);
                            }
                            break;
                        case '%': 
                            tok = s.switch2(token.REM, token.REM_ASSIGN);
                            break;
                        case '^': 
                            tok = s.switch2(token.XOR, token.XOR_ASSIGN);
                            break;
                        case '<': 
                            if (s.ch == '-')
                            {
                                s.next();
                                tok = token.ARROW;
                            }
                            else
                            {
                                tok = s.switch4(token.LSS, token.LEQ, '<', token.SHL, token.SHL_ASSIGN);
                            }
                            break;
                        case '>': 
                            tok = s.switch4(token.GTR, token.GEQ, '>', token.SHR, token.SHR_ASSIGN);
                            break;
                        case '=': 
                            tok = s.switch2(token.ASSIGN, token.EQL);
                            break;
                        case '!': 
                            tok = s.switch2(token.NOT, token.NEQ);
                            break;
                        case '&': 
                            if (s.ch == '^')
                            {
                                s.next();
                                tok = s.switch2(token.AND_NOT, token.AND_NOT_ASSIGN);
                            }
                            else
                            {
                                tok = s.switch3(token.AND, token.AND_ASSIGN, '&', token.LAND);
                            }
                            break;
                        case '|': 
                            tok = s.switch3(token.OR, token.OR_ASSIGN, '|', token.LOR);
                            break;
                        default: 
                            // next reports unexpected BOMs - don't repeat
                            if (ch != bom)
                            {
                                s.error(s.file.Offset(pos), fmt.Sprintf("illegal character %#U", ch));
                            }
                            insertSemi = s.insertSemi; // preserve insertSemi info
                            tok = token.ILLEGAL;
                            lit = string(ch);
                            break;
                    }

            }
            if (s.mode & dontInsertSemis == 0L)
            {
                s.insertSemi = insertSemi;
            }
            return;
        }
    }
}}
