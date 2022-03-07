// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package codesign provides basic functionalities for
// ad-hoc code signing of Mach-O files.
//
// This is not a general tool for code-signing. It is made
// specifically for the Go toolchain. It uses the same
// ad-hoc signing algorithm as the Darwin linker.
// package codesign -- go2cs converted at 2022 March 06 22:46:42 UTC
// import "cmd/internal/codesign" ==> using codesign = go.cmd.@internal.codesign_package
// Original source: C:\Program Files\Go\src\cmd\internal\codesign\codesign.go
using sha256 = go.crypto.sha256_package;
using macho = go.debug.macho_package;
using binary = go.encoding.binary_package;
using io = go.io_package;

namespace go.cmd.@internal;

public static partial class codesign_package {

    // Code signature layout.
    //
    // The code signature is a block of bytes that contains
    // a SuperBlob, which contains one or more Blobs. For ad-hoc
    // signing, a single CodeDirectory Blob suffices.
    //
    // A SuperBlob starts with its header (the binary representation
    // of the SuperBlob struct), followed by a list of (in our case,
    // one) Blobs (offset and size). A CodeDirectory Blob starts
    // with its head (the binary representation of CodeDirectory struct),
    // followed by the identifier (as a C string) and the hashes, at
    // the corresponding offsets.
    //
    // The signature data must be included in the __LINKEDIT segment.
    // In the Mach-O file header, an LC_CODE_SIGNATURE load command
    // points to the data.
private static readonly nint pageSizeBits = 12;
private static readonly nint pageSize = 1 << (int)(pageSizeBits);


public static readonly nuint LC_CODE_SIGNATURE = 0x1d;

// Constants and struct layouts are from
// https://opensource.apple.com/source/xnu/xnu-4903.270.47/osfmk/kern/cs_blobs.h



// Constants and struct layouts are from
// https://opensource.apple.com/source/xnu/xnu-4903.270.47/osfmk/kern/cs_blobs.h

public static readonly nuint CSMAGIC_REQUIREMENT = 0xfade0c00; // single Requirement blob
public static readonly nuint CSMAGIC_REQUIREMENTS = 0xfade0c01; // Requirements vector (internal requirements)
public static readonly nuint CSMAGIC_CODEDIRECTORY = 0xfade0c02; // CodeDirectory blob
public static readonly nuint CSMAGIC_EMBEDDED_SIGNATURE = 0xfade0cc0; // embedded form of signature data
public static readonly nuint CSMAGIC_DETACHED_SIGNATURE = 0xfade0cc1; // multi-arch collection of embedded signatures

public static readonly nint CSSLOT_CODEDIRECTORY = 0; // slot index for CodeDirectory

public static readonly nint CS_HASHTYPE_SHA1 = 1;
public static readonly nint CS_HASHTYPE_SHA256 = 2;
public static readonly nint CS_HASHTYPE_SHA256_TRUNCATED = 3;
public static readonly nint CS_HASHTYPE_SHA384 = 4;


public static readonly nuint CS_EXECSEG_MAIN_BINARY = 0x1; // executable segment denotes main binary
public static readonly nuint CS_EXECSEG_ALLOW_UNSIGNED = 0x10; // allow unsigned pages (for debugging)
public static readonly nuint CS_EXECSEG_DEBUGGER = 0x20; // main binary is debugger
public static readonly nuint CS_EXECSEG_JIT = 0x40; // JIT enabled
public static readonly nuint CS_EXECSEG_SKIP_LV = 0x80; // skip library validation
public static readonly nuint CS_EXECSEG_CAN_LOAD_CDHASH = 0x100; // can bless cdhash for execution
public static readonly nuint CS_EXECSEG_CAN_EXEC_CDHASH = 0x200; // can execute blessed cdhash

public partial struct Blob {
    public uint typ; // type of entry
    public uint offset; // offset of entry
// data follows
}

private static slice<byte> put(this ptr<Blob> _addr_b, slice<byte> @out) {
    ref Blob b = ref _addr_b.val;

    out = put32be(out, b.typ);
    out = put32be(out, b.offset);
    return out;
}

private static readonly nint blobSize = 2 * 4;



public partial struct SuperBlob {
    public uint magic; // magic number
    public uint length; // total length of SuperBlob
    public uint count; // number of index entries following
// blobs []Blob
}

private static slice<byte> put(this ptr<SuperBlob> _addr_s, slice<byte> @out) {
    ref SuperBlob s = ref _addr_s.val;

    out = put32be(out, s.magic);
    out = put32be(out, s.length);
    out = put32be(out, s.count);
    return out;
}

private static readonly nint superBlobSize = 3 * 4;



public partial struct CodeDirectory {
    public uint magic; // magic number (CSMAGIC_CODEDIRECTORY)
    public uint length; // total length of CodeDirectory blob
    public uint version; // compatibility version
    public uint flags; // setup and mode flags
    public uint hashOffset; // offset of hash slot element at index zero
    public uint identOffset; // offset of identifier string
    public uint nSpecialSlots; // number of special hash slots
    public uint nCodeSlots; // number of ordinary (code) hash slots
    public uint codeLimit; // limit to main image signature range
    public byte hashSize; // size of each hash in bytes
    public byte hashType; // type of hash (cdHashType* constants)
    public byte _pad1; // unused (must be zero)
    public byte pageSize; // log2(page size in bytes); 0 => infinite
    public uint _pad2; // unused (must be zero)
    public uint scatterOffset;
    public uint teamOffset;
    public uint _pad3;
    public ulong codeLimit64;
    public ulong execSegBase;
    public ulong execSegLimit;
    public ulong execSegFlags; // data follows
}

private static slice<byte> put(this ptr<CodeDirectory> _addr_c, slice<byte> @out) {
    ref CodeDirectory c = ref _addr_c.val;

    out = put32be(out, c.magic);
    out = put32be(out, c.length);
    out = put32be(out, c.version);
    out = put32be(out, c.flags);
    out = put32be(out, c.hashOffset);
    out = put32be(out, c.identOffset);
    out = put32be(out, c.nSpecialSlots);
    out = put32be(out, c.nCodeSlots);
    out = put32be(out, c.codeLimit);
    out = put8(out, c.hashSize);
    out = put8(out, c.hashType);
    out = put8(out, c._pad1);
    out = put8(out, c.pageSize);
    out = put32be(out, c._pad2);
    out = put32be(out, c.scatterOffset);
    out = put32be(out, c.teamOffset);
    out = put32be(out, c._pad3);
    out = put64be(out, c.codeLimit64);
    out = put64be(out, c.execSegBase);
    out = put64be(out, c.execSegLimit);
    out = put64be(out, c.execSegFlags);
    return out;
}

private static readonly nint codeDirectorySize = 13 * 4 + 4 + 4 * 8;

// CodeSigCmd is Mach-O LC_CODE_SIGNATURE load command.


// CodeSigCmd is Mach-O LC_CODE_SIGNATURE load command.
public partial struct CodeSigCmd {
    public uint Cmd; // LC_CODE_SIGNATURE
    public uint Cmdsize; // sizeof this command (16)
    public uint Dataoff; // file offset of data in __LINKEDIT segment
    public uint Datasize; // file size of data in __LINKEDIT segment
}

public static (CodeSigCmd, bool) FindCodeSigCmd(ptr<macho.File> _addr_f) {
    CodeSigCmd _p0 = default;
    bool _p0 = default;
    ref macho.File f = ref _addr_f.val;

    var get32 = f.ByteOrder.Uint32;
    foreach (var (_, l) in f.Loads) {
        var data = l.Raw();
        var cmd = get32(data);
        if (cmd == LC_CODE_SIGNATURE) {
            return (new CodeSigCmd(cmd,get32(data[4:]),get32(data[8:]),get32(data[12:]),), true);
        }
    }    return (new CodeSigCmd(), false);

}

private static slice<byte> put32be(slice<byte> b, uint x) {
    binary.BigEndian.PutUint32(b, x);

    return b[(int)4..];
}
private static slice<byte> put64be(slice<byte> b, ulong x) {
    binary.BigEndian.PutUint64(b, x);

    return b[(int)8..];
}
private static slice<byte> put8(slice<byte> b, byte x) {
    b[0] = x;

    return b[(int)1..];
}
private static slice<byte> puts(slice<byte> b, slice<byte> s) {
    var n = copy(b, s);

    return b[(int)n..];
}

// Size computes the size of the code signature.
// id is the identifier used for signing (a field in CodeDirectory blob, which
// has no significance in ad-hoc signing).
public static long Size(long codeSize, @string id) {
    var nhashes = (codeSize + pageSize - 1) / pageSize;
    var idOff = int64(codeDirectorySize);
    var hashOff = idOff + int64(len(id) + 1);
    var cdirSz = hashOff + nhashes * sha256.Size;
    return int64(superBlobSize + blobSize) + cdirSz;
}

// Sign generates an ad-hoc code signature and writes it to out.
// out must have length at least Size(codeSize, id).
// data is the file content without the signature, of size codeSize.
// textOff and textSize is the file offset and size of the text segment.
// isMain is true if this is a main executable.
// id is the identifier used for signing (a field in CodeDirectory blob, which
// has no significance in ad-hoc signing).
public static void Sign(slice<byte> @out, io.Reader data, @string id, long codeSize, long textOff, long textSize, bool isMain) => func((_, panic, _) => {
    var nhashes = (codeSize + pageSize - 1) / pageSize;
    var idOff = int64(codeDirectorySize);
    var hashOff = idOff + int64(len(id) + 1);
    var sz = Size(codeSize, id); 

    // emit blob headers
    SuperBlob sb = new SuperBlob(magic:CSMAGIC_EMBEDDED_SIGNATURE,length:uint32(sz),count:1,);
    Blob blob = new Blob(typ:CSSLOT_CODEDIRECTORY,offset:superBlobSize+blobSize,);
    CodeDirectory cdir = new CodeDirectory(magic:CSMAGIC_CODEDIRECTORY,length:uint32(sz)-(superBlobSize+blobSize),version:0x20400,flags:0x20002,hashOffset:uint32(hashOff),identOffset:uint32(idOff),nCodeSlots:uint32(nhashes),codeLimit:uint32(codeSize),hashSize:sha256.Size,hashType:CS_HASHTYPE_SHA256,pageSize:uint8(pageSizeBits),execSegBase:uint64(textOff),execSegLimit:uint64(textSize),);
    if (isMain) {
        cdir.execSegFlags = CS_EXECSEG_MAIN_BINARY;
    }
    var outp = out;
    outp = sb.put(outp);
    outp = blob.put(outp);
    outp = cdir.put(outp); 

    // emit the identifier
    outp = puts(outp, (slice<byte>)id + " "); 

    // emit hashes
    array<byte> buf = new array<byte>(pageSize);
    var h = sha256.New();
    nint p = 0;
    while (p < int(codeSize)) {
        var (n, err) = io.ReadFull(data, buf[..]);
        if (err == io.EOF) {
            break;
        }
        if (err != null && err != io.ErrUnexpectedEOF) {
            panic(err);
        }
        if (p + n > int(codeSize)) {
            n = int(codeSize) - p;
        }
        p += n;
        h.Reset();
        h.Write(buf[..(int)n]);
        var b = h.Sum(null);
        outp = puts(outp, b[..]);

    }

});

} // end codesign_package
