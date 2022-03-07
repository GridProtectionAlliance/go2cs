// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:15:47 UTC
// Original source: C:\Program Files\Go\src\cmd\fix\main.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using parser = go.go.parser_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;

using diff = go.cmd.@internal.diff_package;

namespace go;

public static partial class main_package {

private static var fset = token.NewFileSet();private static nint exitCode = 0;

private static var allowedRewrites = flag.String("r", "", "restrict the rewrites to this comma-separated list");

private static var forceRewrites = flag.String("force", "", "force these fixes to run even if the code looks updated");

private static map<@string, bool> allowed = default;private static map<@string, bool> force = default;



private static var doDiff = flag.Bool("diff", false, "display diffs instead of rewriting files");

// enable for debugging fix failures
private static readonly var debug = false; // display incorrectly reformatted source and exit

 // display incorrectly reformatted source and exit

private static void usage() {
    fmt.Fprintf(os.Stderr, "usage: go tool fix [-diff] [-r fixname,...] [-force fixname,...] [path ...]\n");
    flag.PrintDefaults();
    fmt.Fprintf(os.Stderr, "\nAvailable rewrites are:\n");
    sort.Sort(byName(fixes));
    foreach (var (_, f) in fixes) {
        if (f.disabled) {
            fmt.Fprintf(os.Stderr, "\n%s (disabled)\n", f.name);
        }
        else
 {
            fmt.Fprintf(os.Stderr, "\n%s\n", f.name);
        }
        var desc = strings.TrimSpace(f.desc);
        desc = strings.ReplaceAll(desc, "\n", "\n\t");
        fmt.Fprintf(os.Stderr, "\t%s\n", desc);

    }    os.Exit(2);

}

private static void Main() {
    flag.Usage = usage;
    flag.Parse();

    sort.Sort(byDate(fixes));

    if (allowedRewrites != "".val) {
        allowed = make_map<@string, bool>();
        {
            var f__prev1 = f;

            foreach (var (_, __f) in strings.Split(allowedRewrites.val, ",")) {
                f = __f;
                allowed[f] = true;
            }

            f = f__prev1;
        }
    }
    if (forceRewrites != "".val) {
        force = make_map<@string, bool>();
        {
            var f__prev1 = f;

            foreach (var (_, __f) in strings.Split(forceRewrites.val, ",")) {
                f = __f;
                force[f] = true;
            }

            f = f__prev1;
        }
    }
    if (flag.NArg() == 0) {
        {
            var err__prev2 = err;

            var err = processFile("standard input", true);

            if (err != null) {
                report(err);
            }

            err = err__prev2;

        }

        os.Exit(exitCode);

    }
    for (nint i = 0; i < flag.NArg(); i++) {
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

                    if (err != null) {
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



private static (slice<byte>, error) gofmtFile(ptr<ast.File> _addr_f) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref ast.File f = ref _addr_f.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    {
        var err = format.Node(_addr_buf, fset, f);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    return (buf.Bytes(), error.As(null!)!);

}

private static error processFile(@string filename, bool useStdin) => func((defer, _, _) => {
    ptr<os.File> f;
    error err = default!;
    ref bytes.Buffer fixlog = ref heap(out ptr<bytes.Buffer> _addr_fixlog);

    if (useStdin) {
        f = os.Stdin;
    }
    else
 {
        f, err = os.Open(filename);
        if (err != null) {
            return error.As(err)!;
        }
        defer(f.Close());

    }
    var (src, err) = io.ReadAll(f);
    if (err != null) {
        return error.As(err)!;
    }
    var (file, err) = parser.ParseFile(fset, filename, src, parserMode);
    if (err != null) {
        return error.As(err)!;
    }
    var (newSrc, err) = gofmtFile(_addr_file);
    if (err != null) {
        return error.As(err)!;
    }
    if (!bytes.Equal(newSrc, src)) {
        var (newFile, err) = parser.ParseFile(fset, filename, newSrc, parserMode);
        if (err != null) {
            return error.As(err)!;
        }
        file = newFile;
        fmt.Fprintf(_addr_fixlog, " fmt");

    }
    var newFile = file;
    var @fixed = false;
    foreach (var (_, fix) in fixes) {
        if (allowed != null && !allowed[fix.name]) {
            continue;
        }
        if (fix.disabled && !force[fix.name]) {
            continue;
        }
        if (fix.f(newFile)) {
            fixed = true;
            fmt.Fprintf(_addr_fixlog, " %s", fix.name); 

            // AST changed.
            // Print and parse, to update any missing scoping
            // or position information for subsequent fixers.
            (newSrc, err) = gofmtFile(_addr_newFile);
            if (err != null) {
                return error.As(err)!;
            }

            newFile, err = parser.ParseFile(fset, filename, newSrc, parserMode);
            if (err != null) {
                if (debug) {
                    fmt.Printf("%s", newSrc);
                    report(err);
                    os.Exit(exitCode);
                }
                return error.As(err)!;
            }

        }
    }    if (!fixed) {
        return error.As(null!)!;
    }
    fmt.Fprintf(os.Stderr, "%s: fixed %s\n", filename, fixlog.String()[(int)1..]); 

    // Print AST.  We did that after each fix, so this appears
    // redundant, but it is necessary to generate gofmt-compatible
    // source code in a few cases. The official gofmt style is the
    // output of the printer run on a standard AST generated by the parser,
    // but the source we generated inside the loop above is the
    // output of the printer run on a mangled AST generated by a fixer.
    newSrc, err = gofmtFile(_addr_newFile);
    if (err != null) {
        return error.As(err)!;
    }
    if (doDiff.val) {
        var (data, err) = diff.Diff("go-fix", src, newSrc);
        if (err != null) {
            return error.As(fmt.Errorf("computing diff: %s", err))!;
        }
        fmt.Printf("diff %s fixed/%s\n", filename, filename);
        os.Stdout.Write(data);
        return error.As(null!)!;

    }
    if (useStdin) {
        os.Stdout.Write(newSrc);
        return error.As(null!)!;
    }
    return error.As(os.WriteFile(f.Name(), newSrc, 0))!;

});

private static @string gofmt(object n) {
    ref bytes.Buffer gofmtBuf = ref heap(out ptr<bytes.Buffer> _addr_gofmtBuf);
    {
        var err = format.Node(_addr_gofmtBuf, fset, n);

        if (err != null) {
            return "<" + err.Error() + ">";
        }
    }

    return gofmtBuf.String();

}

private static void report(error err) {
    scanner.PrintError(os.Stderr, err);
    exitCode = 2;
}

private static void walkDir(@string path) {
    filepath.WalkDir(path, visitFile);
}

private static error visitFile(@string path, fs.DirEntry f, error err) {
    if (err == null && isGoFile(f)) {
        err = processFile(path, false);
    }
    if (err != null) {
        report(err);
    }
    return error.As(null!)!;

}

private static bool isGoFile(fs.DirEntry f) { 
    // ignore non-Go files
    var name = f.Name();
    return !f.IsDir() && !strings.HasPrefix(name, ".") && strings.HasSuffix(name, ".go");

}

} // end main_package
