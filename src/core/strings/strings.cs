// strings.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using io = go.io_package;

namespace go;

public static partial class strings_package
{
    // A Reader implements the io.Reader, io.ReaderAt, io.Seeker, io.WriterTo,
    // io.ByteScanner, and io.RuneScanner interfaces by reading
    // from a string.
    // The zero value for Reader operates like a Reader of an empty string.
    public partial struct Reader {
        public @string s;
        public nint i;
        public int prevRune;
    }

    // NewReader returns a new Reader reading from s.
    // It is similar to bytes.NewBufferString but more efficient and read-only.
    public static ж<Reader> NewReader(@string s) {
        return Ꮡ(new Reader(s, 0, -1));
    }

    public static (nint n, error err) Read(this ж<Reader> r, in slice<byte> b) =>
        Read(ref r.Value, b);

    // Size returns the original length of the underlying string.
    // Size is the number of bytes available for reading via ReadAt.
    // The returned value is always the same and is not affected by calls
    // to any other method.
    public static (nint n, error err) Read(this ref Reader r, in slice<byte> b) {
        nint n;
        error err = default!;

        if (r.i >= len(r.s))
            return (0, io.EOF);

        r.prevRune = -1;
        n = copy(b, r.s[(int)r.i..]);
        r.i += n;

        return (n, err);
    }

    // Join the ELEMENTS — passing the slice itself binds string.Join's `params object?[]`
    // overload, which Go-formats the whole slice as one item ("[xy xy]").
    public static @string Join(in slice<@string> source, @string separator)
    {
        string[] parts = new string[source.Length];

        for (nint i = 0; i < source.Length; i++)
            parts[i] = source[i];

        return string.Join(separator, parts);
    }

    public static bool HasPrefix(@string s, @string prefix) => 
        s.ToString().StartsWith(prefix, StringComparison.Ordinal);
}
