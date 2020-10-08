// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package expect -- go2cs converted at 2020 October 08 04:55:53 UTC
// import "golang.org/x/tools/go/expect" ==> using expect = go.golang.org.x.tools.go.expect_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\expect\extract.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;

using modfile = go.golang.org.x.mod.modfile_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class expect_package
    {
        private static readonly @string commentStart = (@string)"@";

        private static readonly var commentStartLen = (var)len(commentStart);

        // Identifier is the type for an identifier in an Note argument list.


        // Identifier is the type for an identifier in an Note argument list.
        public partial struct Identifier // : @string
        {
        }

        // Parse collects all the notes present in a file.
        // If content is nil, the filename specified is read and parsed, otherwise the
        // content is used and the filename is used for positions and error messages.
        // Each comment whose text starts with @ is parsed as a comma-separated
        // sequence of notes.
        // See the package documentation for details about the syntax of those
        // notes.
        public static (slice<ptr<Note>>, error) Parse(ptr<token.FileSet> _addr_fset, @string filename, slice<byte> content)
        {
            slice<ptr<Note>> _p0 = default;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;

            var src = default;
            if (content != null)
            {
                src = content;
            }

            switch (filepath.Ext(filename))
            {
                case ".go": 
                    // TODO: We should write this in terms of the scanner.
                    // there are ways you can break the parser such that it will not add all the
                    // comments to the ast, which may result in files where the tests are silently
                    // not run.
                    var (file, err) = parser.ParseFile(fset, filename, src, parser.ParseComments);
                    if (file == null)
                    {
                        return (null, error.As(err)!);
                    }

                    return ExtractGo(_addr_fset, _addr_file);
                    break;
                case ".mod": 
                    (file, err) = modfile.Parse(filename, content, null);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    var f = fset.AddFile(filename, -1L, len(content));
                    f.SetLinesForContent(content);
                    var (notes, err) = extractMod(_addr_fset, _addr_file);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    } 
                    // Since modfile.Parse does not return an *ast, we need to add the offset
                    // within the file's contents to the file's base relative to the fileset.
                    foreach (var (_, note) in notes)
                    {
                        note.Pos += token.Pos(f.Base());
                    }
                    return (notes, error.As(null!)!);
                    break;
            }
            return (null, error.As(null!)!);

        }

        // extractMod collects all the notes present in a go.mod file.
        // Each comment whose text starts with @ is parsed as a comma-separated
        // sequence of notes.
        // See the package documentation for details about the syntax of those
        // notes.
        // Only allow notes to appear with the following format: "//@mark()" or // @mark()
        private static (slice<ptr<Note>>, error) extractMod(ptr<token.FileSet> _addr_fset, ptr<modfile.File> _addr_file)
        {
            slice<ptr<Note>> _p0 = default;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;
            ref modfile.File file = ref _addr_file.val;

            slice<ptr<Note>> notes = default;
            foreach (var (_, stmt) in file.Syntax.Stmt)
            {
                var comment = stmt.Comment();
                if (comment == null)
                {
                    continue;
                } 
                // Handle the case for markers of `// indirect` to be on the line before
                // the require statement.
                // TODO(golang/go#36894): have a more intuitive approach for // indirect
                {
                    var cmt__prev2 = cmt;

                    foreach (var (_, __cmt) in comment.Before)
                    {
                        cmt = __cmt;
                        var (text, adjust) = getAdjustedNote(cmt.Token);
                        if (text == "")
                        {
                            continue;
                        }

                        var (parsed, err) = parse(_addr_fset, token.Pos(int(cmt.Start.Byte) + adjust), text);
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        notes = append(notes, parsed);

                    } 
                    // Handle the normal case for markers on the same line.

                    cmt = cmt__prev2;
                }

                {
                    var cmt__prev2 = cmt;

                    foreach (var (_, __cmt) in comment.Suffix)
                    {
                        cmt = __cmt;
                        (text, adjust) = getAdjustedNote(cmt.Token);
                        if (text == "")
                        {
                            continue;
                        }

                        (parsed, err) = parse(_addr_fset, token.Pos(int(cmt.Start.Byte) + adjust), text);
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        notes = append(notes, parsed);

                    }

                    cmt = cmt__prev2;
                }
            }
            return (notes, error.As(null!)!);

        }

        // ExtractGo collects all the notes present in an AST.
        // Each comment whose text starts with @ is parsed as a comma-separated
        // sequence of notes.
        // See the package documentation for details about the syntax of those
        // notes.
        public static (slice<ptr<Note>>, error) ExtractGo(ptr<token.FileSet> _addr_fset, ptr<ast.File> _addr_file)
        {
            slice<ptr<Note>> _p0 = default;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;
            ref ast.File file = ref _addr_file.val;

            slice<ptr<Note>> notes = default;
            foreach (var (_, g) in file.Comments)
            {
                foreach (var (_, c) in g.List)
                {
                    var (text, adjust) = getAdjustedNote(c.Text);
                    if (text == "")
                    {
                        continue;
                    }

                    var (parsed, err) = parse(_addr_fset, token.Pos(int(c.Pos()) + adjust), text);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    notes = append(notes, parsed);

                }

            }
            return (notes, error.As(null!)!);

        }

        private static (@string, long) getAdjustedNote(@string text)
        {
            @string _p0 = default;
            long _p0 = default;

            if (strings.HasPrefix(text, "/*"))
            {
                text = strings.TrimSuffix(text, "*/");
            }

            text = text[2L..]; // remove "//" or "/*" prefix

            // Allow notes to appear within comments.
            // For example:
            // "// //@mark()" is valid.
            // "// @mark()" is not valid.
            // "// /*@mark()*/" is not valid.
            long adjust = default;
            {
                var i = strings.Index(text, commentStart);

                if (i > 2L)
                { 
                    // Get the text before the commentStart.
                    var pre = text[i - 2L..i];
                    if (pre != "//")
                    {
                        return ("", 0L);
                    }

                    text = text[i..];
                    adjust = i;

                }

            }

            if (!strings.HasPrefix(text, commentStart))
            {
                return ("", 0L);
            }

            text = text[commentStartLen..];
            return (text, commentStartLen + adjust + 1L);

        }

        private static readonly int invalidToken = (int)0L;



        private partial struct tokens
        {
            public scanner.Scanner scanner;
            public int current;
            public error err;
            public token.Pos @base;
        }

        private static ptr<tokens> Init(this ptr<tokens> _addr_t, token.Pos @base, @string text)
        {
            ref tokens t = ref _addr_t.val;

            t.@base = base;
            t.scanner.Init(strings.NewReader(text));
            t.scanner.Mode = scanner.GoTokens;
            t.scanner.Whitespace ^= 1L << (int)('\n'); // don't skip new lines
            t.scanner.Error = (s, msg) =>
            {
                t.Errorf("%v", msg);
            }
;
            return _addr_t!;

        }

        private static @string Consume(this ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            t.current = invalidToken;
            return t.scanner.TokenText();
        }

        private static int Token(this ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            if (t.err != null)
            {
                return scanner.EOF;
            }

            if (t.current == invalidToken)
            {
                t.current = t.scanner.Scan();
            }

            return t.current;

        }

        private static long Skip(this ptr<tokens> _addr_t, int r)
        {
            ref tokens t = ref _addr_t.val;

            long i = 0L;
            while (t.Token() == '\n')
            {
                t.Consume();
                i++;
            }

            return i;

        }

        private static @string TokenString(this ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            return scanner.TokenString(t.Token());
        }

        private static token.Pos Pos(this ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            return t.@base + token.Pos(t.scanner.Position.Offset);
        }

        private static void Errorf(this ptr<tokens> _addr_t, @string msg, params object[] args)
        {
            args = args.Clone();
            ref tokens t = ref _addr_t.val;

            if (t.err != null)
            {
                return ;
            }

            t.err = fmt.Errorf(msg, args);

        }

        private static (slice<ptr<Note>>, error) parse(ptr<token.FileSet> _addr_fset, token.Pos @base, @string text)
        {
            slice<ptr<Note>> _p0 = default;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;

            ptr<tokens> t = @new<tokens>().Init(base, text);
            var notes = parseComment(t);
            if (t.err != null)
            {
                return (null, error.As(fmt.Errorf("%v:%s", fset.Position(t.Pos()), t.err))!);
            }

            return (notes, error.As(null!)!);

        }

        private static slice<ptr<Note>> parseComment(ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            slice<ptr<Note>> notes = default;
            while (true)
            {
                t.Skip('\n');

                if (t.Token() == scanner.EOF) 
                    return notes;
                else if (t.Token() == scanner.Ident) 
                    notes = append(notes, parseNote(_addr_t));
                else 
                    t.Errorf("unexpected %s parsing comment, expect identifier", t.TokenString());
                    return null;
                
                if (t.Token() == scanner.EOF) 
                    return notes;
                else if (t.Token() == ',' || t.Token() == '\n') 
                    t.Consume();
                else 
                    t.Errorf("unexpected %s parsing comment, expect separator", t.TokenString());
                    return null;
                
            }


        }

        private static ptr<Note> parseNote(ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            ptr<Note> n = addr(new Note(Pos:t.Pos(),Name:t.Consume(),));


            if (t.Token() == ',' || t.Token() == '\n' || t.Token() == scanner.EOF) 
                // no argument list present
                return _addr_n!;
            else if (t.Token() == '(') 
                n.Args = parseArgumentList(_addr_t);
                return _addr_n!;
            else 
                t.Errorf("unexpected %s parsing note", t.TokenString());
                return _addr_null!;
            
        }

        private static slice<object> parseArgumentList(ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;

            t.Consume(); // '('
            t.Skip('\n');
            while (t.Token() != ')')
            {
                args = append(args, parseArgument(_addr_t));
                if (t.Token() != ',')
                {
                    break;
                }

                t.Consume();
                t.Skip('\n');

            }

            if (t.Token() != ')')
            {
                t.Errorf("unexpected %s parsing argument list", t.TokenString());
                return null;
            }

            t.Consume(); // ')'
            return args;

        }

        private static void parseArgument(ptr<tokens> _addr_t)
        {
            ref tokens t = ref _addr_t.val;


            if (t.Token() == scanner.Ident) 
                var v = t.Consume();
                switch (v)
                {
                    case "true": 
                        return true;
                        break;
                    case "false": 
                        return false;
                        break;
                    case "nil": 
                        return null;
                        break;
                    case "re": 
                        if (t.Token() != scanner.String && t.Token() != scanner.RawString)
                        {
                            t.Errorf("re must be followed by string, got %s", t.TokenString());
                            return null;
                        }

                        var (pattern, _) = strconv.Unquote(t.Consume()); // can't fail
                        var (re, err) = regexp.Compile(pattern);
                        if (err != null)
                        {
                            t.Errorf("invalid regular expression %s: %v", pattern, err);
                            return null;
                        }

                        return re;
                        break;
                    default: 
                        return Identifier(v);
                        break;
                }
            else if (t.Token() == scanner.String || t.Token() == scanner.RawString) 
                var (v, _) = strconv.Unquote(t.Consume()); // can't fail
                return v;
            else if (t.Token() == scanner.Int) 
                var s = t.Consume();
                var (v, err) = strconv.ParseInt(s, 0L, 0L);
                if (err != null)
                {
                    t.Errorf("cannot convert %v to int: %v", s, err);
                }

                return v;
            else if (t.Token() == scanner.Float) 
                s = t.Consume();
                (v, err) = strconv.ParseFloat(s, 64L);
                if (err != null)
                {
                    t.Errorf("cannot convert %v to float: %v", s, err);
                }

                return v;
            else if (t.Token() == scanner.Char) 
                t.Errorf("unexpected char literal %s", t.Consume());
                return null;
            else 
                t.Errorf("unexpected %s parsing argument", t.TokenString());
                return null;
            
        }
    }
}}}}}
