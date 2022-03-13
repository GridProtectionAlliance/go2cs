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

// package modfile -- go2cs converted at 2022 March 13 06:40:56 UTC
// import "cmd/vendor/golang.org/x/mod/modfile" ==> using modfile = go.cmd.vendor.golang.org.x.mod.modfile_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\modfile\rule.go
namespace go.cmd.vendor.golang.org.x.mod;

using errors = errors_package;
using fmt = fmt_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;

using lazyregexp = golang.org.x.mod.@internal.lazyregexp_package;
using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// A File is the parsed, interpreted form of a go.mod file.

using System;
public static partial class modfile_package {

public partial struct File {
    public ptr<Module> Module;
    public ptr<Go> Go;
    public slice<ptr<Require>> Require;
    public slice<ptr<Exclude>> Exclude;
    public slice<ptr<Replace>> Replace;
    public slice<ptr<Retract>> Retract;
    public ptr<FileSyntax> Syntax;
}

// A Module is the module statement.
public partial struct Module {
    public module.Version Mod;
    public @string Deprecated;
    public ptr<Line> Syntax;
}

// A Go is the go statement.
public partial struct Go {
    public @string Version; // "1.23"
    public ptr<Line> Syntax;
}

// An Exclude is a single exclude statement.
public partial struct Exclude {
    public module.Version Mod;
    public ptr<Line> Syntax;
}

// A Replace is a single replace statement.
public partial struct Replace {
    public module.Version Old;
    public module.Version New;
    public ptr<Line> Syntax;
}

// A Retract is a single retract statement.
public partial struct Retract {
    public ref VersionInterval VersionInterval => ref VersionInterval_val;
    public @string Rationale;
    public ptr<Line> Syntax;
}

// A VersionInterval represents a range of versions with upper and lower bounds.
// Intervals are closed: both bounds are included. When Low is equal to High,
// the interval may refer to a single version ('v1.2.3') or an interval
// ('[v1.2.3, v1.2.3]'); both have the same representation.
public partial struct VersionInterval {
    public @string Low;
    public @string High;
}

// A Require is a single require statement.
public partial struct Require {
    public module.Version Mod;
    public bool Indirect; // has "// indirect" comment
    public ptr<Line> Syntax;
}

private static void markRemoved(this ptr<Require> _addr_r) {
    ref Require r = ref _addr_r.val;

    r.Syntax.markRemoved();
    r.val = new Require();
}

private static void setVersion(this ptr<Require> _addr_r, @string v) {
    ref Require r = ref _addr_r.val;

    r.Mod.Version = v;

    {
        var line = r.Syntax;

        if (len(line.Token) > 0) {
            if (line.InBlock) { 
                // If the line is preceded by an empty line, remove it; see
                // https://golang.org/issue/33779.
                if (len(line.Comments.Before) == 1 && len(line.Comments.Before[0].Token) == 0) {
                    line.Comments.Before = line.Comments.Before[..(int)0];
                }
                if (len(line.Token) >= 2) { // example.com v1.2.3
                    line.Token[1] = v;
                }
            }
            else
 {
                if (len(line.Token) >= 3) { // require example.com v1.2.3
                    line.Token[2] = v;
                }
            }
        }
    }
}

// setIndirect sets line to have (or not have) a "// indirect" comment.
private static void setIndirect(this ptr<Require> _addr_r, bool indirect) {
    ref Require r = ref _addr_r.val;

    r.Indirect = indirect;
    var line = r.Syntax;
    if (isIndirect(_addr_line) == indirect) {
        return ;
    }
    if (indirect) { 
        // Adding comment.
        if (len(line.Suffix) == 0) { 
            // New comment.
            line.Suffix = new slice<Comment>(new Comment[] { {Token:"// indirect",Suffix:true} });
            return ;
        }
        var com = _addr_line.Suffix[0];
        var text = strings.TrimSpace(strings.TrimPrefix(com.Token, string(slashSlash)));
        if (text == "") { 
            // Empty comment.
            com.Token = "// indirect";
            return ;
        }
        com.Token = "// indirect; " + text;
        return ;
    }
    var f = strings.TrimSpace(strings.TrimPrefix(line.Suffix[0].Token, string(slashSlash)));
    if (f == "indirect") { 
        // Remove whole comment.
        line.Suffix = null;
        return ;
    }
    com = _addr_line.Suffix[0];
    var i = strings.Index(com.Token, "indirect;");
    com.Token = "//" + com.Token[(int)i + len("indirect;")..];
}

// isIndirect reports whether line has a "// indirect" comment,
// meaning it is in go.mod only for its effect on indirect dependencies,
// so that it can be dropped entirely once the effective version of the
// indirect dependency reaches the given minimum version.
private static bool isIndirect(ptr<Line> _addr_line) {
    ref Line line = ref _addr_line.val;

    if (len(line.Suffix) == 0) {
        return false;
    }
    var f = strings.Fields(strings.TrimPrefix(line.Suffix[0].Token, string(slashSlash)));
    return (len(f) == 1 && f[0] == "indirect" || len(f) > 1 && f[0] == "indirect;");
}

private static error AddModuleStmt(this ptr<File> _addr_f, @string path) {
    ref File f = ref _addr_f.val;

    if (f.Syntax == null) {
        f.Syntax = @new<FileSyntax>();
    }
    if (f.Module == null) {
        f.Module = addr(new Module(Mod:module.Version{Path:path},Syntax:f.Syntax.addLine(nil,"module",AutoQuote(path)),));
    }
    else
 {
        f.Module.Mod.Path = path;
        f.Syntax.updateLine(f.Module.Syntax, "module", AutoQuote(path));
    }
    return error.As(null!)!;
}

private static void AddComment(this ptr<File> _addr_f, @string text) {
    ref File f = ref _addr_f.val;

    if (f.Syntax == null) {
        f.Syntax = @new<FileSyntax>();
    }
    f.Syntax.Stmt = append(f.Syntax.Stmt, addr(new CommentBlock(Comments:Comments{Before:[]Comment{{Token:text,},},},)));
}

public delegate  error) VersionFixer(@string,  @string,  (@string);

// errDontFix is returned by a VersionFixer to indicate the version should be
// left alone, even if it's not canonical.
private static VersionFixer dontFixRetract = (_, vers) => (vers, null);

// Parse parses and returns a go.mod file.
//
// file is the name of the file, used in positions and errors.
//
// data is the content of the file.
//
// fix is an optional function that canonicalizes module versions.
// If fix is nil, all module versions must be canonical (module.CanonicalVersion
// must return the same string).
public static (ptr<File>, error) Parse(@string file, slice<byte> data, VersionFixer fix) {
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
public static (ptr<File>, error) ParseLax(@string file, slice<byte> data, VersionFixer fix) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    return _addr_parseToFile(file, data, fix, false)!;
}

private static (ptr<File>, error) parseToFile(@string file, slice<byte> data, VersionFixer fix, bool strict) => func((defer, _, _) => {
    ptr<File> parsed = default!;
    error err = default!;

    var (fs, err) = parse(file, data);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ptr<File> f = addr(new File(Syntax:fs,));
    ref ErrorList errs = ref heap(out ptr<ErrorList> _addr_errs); 

    // fix versions in retract directives after the file is parsed.
    // We need the module path to fix versions, and it might be at the end.
    defer(() => {
        var oldLen = len(errs);
        f.fixRetract(fix, _addr_errs);
        if (len(errs) > oldLen) {
            (parsed, err) = (null, errs);
        }
    }());

    {
        var x__prev1 = x;

        foreach (var (_, __x) in fs.Stmt) {
            x = __x;
            switch (x.type()) {
                case ptr<Line> x:
                    f.add(_addr_errs, null, x, x.Token[0], x.Token[(int)1..], fix, strict);
                    break;
                case ptr<LineBlock> x:
                    if (len(x.Token) > 1) {
                        if (strict) {
                            errs = append(errs, new Error(Filename:file,Pos:x.Start,Err:fmt.Errorf("unknown block type: %s",strings.Join(x.Token," ")),));
                        }
                        continue;
                    }
                    switch (x.Token[0]) {
                        case "module": 

                        case "require": 

                        case "exclude": 

                        case "replace": 

                        case "retract": 
                            foreach (var (_, l) in x.Line) {
                                f.add(_addr_errs, x, l, x.Token[0], l.Token, fix, strict);
                            }
                            break;
                        default: 
                            if (strict) {
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

    if (len(errs) > 0) {
        return (_addr_null!, error.As(errs)!);
    }
    return (_addr_f!, error.As(null!)!);
});

public static var GoVersionRE = lazyregexp.New("^([1-9][0-9]*)\\.(0|[1-9][0-9]*)$");
private static var laxGoVersionRE = lazyregexp.New("^v?(([1-9][0-9]*)\\.(0|[1-9][0-9]*))([^0-9].*)$");

private static void add(this ptr<File> _addr_f, ptr<ErrorList> _addr_errs, ptr<LineBlock> _addr_block, ptr<Line> _addr_line, @string verb, slice<@string> args, VersionFixer fix, bool strict) {
    ref File f = ref _addr_f.val;
    ref ErrorList errs = ref _addr_errs.val;
    ref LineBlock block = ref _addr_block.val;
    ref Line line = ref _addr_line.val;
 
    // If strict is false, this module is a dependency.
    // We ignore all unknown directives as well as main-module-only
    // directives like replace and exclude. It will work better for
    // forward compatibility if we can depend on modules that have unknown
    // statements (presumed relevant only when acting as the main module)
    // and simply ignore those statements.
    if (!strict) {
        switch (verb) {
            case "go": 

            case "module": 

            case "retract": 

            case "require": 

                break;
            default: 
                return ;
                break;
        }
    }
    Action<@string, error> wrapModPathError = (modPath, err) => {
        errs = append(errs, new Error(Filename:f.Syntax.Name,Pos:line.Start,ModPath:modPath,Verb:verb,Err:err,));
    };
    Action<error> wrapError = err => {
        errs = append(errs, new Error(Filename:f.Syntax.Name,Pos:line.Start,Err:err,));
    };
    Action<@string, object[]> errorf = (format, args) => {
        wrapError(fmt.Errorf(format, args));
    };

    switch (verb) {
        case "go": 
            if (f.Go != null) {
                errorf("repeated go statement");
                return ;
            }
            if (len(args) != 1) {
                errorf("go directive expects exactly one argument");
                return ;
            }
            else if (!GoVersionRE.MatchString(args[0])) {
                var @fixed = false;
                if (!strict) {
                    {
                        var m = laxGoVersionRE.FindStringSubmatch(args[0]);

                        if (m != null) {
                            args[0] = m[1];
                            fixed = true;
                        }

                    }
                }
                if (!fixed) {
                    errorf("invalid go version '%s': must match format 1.23", args[0]);
                    return ;
                }
            }
            f.Go = addr(new Go(Syntax:line));
            f.Go.Version = args[0];
            break;
        case "module": 
            if (f.Module != null) {
                errorf("repeated module statement");
                return ;
            }
            var deprecated = parseDeprecation(_addr_block, _addr_line);
            f.Module = addr(new Module(Syntax:line,Deprecated:deprecated,));
            if (len(args) != 1) {
                errorf("usage: module module/path");
                return ;
            }
            var (s, err) = parseString(_addr_args[0]);
            if (err != null) {
                errorf("invalid quoted string: %v", err);
                return ;
            }
            f.Module.Mod = new module.Version(Path:s);
            break;
        case "require": 

        case "exclude": 
                   if (len(args) != 2) {
                       errorf("usage: %s module/path v1.2.3", verb);
                       return ;
                   }
                   (s, err) = parseString(_addr_args[0]);
                   if (err != null) {
                       errorf("invalid quoted string: %v", err);
                       return ;
                   }
                   var (v, err) = parseVersion(verb, s, _addr_args[1], fix);
                   if (err != null) {
                       wrapError(err);
                       return ;
                   }
                   var (pathMajor, err) = modulePathMajor(s);
                   if (err != null) {
                       wrapError(err);
                       return ;
                   }
                   {
                       var err__prev1 = err;

                       var err = module.CheckPathMajor(v, pathMajor);

                       if (err != null) {
                           wrapModPathError(s, err);
                           return ;
                       }

                       err = err__prev1;

                   }
                   if (verb == "require") {
                       f.Require = append(f.Require, addr(new Require(Mod:module.Version{Path:s,Version:v},Syntax:line,Indirect:isIndirect(line),)));
                   }
                   else
            {
                       f.Exclude = append(f.Exclude, addr(new Exclude(Mod:module.Version{Path:s,Version:v},Syntax:line,)));
                   }
            break;
        case "replace": 
            nint arrow = 2;
            if (len(args) >= 2 && args[1] == "=>") {
                arrow = 1;
            }
            if (len(args) < arrow + 2 || len(args) > arrow + 3 || args[arrow] != "=>") {
                errorf("usage: %s module/path [v1.2.3] => other/module v1.4\n\t or %s module/path [v1.2.3] => ../local/directory", verb, verb);
                return ;
            }
            (s, err) = parseString(_addr_args[0]);
            if (err != null) {
                errorf("invalid quoted string: %v", err);
                return ;
            }
            (pathMajor, err) = modulePathMajor(s);
            if (err != null) {
                wrapModPathError(s, err);
                return ;
            }
            @string v = default;
            if (arrow == 2) {
                v, err = parseVersion(verb, s, _addr_args[1], fix);
                if (err != null) {
                    wrapError(err);
                    return ;
                }
                {
                    var err__prev2 = err;

                    err = module.CheckPathMajor(v, pathMajor);

                    if (err != null) {
                        wrapModPathError(s, err);
                        return ;
                    }

                    err = err__prev2;

                }
            }
            var (ns, err) = parseString(_addr_args[arrow + 1]);
            if (err != null) {
                errorf("invalid quoted string: %v", err);
                return ;
            }
            @string nv = "";
            if (len(args) == arrow + 2) {
                if (!IsDirectoryPath(ns)) {
                    errorf("replacement module without version must be directory path (rooted or starting with ./ or ../)");
                    return ;
                }
                if (filepath.Separator == '/' && strings.Contains(ns, "\\")) {
                    errorf("replacement directory appears to be Windows path (on a non-windows system)");
                    return ;
                }
            }
            if (len(args) == arrow + 3) {
                nv, err = parseVersion(verb, ns, _addr_args[arrow + 2], fix);
                if (err != null) {
                    wrapError(err);
                    return ;
                }
                if (IsDirectoryPath(ns)) {
                    errorf("replacement module directory path %q cannot have version", ns);
                    return ;
                }
            }
            f.Replace = append(f.Replace, addr(new Replace(Old:module.Version{Path:s,Version:v},New:module.Version{Path:ns,Version:nv},Syntax:line,)));
            break;
        case "retract": 
                   var rationale = parseDirectiveComment(_addr_block, _addr_line);
                   var (vi, err) = parseVersionInterval(verb, "", _addr_args, dontFixRetract);
                   if (err != null) {
                       if (strict) {
                           wrapError(err);
                           return ;
                       }
                       else
            { 
                           // Only report errors parsing intervals in the main module. We may
                           // support additional syntax in the future, such as open and half-open
                           // intervals. Those can't be supported now, because they break the
                           // go.mod parser, even in lax mode.
                           return ;
                       }
                   }
                   if (len(args) > 0 && strict) { 
                       // In the future, there may be additional information after the version.
                       errorf("unexpected token after version: %q", args[0]);
                       return ;
                   }
                   ptr<Retract> retract = addr(new Retract(VersionInterval:vi,Rationale:rationale,Syntax:line,));
                   f.Retract = append(f.Retract, retract);
            break;
        default: 
            errorf("unknown directive: %s", verb);
            break;
    }
}

// fixRetract applies fix to each retract directive in f, appending any errors
// to errs.
//
// Most versions are fixed as we parse the file, but for retract directives,
// the relevant module path is the one specified with the module directive,
// and that might appear at the end of the file (or not at all).
private static void fixRetract(this ptr<File> _addr_f, VersionFixer fix, ptr<ErrorList> _addr_errs) {
    ref File f = ref _addr_f.val;
    ref ErrorList errs = ref _addr_errs.val;

    if (fix == null) {
        return ;
    }
    @string path = "";
    if (f.Module != null) {
        path = f.Module.Mod.Path;
    }
    ptr<Retract> r;
    Action<error> wrapError = err => {
        errs = append(errs, new Error(Filename:f.Syntax.Name,Pos:r.Syntax.Start,Err:err,));
    };

    foreach (var (_, __r) in f.Retract) {
        r = __r;
        if (path == "") {
            wrapError(errors.New("no module directive found, so retract cannot be used"));
            return ; // only print the first one of these
        }
        ref var args = ref heap(r.Syntax.Token, out ptr<var> _addr_args);
        if (args[0] == "retract") {
            args = args[(int)1..];
        }
        var (vi, err) = parseVersionInterval("retract", path, _addr_args, fix);
        if (err != null) {
            wrapError(err);
        }
        r.VersionInterval = vi;
    }
}

// IsDirectoryPath reports whether the given path should be interpreted
// as a directory path. Just like on the go command line, relative paths
// and rooted paths are directory paths; the rest are module paths.
public static bool IsDirectoryPath(@string ns) { 
    // Because go.mod files can move from one system to another,
    // we check all known path syntaxes, both Unix and Windows.
    return strings.HasPrefix(ns, "./") || strings.HasPrefix(ns, "../") || strings.HasPrefix(ns, "/") || strings.HasPrefix(ns, ".\\") || strings.HasPrefix(ns, "..\\") || strings.HasPrefix(ns, "\\") || len(ns) >= 2 && ('A' <= ns[0] && ns[0] <= 'Z' || 'a' <= ns[0] && ns[0] <= 'z') && ns[1] == ':';
}

// MustQuote reports whether s must be quoted in order to appear as
// a single token in a go.mod line.
public static bool MustQuote(@string s) {
    foreach (var (_, r) in s) {
        switch (r) {
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
                if (len(s) > 1) {
                    return true;
                }
                break;
            default: 
                if (!unicode.IsPrint(r)) {
                    return true;
                }
                break;
        }
    }    return s == "" || strings.Contains(s, "//") || strings.Contains(s, "/*");
}

// AutoQuote returns s or, if quoting is required for s to appear in a go.mod,
// the quotation of s.
public static @string AutoQuote(@string s) {
    if (MustQuote(s)) {
        return strconv.Quote(s);
    }
    return s;
}

private static (VersionInterval, error) parseVersionInterval(@string verb, @string path, ptr<slice<@string>> _addr_args, VersionFixer fix) {
    VersionInterval _p0 = default;
    error _p0 = default!;
    ref slice<@string> args = ref _addr_args.val;

    slice<@string> toks = args;
    if (len(toks) == 0 || toks[0] == "(") {
        return (new VersionInterval(), error.As(fmt.Errorf("expected '[' or version"))!);
    }
    if (toks[0] != "[") {
        var (v, err) = parseVersion(verb, path, _addr_toks[0], fix);
        if (err != null) {
            return (new VersionInterval(), error.As(err)!);
        }
        args = toks[(int)1..];
        return (new VersionInterval(Low:v,High:v), error.As(null!)!);
    }
    toks = toks[(int)1..];

    if (len(toks) == 0) {
        return (new VersionInterval(), error.As(fmt.Errorf("expected version after '['"))!);
    }
    var (low, err) = parseVersion(verb, path, _addr_toks[0], fix);
    if (err != null) {
        return (new VersionInterval(), error.As(err)!);
    }
    toks = toks[(int)1..];

    if (len(toks) == 0 || toks[0] != ",") {
        return (new VersionInterval(), error.As(fmt.Errorf("expected ',' after version"))!);
    }
    toks = toks[(int)1..];

    if (len(toks) == 0) {
        return (new VersionInterval(), error.As(fmt.Errorf("expected version after ','"))!);
    }
    var (high, err) = parseVersion(verb, path, _addr_toks[0], fix);
    if (err != null) {
        return (new VersionInterval(), error.As(err)!);
    }
    toks = toks[(int)1..];

    if (len(toks) == 0 || toks[0] != "]") {
        return (new VersionInterval(), error.As(fmt.Errorf("expected ']' after version"))!);
    }
    toks = toks[(int)1..];

    args = toks;
    return (new VersionInterval(Low:low,High:high), error.As(null!)!);
}

private static (@string, error) parseString(ptr<@string> _addr_s) {
    @string _p0 = default;
    error _p0 = default!;
    ref @string s = ref _addr_s.val;

    @string t = s;
    if (strings.HasPrefix(t, "\"")) {
        error err = default!;
        t, err = strconv.Unquote(t);

        if (err != null) {
            return ("", error.As(err)!);
        }
    }
    else if (strings.ContainsAny(t, "\"'`")) { 
        // Other quotes are reserved both for possible future expansion
        // and to avoid confusion. For example if someone types 'x'
        // we want that to be a syntax error and not a literal x in literal quotation marks.
        return ("", error.As(fmt.Errorf("unquoted string cannot contain quote"))!);
    }
    s = AutoQuote(t);
    return (t, error.As(null!)!);
}

private static var deprecatedRE = lazyregexp.New("(?s)(?:^|\\n\\n)Deprecated: *(.*?)(?:$|\\n\\n)");

// parseDeprecation extracts the text of comments on a "module" directive and
// extracts a deprecation message from that.
//
// A deprecation message is contained in a paragraph within a block of comments
// that starts with "Deprecated:" (case sensitive). The message runs until the
// end of the paragraph and does not include the "Deprecated:" prefix. If the
// comment block has multiple paragraphs that start with "Deprecated:",
// parseDeprecation returns the message from the first.
private static @string parseDeprecation(ptr<LineBlock> _addr_block, ptr<Line> _addr_line) {
    ref LineBlock block = ref _addr_block.val;
    ref Line line = ref _addr_line.val;

    var text = parseDirectiveComment(_addr_block, _addr_line);
    var m = deprecatedRE.FindStringSubmatch(text);
    if (m == null) {
        return "";
    }
    return m[1];
}

// parseDirectiveComment extracts the text of comments on a directive.
// If the directive's line does not have comments and is part of a block that
// does have comments, the block's comments are used.
private static @string parseDirectiveComment(ptr<LineBlock> _addr_block, ptr<Line> _addr_line) {
    ref LineBlock block = ref _addr_block.val;
    ref Line line = ref _addr_line.val;

    var comments = line.Comment();
    if (block != null && len(comments.Before) == 0 && len(comments.Suffix) == 0) {
        comments = block.Comment();
    }
    slice<Comment> groups = new slice<slice<Comment>>(new slice<Comment>[] { comments.Before, comments.Suffix });
    slice<@string> lines = default;
    foreach (var (_, g) in groups) {
        foreach (var (_, c) in g) {
            if (!strings.HasPrefix(c.Token, "//")) {
                continue; // blank line
            }
            lines = append(lines, strings.TrimSpace(strings.TrimPrefix(c.Token, "//")));
        }
    }    return strings.Join(lines, "\n");
}

public partial struct ErrorList { // : slice<Error>
}

public static @string Error(this ErrorList e) {
    var errStrs = make_slice<@string>(len(e));
    foreach (var (i, err) in e) {
        errStrs[i] = err.Error();
    }    return strings.Join(errStrs, "\n");
}

public partial struct Error {
    public @string Filename;
    public Position Pos;
    public @string Verb;
    public @string ModPath;
    public error Err;
}

private static @string Error(this ptr<Error> _addr_e) {
    ref Error e = ref _addr_e.val;

    @string pos = default;
    if (e.Pos.LineRune > 1) { 
        // Don't print LineRune if it's 1 (beginning of line).
        // It's always 1 except in scanner errors, which are rare.
        pos = fmt.Sprintf("%s:%d:%d: ", e.Filename, e.Pos.Line, e.Pos.LineRune);
    }
    else if (e.Pos.Line > 0) {
        pos = fmt.Sprintf("%s:%d: ", e.Filename, e.Pos.Line);
    }
    else if (e.Filename != "") {
        pos = fmt.Sprintf("%s: ", e.Filename);
    }
    @string directive = default;
    if (e.ModPath != "") {
        directive = fmt.Sprintf("%s %s: ", e.Verb, e.ModPath);
    }
    else if (e.Verb != "") {
        directive = fmt.Sprintf("%s: ", e.Verb);
    }
    return pos + directive + e.Err.Error();
}

private static error Unwrap(this ptr<Error> _addr_e) {
    ref Error e = ref _addr_e.val;

    return error.As(e.Err)!;
}

private static (@string, error) parseVersion(@string verb, @string path, ptr<@string> _addr_s, VersionFixer fix) {
    @string _p0 = default;
    error _p0 = default!;
    ref @string s = ref _addr_s.val;

    var (t, err) = parseString(_addr_s);
    if (err != null) {
        return ("", error.As(addr(new Error(Verb:verb,ModPath:path,Err:&module.InvalidVersionError{Version:*s,Err:err,},))!)!);
    }
    if (fix != null) {
        var (fixed, err) = fix(path, t);
        if (err != null) {
            {
                ptr<module.ModuleError> (err, ok) = err._<ptr<module.ModuleError>>();

                if (ok) {
                    return ("", error.As(addr(new Error(Verb:verb,ModPath:path,Err:err.Err,))!)!);
                }

            }
            return ("", error.As(err)!);
        }
        t = fixed;
    }
    else
 {
        var cv = module.CanonicalVersion(t);
        if (cv == "") {
            return ("", error.As(addr(new Error(Verb:verb,ModPath:path,Err:&module.InvalidVersionError{Version:t,Err:errors.New("must be of the form v1.2.3"),},))!)!);
        }
        t = cv;
    }
    s = t;
    return (s, error.As(null!)!);
}

private static (@string, error) modulePathMajor(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    var (_, major, ok) = module.SplitPathVersion(path);
    if (!ok) {
        return ("", error.As(fmt.Errorf("invalid module path"))!);
    }
    return (major, error.As(null!)!);
}

private static (slice<byte>, error) Format(this ptr<File> _addr_f) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return (Format(f.Syntax), error.As(null!)!);
}

// Cleanup cleans up the file f after any edit operations.
// To avoid quadratic behavior, modifications like DropRequire
// clear the entry but do not remove it from the slice.
// Cleanup cleans out all the cleared entries.
private static void Cleanup(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    nint w = 0;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in f.Require) {
            r = __r;
            if (r.Mod.Path != "") {
                f.Require[w] = r;
                w++;
            }
        }
        r = r__prev1;
    }

    f.Require = f.Require[..(int)w];

    w = 0;
    foreach (var (_, x) in f.Exclude) {
        if (x.Mod.Path != "") {
            f.Exclude[w] = x;
            w++;
        }
    }    f.Exclude = f.Exclude[..(int)w];

    w = 0;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in f.Replace) {
            r = __r;
            if (r.Old.Path != "") {
                f.Replace[w] = r;
                w++;
            }
        }
        r = r__prev1;
    }

    f.Replace = f.Replace[..(int)w];

    w = 0;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in f.Retract) {
            r = __r;
            if (r.Low != "" || r.High != "") {
                f.Retract[w] = r;
                w++;
            }
        }
        r = r__prev1;
    }

    f.Retract = f.Retract[..(int)w];

    f.Syntax.Cleanup();
}

