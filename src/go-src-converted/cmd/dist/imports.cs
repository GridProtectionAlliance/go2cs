// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is forked from go/build/read.go.
// (cmd/dist must not import go/build because we do not want it to be
// sensitive to the specific version of go/build present in $GOROOT_BOOTSTRAP.)

// package main -- go2cs converted at 2020 August 29 09:59:44 UTC
// Original source: C:\Go\src\cmd\dist\imports.go
using bufio = go.bufio_package;
using errors = go.errors_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private partial struct importReader
        {
            public ptr<bufio.Reader> b;
            public slice<byte> buf;
            public byte peek;
            public error err;
            public bool eof;
            public long nerr;
        }

        private static bool isIdent(byte c)
        {
            return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_' || c >= utf8.RuneSelf;
        }

        private static var errSyntax = errors.New("syntax error");        private static var errNUL = errors.New("unexpected NUL in input");

        // syntaxError records a syntax error, but only if an I/O error has not already been recorded.
        private static void syntaxError(this ref importReader r)
        {
            if (r.err == null)
            {
                r.err = errSyntax;
            }
        }

        // readByte reads the next byte from the input, saves it in buf, and returns it.
        // If an error occurs, readByte records the error in r.err and returns 0.
        private static byte readByte(this ref importReader r)
        {
            var (c, err) = r.b.ReadByte();
            if (err == null)
            {
                r.buf = append(r.buf, c);
                if (c == 0L)
                {
                    err = errNUL;
                }
            }
            if (err != null)
            {
                if (err == io.EOF)
                {
                    r.eof = true;
                }
                else if (r.err == null)
                {
                    r.err = err;
                }
                c = 0L;
            }
            return c;
        }

        // peekByte returns the next byte from the input reader but does not advance beyond it.
        // If skipSpace is set, peekByte skips leading spaces and comments.
        private static byte peekByte(this ref importReader _r, bool skipSpace) => func(_r, (ref importReader r, Defer _, Panic panic, Recover __) =>
        {
            if (r.err != null)
            {
                r.nerr++;

                if (r.nerr > 10000L)
                {
                    panic("go/build: import reader looping");
                }
                return 0L;
            } 

            // Use r.peek as first input byte.
            // Don't just return r.peek here: it might have been left by peekByte(false)
            // and this might be peekByte(true).
            var c = r.peek;
            if (c == 0L)
            {
                c = r.readByte();
            }
            while (r.err == null && !r.eof)
            {
                if (skipSpace)
                { 
                    // For the purposes of this reader, semicolons are never necessary to
                    // understand the input and are treated as spaces.
                    switch (c)
                    {
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
                            if (c == '/')
                            {
                                while (c != '\n' && r.err == null && !r.eof)
                                {
                                    c = r.readByte();
                                }

                            }
                            else if (c == '*')
                            {
                                byte c1 = default;
                                while ((c != '*' || c1 != '/') && r.err == null)
                                {
                                    if (r.eof)
                                    {
                                        r.syntaxError();
                                    }
                                    c = c1;
                                    c1 = r.readByte();
                                }
                            else

                            }                        {
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
        private static byte nextByte(this ref importReader r, bool skipSpace)
        {
            var c = r.peekByte(skipSpace);
            r.peek = 0L;
            return c;
        }

        // readKeyword reads the given keyword from the input.
        // If the keyword is not present, readKeyword records a syntax error.
        private static void readKeyword(this ref importReader r, @string kw)
        {
            r.peekByte(true);
            for (long i = 0L; i < len(kw); i++)
            {
                if (r.nextByte(false) != kw[i])
                {
                    r.syntaxError();
                    return;
                }
            }

            if (isIdent(r.peekByte(false)))
            {
                r.syntaxError();
            }
        }

        // readIdent reads an identifier from the input.
        // If an identifier is not present, readIdent records a syntax error.
        private static void readIdent(this ref importReader r)
        {
            var c = r.peekByte(true);
            if (!isIdent(c))
            {
                r.syntaxError();
                return;
            }
            while (isIdent(r.peekByte(false)))
            {
                r.peek = 0L;
            }

        }

        // readString reads a quoted string literal from the input.
        // If an identifier is not present, readString records a syntax error.
        private static void readString(this ref importReader r, ref slice<@string> save)
        {
            switch (r.nextByte(true))
            {
                case '`': 
                    var start = len(r.buf) - 1L;
                    while (r.err == null)
                    {
                        if (r.nextByte(false) == '`')
                        {
                            if (save != null)
                            {
                                save.Value = append(save.Value, string(r.buf[start..]));
                            }
                            break;
                        }
                        if (r.eof)
                        {
                            r.syntaxError();
                        }
                    }
                    break;
                case '"': 
                    start = len(r.buf) - 1L;
                    while (r.err == null)
                    {
                        var c = r.nextByte(false);
                        if (c == '"')
                        {
                            if (save != null)
                            {
                                save.Value = append(save.Value, string(r.buf[start..]));
                            }
                            break;
                        }
                        if (r.eof || c == '\n')
                        {
                            r.syntaxError();
                        }
                        if (c == '\\')
                        {
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
        private static void readImport(this ref importReader r, ref slice<@string> imports)
        {
            var c = r.peekByte(true);
            if (c == '.')
            {
                r.peek = 0L;
            }
            else if (isIdent(c))
            {
                r.readIdent();
            }
            r.readString(imports);
        }

        // readComments is like ioutil.ReadAll, except that it only reads the leading
        // block of comments in the file.
        private static (slice<byte>, error) readComments(io.Reader f)
        {
            importReader r = ref new importReader(b:bufio.NewReader(f));
            r.peekByte(true);
            if (r.err == null && !r.eof)
            { 
                // Didn't reach EOF, so must have found a non-space byte. Remove it.
                r.buf = r.buf[..len(r.buf) - 1L];
            }
            return (r.buf, r.err);
        }

        // readimports returns the imports found in the named file.
        private static slice<@string> readimports(@string file)
        {
            slice<@string> imports = default;
            importReader r = ref new importReader(b:bufio.NewReader(strings.NewReader(readfile(file))));
            r.readKeyword("package");
            r.readIdent();
            while (r.peekByte(true) == 'i')
            {
                r.readKeyword("import");
                if (r.peekByte(true) == '(')
                {
                    r.nextByte(false);
                    while (r.peekByte(true) != ')' && r.err == null)
                    {
                        r.readImport(ref imports);
                    }
                else

                    r.nextByte(false);
                }                {
                    r.readImport(ref imports);
                }
            }


            foreach (var (i) in imports)
            {
                var (unquoted, err) = strconv.Unquote(imports[i]);
                if (err != null)
                {
                    fatalf("reading imports from %s: %v", file, err);
                }
                imports[i] = unquoted;
            }
            return imports;
        }
    }
}
