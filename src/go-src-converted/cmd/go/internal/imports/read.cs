// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Copied from Go distribution src/go/build/read.go.

// package imports -- go2cs converted at 2022 March 13 06:30:12 UTC
// import "cmd/go/internal/imports" ==> using imports = go.cmd.go.@internal.imports_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\imports\read.go
namespace go.cmd.go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using utf8 = unicode.utf8_package;

public static partial class imports_package {

private partial struct importReader {
    public ptr<bufio.Reader> b;
    public slice<byte> buf;
    public byte peek;
    public error err;
    public bool eof;
    public nint nerr;
}

private static byte bom = new slice<byte>(new byte[] { 0xef, 0xbb, 0xbf });

private static ptr<importReader> newImportReader(ptr<bufio.Reader> _addr_b) {
    ref bufio.Reader b = ref _addr_b.val;
 
    // Remove leading UTF-8 BOM.
    // Per https://golang.org/ref/spec#Source_code_representation:
    // a compiler may ignore a UTF-8-encoded byte order mark (U+FEFF)
    // if it is the first Unicode code point in the source text.
    {
        var (leadingBytes, err) = b.Peek(3);

        if (err == null && bytes.Equal(leadingBytes, bom)) {
            b.Discard(3);
        }
    }
    return addr(new importReader(b:b));
}

private static bool isIdent(byte c) {
    return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_' || c >= utf8.RuneSelf;
}

private static var errSyntax = errors.New("syntax error");private static var errNUL = errors.New("unexpected NUL in input");

// syntaxError records a syntax error, but only if an I/O error has not already been recorded.
private static void syntaxError(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    if (r.err == null) {
        r.err = errSyntax;
    }
}

// readByte reads the next byte from the input, saves it in buf, and returns it.
// If an error occurs, readByte records the error in r.err and returns 0.
private static byte readByte(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (c, err) = r.b.ReadByte();
    if (err == null) {
        r.buf = append(r.buf, c);
        if (c == 0) {
            err = errNUL;
        }
    }
    if (err != null) {
        if (err == io.EOF) {
            r.eof = true;
        }
        else if (r.err == null) {
            r.err = err;
        }
        c = 0;
    }
    return c;
}

// peekByte returns the next byte from the input reader but does not advance beyond it.
// If skipSpace is set, peekByte skips leading spaces and comments.
private static byte peekByte(this ptr<importReader> _addr_r, bool skipSpace) => func((_, panic, _) => {
    ref importReader r = ref _addr_r.val;

    if (r.err != null) {
        r.nerr++;

        if (r.nerr > 10000) {
            panic("go/build: import reader looping");
        }
        return 0;
    }
    var c = r.peek;
    if (c == 0) {
        c = r.readByte();
    }
    while (r.err == null && !r.eof) {
        if (skipSpace) { 
            // For the purposes of this reader, semicolons are never necessary to
            // understand the input and are treated as spaces.
            switch (c) {
                case ' ': 

                case '\f': 

                case '\t': 

                case '\r': 

                case '\n': 

                case ';': 
                    c = r.readByte();
                    continue;
                    break;
                case '/': 
                    c = r.readByte();
                    if (c == '/') {
                        while (c != '\n' && r.err == null && !r.eof) {
                            c = r.readByte();
                        }
                    }
                    else if (c == '*') {
                        byte c1 = default;
                        while ((c != '*' || c1 != '/') && r.err == null) {
                            if (r.eof) {
                                r.syntaxError();
                            }
                            (c, c1) = (c1, r.readByte());
                        }
                    else
                    } {
                        r.syntaxError();
                    }
                    c = r.readByte();
                    continue;
                    break;
            }
        }
        break;
    }
    r.peek = c;
    return r.peek;
});

// nextByte is like peekByte but advances beyond the returned byte.
private static byte nextByte(this ptr<importReader> _addr_r, bool skipSpace) {
    ref importReader r = ref _addr_r.val;

    var c = r.peekByte(skipSpace);
    r.peek = 0;
    return c;
}

// readKeyword reads the given keyword from the input.
// If the keyword is not present, readKeyword records a syntax error.
private static void readKeyword(this ptr<importReader> _addr_r, @string kw) {
    ref importReader r = ref _addr_r.val;

    r.peekByte(true);
    for (nint i = 0; i < len(kw); i++) {
        if (r.nextByte(false) != kw[i]) {
            r.syntaxError();
            return ;
        }
    }
    if (isIdent(r.peekByte(false))) {
        r.syntaxError();
    }
}

// readIdent reads an identifier from the input.
// If an identifier is not present, readIdent records a syntax error.
private static void readIdent(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var c = r.peekByte(true);
    if (!isIdent(c)) {
        r.syntaxError();
        return ;
    }
    while (isIdent(r.peekByte(false))) {
        r.peek = 0;
    }
}

// readString reads a quoted string literal from the input.
// If an identifier is not present, readString records a syntax error.
private static void readString(this ptr<importReader> _addr_r, ptr<slice<@string>> _addr_save) {
    ref importReader r = ref _addr_r.val;
    ref slice<@string> save = ref _addr_save.val;

    switch (r.nextByte(true)) {
        case '`': 
            var start = len(r.buf) - 1;
            while (r.err == null) {
                if (r.nextByte(false) == '`') {
                    if (save != null) {
                        save = append(save, string(r.buf[(int)start..]));
                    }
                    break;
                }
                if (r.eof) {
                    r.syntaxError();
                }
            }
            break;
        case '"': 
            start = len(r.buf) - 1;
            while (r.err == null) {
                var c = r.nextByte(false);
                if (c == '"') {
                    if (save != null) {
                        save = append(save, string(r.buf[(int)start..]));
                    }
                    break;
                }
                if (r.eof || c == '\n') {
                    r.syntaxError();
                }
                if (c == '\\') {
                    r.nextByte(false);
                }
            }
            break;
        default: 
            r.syntaxError();
            break;
    }
}

// readImport reads an import clause - optional identifier followed by quoted string -
// from the input.
private static void readImport(this ptr<importReader> _addr_r, ptr<slice<@string>> _addr_imports) {
    ref importReader r = ref _addr_r.val;
    ref slice<@string> imports = ref _addr_imports.val;

    var c = r.peekByte(true);
    if (c == '.') {
        r.peek = 0;
    }
    else if (isIdent(c)) {
        r.readIdent();
    }
    r.readString(imports);
}

// ReadComments is like io.ReadAll, except that it only reads the leading
// block of comments in the file.
public static (slice<byte>, error) ReadComments(io.Reader f) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var r = newImportReader(_addr_bufio.NewReader(f));
    r.peekByte(true);
    if (r.err == null && !r.eof) { 
        // Didn't reach EOF, so must have found a non-space byte. Remove it.
        r.buf = r.buf[..(int)len(r.buf) - 1];
    }
    return (r.buf, error.As(r.err)!);
}

// ReadImports is like io.ReadAll, except that it expects a Go file as input
// and stops reading the input once the imports have completed.
public static (slice<byte>, error) ReadImports(io.Reader f, bool reportSyntaxError, ptr<slice<@string>> _addr_imports) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref slice<@string> imports = ref _addr_imports.val;

    var r = newImportReader(_addr_bufio.NewReader(f));

    r.readKeyword("package");
    r.readIdent();
    while (r.peekByte(true) == 'i') {
        r.readKeyword("import");
        if (r.peekByte(true) == '(') {
            r.nextByte(false);
            while (r.peekByte(true) != ')' && r.err == null) {
                r.readImport(imports);
            }
        else

            r.nextByte(false);
        } {
            r.readImport(imports);
        }
    } 

    // If we stopped successfully before EOF, we read a byte that told us we were done.
    // Return all but that last byte, which would cause a syntax error if we let it through.
    if (r.err == null && !r.eof) {
        return (r.buf[..(int)len(r.buf) - 1], error.As(null!)!);
    }
    if (r.err == errSyntax && !reportSyntaxError) {
        r.err = null;
        while (r.err == null && !r.eof) {
            r.readByte();
        }
    }
    return (r.buf, error.As(r.err)!);
}

} // end imports_package