private static error AddGoStmt(this ptr<File> _addr_f, @string version) {
    ref File f = ref _addr_f.val;

    if (!GoVersionRE.MatchString(version)) {
        return error.As(fmt.Errorf("invalid language version string %q", version))!;
    }
    if (f.Go == null) {
        Expr hint = default;
        if (f.Module != null && f.Module.Syntax != null) {
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

// AddRequire sets the first require line for path to version vers,
// preserving any existing comments for that line and removing all
// other lines for path.
//
// If no line currently exists for path, AddRequire adds a new line
// at the end of the last require block.
private static error AddRequire(this ptr<File> _addr_f, @string path, @string vers) {
    ref File f = ref _addr_f.val;

    var need = true;
    foreach (var (_, r) in f.Require) {
        if (r.Mod.Path == path) {
            if (need) {
                r.Mod.Version = vers;
                f.Syntax.updateLine(r.Syntax, "require", AutoQuote(path), vers);
                need = false;
            }
            else
 {
                r.Syntax.markRemoved();
                r.val = new Require();
            }
        }
    }    if (need) {
        f.AddNewRequire(path, vers, false);
    }
    return error.As(null!)!;
}

// AddNewRequire adds a new require line for path at version vers at the end of
// the last require block, regardless of any existing require lines for path.
private static void AddNewRequire(this ptr<File> _addr_f, @string path, @string vers, bool indirect) {
    ref File f = ref _addr_f.val;

    var line = f.Syntax.addLine(null, "require", AutoQuote(path), vers);
    ptr<Require> r = addr(new Require(Mod:module.Version{Path:path,Version:vers},Syntax:line,));
    r.setIndirect(indirect);
    f.Require = append(f.Require, r);
}

// SetRequire updates the requirements of f to contain exactly req, preserving
// the existing block structure and line comment contents (except for 'indirect'
// markings) for the first requirement on each named module path.
//
// The Syntax field is ignored for the requirements in req.
//
// Any requirements not already present in the file are added to the block
// containing the last require line.
//
// The requirements in req must specify at most one distinct version for each
// module path.
//
// If any existing requirements may be removed, the caller should call Cleanup
// after all edits are complete.
private static void SetRequire(this ptr<File> _addr_f, slice<ptr<Require>> req) => func((_, panic, _) => {
    ref File f = ref _addr_f.val;

    private partial struct elem {
        public @string version;
        public bool indirect;
    }
    var need = make_map<@string, elem>();
    {
        var r__prev1 = r;

        foreach (var (_, __r) in req) {
            r = __r;
            {
                var (prev, dup) = need[r.Mod.Path];

                if (dup && prev.version != r.Mod.Version) {
                    panic(fmt.Errorf("SetRequire called with conflicting versions for path %s (%s and %s)", r.Mod.Path, prev.version, r.Mod.Version));
                }

            }
            need[r.Mod.Path] = new elem(r.Mod.Version,r.Indirect);
        }
        r = r__prev1;
    }

    {
        var r__prev1 = r;

        foreach (var (_, __r) in f.Require) {
            r = __r;
            var (e, ok) = need[r.Mod.Path];
            if (ok) {
                r.setVersion(e.version);
                r.setIndirect(e.indirect);
            }
            else
 {
                r.markRemoved();
            }
            delete(need, r.Mod.Path);
        }
        r = r__prev1;
    }

    {
        var e__prev1 = e;

        foreach (var (__path, __e) in need) {
            path = __path;
            e = __e;
            f.AddNewRequire(path, e.version, e.indirect);
        }
        e = e__prev1;
    }

    f.SortBlocks();
});

// SetRequireSeparateIndirect updates the requirements of f to contain the given
// requirements. Comment contents (except for 'indirect' markings) are retained
// from the first existing requirement for each module path. Like SetRequire,
// SetRequireSeparateIndirect adds requirements for new paths in req,
// updates the version and "// indirect" comment on existing requirements,
// and deletes requirements on paths not in req. Existing duplicate requirements
// are deleted.
//
// As its name suggests, SetRequireSeparateIndirect puts direct and indirect
// requirements into two separate blocks, one containing only direct
// requirements, and the other containing only indirect requirements.
// SetRequireSeparateIndirect may move requirements between these two blocks
// when their indirect markings change. However, SetRequireSeparateIndirect
// won't move requirements from other blocks, especially blocks with comments.
//
// If the file initially has one uncommented block of requirements,
// SetRequireSeparateIndirect will split it into a direct-only and indirect-only
// block. This aids in the transition to separate blocks.
private static void SetRequireSeparateIndirect(this ptr<File> _addr_f, slice<ptr<Require>> req) => func((_, panic, _) => {
    ref File f = ref _addr_f.val;
 
    // hasComments returns whether a line or block has comments
    // other than "indirect".
    Func<Comments, bool> hasComments = c => len(c.Before) > 0 || len(c.After) > 0 || len(c.Suffix) > 1 || (len(c.Suffix) == 1 && strings.TrimSpace(strings.TrimPrefix(c.Suffix[0].Token, string(slashSlash))) != "indirect"); 

    // moveReq adds r to block. If r was in another block, moveReq deletes
    // it from that block and transfers its comments.
    Action<ptr<Require>, ptr<LineBlock>> moveReq = (r, block) => {
        ptr<Line> line;
        if (r.Syntax == null) {
            line = addr(new Line(Token:[]string{AutoQuote(r.Mod.Path),r.Mod.Version}));
            r.Syntax = line;
            if (r.Indirect) {
                r.setIndirect(true);
            }
        }
        else
 {
            line = @new<Line>();
            line.val = r.Syntax.val;
            if (!line.InBlock && len(line.Token) > 0 && line.Token[0] == "require") {
                line.Token = line.Token[(int)1..];
            }
            r.Syntax.Token = null; // Cleanup will delete the old line.
            r.Syntax = line;
        }
        line.InBlock = true;
        block.Line = append(block.Line, line);
    }; 

    // Examine existing require lines and blocks.
 
    // We may insert new requirements into the last uncommented
    // direct-only and indirect-only blocks. We may also move requirements
    // to the opposite block if their indirect markings change.
    nint lastDirectIndex = -1;    nint lastIndirectIndex = -1;    nint lastRequireIndex = -1;    nint requireLineOrBlockCount = 0;    var lineToBlock = make_map<ptr<Line>, ptr<LineBlock>>();
    {
        var stmt__prev1 = stmt;

        foreach (var (__i, __stmt) in f.Syntax.Stmt) {
            i = __i;
            stmt = __stmt;
            switch (stmt.type()) {
                case ptr<Line> stmt:
                    if (len(stmt.Token) == 0 || stmt.Token[0] != "require") {
                        continue;
                    }
                    lastRequireIndex = i;
                    requireLineOrBlockCount++;
                    if (!hasComments(stmt.Comments)) {
                        if (isIndirect(_addr_stmt)) {
                            lastIndirectIndex = i;
                        }
                        else
 {
                            lastDirectIndex = i;
                        }
                    }
                    break;
                case ptr<LineBlock> stmt:
                    if (len(stmt.Token) == 0 || stmt.Token[0] != "require") {
                        continue;
                    }
                    lastRequireIndex = i;
                    requireLineOrBlockCount++;
                    var allDirect = len(stmt.Line) > 0 && !hasComments(stmt.Comments);
                    var allIndirect = len(stmt.Line) > 0 && !hasComments(stmt.Comments);
                    {
                        ptr<Line> line__prev2 = line;

                        foreach (var (_, __line) in stmt.Line) {
                            line = __line;
                            lineToBlock[line] = stmt;
                            if (hasComments(line.Comments)) {
                                allDirect = false;
                                allIndirect = false;
                            }
                            else if (isIndirect(line)) {
                                allDirect = false;
                            }
                            else
 {
                                allIndirect = false;
                            }
                        }

                        line = line__prev2;
                    }

                    if (allDirect) {
                        lastDirectIndex = i;
                    }
                    if (allIndirect) {
                        lastIndirectIndex = i;
                    }
                    break;
            }
        }
        stmt = stmt__prev1;
    }

    var oneFlatUncommentedBlock = requireLineOrBlockCount == 1 && !hasComments(f.Syntax.Stmt[lastRequireIndex].Comment().val); 

    // Create direct and indirect blocks if needed. Convert lines into blocks
    // if needed. If we end up with an empty block or a one-line block,
    // Cleanup will delete it or convert it to a line later.
    Func<nint, ptr<LineBlock>> insertBlock = i => {
        ptr<LineBlock> block = addr(new LineBlock(Token:[]string{"require"}));
        f.Syntax.Stmt = append(f.Syntax.Stmt, null);
        copy(f.Syntax.Stmt[(int)i + 1..], f.Syntax.Stmt[(int)i..]);
        f.Syntax.Stmt[i] = block;
        return block;
    };

    Func<nint, ptr<LineBlock>> ensureBlock = i => {
        switch (f.Syntax.Stmt[i].type()) {
            case ptr<LineBlock> stmt:
                return stmt;
                break;
            case ptr<Line> stmt:
                block = addr(new LineBlock(Token:[]string{"require"},Line:[]*Line{stmt},));
                stmt.Token = stmt.Token[(int)1..]; // remove "require"
                stmt.InBlock = true;
                f.Syntax.Stmt[i] = block;
                return block;
                break;
            default:
            {
                var stmt = f.Syntax.Stmt[i].type();
                panic(fmt.Sprintf("unexpected statement: %v", stmt));
                break;
            }
        }
    };

    ptr<LineBlock> lastDirectBlock;
    if (lastDirectIndex < 0) {
        if (lastIndirectIndex >= 0) {
            lastDirectIndex = lastIndirectIndex;
            lastIndirectIndex++;
        }
        else if (lastRequireIndex >= 0) {
            lastDirectIndex = lastRequireIndex + 1;
        }
        else
 {
            lastDirectIndex = len(f.Syntax.Stmt);
        }
        lastDirectBlock = insertBlock(lastDirectIndex);
    }
    else
 {
        lastDirectBlock = ensureBlock(lastDirectIndex);
    }
    ptr<LineBlock> lastIndirectBlock;
    if (lastIndirectIndex < 0) {
        lastIndirectIndex = lastDirectIndex + 1;
        lastIndirectBlock = insertBlock(lastIndirectIndex);
    }
    else
 {
        lastIndirectBlock = ensureBlock(lastIndirectIndex);
    }
    var need = make_map<@string, ptr<Require>>();
    {
        var r__prev1 = r;

        foreach (var (_, __r) in req) {
            r = __r;
            need[r.Mod.Path] = r;
        }
        r = r__prev1;
    }

    var have = make_map<@string, ptr<Require>>();
    {
        var r__prev1 = r;

        foreach (var (_, __r) in f.Require) {
            r = __r;
            var path = r.Mod.Path;
            if (need[path] == null || have[path] != null) { 
                // Requirement not needed, or duplicate requirement. Delete.
                r.markRemoved();
                continue;
            }
            have[r.Mod.Path] = r;
            r.setVersion(need[path].Mod.Version);
            r.setIndirect(need[path].Indirect);
            if (need[path].Indirect && (oneFlatUncommentedBlock || lineToBlock[r.Syntax] == lastDirectBlock)) {
                moveReq(r, lastIndirectBlock);
            }
            else if (!need[path].Indirect && (oneFlatUncommentedBlock || lineToBlock[r.Syntax] == lastIndirectBlock)) {
                moveReq(r, lastDirectBlock);
            }
        }
        r = r__prev1;
    }

    {
        var path__prev1 = path;
        var r__prev1 = r;

        foreach (var (__path, __r) in need) {
            path = __path;
            r = __r;
            if (have[path] == null) {
                if (r.Indirect) {
                    moveReq(r, lastIndirectBlock);
                }
                else
 {
                    moveReq(r, lastDirectBlock);
                }
                f.Require = append(f.Require, r);
            }
        }
        path = path__prev1;
        r = r__prev1;
    }

    f.SortBlocks();
});

private static error DropRequire(this ptr<File> _addr_f, @string path) {
    ref File f = ref _addr_f.val;

    foreach (var (_, r) in f.Require) {
        if (r.Mod.Path == path) {
            r.Syntax.markRemoved();
            r.val = new Require();
        }
    }    return error.As(null!)!;
}

// AddExclude adds a exclude statement to the mod file. Errors if the provided
// version is not a canonical version string
private static error AddExclude(this ptr<File> _addr_f, @string path, @string vers) {
    ref File f = ref _addr_f.val;

    {
        var err = checkCanonicalVersion(path, vers);

        if (err != null) {
            return error.As(err)!;
        }
    }

    ptr<Line> hint;
    foreach (var (_, x) in f.Exclude) {
        if (x.Mod.Path == path && x.Mod.Version == vers) {
            return error.As(null!)!;
        }
        if (x.Mod.Path == path) {
            hint = x.Syntax;
        }
    }    f.Exclude = append(f.Exclude, addr(new Exclude(Mod:module.Version{Path:path,Version:vers},Syntax:f.Syntax.addLine(hint,"exclude",AutoQuote(path),vers))));
    return error.As(null!)!;
}

private static error DropExclude(this ptr<File> _addr_f, @string path, @string vers) {
    ref File f = ref _addr_f.val;

    foreach (var (_, x) in f.Exclude) {
        if (x.Mod.Path == path && x.Mod.Version == vers) {
            x.Syntax.markRemoved();
            x.val = new Exclude();
        }
    }    return error.As(null!)!;
}

private static error AddReplace(this ptr<File> _addr_f, @string oldPath, @string oldVers, @string newPath, @string newVers) {
    ref File f = ref _addr_f.val;

    var need = true;
    module.Version old = new module.Version(Path:oldPath,Version:oldVers);
    module.Version @new = new module.Version(Path:newPath,Version:newVers);
    @string tokens = new slice<@string>(new @string[] { "replace", AutoQuote(oldPath) });
    if (oldVers != "") {
        tokens = append(tokens, oldVers);
    }
    tokens = append(tokens, "=>", AutoQuote(newPath));
    if (newVers != "") {
        tokens = append(tokens, newVers);
    }
    ptr<Line> hint;
    foreach (var (_, r) in f.Replace) {
        if (r.Old.Path == oldPath && (oldVers == "" || r.Old.Version == oldVers)) {
            if (need) { 
                // Found replacement for old; update to use new.
                r.New = new;
                f.Syntax.updateLine(r.Syntax, tokens);
                need = false;
                continue;
            } 
            // Already added; delete other replacements for same.
            r.Syntax.markRemoved();
            r.val = new Replace();
        }
        if (r.Old.Path == oldPath) {
            hint = r.Syntax;
        }
    }    if (need) {
        f.Replace = append(f.Replace, addr(new Replace(Old:old,New:new,Syntax:f.Syntax.addLine(hint,tokens...))));
    }
    return error.As(null!)!;
}

private static error DropReplace(this ptr<File> _addr_f, @string oldPath, @string oldVers) {
    ref File f = ref _addr_f.val;

    foreach (var (_, r) in f.Replace) {
        if (r.Old.Path == oldPath && r.Old.Version == oldVers) {
            r.Syntax.markRemoved();
            r.val = new Replace();
        }
    }    return error.As(null!)!;
}

// AddRetract adds a retract statement to the mod file. Errors if the provided
// version interval does not consist of canonical version strings
private static error AddRetract(this ptr<File> _addr_f, VersionInterval vi, @string rationale) {
    ref File f = ref _addr_f.val;

    @string path = default;
    if (f.Module != null) {
        path = f.Module.Mod.Path;
    }
    {
        var err__prev1 = err;

        var err = checkCanonicalVersion(path, vi.High);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    {
        var err__prev1 = err;

        err = checkCanonicalVersion(path, vi.Low);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    ptr<Retract> r = addr(new Retract(VersionInterval:vi,));
    if (vi.Low == vi.High) {
        r.Syntax = f.Syntax.addLine(null, "retract", AutoQuote(vi.Low));
    }
    else
 {
        r.Syntax = f.Syntax.addLine(null, "retract", "[", AutoQuote(vi.Low), ",", AutoQuote(vi.High), "]");
    }
    if (rationale != "") {
        foreach (var (_, line) in strings.Split(rationale, "\n")) {
            Comment com = new Comment(Token:"// "+line);
            r.Syntax.Comment().Before = append(r.Syntax.Comment().Before, com);
        }
    }
    return error.As(null!)!;
}

private static error DropRetract(this ptr<File> _addr_f, VersionInterval vi) {
    ref File f = ref _addr_f.val;

    foreach (var (_, r) in f.Retract) {
        if (r.VersionInterval == vi) {
            r.Syntax.markRemoved();
            r.val = new Retract();
        }
    }    return error.As(null!)!;
}

private static void SortBlocks(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    f.removeDups(); // otherwise sorting is unsafe

    foreach (var (_, stmt) in f.Syntax.Stmt) {
        ptr<LineBlock> (block, ok) = stmt._<ptr<LineBlock>>();
        if (!ok) {
            continue;
        }
        var less = lineLess;
        if (block.Token[0] == "retract") {
            less = lineRetractLess;
        }
        sort.SliceStable(block.Line, (i, j) => less(block.Line[i], block.Line[j]));
    }
}

// removeDups removes duplicate exclude and replace directives.
//
// Earlier exclude directives take priority.
//
// Later replace directives take priority.
//
// require directives are not de-duplicated. That's left up to higher-level
// logic (MVS).
//
// retract directives are not de-duplicated since comments are
// meaningful, and versions may be retracted multiple times.
private static void removeDups(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    var kill = make_map<ptr<Line>, bool>(); 

    // Remove duplicate excludes.
    var haveExclude = make_map<module.Version, bool>();
    {
        var x__prev1 = x;

        foreach (var (_, __x) in f.Exclude) {
            x = __x;
            if (haveExclude[x.Mod]) {
                kill[x.Syntax] = true;
                continue;
            }
            haveExclude[x.Mod] = true;
        }
        x = x__prev1;
    }

    slice<ptr<Exclude>> excl = default;
    {
        var x__prev1 = x;

        foreach (var (_, __x) in f.Exclude) {
            x = __x;
            if (!kill[x.Syntax]) {
                excl = append(excl, x);
            }
        }
        x = x__prev1;
    }

    f.Exclude = excl; 

    // Remove duplicate replacements.
    // Later replacements take priority over earlier ones.
    var haveReplace = make_map<module.Version, bool>();
    for (var i = len(f.Replace) - 1; i >= 0; i--) {
        var x = f.Replace[i];
        if (haveReplace[x.Old]) {
            kill[x.Syntax] = true;
            continue;
        }
        haveReplace[x.Old] = true;
    }
    slice<ptr<Replace>> repl = default;
    {
        var x__prev1 = x;

        foreach (var (_, __x) in f.Replace) {
            x = __x;
            if (!kill[x.Syntax]) {
                repl = append(repl, x);
            }
        }
        x = x__prev1;
    }

    f.Replace = repl; 

    // Duplicate require and retract directives are not removed.

    // Drop killed statements from the syntax tree.
    slice<Expr> stmts = default;
    {
        var stmt__prev1 = stmt;

        foreach (var (_, __stmt) in f.Syntax.Stmt) {
            stmt = __stmt;
            switch (stmt.type()) {
                case ptr<Line> stmt:
                    if (kill[stmt]) {
                        continue;
                    }
                    break;
                case ptr<LineBlock> stmt:
                    slice<ptr<Line>> lines = default;
                    foreach (var (_, line) in stmt.Line) {
                        if (!kill[line]) {
                            lines = append(lines, line);
                        }
                    }
                    stmt.Line = lines;
                    if (len(lines) == 0) {
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

// lineLess returns whether li should be sorted before lj. It sorts
// lexicographically without assigning any special meaning to tokens.
private static bool lineLess(ptr<Line> _addr_li, ptr<Line> _addr_lj) {
    ref Line li = ref _addr_li.val;
    ref Line lj = ref _addr_lj.val;

    for (nint k = 0; k < len(li.Token) && k < len(lj.Token); k++) {
        if (li.Token[k] != lj.Token[k]) {
            return li.Token[k] < lj.Token[k];
        }
    }
    return len(li.Token) < len(lj.Token);
}

// lineRetractLess returns whether li should be sorted before lj for lines in
// a "retract" block. It treats each line as a version interval. Single versions
// are compared as if they were intervals with the same low and high version.
// Intervals are sorted in descending order, first by low version, then by
// high version, using semver.Compare.
private static bool lineRetractLess(ptr<Line> _addr_li, ptr<Line> _addr_lj) {
    ref Line li = ref _addr_li.val;
    ref Line lj = ref _addr_lj.val;

    Func<ptr<Line>, VersionInterval> interval = l => {
        if (len(l.Token) == 1) {
            return new VersionInterval(Low:l.Token[0],High:l.Token[0]);
        }
        else if (len(l.Token) == 5 && l.Token[0] == "[" && l.Token[2] == "," && l.Token[4] == "]") {
            return new VersionInterval(Low:l.Token[1],High:l.Token[3]);
        }
        else
 { 
            // Line in unknown format. Treat as an invalid version.
            return new VersionInterval();
        }
    };
    var vii = interval(li);
    var vij = interval(lj);
    {
        var cmp = semver.Compare(vii.Low, vij.Low);

        if (cmp != 0) {
            return cmp > 0;
        }
    }
    return semver.Compare(vii.High, vij.High) > 0;
}

// checkCanonicalVersion returns a non-nil error if vers is not a canonical
// version string or does not match the major version of path.
//
// If path is non-empty, the error text suggests a format with a major version
// corresponding to the path.
private static error checkCanonicalVersion(@string path, @string vers) {
    var (_, pathMajor, pathMajorOk) = module.SplitPathVersion(path);

    if (vers == "" || vers != module.CanonicalVersion(vers)) {
        if (pathMajor == "") {
            return error.As(addr(new module.InvalidVersionError(Version:vers,Err:fmt.Errorf("must be of the form v1.2.3"),))!)!;
        }
        return error.As(addr(new module.InvalidVersionError(Version:vers,Err:fmt.Errorf("must be of the form %s.2.3",module.PathMajorPrefix(pathMajor)),))!)!;
    }
    if (pathMajorOk) {
        {
            var err = module.CheckPathMajor(vers, pathMajor);

            if (err != null) {
                if (pathMajor == "") { 
                    // In this context, the user probably wrote "v2.3.4" when they meant
                    // "v2.3.4+incompatible". Suggest that instead of "v0 or v1".
                    return error.As(addr(new module.InvalidVersionError(Version:vers,Err:fmt.Errorf("should be %s+incompatible (or module %s/%v)",vers,path,semver.Major(vers)),))!)!;
                }
                return error.As(err)!;
            }

        }
    }
    return error.As(null!)!;
}

} // end modfile_package
