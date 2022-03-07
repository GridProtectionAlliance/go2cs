// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package span -- go2cs converted at 2022 March 06 23:31:26 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\utf16.go
using fmt = go.fmt_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;

namespace go.golang.org.x.tools.@internal;

public static partial class span_package {

    // ToUTF16Column calculates the utf16 column expressed by the point given the
    // supplied file contents.
    // This is used to convert from the native (always in bytes) column
    // representation and the utf16 counts used by some editors.
public static (nint, error) ToUTF16Column(Point p, slice<byte> content) {
    nint _p0 = default;
    error _p0 = default!;

    if (content == null) {
        return (-1, error.As(fmt.Errorf("ToUTF16Column: missing content"))!);
    }
    if (!p.HasPosition()) {
        return (-1, error.As(fmt.Errorf("ToUTF16Column: point is missing position"))!);
    }
    if (!p.HasOffset()) {
        return (-1, error.As(fmt.Errorf("ToUTF16Column: point is missing offset"))!);
    }
    var offset = p.Offset(); // 0-based
    var colZero = p.Column() - 1; // 0-based
    if (colZero == 0) { 
        // 0-based column 0, so it must be chr 1
        return (1, error.As(null!)!);

    }
    else if (colZero < 0) {
        return (-1, error.As(fmt.Errorf("ToUTF16Column: column is invalid (%v)", colZero))!);
    }
    var lineOffset = offset - colZero;
    if (lineOffset < 0 || offset > len(content)) {
        return (-1, error.As(fmt.Errorf("ToUTF16Column: offsets %v-%v outside file contents (%v)", lineOffset, offset, len(content)))!);
    }
    var start = content[(int)lineOffset..]; 

    // Now, truncate down to the supplied column.
    start = start[..(int)colZero]; 

    // and count the number of utf16 characters
    // in theory we could do this by hand more efficiently...
    return (len(utf16.Encode((slice<int>)string(start))) + 1, error.As(null!)!);

}

// FromUTF16Column advances the point by the utf16 character offset given the
// supplied line contents.
// This is used to convert from the utf16 counts used by some editors to the
// native (always in bytes) column representation.
public static (Point, error) FromUTF16Column(Point p, nint chr, slice<byte> content) {
    Point _p0 = default;
    error _p0 = default!;

    if (!p.HasOffset()) {
        return (new Point(), error.As(fmt.Errorf("FromUTF16Column: point is missing offset"))!);
    }
    if (chr <= 1) {
        return (p, error.As(null!)!);
    }
    if (p.Offset() >= len(content)) {
        return (p, error.As(fmt.Errorf("FromUTF16Column: offset (%v) greater than length of content (%v)", p.Offset(), len(content)))!);
    }
    var remains = content[(int)p.Offset()..]; 
    // scan forward the specified number of characters
    for (nint count = 1; count < chr; count++) {
        if (len(remains) <= 0) {
            return (new Point(), error.As(fmt.Errorf("FromUTF16Column: chr goes beyond the content"))!);
        }
        var (r, w) = utf8.DecodeRune(remains);
        if (r == '\n') { 
            // Per the LSP spec:
            //
            // > If the character value is greater than the line length it
            // > defaults back to the line length.
            break;

        }
        remains = remains[(int)w..];
        if (r >= 0x10000) { 
            // a two point rune
            count++; 
            // if we finished in a two point rune, do not advance past the first
            if (count >= chr) {
                break;
            }

        }
        p.v.Column += w;
        p.v.Offset += w;

    }
    return (p, error.As(null!)!);

}

} // end span_package
