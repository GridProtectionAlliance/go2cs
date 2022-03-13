// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package search -- go2cs converted at 2022 March 13 06:30:08 UTC
// import "cmd/go/internal/search" ==> using search = go.cmd.go.@internal.search_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\search\search.go
namespace go.cmd.go.@internal;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using fsys = cmd.go.@internal.fsys_package;
using fmt = fmt_package;
using build = go.build_package;
using fs = io.fs_package;
using os = os_package;
using path = path_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strings = strings_package;


// A Match represents the result of matching a single package pattern.

using System;
public static partial class search_package {

public partial struct Match {
    public @string pattern; // the pattern itself
    public slice<@string> Dirs; // if the pattern is local, directories that potentially contain matching packages
    public slice<@string> Pkgs; // matching packages (import paths)
    public slice<error> Errs; // errors matching the patterns to packages, NOT errors loading those packages

// Errs may be non-empty even if len(Pkgs) > 0, indicating that some matching
// packages could be located but results may be incomplete.
// If len(Pkgs) == 0 && len(Errs) == 0, the pattern is well-formed but did not
// match any packages.
}

// NewMatch returns a Match describing the given pattern,
// without resolving its packages or errors.
public static ptr<Match> NewMatch(@string pattern) {
    return addr(new Match(pattern:pattern));
}

// Pattern returns the pattern to be matched.
private static @string Pattern(this ptr<Match> _addr_m) {
    ref Match m = ref _addr_m.val;

    return m.pattern;
}

// AddError appends a MatchError wrapping err to m.Errs.
private static void AddError(this ptr<Match> _addr_m, error err) {
    ref Match m = ref _addr_m.val;

    m.Errs = append(m.Errs, addr(new MatchError(Match:m,Err:err)));
}

// Literal reports whether the pattern is free of wildcards and meta-patterns.
//
// A literal pattern must match at most one package.
private static bool IsLiteral(this ptr<Match> _addr_m) {
    ref Match m = ref _addr_m.val;

    return !strings.Contains(m.pattern, "...") && !m.IsMeta();
}

// Local reports whether the pattern must be resolved from a specific root or
// directory, such as a filesystem path or a single module.
private static bool IsLocal(this ptr<Match> _addr_m) {
    ref Match m = ref _addr_m.val;

    return build.IsLocalImport(m.pattern) || filepath.IsAbs(m.pattern);
}

// Meta reports whether the pattern is a “meta-package” keyword that represents
// multiple packages, such as "std", "cmd", or "all".
private static bool IsMeta(this ptr<Match> _addr_m) {
    ref Match m = ref _addr_m.val;

    return IsMetaPackage(m.pattern);
}

// IsMetaPackage checks if name is a reserved package name that expands to multiple packages.
public static bool IsMetaPackage(@string name) {
    return name == "std" || name == "cmd" || name == "all";
}

// A MatchError indicates an error that occurred while attempting to match a
// pattern.
public partial struct MatchError {
    public ptr<Match> Match;
    public error Err;
}

private static @string Error(this ptr<MatchError> _addr_e) {
    ref MatchError e = ref _addr_e.val;

    if (e.Match.IsLiteral()) {
        return fmt.Sprintf("%s: %v", e.Match.Pattern(), e.Err);
    }
    return fmt.Sprintf("pattern %s: %v", e.Match.Pattern(), e.Err);
}

private static error Unwrap(this ptr<MatchError> _addr_e) {
    ref MatchError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// MatchPackages sets m.Pkgs to a non-nil slice containing all the packages that
// can be found under the $GOPATH directories and $GOROOT that match the
// pattern. The pattern must be either "all" (all packages), "std" (standard
// packages), "cmd" (standard commands), or a path including "...".
//
// If any errors may have caused the set of packages to be incomplete,
// MatchPackages appends those errors to m.Errs.
private static void MatchPackages(this ptr<Match> _addr_m) {
    ref Match m = ref _addr_m.val;

    m.Pkgs = new slice<@string>(new @string[] {  });
    if (m.IsLocal()) {
        m.AddError(fmt.Errorf("internal error: MatchPackages: %s is not a valid package pattern", m.pattern));
        return ;
    }
    if (m.IsLiteral()) {
        m.Pkgs = new slice<@string>(new @string[] { m.pattern });
        return ;
    }
    Func<@string, bool> match = _p0 => true;
    Func<@string, bool> treeCanMatch = _p0 => true;
    if (!m.IsMeta()) {
        match = MatchPattern(m.pattern);
        treeCanMatch = TreeCanMatchPattern(m.pattern);
    }
    map have = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"builtin":true,};
    if (!cfg.BuildContext.CgoEnabled) {
        have["runtime/cgo"] = true; // ignore during walk
    }
    foreach (var (_, src) in cfg.BuildContext.SrcDirs()) {
        if ((m.pattern == "std" || m.pattern == "cmd") && src != cfg.GOROOTsrc) {
            continue;
        }
        src = filepath.Clean(src) + string(filepath.Separator);
        var root = src;
        if (m.pattern == "cmd") {
            root += "cmd" + string(filepath.Separator);
        }
        var err = fsys.Walk(root, (path, fi, err) => {
            if (err != null) {
                return err; // Likely a permission error, which could interfere with matching.
            }
            if (path == src) {
                return null; // GOROOT/src and GOPATH/src cannot contain packages.
            }
            var want = true; 
            // Avoid .foo, _foo, and testdata directory trees.
            var (_, elem) = filepath.Split(path);
            if (strings.HasPrefix(elem, ".") || strings.HasPrefix(elem, "_") || elem == "testdata") {
                want = false;
            }
            var name = filepath.ToSlash(path[(int)len(src)..]);
            if (m.pattern == "std" && (!IsStandardImportPath(name) || name == "cmd")) { 
                // The name "std" is only the standard library.
                // If the name is cmd, it's the root of the command tree.
                want = false;
            }
            if (!treeCanMatch(name)) {
                want = false;
            }
            if (!fi.IsDir()) {
                if (fi.Mode() & fs.ModeSymlink != 0 && want && strings.Contains(m.pattern, "...")) {
                    {
                        var (target, err) = fsys.Stat(path);

                        if (err == null && target.IsDir()) {
                            fmt.Fprintf(os.Stderr, "warning: ignoring symlink %s\n", path);
                        }

                    }
                }
                return null;
            }
            if (!want) {
                return filepath.SkipDir;
            }
            if (have[name]) {
                return null;
            }
            have[name] = true;
            if (!match(name)) {
                return null;
            }
            var (pkg, err) = cfg.BuildContext.ImportDir(path, 0);
            if (err != null) {
                {
                    ptr<build.NoGoError> (_, noGo) = err._<ptr<build.NoGoError>>();

                    if (noGo) { 
                        // The package does not actually exist, so record neither the package
                        // nor the error.
                        return null;
                    } 
                    // There was an error importing path, but not matching it,
                    // which is all that Match promises to do.
                    // Ignore the import error.

                } 
                // There was an error importing path, but not matching it,
                // which is all that Match promises to do.
                // Ignore the import error.
            } 

            // If we are expanding "cmd", skip main
            // packages under cmd/vendor. At least as of
            // March, 2017, there is one there for the
            // vendored pprof tool.
            if (m.pattern == "cmd" && pkg != null && strings.HasPrefix(pkg.ImportPath, "cmd/vendor") && pkg.Name == "main") {
                return null;
            }
            m.Pkgs = append(m.Pkgs, name);
            return null;
        });
        if (err != null) {
            m.AddError(err);
        }
    }
}

private static @string modRoot = default;

public static void SetModRoot(@string dir) {
    modRoot = dir;
}

// MatchDirs sets m.Dirs to a non-nil slice containing all directories that
// potentially match a local pattern. The pattern must begin with an absolute
// path, or "./", or "../". On Windows, the pattern may use slash or backslash
// separators or a mix of both.
//
// If any errors may have caused the set of directories to be incomplete,
// MatchDirs appends those errors to m.Errs.
private static void MatchDirs(this ptr<Match> _addr_m) {
    ref Match m = ref _addr_m.val;

    m.Dirs = new slice<@string>(new @string[] {  });
    if (!m.IsLocal()) {
        m.AddError(fmt.Errorf("internal error: MatchDirs: %s is not a valid filesystem pattern", m.pattern));
        return ;
    }
    if (m.IsLiteral()) {
        m.Dirs = new slice<@string>(new @string[] { m.pattern });
        return ;
    }
    var cleanPattern = filepath.Clean(m.pattern);
    var isLocal = strings.HasPrefix(m.pattern, "./") || (os.PathSeparator == '\\' && strings.HasPrefix(m.pattern, ".\\"));
    @string prefix = "";
    if (cleanPattern != "." && isLocal) {
        prefix = "./";
        cleanPattern = "." + string(os.PathSeparator) + cleanPattern;
    }
    var slashPattern = filepath.ToSlash(cleanPattern);
    var match = MatchPattern(slashPattern); 

    // Find directory to begin the scan.
    // Could be smarter but this one optimization
    // is enough for now, since ... is usually at the
    // end of a path.
    var i = strings.Index(cleanPattern, "...");
    var (dir, _) = filepath.Split(cleanPattern[..(int)i]); 

    // pattern begins with ./ or ../.
    // path.Clean will discard the ./ but not the ../.
    // We need to preserve the ./ for pattern matching
    // and in the returned import paths.

    if (modRoot != "") {
        var (abs, err) = filepath.Abs(dir);
        if (err != null) {
            m.AddError(err);
            return ;
        }
        if (!hasFilepathPrefix(abs, modRoot)) {
            m.AddError(fmt.Errorf("directory %s is outside module root (%s)", abs, modRoot));
            return ;
        }
    }
    var err = fsys.Walk(dir, (path, fi, err) => {
        if (err != null) {
            return err; // Likely a permission error, which could interfere with matching.
        }
        if (!fi.IsDir()) {
            return null;
        }
        var top = false;
        if (path == dir) { 
            // Walk starts at dir and recurses. For the recursive case,
            // the path is the result of filepath.Join, which calls filepath.Clean.
            // The initial case is not Cleaned, though, so we do this explicitly.
            //
            // This converts a path like "./io/" to "io". Without this step, running
            // "cd $GOROOT/src; go list ./io/..." would incorrectly skip the io
            // package, because prepending the prefix "./" to the unclean path would
            // result in "././io", and match("././io") returns false.
            top = true;
            path = filepath.Clean(path);
        }
        var (_, elem) = filepath.Split(path);
        var dot = strings.HasPrefix(elem, ".") && elem != "." && elem != "..";
        if (dot || strings.HasPrefix(elem, "_") || elem == "testdata") {
            return filepath.SkipDir;
        }
        if (!top && cfg.ModulesEnabled) { 
            // Ignore other modules found in subdirectories.
            {
                var (fi, err) = fsys.Stat(filepath.Join(path, "go.mod"));

                if (err == null && !fi.IsDir()) {
                    return filepath.SkipDir;
                }

            }
        }
        var name = prefix + filepath.ToSlash(path);
        if (!match(name)) {
            return null;
        }
        {
            var (p, err) = cfg.BuildContext.ImportDir(path, 0);

            if (err != null && (p == null || len(p.InvalidGoFiles) == 0)) {
                {
                    ptr<build.NoGoError> (_, noGo) = err._<ptr<build.NoGoError>>();

                    if (noGo) { 
                        // The package does not actually exist, so record neither the package
                        // nor the error.
                        return null;
                    } 
                    // There was an error importing path, but not matching it,
                    // which is all that Match promises to do.
                    // Ignore the import error.

                } 
                // There was an error importing path, but not matching it,
                // which is all that Match promises to do.
                // Ignore the import error.
            }

        }
        m.Dirs = append(m.Dirs, name);
        return null;
    });
    if (err != null) {
        m.AddError(err);
    }
}

// TreeCanMatchPattern(pattern)(name) reports whether
// name or children of name can possibly match pattern.
// Pattern is the same limited glob accepted by matchPattern.
public static Func<@string, bool> TreeCanMatchPattern(@string pattern) {
    var wildCard = false;
    {
        var i = strings.Index(pattern, "...");

        if (i >= 0) {
            wildCard = true;
            pattern = pattern[..(int)i];
        }
    }
    return name => len(name) <= len(pattern) && hasPathPrefix(pattern, name) || wildCard && strings.HasPrefix(name, pattern);
}

// MatchPattern(pattern)(name) reports whether
// name matches pattern. Pattern is a limited glob
// pattern in which '...' means 'any string' and there
// is no other special syntax.
// Unfortunately, there are two special cases. Quoting "go help packages":
//
// First, /... at the end of the pattern can match an empty string,
// so that net/... matches both net and packages in its subdirectories, like net/http.
// Second, any slash-separated pattern element containing a wildcard never
// participates in a match of the "vendor" element in the path of a vendored
// package, so that ./... does not match packages in subdirectories of
// ./vendor or ./mycode/vendor, but ./vendor/... and ./mycode/vendor/... do.
// Note, however, that a directory named vendor that itself contains code
// is not a vendored package: cmd/vendor would be a command named vendor,
// and the pattern cmd/... matches it.
public static Func<@string, bool> MatchPattern(@string pattern) { 
    // Convert pattern to regular expression.
    // The strategy for the trailing /... is to nest it in an explicit ? expression.
    // The strategy for the vendor exclusion is to change the unmatchable
    // vendor strings to a disallowed code point (vendorChar) and to use
    // "(anything but that codepoint)*" as the implementation of the ... wildcard.
    // This is a bit complicated but the obvious alternative,
    // namely a hand-written search like in most shell glob matchers,
    // is too easy to make accidentally exponential.
    // Using package regexp guarantees linear-time matching.

    const @string vendorChar = "\x00";



    if (strings.Contains(pattern, vendorChar)) {
        return name => false;
    }
    var re = regexp.QuoteMeta(pattern);
    re = replaceVendor(re, vendorChar);

    if (strings.HasSuffix(re, "/" + vendorChar + "/\\.\\.\\.")) 
        re = strings.TrimSuffix(re, "/" + vendorChar + "/\\.\\.\\.") + "(/vendor|/" + vendorChar + "/\\.\\.\\.)";
    else if (re == vendorChar + "/\\.\\.\\.") 
        re = "(/vendor|/" + vendorChar + "/\\.\\.\\.)";
    else if (strings.HasSuffix(re, "/\\.\\.\\.")) 
        re = strings.TrimSuffix(re, "/\\.\\.\\.") + "(/\\.\\.\\.)?";
        re = strings.ReplaceAll(re, "\\.\\.\\.", "[^" + vendorChar + "]*");

    var reg = regexp.MustCompile("^" + re + "$");

    return name => {
        if (strings.Contains(name, vendorChar)) {
            return false;
        }
        return reg.MatchString(replaceVendor(name, vendorChar));
    };
}

// replaceVendor returns the result of replacing
// non-trailing vendor path elements in x with repl.
private static @string replaceVendor(@string x, @string repl) {
    if (!strings.Contains(x, "vendor")) {
        return x;
    }
    var elem = strings.Split(x, "/");
    for (nint i = 0; i < len(elem) - 1; i++) {
        if (elem[i] == "vendor") {
            elem[i] = repl;
        }
    }
    return strings.Join(elem, "/");
}

// WarnUnmatched warns about patterns that didn't match any packages.
public static void WarnUnmatched(slice<ptr<Match>> matches) {
    foreach (var (_, m) in matches) {
        if (len(m.Pkgs) == 0 && len(m.Errs) == 0) {
            fmt.Fprintf(os.Stderr, "go: warning: %q matched no packages\n", m.pattern);
        }
    }
}

// ImportPaths returns the matching paths to use for the given command line.
// It calls ImportPathsQuiet and then WarnUnmatched.
public static slice<ptr<Match>> ImportPaths(slice<@string> patterns) {
    var matches = ImportPathsQuiet(patterns);
    WarnUnmatched(matches);
    return matches;
}

// ImportPathsQuiet is like ImportPaths but does not warn about patterns with no matches.
public static slice<ptr<Match>> ImportPathsQuiet(slice<@string> patterns) {
    slice<ptr<Match>> @out = default;
    foreach (var (_, a) in CleanPatterns(patterns)) {
        var m = NewMatch(a);
        if (m.IsLocal()) {
            m.MatchDirs(); 

            // Change the file import path to a regular import path if the package
            // is in GOPATH or GOROOT. We don't report errors here; LoadImport
            // (or something similar) will report them later.
            m.Pkgs = make_slice<@string>(len(m.Dirs));
            foreach (var (i, dir) in m.Dirs) {
                var absDir = dir;
                if (!filepath.IsAbs(dir)) {
                    absDir = filepath.Join(@base.Cwd(), dir);
                }
                {
                    var (bp, _) = cfg.BuildContext.ImportDir(absDir, build.FindOnly);

                    if (bp.ImportPath != "" && bp.ImportPath != ".") {
                        m.Pkgs[i] = bp.ImportPath;
                    }
                    else
 {
                        m.Pkgs[i] = dir;
                    }

                }
            }
        else
        } {
            m.MatchPackages();
        }
        out = append(out, m);
    }    return out;
}

// CleanPatterns returns the patterns to use for the given command line. It
// canonicalizes the patterns but does not evaluate any matches. For patterns
// that are not local or absolute paths, it preserves text after '@' to avoid
// modifying version queries.
public static slice<@string> CleanPatterns(slice<@string> patterns) {
    if (len(patterns) == 0) {
        return new slice<@string>(new @string[] { "." });
    }
    slice<@string> @out = default;
    foreach (var (_, a) in patterns) {
        @string p = default;        @string v = default;

        if (build.IsLocalImport(a) || filepath.IsAbs(a)) {
            p = a;
        }        {
            var i = strings.IndexByte(a, '@');


            else if (i < 0) {
                p = a;
            }
            else
 {
                p = a[..(int)i];
                v = a[(int)i..];
            } 

            // Arguments may be either file paths or import paths.
            // As a courtesy to Windows developers, rewrite \ to /
            // in arguments that look like import paths.
            // Don't replace slashes in absolute paths.

        } 

        // Arguments may be either file paths or import paths.
        // As a courtesy to Windows developers, rewrite \ to /
        // in arguments that look like import paths.
        // Don't replace slashes in absolute paths.
        if (filepath.IsAbs(p)) {
            p = filepath.Clean(p);
        }
        else
 {
            if (filepath.Separator == '\\') {
                p = strings.ReplaceAll(p, "\\", "/");
            } 

            // Put argument in canonical form, but preserve leading ./.
            if (strings.HasPrefix(p, "./")) {
                p = "./" + path.Clean(p);
                if (p == "./.") {
                    p = ".";
                }
            }
            else
 {
                p = path.Clean(p);
            }
        }
        out = append(out, p + v);
    }    return out;
}

// hasPathPrefix reports whether the path s begins with the
// elements in prefix.
private static bool hasPathPrefix(@string s, @string prefix) {

    if (len(s) == len(prefix)) 
        return s == prefix;
    else if (len(s) > len(prefix)) 
        if (prefix != "" && prefix[len(prefix) - 1] == '/') {
            return strings.HasPrefix(s, prefix);
        }
        return s[len(prefix)] == '/' && s[..(int)len(prefix)] == prefix;
    else 
        return false;
    }

// hasFilepathPrefix reports whether the path s begins with the
// elements in prefix.
private static bool hasFilepathPrefix(@string s, @string prefix) {

    if (len(s) == len(prefix)) 
        return s == prefix;
    else if (len(s) > len(prefix)) 
        if (prefix != "" && prefix[len(prefix) - 1] == filepath.Separator) {
            return strings.HasPrefix(s, prefix);
        }
        return s[len(prefix)] == filepath.Separator && s[..(int)len(prefix)] == prefix;
    else 
        return false;
    }

// IsStandardImportPath reports whether $GOROOT/src/path should be considered
// part of the standard distribution. For historical reasons we allow people to add
// their own code to $GOROOT instead of using $GOPATH, but we assume that
// code will start with a domain name (dot in the first element).
//
// Note that this function is meant to evaluate whether a directory found in GOROOT
// should be treated as part of the standard library. It should not be used to decide
// that a directory found in GOPATH should be rejected: directories in GOPATH
// need not have dots in the first element, and they just take their chances
// with future collisions in the standard library.
public static bool IsStandardImportPath(@string path) {
    var i = strings.Index(path, "/");
    if (i < 0) {
        i = len(path);
    }
    var elem = path[..(int)i];
    return !strings.Contains(elem, ".");
}

// IsRelativePath reports whether pattern should be interpreted as a directory
// path relative to the current directory, as opposed to a pattern matching
// import paths.
public static bool IsRelativePath(@string pattern) {
    return strings.HasPrefix(pattern, "./") || strings.HasPrefix(pattern, "../") || pattern == "." || pattern == "..";
}

// InDir checks whether path is in the file tree rooted at dir.
// If so, InDir returns an equivalent path relative to dir.
// If not, InDir returns an empty string.
// InDir makes some effort to succeed even in the presence of symbolic links.
public static @string InDir(@string path, @string dir) {
    {
        var rel__prev1 = rel;

        var rel = inDirLex(path, dir);

        if (rel != "") {
            return rel;
        }
        rel = rel__prev1;

    }
    var (xpath, err) = filepath.EvalSymlinks(path);
    if (err != null || xpath == path) {
        xpath = "";
    }
    else
 {
        {
            var rel__prev2 = rel;

            rel = inDirLex(xpath, dir);

            if (rel != "") {
                return rel;
            }

            rel = rel__prev2;

        }
    }
    var (xdir, err) = filepath.EvalSymlinks(dir);
    if (err == null && xdir != dir) {
        {
            var rel__prev2 = rel;

            rel = inDirLex(path, xdir);

            if (rel != "") {
                return rel;
            }

            rel = rel__prev2;

        }
        if (xpath != "") {
            {
                var rel__prev3 = rel;

                rel = inDirLex(xpath, xdir);

                if (rel != "") {
                    return rel;
                }

                rel = rel__prev3;

            }
        }
    }
    return "";
}

// inDirLex is like inDir but only checks the lexical form of the file names.
// It does not consider symbolic links.
// TODO(rsc): This is a copy of str.HasFilePathPrefix, modified to
// return the suffix. Most uses of str.HasFilePathPrefix should probably
// be calling InDir instead.
private static @string inDirLex(@string path, @string dir) {
    var pv = strings.ToUpper(filepath.VolumeName(path));
    var dv = strings.ToUpper(filepath.VolumeName(dir));
    path = path[(int)len(pv)..];
    dir = dir[(int)len(dv)..];

    if (pv != dv) 
        return "";
    else if (len(path) == len(dir)) 
        if (path == dir) {
            return ".";
        }
        return "";
    else if (dir == "") 
        return path;
    else if (len(path) > len(dir)) 
        if (dir[len(dir) - 1] == filepath.Separator) {
            if (path[..(int)len(dir)] == dir) {
                return path[(int)len(dir)..];
            }
            return "";
        }
        if (path[len(dir)] == filepath.Separator && path[..(int)len(dir)] == dir) {
            if (len(path) == len(dir) + 1) {
                return ".";
            }
            return path[(int)len(dir) + 1..];
        }
        return "";
    else 
        return "";
    }

} // end search_package
