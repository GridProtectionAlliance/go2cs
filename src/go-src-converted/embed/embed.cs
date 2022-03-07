// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package embed provides access to files embedded in the running Go program.
//
// Go source files that import "embed" can use the //go:embed directive
// to initialize a variable of type string, []byte, or FS with the contents of
// files read from the package directory or subdirectories at compile time.
//
// For example, here are three ways to embed a file named hello.txt
// and then print its contents at run time.
//
// Embedding one file into a string:
//
//    import _ "embed"
//
//    //go:embed hello.txt
//    var s string
//    print(s)
//
// Embedding one file into a slice of bytes:
//
//    import _ "embed"
//
//    //go:embed hello.txt
//    var b []byte
//    print(string(b))
//
// Embedded one or more files into a file system:
//
//    import "embed"
//
//    //go:embed hello.txt
//    var f embed.FS
//    data, _ := f.ReadFile("hello.txt")
//    print(string(data))
//
// Directives
//
// A //go:embed directive above a variable declaration specifies which files to embed,
// using one or more path.Match patterns.
//
// The directive must immediately precede a line containing the declaration of a single variable.
// Only blank lines and ‘//’ line comments are permitted between the directive and the declaration.
//
// The type of the variable must be a string type, or a slice of a byte type,
// or FS (or an alias of FS).
//
// For example:
//
//    package server
//
//    import "embed"
//
//    // content holds our static web server content.
//    //go:embed image/* template/*
//    //go:embed html/index.html
//    var content embed.FS
//
// The Go build system will recognize the directives and arrange for the declared variable
// (in the example above, content) to be populated with the matching files from the file system.
//
// The //go:embed directive accepts multiple space-separated patterns for
// brevity, but it can also be repeated, to avoid very long lines when there are
// many patterns. The patterns are interpreted relative to the package directory
// containing the source file. The path separator is a forward slash, even on
// Windows systems. Patterns may not contain ‘.’ or ‘..’ or empty path elements,
// nor may they begin or end with a slash. To match everything in the current
// directory, use ‘*’ instead of ‘.’. To allow for naming files with spaces in
// their names, patterns can be written as Go double-quoted or back-quoted
// string literals.
//
// If a pattern names a directory, all files in the subtree rooted at that directory are
// embedded (recursively), except that files with names beginning with ‘.’ or ‘_’
// are excluded. So the variable in the above example is almost equivalent to:
//
//    // content is our static web server content.
//    //go:embed image template html/index.html
//    var content embed.FS
//
// The difference is that ‘image/*’ embeds ‘image/.tempfile’ while ‘image’ does not.
//
// The //go:embed directive can be used with both exported and unexported variables,
// depending on whether the package wants to make the data available to other packages.
// It can only be used with global variables at package scope,
// not with local variables.
//
// Patterns must not match files outside the package's module, such as ‘.git/*’ or symbolic links.
// Matches for empty directories are ignored. After that, each pattern in a //go:embed line
// must match at least one file or non-empty directory.
//
// If any patterns are invalid or have invalid matches, the build will fail.
//
// Strings and Bytes
//
// The //go:embed line for a variable of type string or []byte can have only a single pattern,
// and that pattern can match only a single file. The string or []byte is initialized with
// the contents of that file.
//
// The //go:embed directive requires importing "embed", even when using a string or []byte.
// In source files that don't refer to embed.FS, use a blank import (import _ "embed").
//
// File Systems
//
// For embedding a single file, a variable of type string or []byte is often best.
// The FS type enables embedding a tree of files, such as a directory of static
// web server content, as in the example above.
//
// FS implements the io/fs package's FS interface, so it can be used with any package that
// understands file systems, including net/http, text/template, and html/template.
//
// For example, given the content variable in the example above, we can write:
//
//    http.Handle("/static/", http.StripPrefix("/static/", http.FileServer(http.FS(content))))
//
//    template.ParseFS(content, "*.tmpl")
//
// Tools
//
// To support tools that analyze Go packages, the patterns found in //go:embed lines
// are available in “go list” output. See the EmbedPatterns, TestEmbedPatterns,
// and XTestEmbedPatterns fields in the “go help list” output.
//
// package embed -- go2cs converted at 2022 March 06 23:35:42 UTC
// import "embed" ==> using embed = go.embed_package
// Original source: C:\Program Files\Go\src\embed\embed.go
using errors = go.errors_package;
using io = go.io_package;
using fs = go.io.fs_package;
using time = go.time_package;
using System;


namespace go;

public static partial class embed_package {

