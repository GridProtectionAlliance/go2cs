// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tlog -- go2cs converted at 2022 March 06 23:19:05 UTC
// import "golang.org/x/mod/sumdb/tlog" ==> using tlog = go.golang.org.x.mod.sumdb.tlog_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\tlog\note.go
using bytes = go.bytes_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

namespace go.golang.org.x.mod.sumdb;

public static partial class tlog_package {

    // A Tree is a tree description, to be signed by a go.sum database server.
public partial struct Tree {
    public long N;
    public Hash Hash;
}

// FormatTree formats a tree description for inclusion in a note.
//
// The encoded form is three lines, each ending in a newline (U+000A):
//
//    go.sum database tree
//    N
//    Hash
//
// where N is in decimal and Hash is in base64.
//
// A future backwards-compatible encoding may add additional lines,
// which the parser can ignore.
// A future backwards-incompatible encoding would use a different
// first line (for example, "go.sum database tree v2").
public static slice<byte> FormatTree(Tree tree) {
    return (slice<byte>)fmt.Sprintf("go.sum database tree\n%d\n%s\n", tree.N, tree.Hash);
}

private static var errMalformedTree = errors.New("malformed tree note");
private static slice<byte> treePrefix = (slice<byte>)"go.sum database tree\n";

// ParseTree parses a formatted tree root description.
public static (Tree, error) ParseTree(slice<byte> text) {
    Tree tree = default;
    error err = default!;
 
    // The message looks like:
    //
    //    go.sum database tree
    //    2
    //    nND/nri/U0xuHUrYSy0HtMeal2vzD9V4k/BO79C+QeI=
    //
    // For forwards compatibility, extra text lines after the encoding are ignored.
    if (!bytes.HasPrefix(text, treePrefix) || bytes.Count(text, (slice<byte>)"\n") < 3 || len(text) > 1e6F) {
        return (new Tree(), error.As(errMalformedTree)!);
    }
    var lines = strings.SplitN(string(text), "\n", 4);
    var (n, err) = strconv.ParseInt(lines[1], 10, 64);
    if (err != null || n < 0 || lines[1] != strconv.FormatInt(n, 10)) {
        return (new Tree(), error.As(errMalformedTree)!);
    }
    var (h, err) = base64.StdEncoding.DecodeString(lines[2]);
    if (err != null || len(h) != HashSize) {
        return (new Tree(), error.As(errMalformedTree)!);
    }
    Hash hash = default;
    copy(hash[..], h);
    return (new Tree(n,hash), error.As(null!)!);
}

private static var errMalformedRecord = errors.New("malformed record data");

// FormatRecord formats a record for serving to a client
// in a lookup response or data tile.
//
// The encoded form is the record ID as a single number,
// then the text of the record, and then a terminating blank line.
// Record text must be valid UTF-8 and must not contain any ASCII control
// characters (those below U+0020) other than newline (U+000A).
// It must end in a terminating newline and not contain any blank lines.
public static (slice<byte>, error) FormatRecord(long id, slice<byte> text) {
    slice<byte> msg = default;
    error err = default!;

    if (!isValidRecordText(text)) {
        return (null, error.As(errMalformedRecord)!);
    }
    msg = (slice<byte>)fmt.Sprintf("%d\n", id);
    msg = append(msg, text);
    msg = append(msg, '\n');
    return (msg, error.As(null!)!);
}

// isValidRecordText reports whether text is syntactically valid record text.
private static bool isValidRecordText(slice<byte> text) {
    int last = default;
    {
        nint i = 0;

        while (i < len(text)) {
            var (r, size) = utf8.DecodeRune(text[(int)i..]);
            if (r < 0x20 && r != '\n' || r == utf8.RuneError && size == 1 || last == '\n' && r == '\n') {
                return false;
            }
            i += size;
            last = r;
        }
    }
    if (last != '\n') {
        return false;
    }
    return true;
}

// ParseRecord parses a record description at the start of text,
// stopping immediately after the terminating blank line.
// It returns the record id, the record text, and the remainder of text.
public static (long, slice<byte>, slice<byte>, error) ParseRecord(slice<byte> msg) {
    long id = default;
    slice<byte> text = default;
    slice<byte> rest = default;
    error err = default!;
 
    // Leading record id.
    var i = bytes.IndexByte(msg, '\n');
    if (i < 0) {
        return (0, null, null, error.As(errMalformedRecord)!);
    }
    id, err = strconv.ParseInt(string(msg[..(int)i]), 10, 64);
    if (err != null) {
        return (0, null, null, error.As(errMalformedRecord)!);
    }
    msg = msg[(int)i + 1..]; 

    // Record text.
    i = bytes.Index(msg, (slice<byte>)"\n\n");
    if (i < 0) {
        return (0, null, null, error.As(errMalformedRecord)!);
    }
    (text, rest) = (msg[..(int)i + 1], msg[(int)i + 2..]);    if (!isValidRecordText(text)) {
        return (0, null, null, error.As(errMalformedRecord)!);
    }
    return (id, text, rest, error.As(null!)!);
}

} // end tlog_package
