// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This package defines the Go object file format, and provide "low-level" functions
// for reading and writing object files.

// The object file is understood by the compiler, assembler, linker, and tools. They
// have "high level" code that operates on object files, handling application-specific
// logics, and use this package for the actual reading and writing. Specifically, the
// code below:
//
// - cmd/internal/obj/objfile.go (used by cmd/asm and cmd/compile)
// - cmd/internal/objfile/goobj.go (used cmd/nm, cmd/objdump)
// - cmd/link/internal/loader package (used by cmd/link)
//
// If the object file format changes, they may (or may not) need to change.

// package goobj -- go2cs converted at 2022 March 06 22:32:29 UTC
// import "cmd/internal/goobj" ==> using goobj = go.cmd.@internal.goobj_package
// Original source: C:\Program Files\Go\src\cmd\internal\goobj\objfile.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using sha1 = go.crypto.sha1_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using unsafeheader = go.@internal.unsafeheader_package;
using io = go.io_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.@internal;

public static partial class goobj_package {

    // New object file format.
    //
    //    Header struct {
    //       Magic       [...]byte   // "\x00go117ld"
    //       Fingerprint [8]byte
    //       Flags       uint32
    //       Offsets     [...]uint32 // byte offset of each block below
    //    }
    //
    //    Strings [...]struct {
    //       Data [...]byte
    //    }
    //
    //    Autolib  [...]struct { // imported packages (for file loading)
    //       Pkg         string
    //       Fingerprint [8]byte
    //    }
    //
    //    PkgIndex [...]string // referenced packages by index
    //
    //    Files [...]string
    //
    //    SymbolDefs [...]struct {
    //       Name  string
    //       ABI   uint16
    //       Type  uint8
    //       Flag  uint8
    //       Flag2 uint8
    //       Size  uint32
    //    }
    //    Hashed64Defs [...]struct { // short hashed (content-addressable) symbol definitions
    //       ... // same as SymbolDefs
    //    }
    //    HashedDefs [...]struct { // hashed (content-addressable) symbol definitions
    //       ... // same as SymbolDefs
    //    }
    //    NonPkgDefs [...]struct { // non-pkg symbol definitions
    //       ... // same as SymbolDefs
    //    }
    //    NonPkgRefs [...]struct { // non-pkg symbol references
    //       ... // same as SymbolDefs
    //    }
    //
    //    RefFlags [...]struct { // referenced symbol flags
    //       Sym   symRef
    //       Flag  uint8
    //       Flag2 uint8
    //    }
    //
    //    Hash64 [...][8]byte
    //    Hash   [...][N]byte
    //
    //    RelocIndex [...]uint32 // index to Relocs
    //    AuxIndex   [...]uint32 // index to Aux
    //    DataIndex  [...]uint32 // offset to Data
    //
    //    Relocs [...]struct {
    //       Off  int32
    //       Size uint8
    //       Type uint16
    //       Add  int64
    //       Sym  symRef
    //    }
    //
    //    Aux [...]struct {
    //       Type uint8
    //       Sym  symRef
    //    }
    //
    //    Data   [...]byte
    //    Pcdata [...]byte
    //
    //    // blocks only used by tools (objdump, nm)
    //
    //    RefNames [...]struct { // referenced symbol names
    //       Sym  symRef
    //       Name string
    //       // TODO: include ABI version as well?
    //    }
    //
    // string is encoded as is a uint32 length followed by a uint32 offset
    // that points to the corresponding string bytes.
    //
    // symRef is struct { PkgIdx, SymIdx uint32 }.
    //
    // Slice type (e.g. []symRef) is encoded as a length prefix (uint32)
    // followed by that number of elements.
    //
    // The types below correspond to the encoded data structure in the
    // object file.

