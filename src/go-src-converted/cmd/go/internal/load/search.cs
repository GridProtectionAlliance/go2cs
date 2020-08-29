// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2020 August 29 10:01:05 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Go\src\cmd\go\internal\load\search.go
using cfg = go.cmd.go.@internal.cfg_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using log = go.log_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        // allPackages returns all the packages that can be found
        // under the $GOPATH directories and $GOROOT matching pattern.
        // The pattern is either "all" (all packages), "std" (standard packages),
        // "cmd" (standard commands), or a path including "...".
        private static slice<@string> allPackages(@string pattern)
        {
            var pkgs = MatchPackages(pattern);
            if (len(pkgs) == 0L)
            {
                fmt.Fprintf(os.Stderr, "warning: %q matched no packages\n", pattern);
            }
            return pkgs;
        }

        // allPackagesInFS is like allPackages but is passed a pattern
        // beginning ./ or ../, meaning it should scan the tree rooted
        // at the given directory. There are ... in the pattern too.
        private static slice<@string> allPackagesInFS(@string pattern)
        {
            var pkgs = MatchPackagesInFS(pattern);
            if (len(pkgs) == 0L)
            {
                fmt.Fprintf(os.Stderr, "warning: %q matched no packages\n", pattern);
            }
            return pkgs;
        }

        // MatchPackages returns a list of package paths matching pattern
        // (see go help packages for pattern syntax).
        public static slice<@string> MatchPackages(@string pattern)
        {
            Func<@string, bool> match = _p0 => true;
            Func<@string, bool> treeCanMatch = _p0 => true;
            if (!IsMetaPackage(pattern))
            {
                match = matchPattern(pattern);
                treeCanMatch = treeCanMatchPattern(pattern);
            }
            map have = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"builtin":true,};
            if (!cfg.BuildContext.CgoEnabled)
            {
                have["runtime/cgo"] = true; // ignore during walk
            }
            slice<@string> pkgs = default;

            foreach (var (_, src) in cfg.BuildContext.SrcDirs())
            {
                if ((pattern == "std" || pattern == "cmd") && src != cfg.GOROOTsrc)
                {
                    continue;
                }
                src = filepath.Clean(src) + string(filepath.Separator);
                var root = src;
                if (pattern == "cmd")
                {
                    root += "cmd" + string(filepath.Separator);
                }
                filepath.Walk(root, (path, fi, err) =>
                {
                    if (err != null || path == src)
                    {
                        return null;
                    }
                    var want = true; 
                    // Avoid .foo, _foo, and testdata directory trees.
                    var (_, elem) = filepath.Split(path);
                    if (strings.HasPrefix(elem, ".") || strings.HasPrefix(elem, "_") || elem == "testdata")
                    {
                        want = false;
                    }
                    var name = filepath.ToSlash(path[len(src)..]);
                    if (pattern == "std" && (!isStandardImportPath(name) || name == "cmd"))
                    { 
                        // The name "std" is only the standard library.
                        // If the name is cmd, it's the root of the command tree.
                        want = false;
                    }
                    if (!treeCanMatch(name))
                    {
                        want = false;
                    }
                    if (!fi.IsDir())
                    {
                        if (fi.Mode() & os.ModeSymlink != 0L && want)
                        {
                            {
                                var (target, err) = os.Stat(path);

                                if (err == null && target.IsDir())
                                {
                                    fmt.Fprintf(os.Stderr, "warning: ignoring symlink %s\n", path);
                                }

                            }
                        }
                        return null;
                    }
                    if (!want)
                    {
                        return filepath.SkipDir;
                    }
                    if (have[name])
                    {
                        return null;
                    }
                    have[name] = true;
                    if (!match(name))
                    {
                        return null;
                    }
                    var (pkg, err) = cfg.BuildContext.ImportDir(path, 0L);
                    if (err != null)
                    {
                        {
                            ref build.NoGoError (_, noGo) = err._<ref build.NoGoError>();

                            if (noGo)
                            {
                                return null;
                            }

                        }
                    } 

                    // If we are expanding "cmd", skip main
                    // packages under cmd/vendor. At least as of
                    // March, 2017, there is one there for the
                    // vendored pprof tool.
                    if (pattern == "cmd" && strings.HasPrefix(pkg.ImportPath, "cmd/vendor") && pkg.Name == "main")
                    {
                        return null;
                    }
                    pkgs = append(pkgs, name);
                    return null;
                });
            }
            return pkgs;
        }

        // MatchPackagesInFS returns a list of package paths matching pattern,
        // which must begin with ./ or ../
        // (see go help packages for pattern syntax).
        public static slice<@string> MatchPackagesInFS(@string pattern)
        { 
            // Find directory to begin the scan.
            // Could be smarter but this one optimization
            // is enough for now, since ... is usually at the
            // end of a path.
            var i = strings.Index(pattern, "...");
            var (dir, _) = path.Split(pattern[..i]); 

            // pattern begins with ./ or ../.
            // path.Clean will discard the ./ but not the ../.
            // We need to preserve the ./ for pattern matching
            // and in the returned import paths.
            @string prefix = "";
            if (strings.HasPrefix(pattern, "./"))
            {
                prefix = "./";
            }
            var match = matchPattern(pattern);

            slice<@string> pkgs = default;
            filepath.Walk(dir, (path, fi, err) =>
            {
                if (err != null || !fi.IsDir())
                {
                    return null;
                }
                if (path == dir)
                { 
                    // filepath.Walk starts at dir and recurses. For the recursive case,
                    // the path is the result of filepath.Join, which calls filepath.Clean.
                    // The initial case is not Cleaned, though, so we do this explicitly.
                    //
                    // This converts a path like "./io/" to "io". Without this step, running
                    // "cd $GOROOT/src; go list ./io/..." would incorrectly skip the io
                    // package, because prepending the prefix "./" to the unclean path would
                    // result in "././io", and match("././io") returns false.
                    path = filepath.Clean(path);
                } 

                // Avoid .foo, _foo, and testdata directory trees, but do not avoid "." or "..".
                var (_, elem) = filepath.Split(path);
                var dot = strings.HasPrefix(elem, ".") && elem != "." && elem != "..";
                if (dot || strings.HasPrefix(elem, "_") || elem == "testdata")
                {
                    return filepath.SkipDir;
                }
                var name = prefix + filepath.ToSlash(path);
                if (!match(name))
                {
                    return null;
                } 

                // We keep the directory if we can import it, or if we can't import it
                // due to invalid Go source files. This means that directories containing
                // parse errors will be built (and fail) instead of being silently skipped
                // as not matching the pattern. Go 1.5 and earlier skipped, but that
                // behavior means people miss serious mistakes.
                // See golang.org/issue/11407.
                {
                    var (p, err) = cfg.BuildContext.ImportDir(path, 0L);

                    if (err != null && (p == null || len(p.InvalidGoFiles) == 0L))
                    {
                        {
                            ref build.NoGoError (_, noGo) = err._<ref build.NoGoError>();

                            if (!noGo)
                            {
                                log.Print(err);
                            }

                        }
                        return null;
                    }

                }
                pkgs = append(pkgs, name);
                return null;
            });
            return pkgs;
        }

        // treeCanMatchPattern(pattern)(name) reports whether
        // name or children of name can possibly match pattern.
        // Pattern is the same limited glob accepted by matchPattern.
        private static Func<@string, bool> treeCanMatchPattern(@string pattern)
        {
            var wildCard = false;
            {
                var i = strings.Index(pattern, "...");

                if (i >= 0L)
                {
                    wildCard = true;
                    pattern = pattern[..i];
                }

            }
            return name =>
            {
                return len(name) <= len(pattern) && hasPathPrefix(pattern, name) || wildCard && strings.HasPrefix(name, pattern);
            }
;
        }

        // matchPattern(pattern)(name) reports whether
        // name matches pattern. Pattern is a limited glob
        // pattern in which '...' means 'any string' and there
        // is no other special syntax.
        // Unfortunately, there are two special cases. Quoting "go help packages":
        //
        // First, /... at the end of the pattern can match an empty string,
        // so that net/... matches both net and packages in its subdirectories, like net/http.
        // Second, any slash-separted pattern element containing a wildcard never
        // participates in a match of the "vendor" element in the path of a vendored
        // package, so that ./... does not match packages in subdirectories of
        // ./vendor or ./mycode/vendor, but ./vendor/... and ./mycode/vendor/... do.
        // Note, however, that a directory named vendor that itself contains code
        // is not a vendored package: cmd/vendor would be a command named vendor,
        // and the pattern cmd/... matches it.
        private static Func<@string, bool> matchPattern(@string pattern)
        { 
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



            if (strings.Contains(pattern, vendorChar))
            {
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
                        re = strings.Replace(re, "\\.\\.\\.", "[^" + vendorChar + "]*", -1L);

            var reg = regexp.MustCompile("^" + re + "$");

            return name =>
            {
                if (strings.Contains(name, vendorChar))
                {
                    return false;
                }
                return reg.MatchString(replaceVendor(name, vendorChar));
            }
;
        }

        // MatchPackage(pattern, cwd)(p) reports whether package p matches pattern in the working directory cwd.
        public static Func<ref Package, bool> MatchPackage(@string pattern, @string cwd)
        {

            if (strings.HasPrefix(pattern, "./") || strings.HasPrefix(pattern, "../") || pattern == "." || pattern == "..") 
                // Split pattern into leading pattern-free directory path
                // (including all . and .. elements) and the final pattern.
                @string dir = default;
                var i = strings.Index(pattern, "...");
                if (i < 0L)
                {
                    dir = pattern;
                    pattern = "";
                }
                else
                {
                    var j = strings.LastIndex(pattern[..i], "/");
                    dir = pattern[..j];
                    pattern = pattern[j + 1L..];
                }
                dir = filepath.Join(cwd, dir);
                if (pattern == "")
                {
                    return p => p.Dir == dir;
                }
                var matchPath = matchPattern(pattern);
                return p =>
                { 
                    // Compute relative path to dir and see if it matches the pattern.
                    var (rel, err) = filepath.Rel(dir, p.Dir);
                    if (err != null)
                    { 
                        // Cannot make relative - e.g. different drive letters on Windows.
                        return false;
                    }
                    rel = filepath.ToSlash(rel);
                    if (rel == ".." || strings.HasPrefix(rel, "../"))
                    {
                        return false;
                    }
                    return matchPath(rel);
                }
;
            else if (pattern == "all") 
                return p => true;
            else if (pattern == "std") 
                return p => p.Standard;
            else if (pattern == "cmd") 
                return p => p.Standard && strings.HasPrefix(p.ImportPath, "cmd/");
            else 
                matchPath = matchPattern(pattern);
                return p => matchPath(p.ImportPath);
                    }

        // replaceVendor returns the result of replacing
        // non-trailing vendor path elements in x with repl.
        private static @string replaceVendor(@string x, @string repl)
        {
            if (!strings.Contains(x, "vendor"))
            {
                return x;
            }
            var elem = strings.Split(x, "/");
            for (long i = 0L; i < len(elem) - 1L; i++)
            {
                if (elem[i] == "vendor")
                {
                    elem[i] = repl;
                }
            }

            return strings.Join(elem, "/");
        }

        // ImportPaths returns the import paths to use for the given command line.
        public static slice<@string> ImportPaths(slice<@string> args)
        {
            args = ImportPathsNoDotExpansion(args);
            slice<@string> @out = default;
            foreach (var (_, a) in args)
            {
                if (strings.Contains(a, "..."))
                {
                    if (build.IsLocalImport(a))
                    {
                        out = append(out, allPackagesInFS(a));
                    }
                    else
                    {
                        out = append(out, allPackages(a));
                    }
                    continue;
                }
                out = append(out, a);
            }
            return out;
        }

        // ImportPathsNoDotExpansion returns the import paths to use for the given
        // command line, but it does no ... expansion.
        public static slice<@string> ImportPathsNoDotExpansion(slice<@string> args)
        {
            if (cmdlineMatchers == null)
            {
                SetCmdlinePatterns(args);
            }
            if (len(args) == 0L)
            {
                return new slice<@string>(new @string[] { "." });
            }
            slice<@string> @out = default;
            foreach (var (_, a) in args)
            { 
                // Arguments are supposed to be import paths, but
                // as a courtesy to Windows developers, rewrite \ to /
                // in command-line arguments. Handles .\... and so on.
                if (filepath.Separator == '\\')
                {
                    a = strings.Replace(a, "\\", "/", -1L);
                } 

                // Put argument in canonical form, but preserve leading ./.
                if (strings.HasPrefix(a, "./"))
                {
                    a = "./" + path.Clean(a);
                    if (a == "./.")
                    {
                        a = ".";
                    }
                }
                else
                {
                    a = path.Clean(a);
                }
                if (IsMetaPackage(a))
                {
                    out = append(out, allPackages(a));
                    continue;
                }
                out = append(out, a);
            }
            return out;
        }

        // IsMetaPackage checks if name is a reserved package name that expands to multiple packages.
        public static bool IsMetaPackage(@string name)
        {
            return name == "std" || name == "cmd" || name == "all";
        }
    }
}}}}
