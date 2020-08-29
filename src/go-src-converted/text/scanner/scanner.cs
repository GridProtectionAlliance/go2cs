// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package scanner provides a scanner and tokenizer for UTF-8-encoded text.
// It takes an io.Reader providing the source, which then can be tokenized
// through repeated calls to the Scan function. For compatibility with
// existing tools, the NUL character is not allowed. If the first character
// in the source is a UTF-8 encoded byte order mark (BOM), it is discarded.
//
// By default, a Scanner skips white space and Go comments and recognizes all
// literals as defined by the Go language specification. It may be
// customized to recognize only a subset of those literals and to recognize
// different identifier and white space characters.
// package scanner -- go2cs converted at 2020 August 29 08:48:55 UTC
// import "text/scanner" ==> using scanner = go.text.scanner_package
// Original source: C:\Go\src\text\scanner\scanner.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace text
{
    public static partial class scanner_package
    {
        // A source position is represented by a Position value.
        // A position is valid if Line > 0.
        public partial struct Position
        {
            public @string Filename; // filename, if any
            public long Offset; // byte offset, starting at 0
            public long Line; // line number, starting at 1
            public long Column; // column number, starting at 1 (character count per line)
        }

        // IsValid reports whether the position is valid.
        private static bool IsValid(this ref Position pos)
        {
            return pos.Line > 0L;
        }

        public static @string String(this Position pos)
        {
            var s = pos.Filename;
            if (s == "")
            {
                s = "<input>";
            }
            if (pos.IsValid())
            {
                s += fmt.Sprintf(":%d:%d", pos.Line, pos.Column);
            }
            return s;
        }

        // Predefined mode bits to control recognition of tokens. For instance,
        // to configure a Scanner such that it only recognizes (Go) identifiers,
        // integers, and skips comments, set the Scanner's Mode field to:
        //
        //    ScanIdents | ScanInts | SkipComments
        //
        // With the exceptions of comments, which are skipped if SkipComments is
        // set, unrecognized tokens are not ignored. Instead, the scanner simply
        // returns the respective individual characters (or possibly sub-tokens).
        // For instance, if the mode is ScanIdents (not ScanStrings), the string
        // "foo" is scanned as the token sequence '"' Ident '"'.
        //
        public static readonly long ScanIdents = 1L << (int)(-Ident);
        public static readonly long ScanInts = 1L << (int)(-Int);
        public static readonly long ScanFloats = 1L << (int)(-Float); // includes Ints
        public static readonly long ScanChars = 1L << (int)(-Char);
        public static readonly long ScanStrings = 1L << (int)(-String);
        public static readonly long ScanRawStrings = 1L << (int)(-RawString);
        public static readonly long ScanComments = 1L << (int)(-Comment);
        public static readonly long SkipComments = 1L << (int)(-skipComment); // if set with ScanComments, comments become white space
        public static readonly var GoTokens = ScanIdents | ScanFloats | ScanChars | ScanStrings | ScanRawStrings | ScanComments | SkipComments;

        // The result of Scan is one of these tokens or a Unicode character.
        public static readonly var EOF = -(iota + 1L);
        public static readonly var Ident = 0;
        public static readonly var Int = 1;
        public static readonly var Float = 2;
        public static readonly var Char = 3;
        public static readonly var String = 4;
        public static readonly var RawString = 5;
        public static readonly var Comment = 6;
        private static readonly var skipComment = 7;

        private static map tokenString = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<int, @string>{EOF:"EOF",Ident:"Ident",Int:"Int",Float:"Float",Char:"Char",String:"String",RawString:"RawString",Comment:"Comment",};

        // TokenString returns a printable string for a token or Unicode character.
        public static @string TokenString(int tok)
        {
            {
                var (s, found) = tokenString[tok];

                if (found)
                {
                    return s;
                }

            }
            return fmt.Sprintf("%q", string(tok));
        }

        // GoWhitespace is the default value for the Scanner's Whitespace field.
        // Its value selects Go's white space characters.
        public static readonly long GoWhitespace = 1L << (int)('\t') | 1L << (int)('\n') | 1L << (int)('\r') | 1L << (int)(' ');



        private static readonly long bufLen = 1024L; // at least utf8.UTFMax

        // A Scanner implements reading of Unicode characters and tokens from an io.Reader.
 // at least utf8.UTFMax

        // A Scanner implements reading of Unicode characters and tokens from an io.Reader.
        public partial struct Scanner
        {
            public io.Reader src; // Source buffer
            public array<byte> srcBuf; // +1 for sentinel for common case of s.next()
            public long srcPos; // reading position (srcBuf index)
            public long srcEnd; // source end (srcBuf index)

// Source position
            public long srcBufOffset; // byte offset of srcBuf[0] in source
            public long line; // line count
            public long column; // character count
            public long lastLineLen; // length of last line in characters (for correct column reporting)
            public long lastCharLen; // length of last character in bytes

// Token text buffer
// Typically, token text is stored completely in srcBuf, but in general
// the token text's head may be buffered in tokBuf while the token text's
// tail is stored in srcBuf.
            public bytes.Buffer tokBuf; // token text head that is not in srcBuf anymore
            public long tokPos; // token text tail position (srcBuf index); valid if >= 0
            public long tokEnd; // token text tail end (srcBuf index)

// One character look-ahead
            public int ch; // character before current srcPos

// Error is called for each error encountered. If no Error
// function is set, the error is reported to os.Stderr.
            public Action<ref Scanner, @string> Error; // ErrorCount is incremented by one for each error encountered.
            public long ErrorCount; // The Mode field controls which tokens are recognized. For instance,
// to recognize Ints, set the ScanInts bit in Mode. The field may be
// changed at any time.
            public ulong Mode; // The Whitespace field controls which characters are recognized
// as white space. To recognize a character ch <= ' ' as white space,
// set the ch'th bit in Whitespace (the Scanner's behavior is undefined
// for values ch > ' '). The field may be changed at any time.
            public ulong Whitespace; // IsIdentRune is a predicate controlling the characters accepted
// as the ith rune in an identifier. The set of valid characters
// must not intersect with the set of white space characters.
// If no IsIdentRune function is set, regular Go identifiers are
// accepted instead. The field may be changed at any time.
            public Func<int, long, bool> IsIdentRune; // Start position of most recently scanned token; set by Scan.
// Calling Init or Next invalidates the position (Line == 0).
// The Filename field is always left untouched by the Scanner.
// If an error is reported (via Error) and Position is invalid,
// the scanner is not inside a token. Call Pos to obtain an error
// position in that case, or to obtain the position immediately
// after the most recently scanned token.
            public ref Position Position => ref Position_val;
        }

        // Init initializes a Scanner with a new source and returns s.
        // Error is set to nil, ErrorCount is set to 0, Mode is set to GoTokens,
        // and Whitespace is set to GoWhitespace.
        private static ref Scanner Init(this ref Scanner s, io.Reader src)
        {
            s.src = src; 

            // initialize source buffer
            // (the first call to next() will fill it by calling src.Read)
            s.srcBuf[0L] = utf8.RuneSelf; // sentinel
            s.srcPos = 0L;
            s.srcEnd = 0L; 

            // initialize source position
            s.srcBufOffset = 0L;
            s.line = 1L;
            s.column = 0L;
            s.lastLineLen = 0L;
            s.lastCharLen = 0L; 

            // initialize token text buffer
            // (required for first call to next()).
            s.tokPos = -1L; 

            // initialize one character look-ahead
            s.ch = -2L; // no char read yet, not EOF

            // initialize public fields
            s.Error = null;
            s.ErrorCount = 0L;
            s.Mode = GoTokens;
            s.Whitespace = GoWhitespace;
            s.Line = 0L; // invalidate token position

            return s;
        }

        // next reads and returns the next Unicode character. It is designed such
        // that only a minimal amount of work needs to be done in the common ASCII
        // case (one test to check for both ASCII and end-of-buffer, and one test
        // to check for newlines).
        private static int next(this ref Scanner s)
        {
            var ch = rune(s.srcBuf[s.srcPos]);
            long width = 1L;

            if (ch >= utf8.RuneSelf)
            { 
                // uncommon case: not ASCII or not enough bytes
                while (s.srcPos + utf8.UTFMax > s.srcEnd && !utf8.FullRune(s.srcBuf[s.srcPos..s.srcEnd]))
                { 
                    // not enough bytes: read some more, but first
                    // save away token text if any
                    if (s.tokPos >= 0L)
                    {
                        s.tokBuf.Write(s.srcBuf[s.tokPos..s.srcPos]);
                        s.tokPos = 0L; 
                        // s.tokEnd is set by Scan()
                    } 
                    // move unread bytes to beginning of buffer
                    copy(s.srcBuf[0L..], s.srcBuf[s.srcPos..s.srcEnd]);
                    s.srcBufOffset += s.srcPos; 
                    // read more bytes
                    // (an io.Reader must return io.EOF when it reaches
                    // the end of what it is reading - simply returning
                    // n == 0 will make this loop retry forever; but the
                    // error is in the reader implementation in that case)
                    var i = s.srcEnd - s.srcPos;
                    var (n, err) = s.src.Read(s.srcBuf[i..bufLen]);
                    s.srcPos = 0L;
                    s.srcEnd = i + n;
                    s.srcBuf[s.srcEnd] = utf8.RuneSelf; // sentinel
                    if (err != null)
                    {
                        if (err != io.EOF)
                        {
                            s.error(err.Error());
                        }
                        if (s.srcEnd == 0L)
                        {
                            if (s.lastCharLen > 0L)
                            { 
                                // previous character was not EOF
                                s.column++;
                            }
                            s.lastCharLen = 0L;
                            return EOF;
                        } 
                        // If err == EOF, we won't be getting more
                        // bytes; break to avoid infinite loop. If
                        // err is something else, we don't know if
                        // we can get more bytes; thus also break.
                        break;
                    }
                } 
                // at least one byte
 
                // at least one byte
                ch = rune(s.srcBuf[s.srcPos]);
                if (ch >= utf8.RuneSelf)
                { 
                    // uncommon case: not ASCII
                    ch, width = utf8.DecodeRune(s.srcBuf[s.srcPos..s.srcEnd]);
                    if (ch == utf8.RuneError && width == 1L)
                    { 
                        // advance for correct error position
                        s.srcPos += width;
                        s.lastCharLen = width;
                        s.column++;
                        s.error("illegal UTF-8 encoding");
                        return ch;
                    }
                }
            } 

            // advance
            s.srcPos += width;
            s.lastCharLen = width;
            s.column++; 

            // special situations
            switch (ch)
            {
                case 0L: 
                    // for compatibility with other tools
                    s.error("illegal character NUL");
                    break;
                case '\n': 
                    s.line++;
                    s.lastLineLen = s.column;
                    s.column = 0L;
                    break;
            }

            return ch;
        }

        // Next reads and returns the next Unicode character.
        // It returns EOF at the end of the source. It reports
        // a read error by calling s.Error, if not nil; otherwise
        // it prints an error message to os.Stderr. Next does not
        // update the Scanner's Position field; use Pos() to
        // get the current position.
        private static int Next(this ref Scanner s)
        {
            s.tokPos = -1L; // don't collect token text
            s.Line = 0L; // invalidate token position
            var ch = s.Peek();
            if (ch != EOF)
            {
                s.ch = s.next();
            }
            return ch;
        }

        // Peek returns the next Unicode character in the source without advancing
        // the scanner. It returns EOF if the scanner's position is at the last
        // character of the source.
        private static int Peek(this ref Scanner s)
        {
            if (s.ch == -2L)
            { 
                // this code is only run for the very first character
                s.ch = s.next();
                if (s.ch == '\uFEFF')
                {
                    s.ch = s.next(); // ignore BOM
                }
            }
            return s.ch;
        }

        private static void error(this ref Scanner s, @string msg)
        {
            s.ErrorCount++;
            if (s.Error != null)
            {
                s.Error(s, msg);
                return;
            }
            var pos = s.Position;
            if (!pos.IsValid())
            {
                pos = s.Pos();
            }
            fmt.Fprintf(os.Stderr, "%s: %s\n", pos, msg);
        }

        private static bool isIdentRune(this ref Scanner s, int ch, long i)
        {
            if (s.IsIdentRune != null)
            {
                return s.IsIdentRune(ch, i);
            }
            return ch == '_' || unicode.IsLetter(ch) || unicode.IsDigit(ch) && i > 0L;
        }

        private static int scanIdentifier(this ref Scanner s)
        { 
            // we know the zero'th rune is OK; start scanning at the next one
            var ch = s.next();
            for (long i = 1L; s.isIdentRune(ch, i); i++)
            {
                ch = s.next();
            }

            return ch;
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

        private static bool isDecimal(int ch)
        {
            return '0' <= ch && ch <= '9';
        }

        private static int scanMantissa(this ref Scanner s, int ch)
        {
            while (isDecimal(ch))
            {
                ch = s.next();
            }

            return ch;
        }

        private static int scanFraction(this ref Scanner s, int ch)
        {
            if (ch == '.')
            {
                ch = s.scanMantissa(s.next());
            }
            return ch;
        }

        private static int scanExponent(this ref Scanner s, int ch)
        {
            if (ch == 'e' || ch == 'E')
            {
                ch = s.next();
                if (ch == '-' || ch == '+')
                {
                    ch = s.next();
                }
                ch = s.scanMantissa(ch);
            }
            return ch;
        }

        private static (int, int) scanNumber(this ref Scanner s, int ch)
        { 
            // isDecimal(ch)
            if (ch == '0')
            { 
                // int or float
                ch = s.next();
                if (ch == 'x' || ch == 'X')
                { 
                    // hexadecimal int
                    ch = s.next();
                    var hasMantissa = false;
                    while (digitVal(ch) < 16L)
                    {
                        ch = s.next();
                        hasMantissa = true;
                    }
                else

                    if (!hasMantissa)
                    {
                        s.error("illegal hexadecimal number");
                    }
                }                { 
                    // octal int or float
                    var has8or9 = false;
                    while (isDecimal(ch))
                    {
                        if (ch > '7')
                        {
                            has8or9 = true;
                        }
                        ch = s.next();
                    }

                    if (s.Mode & ScanFloats != 0L && (ch == '.' || ch == 'e' || ch == 'E'))
                    { 
                        // float
                        ch = s.scanFraction(ch);
                        ch = s.scanExponent(ch);
                        return (Float, ch);
                    } 
                    // octal int
                    if (has8or9)
                    {
                        s.error("illegal octal number");
                    }
                }
                return (Int, ch);
            } 
            // decimal int or float
            ch = s.scanMantissa(ch);
            if (s.Mode & ScanFloats != 0L && (ch == '.' || ch == 'e' || ch == 'E'))
            { 
                // float
                ch = s.scanFraction(ch);
                ch = s.scanExponent(ch);
                return (Float, ch);
            }
            return (Int, ch);
        }

        private static int scanDigits(this ref Scanner s, int ch, long @base, long n)
        {
            while (n > 0L && digitVal(ch) < base)
            {
                ch = s.next();
                n--;
            }

            if (n > 0L)
            {
                s.error("illegal char escape");
            }
            return ch;
        }

        private static int scanEscape(this ref Scanner s, int quote)
        {
            var ch = s.next(); // read character after '/'

            if (ch == 'a' || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v' || ch == '\\' || ch == quote) 
                // nothing to do
                ch = s.next();
            else if (ch == '0' || ch == '1' || ch == '2' || ch == '3' || ch == '4' || ch == '5' || ch == '6' || ch == '7') 
                ch = s.scanDigits(ch, 8L, 3L);
            else if (ch == 'x') 
                ch = s.scanDigits(s.next(), 16L, 2L);
            else if (ch == 'u') 
                ch = s.scanDigits(s.next(), 16L, 4L);
            else if (ch == 'U') 
                ch = s.scanDigits(s.next(), 16L, 8L);
            else 
                s.error("illegal char escape");
                        return ch;
        }

        private static long scanString(this ref Scanner s, int quote)
        {
            var ch = s.next(); // read character after quote
            while (ch != quote)
            {
                if (ch == '\n' || ch < 0L)
                {
                    s.error("literal not terminated");
                    return;
                }
                if (ch == '\\')
                {
                    ch = s.scanEscape(quote);
                }
                else
                {
                    ch = s.next();
                }
                n++;
            }

            return;
        }

        private static void scanRawString(this ref Scanner s)
        {
            var ch = s.next(); // read character after '`'
            while (ch != '`')
            {
                if (ch < 0L)
                {
                    s.error("literal not terminated");
                    return;
                }
                ch = s.next();
            }

        }

        private static void scanChar(this ref Scanner s)
        {
            if (s.scanString('\'') != 1L)
            {
                s.error("illegal char literal");
            }
        }

        private static int scanComment(this ref Scanner s, int ch)
        { 
            // ch == '/' || ch == '*'
            if (ch == '/')
            { 
                // line comment
                ch = s.next(); // read character after "//"
                while (ch != '\n' && ch >= 0L)
                {
                    ch = s.next();
                }

                return ch;
            } 

            // general comment
            ch = s.next(); // read character after "/*"
            while (true)
            {
                if (ch < 0L)
                {
                    s.error("comment not terminated");
                    break;
                }
                var ch0 = ch;
                ch = s.next();
                if (ch0 == '*' && ch == '/')
                {
                    ch = s.next();
                    break;
                }
            }

            return ch;
        }

        // Scan reads the next token or Unicode character from source and returns it.
        // It only recognizes tokens t for which the respective Mode bit (1<<-t) is set.
        // It returns EOF at the end of the source. It reports scanner errors (read and
        // token errors) by calling s.Error, if not nil; otherwise it prints an error
        // message to os.Stderr.
        private static int Scan(this ref Scanner s)
        {
            var ch = s.Peek(); 

            // reset token text position
            s.tokPos = -1L;
            s.Line = 0L;

redo: 

            // start collecting token text
            while (s.Whitespace & (1L << (int)(uint(ch))) != 0L)
            {
                ch = s.next();
            } 

            // start collecting token text
 

            // start collecting token text
            s.tokBuf.Reset();
            s.tokPos = s.srcPos - s.lastCharLen; 

            // set token position
            // (this is a slightly optimized version of the code in Pos())
            s.Offset = s.srcBufOffset + s.tokPos;
            if (s.column > 0L)
            { 
                // common case: last character was not a '\n'
                s.Line = s.line;
                s.Column = s.column;
            }
            else
            { 
                // last character was a '\n'
                // (we cannot be at the beginning of the source
                // since we have called next() at least once)
                s.Line = s.line - 1L;
                s.Column = s.lastLineLen;
            } 

            // determine token value
            var tok = ch;

            if (s.isIdentRune(ch, 0L)) 
                if (s.Mode & ScanIdents != 0L)
                {
                    tok = Ident;
                    ch = s.scanIdentifier();
                }
                else
                {
                    ch = s.next();
                }
            else if (isDecimal(ch)) 
                if (s.Mode & (ScanInts | ScanFloats) != 0L)
                {
                    tok, ch = s.scanNumber(ch);
                }
                else
                {
                    ch = s.next();
                }
            else 

                if (ch == EOF) 
                    break;
                else if (ch == '"') 
                    if (s.Mode & ScanStrings != 0L)
                    {
                        s.scanString('"');
                        tok = String;
                    }
                    ch = s.next();
                else if (ch == '\'') 
                    if (s.Mode & ScanChars != 0L)
                    {
                        s.scanChar();
                        tok = Char;
                    }
                    ch = s.next();
                else if (ch == '.') 
                    ch = s.next();
                    if (isDecimal(ch) && s.Mode & ScanFloats != 0L)
                    {
                        tok = Float;
                        ch = s.scanMantissa(ch);
                        ch = s.scanExponent(ch);
                    }
                else if (ch == '/') 
                    ch = s.next();
                    if ((ch == '/' || ch == '*') && s.Mode & ScanComments != 0L)
                    {
                        if (s.Mode & SkipComments != 0L)
                        {
                            s.tokPos = -1L; // don't collect token text
                            ch = s.scanComment(ch);
                            goto redo;
                        }
                        ch = s.scanComment(ch);
                        tok = Comment;
                    }
                else if (ch == '`') 
                    if (s.Mode & ScanRawStrings != 0L)
                    {
                        s.scanRawString();
                        tok = String;
                    }
                    ch = s.next();
                else 
                    ch = s.next();
                            // end of token text
            s.tokEnd = s.srcPos - s.lastCharLen;

            s.ch = ch;
            return tok;
        }

        // Pos returns the position of the character immediately after
        // the character or token returned by the last call to Next or Scan.
        // Use the Scanner's Position field for the start position of the most
        // recently scanned token.
        private static Position Pos(this ref Scanner s)
        {
            pos.Filename = s.Filename;
            pos.Offset = s.srcBufOffset + s.srcPos - s.lastCharLen;

            if (s.column > 0L) 
                // common case: last character was not a '\n'
                pos.Line = s.line;
                pos.Column = s.column;
            else if (s.lastLineLen > 0L) 
                // last character was a '\n'
                pos.Line = s.line - 1L;
                pos.Column = s.lastLineLen;
            else 
                // at the beginning of the source
                pos.Line = 1L;
                pos.Column = 1L;
                        return;
        }

        // TokenText returns the string corresponding to the most recently scanned token.
        // Valid after calling Scan().
        private static @string TokenText(this ref Scanner s)
        {
            if (s.tokPos < 0L)
            { 
                // no token text
                return "";
            }
            if (s.tokEnd < 0L)
            { 
                // if EOF was reached, s.tokEnd is set to -1 (s.srcPos == 0)
                s.tokEnd = s.tokPos;
            }
            if (s.tokBuf.Len() == 0L)
            { 
                // common case: the entire token text is still in srcBuf
                return string(s.srcBuf[s.tokPos..s.tokEnd]);
            } 

            // part of the token text was saved in tokBuf: save the rest in
            // tokBuf as well and return its content
            s.tokBuf.Write(s.srcBuf[s.tokPos..s.tokEnd]);
            s.tokPos = s.tokEnd; // ensure idempotency of TokenText() call
            return s.tokBuf.String();
        }
    }
}}
