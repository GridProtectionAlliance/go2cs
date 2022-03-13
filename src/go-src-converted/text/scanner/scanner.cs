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

// package scanner -- go2cs converted at 2022 March 13 05:54:26 UTC
// import "text/scanner" ==> using scanner = go.text.scanner_package
// Original source: C:\Program Files\Go\src\text\scanner\scanner.go
namespace go.text;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// Position is a value that represents a source position.
// A position is valid if Line > 0.

using System;
public static partial class scanner_package {

public partial struct Position {
    public @string Filename; // filename, if any
    public nint Offset; // byte offset, starting at 0
    public nint Line; // line number, starting at 1
    public nint Column; // column number, starting at 1 (character count per line)
}

// IsValid reports whether the position is valid.
private static bool IsValid(this ptr<Position> _addr_pos) {
    ref Position pos = ref _addr_pos.val;

    return pos.Line > 0;
}

public static @string String(this Position pos) {
    var s = pos.Filename;
    if (s == "") {
        s = "<input>";
    }
    if (pos.IsValid()) {
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
// Use GoTokens to configure the Scanner such that it accepts all Go
// literal tokens including Go identifiers. Comments will be skipped.
//
public static readonly nint ScanIdents = 1 << (int)(-Ident);
public static readonly nint ScanInts = 1 << (int)(-Int);
public static readonly nint ScanFloats = 1 << (int)(-Float); // includes Ints and hexadecimal floats
public static readonly nint ScanChars = 1 << (int)(-Char);
public static readonly nint ScanStrings = 1 << (int)(-String);
public static readonly nint ScanRawStrings = 1 << (int)(-RawString);
public static readonly nint ScanComments = 1 << (int)(-Comment);
public static readonly nint SkipComments = 1 << (int)(-skipComment); // if set with ScanComments, comments become white space
public static readonly var GoTokens = ScanIdents | ScanFloats | ScanChars | ScanStrings | ScanRawStrings | ScanComments | SkipComments;

// The result of Scan is one of these tokens or a Unicode character.
public static readonly var EOF = -(iota + 1);
public static readonly var Ident = 0;
public static readonly var Int = 1;
public static readonly var Float = 2;
public static readonly var Char = 3;
public static readonly var String = 4;
public static readonly var RawString = 5;
public static readonly var Comment = 6; 

// internal use only
private static readonly var skipComment = 7;

private static map tokenString = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<int, @string>{EOF:"EOF",Ident:"Ident",Int:"Int",Float:"Float",Char:"Char",String:"String",RawString:"RawString",Comment:"Comment",};

// TokenString returns a printable string for a token or Unicode character.
public static @string TokenString(int tok) {
    {
        var (s, found) = tokenString[tok];

        if (found) {
            return s;
        }
    }
    return fmt.Sprintf("%q", string(tok));
}

// GoWhitespace is the default value for the Scanner's Whitespace field.
// Its value selects Go's white space characters.
public static readonly nint GoWhitespace = 1 << (int)('\t') | 1 << (int)('\n') | 1 << (int)('\r') | 1 << (int)(' ');



private static readonly nint bufLen = 1024; // at least utf8.UTFMax

// A Scanner implements reading of Unicode characters and tokens from an io.Reader.
 // at least utf8.UTFMax

// A Scanner implements reading of Unicode characters and tokens from an io.Reader.
public partial struct Scanner {
    public io.Reader src; // Source buffer
    public array<byte> srcBuf; // +1 for sentinel for common case of s.next()
    public nint srcPos; // reading position (srcBuf index)
    public nint srcEnd; // source end (srcBuf index)

// Source position
    public nint srcBufOffset; // byte offset of srcBuf[0] in source
    public nint line; // line count
    public nint column; // character count
    public nint lastLineLen; // length of last line in characters (for correct column reporting)
    public nint lastCharLen; // length of last character in bytes

// Token text buffer
// Typically, token text is stored completely in srcBuf, but in general
// the token text's head may be buffered in tokBuf while the token text's
// tail is stored in srcBuf.
    public bytes.Buffer tokBuf; // token text head that is not in srcBuf anymore
    public nint tokPos; // token text tail position (srcBuf index); valid if >= 0
    public nint tokEnd; // token text tail end (srcBuf index)

// One character look-ahead
    public int ch; // character before current srcPos

// Error is called for each error encountered. If no Error
// function is set, the error is reported to os.Stderr.
    public Action<ptr<Scanner>, @string> Error; // ErrorCount is incremented by one for each error encountered.
    public nint ErrorCount; // The Mode field controls which tokens are recognized. For instance,
// to recognize Ints, set the ScanInts bit in Mode. The field may be
// changed at any time.
    public nuint Mode; // The Whitespace field controls which characters are recognized
// as white space. To recognize a character ch <= ' ' as white space,
// set the ch'th bit in Whitespace (the Scanner's behavior is undefined
// for values ch > ' '). The field may be changed at any time.
    public ulong Whitespace; // IsIdentRune is a predicate controlling the characters accepted
// as the ith rune in an identifier. The set of valid characters
// must not intersect with the set of white space characters.
// If no IsIdentRune function is set, regular Go identifiers are
// accepted instead. The field may be changed at any time.
    public Func<int, nint, bool> IsIdentRune; // Start position of most recently scanned token; set by Scan.
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
private static ptr<Scanner> Init(this ptr<Scanner> _addr_s, io.Reader src) {
    ref Scanner s = ref _addr_s.val;

    s.src = src; 

    // initialize source buffer
    // (the first call to next() will fill it by calling src.Read)
    s.srcBuf[0] = utf8.RuneSelf; // sentinel
    s.srcPos = 0;
    s.srcEnd = 0; 

    // initialize source position
    s.srcBufOffset = 0;
    s.line = 1;
    s.column = 0;
    s.lastLineLen = 0;
    s.lastCharLen = 0; 

    // initialize token text buffer
    // (required for first call to next()).
    s.tokPos = -1; 

    // initialize one character look-ahead
    s.ch = -2; // no char read yet, not EOF

    // initialize public fields
    s.Error = null;
    s.ErrorCount = 0;
    s.Mode = GoTokens;
    s.Whitespace = GoWhitespace;
    s.Line = 0; // invalidate token position

    return _addr_s!;
}

// next reads and returns the next Unicode character. It is designed such
// that only a minimal amount of work needs to be done in the common ASCII
// case (one test to check for both ASCII and end-of-buffer, and one test
// to check for newlines).
private static int next(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    var ch = rune(s.srcBuf[s.srcPos]);
    nint width = 1;

    if (ch >= utf8.RuneSelf) { 
        // uncommon case: not ASCII or not enough bytes
        while (s.srcPos + utf8.UTFMax > s.srcEnd && !utf8.FullRune(s.srcBuf[(int)s.srcPos..(int)s.srcEnd])) { 
            // not enough bytes: read some more, but first
            // save away token text if any
            if (s.tokPos >= 0) {
                s.tokBuf.Write(s.srcBuf[(int)s.tokPos..(int)s.srcPos]);
                s.tokPos = 0; 
                // s.tokEnd is set by Scan()
            } 
            // move unread bytes to beginning of buffer
            copy(s.srcBuf[(int)0..], s.srcBuf[(int)s.srcPos..(int)s.srcEnd]);
            s.srcBufOffset += s.srcPos; 
            // read more bytes
            // (an io.Reader must return io.EOF when it reaches
            // the end of what it is reading - simply returning
            // n == 0 will make this loop retry forever; but the
            // error is in the reader implementation in that case)
            var i = s.srcEnd - s.srcPos;
            var (n, err) = s.src.Read(s.srcBuf[(int)i..(int)bufLen]);
            s.srcPos = 0;
            s.srcEnd = i + n;
            s.srcBuf[s.srcEnd] = utf8.RuneSelf; // sentinel
            if (err != null) {
                if (err != io.EOF) {
                    s.error(err.Error());
                }
                if (s.srcEnd == 0) {
                    if (s.lastCharLen > 0) { 
                        // previous character was not EOF
                        s.column++;
                    }
                    s.lastCharLen = 0;
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
        ch = rune(s.srcBuf[s.srcPos]);
        if (ch >= utf8.RuneSelf) { 
            // uncommon case: not ASCII
            ch, width = utf8.DecodeRune(s.srcBuf[(int)s.srcPos..(int)s.srcEnd]);
            if (ch == utf8.RuneError && width == 1) { 
                // advance for correct error position
                s.srcPos += width;
                s.lastCharLen = width;
                s.column++;
                s.error("invalid UTF-8 encoding");
                return ch;
            }
        }
    }
    s.srcPos += width;
    s.lastCharLen = width;
    s.column++; 

    // special situations
    switch (ch) {
        case 0: 
            // for compatibility with other tools
            s.error("invalid character NUL");
            break;
        case '\n': 
            s.line++;
            s.lastLineLen = s.column;
            s.column = 0;
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
private static int Next(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    s.tokPos = -1; // don't collect token text
    s.Line = 0; // invalidate token position
    var ch = s.Peek();
    if (ch != EOF) {
        s.ch = s.next();
    }
    return ch;
}

// Peek returns the next Unicode character in the source without advancing
// the scanner. It returns EOF if the scanner's position is at the last
// character of the source.
private static int Peek(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    if (s.ch == -2) { 
        // this code is only run for the very first character
        s.ch = s.next();
        if (s.ch == '\uFEFF') {
            s.ch = s.next(); // ignore BOM
        }
    }
    return s.ch;
}

private static void error(this ptr<Scanner> _addr_s, @string msg) {
    ref Scanner s = ref _addr_s.val;

    s.tokEnd = s.srcPos - s.lastCharLen; // make sure token text is terminated
    s.ErrorCount++;
    if (s.Error != null) {
        s.Error(s, msg);
        return ;
    }
    var pos = s.Position;
    if (!pos.IsValid()) {
        pos = s.Pos();
    }
    fmt.Fprintf(os.Stderr, "%s: %s\n", pos, msg);
}

private static void errorf(this ptr<Scanner> _addr_s, @string format, params object[] args) {
    args = args.Clone();
    ref Scanner s = ref _addr_s.val;

    s.error(fmt.Sprintf(format, args));
}

private static bool isIdentRune(this ptr<Scanner> _addr_s, int ch, nint i) {
    ref Scanner s = ref _addr_s.val;

    if (s.IsIdentRune != null) {
        return s.IsIdentRune(ch, i);
    }
    return ch == '_' || unicode.IsLetter(ch) || unicode.IsDigit(ch) && i > 0;
}

private static int scanIdentifier(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;
 
    // we know the zero'th rune is OK; start scanning at the next one
    var ch = s.next();
    for (nint i = 1; s.isIdentRune(ch, i); i++) {
        ch = s.next();
    }
    return ch;
}

private static int lower(int ch) {
    return ('a' - 'A') | ch;
} // returns lower-case ch iff ch is ASCII letter
private static bool isDecimal(int ch) {
    return '0' <= ch && ch <= '9';
}
private static bool isHex(int ch) {
    return '0' <= ch && ch <= '9' || 'a' <= lower(ch) && lower(ch) <= 'f';
}

// digits accepts the sequence { digit | '_' } starting with ch0.
// If base <= 10, digits accepts any decimal digit but records
// the first invalid digit >= base in *invalid if *invalid == 0.
// digits returns the first rune that is not part of the sequence
// anymore, and a bitset describing whether the sequence contained
// digits (bit 0 is set), or separators '_' (bit 1 is set).
private static (int, nint) digits(this ptr<Scanner> _addr_s, int ch0, nint @base, ptr<int> _addr_invalid) {
    int ch = default;
    nint digsep = default;
    ref Scanner s = ref _addr_s.val;
    ref int invalid = ref _addr_invalid.val;

    ch = ch0;
    if (base <= 10) {
        var max = rune('0' + base);
        while (isDecimal(ch) || ch == '_') {
            nint ds = 1;
            if (ch == '_') {
                ds = 2;
            }
            else if (ch >= max && invalid == 0.val) {
                invalid = ch;
            }
            digsep |= ds;
            ch = s.next();
        }
    else
    } {
        while (isHex(ch) || ch == '_') {
            ds = 1;
            if (ch == '_') {
                ds = 2;
            }
            digsep |= ds;
            ch = s.next();
        }
    }
    return ;
}

private static (int, int) scanNumber(this ptr<Scanner> _addr_s, int ch, bool seenDot) {
    int _p0 = default;
    int _p0 = default;
    ref Scanner s = ref _addr_s.val;

    nint @base = 10; // number base
    var prefix = rune(0); // one of 0 (decimal), '0' (0-octal), 'x', 'o', or 'b'
    nint digsep = 0; // bit 0: digit present, bit 1: '_' present
    ref var invalid = ref heap(rune(0), out ptr<var> _addr_invalid); // invalid digit in literal, or 0

    // integer part
    int tok = default;
    nint ds = default;
    if (!seenDot) {
        tok = Int;
        if (ch == '0') {
            ch = s.next();
            switch (lower(ch)) {
                case 'x': 
                    ch = s.next();
                    (base, prefix) = (16, 'x');
                    break;
                case 'o': 
                    ch = s.next();
                    (base, prefix) = (8, 'o');
                    break;
                case 'b': 
                    ch = s.next();
                    (base, prefix) = (2, 'b');
                    break;
                default: 
                    (base, prefix) = (8, '0');                digsep = 1; // leading 0
                    break;
            }
        }
        ch, ds = s.digits(ch, base, _addr_invalid);
        digsep |= ds;
        if (ch == '.' && s.Mode & ScanFloats != 0) {
            ch = s.next();
            seenDot = true;
        }
    }
    if (seenDot) {
        tok = Float;
        if (prefix == 'o' || prefix == 'b') {
            s.error("invalid radix point in " + litname(prefix));
        }
        ch, ds = s.digits(ch, base, _addr_invalid);
        digsep |= ds;
    }
    if (digsep & 1 == 0) {
        s.error(litname(prefix) + " has no digits");
    }
    {
        var e = lower(ch);

        if ((e == 'e' || e == 'p') && s.Mode & ScanFloats != 0) {

            if (e == 'e' && prefix != 0 && prefix != '0') 
                s.errorf("%q exponent requires decimal mantissa", ch);
            else if (e == 'p' && prefix != 'x') 
                s.errorf("%q exponent requires hexadecimal mantissa", ch);
                        ch = s.next();
            tok = Float;
            if (ch == '+' || ch == '-') {
                ch = s.next();
            }
            ch, ds = s.digits(ch, 10, null);
            digsep |= ds;
            if (ds & 1 == 0) {
                s.error("exponent has no digits");
            }
        }
        else if (prefix == 'x' && tok == Float) {
            s.error("hexadecimal mantissa requires a 'p' exponent");
        }

    }

    if (tok == Int && invalid != 0) {
        s.errorf("invalid digit %q in %s", invalid, litname(prefix));
    }
    if (digsep & 2 != 0) {
        s.tokEnd = s.srcPos - s.lastCharLen; // make sure token text is terminated
        {
            var i = invalidSep(s.TokenText());

            if (i >= 0) {
                s.error("'_' must separate successive digits");
            }

        }
    }
    return (tok, ch);
}

private static @string litname(int prefix) {
    switch (prefix) {
        case 'x': 
            return "hexadecimal literal";
            break;
        case 'o': 

        case '0': 
            return "octal literal";
            break;
        case 'b': 
            return "binary literal";
            break;
        default: 
            return "decimal literal";
            break;
    }
}

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

private static nint digitVal(int ch) {

    if ('0' <= ch && ch <= '9') 
        return int(ch - '0');
    else if ('a' <= lower(ch) && lower(ch) <= 'f') 
        return int(lower(ch) - 'a' + 10);
        return 16; // larger than any legal digit val
}

private static int scanDigits(this ptr<Scanner> _addr_s, int ch, nint @base, nint n) {
    ref Scanner s = ref _addr_s.val;

    while (n > 0 && digitVal(ch) < base) {
        ch = s.next();
        n--;
    }
    if (n > 0) {
        s.error("invalid char escape");
    }
    return ch;
}

private static int scanEscape(this ptr<Scanner> _addr_s, int quote) {
    ref Scanner s = ref _addr_s.val;

    var ch = s.next(); // read character after '/'

    if (ch == 'a' || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v' || ch == '\\' || ch == quote) 
        // nothing to do
        ch = s.next();
    else if (ch == '0' || ch == '1' || ch == '2' || ch == '3' || ch == '4' || ch == '5' || ch == '6' || ch == '7') 
        ch = s.scanDigits(ch, 8, 3);
    else if (ch == 'x') 
        ch = s.scanDigits(s.next(), 16, 2);
    else if (ch == 'u') 
        ch = s.scanDigits(s.next(), 16, 4);
    else if (ch == 'U') 
        ch = s.scanDigits(s.next(), 16, 8);
    else 
        s.error("invalid char escape");
        return ch;
}

private static nint scanString(this ptr<Scanner> _addr_s, int quote) {
    nint n = default;
    ref Scanner s = ref _addr_s.val;

    var ch = s.next(); // read character after quote
    while (ch != quote) {
        if (ch == '\n' || ch < 0) {
            s.error("literal not terminated");
            return ;
        }
        if (ch == '\\') {
            ch = s.scanEscape(quote);
        }
        else
 {
            ch = s.next();
        }
        n++;
    }
    return ;
}

private static void scanRawString(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    var ch = s.next(); // read character after '`'
    while (ch != '`') {
        if (ch < 0) {
            s.error("literal not terminated");
            return ;
        }
        ch = s.next();
    }
}

private static void scanChar(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    if (s.scanString('\'') != 1) {
        s.error("invalid char literal");
    }
}

private static int scanComment(this ptr<Scanner> _addr_s, int ch) {
    ref Scanner s = ref _addr_s.val;
 
    // ch == '/' || ch == '*'
    if (ch == '/') { 
        // line comment
        ch = s.next(); // read character after "//"
        while (ch != '\n' && ch >= 0) {
            ch = s.next();
        }
        return ch;
    }
    ch = s.next(); // read character after "/*"
    while (true) {
        if (ch < 0) {
            s.error("comment not terminated");
            break;
        }
        var ch0 = ch;
        ch = s.next();
        if (ch0 == '*' && ch == '/') {
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
private static int Scan(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    var ch = s.Peek(); 

    // reset token text position
    s.tokPos = -1;
    s.Line = 0;

redo: 

    // start collecting token text
    while (s.Whitespace & (1 << (int)(uint(ch))) != 0) {
        ch = s.next();
    } 

    // start collecting token text
    s.tokBuf.Reset();
    s.tokPos = s.srcPos - s.lastCharLen; 

    // set token position
    // (this is a slightly optimized version of the code in Pos())
    s.Offset = s.srcBufOffset + s.tokPos;
    if (s.column > 0) { 
        // common case: last character was not a '\n'
        s.Line = s.line;
        s.Column = s.column;
    }
    else
 { 
        // last character was a '\n'
        // (we cannot be at the beginning of the source
        // since we have called next() at least once)
        s.Line = s.line - 1;
        s.Column = s.lastLineLen;
    }
    var tok = ch;

    if (s.isIdentRune(ch, 0)) 
        if (s.Mode & ScanIdents != 0) {
            tok = Ident;
            ch = s.scanIdentifier();
        }
        else
 {
            ch = s.next();
        }
    else if (isDecimal(ch)) 
        if (s.Mode & (ScanInts | ScanFloats) != 0) {
            tok, ch = s.scanNumber(ch, false);
        }
        else
 {
            ch = s.next();
        }
    else 

        if (ch == EOF) 
            break;
        else if (ch == '"') 
            if (s.Mode & ScanStrings != 0) {
                s.scanString('"');
                tok = String;
            }
            ch = s.next();
        else if (ch == '\'') 
            if (s.Mode & ScanChars != 0) {
                s.scanChar();
                tok = Char;
            }
            ch = s.next();
        else if (ch == '.') 
            ch = s.next();
            if (isDecimal(ch) && s.Mode & ScanFloats != 0) {
                tok, ch = s.scanNumber(ch, true);
            }
        else if (ch == '/') 
            ch = s.next();
            if ((ch == '/' || ch == '*') && s.Mode & ScanComments != 0) {
                if (s.Mode & SkipComments != 0) {
                    s.tokPos = -1; // don't collect token text
                    ch = s.scanComment(ch);
                    goto redo;
                }
                ch = s.scanComment(ch);
                tok = Comment;
            }
        else if (ch == '`') 
            if (s.Mode & ScanRawStrings != 0) {
                s.scanRawString();
                tok = RawString;
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
private static Position Pos(this ptr<Scanner> _addr_s) {
    Position pos = default;
    ref Scanner s = ref _addr_s.val;

    pos.Filename = s.Filename;
    pos.Offset = s.srcBufOffset + s.srcPos - s.lastCharLen;

    if (s.column > 0) 
        // common case: last character was not a '\n'
        pos.Line = s.line;
        pos.Column = s.column;
    else if (s.lastLineLen > 0) 
        // last character was a '\n'
        pos.Line = s.line - 1;
        pos.Column = s.lastLineLen;
    else 
        // at the beginning of the source
        pos.Line = 1;
        pos.Column = 1;
        return ;
}

// TokenText returns the string corresponding to the most recently scanned token.
// Valid after calling Scan and in calls of Scanner.Error.
private static @string TokenText(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    if (s.tokPos < 0) { 
        // no token text
        return "";
    }
    if (s.tokEnd < s.tokPos) { 
        // if EOF was reached, s.tokEnd is set to -1 (s.srcPos == 0)
        s.tokEnd = s.tokPos;
    }
    if (s.tokBuf.Len() == 0) { 
        // common case: the entire token text is still in srcBuf
        return string(s.srcBuf[(int)s.tokPos..(int)s.tokEnd]);
    }
    s.tokBuf.Write(s.srcBuf[(int)s.tokPos..(int)s.tokEnd]);
    s.tokPos = s.tokEnd; // ensure idempotency of TokenText() call
    return s.tokBuf.String();
}

} // end scanner_package
