// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package textproto -- go2cs converted at 2022 March 13 05:36:19 UTC
// import "net/textproto" ==> using textproto = go.net.textproto_package
// Original source: C:\Program Files\Go\src\net\textproto\writer.go
namespace go.net;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;


// A Writer implements convenience methods for writing
// requests or responses to a text protocol network connection.

public static partial class textproto_package {

public partial struct Writer {
    public ptr<bufio.Writer> W;
    public ptr<dotWriter> dot;
}

// NewWriter returns a new Writer writing to w.
public static ptr<Writer> NewWriter(ptr<bufio.Writer> _addr_w) {
    ref bufio.Writer w = ref _addr_w.val;

    return addr(new Writer(W:w));
}

private static byte crnl = new slice<byte>(new byte[] { '\r', '\n' });
private static byte dotcrnl = new slice<byte>(new byte[] { '.', '\r', '\n' });

// PrintfLine writes the formatted output followed by \r\n.
private static error PrintfLine(this ptr<Writer> _addr_w, @string format, params object[] args) {
    args = args.Clone();
    ref Writer w = ref _addr_w.val;

    w.closeDot();
    fmt.Fprintf(w.W, format, args);
    w.W.Write(crnl);
    return error.As(w.W.Flush())!;
}

// DotWriter returns a writer that can be used to write a dot-encoding to w.
// It takes care of inserting leading dots when necessary,
// translating line-ending \n into \r\n, and adding the final .\r\n line
// when the DotWriter is closed. The caller should close the
// DotWriter before the next call to a method on w.
//
// See the documentation for Reader's DotReader method for details about dot-encoding.
private static io.WriteCloser DotWriter(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    w.closeDot();
    w.dot = addr(new dotWriter(w:w));
    return w.dot;
}

private static void closeDot(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    if (w.dot != null) {
        w.dot.Close(); // sets w.dot = nil
    }
}

private partial struct dotWriter {
    public ptr<Writer> w;
    public nint state;
}

private static readonly var wstateBegin = iota; // initial state; must be zero
private static readonly var wstateBeginLine = 0; // beginning of line
private static readonly var wstateCR = 1; // wrote \r (possibly at end of line)
private static readonly var wstateData = 2; // writing data in middle of line

private static (nint, error) Write(this ptr<dotWriter> _addr_d, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref dotWriter d = ref _addr_d.val;

    var bw = d.w.W;
    while (n < len(b)) {
        var c = b[n];

        if (d.state == wstateBegin || d.state == wstateBeginLine)
        {
            d.state = wstateData;
            if (c == '.') { 
                // escape leading dot
                bw.WriteByte('.');
            }
            fallthrough = true;

        }
        if (fallthrough || d.state == wstateData)
        {
            if (c == '\r') {
                d.state = wstateCR;
            }
            if (c == '\n') {
                bw.WriteByte('\r');
                d.state = wstateBeginLine;
            }
            goto __switch_break0;
        }
        if (d.state == wstateCR)
        {
            d.state = wstateData;
            if (c == '\n') {
                d.state = wstateBeginLine;
            }
            goto __switch_break0;
        }

        __switch_break0:;
        err = bw.WriteByte(c);

        if (err != null) {
            break;
        }
        n++;
    }
    return ;
}

private static error Close(this ptr<dotWriter> _addr_d) {
    ref dotWriter d = ref _addr_d.val;

    if (d.w.dot == d) {
        d.w.dot = null;
    }
    var bw = d.w.W;

    if (d.state == wstateCR)
    {
        bw.WriteByte('\n');
        fallthrough = true;
    }
    if (fallthrough || d.state == wstateBeginLine)
    {
        bw.Write(dotcrnl);
        goto __switch_break1;
    }
    // default: 
        bw.WriteByte('\r');

    __switch_break1:;
    return error.As(bw.Flush())!;
}

} // end textproto_package