    // Symbol indexing.
    //
    // Each symbol is referenced with a pair of indices, { PkgIdx, SymIdx },
    // as the symRef struct above.
    //
    // PkgIdx is either a predeclared index (see PkgIdxNone below) or
    // an index of an imported package. For the latter case, PkgIdx is the
    // index of the package in the PkgIndex array. 0 is an invalid index.
    //
    // SymIdx is the index of the symbol in the given package.
    // - If PkgIdx is PkgIdxSelf, SymIdx is the index of the symbol in the
    //   SymbolDefs array.
    // - If PkgIdx is PkgIdxHashed64, SymIdx is the index of the symbol in the
    //   Hashed64Defs array.
    // - If PkgIdx is PkgIdxHashed, SymIdx is the index of the symbol in the
    //   HashedDefs array.
    // - If PkgIdx is PkgIdxNone, SymIdx is the index of the symbol in the
    //   NonPkgDefs array (could natually overflow to NonPkgRefs array).
    // - Otherwise, SymIdx is the index of the symbol in some other package's
    //   SymbolDefs array.
    //
    // {0, 0} represents a nil symbol. Otherwise PkgIdx should not be 0.
    //
    // Hash contains the content hashes of content-addressable symbols, of
    // which PkgIdx is PkgIdxHashed, in the same order of HashedDefs array.
    // Hash64 is similar, for PkgIdxHashed64 symbols.
    //
    // RelocIndex, AuxIndex, and DataIndex contains indices/offsets to
    // Relocs/Aux/Data blocks, one element per symbol, first for all the
    // defined symbols, then all the defined hashed and non-package symbols,
    // in the same order of SymbolDefs/Hashed64Defs/HashedDefs/NonPkgDefs
    // arrays. For N total defined symbols, the array is of length N+1. The
    // last element is the total number of relocations (aux symbols, data
    // blocks, etc.).
    //
    // They can be accessed by index. For the i-th symbol, its relocations
    // are the RelocIndex[i]-th (inclusive) to RelocIndex[i+1]-th (exclusive)
    // elements in the Relocs array. Aux/Data are likewise. (The index is
    // 0-based.)

    // Auxiliary symbols.
    //
    // Each symbol may (or may not) be associated with a number of auxiliary
    // symbols. They are described in the Aux block. See Aux struct below.
    // Currently a symbol's Gotype, FuncInfo, and associated DWARF symbols
    // are auxiliary symbols.
private static readonly nint stringRefSize = 8; // two uint32s

