// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dirhash defines hashes over directory trees.
// These hashes are recorded in go.sum files and in the Go checksum database,
// to allow verifying that a newly-downloaded module has the expected content.
// package dirhash -- go2cs converted at 2022 March 06 23:18:55 UTC
// import "golang.org/x/mod/sumdb/dirhash" ==> using dirhash = go.golang.org.x.mod.sumdb.dirhash_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\dirhash\hash.go
using zip = go.archive.zip_package;
using sha256 = go.crypto.sha256_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using System;


namespace go.golang.org.x.mod.sumdb;

public static partial class dirhash_package {

    // DefaultHash is the default hash function used in new go.sum entries.
public static Hash DefaultHash = Hash1;

// A Hash is a directory hash function.
// It accepts a list of files along with a function that opens the content of each file.
// It opens, reads, hashes, and closes each file and returns the overall directory hash.
public delegate  error) Hash(slice<@string>,  Func<@string,  (io.ReadCloser,  error)>,  (@string);

// Hash1 is the "h1:" directory hash function, using SHA-256.
//
// Hash1 is "h1:" followed by the base64-encoded SHA-256 hash of a summary
// prepared as if by the Unix command:
//
//    find . -type f | sort | sha256sum
//
// More precisely, the hashed summary contains a single line for each file in the list,
// ordered by sort.Strings applied to the file names, where each line consists of
// the hexadecimal SHA-256 hash of the file content,
// two spaces (U+0020), the file name, and a newline (U+000A).
//
// File names with newlines (U+000A) are disallowed.
public static (@string, error) Hash1(slice<@string> files, Func<@string, (io.ReadCloser, error)> open) {
    @string _p0 = default;
    error _p0 = default!;

    var h = sha256.New();
    files = append((slice<@string>)null, files);
    sort.Strings(files);
    foreach (var (_, file) in files) {
        if (strings.Contains(file, "\n")) {
            return ("", error.As(errors.New("dirhash: filenames with newlines are not supported"))!);
        }
        var (r, err) = open(file);
        if (err != null) {
            return ("", error.As(err)!);
        }
        var hf = sha256.New();
        _, err = io.Copy(hf, r);
        r.Close();
        if (err != null) {
            return ("", error.As(err)!);
        }
        fmt.Fprintf(h, "%x  %s\n", hf.Sum(null), file);
    }    return ("h1:" + base64.StdEncoding.EncodeToString(h.Sum(null)), error.As(null!)!);
}

// HashDir returns the hash of the local file system directory dir,
// replacing the directory name itself with prefix in the file names
// used in the hash function.
public static (@string, error) HashDir(@string dir, @string prefix, Hash hash) {
    @string _p0 = default;
    error _p0 = default!;

    var (files, err) = DirFiles(dir, prefix);
    if (err != null) {
        return ("", error.As(err)!);
    }
    Func<@string, (io.ReadCloser, error)> osOpen = name => os.Open(filepath.Join(dir, strings.TrimPrefix(name, prefix)));
    return hash(files, osOpen);
}

// DirFiles returns the list of files in the tree rooted at dir,
// replacing the directory name dir with prefix in each name.
// The resulting names always use forward slashes.
public static (slice<@string>, error) DirFiles(@string dir, @string prefix) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    slice<@string> files = default;
    dir = filepath.Clean(dir);
    var err = filepath.Walk(dir, (file, info, err) => {
        if (err != null) {
            return err;
        }
        if (info.IsDir()) {
            return null;
        }
        var rel = file;
        if (dir != ".") {
            rel = file[(int)len(dir) + 1..];
        }
        var f = filepath.Join(prefix, rel);
        files = append(files, filepath.ToSlash(f));
        return null;
    });
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (files, error.As(null!)!);
}

// HashZip returns the hash of the file content in the named zip file.
// Only the file names and their contents are included in the hash:
// the exact zip file format encoding, compression method,
// per-file modification times, and other metadata are ignored.
public static (@string, error) HashZip(@string zipfile, Hash hash) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;

    var (z, err) = zip.OpenReader(zipfile);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(z.Close());
    slice<@string> files = default;
    var zfiles = make_map<@string, ptr<zip.File>>();
    foreach (var (_, file) in z.File) {
        files = append(files, file.Name);
        zfiles[file.Name] = file;
    }    Func<@string, (io.ReadCloser, error)> zipOpen = name => {
        var f = zfiles[name];
        if (f == null) {
            return (null, error.As(fmt.Errorf("file %q not found in zip", name))!); // should never happen
        }
        return f.Open();
    };
    return hash(files, zipOpen);
});

} // end dirhash_package