    // An FS is a read-only collection of files, usually initialized with a //go:embed directive.
    // When declared without a //go:embed directive, an FS is an empty file system.
    //
    // An FS is a read-only value, so it is safe to use from multiple goroutines
    // simultaneously and also safe to assign values of type FS to each other.
    //
    // FS implements fs.FS, so it can be used with any package that understands
    // file system interfaces, including net/http, text/template, and html/template.
    //
    // See the package documentation for more details about initializing an FS.
public partial struct FS {
    public ptr<slice<file>> files;
}

// split splits the name into dir and elem as described in the
// comment in the FS struct above. isDir reports whether the
// final trailing slash was present, indicating that name is a directory.
private static (@string, @string, bool) split(@string name) {
    @string dir = default;
    @string elem = default;
    bool isDir = default;

    if (name[len(name) - 1] == '/') {
        isDir = true;
        name = name[..(int)len(name) - 1];
    }
    var i = len(name) - 1;
    while (i >= 0 && name[i] != '/') {
        i--;
    }
    if (i < 0) {
        return (".", name, isDir);
    }
    return (name[..(int)i], name[(int)i + 1..], isDir);

}

// trimSlash trims a trailing slash from name, if present,
// returning the possibly shortened name.
private static @string trimSlash(@string name) {
    if (len(name) > 0 && name[len(name) - 1] == '/') {
        return name[..(int)len(name) - 1];
    }
    return name;

}

private static fs.ReadDirFS _ = new FS();private static fs.ReadFileFS _ = new FS();

// A file is a single file in the FS.
// It implements fs.FileInfo and fs.DirEntry.
private partial struct file {
    public @string name;
    public @string data;
    public array<byte> hash; // truncated SHA256 hash
}

private static fs.FileInfo _ = (file.val)(null);private static fs.DirEntry _ = (file.val)(null);

private static @string Name(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    var (_, elem, _) = split(f.name);

    return elem;
}
private static long Size(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return int64(len(f.data));
}
private static time.Time ModTime(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return new time.Time();
}
private static bool IsDir(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    var (_, _, isDir) = split(f.name);

    return isDir;
}
private static void Sys(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return null;
}
private static fs.FileMode Type(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return f.Mode().Type();
}
private static (fs.FileInfo, error) Info(this ptr<file> _addr_f) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;

    return (f, error.As(null!)!);
}

private static fs.FileMode Mode(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    if (f.IsDir()) {
        return fs.ModeDir | 0555;
    }
    return 0444;

}

// dotFile is a file for the root directory,
// which is omitted from the files list in a FS.
private static ptr<file> dotFile = addr(new file(name:"./"));

// lookup returns the named file, or nil if it is not present.
public static ptr<file> lookup(this FS f, @string name) {
    if (!fs.ValidPath(name)) { 
        // The compiler should never emit a file with an invalid name,
        // so this check is not strictly necessary (if name is invalid,
        // we shouldn't find a match below), but it's a good backstop anyway.
        return _addr_null!;

    }
    if (name == ".") {
        return _addr_dotFile!;
    }
    if (f.files == null) {
        return _addr_null!;
    }
    var (dir, elem, _) = split(name);
    var files = f.files.val;
    var i = sortSearch(len(files), i => {
        var (idir, ielem, _) = split(files[i].name);
        return _addr_idir > dir || idir == dir && ielem >= elem!;
    });
    if (i < len(files) && trimSlash(files[i].name) == name) {
        return _addr__addr_files[i]!;
    }
    return _addr_null!;

}

// readDir returns the list of files corresponding to the directory dir.
public static slice<file> readDir(this FS f, @string dir) {
    if (f.files == null) {
        return null;
    }
    var files = f.files.val;
    var i = sortSearch(len(files), i => {
        var (idir, _, _) = split(files[i].name);
        return idir >= dir;
    });
    var j = sortSearch(len(files), j => {
        var (jdir, _, _) = split(files[j].name);
        return jdir > dir;
    });
    return files[(int)i..(int)j];

}

// Open opens the named file for reading and returns it as an fs.File.
public static (fs.File, error) Open(this FS f, @string name) {
    fs.File _p0 = default;
    error _p0 = default!;

    var file = f.lookup(name);
    if (file == null) {
        return (null, error.As(addr(new fs.PathError(Op:"open",Path:name,Err:fs.ErrNotExist))!)!);
    }
    if (file.IsDir()) {
        return (addr(new openDir(file,f.readDir(name),0)), error.As(null!)!);
    }
    return (addr(new openFile(file,0)), error.As(null!)!);

}

