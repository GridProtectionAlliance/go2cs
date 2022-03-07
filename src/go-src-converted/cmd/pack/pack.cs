// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:22:38 UTC
// Original source: C:\Program Files\Go\src\cmd\pack\pack.go
using archive = go.cmd.@internal.archive_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using System;


namespace go;

public static partial class main_package {

private static readonly @string usageMessage = "Usage: pack op file.a [name....]\nWhere op is one of cprtx optionally followed by " +
    "v for verbose output.\nFor compatibility with old Go build environments the op st" +
    "ring grc is\naccepted as a synonym for c.\n\nFor more information, run\n\tgo doc cmd/" +
    "pack";



private static void usage() {
    fmt.Fprintln(os.Stderr, usageMessage);
    os.Exit(2);
}

private static void Main() {
    log.SetFlags(0);
    log.SetPrefix("pack: "); 
    // need "pack op archive" at least.
    if (len(os.Args) < 3) {
        log.Print("not enough arguments");
        fmt.Fprintln(os.Stderr);
        usage();
    }
    setOp(os.Args[1]);
    ptr<Archive> ar;
    switch (op) {
        case 'p': 
            ar = openArchive(os.Args[2], os.O_RDONLY, os.Args[(int)3..]);
            ar.scan(ar.printContents);
            break;
        case 'r': 
            ar = openArchive(os.Args[2], os.O_RDWR | os.O_CREATE, os.Args[(int)3..]);
            ar.addFiles();
            break;
        case 'c': 
            ar = openArchive(os.Args[2], os.O_RDWR | os.O_TRUNC | os.O_CREATE, os.Args[(int)3..]);
            ar.addPkgdef();
            ar.addFiles();
            break;
        case 't': 
            ar = openArchive(os.Args[2], os.O_RDONLY, os.Args[(int)3..]);
            ar.scan(ar.tableOfContents);
            break;
        case 'x': 
            ar = openArchive(os.Args[2], os.O_RDONLY, os.Args[(int)3..]);
            ar.scan(ar.extractContents);
            break;
        default: 
            log.Printf("invalid operation %q", os.Args[1]);
            fmt.Fprintln(os.Stderr);
            usage();
            break;
    }
    if (len(ar.files) > 0) {
        log.Fatalf("file %q not in archive", ar.files[0]);
    }
}

// The unusual ancestry means the arguments are not Go-standard.
// These variables hold the decoded operation specified by the first argument.
// op holds the operation we are doing (prtx).
// verbose tells whether the 'v' option was specified.
private static int op = default;private static bool verbose = default;

// setOp parses the operation string (first argument).
private static void setOp(@string arg) { 
    // Recognize 'go tool pack grc' because that was the
    // formerly canonical way to build a new archive
    // from a set of input files. Accepting it keeps old
    // build systems working with both Go 1.2 and Go 1.3.
    if (arg == "grc") {
        arg = "c";
    }
    foreach (var (_, r) in arg) {
        switch (r) {
            case 'c': 

            case 'p': 

            case 'r': 

            case 't': 

            case 'x': 
                if (op != 0) { 
                    // At most one can be set.
                    usage();

                }

                op = r;

                break;
            case 'v': 
                if (verbose) { 
                    // Can be set only once.
                    usage();

                }

                verbose = true;

                break;
            default: 
                usage();
                break;
        }

    }
}

private static readonly @string arHeader = "!<arch>\n";


// An Archive represents an open archive file. It is always scanned sequentially
// from start to end, without backing up.
public partial struct Archive {
    public ptr<archive.Archive> a;
    public slice<@string> files; // Explicit list of files to be processed.
    public nint pad; // Padding bytes required at end of current archive file
    public bool matchAll; // match all files in archive
}

// archive opens (and if necessary creates) the named archive.
private static ptr<Archive> openArchive(@string name, nint mode, slice<@string> files) {
    var (f, err) = os.OpenFile(name, mode, 0666);
    if (err != null) {
        log.Fatal(err);
    }
    ptr<archive.Archive> a;
    if (mode & os.O_TRUNC != 0) { // the c command
        a, err = archive.New(f);

    }
    else
 {
        a, err = archive.Parse(f, verbose);
        if (err != null && mode & os.O_CREATE != 0) { // the r command
            a, err = archive.New(f);

        }
    }
    if (err != null) {
        log.Fatal(err);
    }
    return addr(new Archive(a:a,files:files,matchAll:len(files)==0,));

}

// scan scans the archive and executes the specified action on each entry.
private static void scan(this ptr<Archive> _addr_ar, Action<ptr<archive.Entry>> action) {
    ref Archive ar = ref _addr_ar.val;

    foreach (var (i) in ar.a.Entries) {
        var e = _addr_ar.a.Entries[i];
        action(e);
    }
}

// listEntry prints to standard output a line describing the entry.
private static void listEntry(ptr<archive.Entry> _addr_e, bool verbose) {
    ref archive.Entry e = ref _addr_e.val;

    if (verbose) {
        fmt.Fprintf(stdout, "%s\n", e.String());
    }
    else
 {
        fmt.Fprintf(stdout, "%s\n", e.Name);
    }
}

// output copies the entry to the specified writer.
private static void output(this ptr<Archive> _addr_ar, ptr<archive.Entry> _addr_e, io.Writer w) {
    ref Archive ar = ref _addr_ar.val;
    ref archive.Entry e = ref _addr_e.val;

    var r = io.NewSectionReader(ar.a.File(), e.Offset, e.Size);
    var (n, err) = io.Copy(w, r);
    if (err != null) {
        log.Fatal(err);
    }
    if (n != e.Size) {
        log.Fatal("short file");
    }
}

// match reports whether the entry matches the argument list.
// If it does, it also drops the file from the to-be-processed list.
private static bool match(this ptr<Archive> _addr_ar, ptr<archive.Entry> _addr_e) {
    ref Archive ar = ref _addr_ar.val;
    ref archive.Entry e = ref _addr_e.val;

    if (ar.matchAll) {
        return true;
    }
    foreach (var (i, name) in ar.files) {
        if (e.Name == name) {
            copy(ar.files[(int)i..], ar.files[(int)i + 1..]);
            ar.files = ar.files[..(int)len(ar.files) - 1];
            return true;
        }
    }    return false;

}

// addFiles adds files to the archive. The archive is known to be
// sane and we are positioned at the end. No attempt is made
// to check for existing files.
private static void addFiles(this ptr<Archive> _addr_ar) {
    ref Archive ar = ref _addr_ar.val;

    if (len(ar.files) == 0) {
        usage();
    }
    foreach (var (_, file) in ar.files) {
        if (verbose) {
            fmt.Printf("%s\n", file);
        }
        var (f, err) = os.Open(file);
        if (err != null) {
            log.Fatal(err);
        }
        var (aro, err) = archive.Parse(f, false);
        if (err != null || !isGoCompilerObjFile(_addr_aro)) {
            f.Seek(0, io.SeekStart);
            ar.addFile(f);
            goto close;
        }
        foreach (var (_, e) in aro.Entries) {
            if (e.Type != archive.EntryGoObj || e.Name != "_go_.o") {
                continue;
            }
            ar.a.AddEntry(archive.EntryGoObj, filepath.Base(file), 0, 0, 0, 0644, e.Size, io.NewSectionReader(f, e.Offset, e.Size));
        }close:
        f.Close();

    }    ar.files = null;

}

// FileLike abstracts the few methods we need, so we can test without needing real files.
public partial interface FileLike {
    error Name();
    error Stat();
    error Read(slice<byte> _p0);
    error Close();
}

// addFile adds a single file to the archive
private static void addFile(this ptr<Archive> _addr_ar, FileLike fd) {
    ref Archive ar = ref _addr_ar.val;
 
    // Format the entry.
    // First, get its info.
    var (info, err) = fd.Stat();
    if (err != null) {
        log.Fatal(err);
    }
    var mtime = int64(0);
    nint uid = 0;
    nint gid = 0;
    ar.a.AddEntry(archive.EntryNativeObj, info.Name(), mtime, uid, gid, info.Mode(), info.Size(), fd);

}

// addPkgdef adds the __.PKGDEF file to the archive, copied
// from the first Go object file on the file list, if any.
// The archive is known to be empty.
private static void addPkgdef(this ptr<Archive> _addr_ar) {
    ref Archive ar = ref _addr_ar.val;

    var done = false;
    foreach (var (_, file) in ar.files) {
        var (f, err) = os.Open(file);
        if (err != null) {
            log.Fatal(err);
        }
        var (aro, err) = archive.Parse(f, false);
        if (err != null || !isGoCompilerObjFile(_addr_aro)) {
            goto close;
        }
        foreach (var (_, e) in aro.Entries) {
            if (e.Type != archive.EntryPkgDef) {
                continue;
            }
            if (verbose) {
                fmt.Printf("__.PKGDEF # %s\n", file);
            }
            ar.a.AddEntry(archive.EntryPkgDef, "__.PKGDEF", 0, 0, 0, 0644, e.Size, io.NewSectionReader(f, e.Offset, e.Size));
            done = true;
        }close:
        f.Close();
        if (done) {
            break;
        }
    }
}

// Finally, the actual commands. Each is an action.

// can be modified for testing.
private static io.Writer stdout = os.Stdout;

// printContents implements the 'p' command.
private static void printContents(this ptr<Archive> _addr_ar, ptr<archive.Entry> _addr_e) {
    ref Archive ar = ref _addr_ar.val;
    ref archive.Entry e = ref _addr_e.val;

    ar.extractContents1(e, stdout);
}

// tableOfContents implements the 't' command.
private static void tableOfContents(this ptr<Archive> _addr_ar, ptr<archive.Entry> _addr_e) {
    ref Archive ar = ref _addr_ar.val;
    ref archive.Entry e = ref _addr_e.val;

    if (ar.match(e)) {
        listEntry(_addr_e, verbose);
    }
}

// extractContents implements the 'x' command.
private static void extractContents(this ptr<Archive> _addr_ar, ptr<archive.Entry> _addr_e) {
    ref Archive ar = ref _addr_ar.val;
    ref archive.Entry e = ref _addr_e.val;

    ar.extractContents1(e, null);
}

private static void extractContents1(this ptr<Archive> _addr_ar, ptr<archive.Entry> _addr_e, io.Writer @out) => func((defer, _, _) => {
    ref Archive ar = ref _addr_ar.val;
    ref archive.Entry e = ref _addr_e.val;

    if (ar.match(e)) {
        if (verbose) {
            listEntry(_addr_e, false);
        }
        if (out == null) {
            var (f, err) = os.OpenFile(e.Name, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, 0444);
            if (err != null) {
                log.Fatal(err);
            }
            defer(f.Close());
            out = f;
        }
        ar.output(e, out);

    }
});

// isGoCompilerObjFile reports whether file is an object file created
// by the Go compiler, which is an archive file with exactly one entry
// of __.PKGDEF, or _go_.o, or both entries.
private static bool isGoCompilerObjFile(ptr<archive.Archive> _addr_a) {
    ref archive.Archive a = ref _addr_a.val;

    switch (len(a.Entries)) {
        case 1: 
            return (a.Entries[0].Type == archive.EntryGoObj && a.Entries[0].Name == "_go_.o") || (a.Entries[0].Type == archive.EntryPkgDef && a.Entries[0].Name == "__.PKGDEF");
            break;
        case 2: 
            bool foundPkgDef = default;        bool foundGo = default;

            foreach (var (_, e) in a.Entries) {
                if (e.Type == archive.EntryPkgDef && e.Name == "__.PKGDEF") {
                    foundPkgDef = true;
                }
                if (e.Type == archive.EntryGoObj && e.Name == "_go_.o") {
                    foundGo = true;
                }
            }        return foundPkgDef && foundGo;
            break;
        default: 
            return false;
            break;
    }

}

} // end main_package