 // two uint32s

public partial struct FingerprintType { // : array<byte>
}

public static bool IsZero(this FingerprintType fp) {
    return fp == new FingerprintType();
}

// Package Index.
public static readonly nint PkgIdxNone = (1 << 31 - 1) - iota; // Non-package symbols
public static readonly var PkgIdxHashed64 = 0; // Short hashed (content-addressable) symbols
public static readonly var PkgIdxHashed = 1; // Hashed (content-addressable) symbols
public static readonly var PkgIdxBuiltin = 2; // Predefined runtime symbols (ex: runtime.newobject)
public static readonly PkgIdxInvalid PkgIdxSelf = 0; 
// The index of other referenced packages starts from 1.

// Blocks
public static readonly var BlkAutolib = iota;
public static readonly var BlkPkgIdx = 0;
public static readonly var BlkFile = 1;
public static readonly var BlkSymdef = 2;
public static readonly var BlkHashed64def = 3;
public static readonly var BlkHasheddef = 4;
public static readonly var BlkNonpkgdef = 5;
public static readonly var BlkNonpkgref = 6;
public static readonly var BlkRefFlags = 7;
public static readonly var BlkHash64 = 8;
public static readonly var BlkHash = 9;
public static readonly var BlkRelocIdx = 10;
public static readonly var BlkAuxIdx = 11;
public static readonly var BlkDataIdx = 12;
public static readonly var BlkReloc = 13;
public static readonly var BlkAux = 14;
public static readonly var BlkData = 15;
public static readonly var BlkPcdata = 16;
public static readonly var BlkRefName = 17;
public static readonly var BlkEnd = 18;
public static readonly var NBlk = 19;


// File header.
// TODO: probably no need to export this.
public partial struct Header {
    public @string Magic;
    public FingerprintType Fingerprint;
    public uint Flags;
    public array<uint> Offsets;
}

public static readonly @string Magic = "\x00go117ld";



private static void Write(this ptr<Header> _addr_h, ptr<Writer> _addr_w) {
    ref Header h = ref _addr_h.val;
    ref Writer w = ref _addr_w.val;

    w.RawString(h.Magic);
    w.Bytes(h.Fingerprint[..]);
    w.Uint32(h.Flags);
    foreach (var (_, x) in h.Offsets) {
        w.Uint32(x);
    }
}

private static error Read(this ptr<Header> _addr_h, ptr<Reader> _addr_r) {
    ref Header h = ref _addr_h.val;
    ref Reader r = ref _addr_r.val;

    var b = r.BytesAt(0, len(Magic));
    h.Magic = string(b);
    if (h.Magic != Magic) {
        return error.As(errors.New("wrong magic, not a Go object file"))!;
    }
    var off = uint32(len(h.Magic));
    copy(h.Fingerprint[..], r.BytesAt(off, len(h.Fingerprint)));
    off += 8;
    h.Flags = r.uint32At(off);
    off += 4;
    foreach (var (i) in h.Offsets) {
        h.Offsets[i] = r.uint32At(off);
        off += 4;
    }    return error.As(null!)!;

}

private static nint Size(this ptr<Header> _addr_h) {
    ref Header h = ref _addr_h.val;

    return len(h.Magic) + 4 + 4 * len(h.Offsets);
}

// Autolib
public partial struct ImportedPkg {
    public @string Pkg;
    public FingerprintType Fingerprint;
}

private static readonly var importedPkgSize = stringRefSize + 8;



private static void Write(this ptr<ImportedPkg> _addr_p, ptr<Writer> _addr_w) {
    ref ImportedPkg p = ref _addr_p.val;
    ref Writer w = ref _addr_w.val;

    w.StringRef(p.Pkg);
    w.Bytes(p.Fingerprint[..]);
}

// Symbol definition.
//
// Serialized format:
// Sym struct {
//    Name  string
//    ABI   uint16
//    Type  uint8
//    Flag  uint8
//    Flag2 uint8
//    Siz   uint32
//    Align uint32
// }
public partial struct Sym { // : array<byte>
}

public static readonly var SymSize = stringRefSize + 2 + 1 + 1 + 1 + 4 + 4;



public static readonly var SymABIstatic = ~uint16(0);



public static readonly nint ObjFlagShared = 1 << (int)(iota); // this object is built with -shared
public static readonly var ObjFlagNeedNameExpansion = 0; // the linker needs to expand `"".` to package path in symbol names
public static readonly var ObjFlagFromAssembly = 1; // object is from asm src, not go

// Sym.Flag
public static readonly nint SymFlagDupok = 1 << (int)(iota);
public static readonly var SymFlagLocal = 0;
public static readonly var SymFlagTypelink = 1;
public static readonly var SymFlagLeaf = 2;
public static readonly var SymFlagNoSplit = 3;
public static readonly var SymFlagReflectMethod = 4;
public static readonly var SymFlagGoType = 5;


// Sym.Flag2
public static readonly nint SymFlagUsedInIface = 1 << (int)(iota);
public static readonly var SymFlagItab = 0;


// Returns the length of the name of the symbol.
private static nint NameLen(this ptr<Sym> _addr_s, ptr<Reader> _addr_r) {
    ref Sym s = ref _addr_s.val;
    ref Reader r = ref _addr_r.val;

    return int(binary.LittleEndian.Uint32(s[..]));
}

private static @string Name(this ptr<Sym> _addr_s, ptr<Reader> _addr_r) {
    ref Sym s = ref _addr_s.val;
    ref Reader r = ref _addr_r.val;

    var len = binary.LittleEndian.Uint32(s[..]);
    var off = binary.LittleEndian.Uint32(s[(int)4..]);
    return r.StringAt(off, len);
}

private static ushort ABI(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return binary.LittleEndian.Uint16(s[(int)8..]);
}
private static byte Type(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s[10];
}
private static byte Flag(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s[11];
}
private static byte Flag2(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s[12];
}
private static uint Siz(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return binary.LittleEndian.Uint32(s[(int)13..]);
}
private static uint Align(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return binary.LittleEndian.Uint32(s[(int)17..]);
}

private static bool Dupok(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagDupok != 0;
}
private static bool Local(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagLocal != 0;
}
private static bool Typelink(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagTypelink != 0;
}
private static bool Leaf(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagLeaf != 0;
}
private static bool NoSplit(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagNoSplit != 0;
}
private static bool ReflectMethod(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagReflectMethod != 0;
}
private static bool IsGoType(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag() & SymFlagGoType != 0;
}
private static bool UsedInIface(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag2() & SymFlagUsedInIface != 0;
}
private static bool IsItab(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.Flag2() & SymFlagItab != 0;
}

private static void SetName(this ptr<Sym> _addr_s, @string x, ptr<Writer> _addr_w) {
    ref Sym s = ref _addr_s.val;
    ref Writer w = ref _addr_w.val;

    binary.LittleEndian.PutUint32(s[..], uint32(len(x)));
    binary.LittleEndian.PutUint32(s[(int)4..], w.stringOff(x));
}

private static void SetABI(this ptr<Sym> _addr_s, ushort x) {
    ref Sym s = ref _addr_s.val;

    binary.LittleEndian.PutUint16(s[(int)8..], x);
}
private static void SetType(this ptr<Sym> _addr_s, byte x) {
    ref Sym s = ref _addr_s.val;

    s[10] = x;
}
private static void SetFlag(this ptr<Sym> _addr_s, byte x) {
    ref Sym s = ref _addr_s.val;

    s[11] = x;
}
private static void SetFlag2(this ptr<Sym> _addr_s, byte x) {
    ref Sym s = ref _addr_s.val;

    s[12] = x;
}
private static void SetSiz(this ptr<Sym> _addr_s, uint x) {
    ref Sym s = ref _addr_s.val;

    binary.LittleEndian.PutUint32(s[(int)13..], x);
}
private static void SetAlign(this ptr<Sym> _addr_s, uint x) {
    ref Sym s = ref _addr_s.val;

    binary.LittleEndian.PutUint32(s[(int)17..], x);
}

private static void Write(this ptr<Sym> _addr_s, ptr<Writer> _addr_w) {
    ref Sym s = ref _addr_s.val;
    ref Writer w = ref _addr_w.val;

    w.Bytes(s[..]);
}

// for testing
private static void fromBytes(this ptr<Sym> _addr_s, slice<byte> b) {
    ref Sym s = ref _addr_s.val;

    copy(s[..], b);
}

// Symbol reference.
public partial struct SymRef {
    public uint PkgIdx;
    public uint SymIdx;
}

// Hash64
public partial struct Hash64Type { // : array<byte>
}

public static readonly nint Hash64Size = 8;

// Hash


// Hash
public partial struct HashType { // : array<byte>
}

public static readonly var HashSize = sha1.Size;

// Relocation.
//
// Serialized format:
// Reloc struct {
//    Off  int32
//    Siz  uint8
//    Type uint16
//    Add  int64
//    Sym  SymRef
// }


// Relocation.
//
// Serialized format:
// Reloc struct {
//    Off  int32
//    Siz  uint8
//    Type uint16
//    Add  int64
//    Sym  SymRef
// }
public partial struct Reloc { // : array<byte>
}

public static readonly nint RelocSize = 4 + 1 + 2 + 8 + 8;



private static int Off(this ptr<Reloc> _addr_r) {
    ref Reloc r = ref _addr_r.val;

    return int32(binary.LittleEndian.Uint32(r[..]));
}
private static byte Siz(this ptr<Reloc> _addr_r) {
    ref Reloc r = ref _addr_r.val;

    return r[4];
}
private static ushort Type(this ptr<Reloc> _addr_r) {
    ref Reloc r = ref _addr_r.val;

    return binary.LittleEndian.Uint16(r[(int)5..]);
}
private static long Add(this ptr<Reloc> _addr_r) {
    ref Reloc r = ref _addr_r.val;

    return int64(binary.LittleEndian.Uint64(r[(int)7..]));
}
private static SymRef Sym(this ptr<Reloc> _addr_r) {
    ref Reloc r = ref _addr_r.val;

    return new SymRef(binary.LittleEndian.Uint32(r[15:]),binary.LittleEndian.Uint32(r[19:]));
}

private static void SetOff(this ptr<Reloc> _addr_r, int x) {
    ref Reloc r = ref _addr_r.val;

    binary.LittleEndian.PutUint32(r[..], uint32(x));
}
private static void SetSiz(this ptr<Reloc> _addr_r, byte x) {
    ref Reloc r = ref _addr_r.val;

    r[4] = x;
}
private static void SetType(this ptr<Reloc> _addr_r, ushort x) {
    ref Reloc r = ref _addr_r.val;

    binary.LittleEndian.PutUint16(r[(int)5..], x);
}
private static void SetAdd(this ptr<Reloc> _addr_r, long x) {
    ref Reloc r = ref _addr_r.val;

    binary.LittleEndian.PutUint64(r[(int)7..], uint64(x));
}
private static void SetSym(this ptr<Reloc> _addr_r, SymRef x) {
    ref Reloc r = ref _addr_r.val;

    binary.LittleEndian.PutUint32(r[(int)15..], x.PkgIdx);
    binary.LittleEndian.PutUint32(r[(int)19..], x.SymIdx);
}

private static void Set(this ptr<Reloc> _addr_r, int off, byte size, ushort typ, long add, SymRef sym) {
    ref Reloc r = ref _addr_r.val;

    r.SetOff(off);
    r.SetSiz(size);
    r.SetType(typ);
    r.SetAdd(add);
    r.SetSym(sym);
}

private static void Write(this ptr<Reloc> _addr_r, ptr<Writer> _addr_w) {
    ref Reloc r = ref _addr_r.val;
    ref Writer w = ref _addr_w.val;

    w.Bytes(r[..]);
}

// for testing
private static void fromBytes(this ptr<Reloc> _addr_r, slice<byte> b) {
    ref Reloc r = ref _addr_r.val;

    copy(r[..], b);
}

// Aux symbol info.
//
// Serialized format:
// Aux struct {
//    Type uint8
//    Sym  SymRef
// }
public partial struct Aux { // : array<byte>
}

public static readonly nint AuxSize = 1 + 8;

// Aux Type


// Aux Type
public static readonly var AuxGotype = iota;
public static readonly var AuxFuncInfo = 0;
public static readonly var AuxFuncdata = 1;
public static readonly var AuxDwarfInfo = 2;
public static readonly var AuxDwarfLoc = 3;
public static readonly var AuxDwarfRanges = 4;
public static readonly var AuxDwarfLines = 5;
public static readonly var AuxPcsp = 6;
public static readonly var AuxPcfile = 7;
public static readonly var AuxPcline = 8;
public static readonly var AuxPcinline = 9;
public static readonly var AuxPcdata = 10;


private static byte Type(this ptr<Aux> _addr_a) {
    ref Aux a = ref _addr_a.val;

    return a[0];
}
private static SymRef Sym(this ptr<Aux> _addr_a) {
    ref Aux a = ref _addr_a.val;

    return new SymRef(binary.LittleEndian.Uint32(a[1:]),binary.LittleEndian.Uint32(a[5:]));
}

private static void SetType(this ptr<Aux> _addr_a, byte x) {
    ref Aux a = ref _addr_a.val;

    a[0] = x;
}
private static void SetSym(this ptr<Aux> _addr_a, SymRef x) {
    ref Aux a = ref _addr_a.val;

    binary.LittleEndian.PutUint32(a[(int)1..], x.PkgIdx);
    binary.LittleEndian.PutUint32(a[(int)5..], x.SymIdx);
}

private static void Write(this ptr<Aux> _addr_a, ptr<Writer> _addr_w) {
    ref Aux a = ref _addr_a.val;
    ref Writer w = ref _addr_w.val;

    w.Bytes(a[..]);
}

// for testing
private static void fromBytes(this ptr<Aux> _addr_a, slice<byte> b) {
    ref Aux a = ref _addr_a.val;

    copy(a[..], b);
}

// Referenced symbol flags.
//
// Serialized format:
// RefFlags struct {
//    Sym   symRef
//    Flag  uint8
//    Flag2 uint8
// }
public partial struct RefFlags { // : array<byte>
}

public static readonly nint RefFlagsSize = 8 + 1 + 1;



private static SymRef Sym(this ptr<RefFlags> _addr_r) {
    ref RefFlags r = ref _addr_r.val;

    return new SymRef(binary.LittleEndian.Uint32(r[:]),binary.LittleEndian.Uint32(r[4:]));
}
private static byte Flag(this ptr<RefFlags> _addr_r) {
    ref RefFlags r = ref _addr_r.val;

    return r[8];
}
private static byte Flag2(this ptr<RefFlags> _addr_r) {
    ref RefFlags r = ref _addr_r.val;

    return r[9];
}

private static void SetSym(this ptr<RefFlags> _addr_r, SymRef x) {
    ref RefFlags r = ref _addr_r.val;

    binary.LittleEndian.PutUint32(r[..], x.PkgIdx);
    binary.LittleEndian.PutUint32(r[(int)4..], x.SymIdx);
}
private static void SetFlag(this ptr<RefFlags> _addr_r, byte x) {
    ref RefFlags r = ref _addr_r.val;

    r[8] = x;
}
private static void SetFlag2(this ptr<RefFlags> _addr_r, byte x) {
    ref RefFlags r = ref _addr_r.val;

    r[9] = x;
}

private static void Write(this ptr<RefFlags> _addr_r, ptr<Writer> _addr_w) {
    ref RefFlags r = ref _addr_r.val;
    ref Writer w = ref _addr_w.val;

    w.Bytes(r[..]);
}

// Used to construct an artificially large array type when reading an
// item from the object file relocs section or aux sym section (needs
// to work on 32-bit as well as 64-bit). See issue 41621.
private static readonly nint huge = (1 << 31 - 1) / RelocSize;

// Referenced symbol name.
//
// Serialized format:
// RefName struct {
//    Sym  symRef
//    Name string
// }


// Referenced symbol name.
//
// Serialized format:
// RefName struct {
//    Sym  symRef
//    Name string
// }
public partial struct RefName { // : array<byte>
}

public static readonly nint RefNameSize = 8 + stringRefSize;



private static SymRef Sym(this ptr<RefName> _addr_n) {
    ref RefName n = ref _addr_n.val;

    return new SymRef(binary.LittleEndian.Uint32(n[:]),binary.LittleEndian.Uint32(n[4:]));
}
private static @string Name(this ptr<RefName> _addr_n, ptr<Reader> _addr_r) {
    ref RefName n = ref _addr_n.val;
    ref Reader r = ref _addr_r.val;

    var len = binary.LittleEndian.Uint32(n[(int)8..]);
    var off = binary.LittleEndian.Uint32(n[(int)12..]);
    return r.StringAt(off, len);
}

private static void SetSym(this ptr<RefName> _addr_n, SymRef x) {
    ref RefName n = ref _addr_n.val;

    binary.LittleEndian.PutUint32(n[..], x.PkgIdx);
    binary.LittleEndian.PutUint32(n[(int)4..], x.SymIdx);
}
private static void SetName(this ptr<RefName> _addr_n, @string x, ptr<Writer> _addr_w) {
    ref RefName n = ref _addr_n.val;
    ref Writer w = ref _addr_w.val;

    binary.LittleEndian.PutUint32(n[(int)8..], uint32(len(x)));
    binary.LittleEndian.PutUint32(n[(int)12..], w.stringOff(x));
}

private static void Write(this ptr<RefName> _addr_n, ptr<Writer> _addr_w) {
    ref RefName n = ref _addr_n.val;
    ref Writer w = ref _addr_w.val;

    w.Bytes(n[..]);
}

public partial struct Writer {
    public ptr<bio.Writer> wr;
    public map<@string, uint> stringMap;
    public uint off; // running offset
}

public static ptr<Writer> NewWriter(ptr<bio.Writer> _addr_wr) {
    ref bio.Writer wr = ref _addr_wr.val;

    return addr(new Writer(wr:wr,stringMap:make(map[string]uint32)));
}

private static void AddString(this ptr<Writer> _addr_w, @string s) {
    ref Writer w = ref _addr_w.val;

    {
        var (_, ok) = w.stringMap[s];

        if (ok) {
            return ;
        }
    }

    w.stringMap[s] = w.off;
    w.RawString(s);

}

private static uint stringOff(this ptr<Writer> _addr_w, @string s) => func((_, panic, _) => {
    ref Writer w = ref _addr_w.val;

    var (off, ok) = w.stringMap[s];
    if (!ok) {
        panic(fmt.Sprintf("writeStringRef: string not added: %q", s));
    }
    return off;

});

private static void StringRef(this ptr<Writer> _addr_w, @string s) {
    ref Writer w = ref _addr_w.val;

    w.Uint32(uint32(len(s)));
    w.Uint32(w.stringOff(s));
}

private static void RawString(this ptr<Writer> _addr_w, @string s) {
    ref Writer w = ref _addr_w.val;

    w.wr.WriteString(s);
    w.off += uint32(len(s));
}

private static void Bytes(this ptr<Writer> _addr_w, slice<byte> s) {
    ref Writer w = ref _addr_w.val;

    w.wr.Write(s);
    w.off += uint32(len(s));
}

private static void Uint64(this ptr<Writer> _addr_w, ulong x) {
    ref Writer w = ref _addr_w.val;

    array<byte> b = new array<byte>(8);
    binary.LittleEndian.PutUint64(b[..], x);
    w.wr.Write(b[..]);
    w.off += 8;
}

private static void Uint32(this ptr<Writer> _addr_w, uint x) {
    ref Writer w = ref _addr_w.val;

    array<byte> b = new array<byte>(4);
    binary.LittleEndian.PutUint32(b[..], x);
    w.wr.Write(b[..]);
    w.off += 4;
}

private static void Uint16(this ptr<Writer> _addr_w, ushort x) {
    ref Writer w = ref _addr_w.val;

    array<byte> b = new array<byte>(2);
    binary.LittleEndian.PutUint16(b[..], x);
    w.wr.Write(b[..]);
    w.off += 2;
}

private static void Uint8(this ptr<Writer> _addr_w, byte x) {
    ref Writer w = ref _addr_w.val;

    w.wr.WriteByte(x);
    w.off++;
}

private static uint Offset(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    return w.off;
}

public partial struct Reader {
    public slice<byte> b; // mmapped bytes, if not nil
    public bool @readonly; // whether b is backed with read-only memory

