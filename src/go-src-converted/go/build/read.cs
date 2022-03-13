// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package build -- go2cs converted at 2022 March 13 05:54:04 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Program Files\Go\src\go\build\read.go
namespace go.go;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
using token = go.token_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class build_package {

private partial struct importReader {
    public ptr<bufio.Reader> b;
    public slice<byte> buf;
    public byte peek;
    public error err;
    public bool eof;
    public nint nerr;
    public token.Position pos;
}

private static byte bom = new slice<byte>(new byte[] { 0xef, 0xbb, 0xbf });

private static ptr<importReader> newImportReader(@string name, io.Reader r) {
    var b = bufio.NewReader(r); 
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
    return addr(new importReader(b:b,pos:token.Position{Filename:name,Line:1,Column:1,},));
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

// readByteNoBuf is like readByte but doesn't buffer the byte.
// It exhausts r.buf before reading from r.b.
private static byte readByteNoBuf(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    byte c = default;
    error err = default!;
    if (len(r.buf) > 0) {
        c = r.buf[0];
        r.buf = r.buf[(int)1..];
    }
    else
 {
        c, err = r.b.ReadByte();
        if (err == null && c == 0) {
            err = error.As(errNUL)!;
        }
    }
    if (err != null) {
        if (err == io.EOF) {
            r.eof = true;
        }
        else if (r.err == null) {
            r.err = err;
        }
        return 0;
    }
    r.pos.Offset++;
    if (c == '\n') {
        r.pos.Line++;
        r.pos.Column = 1;
    }
    else
 {
        r.pos.Column++;
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

private static slice<byte> goEmbed = (slice<byte>)"go:embed";

// findEmbed advances the input reader to the next //go:embed comment.
// It reports whether it found a comment.
// (Otherwise it found an error or EOF.)
private static bool findEmbed(this ptr<importReader> _addr_r, bool first) {
    ref importReader r = ref _addr_r.val;
 
    // The import block scan stopped after a non-space character,
    // so the reader is not at the start of a line on the first call.
    // After that, each //go:embed extraction leaves the reader
    // at the end of a line.
    var startLine = !first;
    byte c = default;
    while (r.err == null && !r.eof) {
        c = r.readByteNoBuf();
Reswitch:
        switch (c) {
            case '\n': 
                startLine = true;
                break;
            case ' ': 

            case '\t': 

                break;
            case '"': 
                startLine = false;
                while (r.err == null) {
                    if (r.eof) {
                        r.syntaxError();
                    }
                    c = r.readByteNoBuf();
                    if (c == '\\') {
                        r.readByteNoBuf();
                        if (r.err != null) {
                            r.syntaxError();
                            return false;
                        }
                        continue;
                    }
                    if (c == '"') {
                        c = r.readByteNoBuf();
                        goto Reswitch;
                    }
                }

                goto Reswitch;
                break;
            case '`': 
                startLine = false;
                while (r.err == null) {
                    if (r.eof) {
                        r.syntaxError();
                    }
                    c = r.readByteNoBuf();
                    if (c == '`') {
                        c = r.readByteNoBuf();
                        goto Reswitch;
                    }
                }

                break;
            case '/': 
                c = r.readByteNoBuf();
                switch (c) {
                    case '*': 
                        byte c1 = default;
                        while ((c != '*' || c1 != '/') && r.err == null) {
                            if (r.eof) {
                                r.syntaxError();
                            }
                            (c, c1) = (c1, r.readByteNoBuf());
                        }

                        startLine = false;
                        break;
                    case '/': 
                                        if (startLine) { 
                                            // Try to read this as a //go:embed comment.
                                            foreach (var (i) in goEmbed) {
                                                c = r.readByteNoBuf();
                                                if (c != goEmbed[i]) {
                                                    goto SkipSlashSlash;
                                                }
                                            }
                                            c = r.readByteNoBuf();
                                            if (c == ' ' || c == '\t') { 
                                                // Found one!
                                                return true;
                                            }
                                        }
                        SkipSlashSlash:
                                        while (c != '\n' && r.err == null && !r.eof) {
                                            c = r.readByteNoBuf();
                                        }
                                        startLine = true;
                        break;
                    default: 
                        startLine = false;
                        goto Reswitch;
                        break;
                }
                break;
            default: 
                startLine = false;
                break;
        }
    }
    return false;
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
private static void readString(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    switch (r.nextByte(true)) {
        case '`': 
            while (r.err == null) {
                if (r.nextByte(false) == '`') {
                    break;
                }
                if (r.eof) {
                    r.syntaxError();
                }
            }
            break;
        case '"': 
            while (r.err == null) {
                var c = r.nextByte(false);
                if (c == '"') {
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
private static void readImport(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var c = r.peekByte(true);
    if (c == '.') {
        r.peek = 0;
    }
    else if (isIdent(c)) {
        r.readIdent();
    }
    r.readString();
}

// readComments is like io.ReadAll, except that it only reads the leading
// block of comments in the file.
private static (slice<byte>, error) readComments(io.Reader f) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var r = newImportReader("", f);
    r.peekByte(true);
    if (r.err == null && !r.eof) { 
        // Didn't reach EOF, so must have found a non-space byte. Remove it.
        r.buf = r.buf[..(int)len(r.buf) - 1];
    }
    return (r.buf, error.As(r.err)!);
}

// readGoInfo expects a Go file as input and reads the file up to and including the import section.
// It records what it learned in *info.
// If info.fset is non-nil, readGoInfo parses the file and sets info.parsed, info.parseErr,
// info.imports, info.embeds, and info.embedErr.
//
// It only returns an error if there are problems reading the file,
// not for syntax errors in the file itself.
private static error readGoInfo(io.Reader f, ptr<fileInfo> _addr_info) {
    ref fileInfo info = ref _addr_info.val;

    var r = newImportReader(info.name, f);

    r.readKeyword("package");
    r.readIdent();
    while (r.peekByte(true) == 'i') {
        r.readKeyword("import");
        if (r.peekByte(true) == '(') {
            r.nextByte(false);
            while (r.peekByte(true) != ')' && r.err == null) {
                r.readImport();
            }
        else

            r.nextByte(false);
        } {
            r.readImport();
        }
    }

    info.header = r.buf; 

    // If we stopped successfully before EOF, we read a byte that told us we were done.
    // Return all but that last byte, which would cause a syntax error if we let it through.
    if (r.err == null && !r.eof) {
        info.header = r.buf[..(int)len(r.buf) - 1];
    }
    if (r.err == errSyntax) {
        r.err = null;
        while (r.err == null && !r.eof) {
            r.readByte();
        }
        info.header = r.buf;
    }
    if (r.err != null) {
        return error.As(r.err)!;
    }
    if (info.fset == null) {
        return error.As(null!)!;
    }
    info.parsed, info.parseErr = parser.ParseFile(info.fset, info.name, info.header, parser.ImportsOnly | parser.ParseComments);
    if (info.parseErr != null) {
        return error.As(null!)!;
    }
    var hasEmbed = false;
    foreach (var (_, decl) in info.parsed.Decls) {
        ptr<ast.GenDecl> (d, ok) = decl._<ptr<ast.GenDecl>>();
        if (!ok) {
            continue;
        }
        foreach (var (_, dspec) in d.Specs) {
            ptr<ast.ImportSpec> (spec, ok) = dspec._<ptr<ast.ImportSpec>>();
            if (!ok) {
                continue;
            }
            var quoted = spec.Path.Value;
            var (path, err) = strconv.Unquote(quoted);
            if (err != null) {
                return error.As(fmt.Errorf("parser returned invalid quoted string: <%s>", quoted))!;
            }
            if (path == "embed") {
                hasEmbed = true;
            }
            var doc = spec.Doc;
            if (doc == null && len(d.Specs) == 1) {
                doc = d.Doc;
            }
            info.imports = append(info.imports, new fileImport(path,spec.Pos(),doc));
        }
    }    if (hasEmbed) {
        slice<byte> line = default;
        {
            var first = true;

            while (r.findEmbed(first)) {
                line = line[..(int)0];
                var pos = r.pos;
                while (true) {
                    var c = r.readByteNoBuf();
                    if (c == '\n' || r.err != null || r.eof) {
                        break;
                first = false;
                    }
                    line = append(line, c);
                } 
                // Add args if line is well-formed.
                // Ignore badly-formed lines - the compiler will report them when it finds them,
                // and we can pretend they are not there to help go list succeed with what it knows.
 
                // Add args if line is well-formed.
                // Ignore badly-formed lines - the compiler will report them when it finds them,
                // and we can pretend they are not there to help go list succeed with what it knows.
                var (embs, err) = parseGoEmbed(string(line), pos);
                if (err == null) {
                    info.embeds = append(info.embeds, embs);
                }
            }

        }
    }
    return error.As(null!)!;
}

// parseGoEmbed parses the text following "//go:embed" to extract the glob patterns.
// It accepts unquoted space-separated patterns as well as double-quoted and back-quoted Go strings.
// This is based on a similar function in cmd/compile/internal/gc/noder.go;
// this version calculates position information as well.
private static (slice<fileEmbed>, error) parseGoEmbed(@string args, token.Position pos) {
    slice<fileEmbed> _p0 = default;
    error _p0 = default!;

    Action<nint> trimBytes = n => {
        pos.Offset += n;
        pos.Column += utf8.RuneCountInString(args[..(int)n]);
        args = args[(int)n..];
    };
    Action trimSpace = () => {
        var trim = strings.TrimLeftFunc(args, unicode.IsSpace);
        trimBytes(len(args) - len(trim));
    };

    slice<fileEmbed> list = default;
    trimSpace();

    while (args != "") {
        @string path = default;
        var pathPos = pos;
Switch:

        switch (args[0]) {
            case '`': 
                i = strings.Index(args[(int)1..], "`");
                if (i < 0) {
                    return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args))!);
                }
                path = args[(int)1..(int)1 + i];
                trimBytes(1 + i + 1);
                break;
            case '"': 
                i = 1;
                while (i < len(args)) {
                    if (args[i] == '\\') {
                        i++;
                        continue;
                    i++;
                    }
                    if (args[i] == '"') {
                        var (q, err) = strconv.Unquote(args[..(int)i + 1]);
                        if (err != null) {
                            return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args[..(int)i + 1]))!);
                        }
                        path = q;
                        trimBytes(i + 1);
                        _breakSwitch = true;
                        break;
                    }
                }

                if (i >= len(args)) {
                    return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args))!);
                }
                break;
            default: 
                        var i = len(args);
                        foreach (var (j, c) in args) {
                            if (unicode.IsSpace(c)) {
                                i = j;
                                break;
                            }
                    trimSpace();
                        }
                        path = args[..(int)i];
                        trimBytes(i);
                break;
        }
        if (args != "") {
            var (r, _) = utf8.DecodeRuneInString(args);
            if (!unicode.IsSpace(r)) {
                return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args))!);
            }
        }
        list = append(list, new fileEmbed(path,pathPos));
    }
    return (list, error.As(null!)!);
}

} // end build_package
