// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package scanner implements a scanner for Go source text.
// It takes a []byte as source which can then be tokenized
// through repeated calls to the Scan method.
//

// package scanner -- go2cs converted at 2022 March 13 05:53:58 UTC
// import "go/scanner" ==> using scanner = go.go.scanner_package
// Original source: C:\Program Files\Go\src\go\scanner\scanner.go
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using token = go.token_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// An ErrorHandler may be provided to Scanner.Init. If a syntax error is
// encountered and a handler was installed, the handler is called with a
// position and an error message. The position points to the beginning of
// the offending token.
//

using System;
public static partial class scanner_package {

public delegate void ErrorHandler(token.Position, @string);

// A Scanner holds the scanner's internal state while processing
// a given text. It can be allocated as part of another data
// structure but must be initialized via Init before use.
//
public partial struct Scanner {
    public ptr<token.File> file; // source file handle
    public @string dir; // directory portion of file.Name()
    public slice<byte> src; // source
    public ErrorHandler err; // error reporting; or nil
    public Mode mode; // scanning mode

// scanning state
    public int ch; // current character
    public nint offset; // character offset
    public nint rdOffset; // reading offset (position after current character)
    public nint lineOffset; // current line offset
    public bool insertSemi; // insert a semicolon before next newline

// public state - ok to modify
    public nint ErrorCount; // number of errors encountered
}

private static readonly nuint bom = 0xFEFF; // byte order mark, only permitted as very first character
private static readonly nint eof = -1; // end of file

// Read the next Unicode char into s.ch.
// s.ch < 0 means end-of-file.
//
// For optimization, there is some overlap between this method and
// s.scanIdentifier.
private static void next(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    if (s.rdOffset < len(s.src)) {
        s.offset = s.rdOffset;
        if (s.ch == '\n') {
            s.lineOffset = s.offset;
            s.file.AddLine(s.offset);
        }
        var r = rune(s.src[s.rdOffset]);
        nint w = 1;

        if (r == 0) 
            s.error(s.offset, "illegal character NUL");
        else if (r >= utf8.RuneSelf) 
            // not ASCII
            r, w = utf8.DecodeRune(s.src[(int)s.rdOffset..]);
            if (r == utf8.RuneError && w == 1) {
                s.error(s.offset, "illegal UTF-8 encoding");
            }
            else if (r == bom && s.offset > 0) {
                s.error(s.offset, "illegal byte order mark");
            }
                s.rdOffset += w;
        s.ch = r;
    }
    else
 {
        s.offset = len(s.src);
        if (s.ch == '\n') {
            s.lineOffset = s.offset;
            s.file.AddLine(s.offset);
        }
        s.ch = eof;
    }
}

// peek returns the byte following the most recently read character without
// advancing the scanner. If the scanner is at EOF, peek returns 0.
private static byte peek(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    if (s.rdOffset < len(s.src)) {
        return s.src[s.rdOffset];
    }
    return 0;
}

// A mode value is a set of flags (or 0).
// They control scanner behavior.
//
public partial struct Mode { // : nuint
}

public static readonly Mode ScanComments = 1 << (int)(iota); // return comments as COMMENT tokens
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
private static void Init(this ptr<Scanner> _addr_s, ptr<token.File> _addr_file, slice<byte> src, ErrorHandler err, Mode mode) => func((_, panic, _) => {
    ref Scanner s = ref _addr_s.val;
    ref token.File file = ref _addr_file.val;
 
    // Explicitly initialize all fields since a scanner may be reused.
    if (file.Size() != len(src)) {
        panic(fmt.Sprintf("file size (%d) does not match src len (%d)", file.Size(), len(src)));
    }
    s.file = file;
    s.dir, _ = filepath.Split(file.Name());
    s.src = src;
    s.err = err;
    s.mode = mode;

    s.ch = ' ';
    s.offset = 0;
    s.rdOffset = 0;
    s.lineOffset = 0;
    s.insertSemi = false;
    s.ErrorCount = 0;

    s.next();
    if (s.ch == bom) {
        s.next(); // ignore BOM at file beginning
    }
});

private static void error(this ptr<Scanner> _addr_s, nint offs, @string msg) {
    ref Scanner s = ref _addr_s.val;

    if (s.err != null) {
        s.err(s.file.Position(s.file.Pos(offs)), msg);
    }
    s.ErrorCount++;
}

private static void errorf(this ptr<Scanner> _addr_s, nint offs, @string format, params object[] args) {
    args = args.Clone();
    ref Scanner s = ref _addr_s.val;

    s.error(offs, fmt.Sprintf(format, args));
}

private static @string scanComment(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;
 
    // initial '/' already consumed; s.ch == '/' || s.ch == '*'
    var offs = s.offset - 1; // position of initial '/'
    nint next = -1; // position immediately following the comment; < 0 means invalid comment
    nint numCR = 0;

    if (s.ch == '/') { 
        //-style comment
        // (the final '\n' is not considered part of the comment)
        s.next();
        while (s.ch != '\n' && s.ch >= 0) {
            if (s.ch == '\r') {
                numCR++;
            }
            s.next();
        } 
        // if we are at '\n', the position following the comment is afterwards
        next = s.offset;
        if (s.ch == '\n') {
            next++;
        }
        goto exit;
    }
    s.next();
    while (s.ch >= 0) {
        var ch = s.ch;
        if (ch == '\r') {
            numCR++;
        }
        s.next();
        if (ch == '*' && s.ch == '/') {
            s.next();
            next = s.offset;
            goto exit;
        }
    }

    s.error(offs, "comment not terminated");

exit: 

    // On Windows, a (//-comment) line may end in "\r\n".
    // Remove the final '\r' before analyzing the text for
    // line directives (matching the compiler). Remove any
    // other '\r' afterwards (matching the pre-existing be-
    // havior of the scanner).
    var lit = s.src[(int)offs..(int)s.offset]; 

    // On Windows, a (//-comment) line may end in "\r\n".
    // Remove the final '\r' before analyzing the text for
    // line directives (matching the compiler). Remove any
    // other '\r' afterwards (matching the pre-existing be-
    // havior of the scanner).
    if (numCR > 0 && len(lit) >= 2 && lit[1] == '/' && lit[len(lit) - 1] == '\r') {
        lit = lit[..(int)len(lit) - 1];
        numCR--;
    }
    if (next >= 0 && (lit[1] == '*' || offs == s.lineOffset) && bytes.HasPrefix(lit[(int)2..], prefix)) {
        s.updateLineInfo(next, offs, lit);
    }
    if (numCR > 0) {
        lit = stripCR(lit, lit[1] == '*');
    }
    return string(lit);
}

private static slice<byte> prefix = (slice<byte>)"line ";

// updateLineInfo parses the incoming comment text at offset offs
// as a line directive. If successful, it updates the line info table
// for the position next per the line directive.
private static void updateLineInfo(this ptr<Scanner> _addr_s, nint next, nint offs, slice<byte> text) {
    ref Scanner s = ref _addr_s.val;
 
    // extract comment text
    if (text[1] == '*') {
        text = text[..(int)len(text) - 2]; // lop off trailing "*/"
    }
    text = text[(int)7..]; // lop off leading "//line " or "/*line "
    offs += 7;

    var (i, n, ok) = trailingDigits(text);
    if (i == 0) {
        return ; // ignore (not a line directive)
    }
    if (!ok) { 
        // text has a suffix :xxx but xxx is not a number
        s.error(offs + i, "invalid line number: " + string(text[(int)i..]));
        return ;
    }
    nint line = default;    nint col = default;

    var (i2, n2, ok2) = trailingDigits(text[..(int)i - 1]);
    if (ok2) { 
        //line filename:line:col
        (i, i2) = (i2, i);        (line, col) = (n2, n);        if (col == 0) {
            s.error(offs + i2, "invalid column number: " + string(text[(int)i2..]));
            return ;
        }
        text = text[..(int)i2 - 1]; // lop off ":col"
    }
    else
 { 
        //line filename:line
        line = n;
    }
    if (line == 0) {
        s.error(offs + i, "invalid line number: " + string(text[(int)i..]));
        return ;
    }
    var filename = string(text[..(int)i - 1]); // lop off ":line", and trim white space
    if (filename == "" && ok2) {
        filename = s.file.Position(s.file.Pos(offs)).Filename;
    }
    else if (filename != "") { 
        // Put a relative filename in the current directory.
        // This is for compatibility with earlier releases.
        // See issue 26671.
        filename = filepath.Clean(filename);
        if (!filepath.IsAbs(filename)) {
            filename = filepath.Join(s.dir, filename);
        }
    }
    s.file.AddLineColumnInfo(next, filename, line, col);
}

private static (nint, nint, bool) trailingDigits(slice<byte> text) {
    nint _p0 = default;
    nint _p0 = default;
    bool _p0 = default;

    var i = bytes.LastIndexByte(text, ':'); // look from right (Windows filenames may contain ':')
    if (i < 0) {
        return (0, 0, false); // no ":"
    }
    var (n, err) = strconv.ParseUint(string(text[(int)i + 1..]), 10, 0);
    return (i + 1, int(n), err == null);
}

private static bool findLineEnd(this ptr<Scanner> _addr_s) => func((defer, _, _) => {
    ref Scanner s = ref _addr_s.val;
 
    // initial '/' already consumed

    defer(offs => { 
        // reset scanner state to where it was upon calling findLineEnd
        s.ch = '/';
        s.offset = offs;
        s.rdOffset = offs + 1;
        s.next(); // consume initial '/' again
    }(s.offset - 1)); 

    // read ahead until a newline, EOF, or non-comment token is found
    while (s.ch == '/' || s.ch == '*') {
        if (s.ch == '/') { 
            //-style comment always contains a newline
            return true;
        }
        s.next();
        while (s.ch >= 0) {
            var ch = s.ch;
            if (ch == '\n') {
                return true;
            }
            s.next();
            if (ch == '*' && s.ch == '/') {
                s.next();
                break;
            }
        }
        s.skipWhitespace(); // s.insertSemi is set
        if (s.ch < 0 || s.ch == '\n') {
            return true;
        }
        if (s.ch != '/') { 
            // non-comment token
            return false;
        }
        s.next(); // consume '/'
    }

    return false;
});

private static bool isLetter(int ch) {
    return 'a' <= lower(ch) && lower(ch) <= 'z' || ch == '_' || ch >= utf8.RuneSelf && unicode.IsLetter(ch);
}

private static bool isDigit(int ch) {
    return isDecimal(ch) || ch >= utf8.RuneSelf && unicode.IsDigit(ch);
}

// scanIdentifier reads the string of valid identifier characters at s.offset.
// It must only be called when s.ch is known to be a valid letter.
//
// Be careful when making changes to this function: it is optimized and affects
// scanning performance significantly.
private static @string scanIdentifier(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    var offs = s.offset; 

    // Optimize for the common case of an ASCII identifier.
    //
    // Ranging over s.src[s.rdOffset:] lets us avoid some bounds checks, and
    // avoids conversions to runes.
    //
    // In case we encounter a non-ASCII character, fall back on the slower path
    // of calling into s.next().
    foreach (var (rdOffset, b) in s.src[(int)s.rdOffset..]) {
        if ('a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || b == '_' || '0' <= b && b <= '9') { 
            // Avoid assigning a rune for the common case of an ascii character.
            continue;
        }
        s.rdOffset += rdOffset;
        if (0 < b && b < utf8.RuneSelf) { 
            // Optimization: we've encountered an ASCII character that's not a letter
            // or number. Avoid the call into s.next() and corresponding set up.
            //
            // Note that s.next() does some line accounting if s.ch is '\n', so this
            // shortcut is only possible because we know that the preceding character
            // is not '\n'.
            s.ch = rune(b);
            s.offset = s.rdOffset;
            s.rdOffset++;
            goto exit;
        }
        s.next();
        while (isLetter(s.ch) || isDigit(s.ch)) {
            s.next();
        }
        goto exit;
    }    s.offset = len(s.src);
    s.rdOffset = len(s.src);
    s.ch = eof;

exit:
    return string(s.src[(int)offs..(int)s.offset]);
}

private static nint digitVal(int ch) {

    if ('0' <= ch && ch <= '9') 
        return int(ch - '0');
    else if ('a' <= lower(ch) && lower(ch) <= 'f') 
        return int(lower(ch) - 'a' + 10);
        return 16; // larger than any legal digit val
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

// digits accepts the sequence { digit | '_' }.
// If base <= 10, digits accepts any decimal digit but records
// the offset (relative to the source start) of a digit >= base
// in *invalid, if *invalid < 0.
// digits returns a bitset describing whether the sequence contained
// digits (bit 0 is set), or separators '_' (bit 1 is set).
private static nint digits(this ptr<Scanner> _addr_s, nint @base, ptr<nint> _addr_invalid) {
    nint digsep = default;
    ref Scanner s = ref _addr_s.val;
    ref nint invalid = ref _addr_invalid.val;

    if (base <= 10) {
        var max = rune('0' + base);
        while (isDecimal(s.ch) || s.ch == '_') {
            nint ds = 1;
            if (s.ch == '_') {
                ds = 2;
            }
            else if (s.ch >= max && invalid < 0.val) {
                invalid = s.offset; // record invalid rune offset
            }
            digsep |= ds;
            s.next();
        }
    else
    } {
        while (isHex(s.ch) || s.ch == '_') {
            ds = 1;
            if (s.ch == '_') {
                ds = 2;
            }
            digsep |= ds;
            s.next();
        }
    }
    return ;
}

private static (token.Token, @string) scanNumber(this ptr<Scanner> _addr_s) {
    token.Token _p0 = default;
    @string _p0 = default;
    ref Scanner s = ref _addr_s.val;

    var offs = s.offset;
    var tok = token.ILLEGAL;

    nint @base = 10; // number base
    var prefix = rune(0); // one of 0 (decimal), '0' (0-octal), 'x', 'o', or 'b'
    nint digsep = 0; // bit 0: digit present, bit 1: '_' present
    ref nint invalid = ref heap(-1, out ptr<nint> _addr_invalid); // index of invalid digit in literal, or < 0

    // integer part
    if (s.ch != '.') {
        tok = token.INT;
        if (s.ch == '0') {
            s.next();
            switch (lower(s.ch)) {
                case 'x': 
                    s.next();
                    (base, prefix) = (16, 'x');
                    break;
                case 'o': 
                    s.next();
                    (base, prefix) = (8, 'o');
                    break;
                case 'b': 
                    s.next();
                    (base, prefix) = (2, 'b');
                    break;
                default: 
                    (base, prefix) = (8, '0');                digsep = 1; // leading 0
                    break;
            }
        }
        digsep |= s.digits(base, _addr_invalid);
    }
    if (s.ch == '.') {
        tok = token.FLOAT;
        if (prefix == 'o' || prefix == 'b') {
            s.error(s.offset, "invalid radix point in " + litname(prefix));
        }
        s.next();
        digsep |= s.digits(base, _addr_invalid);
    }
    if (digsep & 1 == 0) {
        s.error(s.offset, litname(prefix) + " has no digits");
    }
    {
        var e = lower(s.ch);

        if (e == 'e' || e == 'p') {

            if (e == 'e' && prefix != 0 && prefix != '0') 
                s.errorf(s.offset, "%q exponent requires decimal mantissa", s.ch);
            else if (e == 'p' && prefix != 'x') 
                s.errorf(s.offset, "%q exponent requires hexadecimal mantissa", s.ch);
                        s.next();
            tok = token.FLOAT;
            if (s.ch == '+' || s.ch == '-') {
                s.next();
            }
            var ds = s.digits(10, null);
            digsep |= ds;
            if (ds & 1 == 0) {
                s.error(s.offset, "exponent has no digits");
            }
        }
        else if (prefix == 'x' && tok == token.FLOAT) {
            s.error(s.offset, "hexadecimal mantissa requires a 'p' exponent");
        }

    } 

    // suffix 'i'
    if (s.ch == 'i') {
        tok = token.IMAG;
        s.next();
    }
    var lit = string(s.src[(int)offs..(int)s.offset]);
    if (tok == token.INT && invalid >= 0) {
        s.errorf(invalid, "invalid digit %q in %s", lit[invalid - offs], litname(prefix));
    }
    if (digsep & 2 != 0) {
        {
            var i = invalidSep(lit);

            if (i >= 0) {
                s.error(offs + i, "'_' must separate successive digits");
            }

        }
    }
    return (tok, lit);
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
    }
    return "decimal literal";
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

// scanEscape parses an escape sequence where rune is the accepted
// escaped quote. In case of a syntax error, it stops at the offending
// character (without consuming it) and returns false. Otherwise
// it returns true.
private static bool scanEscape(this ptr<Scanner> _addr_s, int quote) {
    ref Scanner s = ref _addr_s.val;

    var offs = s.offset;

    nint n = default;
    uint @base = default;    uint max = default;


    if (s.ch == 'a' || s.ch == 'b' || s.ch == 'f' || s.ch == 'n' || s.ch == 'r' || s.ch == 't' || s.ch == 'v' || s.ch == '\\' || s.ch == quote) 
        s.next();
        return true;
    else if (s.ch == '0' || s.ch == '1' || s.ch == '2' || s.ch == '3' || s.ch == '4' || s.ch == '5' || s.ch == '6' || s.ch == '7') 
        (n, base, max) = (3, 8, 255);    else if (s.ch == 'x') 
        s.next();
        (n, base, max) = (2, 16, 255);    else if (s.ch == 'u') 
        s.next();
        (n, base, max) = (4, 16, unicode.MaxRune);    else if (s.ch == 'U') 
        s.next();
        (n, base, max) = (8, 16, unicode.MaxRune);    else 
        @string msg = "unknown escape sequence";
        if (s.ch < 0) {
            msg = "escape sequence not terminated";
        }
        s.error(offs, msg);
        return false;
        uint x = default;
    while (n > 0) {
        var d = uint32(digitVal(s.ch));
        if (d >= base) {
            msg = fmt.Sprintf("illegal character %#U in escape sequence", s.ch);
            if (s.ch < 0) {
                msg = "escape sequence not terminated";
            }
            s.error(s.offset, msg);
            return false;
        }
        x = x * base + d;
        s.next();
        n--;
    }

    if (x > max || 0xD800 <= x && x < 0xE000) {
        s.error(offs, "escape sequence is invalid Unicode code point");
        return false;
    }
    return true;
}

private static @string scanRune(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;
 
    // '\'' opening already consumed
    var offs = s.offset - 1;

    var valid = true;
    nint n = 0;
    while (true) {
        var ch = s.ch;
        if (ch == '\n' || ch < 0) { 
            // only report error if we don't have one already
            if (valid) {
                s.error(offs, "rune literal not terminated");
                valid = false;
            }
            break;
        }
        s.next();
        if (ch == '\'') {
            break;
        }
        n++;
        if (ch == '\\') {
            if (!s.scanEscape('\'')) {
                valid = false;
            } 
            // continue to read to closing quote
        }
    }

    if (valid && n != 1) {
        s.error(offs, "illegal rune literal");
    }
    return string(s.src[(int)offs..(int)s.offset]);
}

private static @string scanString(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;
 
    // '"' opening already consumed
    var offs = s.offset - 1;

    while (true) {
        var ch = s.ch;
        if (ch == '\n' || ch < 0) {
            s.error(offs, "string literal not terminated");
            break;
        }
        s.next();
        if (ch == '"') {
            break;
        }
        if (ch == '\\') {
            s.scanEscape('"');
        }
    }

    return string(s.src[(int)offs..(int)s.offset]);
}

private static slice<byte> stripCR(slice<byte> b, bool comment) {
    var c = make_slice<byte>(len(b));
    nint i = 0;
    foreach (var (j, ch) in b) { 
        // In a /*-style comment, don't strip \r from *\r/ (incl.
        // sequences of \r from *\r\r...\r/) since the resulting
        // */ would terminate the comment too early unless the \r
        // is immediately following the opening /* in which case
        // it's ok because /*/ is not closed yet (issue #11151).
        if (ch != '\r' || comment && i > len("/*") && c[i - 1] == '*' && j + 1 < len(b) && b[j + 1] == '/') {
            c[i] = ch;
            i++;
        }
    }    return c[..(int)i];
}

private static @string scanRawString(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;
 
    // '`' opening already consumed
    var offs = s.offset - 1;

    var hasCR = false;
    while (true) {
        var ch = s.ch;
        if (ch < 0) {
            s.error(offs, "raw string literal not terminated");
            break;
        }
        s.next();
        if (ch == '`') {
            break;
        }
        if (ch == '\r') {
            hasCR = true;
        }
    }

    var lit = s.src[(int)offs..(int)s.offset];
    if (hasCR) {
        lit = stripCR(lit, false);
    }
    return string(lit);
}

private static void skipWhitespace(this ptr<Scanner> _addr_s) {
    ref Scanner s = ref _addr_s.val;

    while (s.ch == ' ' || s.ch == '\t' || s.ch == '\n' && !s.insertSemi || s.ch == '\r') {
        s.next();
    }
}

// Helper functions for scanning multi-byte tokens such as >> += >>= .
// Different routines recognize different length tok_i based on matches
// of ch_i. If a token ends in '=', the result is tok1 or tok3
// respectively. Otherwise, the result is tok0 if there was no other
// matching character, or tok2 if the matching character was ch2.

private static token.Token switch2(this ptr<Scanner> _addr_s, token.Token tok0, token.Token tok1) {
    ref Scanner s = ref _addr_s.val;

    if (s.ch == '=') {
        s.next();
        return tok1;
    }
    return tok0;
}

private static token.Token switch3(this ptr<Scanner> _addr_s, token.Token tok0, token.Token tok1, int ch2, token.Token tok2) {
    ref Scanner s = ref _addr_s.val;

    if (s.ch == '=') {
        s.next();
        return tok1;
    }
    if (s.ch == ch2) {
        s.next();
        return tok2;
    }
    return tok0;
}

private static token.Token switch4(this ptr<Scanner> _addr_s, token.Token tok0, token.Token tok1, int ch2, token.Token tok2, token.Token tok3) {
    ref Scanner s = ref _addr_s.val;

    if (s.ch == '=') {
        s.next();
        return tok1;
    }
    if (s.ch == ch2) {
        s.next();
        if (s.ch == '=') {
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
private static (token.Pos, token.Token, @string) Scan(this ptr<Scanner> _addr_s) {
    token.Pos pos = default;
    token.Token tok = default;
    @string lit = default;
    ref Scanner s = ref _addr_s.val;

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
            if (len(lit) > 1) { 
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
        else if (isDecimal(ch) || ch == '.' && isDecimal(rune(s.peek()))) 
            insertSemi = true;
            tok, lit = s.scanNumber();
        else 
            s.next(); // always make progress
            switch (ch) {
                case -1: 
                    if (s.insertSemi) {
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
                    // fractions starting with a '.' are handled by outer switch
                    tok = token.PERIOD;
                    if (s.ch == '.' && s.peek() == '.') {
                        s.next();
                        s.next(); // consume last '.'
                        tok = token.ELLIPSIS;
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
                    if (tok == token.INC) {
                        insertSemi = true;
                    }
                    break;
                case '-': 
                    tok = s.switch3(token.SUB, token.SUB_ASSIGN, '-', token.DEC);
                    if (tok == token.DEC) {
                        insertSemi = true;
                    }
                    break;
                case '*': 
                    tok = s.switch2(token.MUL, token.MUL_ASSIGN);
                    break;
                case '/': 
                                   if (s.ch == '/' || s.ch == '*') { 
                                       // comment
                                       if (s.insertSemi && s.findLineEnd()) { 
                                           // reset position to the beginning of the comment
                                           s.ch = '/';
                                           s.offset = s.file.Offset(pos);
                                           s.rdOffset = s.offset + 1;
                                           s.insertSemi = false; // newline consumed
                                           return (pos, token.SEMICOLON, "\n");
                                       }
                                       var comment = s.scanComment();
                                       if (s.mode & ScanComments == 0) { 
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
                                   if (s.ch == '-') {
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
                                   if (s.ch == '^') {
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
                    if (ch != bom) {
                        s.errorf(s.file.Offset(pos), "illegal character %#U", ch);
                    }
                    insertSemi = s.insertSemi; // preserve insertSemi info
                    tok = token.ILLEGAL;
                    lit = string(ch);
                    break;
            }

    }
    if (s.mode & dontInsertSemis == 0) {
        s.insertSemi = insertSemi;
    }
    return ;
}

} // end scanner_package