    public io.ReaderAt rd;
    public uint start;
    public Header h; // keep block offsets
}

public static ptr<Reader> NewReaderFromBytes(slice<byte> b, bool @readonly) {
    ptr<Reader> r = addr(new Reader(b:b,readonly:readonly,rd:bytes.NewReader(b),start:0));
    var err = r.h.Read(r);
    if (err != null) {
        return _addr_null!;
    }
    return _addr_r!;

}

private static slice<byte> BytesAt(this ptr<Reader> _addr_r, uint off, nint len) {
    ref Reader r = ref _addr_r.val;

    if (len == 0) {
        return null;
    }
    var end = int(off) + len;
    return r.b.slice(int(off), end, end);

}

private static ulong uint64At(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    var b = r.BytesAt(off, 8);
    return binary.LittleEndian.Uint64(b);
}

private static long int64At(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    return int64(r.uint64At(off));
}

private static uint uint32At(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    var b = r.BytesAt(off, 4);
    return binary.LittleEndian.Uint32(b);
}

private static int int32At(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    return int32(r.uint32At(off));
}

private static ushort uint16At(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    var b = r.BytesAt(off, 2);
    return binary.LittleEndian.Uint16(b);
}

private static byte uint8At(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    var b = r.BytesAt(off, 1);
    return b[0];
}

private static @string StringAt(this ptr<Reader> _addr_r, uint off, uint len) {
    ref Reader r = ref _addr_r.val;

    var b = r.b[(int)off..(int)off + len];
    if (r.@readonly) {
        return toString(b); // backed by RO memory, ok to make unsafe string
    }
    return string(b);

}

private static @string toString(slice<byte> b) {
    if (len(b) == 0) {
        return "";
    }
    ref @string s = ref heap(out ptr<@string> _addr_s);
    var hdr = (unsafeheader.String.val)(@unsafe.Pointer(_addr_s));
    hdr.Data = @unsafe.Pointer(_addr_b[0]);
    hdr.Len = len(b);

    return s;

}

private static @string StringRef(this ptr<Reader> _addr_r, uint off) {
    ref Reader r = ref _addr_r.val;

    var l = r.uint32At(off);
    return r.StringAt(r.uint32At(off + 4), l);
}

private static FingerprintType Fingerprint(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return r.h.Fingerprint;
}

private static slice<ImportedPkg> Autolib(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    var n = (r.h.Offsets[BlkAutolib + 1] - r.h.Offsets[BlkAutolib]) / importedPkgSize;
    var s = make_slice<ImportedPkg>(n);
    var off = r.h.Offsets[BlkAutolib];
    foreach (var (i) in s) {
        s[i].Pkg = r.StringRef(off);
        copy(s[i].Fingerprint[..], r.BytesAt(off + stringRefSize, len(s[i].Fingerprint)));
        off += importedPkgSize;
    }    return s;
}

private static slice<@string> Pkglist(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    var n = (r.h.Offsets[BlkPkgIdx + 1] - r.h.Offsets[BlkPkgIdx]) / stringRefSize;
    var s = make_slice<@string>(n);
    var off = r.h.Offsets[BlkPkgIdx];
    foreach (var (i) in s) {
        s[i] = r.StringRef(off);
        off += stringRefSize;
    }    return s;
}

private static nint NPkg(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkPkgIdx + 1] - r.h.Offsets[BlkPkgIdx]) / stringRefSize;
}

