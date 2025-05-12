// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using flate = compress.flate_package;
using errors = errors_package;
using fmt = fmt_package;
using crc32 = hash.crc32_package;
using io = io_package;
using time = time_package;
using hash;

partial class gzip_package {

// These constants are copied from the flate package, so that code that imports
// "compress/gzip" does not also have to import "compress/flate".
public static readonly UntypedInt NoCompression = /* flate.NoCompression */ 0;

public static readonly UntypedInt BestSpeed = /* flate.BestSpeed */ 1;

public static readonly UntypedInt BestCompression = /* flate.BestCompression */ 9;

public static readonly GoUntyped DefaultCompression = /* flate.DefaultCompression */
    GoUntyped.Parse("-1");

public static readonly GoUntyped HuffmanOnly = /* flate.HuffmanOnly */
    GoUntyped.Parse("-2");

// A Writer is an io.WriteCloser.
// Writes to a Writer are compressed and written to w.
[GoType] partial struct Writer {
    public partial ref Header Header { get; }      // written at first call to Write, Flush, or Close
    internal io_package.Writer w;
    internal nint level;
    internal bool wroteHeader;
    internal bool closed;
    internal array<byte> buf = new(10);
    internal ж<compress.flate_package.Writer> compressor;
    internal uint32 digest; // CRC-32, IEEE polynomial (section 8)
    internal uint32 size; // Uncompressed size (section 2.3.1)
    internal error err;
}

// NewWriter returns a new [Writer].
// Writes to the returned writer are compressed and written to w.
//
// It is the caller's responsibility to call Close on the [Writer] when done.
// Writes may be buffered and not flushed until Close.
//
// Callers that wish to set the fields in Writer.Header must do so before
// the first call to Write, Flush, or Close.
public static ж<Writer> NewWriter(io.Writer w) {
    (z, _) = NewWriterLevel(w, DefaultCompression);
    return z;
}

// NewWriterLevel is like [NewWriter] but specifies the compression level instead
// of assuming [DefaultCompression].
//
// The compression level can be [DefaultCompression], [NoCompression], [HuffmanOnly]
// or any integer value between [BestSpeed] and [BestCompression] inclusive.
// The error returned will be nil if the level is valid.
public static (ж<Writer>, error) NewWriterLevel(io.Writer w, nint level) {
    if (level < HuffmanOnly || level > BestCompression) {
        return (default!, fmt.Errorf("gzip: invalid compression level: %d"u8, level));
    }
    var z = @new<Writer>();
    z.init(w, level);
    return (z, default!);
}

[GoRecv] internal static void init(this ref Writer z, io.Writer w, nint level) {
    var compressor = z.compressor;
    if (compressor != nil) {
        compressor.Reset(w);
    }
    z = new Writer(
        Header: new Header(
            OS: 255
        ), // unknown

        w: w,
        level: level,
        compressor: compressor
    );
}

// Reset discards the [Writer] z's state and makes it equivalent to the
// result of its original state from [NewWriter] or [NewWriterLevel], but
// writing to w instead. This permits reusing a [Writer] rather than
// allocating a new one.
[GoRecv] public static void Reset(this ref Writer z, io.Writer w) {
    z.init(w, z.level);
}

// writeBytes writes a length-prefixed byte slice to z.w.
[GoRecv] internal static error writeBytes(this ref Writer z, slice<byte> b) {
    if (len(b) > 65535) {
        return errors.New("gzip.Write: Extra data is too large"u8);
    }
    le.PutUint16(z.buf[..2], ((uint16)len(b)));
    var (_, err) = z.w.Write(z.buf[..2]);
    if (err != default!) {
        return err;
    }
    (_, err) = z.w.Write(b);
    return err;
}

// writeString writes a UTF-8 string s in GZIP's format to z.w.
// GZIP (RFC 1952) specifies that strings are NUL-terminated ISO 8859-1 (Latin-1).
[GoRecv] internal static error /*err*/ writeString(this ref Writer z, @string s) {
    error err = default!;

    // GZIP stores Latin-1 strings; error if non-Latin-1; convert if non-ASCII.
    var needconv = false;
    foreach (var (_, v) in s) {
        if (v == 0 || v > 255) {
            return errors.New("gzip.Write: non-Latin-1 header string"u8);
        }
        if (v > 127) {
            needconv = true;
        }
    }
    if (needconv){
        var b = new slice<byte>(0, len(s));
        foreach (var (_, v) in s) {
            b = append(b, ((byte)v));
        }
        (_, err) = z.w.Write(b);
    } else {
        (_, err) = io.WriteString(z.w, s);
    }
    if (err != default!) {
        return err;
    }
    // GZIP strings are NUL-terminated.
    z.buf[0] = 0;
    (_, err) = z.w.Write(z.buf[..1]);
    return err;
}

// Write writes a compressed form of p to the underlying [io.Writer]. The
// compressed bytes are not necessarily flushed until the [Writer] is closed.
[GoRecv] public static (nint, error) Write(this ref Writer z, slice<byte> p) {
    if (z.err != default!) {
        return (0, z.err);
    }
    nint n = default!;
    // Write the GZIP header lazily.
    if (!z.wroteHeader) {
        z.wroteHeader = true;
        z.buf = new array<byte>(10){[0] = gzipID1, [1] = gzipID2, [2] = gzipDeflate};
        if (z.Extra != default!) {
            z.buf[3] |= (byte)(4);
        }
        if (z.Name != ""u8) {
            z.buf[3] |= (byte)(8);
        }
        if (z.Comment != ""u8) {
            z.buf[3] |= (byte)(16);
        }
        if (z.ModTime.After(time.Unix(0, 0))) {
            // Section 2.3.1, the zero value for MTIME means that the
            // modified time is not set.
            le.PutUint32(z.buf[4..8], ((uint32)z.ModTime.Unix()));
        }
        if (z.level == BestCompression){
            z.buf[8] = 2;
        } else 
        if (z.level == BestSpeed) {
            z.buf[8] = 4;
        }
        z.buf[9] = z.OS;
        (_, z.err) = z.w.Write(z.buf[..10]);
        if (z.err != default!) {
            return (0, z.err);
        }
        if (z.Extra != default!) {
            z.err = z.writeBytes(z.Extra);
            if (z.err != default!) {
                return (0, z.err);
            }
        }
        if (z.Name != ""u8) {
            z.err = z.writeString(z.Name);
            if (z.err != default!) {
                return (0, z.err);
            }
        }
        if (z.Comment != ""u8) {
            z.err = z.writeString(z.Comment);
            if (z.err != default!) {
                return (0, z.err);
            }
        }
        if (z.compressor == nil) {
            (z.compressor, _) = flate.NewWriter(z.w, z.level);
        }
    }
    z.size += ((uint32)len(p));
    z.digest = crc32.Update(z.digest, crc32.IEEETable, p);
    (n, z.err) = z.compressor.Write(p);
    return (n, z.err);
}

// Flush flushes any pending compressed data to the underlying writer.
//
// It is useful mainly in compressed network protocols, to ensure that
// a remote reader has enough data to reconstruct a packet. Flush does
// not return until the data has been written. If the underlying
// writer returns an error, Flush returns that error.
//
// In the terminology of the zlib library, Flush is equivalent to Z_SYNC_FLUSH.
[GoRecv] public static error Flush(this ref Writer z) {
    if (z.err != default!) {
        return z.err;
    }
    if (z.closed) {
        return default!;
    }
    if (!z.wroteHeader) {
        z.Write(default!);
        if (z.err != default!) {
            return z.err;
        }
    }
    z.err = z.compressor.Flush();
    return z.err;
}

// Close closes the [Writer] by flushing any unwritten data to the underlying
// [io.Writer] and writing the GZIP footer.
// It does not close the underlying [io.Writer].
[GoRecv] public static error Close(this ref Writer z) {
    if (z.err != default!) {
        return z.err;
    }
    if (z.closed) {
        return default!;
    }
    z.closed = true;
    if (!z.wroteHeader) {
        z.Write(default!);
        if (z.err != default!) {
            return z.err;
        }
    }
    z.err = z.compressor.Close();
    if (z.err != default!) {
        return z.err;
    }
    le.PutUint32(z.buf[..4], z.digest);
    le.PutUint32(z.buf[4..8], z.size);
    (_, z.err) = z.w.Write(z.buf[..8]);
    return z.err;
}

} // end gzip_package
