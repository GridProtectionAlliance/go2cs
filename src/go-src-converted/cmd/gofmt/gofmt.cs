// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:32:34 UTC
// Original source: C:\Program Files\Go\src\cmd\gofmt\gofmt.go
namespace go;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
using printer = go.printer_package;
using scanner = go.scanner_package;
using token = go.token_package;
using io = io_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using pprof = runtime.pprof_package;
using strings = strings_package;

using diff = cmd.@internal.diff_package;
using System;

public static partial class main_package {

 
// main operation modes
private static var list = flag.Bool("l", false, "list files whose formatting differs from gofmt's");private static var write = flag.Bool("w", false, "write result to (source) file instead of stdout");private static var rewriteRule = flag.String("r", "", "rewrite rule (e.g., 'a[b:len(a)] -> a[b:]')");private static var simplifyAST = flag.Bool("s", false, "simplify code");private static var doDiff = flag.Bool("d", false, "display diffs instead of rewriting files");private static var allErrors = flag.Bool("e", false, "report all errors (not just the first 10 on different lines)");private static var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to this file");

// Keep these in sync with go/format/format.go.
private static readonly nint tabWidth = 8;
private static readonly var printerMode = printer.UseSpaces | printer.TabIndent | printerNormalizeNumbers; 

// printerNormalizeNumbers means to canonicalize number literal prefixes
// and exponents while printing. See https://golang.org/doc/go1.13#gofmt.
//
// This value is defined in go/printer specifically for go/format and cmd/gofmt.
private static readonly nint printerNormalizeNumbers = 1 << 30;

private static var fileSet = token.NewFileSet();private static nint exitCode = 0;private static Func<ptr<ast.File>, ptr<ast.File>> rewrite = default;private static parser.Mode parserMode = default;

private static void report(error err) {
    scanner.PrintError(os.Stderr, err);
    exitCode = 2;
}

private static void usage() {
    fmt.Fprintf(os.Stderr, "usage: gofmt [flags] [path ...]\n");
    flag.PrintDefaults();
}

private static void initParserMode() {
    parserMode = parser.ParseComments;
    if (allErrors.val) {
        parserMode |= parser.AllErrors;
    }
}

private static bool isGoFile(fs.DirEntry f) { 
    // ignore non-Go files
    var name = f.Name();
    return !f.IsDir() && !strings.HasPrefix(name, ".") && strings.HasSuffix(name, ".go");
}

// If in == nil, the source is the contents of the file with the given filename.
private static error processFile(@string filename, io.Reader @in, io.Writer @out, bool stdin) => func((defer, _, _) => {
    fs.FileMode perm = 0644;
    if (in == null) {
        var (f, err) = os.Open(filename);
        if (err != null) {
            return error.As(err)!;
        }
        defer(f.Close());
        var (fi, err) = f.Stat();
        if (err != null) {
            return error.As(err)!;
        }
        in = f;
        perm = fi.Mode().Perm();
    }
    var (src, err) = io.ReadAll(in);
    if (err != null) {
        return error.As(err)!;
    }
    var (file, sourceAdj, indentAdj, err) = parse(fileSet, filename, src, stdin);
    if (err != null) {
        return error.As(err)!;
    }
    if (rewrite != null) {
        if (sourceAdj == null) {
            file = rewrite(file);
        }
        else
 {
            fmt.Fprintf(os.Stderr, "warning: rewrite ignored for incomplete programs\n");
        }
    }
    ast.SortImports(fileSet, file);

    if (simplifyAST.val) {
        simplify(file);
    }
    var (res, err) = format(fileSet, file, sourceAdj, indentAdj, src, new printer.Config(Mode:printerMode,Tabwidth:tabWidth));
    if (err != null) {
        return error.As(err)!;
    }
    if (!bytes.Equal(src, res)) { 
        // formatting has changed
        if (list.val) {
            fmt.Fprintln(out, filename);
        }
        if (write.val) { 
            // make a temporary backup before overwriting original
            var (bakname, err) = backupFile(filename + ".", src, perm);
            if (err != null) {
                return error.As(err)!;
            }
            err = os.WriteFile(filename, res, perm);
            if (err != null) {
                os.Rename(bakname, filename);
                return error.As(err)!;
            }
            err = os.Remove(bakname);
            if (err != null) {
                return error.As(err)!;
            }
        }
        if (doDiff.val) {
            var (data, err) = diffWithReplaceTempFile(src, res, filename);
            if (err != null) {
                return error.As(fmt.Errorf("computing diff: %s", err))!;
            }
            fmt.Fprintf(out, "diff -u %s %s\n", filepath.ToSlash(filename + ".orig"), filepath.ToSlash(filename));
            @out.Write(data);
        }
    }
    if (!list && !write && !doDiff.val) {
        _, err = @out.Write(res);
    }
    return error.As(err)!;
});

private static error visitFile(@string path, fs.DirEntry f, error err) {
    if (err != null || !isGoFile(f)) {
        return error.As(err)!;
    }
    {
        var err = processFile(path, null, os.Stdout, false);

        if (err != null) {
            report(err);
        }
    }
    return error.As(null!)!;
}

private static void Main() { 
    // call gofmtMain in a separate function
    // so that it can use defer and have them
    // run before the exit.
    gofmtMain();
    os.Exit(exitCode);
}

private static void gofmtMain() => func((defer, _, _) => {
    flag.Usage = usage;
    flag.Parse();

    if (cpuprofile != "".val) {
        var (f, err) = os.Create(cpuprofile.val);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "creating cpu profile: %s\n", err);
            exitCode = 2;
            return ;
        }
        defer(f.Close());
        pprof.StartCPUProfile(f);
        defer(pprof.StopCPUProfile());
    }
    initParserMode();
    initRewrite();

