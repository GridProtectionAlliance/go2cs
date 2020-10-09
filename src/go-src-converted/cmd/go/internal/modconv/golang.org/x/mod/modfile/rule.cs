// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package modfile implements a parser and formatter for go.mod files.
//
// The go.mod syntax is described in
// https://golang.org/cmd/go/#hdr-The_go_mod_file.
//
// The Parse and ParseLax functions both parse a go.mod file and return an
// abstract syntax tree. ParseLax ignores unknown statements and may be used to
// parse go.mod files that may have been developed with newer versions of Go.
//
// The File struct returned by Parse and ParseLax represent an abstract
// go.mod file. File has several methods like AddNewRequire and DropReplace
// that can be used to programmatically edit a file.
//
// The Format function formats a File back to a byte slice which can be
// written to a file.
// package modfile -- go2cs converted at 2020 October 09 05:46:46 UTC
// import "golang.org/x/mod/modfile" ==> using modfile = go.golang.org.x.mod.modfile_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\modfile\rule.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;

using lazyregexp = go.golang.org.x.mod.@internal.lazyregexp_package;
using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace mod
{
    public static partial class modfile_package
    {
        // A File is the parsed, interpreted form of a go.mod file.
        public partial struct File
        {
            public ptr<Module> Module;
            public ptr<Go> Go;
            public slice<ptr<Require>> Require;
            public slice<ptr<Exclude>> Exclude;
            public slice<ptr<Replace>> Replace;
            public ptr<FileSyntax> Syntax;
        }

        // A Module is the module statement.
        public partial struct Module
        {
            public module.Version Mod;
            public ptr<Line> Syntax;
        }

        // A Go is the go statement.
        public partial struct Go
        {
            public @string Version; // "1.23"
            public ptr<Line> Syntax;
        }

        // A Require is a single require statement.
        public partial struct Require
        {
            public module.Version Mod;
            public bool Indirect; // has "// indirect" comment
            public ptr<Line> Syntax;
        }

        // An Exclude is a single exclude statement.
        public partial struct Exclude
        {
            public module.Version Mod;
            public ptr<Line> Syntax;
        }

        // A Replace is a single replace statement.
        public partial struct Replace
        {
            public module.Version Old;
            public module.Version New;
            public ptr<Line> Syntax;
        }

        private static error AddModuleStmt(this ptr<File> _addr_f, @string path)
        {
            ref File f = ref _addr_f.val;

            if (f.Syntax == null)
            {
                f.Syntax = @new<FileSyntax>();
            }
            if (f.Module == null)
            {
                f.Module = addr(new Module(Mod:module.Version{Path:path},Syntax:f.Syntax.addLine(nil,"module",AutoQuote(path)),));
            }
            else
            {
                f.Module.Mod.Path = path;
                f.Syntax.updateLine(f.Module.Syntax, "module", AutoQuote(path));
            }
            return error.As(null!)!;
        }

        private static void AddComment(this ptr<File> _addr_f, @string text)
        {
            ref File f = ref _addr_f.val;

            if (f.Syntax == null)
            {
                f.Syntax = @new<FileSyntax>();
            }
            f.Syntax.Stmt = append(f.Syntax.Stmt, addr(new CommentBlock(Comments:Comments{Before:[]Comment{{Token:text,},},},)));
        }

        public delegate  error) VersionFixer(@string,  @string,  (@string);

        // Parse parses the data, reported in errors as being from file,
        // into a File struct. It applies fix, if non-nil, to canonicalize all module versions found.
        public static (ptr<File>, error) Parse(@string file, slice<byte> data, VersionFixer fix)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            return _addr_parseToFile(file, data, fix, true)!;
        }

        // ParseLax is like Parse but ignores unknown statements.
        // It is used when parsing go.mod files other than the main module,
        // under the theory that most statement types we add in the future will
        // only apply in the main module, like exclude and replace,
        // and so we get better gradual deployments if old go commands
        // simply ignore those statements when found in go.mod files
        // in dependencies.
        public static (ptr<File>, error) ParseLax(@string file, slice<byte> data, VersionFixer fix)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            return _addr_parseToFile(file, data, fix, false)!;
        }

        private static (ptr<File>, error) parseToFile(@string file, slice<byte> data, VersionFixer fix, bool strict)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            var (fs, err) = parse(file, data);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }
            ptr<File> f = addr(new File(Syntax:fs,));

            ref ErrorList errs = ref heap(out ptr<ErrorList> _addr_errs);
            {
                var x__prev1 = x;

                foreach (var (_, __x) in fs.Stmt)
                {
                    x = __x;
                    switch (x.type())
                    {
                        case ptr<Line> x:
                            f.add(_addr_errs, x, x.Token[0L], x.Token[1L..], fix, strict);
                            break;
                        case ptr<LineBlock> x:
                            if (len(x.Token) > 1L)
                            {
                                if (strict)
                                {
                                    errs = append(errs, new Error(Filename:file,Pos:x.Start,Err:fmt.Errorf("unknown block type: %s",strings.Join(x.Token," ")),));
                                }
                                continue;
                            }
                            switch (x.Token[0L])
                            {
                                case "module": 

                                case "require": 

                                case "exclude": 

                                case "replace": 
                                    foreach (var (_, l) in x.Line)
                                    {
                                        f.add(_addr_errs, l, x.Token[0L], l.Token, fix, strict);
                                    }
                                    break;
                                default: 
                                    if (strict)
                                    {
                                        errs = append(errs, new Error(Filename:file,Pos:x.Start,Err:fmt.Errorf("unknown block type: %s",strings.Join(x.Token," ")),));
                                    }
                                    continue;
                                    break;
                            }
                            break;
                    }
                }

                x = x__prev1;
            }

            if (len(errs) > 0L)
            {
                return (_addr_null!, error.As(errs)!);
            }
            return (_addr_f!, error.As(null!)!);
        }

        public static var GoVersionRE = lazyregexp.New("^([1-9][0-9]*)\\.(0|[1-9][0-9]*)$");

        private static void add(this ptr<File> _addr_f, ptr<ErrorList> _addr_errs, ptr<Line> _addr_line, @string verb, slice<@string> args, VersionFixer fix, bool strict)
        {
            ref File f = ref _addr_f.val;
            ref ErrorList errs = ref _addr_errs.val;
            ref Line line = ref _addr_line.val;
 
            // If strict is false, this module is a dependency.
            // We ignore all unknown directives as well as main-module-only
            // directives like replace and exclude. It will work better for
            // forward compatibility if we can depend on modules that have unknown
            // statements (presumed relevant only when acting as the main module)
            // and simply ignore those statements.
            if (!strict)
            {
                switch (verb)
                {
                    case "module": 

                    case "require": 

                    case "go": 
                        break;
                    default: 
                        return ;
                        break;
                }
            }
            Action<@string, error> wrapModPathError = (modPath, err) =>
            {
                errs = append(errs, new Error(Filename:f.Syntax.Name,Pos:line.Start,ModPath:modPath,Verb:verb,Err:err,));
            }
;
            Action<error> wrapError = err =>
            {
                errs = append(errs, new Error(Filename:f.Syntax.Name,Pos:line.Start,Err:err,));
            }
;
            Action<@string, object[]> errorf = (format, args) =>
            {
                wrapError(fmt.Errorf(format, args));
            }
;

            switch (verb)
            {
                case "go": 
                    if (f.Go != null)
                    {
                        errorf("repeated go statement");
                        return ;
                    }
                    if (len(args) != 1L)
                    {
                        errorf("go directive expects exactly one argument");
                        return ;
                    }
                    else if (!GoVersionRE.MatchString(args[0L]))
                    {
                        errorf("invalid go version '%s': must match format 1.23", args[0L]);
                        return ;
                    }
                    f.Go = addr(new Go(Syntax:line));
                    f.Go.Version = args[0L];
                    break;
                case "module": 
                    if (f.Module != null)
                    {
                        errorf("repeated module statement");
                        return ;
                    }
                    f.Module = addr(new Module(Syntax:line));
                    if (len(args) != 1L)
                    {
                        errorf("usage: module module/path");
                        return ;
                    }
                    var (s, err) = parseString(_addr_args[0L]);
                    if (err != null)
                    {
                        errorf("invalid quoted string: %v", err);
                        return ;
                    }
                    f.Module.Mod = new module.Version(Path:s);
                    break;
                case "require": 

                case "exclude": 
                    if (len(args) != 2L)
                    {
                        errorf("usage: %s module/path v1.2.3", verb);
                        return ;
                    }
                    (s, err) = parseString(_addr_args[0L]);
                    if (err != null)
                    {
                        errorf("invalid quoted string: %v", err);
                        return ;
                    }
                    var (v, err) = parseVersion(verb, s, _addr_args[1L], fix);
                    if (err != null)
                    {
                        wrapError(err);
                        return ;
                    }
                    var (pathMajor, err) = modulePathMajor(s);
                    if (err != null)
                    {
                        wrapError(err);
                        return ;
                    }
                    {
                        var err__prev1 = err;

                        var err = module.CheckPathMajor(v, pathMajor);

                        if (err != null)
                        {
                            wrapModPathError(s, err);
                            return ;
                        }

                        err = err__prev1;

                    }
                    if (verb == "require")
                    {
                        f.Require = append(f.Require, addr(new Require(Mod:module.Version{Path:s,Version:v},Syntax:line,Indirect:isIndirect(line),)));
                    }
                    else
                    {
                        f.Exclude = append(f.Exclude, addr(new Exclude(Mod:module.Version{Path:s,Version:v},Syntax:line,)));
                    }
                    break;
                case "replace": 
                    long arrow = 2L;
                    if (len(args) >= 2L && args[1L] == "=>")
                    {
                        arrow = 1L;
                    }
                    if (len(args) < arrow + 2L || len(args) > arrow + 3L || args[arrow] != "=>")
                    {
                        errorf("usage: %s module/path [v1.2.3] => other/module v1.4\n\t or %s module/path [v1.2.3] => ../local/directory", verb, verb);
                        return ;
                    }
                    (s, err) = parseString(_addr_args[0L]);
                    if (err != null)
                    {
                        errorf("invalid quoted string: %v", err);
                        return ;
                    }
                    (pathMajor, err) = modulePathMajor(s);
                    if (err != null)
                    {
                        wrapModPathError(s, err);
                        return ;
                    }
                    @string v = default;
                    if (arrow == 2L)
                    {
                        v, err = parseVersion(verb, s, _addr_args[1L], fix);
                        if (err != null)
                        {
                            wrapError(err);
                            return ;
                        }
                        {
                            var err__prev2 = err;

                            err = module.CheckPathMajor(v, pathMajor);

                            if (err != null)
                            {
                                wrapModPathError(s, err);
                                return ;
                            }

                            err = err__prev2;

                        }
                    }
                    var (ns, err) = parseString(_addr_args[arrow + 1L]);
                    if (err != null)
                    {
                        errorf("invalid quoted string: %v", err);
                        return ;
                    }
                    @string nv = "";
                    if (len(args) == arrow + 2L)
                    {
                        if (!IsDirectoryPath(ns))
                        {
                            errorf("replacement module without version must be directory path (rooted or starting with ./ or ../)");
                            return ;
                        }
                        if (filepath.Separator == '/' && strings.Contains(ns, "\\"))
                        {
                            errorf("replacement directory appears to be Windows path (on a non-windows system)");
                            return ;
                        }
                    }
                    if (len(args) == arrow + 3L)
                    {
                        nv, err = parseVersion(verb, ns, _addr_args[arrow + 2L], fix);
                        if (err != null)
                        {
                            wrapError(err);
                            return ;
                        }
                        if (IsDirectoryPath(ns))
                        {
                            errorf("replacement module directory path %q cannot have version", ns);
                            return ;
                        }
                    }
                    f.Replace = append(f.Replace, addr(new Replace(Old:module.Version{Path:s,Version:v},New:module.Version{Path:ns,Version:nv},Syntax:line,)));
                    break;
                default: 
                    errorf("unknown directive: %s", verb);
                    break;
            }
        }

        // isIndirect reports whether line has a "// indirect" comment,
        // meaning it is in go.mod only for its effect on indirect dependencies,
        // so that it can be dropped entirely once the effective version of the
        // indirect dependency reaches the given minimum version.
        private static bool isIndirect(ptr<Line> _addr_line)
        {
            ref Line line = ref _addr_line.val;

            if (len(line.Suffix) == 0L)
            {
                return false;
            }
            var f = strings.Fields(strings.TrimPrefix(line.Suffix[0L].Token, string(slashSlash)));
            return (len(f) == 1L && f[0L] == "indirect" || len(f) > 1L && f[0L] == "indirect;");
        }

        // setIndirect sets line to have (or not have) a "// indirect" comment.
        private static void setIndirect(ptr<Line> _addr_line, bool indirect)
        {
            ref Line line = ref _addr_line.val;

            if (isIndirect(_addr_line) == indirect)
            {
                return ;
            }
            if (indirect)
            { 
                // Adding comment.
                if (len(line.Suffix) == 0L)
                { 
                    // New comment.
                    line.Suffix = new slice<Comment>(new Comment[] { {Token:"// indirect",Suffix:true} });
                    return ;
                }
                var com = _addr_line.Suffix[0L];
                var text = strings.TrimSpace(strings.TrimPrefix(com.Token, string(slashSlash)));
                if (text == "")
                { 
                    // Empty comment.
                    com.Token = "// indirect";
                    return ;
                } 

                // Insert at beginning of existing comment.
                com.Token = "// indirect; " + text;
                return ;
            } 

            // Removing comment.
            var f = strings.Fields(line.Suffix[0L].Token);
            if (len(f) == 2L)
            { 
                // Remove whole comment.
                line.Suffix = null;
                return ;
            } 

            // Remove comment prefix.
            com = _addr_line.Suffix[0L];
            var i = strings.Index(com.Token, "indirect;");
            com.Token = "//" + com.Token[i + len("indirect;")..];
        }

        // IsDirectoryPath reports whether the given path should be interpreted
        // as a directory path. Just like on the go command line, relative paths
        // and rooted paths are directory paths; the rest are module paths.
        public static bool IsDirectoryPath(@string ns)
        { 
            // Because go.mod files can move from one system to another,
            // we check all known path syntaxes, both Unix and Windows.
            return strings.HasPrefix(ns, "./") || strings.HasPrefix(ns, "../") || strings.HasPrefix(ns, "/") || strings.HasPrefix(ns, ".\\") || strings.HasPrefix(ns, "..\\") || strings.HasPrefix(ns, "\\") || len(ns) >= 2L && ('A' <= ns[0L] && ns[0L] <= 'Z' || 'a' <= ns[0L] && ns[0L] <= 'z') && ns[1L] == ':';
        }

        // MustQuote reports whether s must be quoted in order to appear as
        // a single token in a go.mod line.
        public static bool MustQuote(@string s)
        {
            foreach (var (_, r) in s)
            {
                switch (r)
                {
                    case ' ': 

                    case '"': 

                    case '\'': 

                    case '`': 
                        return true;
                        break;
                    case '(': 

                    case ')': 

                    case '[': 

                    case ']': 

                    case '{': 

                    case '}': 

                    case ',': 
                        if (len(s) > 1L)
                        {
                            return true;
                        }
                        break;
                    default: 
                        if (!unicode.IsPrint(r))
                        {
                            return true;
                        }
                        break;
                }
            }
            return s == "" || strings.Contains(s, "//") || strings.Contains(s, "/*");
        }

        // AutoQuote returns s or, if quoting is required for s to appear in a go.mod,
        // the quotation of s.
        public static @string AutoQuote(@string s)
        {
            if (MustQuote(s))
            {
                return strconv.Quote(s);
            }
            return s;
        }

        private static (@string, error) parseString(ptr<@string> _addr_s)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref @string s = ref _addr_s.val;

            @string t = s;
            if (strings.HasPrefix(t, "\""))
            {
                error err = default!;
                t, err = strconv.Unquote(t);

                if (err != null)
                {
                    return ("", error.As(err)!);
                }
            }
            else if (strings.ContainsAny(t, "\"'`"))
            { 
                // Other quotes are reserved both for possible future expansion
                // and to avoid confusion. For example if someone types 'x'
                // we want that to be a syntax error and not a literal x in literal quotation marks.
                return ("", error.As(fmt.Errorf("unquoted string cannot contain quote"))!);
            }
            s = AutoQuote(t);
            return (t, error.As(null!)!);
        }

        public partial struct ErrorList // : slice<Error>
        {
        }

        public static @string Error(this ErrorList e)
        {
            var errStrs = make_slice<@string>(len(e));
            foreach (var (i, err) in e)
            {
                errStrs[i] = err.Error();
            }
            return strings.Join(errStrs, "\n");
        }

        public partial struct Error
        {
            public @string Filename;
            public Position Pos;
            public @string Verb;
            public @string ModPath;
            public error Err;
        }

        private static @string Error(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            @string pos = default;
            if (e.Pos.LineRune > 1L)
            { 
                // Don't print LineRune if it's 1 (beginning of line).
                // It's always 1 except in scanner errors, which are rare.
                pos = fmt.Sprintf("%s:%d:%d: ", e.Filename, e.Pos.Line, e.Pos.LineRune);
            }
            else if (e.Pos.Line > 0L)
            {
                pos = fmt.Sprintf("%s:%d: ", e.Filename, e.Pos.Line);
            }
            else if (e.Filename != "")
            {
                pos = fmt.Sprintf("%s: ", e.Filename);
            }
            @string directive = default;
            if (e.ModPath != "")
            {
                directive = fmt.Sprintf("%s %s: ", e.Verb, e.ModPath);
            }
            return pos + directive + e.Err.Error();
        }

        private static error Unwrap(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return error.As(e.Err)!;
        }

        private static (@string, error) parseVersion(@string verb, @string path, ptr<@string> _addr_s, VersionFixer fix)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref @string s = ref _addr_s.val;

            var (t, err) = parseString(_addr_s);
            if (err != null)
            {
                return ("", error.As(addr(new Error(Verb:verb,ModPath:path,Err:&module.InvalidVersionError{Version:*s,Err:err,},))!)!);
            }
            if (fix != null)
            {
                error err = default!;
                t, err = fix(path, t);
                if (err != null)
                {
                    {
                        error err__prev3 = err;

                        ptr<module.ModuleError> (err, ok) = err._<ptr<module.ModuleError>>();

                        if (ok)
                        {
                            return ("", error.As(addr(new Error(Verb:verb,ModPath:path,Err:err.Err,))!)!);
                        }

                        err = err__prev3;

                    }
                    return ("", error.As(err)!);
                }
            }
            {
                var v = module.CanonicalVersion(t);

                if (v != "")
                {
                    s = v;
                    return (s, error.As(null!)!);
                }

            }
            return ("", error.As(addr(new Error(Verb:verb,ModPath:path,Err:&module.InvalidVersionError{Version:t,Err:errors.New("must be of the form v1.2.3"),},))!)!);
        }

        private static (@string, error) modulePathMajor(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            var (_, major, ok) = module.SplitPathVersion(path);
            if (!ok)
            {
                return ("", error.As(fmt.Errorf("invalid module path"))!);
            }
            return (major, error.As(null!)!);
        }

        private static (slice<byte>, error) Format(this ptr<File> _addr_f)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref File f = ref _addr_f.val;

            return (Format(f.Syntax), error.As(null!)!);
        }

        // Cleanup cleans up the file f after any edit operations.
        // To avoid quadratic behavior, modifications like DropRequire
        // clear the entry but do not remove it from the slice.
        // Cleanup cleans out all the cleared entries.
        private static void Cleanup(this ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

            long w = 0L;
            {
                var r__prev1 = r;

                foreach (var (_, __r) in f.Require)
                {
                    r = __r;
                    if (r.Mod.Path != "")
                    {
                        f.Require[w] = r;
                        w++;
                    }
                }

                r = r__prev1;
            }

            f.Require = f.Require[..w];

            w = 0L;
            foreach (var (_, x) in f.Exclude)
            {
                if (x.Mod.Path != "")
                {
                    f.Exclude[w] = x;
                    w++;
                }
            }
            f.Exclude = f.Exclude[..w];

            w = 0L;
            {
                var r__prev1 = r;

                foreach (var (_, __r) in f.Replace)
                {
                    r = __r;
                    if (r.Old.Path != "")
                    {
                        f.Replace[w] = r;
                        w++;
                    }
                }

                r = r__prev1;
            }

            f.Replace = f.Replace[..w];

            f.Syntax.Cleanup();
        }

        private static error AddGoStmt(this ptr<File> _addr_f, @string version)
        {
            ref File f = ref _addr_f.val;

            if (!GoVersionRE.MatchString(version))
            {
                return error.As(fmt.Errorf("invalid language version string %q", version))!;
            }
            if (f.Go == null)
            {
                Expr hint = default;
                if (f.Module != null && f.Module.Syntax != null)
                {
                    hint = f.Module.Syntax;
                }
                f.Go = addr(new Go(Version:version,Syntax:f.Syntax.addLine(hint,"go",version),));
            }
            else
            {
                f.Go.Version = version;
                f.Syntax.updateLine(f.Go.Syntax, "go", version);
            }
            return error.As(null!)!;
        }

        private static error AddRequire(this ptr<File> _addr_f, @string path, @string vers)
        {
            ref File f = ref _addr_f.val;

            var need = true;
            foreach (var (_, r) in f.Require)
            {
                if (r.Mod.Path == path)
                {
                    if (need)
                    {
                        r.Mod.Version = vers;
                        f.Syntax.updateLine(r.Syntax, "require", AutoQuote(path), vers);
                        need = false;
                    }
                    else
                    {
                        f.Syntax.removeLine(r.Syntax);
                        r.val = new Require();
                    }
                }
            }
            if (need)
            {
                f.AddNewRequire(path, vers, false);
            }
            return error.As(null!)!;
        }

        private static void AddNewRequire(this ptr<File> _addr_f, @string path, @string vers, bool indirect)
        {
            ref File f = ref _addr_f.val;

            var line = f.Syntax.addLine(null, "require", AutoQuote(path), vers);
            setIndirect(_addr_line, indirect);
            f.Require = append(f.Require, addr(new Require(module.Version{Path:path,Version:vers},indirect,line)));
        }

        private static void SetRequire(this ptr<File> _addr_f, slice<ptr<Require>> req)
        {
            ref File f = ref _addr_f.val;

            var need = make_map<@string, @string>();
            var indirect = make_map<@string, bool>();
            {
                var r__prev1 = r;

                foreach (var (_, __r) in req)
                {
                    r = __r;
                    need[r.Mod.Path] = r.Mod.Version;
                    indirect[r.Mod.Path] = r.Indirect;
                }

                r = r__prev1;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in f.Require)
                {
                    r = __r;
                    {
                        var (v, ok) = need[r.Mod.Path];

                        if (ok)
                        {
                            r.Mod.Version = v;
                            r.Indirect = indirect[r.Mod.Path];
                        }
                        else
                        {
                            r.val = new Require();
                        }

                    }
                }

                r = r__prev1;
            }

            slice<Expr> newStmts = default;
            {
                var stmt__prev1 = stmt;

                foreach (var (_, __stmt) in f.Syntax.Stmt)
                {
                    stmt = __stmt;
                    switch (stmt.type())
                    {
                        case ptr<LineBlock> stmt:
                            if (len(stmt.Token) > 0L && stmt.Token[0L] == "require")
                            {
                                slice<ptr<Line>> newLines = default;
                                foreach (var (_, line) in stmt.Line)
                                {
                                    {
                                        var p__prev2 = p;

                                        var (p, err) = parseString(_addr_line.Token[0L]);

                                        if (err == null && need[p] != "")
                                        {
                                            if (len(line.Comments.Before) == 1L && len(line.Comments.Before[0L].Token) == 0L)
                                            {
                                                line.Comments.Before = line.Comments.Before[..0L];
                                            }
                                            line.Token[1L] = need[p];
                                            delete(need, p);
                                            setIndirect(_addr_line, indirect[p]);
                                            newLines = append(newLines, line);
                                        }

                                        p = p__prev2;

                                    }
                                }
                                if (len(newLines) == 0L)
                                {
                                    continue; // drop stmt
                                }
                                stmt.Line = newLines;
                            }
                            break;
                        case ptr<Line> stmt:
                            if (len(stmt.Token) > 0L && stmt.Token[0L] == "require")
                            {
                                {
                                    var p__prev2 = p;

                                    (p, err) = parseString(_addr_stmt.Token[1L]);

                                    if (err == null && need[p] != "")
                                    {
                                        stmt.Token[2L] = need[p];
                                        delete(need, p);
                                        setIndirect(_addr_stmt, indirect[p]);
                                    }
                                    else
                                    {
                                        continue; // drop stmt
                                    }

                                    p = p__prev2;

                                }
                            }
                            break;
                    }
                    newStmts = append(newStmts, stmt);
                }

                stmt = stmt__prev1;
            }

            f.Syntax.Stmt = newStmts;

            foreach (var (path, vers) in need)
            {
                f.AddNewRequire(path, vers, indirect[path]);
            }
            f.SortBlocks();
        }

        private static error DropRequire(this ptr<File> _addr_f, @string path)
        {
            ref File f = ref _addr_f.val;

            foreach (var (_, r) in f.Require)
            {
                if (r.Mod.Path == path)
                {
                    f.Syntax.removeLine(r.Syntax);
                    r.val = new Require();
                }
            }
            return error.As(null!)!;
        }

        private static error AddExclude(this ptr<File> _addr_f, @string path, @string vers)
        {
            ref File f = ref _addr_f.val;

            ptr<Line> hint;
            foreach (var (_, x) in f.Exclude)
            {
                if (x.Mod.Path == path && x.Mod.Version == vers)
                {
                    return error.As(null!)!;
                }
                if (x.Mod.Path == path)
                {
                    hint = x.Syntax;
                }
            }
            f.Exclude = append(f.Exclude, addr(new Exclude(Mod:module.Version{Path:path,Version:vers},Syntax:f.Syntax.addLine(hint,"exclude",AutoQuote(path),vers))));
            return error.As(null!)!;
        }

        private static error DropExclude(this ptr<File> _addr_f, @string path, @string vers)
        {
            ref File f = ref _addr_f.val;

            foreach (var (_, x) in f.Exclude)
            {
                if (x.Mod.Path == path && x.Mod.Version == vers)
                {
                    f.Syntax.removeLine(x.Syntax);
                    x.val = new Exclude();
                }
            }
            return error.As(null!)!;
        }

        private static error AddReplace(this ptr<File> _addr_f, @string oldPath, @string oldVers, @string newPath, @string newVers)
        {
            ref File f = ref _addr_f.val;

            var need = true;
            module.Version old = new module.Version(Path:oldPath,Version:oldVers);
            module.Version @new = new module.Version(Path:newPath,Version:newVers);
            @string tokens = new slice<@string>(new @string[] { "replace", AutoQuote(oldPath) });
            if (oldVers != "")
            {
                tokens = append(tokens, oldVers);
            }
            tokens = append(tokens, "=>", AutoQuote(newPath));
            if (newVers != "")
            {
                tokens = append(tokens, newVers);
            }
            ptr<Line> hint;
            foreach (var (_, r) in f.Replace)
            {
                if (r.Old.Path == oldPath && (oldVers == "" || r.Old.Version == oldVers))
                {
                    if (need)
                    { 
                        // Found replacement for old; update to use new.
                        r.New = new;
                        f.Syntax.updateLine(r.Syntax, tokens);
                        need = false;
                        continue;
                    } 
                    // Already added; delete other replacements for same.
                    f.Syntax.removeLine(r.Syntax);
                    r.val = new Replace();
                }
                if (r.Old.Path == oldPath)
                {
                    hint = r.Syntax;
                }
            }
            if (need)
            {
                f.Replace = append(f.Replace, addr(new Replace(Old:old,New:new,Syntax:f.Syntax.addLine(hint,tokens...))));
            }
            return error.As(null!)!;
        }

        private static error DropReplace(this ptr<File> _addr_f, @string oldPath, @string oldVers)
        {
            ref File f = ref _addr_f.val;

            foreach (var (_, r) in f.Replace)
            {
                if (r.Old.Path == oldPath && r.Old.Version == oldVers)
                {
                    f.Syntax.removeLine(r.Syntax);
                    r.val = new Replace();
                }
            }
            return error.As(null!)!;
        }

        private static void SortBlocks(this ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

            f.removeDups(); // otherwise sorting is unsafe

            foreach (var (_, stmt) in f.Syntax.Stmt)
            {
                ptr<LineBlock> (block, ok) = stmt._<ptr<LineBlock>>();
                if (!ok)
                {
                    continue;
                }
                sort.Slice(block.Line, (i, j) =>
                {
                    var li = block.Line[i];
                    var lj = block.Line[j];
                    for (long k = 0L; k < len(li.Token) && k < len(lj.Token); k++)
                    {
                        if (li.Token[k] != lj.Token[k])
                        {
                            return li.Token[k] < lj.Token[k];
                        }
                    }

                    return len(li.Token) < len(lj.Token);
                });
            }
        }

        private static void removeDups(this ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

            var have = make_map<module.Version, bool>();
            var kill = make_map<ptr<Line>, bool>();
            {
                var x__prev1 = x;

                foreach (var (_, __x) in f.Exclude)
                {
                    x = __x;
                    if (have[x.Mod])
                    {
                        kill[x.Syntax] = true;
                        continue;
                    }
                    have[x.Mod] = true;
                }

                x = x__prev1;
            }

            slice<ptr<Exclude>> excl = default;
            {
                var x__prev1 = x;

                foreach (var (_, __x) in f.Exclude)
                {
                    x = __x;
                    if (!kill[x.Syntax])
                    {
                        excl = append(excl, x);
                    }
                }

                x = x__prev1;
            }

            f.Exclude = excl;

            have = make_map<module.Version, bool>(); 
            // Later replacements take priority over earlier ones.
            for (var i = len(f.Replace) - 1L; i >= 0L; i--)
            {
                var x = f.Replace[i];
                if (have[x.Old])
                {
                    kill[x.Syntax] = true;
                    continue;
                }
                have[x.Old] = true;
            }

            slice<ptr<Replace>> repl = default;
            {
                var x__prev1 = x;

                foreach (var (_, __x) in f.Replace)
                {
                    x = __x;
                    if (!kill[x.Syntax])
                    {
                        repl = append(repl, x);
                    }
                }

                x = x__prev1;
            }

            f.Replace = repl;

            slice<Expr> stmts = default;
            {
                var stmt__prev1 = stmt;

                foreach (var (_, __stmt) in f.Syntax.Stmt)
                {
                    stmt = __stmt;
                    switch (stmt.type())
                    {
                        case ptr<Line> stmt:
                            if (kill[stmt])
                            {
                                continue;
                            }
                            break;
                        case ptr<LineBlock> stmt:
                            slice<ptr<Line>> lines = default;
                            foreach (var (_, line) in stmt.Line)
                            {
                                if (!kill[line])
                                {
                                    lines = append(lines, line);
                                }
                            }
                            stmt.Line = lines;
                            if (len(lines) == 0L)
                            {
                                continue;
                            }
                            break;
                    }
                    stmts = append(stmts, stmt);
                }

                stmt = stmt__prev1;
            }

            f.Syntax.Stmt = stmts;
        }
    }
}}}}
