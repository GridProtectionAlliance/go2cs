// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2022 March 13 05:39:53 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Program Files\Go\src\encoding\json\indent.go
namespace go.encoding;

using bytes = bytes_package;


// Compact appends to dst the JSON-encoded src with
// insignificant space characters elided.

public static partial class json_package {

public static error Compact(ptr<bytes.Buffer> _addr_dst, slice<byte> src) {
    ref bytes.Buffer dst = ref _addr_dst.val;

    return error.As(compact(_addr_dst, src, false))!;
}

private static error compact(ptr<bytes.Buffer> _addr_dst, slice<byte> src, bool escape) => func((defer, _, _) => {
    ref bytes.Buffer dst = ref _addr_dst.val;

    var origLen = dst.Len();
    var scan = newScanner();
    defer(freeScanner(scan));
    nint start = 0;
    foreach (var (i, c) in src) {
        if (escape && (c == '<' || c == '>' || c == '&')) {
            if (start < i) {
                dst.Write(src[(int)start..(int)i]);
            }
            dst.WriteString("\\u00");
            dst.WriteByte(hex[c >> 4]);
            dst.WriteByte(hex[c & 0xF]);
            start = i + 1;
        }
        if (escape && c == 0xE2 && i + 2 < len(src) && src[i + 1] == 0x80 && src[i + 2] & ~1 == 0xA8) {
            if (start < i) {
                dst.Write(src[(int)start..(int)i]);
            }
            dst.WriteString("\\u202");
            dst.WriteByte(hex[src[i + 2] & 0xF]);
            start = i + 3;
        }
        var v = scan.step(scan, c);
        if (v >= scanSkipSpace) {
            if (v == scanError) {
                break;
            }
            if (start < i) {
                dst.Write(src[(int)start..(int)i]);
            }
            start = i + 1;
        }
    }    if (scan.eof() == scanError) {
        dst.Truncate(origLen);
        return error.As(scan.err)!;
    }
    if (start < len(src)) {
        dst.Write(src[(int)start..]);
    }
    return error.As(null!)!;
});

private static void newline(ptr<bytes.Buffer> _addr_dst, @string prefix, @string indent, nint depth) {
    ref bytes.Buffer dst = ref _addr_dst.val;

    dst.WriteByte('\n');
    dst.WriteString(prefix);
    for (nint i = 0; i < depth; i++) {
        dst.WriteString(indent);
    }
}

// Indent appends to dst an indented form of the JSON-encoded src.
// Each element in a JSON object or array begins on a new,
// indented line beginning with prefix followed by one or more
// copies of indent according to the indentation nesting.
// The data appended to dst does not begin with the prefix nor
// any indentation, to make it easier to embed inside other formatted JSON data.
// Although leading space characters (space, tab, carriage return, newline)
// at the beginning of src are dropped, trailing space characters
// at the end of src are preserved and copied to dst.
// For example, if src has no trailing spaces, neither will dst;
// if src ends in a trailing newline, so will dst.
public static error Indent(ptr<bytes.Buffer> _addr_dst, slice<byte> src, @string prefix, @string indent) => func((defer, _, _) => {
    ref bytes.Buffer dst = ref _addr_dst.val;

    var origLen = dst.Len();
    var scan = newScanner();
    defer(freeScanner(scan));
    var needIndent = false;
    nint depth = 0;
    foreach (var (_, c) in src) {
        scan.bytes++;
        var v = scan.step(scan, c);
        if (v == scanSkipSpace) {
            continue;
        }
        if (v == scanError) {
            break;
        }
        if (needIndent && v != scanEndObject && v != scanEndArray) {
            needIndent = false;
            depth++;
            newline(_addr_dst, prefix, indent, depth);
        }
        if (v == scanContinue) {
            dst.WriteByte(c);
            continue;
        }
        switch (c) {
            case '{': 
                // delay indent so that empty object and array are formatted as {} and [].

            case '[': 
                // delay indent so that empty object and array are formatted as {} and [].
                needIndent = true;
                dst.WriteByte(c);
                break;
            case ',': 
                dst.WriteByte(c);
                newline(_addr_dst, prefix, indent, depth);
                break;
            case ':': 
                dst.WriteByte(c);
                dst.WriteByte(' ');
                break;
            case '}': 

            case ']': 
                           if (needIndent) { 
                               // suppress indent in empty object/array
                               needIndent = false;
                           }
                           else
                {
                               depth--;
                               newline(_addr_dst, prefix, indent, depth);
                           }
                           dst.WriteByte(c);
                break;
            default: 
                dst.WriteByte(c);
                break;
        }
    }    if (scan.eof() == scanError) {
        dst.Truncate(origLen);
        return error.As(scan.err)!;
    }
    return error.As(null!)!;
});

} // end json_package