private static @string Pkg(this ptr<Reader> _addr_r, nint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.h.Offsets[BlkPkgIdx] + uint32(i) * stringRefSize;
    return r.StringRef(off);
}

private static nint NFile(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkFile + 1] - r.h.Offsets[BlkFile]) / stringRefSize;
}

private static @string File(this ptr<Reader> _addr_r, nint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.h.Offsets[BlkFile] + uint32(i) * stringRefSize;
    return r.StringRef(off);
}

private static nint NSym(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkSymdef + 1] - r.h.Offsets[BlkSymdef]) / SymSize;
}

private static nint NHashed64def(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkHashed64def + 1] - r.h.Offsets[BlkHashed64def]) / SymSize;
}

private static nint NHasheddef(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkHasheddef + 1] - r.h.Offsets[BlkHasheddef]) / SymSize;
}

private static nint NNonpkgdef(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkNonpkgdef + 1] - r.h.Offsets[BlkNonpkgdef]) / SymSize;
}

private static nint NNonpkgref(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkNonpkgref + 1] - r.h.Offsets[BlkNonpkgref]) / SymSize;
}

// SymOff returns the offset of the i-th symbol.
private static uint SymOff(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    return r.h.Offsets[BlkSymdef] + uint32(i * SymSize);
}

// Sym returns a pointer to the i-th symbol.
private static ptr<Sym> Sym(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.SymOff(i);
    return _addr_(Sym.val)(@unsafe.Pointer(_addr_r.b[off]))!;
}

