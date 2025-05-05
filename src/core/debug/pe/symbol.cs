// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using @unsafe = unsafe_package;
using @internal;
using encoding;

partial class pe_package {

public static readonly UntypedInt COFFSymbolSize = 18;

// COFFSymbol represents single COFF symbol table record.
[GoType] partial struct COFFSymbol {
    public array<uint8> Name = new(8);
    public uint32 Value;
    public int16 SectionNumber;
    public uint16 Type;
    public uint8 StorageClass;
    public uint8 NumberOfAuxSymbols;
}

// readCOFFSymbols reads in the symbol table for a PE file, returning
// a slice of COFFSymbol objects. The PE format includes both primary
// symbols (whose fields are described by COFFSymbol above) and
// auxiliary symbols; all symbols are 18 bytes in size. The auxiliary
// symbols for a given primary symbol are placed following it in the
// array, e.g.
//
//	...
//	k+0:  regular sym k
//	k+1:    1st aux symbol for k
//	k+2:    2nd aux symbol for k
//	k+3:  regular sym k+3
//	k+4:    1st aux symbol for k+3
//	k+5:  regular sym k+5
//	k+6:  regular sym k+6
//
// The PE format allows for several possible aux symbol formats. For
// more info see:
//
//	https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#auxiliary-symbol-records
//
// At the moment this package only provides APIs for looking at
// aux symbols of format 5 (associated with section definition symbols).
internal static (slice<COFFSymbol>, error) readCOFFSymbols(ж<FileHeader> Ꮡfh, io.ReadSeeker r) {
    ref var fh = ref Ꮡfh.val;

    if (fh.PointerToSymbolTable == 0) {
        return (default!, default!);
    }
    if (fh.NumberOfSymbols <= 0) {
        return (default!, default!);
    }
    var (_, err) = r.Seek(((int64)fh.PointerToSymbolTable), io.SeekStart);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to seek to symbol table: %v"u8, err));
    }
    nint c = saferio.SliceCap<COFFSymbol>(((uint64)fh.NumberOfSymbols));
    if (c < 0) {
        return (default!, errors.New("too many symbols; file may be corrupt"u8));
    }
    var syms = new slice<COFFSymbol>(0, c);
    nint naux = 0;
    for (var k = ((uint32)0); k < fh.NumberOfSymbols; k++) {
        ref var sym = ref heap(new COFFSymbol(), out var Ꮡsym);
        if (naux == 0){
            // Read a primary symbol.
            err = binary.Read(r, binary.LittleEndian, Ꮡsym);
            if (err != default!) {
                return (default!, fmt.Errorf("fail to read symbol table: %v"u8, err));
            }
            // Record how many auxiliary symbols it has.
            naux = ((nint)sym.NumberOfAuxSymbols);
        } else {
            // Read an aux symbol. At the moment we assume all
            // aux symbols are format 5 (obviously this doesn't always
            // hold; more cases will be needed below if more aux formats
            // are supported in the future).
            naux--;
            var aux = (ж<COFFSymbolAuxFormat5>)(uintptr)(new @unsafe.Pointer(Ꮡsym));
            err = binary.Read(r, binary.LittleEndian, aux);
            if (err != default!) {
                return (default!, fmt.Errorf("fail to read symbol table: %v"u8, err));
            }
        }
        syms = append(syms, sym);
    }
    if (naux != 0) {
        return (default!, fmt.Errorf("fail to read symbol table: %d aux symbols unread"u8, naux));
    }
    return (syms, default!);
}

// isSymNameOffset checks symbol name if it is encoded as offset into string table.
internal static (bool, uint32) isSymNameOffset(array<byte> name) {
    name = name.Clone();

    if (name[0] == 0 && name[1] == 0 && name[2] == 0 && name[3] == 0) {
        return (true, binary.LittleEndian.Uint32(name[4..]));
    }
    return (false, 0);
}

// FullName finds real name of symbol sym. Normally name is stored
// in sym.Name, but if it is longer then 8 characters, it is stored
// in COFF string table st instead.
[GoRecv] public static (@string, error) FullName(this ref COFFSymbol sym, StringTable st) {
    {
        var (ok, offset) = isSymNameOffset(sym.Name); if (ok) {
            return st.String(offset);
        }
    }
    return (cstring(sym.Name[..]), default!);
}

