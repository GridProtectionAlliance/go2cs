// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using io = io_package;
using testing = testing_package;

partial class saferio_package {

public static void TestReadData(ж<testing.T> Ꮡt) {
    UntypedInt count = 100;
    var input = bytes.Repeat(new byte[]{(rune)'a'}.slice(), count);
    var inputʗ1 = input;
    Ꮡt.Run("small"u8, (ж<testing.T> tΔ1) => {
        var (got, err) = ReadData(new bytes_ReaderжReader(bytes.NewReader(inputʗ1)), count);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        if (!bytes.Equal(got, inputʗ1)) {
            tΔ1.Errorf("got %v, want %v"u8, got, inputʗ1);
        }
    });
    var inputʗ3 = input;
    Ꮡt.Run("large"u8, (ж<testing.T> tΔ2) => {
        var (_, err) = ReadData(new bytes_ReaderжReader(bytes.NewReader(inputʗ3)), ((uint64)10 << (int)(30)));
        if (err == default!) {
            tΔ2.Error("large read succeeded unexpectedly");
        }
    });
    var inputʗ5 = input;
    Ꮡt.Run("maxint"u8, (ж<testing.T> tΔ3) => {
        var (_, err) = ReadData(new bytes_ReaderжReader(bytes.NewReader(inputʗ5)), ((uint64)1 << (int)(62)));
        if (err == default!) {
            tΔ3.Error("large read succeeded unexpectedly");
        }
    });
    Ꮡt.Run("small-EOF"u8, (ж<testing.T> tΔ4) => {
        var (_, err) = ReadData(new bytes_ReaderжReader(bytes.NewReader(default!)), chunk - 1);
        if (!AreEqual(err, io.EOF)) {
            tΔ4.Errorf("ReadData = %v, want io.EOF"u8, err);
        }
    });
    Ꮡt.Run("large-EOF"u8, (ж<testing.T> tΔ5) => {
        var (_, err) = ReadData(new bytes_ReaderжReader(bytes.NewReader(default!)), chunk + 1);
        if (!AreEqual(err, io.EOF)) {
            tΔ5.Errorf("ReadData = %v, want io.EOF"u8, err);
        }
    });
    Ꮡt.Run("large-UnexpectedEOF"u8, (ж<testing.T> tΔ6) => {
        var (_, err) = ReadData(new bytes_ReaderжReader(bytes.NewReader(new slice<byte>(chunk))), chunk + 1);
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            tΔ6.Errorf("ReadData = %v, want io.ErrUnexpectedEOF"u8, err);
        }
    });
}

public static void TestReadDataAt(ж<testing.T> Ꮡt) {
    UntypedInt count = 100;
    var input = bytes.Repeat(new byte[]{(rune)'a'}.slice(), count);
    var inputʗ1 = input;
    Ꮡt.Run("small"u8, (ж<testing.T> tΔ1) => {
        var (got, err) = ReadDataAt(new bytes_ReaderжReaderAt(bytes.NewReader(inputʗ1)), count, 0);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        if (!bytes.Equal(got, inputʗ1)) {
            tΔ1.Errorf("got %v, want %v"u8, got, inputʗ1);
        }
    });
    var inputʗ3 = input;
    Ꮡt.Run("large"u8, (ж<testing.T> tΔ2) => {
        var (_, err) = ReadDataAt(new bytes_ReaderжReaderAt(bytes.NewReader(inputʗ3)), ((uint64)10 << (int)(30)), 0);
        if (err == default!) {
            tΔ2.Error("large read succeeded unexpectedly");
        }
    });
    var inputʗ5 = input;
    Ꮡt.Run("maxint"u8, (ж<testing.T> tΔ3) => {
        var (_, err) = ReadDataAt(new bytes_ReaderжReaderAt(bytes.NewReader(inputʗ5)), ((uint64)1 << (int)(62)), 0);
        if (err == default!) {
            tΔ3.Error("large read succeeded unexpectedly");
        }
    });
    var inputʗ7 = input;
    Ꮡt.Run("SectionReader"u8, (ж<testing.T> tΔ4) => {
        // Reading 0 bytes from an io.SectionReader at the end
        // of the section will return EOF, but ReadDataAt
        // should succeed and return 0 bytes.
        var sr = io.NewSectionReader(new bytes_ReaderжReaderAt(bytes.NewReader(inputʗ7)), 0, 0);
        var (got, err) = ReadDataAt(new io.SectionReaderжReaderAt(sr), 0, 0);
        if (err != default!) {
            tΔ4.Fatal(err);
        }
        if (len(got) > 0) {
            tΔ4.Errorf("got %d bytes, expected 0"u8, len(got));
        }
    });
}

public static void TestSliceCap(ж<testing.T> Ꮡt) {
    Ꮡt.Run("small"u8, (ж<testing.T> tΔ1) => {
        nint c = SliceCap<nint>(10);
        if (c != 10) {
            tΔ1.Errorf("got capacity %d, want %d"u8, c, 10);
        }
    });
    Ꮡt.Run("large"u8, (ж<testing.T> tΔ2) => {
        nint c = SliceCap<byte>(((uint64)1 << (int)(30)));
        if (c < 0){
            tΔ2.Error("SliceCap failed unexpectedly");
        } else 
        if (c == (1 << (int)(30))) {
            tΔ2.Errorf("got capacity %d which is too high"u8, c);
        }
    });
    Ꮡt.Run("maxint"u8, (ж<testing.T> tΔ3) => {
        nint c = SliceCap<byte>(((uint64)1 << (int)(63)));
        if (c >= 0) {
            tΔ3.Errorf("SliceCap returned %d, expected failure"u8, c);
        }
    });
    Ꮡt.Run("overflow"u8, (ж<testing.T> tΔ4) => {
        nint c = SliceCap<int64>(((uint64)1 << (int)(62)));
        if (c >= 0) {
            tΔ4.Errorf("SliceCap returned %d, expected failure"u8, c);
        }
    });
}

} // end saferio_package