// NRefFlags returns the number of referenced symbol flags.
private static nint NRefFlags(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkRefFlags + 1] - r.h.Offsets[BlkRefFlags]) / RefFlagsSize;
}

// RefFlags returns a pointer to the i-th referenced symbol flags.
// Note: here i is not a local symbol index, just a counter.
private static ptr<RefFlags> RefFlags(this ptr<Reader> _addr_r, nint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.h.Offsets[BlkRefFlags] + uint32(i * RefFlagsSize);
    return _addr_(RefFlags.val)(@unsafe.Pointer(_addr_r.b[off]))!;
}

// Hash64 returns the i-th short hashed symbol's hash.
// Note: here i is the index of short hashed symbols, not all symbols
// (unlike other accessors).
private static ulong Hash64(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.h.Offsets[BlkHash64] + uint32(i * Hash64Size);
    return r.uint64At(off);
}

// Hash returns a pointer to the i-th hashed symbol's hash.
// Note: here i is the index of hashed symbols, not all symbols
// (unlike other accessors).
private static ptr<HashType> Hash(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.h.Offsets[BlkHash] + uint32(i * HashSize);
    return _addr_(HashType.val)(@unsafe.Pointer(_addr_r.b[off]))!;
}

// NReloc returns the number of relocations of the i-th symbol.
private static nint NReloc(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var relocIdxOff = r.h.Offsets[BlkRelocIdx] + uint32(i * 4);
    return int(r.uint32At(relocIdxOff + 4) - r.uint32At(relocIdxOff));
}

