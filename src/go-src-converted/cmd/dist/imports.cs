// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is forked from go/build/read.go.
// (cmd/dist must not import go/build because we do not want it to be
// sensitive to the specific version of go/build present in $GOROOT_BOOTSTRAP.)

// package main -- go2cs converted at 2022 March 06 23:15:22 UTC
// Original source: C:\Program Files\Go\src\cmd\dist\imports.go
using bufio = go.bufio_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

namespace go;

public static partial class main_package {

private partial struct importReader {
    public ptr<bufio.Reader> b;
    public slice<byte> buf;
    public byte peek;
    public error err;
    public bool eof;
    public nint nerr;
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

// readComments is like ioutil.ReadAll, except that it only reads the leading
// block of comments in the file.
private static (slice<byte>, error) readComments(io.Reader f) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    ptr<importReader> r = addr(new importReader(b:bufio.NewReader(f)));
    r.peekByte(true);
    if (r.err == null && !r.eof) { 
        // Didn't reach EOF, so must have found a non-space byte. Remove it.
        r.buf = r.buf[..(int)len(r.buf) - 1];

    }
    return (r.buf, error.As(r.err)!);

}

// readimports returns the imports found in the named file.
private static slice<@string> readimports(@string file) {
    ref slice<@string> imports = ref heap(out ptr<slice<@string>> _addr_imports);
    ptr<importReader> r = addr(new importReader(b:bufio.NewReader(strings.NewReader(readfile(file)))));
    r.readKeyword("package");
    r.readIdent();
    while (r.peekByte(true) == 'i') {
        r.readKeyword("import");
        if (r.peekByte(true) == '(') {
            r.nextByte(false);
            while (r.peekByte(true) != ')' && r.err == null) {
                r.readImport(_addr_imports);
            }
        else

            r.nextByte(false);

        } {
            r.readImport(_addr_imports);
        }
    }

    foreach (var (i) in imports) {
        var (unquoted, err) = strconv.Unquote(imports[i]);
        if (err != null) {
            fatalf("reading imports from %s: %v", file, err);
        }
        imports[i] = unquoted;

    }    return imports;

}

// resolveVendor returns a unique package path imported with the given import
// path from srcDir.
//
// resolveVendor assumes that a package is vendored if and only if its first
// path component contains a dot. If a package is vendored, its import path
// is returned with a "vendor" or "cmd/vendor" prefix, depending on srcDir.
// Otherwise, the import path is returned verbatim.
private static @string resolveVendor(@string imp, @string srcDir) => func((_, panic, _) => {
    @string first = default;
    {
        var i = strings.Index(imp, "/");

        if (i < 0) {
            first = imp;
        }
        else
 {
            first = imp[..(int)i];
        }
    }

    var isStandard = !strings.Contains(first, ".");
    if (isStandard) {
        return imp;
    }
    if (strings.HasPrefix(srcDir, filepath.Join(goroot, "src", "cmd"))) {
        return path.Join("cmd", "vendor", imp);
    }
    else if (strings.HasPrefix(srcDir, filepath.Join(goroot, "src"))) {
        return path.Join("vendor", imp);
    }
    else
 {
        panic(fmt.Sprintf("srcDir %q not in GOOROT/src", srcDir));
    }
});

} // end main_package
