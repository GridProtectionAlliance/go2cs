// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using ꓸꓸꓸany = Span<any>;

partial class textproto_package {

// A Writer implements convenience methods for writing
// requests or responses to a text protocol network connection.
[GoType] partial struct Writer {
    public ж<bufio_package.Writer> W;
    internal ж<dotWriter> dot;
}

// NewWriter returns a new [Writer] writing to w.
public static ж<Writer> NewWriter(ж<bufio.Writer> Ꮡw) {
    ref var w = ref Ꮡw.val;

    return Ꮡ(new Writer(W: w));
}

internal static slice<byte> crnl = new byte[]{(rune)'\r', (rune)'\n'}.slice();

internal static slice<byte> dotcrnl = new byte[]{(rune)'.', (rune)'\r', (rune)'\n'}.slice();

// PrintfLine writes the formatted output followed by \r\n.
[GoRecv] public static error PrintfLine(this ref Writer w, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    w.closeDot();
    fmt.Fprintf(~w.W, format, args.ꓸꓸꓸ);
    w.W.Write(crnl);
    return w.W.Flush();
}

// DotWriter returns a writer that can be used to write a dot-encoding to w.
// It takes care of inserting leading dots when necessary,
// translating line-ending \n into \r\n, and adding the final .\r\n line
// when the DotWriter is closed. The caller should close the
// DotWriter before the next call to a method on w.
//
// See the documentation for the [Reader.DotReader] method for details about dot-encoding.
[GoRecv] public static io.WriteCloser DotWriter(this ref Writer w) {
    w.closeDot();
    w.dot = Ꮡ(new dotWriter(w: w));
    return ~w.dot;
}

[GoRecv] internal static void closeDot(this ref Writer w) {
    if (w.dot != nil) {
        w.dot.Close();
    }
}

// sets w.dot = nil
[GoType] partial struct dotWriter {
    internal ж<Writer> w;
    internal nint state;
}

internal static readonly UntypedInt wstateBegin = iota; // initial state; must be zero
internal static readonly UntypedInt wstateBeginLine = 1; // beginning of line
internal static readonly UntypedInt wstateCR = 2; // wrote \r (possibly at end of line)
internal static readonly UntypedInt wstateData = 3; // writing data in middle of line

[GoRecv] internal static (nint n, error err) Write(this ref dotWriter d, slice<byte> b) {
    nint n = default!;
    error err = default!;

    var bw = d.w.W;
    while (n < len(b)) {
        var c = b[n];
        var exprᴛ1 = d.state;
        var matchᴛ1 = false;
        if (exprᴛ1 == wstateBegin || exprᴛ1 == wstateBeginLine) { matchᴛ1 = true;
            d.state = wstateData;
            if (c == (rune)'.') {
                // escape leading dot
                bw.WriteByte((rune)'.');
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == wstateData)) {
            if (c == (rune)'\r') {
                d.state = wstateCR;
            }
            if (c == (rune)'\n') {
                bw.WriteByte((rune)'\r');
                d.state = wstateBeginLine;
            }
        }
        else if (exprᴛ1 == wstateCR) { matchᴛ1 = true;
            d.state = wstateData;
            if (c == (rune)'\n') {
                d.state = wstateBeginLine;
            }
        }

        {
            err = bw.WriteByte(c); if (err != default!) {
                break;
            }
        }
        n++;
    }
    return (n, err);
}

[GoRecv] internal static error Close(this ref dotWriter d) {
    if (d.w.dot == d) {
        d.w.dot = default!;
    }
    var bw = d.w.W;
    var exprᴛ1 = d.state;
    var matchᴛ1 = false;
    { /* default: */
        bw.WriteByte((rune)'\r');
    }
    else if (exprᴛ1 == wstateCR) {
        bw.WriteByte((rune)'\n');
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == wstateBeginLine)) { matchᴛ1 = true;
        bw.Write(dotcrnl);
    }

    return bw.Flush();
}

} // end textproto_package