// RelocOff returns the offset of the j-th relocation of the i-th symbol.
private static uint RelocOff(this ptr<Reader> _addr_r, uint i, nint j) {
    ref Reader r = ref _addr_r.val;

    var relocIdxOff = r.h.Offsets[BlkRelocIdx] + uint32(i * 4);
    var relocIdx = r.uint32At(relocIdxOff);
    return r.h.Offsets[BlkReloc] + (relocIdx + uint32(j)) * uint32(RelocSize);
}

// Reloc returns a pointer to the j-th relocation of the i-th symbol.
private static ptr<Reloc> Reloc(this ptr<Reader> _addr_r, uint i, nint j) {
    ref Reader r = ref _addr_r.val;

    var off = r.RelocOff(i, j);
    return _addr_(Reloc.val)(@unsafe.Pointer(_addr_r.b[off]))!;
}

// Relocs returns a pointer to the relocations of the i-th symbol.
private static slice<Reloc> Relocs(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.RelocOff(i, 0);
    var n = r.NReloc(i);
    return new ptr<ptr<array<Reloc>>>(@unsafe.Pointer(_addr_r.b[off])).slice(-1, n, n);
}

// NAux returns the number of aux symbols of the i-th symbol.
private static nint NAux(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var auxIdxOff = r.h.Offsets[BlkAuxIdx] + i * 4;
    return int(r.uint32At(auxIdxOff + 4) - r.uint32At(auxIdxOff));
}

