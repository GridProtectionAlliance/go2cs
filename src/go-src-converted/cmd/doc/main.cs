// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Doc (usually run as go doc) accepts zero, one or two arguments.
//
// Zero arguments:
//    go doc
// Show the documentation for the package in the current directory.
//
// One argument:
//    go doc <pkg>
//    go doc <sym>[.<methodOrField>]
//    go doc [<pkg>.]<sym>[.<methodOrField>]
//    go doc [<pkg>.][<sym>.]<methodOrField>
// The first item in this list that succeeds is the one whose documentation
// is printed. If there is a symbol but no package, the package in the current
// directory is chosen. However, if the argument begins with a capital
// letter it is always assumed to be a symbol in the current directory.
//
// Two arguments:
//    go doc <pkg> <sym>[.<methodOrField>]
//
// Show the documentation for the package, symbol, and method or field. The
// first argument must be a full package path. This is similar to the
// command-line usage for the godoc command.
//
// For commands, unless the -cmd flag is present "go doc command"
// shows only the package-level docs for the package.
//
// The -src flag causes doc to print the full source code for the symbol, such
// as the body of a struct, function or method.
//
// The -all flag causes doc to print all documentation for the package and
// all its visible symbols. The argument must identify a package.
//
// For complete documentation, run "go help doc".
// package main -- go2cs converted at 2020 October 08 04:33:05 UTC
// Original source: C:\Go\src\cmd\doc\main.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using token = go.go.token_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static bool unexported = default;        private static bool matchCase = default;        private static bool showAll = default;        private static bool showCmd = default;        private static bool showSrc = default;        private static bool @short = default;

        // usage is a replacement usage function for the flags package.
        private static void usage()
        {
            fmt.Fprintf(os.Stderr, "Usage of [go] doc:\n");
            fmt.Fprintf(os.Stderr, "\tgo doc\n");
            fmt.Fprintf(os.Stderr, "\tgo doc <pkg>\n");
            fmt.Fprintf(os.Stderr, "\tgo doc <sym>[.<methodOrField>]\n");
            fmt.Fprintf(os.Stderr, "\tgo doc [<pkg>.]<sym>[.<methodOrField>]\n");
            fmt.Fprintf(os.Stderr, "\tgo doc [<pkg>.][<sym>.]<methodOrField>\n");
            fmt.Fprintf(os.Stderr, "\tgo doc <pkg> <sym>[.<methodOrField>]\n");
            fmt.Fprintf(os.Stderr, "For more information run\n");
            fmt.Fprintf(os.Stderr, "\tgo help doc\n\n");
            fmt.Fprintf(os.Stderr, "Flags:\n");
            flag.PrintDefaults();
            os.Exit(2L);
        }

        private static void Main()
        {
            log.SetFlags(0L);
            log.SetPrefix("doc: ");
            dirsInit();
            var err = do(os.Stdout, _addr_flag.CommandLine, os.Args[1L..]);
            if (err != null)
            {
                log.Fatal(err);
            }

        }

        // do is the workhorse, broken out of main to make testing easier.
        private static error @do(io.Writer writer, ptr<flag.FlagSet> _addr_flagSet, slice<@string> args) => func((defer, panic, _) =>
        {
            error err = default!;
            ref flag.FlagSet flagSet = ref _addr_flagSet.val;

            flagSet.Usage = usage;
            unexported = false;
            matchCase = false;
            flagSet.BoolVar(_addr_unexported, "u", false, "show unexported symbols as well as exported");
            flagSet.BoolVar(_addr_matchCase, "c", false, "symbol matching honors case (paths not affected)");
            flagSet.BoolVar(_addr_showAll, "all", false, "show all documentation for package");
            flagSet.BoolVar(_addr_showCmd, "cmd", false, "show symbols with package docs even if package is a command");
            flagSet.BoolVar(_addr_showSrc, "src", false, "show source code for symbol");
            flagSet.BoolVar(_addr_short, "short", false, "one-line representation for each symbol");
            flagSet.Parse(args);
            slice<@string> paths = default;
            @string symbol = default;            @string method = default; 
            // Loop until something is printed.
 
            // Loop until something is printed.
            dirs.Reset();
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                var (buildPackage, userPath, sym, more) = parseArgs(flagSet.Args());
                if (i > 0L && !more)
                { // Ignore the "more" bit on the first iteration.
                    return error.As(failMessage(paths, symbol, method))!;

                }

                if (buildPackage == null)
                {
                    return error.As(fmt.Errorf("no such package: %s", userPath))!;
                }

                symbol, method = parseSymbol(sym);
                var pkg = parsePackage(writer, buildPackage, userPath);
                paths = append(paths, pkg.prettyPath());

                defer(() =>
                {
                    pkg.flush();
                    var e = recover();
                    if (e == null)
                    {
                        return ;
                    }

                    PackageError (pkgError, ok) = e._<PackageError>();
                    if (ok)
                    {
                        err = pkgError;
                        return ;
                    }

                    panic(e);

                }()); 

                // The builtin package needs special treatment: its symbols are lower
                // case but we want to see them, always.
                if (pkg.build.ImportPath == "builtin")
                {
                    unexported = true;
                } 

                // We have a package.
                if (showAll && symbol == "")
                {
                    pkg.allDoc();
                    return ;
                }


                if (symbol == "") 
                    pkg.packageDoc(); // The package exists, so we got some output.
                    return ;
                else if (method == "") 
                    if (pkg.symbolDoc(symbol))
                    {
                        return ;
                    }

                else 
                    if (pkg.methodDoc(symbol, method))
                    {
                        return ;
                    }

                    if (pkg.fieldDoc(symbol, method))
                    {
                        return ;
                    }

                            }


        });

        // failMessage creates a nicely formatted error message when there is no result to show.
        private static error failMessage(slice<@string> paths, @string symbol, @string method)
        {
            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            if (len(paths) > 1L)
            {
                b.WriteString("s");
            }

            b.WriteString(" ");
            foreach (var (i, path) in paths)
            {
                if (i > 0L)
                {
                    b.WriteString(", ");
                }

                b.WriteString(path);

            }
            if (method == "")
            {
                return error.As(fmt.Errorf("no symbol %s in package%s", symbol, _addr_b))!;
            }

            return error.As(fmt.Errorf("no method or field %s.%s in package%s", symbol, method, _addr_b))!;

        }

        // parseArgs analyzes the arguments (if any) and returns the package
        // it represents, the part of the argument the user used to identify
        // the path (or "" if it's the current package) and the symbol
        // (possibly with a .method) within that package.
        // parseSymbol is used to analyze the symbol itself.
        // The boolean final argument reports whether it is possible that
        // there may be more directories worth looking at. It will only
        // be true if the package path is a partial match for some directory
        // and there may be more matches. For example, if the argument
        // is rand.Float64, we must scan both crypto/rand and math/rand
        // to find the symbol, and the first call will return crypto/rand, true.
        private static (ptr<build.Package>, @string, @string, bool) parseArgs(slice<@string> args)
        {
            ptr<build.Package> pkg = default!;
            @string path = default;
            @string symbol = default;
            bool more = default;

            var (wd, err) = os.Getwd();
            if (err != null)
            {
                log.Fatal(err);
            }

            if (len(args) == 0L)
            { 
                // Easy: current directory.
                return (_addr_importDir(wd)!, "", "", false);

            }

            var arg = args[0L]; 
            // We have an argument. If it is a directory name beginning with . or ..,
            // use the absolute path name. This discriminates "./errors" from "errors"
            // if the current directory contains a non-standard errors package.
            if (isDotSlash(arg))
            {
                arg = filepath.Join(wd, arg);
            }

            switch (len(args))
            {
                case 1L: 
                    break;
                case 2L: 
                    // Package must be findable and importable.
                    var (pkg, err) = build.Import(args[0L], wd, build.ImportComment);
                    if (err == null)
                    {
                        return (_addr_pkg!, args[0L], args[1L], false);
                    }

                    while (true)
                    {
                        var (packagePath, ok) = findNextPackage(arg);
                        if (!ok)
                        {
                            break;
                        }

                        {
                            var pkg__prev1 = pkg;

                            (pkg, err) = build.ImportDir(packagePath, build.ImportComment);

                            if (err == null)
                            {
                                return (_addr_pkg!, arg, args[1L], true);
                            }

                            pkg = pkg__prev1;

                        }

                    }

                    return (_addr_null!, args[0L], args[1L], false);
                    break;
                default: 
                    usage();
                    break;
            } 
            // Usual case: one argument.
            // If it contains slashes, it begins with either a package path
            // or an absolute directory.
            // First, is it a complete package path as it is? If so, we are done.
            // This avoids confusion over package paths that have other
            // package paths as their prefix.
            error importErr = default!;
            if (filepath.IsAbs(arg))
            {
                pkg, importErr = build.ImportDir(arg, build.ImportComment);
                if (importErr == null)
                {
                    return (_addr_pkg!, arg, "", false);
                }

            }
            else
            {
                pkg, importErr = build.Import(arg, wd, build.ImportComment);
                if (importErr == null)
                {
                    return (_addr_pkg!, arg, "", false);
                }

            } 
            // Another disambiguator: If the argument starts with an upper
            // case letter, it can only be a symbol in the current directory.
            // Kills the problem caused by case-insensitive file systems
            // matching an upper case name as a package name.
            if (!strings.ContainsAny(arg, "/\\") && token.IsExported(arg))
            {
                (pkg, err) = build.ImportDir(".", build.ImportComment);
                if (err == null)
                {
                    return (_addr_pkg!, "", arg, false);
                }

            } 
            // If it has a slash, it must be a package path but there is a symbol.
            // It's the last package path we care about.
            var slash = strings.LastIndex(arg, "/"); 
            // There may be periods in the package path before or after the slash
            // and between a symbol and method.
            // Split the string at various periods to see what we find.
            // In general there may be ambiguities but this should almost always
            // work.
            long period = default; 
            // slash+1: if there's no slash, the value is -1 and start is 0; otherwise
            // start is the byte after the slash.
            {
                var start = slash + 1L;

                while (start < len(arg))
                {
                    period = strings.Index(arg[start..], ".");
                    @string symbol = "";
                    if (period < 0L)
                    {
                        period = len(arg);
                    start = period + 1L;
                    }
                    else
                    {
                        period += start;
                        symbol = arg[period + 1L..];
                    } 
                    // Have we identified a package already?
                    (pkg, err) = build.Import(arg[0L..period], wd, build.ImportComment);
                    if (err == null)
                    {
                        return (_addr_pkg!, arg[0L..period], symbol, false);
                    } 
                    // See if we have the basename or tail of a package, as in json for encoding/json
                    // or ivy/value for robpike.io/ivy/value.
                    var pkgName = arg[..period];
                    while (true)
                    {
                        var (path, ok) = findNextPackage(pkgName);
                        if (!ok)
                        {
                            break;
                        }

                        pkg, err = build.ImportDir(path, build.ImportComment);

                        if (err == null)
                        {
                            return (_addr_pkg!, arg[0L..period], symbol, true);
                        }

                    }

                    dirs.Reset(); // Next iteration of for loop must scan all the directories again.
                } 
                // If it has a slash, we've failed.

            } 
            // If it has a slash, we've failed.
            if (slash >= 0L)
            { 
                // build.Import should always include the path in its error message,
                // and we should avoid repeating it. Unfortunately, build.Import doesn't
                // return a structured error. That can't easily be fixed, since it
                // invokes 'go list' and returns the error text from the loaded package.
                // TODO(golang.org/issue/34750): load using golang.org/x/tools/go/packages
                // instead of go/build.
                var importErrStr = importErr.Error();
                if (strings.Contains(importErrStr, arg[..period]))
                {
                    log.Fatal(importErrStr);
                }
                else
                {
                    log.Fatalf("no such package %s: %s", arg[..period], importErrStr);
                }

            } 
            // Guess it's a symbol in the current directory.
            return (_addr_importDir(wd)!, "", arg, false);

        }

        // dotPaths lists all the dotted paths legal on Unix-like and
        // Windows-like file systems. We check them all, as the chance
        // of error is minute and even on Windows people will use ./
        // sometimes.
        private static @string dotPaths = new slice<@string>(new @string[] { `./`, `../`, `.\`, `..\` });

        // isDotSlash reports whether the path begins with a reference
        // to the local . or .. directory.
        private static bool isDotSlash(@string arg)
        {
            if (arg == "." || arg == "..")
            {
                return true;
            }

            foreach (var (_, dotPath) in dotPaths)
            {
                if (strings.HasPrefix(arg, dotPath))
                {
                    return true;
                }

            }
            return false;

        }

        // importDir is just an error-catching wrapper for build.ImportDir.
        private static ptr<build.Package> importDir(@string dir)
        {
            var (pkg, err) = build.ImportDir(dir, build.ImportComment);
            if (err != null)
            {
                log.Fatal(err);
            }

            return _addr_pkg!;

        }

        // parseSymbol breaks str apart into a symbol and method.
        // Both may be missing or the method may be missing.
        // If present, each must be a valid Go identifier.
        private static (@string, @string) parseSymbol(@string str)
        {
            @string symbol = default;
            @string method = default;

            if (str == "")
            {
                return ;
            }

            var elem = strings.Split(str, ".");
            switch (len(elem))
            {
                case 1L: 
                    break;
                case 2L: 
                    method = elem[1L];
                    break;
                default: 
                    log.Printf("too many periods in symbol specification");
                    usage();
                    break;
            }
            symbol = elem[0L];
            return ;

        }

        // isExported reports whether the name is an exported identifier.
        // If the unexported flag (-u) is true, isExported returns true because
        // it means that we treat the name as if it is exported.
        private static bool isExported(@string name)
        {
            return unexported || token.IsExported(name);
        }

        // findNextPackage returns the next full file name path that matches the
        // (perhaps partial) package path pkg. The boolean reports if any match was found.
        private static (@string, bool) findNextPackage(@string pkg)
        {
            @string _p0 = default;
            bool _p0 = default;

            if (filepath.IsAbs(pkg))
            {
                if (dirs.offset == 0L)
                {
                    dirs.offset = -1L;
                    return (pkg, true);
                }

                return ("", false);

            }

            if (pkg == "" || token.IsExported(pkg))
            { // Upper case symbol cannot be a package name.
                return ("", false);

            }

            pkg = path.Clean(pkg);
            @string pkgSuffix = "/" + pkg;
            while (true)
            {
                var (d, ok) = dirs.Next();
                if (!ok)
                {
                    return ("", false);
                }

                if (d.importPath == pkg || strings.HasSuffix(d.importPath, pkgSuffix))
                {
                    return (d.dir, true);
                }

            }


        }

        private static var buildCtx = build.Default;

        // splitGopath splits $GOPATH into a list of roots.
        private static slice<@string> splitGopath()
        {
            return filepath.SplitList(buildCtx.GOPATH);
        }
    }
}
