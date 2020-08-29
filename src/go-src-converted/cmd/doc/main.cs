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
// For complete documentation, run "go help doc".
// package main -- go2cs converted at 2020 August 29 10:00:04 UTC
// Original source: C:\Go\src\cmd\doc\main.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static bool unexported = default;        private static bool matchCase = default;        private static bool showCmd = default;

        // usage is a replacement usage function for the flags package.
        private static void usage()
        {
            fmt.Fprintf(os.Stderr, "Usage of [go] doc:\n");
            fmt.Fprintf(os.Stderr, "\tgo doc\n");
            fmt.Fprintf(os.Stderr, "\tgo doc <pkg>\n");
            fmt.Fprintf(os.Stderr, "\tgo doc <sym>[.<method>]\n");
            fmt.Fprintf(os.Stderr, "\tgo doc [<pkg>].<sym>[.<method>]\n");
            fmt.Fprintf(os.Stderr, "\tgo doc <pkg> <sym>[.<method>]\n");
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
            var err = do(os.Stdout, flag.CommandLine, os.Args[1L..]);
            if (err != null)
            {
                log.Fatal(err);
            }
        }

        // do is the workhorse, broken out of main to make testing easier.
        private static error @do(io.Writer writer, ref flag.FlagSet _flagSet, slice<@string> args) => func(_flagSet, (ref flag.FlagSet flagSet, Defer defer, Panic panic, Recover _) =>
        {
            flagSet.Usage = usage;
            unexported = false;
            matchCase = false;
            flagSet.BoolVar(ref unexported, "u", false, "show unexported symbols as well as exported");
            flagSet.BoolVar(ref matchCase, "c", false, "symbol matching honors case (paths not affected)");
            flagSet.BoolVar(ref showCmd, "cmd", false, "show symbols with package docs even if package is a command");
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
                    return error.As(failMessage(paths, symbol, method));
                }
                if (buildPackage == null)
                {
                    return error.As(fmt.Errorf("no such package: %s", userPath));
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
                        return;
                    }
                    PackageError (pkgError, ok) = e._<PackageError>();
                    if (ok)
                    {
                        err = pkgError;
                        return;
                    }
                    panic(e);
                }()); 

                // The builtin package needs special treatment: its symbols are lower
                // case but we want to see them, always.
                if (pkg.build.ImportPath == "builtin")
                {
                    unexported = true;
                }

                if (symbol == "") 
                    pkg.packageDoc(); // The package exists, so we got some output.
                    return;
                else if (method == "") 
                    if (pkg.symbolDoc(symbol))
                    {
                        return;
                    }
                else 
                    if (pkg.methodDoc(symbol, method))
                    {
                        return;
                    }
                    if (pkg.fieldDoc(symbol, method))
                    {
                        return;
                    }
                            }

        });

        // failMessage creates a nicely formatted error message when there is no result to show.
        private static error failMessage(slice<@string> paths, @string symbol, @string method)
        {
            bytes.Buffer b = default;
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
                return error.As(fmt.Errorf("no symbol %s in package%s", symbol, ref b));
            }
            return error.As(fmt.Errorf("no method or field %s.%s in package%s", symbol, method, ref b));
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
        private static (ref build.Package, @string, @string, bool) parseArgs(slice<@string> args)
        {
            switch (len(args))
            {
                case 0L: 
                    // Easy: current directory.
                    return (importDir(pwd()), "", "", false);
                    break;
                case 1L: 
                    break;
                case 2L: 
                    // Package must be findable and importable.
                    var (packagePath, ok) = findPackage(args[0L]);
                    if (!ok)
                    {
                        return (null, args[0L], args[1L], false);
                    }
                    return (importDir(packagePath), args[0L], args[1L], true);
                    break;
                default: 
                    usage();
                    break;
            } 
            // Usual case: one argument.
            var arg = args[0L]; 
            // If it contains slashes, it begins with a package path.
            // First, is it a complete package path as it is? If so, we are done.
            // This avoids confusion over package paths that have other
            // package paths as their prefix.
            var (pkg, err) = build.Import(arg, "", build.ImportComment);
            if (err == null)
            {
                return (pkg, arg, "", false);
            } 
            // Another disambiguator: If the symbol starts with an upper
            // case letter, it can only be a symbol in the current directory.
            // Kills the problem caused by case-insensitive file systems
            // matching an upper case name as a package name.
            if (isUpper(arg))
            {
                (pkg, err) = build.ImportDir(".", build.ImportComment);
                if (err == null)
                {
                    return (pkg, "", arg, false);
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
                    (pkg, err) = build.Import(arg[0L..period], "", build.ImportComment);
                    if (err == null)
                    {
                        return (pkg, arg[0L..period], symbol, false);
                    } 
                    // See if we have the basename or tail of a package, as in json for encoding/json
                    // or ivy/value for robpike.io/ivy/value.
                    var (path, ok) = findPackage(arg[0L..period]);
                    if (ok)
                    {
                        return (importDir(path), arg[0L..period], symbol, true);
                    }
                    dirs.Reset(); // Next iteration of for loop must scan all the directories again.
                } 
                // If it has a slash, we've failed.

            } 
            // If it has a slash, we've failed.
            if (slash >= 0L)
            {
                log.Fatalf("no such package %s", arg[0L..period]);
            } 
            // Guess it's a symbol in the current directory.
            return (importDir(pwd()), "", arg, false);
        }

        // importDir is just an error-catching wrapper for build.ImportDir.
        private static ref build.Package importDir(@string dir)
        {
            var (pkg, err) = build.ImportDir(dir, build.ImportComment);
            if (err != null)
            {
                log.Fatal(err);
            }
            return pkg;
        }

        // parseSymbol breaks str apart into a symbol and method.
        // Both may be missing or the method may be missing.
        // If present, each must be a valid Go identifier.
        private static (@string, @string) parseSymbol(@string str)
        {
            if (str == "")
            {
                return;
            }
            var elem = strings.Split(str, ".");
            switch (len(elem))
            {
                case 1L: 
                    break;
                case 2L: 
                    method = elem[1L];
                    isIdentifier(method);
                    break;
                default: 
                    log.Printf("too many periods in symbol specification");
                    usage();
                    break;
            }
            symbol = elem[0L];
            isIdentifier(symbol);
            return;
        }

        // isIdentifier checks that the name is valid Go identifier, and
        // logs and exits if it is not.
        private static void isIdentifier(@string name)
        {
            if (len(name) == 0L)
            {
                log.Fatal("empty symbol");
            }
            foreach (var (i, ch) in name)
            {
                if (unicode.IsLetter(ch) || ch == '_' || i > 0L && unicode.IsDigit(ch))
                {
                    continue;
                }
                log.Fatalf("invalid identifier %q", name);
            }
        }

        // isExported reports whether the name is an exported identifier.
        // If the unexported flag (-u) is true, isExported returns true because
        // it means that we treat the name as if it is exported.
        private static bool isExported(@string name)
        {
            return unexported || isUpper(name);
        }

        // isUpper reports whether the name starts with an upper case letter.
        private static bool isUpper(@string name)
        {
            var (ch, _) = utf8.DecodeRuneInString(name);
            return unicode.IsUpper(ch);
        }

        // findPackage returns the full file name path that first matches the
        // (perhaps partial) package path pkg. The boolean reports if any match was found.
        private static (@string, bool) findPackage(@string pkg)
        {
            if (pkg == "" || isUpper(pkg))
            { // Upper case symbol cannot be a package name.
                return ("", false);
            }
            var pkgString = filepath.Clean(string(filepath.Separator) + pkg);
            while (true)
            {
                var (path, ok) = dirs.Next();
                if (!ok)
                {
                    return ("", false);
                }
                if (strings.HasSuffix(path, pkgString))
                {
                    return (path, true);
                }
            }

        }

        // splitGopath splits $GOPATH into a list of roots.
        private static slice<@string> splitGopath()
        {
            return filepath.SplitList(build.Default.GOPATH);
        }

        // pwd returns the current directory.
        private static @string pwd()
        {
            var (wd, err) = os.Getwd();
            if (err != null)
            {
                log.Fatal(err);
            }
            return wd;
        }
    }
}