// ReadDir reads and returns the entire named directory.
public static (slice<fs.DirEntry>, error) ReadDir(this FS f, @string name) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;

    var (file, err) = f.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ptr<openDir> (dir, ok) = file._<ptr<openDir>>();
    if (!ok) {
        return (null, error.As(addr(new fs.PathError(Op:"read",Path:name,Err:errors.New("not a directory")))!)!);
    }
    var list = make_slice<fs.DirEntry>(len(dir.files));
    foreach (var (i) in list) {
        list[i] = _addr_dir.files[i];
    }    return (list, error.As(null!)!);

}

// ReadFile reads and returns the content of the named file.
public static (slice<byte>, error) ReadFile(this FS f, @string name) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (file, err) = f.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ptr<openFile> (ofile, ok) = file._<ptr<openFile>>();
    if (!ok) {
        return (null, error.As(addr(new fs.PathError(Op:"read",Path:name,Err:errors.New("is a directory")))!)!);
    }
    return ((slice<byte>)ofile.f.data, error.As(null!)!);

}

// An openFile is a regular file open for reading.
private partial struct openFile {
    public ptr<file> f; // the file itself
    public long offset; // current read offset
}

private static error Close(this ptr<openFile> _addr_f) {
    ref openFile f = ref _addr_f.val;

    return error.As(null!)!;
}
private static (fs.FileInfo, error) Stat(this ptr<openFile> _addr_f) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref openFile f = ref _addr_f.val;

    return (f.f, error.As(null!)!);
}

private static (nint, error) Read(this ptr<openFile> _addr_f, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref openFile f = ref _addr_f.val;

    if (f.offset >= int64(len(f.f.data))) {
        return (0, error.As(io.EOF)!);
    }
    if (f.offset < 0) {
        return (0, error.As(addr(new fs.PathError(Op:"read",Path:f.f.name,Err:fs.ErrInvalid))!)!);
    }
    var n = copy(b, f.f.data[(int)f.offset..]);
    f.offset += int64(n);
    return (n, error.As(null!)!);

}

private static (long, error) Seek(this ptr<openFile> _addr_f, long offset, nint whence) {
    long _p0 = default;
    error _p0 = default!;
    ref openFile f = ref _addr_f.val;

    switch (whence) {
        case 0: 

            break;
        case 1: 
            offset += f.offset;
            break;
        case 2: 
            offset += int64(len(f.f.data));
            break;
    }
    if (offset < 0 || offset > int64(len(f.f.data))) {
        return (0, error.As(addr(new fs.PathError(Op:"seek",Path:f.f.name,Err:fs.ErrInvalid))!)!);
    }
    f.offset = offset;
    return (offset, error.As(null!)!);

}

// An openDir is a directory open for reading.
private partial struct openDir {
    public ptr<file> f; // the directory file itself
    public slice<file> files; // the directory contents
    public nint offset; // the read offset, an index into the files slice
}

private static error Close(this ptr<openDir> _addr_d) {
    ref openDir d = ref _addr_d.val;

    return error.As(null!)!;
}
private static (fs.FileInfo, error) Stat(this ptr<openDir> _addr_d) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref openDir d = ref _addr_d.val;

    return (d.f, error.As(null!)!);
}

private static (nint, error) Read(this ptr<openDir> _addr_d, slice<byte> _p0) {
    nint _p0 = default;
    error _p0 = default!;
    ref openDir d = ref _addr_d.val;

    return (0, error.As(addr(new fs.PathError(Op:"read",Path:d.f.name,Err:errors.New("is a directory")))!)!);
}

private static (slice<fs.DirEntry>, error) ReadDir(this ptr<openDir> _addr_d, nint count) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;
    ref openDir d = ref _addr_d.val;

    var n = len(d.files) - d.offset;
    if (n == 0) {
        if (count <= 0) {
            return (null, error.As(null!)!);
        }
        return (null, error.As(io.EOF)!);

    }
    if (count > 0 && n > count) {
        n = count;
    }
    var list = make_slice<fs.DirEntry>(n);
    foreach (var (i) in list) {
        list[i] = _addr_d.files[d.offset + i];
    }    d.offset += n;
    return (list, error.As(null!)!);

}

// sortSearch is like sort.Search, avoiding an import.
private static nint sortSearch(nint n, Func<nint, bool> f) { 
    // Define f(-1) == false and f(n) == true.
    // Invariant: f(i-1) == false, f(j) == true.
    nint i = 0;
    var j = n;
    while (i < j) {
        var h = int(uint(i + j) >> 1); // avoid overflow when computing h
        // i ≤ h < j
        if (!f(h)) {
            i = h + 1; // preserves f(i-1) == false
        }
        else
 {
            j = h; // preserves f(j) == true
        }
    } 
    // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.
    return i;

}

} // end embed_package
