// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package scanner provides a scanner and tokenizer for UTF-8-encoded text.
// It takes an io.Reader providing the source, which then can be tokenized
// through repeated calls to the Scan function. For compatibility with
// existing tools, the NUL character is not allowed. If the first character
// in the source is a UTF-8 encoded byte order mark (BOM), it is discarded.
//
// By default, a [Scanner] skips white space and Go comments and recognizes all
// literals as defined by the Go language specification. It may be
// customized to recognize only a subset of those literals and to recognize
// different identifier and white space characters.
namespace go.text;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class scanner_package {

// Position is a value that represents a source position.
// A position is valid if Line > 0.
[GoType] partial struct Position {
    public @string Filename; // filename, if any
    public nint Offset;   // byte offset, starting at 0
    public nint Line;   // line number, starting at 1
    public nint Column;   // column number, starting at 1 (character count per line)
}

// IsValid reports whether the position is valid.
[GoRecv] public static bool IsValid(this ref Position pos) {
    return pos.Line > 0;
}

public static @string String(this Position pos) {
    @string s = pos.Filename;
    if (s == ""u8) {
        s = "<input>"u8;
    }
    if (pos.IsValid()) {
        s += fmt.Sprintf(":%d:%d"u8, pos.Line, pos.Column);
    }
    return s;
}

// Predefined mode bits to control recognition of tokens. For instance,
// to configure a [Scanner] such that it only recognizes (Go) identifiers,
// integers, and skips comments, set the Scanner's Mode field to:
//
//	ScanIdents | ScanInts | SkipComments
//
// With the exceptions of comments, which are skipped if SkipComments is
// set, unrecognized tokens are not ignored. Instead, the scanner simply
// returns the respective individual characters (or possibly sub-tokens).
// For instance, if the mode is ScanIdents (not ScanStrings), the string
// "foo" is scanned as the token sequence '"' [Ident] '"'.
//
// Use GoTokens to configure the Scanner such that it accepts all Go
// literal tokens including Go identifiers. Comments will be skipped.
public static readonly UntypedInt ScanIdents = /* 1 << -Ident */ 4;

public static readonly UntypedInt ScanInts = /* 1 << -Int */ 8;

public static readonly UntypedInt ScanFloats = /* 1 << -Float */ 16; // includes Ints and hexadecimal floats

public static readonly UntypedInt ScanChars = /* 1 << -Char */ 32;

public static readonly UntypedInt ScanStrings = /* 1 << -String */ 64;

public static readonly UntypedInt ScanRawStrings = /* 1 << -RawString */ 128;

public static readonly UntypedInt ScanComments = /* 1 << -Comment */ 256;

public static readonly UntypedInt SkipComments = /* 1 << -skipComment */ 512; // if set with ScanComments, comments become white space

public static readonly UntypedInt GoTokens = /* ScanIdents | ScanFloats | ScanChars | ScanStrings | ScanRawStrings | ScanComments | SkipComments */ 1012;

// The result of Scan is one of these tokens or a Unicode character.
public static readonly UntypedInt EOF = /* -(iota + 1) */ -1;

public static readonly UntypedInt Ident = -2;

public static readonly UntypedInt Int = -3;

public static readonly UntypedInt Float = -4;

public static readonly UntypedInt Char = -5;

public static readonly UntypedInt ΔString = -6;

public static readonly UntypedInt RawString = -7;

public static readonly UntypedInt Comment = -8;

internal static readonly UntypedInt skipComment = -9;

internal static map<rune, @string> tokenString = new map<rune, @string>{
    [EOF] = "EOF"u8,
    [Ident] = "Ident"u8,
    [Int] = "Int"u8,
    [Float] = "Float"u8,
    [Char] = "Char"u8,
    [ΔString] = "String"u8,
    [RawString] = "RawString"u8,
    [Comment] = "Comment"u8
};

// TokenString returns a printable string for a token or Unicode character.
public static @string TokenString(rune tok) {
    {
        @string s = tokenString[tok];
        var found = tokenString[tok]; if (found) {
            return s;
        }
    }
    return fmt.Sprintf("%q"u8, ((@string)tok));
}

// GoWhitespace is the default value for the [Scanner]'s Whitespace field.
// Its value selects Go's white space characters.
public static readonly UntypedInt GoWhitespace = /* 1<<'\t' | 1<<'\n' | 1<<'\r' | 1<<' ' */ 4294977024;

internal static readonly UntypedInt bufLen = 1024; // at least utf8.UTFMax

// A Scanner implements reading of Unicode characters and tokens from an [io.Reader].
[GoType] partial struct Scanner {
    // Input
    internal io_package.Reader src;
    // Source buffer
    internal array<byte> srcBuf = new(bufLen + 1); // +1 for sentinel for common case of s.next()
    internal nint srcPos;             // reading position (srcBuf index)
    internal nint srcEnd;             // source end (srcBuf index)
    // Source position
    internal nint srcBufOffset; // byte offset of srcBuf[0] in source
    internal nint line; // line count
    internal nint column; // character count
    internal nint lastLineLen; // length of last line in characters (for correct column reporting)
    internal nint lastCharLen; // length of last character in bytes
    // Token text buffer
    // Typically, token text is stored completely in srcBuf, but in general
    // the token text's head may be buffered in tokBuf while the token text's
    // tail is stored in srcBuf.
    internal bytes_package.Buffer tokBuf; // token text head that is not in srcBuf anymore
    internal nint tokPos;         // token text tail position (srcBuf index); valid if >= 0
    internal nint tokEnd;         // token text tail end (srcBuf index)
    // One character look-ahead
    internal rune ch; // character before current srcPos
    // Error is called for each error encountered. If no Error
    // function is set, the error is reported to os.Stderr.
    public Action<ж<Scanner>, @string> Error;
    // ErrorCount is incremented by one for each error encountered.
    public nint ErrorCount;
    // The Mode field controls which tokens are recognized. For instance,
    // to recognize Ints, set the ScanInts bit in Mode. The field may be
    // changed at any time.
    public nuint Mode;
    // The Whitespace field controls which characters are recognized
    // as white space. To recognize a character ch <= ' ' as white space,
    // set the ch'th bit in Whitespace (the Scanner's behavior is undefined
    // for values ch > ' '). The field may be changed at any time.
    public uint64 Whitespace;
    // IsIdentRune is a predicate controlling the characters accepted
    // as the ith rune in an identifier. The set of valid characters
    // must not intersect with the set of white space characters.
    // If no IsIdentRune function is set, regular Go identifiers are
    // accepted instead. The field may be changed at any time.
    public Func<rune, nint, bool> IsIdentRune;
    // Start position of most recently scanned token; set by Scan.
    // Calling Init or Next invalidates the position (Line == 0).
    // The Filename field is always left untouched by the Scanner.
    // If an error is reported (via Error) and Position is invalid,
    // the scanner is not inside a token. Call Pos to obtain an error
    // position in that case, or to obtain the position immediately
    // after the most recently scanned token.
    public partial ref Position Position { get; }
}

// Init initializes a [Scanner] with a new source and returns s.
// [Scanner.Error] is set to nil, [Scanner.ErrorCount] is set to 0, [Scanner.Mode] is set to [GoTokens],
// and [Scanner.Whitespace] is set to [GoWhitespace].
[GoRecv("capture")] public static ж<Scanner> Init(this ref Scanner s, io.Reader src) {
    s.src = src;
    // initialize source buffer
    // (the first call to next() will fill it by calling src.Read)
    s.srcBuf[0] = utf8.RuneSelf;
    // sentinel
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
    s.ch = -2;
    // no char read yet, not EOF
    // initialize public fields
    s.Error = default!;
    s.ErrorCount = 0;
    s.Mode = GoTokens;
    s.Whitespace = GoWhitespace;
    s.Line = 0;
    // invalidate token position
    return InitꓸᏑs;
}

// next reads and returns the next Unicode character. It is designed such
// that only a minimal amount of work needs to be done in the common ASCII
// case (one test to check for both ASCII and end-of-buffer, and one test
// to check for newlines).
[GoRecv] internal static rune next(this ref Scanner s) {
    var ch = ((rune)s.srcBuf[s.srcPos]);
    nint width = 1;
    if (ch >= utf8.RuneSelf) {
        // uncommon case: not ASCII or not enough bytes
        while (s.srcPos + utf8.UTFMax > s.srcEnd && !utf8.FullRune(s.srcBuf[(int)(s.srcPos)..(int)(s.srcEnd)])) {
            // not enough bytes: read some more, but first
            // save away token text if any
            if (s.tokPos >= 0) {
                s.tokBuf.Write(s.srcBuf[(int)(s.tokPos)..(int)(s.srcPos)]);
                s.tokPos = 0;
            }
            // s.tokEnd is set by Scan()
            // move unread bytes to beginning of buffer
            copy(s.srcBuf[0..], s.srcBuf[(int)(s.srcPos)..(int)(s.srcEnd)]);
            s.srcBufOffset += s.srcPos;
            // read more bytes
            // (an io.Reader must return io.EOF when it reaches
            // the end of what it is reading - simply returning
            // n == 0 will make this loop retry forever; but the
            // error is in the reader implementation in that case)
            nint i = s.srcEnd - s.srcPos;
            var (n, err) = s.src.Read(s.srcBuf[(int)(i)..(int)(bufLen)]);
            s.srcPos = 0;
            s.srcEnd = i + n;
            s.srcBuf[s.srcEnd] = utf8.RuneSelf;
            // sentinel
            if (err != default!) {
                if (!AreEqual(err, io.EOF)) {
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
        ch = ((rune)s.srcBuf[s.srcPos]);
        if (ch >= utf8.RuneSelf) {
            // uncommon case: not ASCII
            (ch, width) = utf8.DecodeRune(s.srcBuf[(int)(s.srcPos)..(int)(s.srcEnd)]);
            if (ch == utf8.RuneError && width == 1) {
                // advance for correct error position
                s.srcPos += width;
                s.lastCharLen = width;
                s.column++;
                s.error("invalid UTF-8 encoding"u8);
                return ch;
            }
        }
    }
    // advance
    s.srcPos += width;
    s.lastCharLen = width;
    s.column++;
    // special situations
    switch (ch) {
    case 0: {
        s.error("invalid character NUL"u8);
        break;
    }
    case (rune)'\n': {
        s.line++;
        s.lastLineLen = s.column;
        s.column = 0;
        break;
    }}

    // for compatibility with other tools
    return ch;
}

// Next reads and returns the next Unicode character.
// It returns [EOF] at the end of the source. It reports
// a read error by calling s.Error, if not nil; otherwise
// it prints an error message to [os.Stderr]. Next does not
// update the [Scanner.Position] field; use [Scanner.Pos]() to
// get the current position.
[GoRecv] public static rune Next(this ref Scanner s) {
    s.tokPos = -1;
    // don't collect token text
    s.Line = 0;
    // invalidate token position
    var ch = s.Peek();
    if (ch != EOF) {
        s.ch = s.next();
    }
    return ch;
}

// Peek returns the next Unicode character in the source without advancing
// the scanner. It returns [EOF] if the scanner's position is at the last
// character of the source.
[GoRecv] public static rune Peek(this ref Scanner s) {
    if (s.ch == -2) {
        // this code is only run for the very first character
        s.ch = s.next();
        if (s.ch == (rune)'\uFEFF') {
            s.ch = s.next();
        }
    }
    // ignore BOM
    return s.ch;
}

[GoRecv] internal static void error(this ref Scanner s, @string msg) {
    s.tokEnd = s.srcPos - s.lastCharLen;
    // make sure token text is terminated
    s.ErrorCount++;
    if (s.Error != default!) {
        s.Error(s, msg);
        return;
    }
    var pos = s.Position;
    if (!pos.IsValid()) {
        pos = s.Pos();
    }
    fmt.Fprintf(~os.Stderr, "%s: %s\n"u8, pos, msg);
}

[GoRecv] internal static void errorf(this ref Scanner s, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    s.error(fmt.Sprintf(format, args.ꓸꓸꓸ));
}

[GoRecv] internal static bool isIdentRune(this ref Scanner s, rune ch, nint i) {
    if (s.IsIdentRune != default!) {
        return ch != EOF && s.IsIdentRune(ch, i);
    }
    return ch == (rune)'_' || unicode.IsLetter(ch) || unicode.IsDigit(ch) && i > 0;
}

[GoRecv] internal static rune scanIdentifier(this ref Scanner s) {
    // we know the zero'th rune is OK; start scanning at the next one
    var ch = s.next();
    for (nint i = 1; s.isIdentRune(ch, i); i++) {
        ch = s.next();
    }
    return ch;
}

internal static rune lower(rune ch) {
    return (rune)(((rune)'a' - (rune)'A') | ch);
}

// returns lower-case ch iff ch is ASCII letter
internal static bool isDecimal(rune ch) {
    return (rune)'0' <= ch && ch <= (rune)'9';
}

internal static bool isHex(rune ch) {
    return (rune)'0' <= ch && ch <= (rune)'9' || (rune)'a' <= lower(ch) && lower(ch) <= (rune)'f';
}

// digits accepts the sequence { digit | '_' } starting with ch0.
// If base <= 10, digits accepts any decimal digit but records
// the first invalid digit >= base in *invalid if *invalid == 0.
// digits returns the first rune that is not part of the sequence
// anymore, and a bitset describing whether the sequence contained
// digits (bit 0 is set), or separators '_' (bit 1 is set).
[GoRecv] public static (rune ch, nint digsep) digits(this ref Scanner s, rune ch0, nint @base, ж<rune> Ꮡinvalid) {
    rune ch = default!;
    nint digsep = default!;

    ref var invalid = ref Ꮡinvalid.val;
    ch = ch0;
    if (@base <= 10){
        var max = ((rune)((rune)'0' + @base));
        while (isDecimal(ch) || ch == (rune)'_') {
            nint ds = 1;
            if (ch == (rune)'_'){
                ds = 2;
            } else 
            if (ch >= max && invalid == 0) {
                invalid = ch;
            }
            digsep |= (nint)(ds);
            ch = s.next();
        }
    } else {
        while (isHex(ch) || ch == (rune)'_') {
            nint ds = 1;
            if (ch == (rune)'_') {
                ds = 2;
            }
            digsep |= (nint)(ds);
            ch = s.next();
        }
    }
    return (ch, digsep);
}

[GoRecv] internal static (rune, rune) scanNumber(this ref Scanner s, rune ch, bool seenDot) {
    nint @base = 10;
    // number base
    var prefix = ((rune)0);
    // one of 0 (decimal), '0' (0-octal), 'x', 'o', or 'b'
    nint digsep = 0;
    // bit 0: digit present, bit 1: '_' present
    ref var invalid = ref heap<rune>(out var Ꮡinvalid);
    invalid = ((rune)0);
    // invalid digit in literal, or 0
    // integer part
    rune tok = default!;
    nint ds = default!;
    if (!seenDot) {
        tok = Int;
        if (ch == (rune)'0') {
            ch = s.next();
            switch (lower(ch)) {
            case (rune)'x': {
                ch = s.next();
                (@base, prefix) = (16, (rune)'x');
                break;
            }
            case (rune)'o': {
                ch = s.next();
                (@base, prefix) = (8, (rune)'o');
                break;
            }
            case (rune)'b': {
                ch = s.next();
                (@base, prefix) = (2, (rune)'b');
                break;
            }
            default: {
                (@base, prefix) = (8, (rune)'0');
                digsep = 1;
                break;
            }}

        }
        // leading 0
        (ch, ds) = s.digits(ch, @base, Ꮡinvalid);
        digsep |= (nint)(ds);
        if (ch == (rune)'.' && (nuint)(s.Mode & ScanFloats) != 0) {
            ch = s.next();
            seenDot = true;
        }
    }
    // fractional part
    if (seenDot) {
        tok = Float;
        if (prefix == (rune)'o' || prefix == (rune)'b') {
            s.error("invalid radix point in "u8 + litname(prefix));
        }
        (ch, ds) = s.digits(ch, @base, Ꮡinvalid);
        digsep |= (nint)(ds);
    }
    if ((nint)(digsep & 1) == 0) {
        s.error(litname(prefix) + " has no digits"u8);
    }
    // exponent
    {
        var e = lower(ch); if ((e == (rune)'e' || e == (rune)'p') && (nuint)(s.Mode & ScanFloats) != 0){
            switch (ᐧ) {
            case {} when e == (rune)'e' && prefix != 0 && prefix != (rune)'0': {
                s.errorf("%q exponent requires decimal mantissa"u8, ch);
                break;
            }
            case {} when e == (rune)'p' && prefix != (rune)'x': {
                s.errorf("%q exponent requires hexadecimal mantissa"u8, ch);
                break;
            }}

            ch = s.next();
            tok = Float;
            if (ch == (rune)'+' || ch == (rune)'-') {
                ch = s.next();
            }
            (ch, ds) = s.digits(ch, 10, nil);
            digsep |= (nint)(ds);
            if ((nint)(ds & 1) == 0) {
                s.error("exponent has no digits"u8);
            }
        } else 
        if (prefix == (rune)'x' && tok == Float) {
            s.error("hexadecimal mantissa requires a 'p' exponent"u8);
        }
    }
    if (tok == Int && invalid != 0) {
        s.errorf("invalid digit %q in %s"u8, invalid, litname(prefix));
    }
    if ((nint)(digsep & 2) != 0) {
        s.tokEnd = s.srcPos - s.lastCharLen;
        // make sure token text is terminated
        {
            nint i = invalidSep(s.TokenText()); if (i >= 0) {
                s.error("'_' must separate successive digits"u8);
            }
        }
    }
    return (tok, ch);
}

internal static @string litname(rune prefix) {
    switch (prefix) {
    default: {
        return "decimal literal"u8;
    }
    case (rune)'x': {
        return "hexadecimal literal"u8;
    }
    case (rune)'o' or (rune)'0': {
        return "octal literal"u8;
    }
    case (rune)'b': {
        return "binary literal"u8;
    }}

}

// invalidSep returns the index of the first invalid separator in x, or -1.
internal static nint invalidSep(@string x) {
    var x1 = (rune)' ';
    // prefix char, we only care if it's 'x'
    var d = (rune)'.';
    // digit, one of '_', '0' (a digit), or '.' (anything else)
    nint i = 0;
    // a prefix counts as a digit
    if (len(x) >= 2 && x[0] == (rune)'0') {
        x1 = lower(((rune)x[1]));
        if (x1 == (rune)'x' || x1 == (rune)'o' || x1 == (rune)'b') {
            d = (rune)'0';
            i = 2;
        }
    }
    // mantissa and exponent
    for (; i < len(x); i++) {
        var p = d;
        // previous digit
        d = ((rune)x[i]);
        switch (ᐧ) {
        case {} when d is (rune)'_': {
            if (p != (rune)'0') {
                return i;
            }
            break;
        }
        case {} when isDecimal(d) || x1 == (rune)'x' && isHex(d): {
            d = (rune)'0';
            break;
        }
        default: {
            if (p == (rune)'_') {
                return i - 1;
            }
            d = (rune)'.';
            break;
        }}

    }
    if (d == (rune)'_') {
        return len(x) - 1;
    }
    return -1;
}

internal static nint digitVal(rune ch) {
    switch (ᐧ) {
    case {} when (rune)'0' <= ch && ch <= (rune)'9': {
        return ((nint)(ch - (rune)'0'));
    }
    case {} when (rune)'a' <= lower(ch) && lower(ch) <= (rune)'f': {
        return ((nint)(lower(ch) - (rune)'a' + 10));
    }}

    return 16;
}

// larger than any legal digit val
[GoRecv] internal static rune scanDigits(this ref Scanner s, rune ch, nint @base, nint n) {
    while (n > 0 && digitVal(ch) < @base) {
        ch = s.next();
        n--;
    }
    if (n > 0) {
        s.error("invalid char escape"u8);
    }
    return ch;
}

[GoRecv] internal static rune scanEscape(this ref Scanner s, rune quote) {
    var ch = s.next();
    // read character after '/'
    switch (ch) {
    case (rune)'a' or (rune)'b' or (rune)'f' or (rune)'n' or (rune)'r' or (rune)'t' or (rune)'v' or (rune)'\\' or quote: {
        ch = s.next();
        break;
    }
    case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7': {
        ch = s.scanDigits(ch, // nothing to do
 8, 3);
        break;
    }
    case (rune)'x': {
        ch = s.scanDigits(s.next(), 16, 2);
        break;
    }
    case (rune)'u': {
        ch = s.scanDigits(s.next(), 16, 4);
        break;
    }
    case (rune)'U': {
        ch = s.scanDigits(s.next(), 16, 8);
        break;
    }
    default: {
        s.error("invalid char escape"u8);
        break;
    }}

    return ch;
}

[GoRecv] internal static nint /*n*/ scanString(this ref Scanner s, rune quote) {
    nint n = default!;

    var ch = s.next();
    // read character after quote
    while (ch != quote) {
        if (ch == (rune)'\n' || ch < 0) {
            s.error("literal not terminated"u8);
            return n;
        }
        if (ch == (rune)'\\'){
            ch = s.scanEscape(quote);
        } else {
            ch = s.next();
        }
        n++;
    }
    return n;
}

[GoRecv] internal static void scanRawString(this ref Scanner s) {
    var ch = s.next();
    // read character after '`'
    while (ch != (rune)'`') {
        if (ch < 0) {
            s.error("literal not terminated"u8);
            return;
        }
        ch = s.next();
    }
}

[GoRecv] internal static void scanChar(this ref Scanner s) {
    if (s.scanString((rune)'\'') != 1) {
        s.error("invalid char literal"u8);
    }
}

[GoRecv] internal static rune scanComment(this ref Scanner s, rune ch) {
    // ch == '/' || ch == '*'
    if (ch == (rune)'/') {
        // line comment
        ch = s.next();
        // read character after "//"
        while (ch != (rune)'\n' && ch >= 0) {
            ch = s.next();
        }
        return ch;
    }
    // general comment
    ch = s.next();
    // read character after "/*"
    while (ᐧ) {
        if (ch < 0) {
            s.error("comment not terminated"u8);
            break;
        }
        var ch0 = ch;
        ch = s.next();
        if (ch0 == (rune)'*' && ch == (rune)'/') {
            ch = s.next();
            break;
        }
    }
    return ch;
}

// Scan reads the next token or Unicode character from source and returns it.
// It only recognizes tokens t for which the respective [Scanner.Mode] bit (1<<-t) is set.
// It returns [EOF] at the end of the source. It reports scanner errors (read and
// token errors) by calling s.Error, if not nil; otherwise it prints an error
// message to [os.Stderr].
[GoRecv] public static rune Scan(this ref Scanner s) {
    var ch = s.Peek();
    // reset token text position
    s.tokPos = -1;
    s.Line = 0;
redo:
    while ((uint64)(s.Whitespace & (1 << (int)(((nuint)ch)))) != 0) {
        // skip white space
        ch = s.next();
continue_redo:;
    }
break_redo:;
    // start collecting token text
    s.tokBuf.Reset();
    s.tokPos = s.srcPos - s.lastCharLen;
    // set token position
    // (this is a slightly optimized version of the code in Pos())
    s.Offset = s.srcBufOffset + s.tokPos;
    if (s.column > 0){
        // common case: last character was not a '\n'
        s.Line = s.line;
        s.Column = s.column;
    } else {
        // last character was a '\n'
        // (we cannot be at the beginning of the source
        // since we have called next() at least once)
        s.Line = s.line - 1;
        s.Column = s.lastLineLen;
    }
    // determine token value
    var tok = ch;
    switch (ᐧ) {
    case {} when s.isIdentRune(ch, 0): {
        if ((nuint)(s.Mode & ScanIdents) != 0){
            tok = Ident;
            ch = s.scanIdentifier();
        } else {
            ch = s.next();
        }
        break;
    }
    case {} when isDecimal(ch): {
        if ((nuint)(s.Mode & ((nuint)(ScanInts | ScanFloats))) != 0){
            (tok, ch) = s.scanNumber(ch, false);
        } else {
            ch = s.next();
        }
        break;
    }
    default: {
        switch (ch) {
        case EOF: {
            break;
            break;
        }
        case (rune)'"': {
            if ((nuint)(s.Mode & ScanStrings) != 0) {
                s.scanString((rune)'"');
                tok = ΔString;
            }
            ch = s.next();
            break;
        }
        case (rune)'\'': {
            if ((nuint)(s.Mode & ScanChars) != 0) {
                s.scanChar();
                tok = Char;
            }
            ch = s.next();
            break;
        }
        case (rune)'.': {
            ch = s.next();
            if (isDecimal(ch) && (nuint)(s.Mode & ScanFloats) != 0) {
                (tok, ch) = s.scanNumber(ch, true);
            }
            break;
        }
        case (rune)'/': {
            ch = s.next();
            if ((ch == (rune)'/' || ch == (rune)'*') && (nuint)(s.Mode & ScanComments) != 0) {
                if ((nuint)(s.Mode & SkipComments) != 0) {
                    s.tokPos = -1;
                    // don't collect token text
                    ch = s.scanComment(ch);
                    goto redo;
                }
                ch = s.scanComment(ch);
                tok = Comment;
            }
            break;
        }
        case (rune)'`': {
            if ((nuint)(s.Mode & ScanRawStrings) != 0) {
                s.scanRawString();
                tok = RawString;
            }
            ch = s.next();
            break;
        }
        default: {
            ch = s.next();
            break;
        }}

        break;
    }}

    // end of token text
    s.tokEnd = s.srcPos - s.lastCharLen;
    s.ch = ch;
    return tok;
}

// Pos returns the position of the character immediately after
// the character or token returned by the last call to [Scanner.Next] or [Scanner.Scan].
// Use the [Scanner.Position] field for the start position of the most
// recently scanned token.
[GoRecv] public static Position /*pos*/ Pos(this ref Scanner s) {
    Position pos = default!;

    pos.Filename = s.Filename;
    pos.Offset = s.srcBufOffset + s.srcPos - s.lastCharLen;
    switch (ᐧ) {
    case {} when s.column is > 0: {
        pos.Line = s.line;
        pos.Column = s.column;
        break;
    }
    case {} when s.lastLineLen is > 0: {
        pos.Line = s.line - 1;
        pos.Column = s.lastLineLen;
        break;
    }
    default: {
        pos.Line = 1;
        pos.Column = 1;
        break;
    }}

    // common case: last character was not a '\n'
    // last character was a '\n'
    // at the beginning of the source
    return pos;
}

// TokenText returns the string corresponding to the most recently scanned token.
// Valid after calling [Scanner.Scan] and in calls of [Scanner.Error].
[GoRecv] public static @string TokenText(this ref Scanner s) {
    if (s.tokPos < 0) {
        // no token text
        return ""u8;
    }
    if (s.tokEnd < s.tokPos) {
        // if EOF was reached, s.tokEnd is set to -1 (s.srcPos == 0)
        s.tokEnd = s.tokPos;
    }
    // s.tokEnd >= s.tokPos
    if (s.tokBuf.Len() == 0) {
        // common case: the entire token text is still in srcBuf
        return ((@string)(s.srcBuf[(int)(s.tokPos)..(int)(s.tokEnd)]));
    }
    // part of the token text was saved in tokBuf: save the rest in
    // tokBuf as well and return its content
    s.tokBuf.Write(s.srcBuf[(int)(s.tokPos)..(int)(s.tokEnd)]);
    s.tokPos = s.tokEnd;
    // ensure idempotency of TokenText() call
    return s.tokBuf.String();
}

} // end scanner_package
