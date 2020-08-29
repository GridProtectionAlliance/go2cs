// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:02:07 UTC
// Original source: C:\Go\src\cmd\gofmt\gofmt.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using printer = go.go.printer_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
 
        // main operation modes
        private static var list = flag.Bool("l", false, "list files whose formatting differs from gofmt's");        private static var write = flag.Bool("w", false, "write result to (source) file instead of stdout");        private static var rewriteRule = flag.String("r", "", "rewrite rule (e.g., 'a[b:len(a)] -> a[b:]')");        private static var simplifyAST = flag.Bool("s", false, "simplify code");        private static var doDiff = flag.Bool("d", false, "display diffs instead of rewriting files");        private static var allErrors = flag.Bool("e", false, "report all errors (not just the first 10 on different lines)");        private static var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to this file");

        private static readonly long tabWidth = 8L;
        private static readonly var printerMode = printer.UseSpaces | printer.TabIndent;

        private static var fileSet = token.NewFileSet();        private static long exitCode = 0L;        private static Func<ref ast.File, ref ast.File> rewrite = default;        private static parser.Mode parserMode = default;

        private static void report(error err)
        {
            scanner.PrintError(os.Stderr, err);
            exitCode = 2L;
        }

        private static void usage()
        {
            fmt.Fprintf(os.Stderr, "usage: gofmt [flags] [path ...]\n");
            flag.PrintDefaults();
        }

        private static void initParserMode()
        {
            parserMode = parser.ParseComments;
            if (allErrors.Value)
            {
                parserMode |= parser.AllErrors;
            }
        }

        private static bool isGoFile(os.FileInfo f)
        { 
            // ignore non-Go files
            var name = f.Name();
            return !f.IsDir() && !strings.HasPrefix(name, ".") && strings.HasSuffix(name, ".go");
        }

        // If in == nil, the source is the contents of the file with the given filename.
        private static error processFile(@string filename, io.Reader @in, io.Writer @out, bool stdin) => func((defer, _, __) =>
        {
            os.FileMode perm = 0644L;
            if (in == null)
            {
                var (f, err) = os.Open(filename);
                if (err != null)
                {
                    return error.As(err);
                }
                defer(f.Close());
                var (fi, err) = f.Stat();
                if (err != null)
                {
                    return error.As(err);
                }
                in = f;
                perm = fi.Mode().Perm();
            }
            var (src, err) = ioutil.ReadAll(in);
            if (err != null)
            {
                return error.As(err);
            }
            var (file, sourceAdj, indentAdj, err) = parse(fileSet, filename, src, stdin);
            if (err != null)
            {
                return error.As(err);
            }
            if (rewrite != null)
            {
                if (sourceAdj == null)
                {
                    file = rewrite(file);
                }
                else
                {
                    fmt.Fprintf(os.Stderr, "warning: rewrite ignored for incomplete programs\n");
                }
            }
            ast.SortImports(fileSet, file);

            if (simplifyAST.Value)
            {
                simplify(file);
            }
            var (res, err) = format(fileSet, file, sourceAdj, indentAdj, src, new printer.Config(Mode:printerMode,Tabwidth:tabWidth));
            if (err != null)
            {
                return error.As(err);
            }
            if (!bytes.Equal(src, res))
            { 
                // formatting has changed
                if (list.Value)
                {
                    fmt.Fprintln(out, filename);
                }
                if (write.Value)
                { 
                    // make a temporary backup before overwriting original
                    var (bakname, err) = backupFile(filename + ".", src, perm);
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    err = ioutil.WriteFile(filename, res, perm);
                    if (err != null)
                    {
                        os.Rename(bakname, filename);
                        return error.As(err);
                    }
                    err = os.Remove(bakname);
                    if (err != null)
                    {
                        return error.As(err);
                    }
                }
                if (doDiff.Value)
                {
                    var (data, err) = diff(src, res, filename);
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("computing diff: %s", err));
                    }
                    fmt.Printf("diff -u %s %s\n", filepath.ToSlash(filename + ".orig"), filepath.ToSlash(filename));
                    @out.Write(data);
                }
            }
            if (!list && !write && !doDiff.Value.Value.Value)
            {
                _, err = @out.Write(res);
            }
            return error.As(err);
        });

        private static error visitFile(@string path, os.FileInfo f, error err)
        {
            if (err == null && isGoFile(f))
            {
                err = processFile(path, null, os.Stdout, false);
            } 
            // Don't complain if a file was deleted in the meantime (i.e.
            // the directory changed concurrently while running gofmt).
            if (err != null && !os.IsNotExist(err))
            {
                report(err);
            }
            return error.As(null);
        }

        private static void walkDir(@string path)
        {
            filepath.Walk(path, visitFile);
        }

        private static void Main()
        { 
            // call gofmtMain in a separate function
            // so that it can use defer and have them
            // run before the exit.
            gofmtMain();
            os.Exit(exitCode);
        }

        private static void gofmtMain() => func((defer, _, __) =>
        {
            flag.Usage = usage;
            flag.Parse();

            if (cpuprofile != "".Value)
            {
                var (f, err) = os.Create(cpuprofile.Value);
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "creating cpu profile: %s\n", err);
                    exitCode = 2L;
                    return;
                }
                defer(f.Close());
                pprof.StartCPUProfile(f);
                defer(pprof.StopCPUProfile());
            }
            initParserMode();
            initRewrite();

            if (flag.NArg() == 0L)
            {
                if (write.Value)
                {
                    fmt.Fprintln(os.Stderr, "error: cannot use -w with standard input");
                    exitCode = 2L;
                    return;
                }
                {
                    var err__prev2 = err;

                    var err = processFile("<standard input>", os.Stdin, os.Stdout, true);

                    if (err != null)
                    {
                        report(err);
                    }

                    err = err__prev2;

                }
                return;
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

                            err = processFile(path, null, os.Stdout, false);

                            if (err != null)
                            {
                                report(err);
                            }

                            err = err__prev1;

                        }


                    err = err__prev1;
                }
            }

        });

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

        private static (slice<byte>, error) diff(slice<byte> b1, slice<byte> b2, @string filename) => func((defer, _, __) =>
        {
            var (f1, err) = writeTempFile("", "gofmt", b1);
            if (err != null)
            {
                return;
            }
            defer(os.Remove(f1));

            var (f2, err) = writeTempFile("", "gofmt", b2);
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
                return replaceTempFilename(data, filename);
            }
            return;
        });

        // replaceTempFilename replaces temporary filenames in diff with actual one.
        //
        // --- /tmp/gofmt316145376    2017-02-03 19:13:00.280468375 -0500
        // +++ /tmp/gofmt617882815    2017-02-03 19:13:00.280468375 -0500
        // ...
        // ->
        // --- path/to/file.go.orig    2017-02-03 19:13:00.280468375 -0500
        // +++ path/to/file.go    2017-02-03 19:13:00.280468375 -0500
        // ...
        private static (slice<byte>, error) replaceTempFilename(slice<byte> diff, @string filename)
        {
            var bs = bytes.SplitN(diff, new slice<byte>(new byte[] { '\n' }), 3L);
            if (len(bs) < 3L)
            {
                return (null, fmt.Errorf("got unexpected diff for %s", filename));
            } 
            // Preserve timestamps.
            slice<byte> t0 = default;            slice<byte> t1 = default;

            {
                var i__prev1 = i;

                var i = bytes.LastIndexByte(bs[0L], '\t');

                if (i != -1L)
                {
                    t0 = bs[0L][i..];
                }

                i = i__prev1;

            }
            {
                var i__prev1 = i;

                i = bytes.LastIndexByte(bs[1L], '\t');

                if (i != -1L)
                {
                    t1 = bs[1L][i..];
                } 
                // Always print filepath with slash separator.

                i = i__prev1;

            } 
            // Always print filepath with slash separator.
            var f = filepath.ToSlash(filename);
            bs[0L] = (slice<byte>)fmt.Sprintf("--- %s%s", f + ".orig", t0);
            bs[1L] = (slice<byte>)fmt.Sprintf("+++ %s%s", f, t1);
            return (bytes.Join(bs, new slice<byte>(new byte[] { '\n' })), null);
        }

        private static readonly var chmodSupported = runtime.GOOS != "windows";

        // backupFile writes data to a new file named filename<number> with permissions perm,
        // with <number randomly chosen such that the file name is unique. backupFile returns
        // the chosen file name.


        // backupFile writes data to a new file named filename<number> with permissions perm,
        // with <number randomly chosen such that the file name is unique. backupFile returns
        // the chosen file name.
        private static (@string, error) backupFile(@string filename, slice<byte> data, os.FileMode perm)
        { 
            // create backup file
            var (f, err) = ioutil.TempFile(filepath.Dir(filename), filepath.Base(filename));
            if (err != null)
            {
                return ("", err);
            }
            var bakname = f.Name();
            if (chmodSupported)
            {
                err = f.Chmod(perm);
                if (err != null)
                {
                    f.Close();
                    os.Remove(bakname);
                    return (bakname, err);
                }
            } 

            // write data to backup file
            var (n, err) = f.Write(data);
            if (err == null && n < len(data))
            {
                err = io.ErrShortWrite;
            }
            {
                var err1 = f.Close();

                if (err == null)
                {
                    err = err1;
                }

            }

            return (bakname, err);
        }
    }
}