// AuxOff returns the offset of the j-th aux symbol of the i-th symbol.
private static uint AuxOff(this ptr<Reader> _addr_r, uint i, nint j) {
    ref Reader r = ref _addr_r.val;

    var auxIdxOff = r.h.Offsets[BlkAuxIdx] + i * 4;
    var auxIdx = r.uint32At(auxIdxOff);
    return r.h.Offsets[BlkAux] + (auxIdx + uint32(j)) * uint32(AuxSize);
}

// Aux returns a pointer to the j-th aux symbol of the i-th symbol.
private static ptr<Aux> Aux(this ptr<Reader> _addr_r, uint i, nint j) {
    ref Reader r = ref _addr_r.val;

    var off = r.AuxOff(i, j);
    return _addr_(Aux.val)(@unsafe.Pointer(_addr_r.b[off]))!;
}

// Auxs returns the aux symbols of the i-th symbol.
private static slice<Aux> Auxs(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.AuxOff(i, 0);
    var n = r.NAux(i);
    return new ptr<ptr<array<Aux>>>(@unsafe.Pointer(_addr_r.b[off])).slice(-1, n, n);
}

// DataOff returns the offset of the i-th symbol's data.
private static uint DataOff(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var dataIdxOff = r.h.Offsets[BlkDataIdx] + i * 4;
    return r.h.Offsets[BlkData] + r.uint32At(dataIdxOff);
}

// DataSize returns the size of the i-th symbol's data.
private static nint DataSize(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var dataIdxOff = r.h.Offsets[BlkDataIdx] + i * 4;
    return int(r.uint32At(dataIdxOff + 4) - r.uint32At(dataIdxOff));
}

// Data returns the i-th symbol's data.
private static slice<byte> Data(this ptr<Reader> _addr_r, uint i) {
    ref Reader r = ref _addr_r.val;

    var dataIdxOff = r.h.Offsets[BlkDataIdx] + i * 4;
    var @base = r.h.Offsets[BlkData];
    var off = r.uint32At(dataIdxOff);
    var end = r.uint32At(dataIdxOff + 4);
    return r.BytesAt(base + off, int(end - off));
}

// NRefName returns the number of referenced symbol names.
private static nint NRefName(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int(r.h.Offsets[BlkRefName + 1] - r.h.Offsets[BlkRefName]) / RefNameSize;
}

// RefName returns a pointer to the i-th referenced symbol name.
// Note: here i is not a local symbol index, just a counter.
private static ptr<RefName> RefName(this ptr<Reader> _addr_r, nint i) {
    ref Reader r = ref _addr_r.val;

    var off = r.h.Offsets[BlkRefName] + uint32(i * RefNameSize);
    return _addr_(RefName.val)(@unsafe.Pointer(_addr_r.b[off]))!;
}

// ReadOnly returns whether r.BytesAt returns read-only bytes.
private static bool ReadOnly(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return r.@readonly;
}

// Flags returns the flag bits read from the object file header.
private static uint Flags(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return r.h.Flags;
}

private static bool Shared(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return r.Flags() & ObjFlagShared != 0;
}
private static bool NeedNameExpansion(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return r.Flags() & ObjFlagNeedNameExpansion != 0;
}
private static bool FromAssembly(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return r.Flags() & ObjFlagFromAssembly != 0;
}

} // end goobj_package
