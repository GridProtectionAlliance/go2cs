// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package txtar implements a trivial text-based file archive format.
//
// The goals for the format are:
//
//    - be trivial enough to create and edit by hand.
//    - be able to store trees of text files describing go command test cases.
//    - diff nicely in git history and code reviews.
//
// Non-goals include being a completely general archive format,
// storing binary data, storing file modes, storing special files like
// symbolic links, and so on.
//
// Txtar format
//
// A txtar archive is zero or more comment lines and then a sequence of file entries.
// Each file entry begins with a file marker line of the form "-- FILENAME --"
// and is followed by zero or more file content lines making up the file data.
// The comment or file content ends at the next file marker line.
// The file marker line must begin with the three-byte sequence "-- "
// and end with the three-byte sequence " --", but the enclosed
// file name can be surrounding by additional white space,
// all of which is stripped.
//
// If the txtar file is missing a trailing newline on the final line,
// parsers should consider a final newline to be present anyway.
//
// There are no possible syntax errors in a txtar archive.
// package txtar -- go2cs converted at 2022 March 06 23:19:44 UTC
// import "cmd/go/internal/txtar" ==> using txtar = go.cmd.go.@internal.txtar_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\txtar\archive.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;

namespace go.cmd.go.@internal;

public static partial class txtar_package {

    // An Archive is a collection of files.
public partial struct Archive {
    public slice<byte> Comment;
    public slice<File> Files;
}

// A File is a single file in an archive.
public partial struct File {
    public @string Name; // name of file ("foo/bar.txt")
    public slice<byte> Data; // text content of file
}

// Format returns the serialized form of an Archive.
// It is assumed that the Archive data structure is well-formed:
// a.Comment and all a.File[i].Data contain no file marker lines,
// and all a.File[i].Name is non-empty.
public static slice<byte> Format(ptr<Archive> _addr_a) {
    ref Archive a = ref _addr_a.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    buf.Write(fixNL(a.Comment));
    foreach (var (_, f) in a.Files) {
        fmt.Fprintf(_addr_buf, "-- %s --\n", f.Name);
        buf.Write(fixNL(f.Data));
    }    return buf.Bytes();
}

// ParseFile parses the named file as an archive.
public static (ptr<Archive>, error) ParseFile(@string file) {
    ptr<Archive> _p0 = default!;
    error _p0 = default!;

    var (data, err) = os.ReadFile(file);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_Parse(data)!, error.As(null!)!);

}

// Parse parses the serialized form of an Archive.
// The returned Archive holds slices of data.
public static ptr<Archive> Parse(slice<byte> data) {
    ptr<Archive> a = @new<Archive>();
    @string name = default;
    a.Comment, name, data = findFileMarker(data);
    while (name != "") {
        File f = new File(name,nil);
        f.Data, name, data = findFileMarker(data);
        a.Files = append(a.Files, f);
    }
    return _addr_a!;
}

private static slice<byte> newlineMarker = (slice<byte>)"\n-- ";private static slice<byte> marker = (slice<byte>)"-- ";private static slice<byte> markerEnd = (slice<byte>)" --";

// findFileMarker finds the next file marker in data,
// extracts the file name, and returns the data before the marker,
// the file name, and the data after the marker.
// If there is no next marker, findFileMarker returns before = fixNL(data), name = "", after = nil.
private static (slice<byte>, @string, slice<byte>) findFileMarker(slice<byte> data) {
    slice<byte> before = default;
    @string name = default;
    slice<byte> after = default;

    nint i = default;
    while (true) {
        name, after = isMarker(data[(int)i..]);

        if (name != "") {
            return (data[..(int)i], name, after);
        }
        var j = bytes.Index(data[(int)i..], newlineMarker);
        if (j < 0) {
            return (fixNL(data), "", null);
        }
        i += j + 1; // positioned at start of new possible marker
    }

}

// isMarker checks whether data begins with a file marker line.
// If so, it returns the name from the line and the data after the line.
// Otherwise it returns name == "" with an unspecified after.
private static (@string, slice<byte>) isMarker(slice<byte> data) {
    @string name = default;
    slice<byte> after = default;

    if (!bytes.HasPrefix(data, marker)) {
        return ("", null);
    }
    {
        var i = bytes.IndexByte(data, '\n');

        if (i >= 0) {
            (data, after) = (data[..(int)i], data[(int)i + 1..]);
        }
    }

    if (!bytes.HasSuffix(data, markerEnd)) {
        return ("", null);
    }
    return (strings.TrimSpace(string(data[(int)len(marker)..(int)len(data) - len(markerEnd)])), after);

}

// If data is empty or ends in \n, fixNL returns data.
// Otherwise fixNL returns a new slice consisting of data with a final \n added.
private static slice<byte> fixNL(slice<byte> data) {
    if (len(data) == 0 || data[len(data) - 1] == '\n') {
        return data;
    }
    var d = make_slice<byte>(len(data) + 1);
    copy(d, data);
    d[len(data)] = '\n';
    return d;

}

} // end txtar_package
