// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:00:18 UTC
// Original source: C:\Go\src\cmd\fix\main.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using parser = go.go.parser_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var fset = token.NewFileSet();        private static long exitCode = 0L;

        private static var allowedRewrites = flag.String("r", "", "restrict the rewrites to this comma-separated list");

        private static var forceRewrites = flag.String("force", "", "force these fixes to run even if the code looks updated");

        private static map<@string, bool> allowed = default;        private static map<@string, bool> force = default;



        private static var doDiff = flag.Bool("diff", false, "display diffs instead of rewriting files");

        // enable for debugging fix failures
        private static readonly var debug = false; // display incorrectly reformatted source and exit

 // display incorrectly reformatted source and exit

        private static void usage()
        {
            fmt.Fprintf(os.Stderr, "usage: go tool fix [-diff] [-r fixname,...] [-force fixname,...] [path ...]\n");
            flag.PrintDefaults();
            fmt.Fprintf(os.Stderr, "\nAvailable rewrites are:\n");
            sort.Sort(byName(fixes));
            foreach (var (_, f) in fixes)
            {
                if (f.disabled)
                {
                    fmt.Fprintf(os.Stderr, "\n%s (disabled)\n", f.name);
                }
                else
                {
                    fmt.Fprintf(os.Stderr, "\n%s\n", f.name);
                }
                var desc = strings.TrimSpace(f.desc);
                desc = strings.Replace(desc, "\n", "\n\t", -1L);
                fmt.Fprintf(os.Stderr, "\t%s\n", desc);
            }
            os.Exit(2L);
        }

        private static void Main()
        {
            flag.Usage = usage;
            flag.Parse();

            sort.Sort(byDate(fixes));

            if (allowedRewrites != "".Value)
            {
                allowed = make_map<@string, bool>();
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in strings.Split(allowedRewrites.Value, ","))
                    {
                        f = __f;
                        allowed[f] = true;
                    }

                    f = f__prev1;
                }

            }
            if (forceRewrites != "".Value)
            {
                force = make_map<@string, bool>();
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in strings.Split(forceRewrites.Value, ","))
                    {
                        f = __f;
                        force[f] = true;
                    }

                    f = f__prev1;
                }

            }
            if (flag.NArg() == 0L)
            {
                {
                    var err__prev2 = err;

                    var err = processFile("standard input", true);

                    if (err != null)
                    {
                        report(err);
                    }

                    err = err__prev2;

                }
                os.Exit(exitCode);
            }
            for (long i = 0L; i < flag.NArg(); i++)
            {
                var path = flag.Arg(i);
                {
                    var err__prev1 = err;

                    var (dir, err) = os.Stat(path);


                    if (err != null) 
                        report(err);
                    else if (dir.IsDir()) 
                        walkDir(path);
                    else 
                        {
                            var err__prev1 = err;

                            err = processFile(path, false);

                            if (err != null)
                            {
                                report(err);
                            }

                            err = err__prev1;

                        }


                    err = err__prev1;
                }
            }


            os.Exit(exitCode);
        }

        private static readonly var parserMode = parser.ParseComments;



        private static (slice<byte>, error) gofmtFile(ref ast.File f)
        {
            bytes.Buffer buf = default;
            {
                var err = format.Node(ref buf, fset, f);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return (buf.Bytes(), null);
        }

        private static error processFile(@string filename, bool useStdin) => func((defer, _, __) =>
        {
            ref os.File f = default;
            error err = default;
            bytes.Buffer fixlog = default;

            if (useStdin)
            {
                f = os.Stdin;
            }
            else
            {
                f, err = os.Open(filename);
                if (err != null)
                {
                    return error.As(err);
                }
                defer(f.Close());
            }
            var (src, err) = ioutil.ReadAll(f);
            if (err != null)
            {
                return error.As(err);
            }
            var (file, err) = parser.ParseFile(fset, filename, src, parserMode);
            if (err != null)
            {
                return error.As(err);
            } 

            // Apply all fixes to file.
            var newFile = file;
            var @fixed = false;
            foreach (var (_, fix) in fixes)
            {
                if (allowed != null && !allowed[fix.name])
                {
                    continue;
                }
                if (fix.disabled && !force[fix.name])
                {
                    continue;
                }
                if (fix.f(newFile))
                {
                    fixed = true;
                    fmt.Fprintf(ref fixlog, " %s", fix.name); 

                    // AST changed.
                    // Print and parse, to update any missing scoping
                    // or position information for subsequent fixers.
                    var (newSrc, err) = gofmtFile(newFile);
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    newFile, err = parser.ParseFile(fset, filename, newSrc, parserMode);
                    if (err != null)
                    {
                        if (debug)
                        {
                            fmt.Printf("%s", newSrc);
                            report(err);
                            os.Exit(exitCode);
                        }
                        return error.As(err);
                    }
                }
            }
            if (!fixed)
            {
                return error.As(null);
            }
            fmt.Fprintf(os.Stderr, "%s: fixed %s\n", filename, fixlog.String()[1L..]); 

            // Print AST.  We did that after each fix, so this appears
            // redundant, but it is necessary to generate gofmt-compatible
            // source code in a few cases. The official gofmt style is the
            // output of the printer run on a standard AST generated by the parser,
            // but the source we generated inside the loop above is the
            // output of the printer run on a mangled AST generated by a fixer.
            (newSrc, err) = gofmtFile(newFile);
            if (err != null)
            {
                return error.As(err);
            }
            if (doDiff.Value)
            {
                var (data, err) = diff(src, newSrc);
                if (err != null)
                {
                    return error.As(fmt.Errorf("computing diff: %s", err));
                }
                fmt.Printf("diff %s fixed/%s\n", filename, filename);
                os.Stdout.Write(data);
                return error.As(null);
            }
            if (useStdin)
            {
                os.Stdout.Write(newSrc);
                return error.As(null);
            }
            return error.As(ioutil.WriteFile(f.Name(), newSrc, 0L));
        });

        private static bytes.Buffer gofmtBuf = default;

        private static @string gofmt(object n)
        {
            gofmtBuf.Reset();
            {
                var err = format.Node(ref gofmtBuf, fset, n);

                if (err != null)
                {
                    return "<" + err.Error() + ">";
                }

            }
            return gofmtBuf.String();
        }

        private static void report(error err)
        {
            scanner.PrintError(os.Stderr, err);
            exitCode = 2L;
        }

        private static void walkDir(@string path)
        {
            filepath.Walk(path, visitFile);
        }

        private static error visitFile(@string path, os.FileInfo f, error err)
        {
            if (err == null && isGoFile(f))
            {
                err = processFile(path, false);
            }
            if (err != null)
            {
                report(err);
            }
            return error.As(null);
        }

        private static bool isGoFile(os.FileInfo f)
        { 
            // ignore non-Go files
            var name = f.Name();
            return !f.IsDir() && !strings.HasPrefix(name, ".") && strings.HasSuffix(name, ".go");
        }

        private static (@string, error) writeTempFile(@string dir, @string prefix, slice<byte> data)
        {
            var (file, err) = ioutil.TempFile(dir, prefix);
            if (err != null)
            {
                return ("", err);
            }
            _, err = file.Write(data);
            {
                var err1 = file.Close();

                if (err == null)
                {
                    err = err1;
                }

            }
            if (err != null)
            {
                os.Remove(file.Name());
                return ("", err);
            }
            return (file.Name(), null);
        }

        private static (slice<byte>, error) diff(slice<byte> b1, slice<byte> b2) => func((defer, _, __) =>
        {
            var (f1, err) = writeTempFile("", "go-fix", b1);
            if (err != null)
            {
                return;
            }
            defer(os.Remove(f1));

            var (f2, err) = writeTempFile("", "go-fix", b2);
            if (err != null)
            {
                return;
            }
            defer(os.Remove(f2));

            @string cmd = "diff";
            if (runtime.GOOS == "plan9")
            {
                cmd = "/bin/ape/diff";
            }
            data, err = exec.Command(cmd, "-u", f1, f2).CombinedOutput();
            if (len(data) > 0L)
            { 
                // diff exits with a non-zero status when the files don't match.
                // Ignore that failure as long as we get output.
                err = null;
            }
            return;
        });
    }
}
