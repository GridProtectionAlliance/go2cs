// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cipher -- go2cs converted at 2022 March 06 22:18:10 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Program Files\Go\src\crypto\cipher\io.go
using io = go.io_package;

namespace go.crypto;

public static partial class cipher_package {

    // The Stream* objects are so simple that all their members are public. Users
    // can create them themselves.

    // StreamReader wraps a Stream into an io.Reader. It calls XORKeyStream
    // to process each slice of data which passes through.
public partial struct StreamReader {
    public Stream S;
    public io.Reader R;
}

public static (nint, error) Read(this StreamReader r, slice<byte> dst) {
    nint n = default;
    error err = default!;

    n, err = r.R.Read(dst);
    r.S.XORKeyStream(dst[..(int)n], dst[..(int)n]);
    return ;
}

// StreamWriter wraps a Stream into an io.Writer. It calls XORKeyStream
// to process each slice of data which passes through. If any Write call
// returns short then the StreamWriter is out of sync and must be discarded.
// A StreamWriter has no internal buffering; Close does not need
// to be called to flush write data.
public partial struct StreamWriter {
    public Stream S;
    public io.Writer W;
    public error Err; // unused
}

public static (nint, error) Write(this StreamWriter w, slice<byte> src) {
    nint n = default;
    error err = default!;

    var c = make_slice<byte>(len(src));
    w.S.XORKeyStream(c, src);
    n, err = w.W.Write(c);
    if (n != len(src) && err == null) { // should never happen
        err = io.ErrShortWrite;

    }
    return ;

}

// Close closes the underlying Writer and returns its Close return value, if the Writer
// is also an io.Closer. Otherwise it returns nil.
public static error Close(this StreamWriter w) {
    {
        io.Closer (c, ok) = w.W._<io.Closer>();

        if (ok) {
            return error.As(c.Close())!;
        }
    }

    return error.As(null!)!;

}

} // end cipher_package
