// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package span -- go2cs converted at 2022 March 06 23:31:24 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\token.go
using fmt = go.fmt_package;
using token = go.go.token_package;

namespace go.golang.org.x.tools.@internal;

public static partial class span_package {

    // Range represents a source code range in token.Pos form.
    // It also carries the FileSet that produced the positions, so that it is
    // self contained.
public partial struct Range {
    public ptr<token.FileSet> FileSet;
    public token.Pos Start;
    public token.Pos End;
    public Converter Converter;
}

// TokenConverter is a Converter backed by a token file set and file.
// It uses the file set methods to work out the conversions, which
// makes it fast and does not require the file contents.
public partial struct TokenConverter {
    public ptr<token.FileSet> fset;
    public ptr<token.File> file;
}

// NewRange creates a new Range from a FileSet and two positions.
// To represent a point pass a 0 as the end pos.
public static Range NewRange(ptr<token.FileSet> _addr_fset, token.Pos start, token.Pos end) {
    ref token.FileSet fset = ref _addr_fset.val;

    return new Range(FileSet:fset,Start:start,End:end,);
}

// NewTokenConverter returns an implementation of Converter backed by a
// token.File.
public static ptr<TokenConverter> NewTokenConverter(ptr<token.FileSet> _addr_fset, ptr<token.File> _addr_f) {
    ref token.FileSet fset = ref _addr_fset.val;
    ref token.File f = ref _addr_f.val;

    return addr(new TokenConverter(fset:fset,file:f));
}

// NewContentConverter returns an implementation of Converter for the
// given file content.
public static ptr<TokenConverter> NewContentConverter(@string filename, slice<byte> content) {
    var fset = token.NewFileSet();
    var f = fset.AddFile(filename, -1, len(content));
    f.SetLinesForContent(content);
    return addr(new TokenConverter(fset:fset,file:f));
}

// IsPoint returns true if the range represents a single point.
public static bool IsPoint(this Range r) {
    return r.Start == r.End;
}

// Span converts a Range to a Span that represents the Range.
// It will fill in all the members of the Span, calculating the line and column
// information.
public static (Span, error) Span(this Range r) {
    Span _p0 = default;
    error _p0 = default!;

    if (!r.Start.IsValid()) {
        return (new Span(), error.As(fmt.Errorf("start pos is not valid"))!);
    }
    var f = r.FileSet.File(r.Start);
    if (f == null) {
        return (new Span(), error.As(fmt.Errorf("file not found in FileSet"))!);
    }
    Span s = default;
    error err = default!;
    @string startFilename = default;
    startFilename, s.v.Start.Line, s.v.Start.Column, err = position(_addr_f, r.Start);
    if (err != null) {
        return (new Span(), error.As(err)!);
    }
    s.v.URI = URIFromPath(startFilename);
    if (r.End.IsValid()) {
        @string endFilename = default;
        endFilename, s.v.End.Line, s.v.End.Column, err = position(_addr_f, r.End);
        if (err != null) {
            return (new Span(), error.As(err)!);
        }
        if (endFilename != startFilename) {
            return (new Span(), error.As(fmt.Errorf("span begins in file %q but ends in %q", startFilename, endFilename))!);
        }
    }
    s.v.Start.clean();
    s.v.End.clean();
    s.v.clean();
    if (r.Converter != null) {
        return s.WithOffset(r.Converter);
    }
    if (startFilename != f.Name()) {
        return (new Span(), error.As(fmt.Errorf("must supply Converter for file %q containing lines from %q", f.Name(), startFilename))!);
    }
    return s.WithOffset(NewTokenConverter(_addr_r.FileSet, _addr_f));

}

private static (@string, nint, nint, error) position(ptr<token.File> _addr_f, token.Pos pos) {
    @string _p0 = default;
    nint _p0 = default;
    nint _p0 = default;
    error _p0 = default!;
    ref token.File f = ref _addr_f.val;

    var (off, err) = offset(_addr_f, pos);
    if (err != null) {
        return ("", 0, 0, error.As(err)!);
    }
    return positionFromOffset(_addr_f, off);

}

private static (@string, nint, nint, error) positionFromOffset(ptr<token.File> _addr_f, nint offset) {
    @string _p0 = default;
    nint _p0 = default;
    nint _p0 = default;
    error _p0 = default!;
    ref token.File f = ref _addr_f.val;

    if (offset > f.Size()) {
        return ("", 0, 0, error.As(fmt.Errorf("offset %v is past the end of the file %v", offset, f.Size()))!);
    }
    var pos = f.Pos(offset);
    var p = f.Position(pos);
    if (offset == f.Size()) {
        return (p.Filename, p.Line + 1, 1, error.As(null!)!);
    }
    return (p.Filename, p.Line, p.Column, error.As(null!)!);

}

// offset is a copy of the Offset function in go/token, but with the adjustment
// that it does not panic on invalid positions.
private static (nint, error) offset(ptr<token.File> _addr_f, token.Pos pos) {
    nint _p0 = default;
    error _p0 = default!;
    ref token.File f = ref _addr_f.val;

    if (int(pos) < f.Base() || int(pos) > f.Base() + f.Size()) {
        return (0, error.As(fmt.Errorf("invalid pos"))!);
    }
    return (int(pos) - f.Base(), error.As(null!)!);

}

// Range converts a Span to a Range that represents the Span for the supplied
// File.
public static (Range, error) Range(this Span s, ptr<TokenConverter> _addr_converter) {
    Range _p0 = default;
    error _p0 = default!;
    ref TokenConverter converter = ref _addr_converter.val;

    var (s, err) = s.WithOffset(converter);
    if (err != null) {
        return (new Range(), error.As(err)!);
    }
    if (s.Start().Offset() > converter.file.Size()) {
        return (new Range(), error.As(fmt.Errorf("start offset %v is past the end of the file %v", s.Start(), converter.file.Size()))!);
    }
    if (s.End().Offset() > converter.file.Size()) {
        return (new Range(), error.As(fmt.Errorf("end offset %v is past the end of the file %v", s.End(), converter.file.Size()))!);
    }
    return (new Range(FileSet:converter.fset,Start:converter.file.Pos(s.Start().Offset()),End:converter.file.Pos(s.End().Offset()),Converter:converter,), error.As(null!)!);

}

private static (nint, nint, error) ToPosition(this ptr<TokenConverter> _addr_l, nint offset) {
    nint _p0 = default;
    nint _p0 = default;
    error _p0 = default!;
    ref TokenConverter l = ref _addr_l.val;

    var (_, line, col, err) = positionFromOffset(_addr_l.file, offset);
    return (line, col, error.As(err)!);
}

private static (nint, error) ToOffset(this ptr<TokenConverter> _addr_l, nint line, nint col) {
    nint _p0 = default;
    error _p0 = default!;
    ref TokenConverter l = ref _addr_l.val;

    if (line < 0) {
        return (-1, error.As(fmt.Errorf("line is not valid"))!);
    }
    var lineMax = l.file.LineCount() + 1;
    if (line > lineMax) {
        return (-1, error.As(fmt.Errorf("line is beyond end of file %v", lineMax))!);
    }
    else if (line == lineMax) {
        if (col > 1) {
            return (-1, error.As(fmt.Errorf("column is beyond end of file"))!);
        }
        return (l.file.Size(), error.As(null!)!);

    }
    var pos = lineStart(l.file, line);
    if (!pos.IsValid()) {
        return (-1, error.As(fmt.Errorf("line is not in file"))!);
    }
    pos += token.Pos(col - 1);
    return offset(_addr_l.file, pos);

}

} // end span_package
