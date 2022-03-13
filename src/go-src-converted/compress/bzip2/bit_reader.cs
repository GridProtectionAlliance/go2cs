// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bzip2 -- go2cs converted at 2022 March 13 06:43:18 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Program Files\Go\src\compress\bzip2\bit_reader.go
namespace go.compress;

using bufio = bufio_package;
using io = io_package;


// bitReader wraps an io.Reader and provides the ability to read values,
// bit-by-bit, from it. Its Read* methods don't return the usual error
// because the error handling was verbose. Instead, any error is kept and can
// be checked afterwards.

public static partial class bzip2_package {

private partial struct bitReader {
    public io.ByteReader r;
    public ulong n;
    public nuint bits;
    public error err;
}

// newBitReader returns a new bitReader reading from r. If r is not
// already an io.ByteReader, it will be converted via a bufio.Reader.
private static bitReader newBitReader(io.Reader r) {
    io.ByteReader (byter, ok) = r._<io.ByteReader>();
    if (!ok) {
        byter = bufio.NewReader(r);
    }
    return new bitReader(r:byter);
}

// ReadBits64 reads the given number of bits and returns them in the
// least-significant part of a uint64. In the event of an error, it returns 0
// and the error can be obtained by calling Err().
private static ulong ReadBits64(this ptr<bitReader> _addr_br, nuint bits) {
    ulong n = default;
    ref bitReader br = ref _addr_br.val;

    while (bits > br.bits) {
        var (b, err) = br.r.ReadByte();
        if (err == io.EOF) {
            err = io.ErrUnexpectedEOF;
        }
        if (err != null) {
            br.err = err;
            return 0;
        }
        br.n<<=8;
        br.n |= uint64(b);
        br.bits += 8;
    } 

    // br.n looks like this (assuming that br.bits = 14 and bits = 6):
    // Bit: 111111
    //      5432109876543210
    //
    //         (6 bits, the desired output)
    //        |-----|
    //        V     V
    //      0101101101001110
    //        ^            ^
    //        |------------|
    //           br.bits (num valid bits)
    //
    // This the next line right shifts the desired bits into the
    // least-significant places and masks off anything above.
    n = (br.n >> (int)((br.bits - bits))) & ((1 << (int)(bits)) - 1);
    br.bits -= bits;
    return ;
}

private static nint ReadBits(this ptr<bitReader> _addr_br, nuint bits) {
    nint n = default;
    ref bitReader br = ref _addr_br.val;

    var n64 = br.ReadBits64(bits);
    return int(n64);
}

private static bool ReadBit(this ptr<bitReader> _addr_br) {
    ref bitReader br = ref _addr_br.val;

    var n = br.ReadBits(1);
    return n != 0;
}

private static error Err(this ptr<bitReader> _addr_br) {
    ref bitReader br = ref _addr_br.val;

    return error.As(br.err)!;
}

} // end bzip2_package
