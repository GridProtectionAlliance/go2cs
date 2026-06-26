// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using hash = hash_package;
using io = io_package;
using rand = math.rand_package;
using testing = testing_package;
using time = time_package;
using math;

partial class cryptotest_package {

public delegate hash.Hash MakeHash();

// TestHash performs a set of tests on hash.Hash implementations, checking the
// documented requirements of Write, Sum, Reset, Size, and BlockSize.
public static void TestHash(ж<testing.T> Ꮡt, MakeHash mh) {
    ref var t = ref Ꮡt.val;

    // Test that Sum returns an appended digest matching output of Size
    t.Run("SumAppend"u8, (ж<testing.T> t) => {
        var h = mh();
        var rng = newRandReader(ᏑtΔ1);
        var emptyBuff = slice<byte>("");
        var shortBuff = slice<byte>("a");
        var longBuff = new slice<byte>(h.BlockSize() + 1);
        rng.Read(longBuff);
        // Set of example strings to append digest to
        var prefixes = new slice<byte>[]{default!, emptyBuff, shortBuff, longBuff}.slice();
        // Go to each string and check digest gets appended to and is correct size.
        foreach (var (_, prefix) in prefixes) {
            h.Reset();
            var sum = getSum(ᏑtΔ1, h, prefix);
            // Append new digest to prefix
            // Check that Sum didn't alter the prefix
            if (!bytes.Equal(sum[0..(int)(len(prefix))], prefix)) {
                tΔ1.Errorf("Sum alters passed buffer instead of appending; got %x, want %x"u8, sum[0..(int)(len(prefix))], prefix);
            }
            // Check that the appended sum wasn't affected by the prefix
            {
                var expectedSum = getSum(ᏑtΔ1, h, default!); if (!bytes.Equal(sum[(int)(len(prefix))..], expectedSum)) {
                    tΔ1.Errorf("Sum behavior affected by data in the input buffer; got %x, want %x"u8, sum[(int)(len(prefix))..], expectedSum);
                }
            }
            // Check size of append
            {
                nint got = len(sum) - len(prefix);
                nint want = h.Size(); if (got != want) {
                    tΔ1.Errorf("Sum appends number of bytes != Size; got %v , want %v"u8, got, want);
                }
            }
        }
    });
    // Test that Hash.Write never returns error.
    t.Run("WriteWithoutError"u8, (ж<testing.T> t) => {
        var h = mh();
        var rng = newRandReader(ᏑtΔ2);
        var emptySlice = slice<byte>("");
        var shortSlice = slice<byte>("a");
        var longSlice = new slice<byte>(h.BlockSize() + 1);
        rng.Read(longSlice);
        // Set of example strings to append digest to
        var slices = new slice<byte>[]{emptySlice, shortSlice, longSlice}.slice();
        foreach (var (_, Δslice) in slices) {
            writeToHash(ᏑtΔ2, h, Δslice);
        }
    });
    // Writes and checks Write doesn't error
    t.Run("ResetState"u8, (ж<testing.T> t) => {
        var h = mh();
        var rng = newRandReader(ᏑtΔ3);
        var emptySum = getSum(ᏑtΔ3, h, default!);
        // Write to hash and then Reset it and see if Sum is same as emptySum
        var writeEx = new slice<byte>(h.BlockSize());
        rng.Read(writeEx);
        writeToHash(ᏑtΔ3, h, writeEx);
        h.Reset();
        var resetSum = getSum(ᏑtΔ3, h, default!);
        if (!bytes.Equal(emptySum, resetSum)) {
            tΔ3.Errorf("Reset hash yields different Sum than new hash; got %x, want %x"u8, emptySum, resetSum);
        }
    });
    // Check that Write isn't reading from beyond input slice's bounds
    t.Run("OutOfBoundsRead"u8, (ж<testing.T> t) => {
        var h = mh();
        nint blockSize = h.BlockSize();
        var rng = newRandReader(ᏑtΔ4);
        var msg = new slice<byte>(blockSize);
        rng.Read(msg);
        writeToHash(ᏑtΔ4, h, msg);
        var expectedDigest = getSum(ᏑtΔ4, h, default!);
        // Record control digest
        h.Reset();
        // Make a buffer with msg in the middle and data on either end
        var buff = new slice<byte>(blockSize * 3);
        nint endOfPrefix = blockSize;
        nint startOfSuffix = blockSize * 2;
        copy(buff[(int)(endOfPrefix)..(int)(startOfSuffix)], msg);
        rng.Read(buff[..(int)(endOfPrefix)]);
        rng.Read(buff[(int)(startOfSuffix)..]);
        writeToHash(ᏑtΔ4, h, buff[(int)(endOfPrefix)..(int)(startOfSuffix)]);
        var testDigest = getSum(ᏑtΔ4, h, default!);
        if (!bytes.Equal(testDigest, expectedDigest)) {
            tΔ4.Errorf("Write affected by data outside of input slice bounds; got %x, want %x"u8, testDigest, expectedDigest);
        }
    });
    // Test that multiple calls to Write is stateful
    t.Run("StatefulWrite"u8, (ж<testing.T> t) => {
        var h = mh();
        var rng = newRandReader(ᏑtΔ5);
        var prefix = new slice<byte>(h.BlockSize());
        var suffix = new slice<byte>(h.BlockSize());
        rng.Read(prefix);
        rng.Read(suffix);
        // Write prefix then suffix sequentially and record resulting hash
        writeToHash(ᏑtΔ5, h, prefix);
        writeToHash(ᏑtΔ5, h, suffix);
        var serialSum = getSum(ᏑtΔ5, h, default!);
        h.Reset();
        // Write prefix and suffix at the same time and record resulting hash
        writeToHash(ᏑtΔ5, h, append(prefix, suffix.ꓸꓸꓸ));
        var compositeSum = getSum(ᏑtΔ5, h, default!);
        // Check that sequential writing results in the same as writing all at once
        if (!bytes.Equal(compositeSum, serialSum)) {
            tΔ5.Errorf("two successive Write calls resulted in a different Sum than a single one; got %x, want %x"u8, compositeSum, serialSum);
        }
    });
}

// Helper function for writing. Verifies that Write does not error.
internal static void writeToHash(ж<testing.T> Ꮡt, hash.Hash h, slice<byte> p) {
    ref var t = ref Ꮡt.val;

    t.Helper();
    var before = new slice<byte>(len(p));
    copy(before, p);
    var (n, err) = h.Write(p);
    if (err != default! || n != len(p)) {
        t.Errorf("Write returned error; got (%v, %v), want (nil, %v)"u8, err, n, len(p));
    }
    if (!bytes.Equal(p, before)) {
        t.Errorf("Write modified input slice; got %x, want %x"u8, p, before);
    }
}

// Helper function for getting Sum. Checks that Sum doesn't change hash state.
internal static slice<byte> getSum(ж<testing.T> Ꮡt, hash.Hash h, slice<byte> buff) {
    ref var t = ref Ꮡt.val;

    t.Helper();
    var testBuff = new slice<byte>(len(buff));
    copy(testBuff, buff);
    var sum = h.Sum(buff);
    var testSum = h.Sum(testBuff);
    // Check that Sum doesn't change underlying hash state
    if (!bytes.Equal(sum, testSum)) {
        t.Errorf("successive calls to Sum yield different results; got %x, want %x"u8, sum, testSum);
    }
    return sum;
}

internal static io.Reader newRandReader(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.val;

    var seed = time.Now().UnixNano();
    t.Logf("Deterministic RNG seed: 0x%x"u8, seed);
    return ~rand.New(rand.NewSource(seed));
}

} // end cryptotest_package