    var args = flag.Args();
    if (len(args) == 0) {
        if (write.val) {
            fmt.Fprintln(os.Stderr, "error: cannot use -w with standard input");
            exitCode = 2;
            return ;
        }
        {
            var err__prev2 = err;

            var err = processFile("<standard input>", os.Stdin, os.Stdout, true);

            if (err != null) {
                report(err);
            }

            err = err__prev2;

        }
        return ;
    }
    foreach (var (_, arg) in args) {
        {
            var err__prev1 = err;

            var (info, err) = os.Stat(arg);


            if (err != null) 
                report(err);
            else if (!info.IsDir()) 
                // Non-directory arguments are always formatted.
                {
                    var err__prev1 = err;

                    err = processFile(arg, null, os.Stdout, false);

                    if (err != null) {
                        report(err);
                    }

                    err = err__prev1;

                }
            else 
                // Directories are walked, ignoring non-Go files.
                {
                    var err__prev1 = err;

                    err = filepath.WalkDir(arg, visitFile);

                    if (err != null) {
                        report(err);
                    }

                    err = err__prev1;

                }


            err = err__prev1;
        }
    }
});

private static (slice<byte>, error) diffWithReplaceTempFile(slice<byte> b1, slice<byte> b2, @string filename) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (data, err) = diff.Diff("gofmt", b1, b2);
    if (len(data) > 0) {
        return replaceTempFilename(data, filename);
    }
    return (data, error.As(err)!);
}

// replaceTempFilename replaces temporary filenames in diff with actual one.
//
// --- /tmp/gofmt316145376    2017-02-03 19:13:00.280468375 -0500
// +++ /tmp/gofmt617882815    2017-02-03 19:13:00.280468375 -0500
// ...
// ->
// --- path/to/file.go.orig    2017-02-03 19:13:00.280468375 -0500
// +++ path/to/file.go    2017-02-03 19:13:00.280468375 -0500
// ...
private static (slice<byte>, error) replaceTempFilename(slice<byte> diff, @string filename) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var bs = bytes.SplitN(diff, new slice<byte>(new byte[] { '\n' }), 3);
    if (len(bs) < 3) {
        return (null, error.As(fmt.Errorf("got unexpected diff for %s", filename))!);
    }
    slice<byte> t0 = default;    slice<byte> t1 = default;

    {
        var i__prev1 = i;

        var i = bytes.LastIndexByte(bs[0], '\t');

        if (i != -1) {
            t0 = bs[0][(int)i..];
        }
        i = i__prev1;

    }
    {
        var i__prev1 = i;

        i = bytes.LastIndexByte(bs[1], '\t');

        if (i != -1) {
            t1 = bs[1][(int)i..];
        }
        i = i__prev1;

    } 
    // Always print filepath with slash separator.
    var f = filepath.ToSlash(filename);
    bs[0] = (slice<byte>)fmt.Sprintf("--- %s%s", f + ".orig", t0);
    bs[1] = (slice<byte>)fmt.Sprintf("+++ %s%s", f, t1);
    return (bytes.Join(bs, new slice<byte>(new byte[] { '\n' })), error.As(null!)!);
}

private static readonly var chmodSupported = runtime.GOOS != "windows";

// backupFile writes data to a new file named filename<number> with permissions perm,
// with <number randomly chosen such that the file name is unique. backupFile returns
// the chosen file name.


// backupFile writes data to a new file named filename<number> with permissions perm,
// with <number randomly chosen such that the file name is unique. backupFile returns
// the chosen file name.
private static (@string, error) backupFile(@string filename, slice<byte> data, fs.FileMode perm) {
    @string _p0 = default;
    error _p0 = default!;
 
    // create backup file
    var (f, err) = os.CreateTemp(filepath.Dir(filename), filepath.Base(filename));
    if (err != null) {
        return ("", error.As(err)!);
    }
    var bakname = f.Name();
    if (chmodSupported) {
        err = f.Chmod(perm);
        if (err != null) {
            f.Close();
            os.Remove(bakname);
            return (bakname, error.As(err)!);
        }
    }
    _, err = f.Write(data);
    {
        var err1 = f.Close();

        if (err == null) {
            err = err1;
        }
    }

    return (bakname, error.As(err)!);
}

} // end main_package
