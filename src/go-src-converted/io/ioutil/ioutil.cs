// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ioutil implements some I/O utility functions.
//
// As of Go 1.16, the same functionality is now provided
// by package io or package os, and those implementations
// should be preferred in new code.
// See the specific function documentation for details.
// package ioutil -- go2cs converted at 2022 March 06 22:12:46 UTC
// import "io/ioutil" ==> using ioutil = go.io.ioutil_package
// Original source: C:\Program Files\Go\src\io\ioutil\ioutil.go
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using sort = go.sort_package;
using System;


namespace go.io;

public static partial class ioutil_package {

    // ReadAll reads from r until an error or EOF and returns the data it read.
    // A successful call returns err == nil, not err == EOF. Because ReadAll is
    // defined to read from src until EOF, it does not treat an EOF from Read
    // as an error to be reported.
    //
    // As of Go 1.16, this function simply calls io.ReadAll.
public static (slice<byte>, error) ReadAll(io.Reader r) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return io.ReadAll(r);
}

// ReadFile reads the file named by filename and returns the contents.
// A successful call returns err == nil, not err == EOF. Because ReadFile
// reads the whole file, it does not treat an EOF from Read as an error
// to be reported.
//
// As of Go 1.16, this function simply calls os.ReadFile.
public static (slice<byte>, error) ReadFile(@string filename) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return os.ReadFile(filename);
}

// WriteFile writes data to a file named by filename.
// If the file does not exist, WriteFile creates it with permissions perm
// (before umask); otherwise WriteFile truncates it before writing, without changing permissions.
//
// As of Go 1.16, this function simply calls os.WriteFile.
public static error WriteFile(@string filename, slice<byte> data, fs.FileMode perm) {
    return error.As(os.WriteFile(filename, data, perm))!;
}

// ReadDir reads the directory named by dirname and returns
// a list of fs.FileInfo for the directory's contents,
// sorted by filename. If an error occurs reading the directory,
// ReadDir returns no directory entries along with the error.
//
// As of Go 1.16, os.ReadDir is a more efficient and correct choice:
// it returns a list of fs.DirEntry instead of fs.FileInfo,
// and it returns partial results in the case of an error
// midway through reading a directory.
public static (slice<fs.FileInfo>, error) ReadDir(@string dirname) {
    slice<fs.FileInfo> _p0 = default;
    error _p0 = default!;

    var (f, err) = os.Open(dirname);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (list, err) = f.Readdir(-1);
    f.Close();
    if (err != null) {
        return (null, error.As(err)!);
    }
    sort.Slice(list, (i, j) => list[i].Name() < list[j].Name());
    return (list, error.As(null!)!);

}

// NopCloser returns a ReadCloser with a no-op Close method wrapping
// the provided Reader r.
//
// As of Go 1.16, this function simply calls io.NopCloser.
public static io.ReadCloser NopCloser(io.Reader r) {
    return io.NopCloser(r);
}

// Discard is an io.Writer on which all Write calls succeed
// without doing anything.
//
// As of Go 1.16, this value is simply io.Discard.
public static io.Writer Discard = io.Discard;

} // end ioutil_package