internal static (slice<ж<Symbol>>, error) removeAuxSymbols(slice<COFFSymbol> allsyms, StringTable st) {
    if (len(allsyms) == 0) {
        return (default!, default!);
    }
    var syms = new slice<ж<Symbol>>(0);
    var aux = ((uint8)0);
    foreach (var (_, sym) in allsyms) {
        if (aux > 0) {
            aux--;
            continue;
        }
        (name, err) = sym.FullName(st);
        if (err != default!) {
            return (default!, err);
        }
        aux = sym.NumberOfAuxSymbols;
        var s = Ꮡ(new Symbol(
            Name: name,
            Value: sym.Value,
            SectionNumber: sym.SectionNumber,
            Type: sym.Type,
            StorageClass: sym.StorageClass
        ));
        syms = append(syms, s);
    }
    return (syms, default!);
}

// Symbol is similar to [COFFSymbol] with Name field replaced
// by Go string. Symbol also does not have NumberOfAuxSymbols.
[GoType] partial struct Symbol {
    public @string Name;
    public uint32 Value;
    public int16 SectionNumber;
    public uint16 Type;
    public uint8 StorageClass;
}

// COFFSymbolAuxFormat5 describes the expected form of an aux symbol
// attached to a section definition symbol. The PE format defines a
// number of different aux symbol formats: format 1 for function
// definitions, format 2 for .be and .ef symbols, and so on. Format 5
// holds extra info associated with a section definition, including
// number of relocations + line numbers, as well as COMDAT info. See
// https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#auxiliary-format-5-section-definitions
// for more on what's going on here.
[GoType] partial struct COFFSymbolAuxFormat5 {
    public uint32 Size;
    public uint16 NumRelocs;
    public uint16 NumLineNumbers;
    public uint32 Checksum;
    public uint16 SecNum;
    public uint8 Selection;
    internal array<uint8> _ = new(3); // padding
}

// These constants make up the possible values for the 'Selection'
// field in an AuxFormat5.
public static readonly UntypedInt IMAGE_COMDAT_SELECT_NODUPLICATES = 1;

public static readonly UntypedInt IMAGE_COMDAT_SELECT_ANY = 2;

public static readonly UntypedInt IMAGE_COMDAT_SELECT_SAME_SIZE = 3;

public static readonly UntypedInt IMAGE_COMDAT_SELECT_EXACT_MATCH = 4;

public static readonly UntypedInt IMAGE_COMDAT_SELECT_ASSOCIATIVE = 5;

public static readonly UntypedInt IMAGE_COMDAT_SELECT_LARGEST = 6;

// COFFSymbolReadSectionDefAux returns a blob of auxiliary information
// (including COMDAT info) for a section definition symbol. Here 'idx'
// is the index of a section symbol in the main [COFFSymbol] array for
// the File. Return value is a pointer to the appropriate aux symbol
// struct. For more info, see:
//
// auxiliary symbols: https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#auxiliary-symbol-records
// COMDAT sections: https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#comdat-sections-object-only
// auxiliary info for section definitions: https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#auxiliary-format-5-section-definitions
[GoRecv] public static (ж<COFFSymbolAuxFormat5>, error) COFFSymbolReadSectionDefAux(this ref File f, nint idx) {
    ж<COFFSymbolAuxFormat5> rv = default!;
    if (idx < 0 || idx >= len(f.COFFSymbols)) {
        return (rv, fmt.Errorf("invalid symbol index"u8));
    }
    var pesym = Ꮡ(f.COFFSymbols[idx]);
    static readonly UntypedInt IMAGE_SYM_CLASS_STATIC = 3;
    if ((~pesym).StorageClass != ((uint8)IMAGE_SYM_CLASS_STATIC)) {
        return (rv, fmt.Errorf("incorrect symbol storage class"u8));
    }
    if ((~pesym).NumberOfAuxSymbols == 0 || idx + 1 >= len(f.COFFSymbols)) {
        return (rv, fmt.Errorf("aux symbol unavailable"u8));
    }
    // Locate and return a pointer to the successor aux symbol.
    var pesymn = Ꮡ(f.COFFSymbols[idx + 1]);
    rv = (ж<COFFSymbolAuxFormat5>)(uintptr)(new @unsafe.Pointer(pesymn));
    return (rv, default!);
}

} // end pe_package
