// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
/*
 * Line tables
 */
namespace go.debug;

using bytes = bytes_package;
using binary = encoding.binary_package;
using sort = sort_package;
using sync = sync_package;
using encoding;

partial class gosym_package {

[GoType("num:nint")] partial struct version;

internal static readonly version verUnknown = /* iota */ 0;
internal static readonly version ver11 = 1;
internal static readonly version ver12 = 2;
internal static readonly version ver116 = 3;
internal static readonly version ver118 = 4;
internal static readonly version ver120 = 5;

// A LineTable is a data structure mapping program counters to line numbers.
//
// In Go 1.1 and earlier, each function (represented by a [Func]) had its own LineTable,
// and the line number corresponded to a numbering of all source lines in the
// program, across all files. That absolute line number would then have to be
// converted separately to a file name and line number within the file.
//
// In Go 1.2, the format of the data changed so that there is a single LineTable
// for the entire program, shared by all Funcs, and there are no absolute line
// numbers, just line numbers within specific files.
//
// For the most part, LineTable's methods should be treated as an internal
// detail of the package; callers should use the methods on [Table] instead.
[GoType] partial struct LineTable {
    public slice<byte> Data;
    public uint64 PC;
    public nint Line;
    // This mutex is used to keep parsing of pclntab synchronous.
    internal sync_package.Mutex mu;
    // Contains the version of the pclntab section.
    internal version version;
    // Go 1.2/1.16/1.18 state
    internal encoding.binary_package.ByteOrder binary;
    internal uint32 quantum;
    internal uint32 ptrsize;
    internal uint64 textStart; // address of runtime.text symbol (1.18+)
    internal slice<byte> funcnametab;
    internal slice<byte> cutab;
    internal slice<byte> funcdata;
    internal slice<byte> functab;
    internal uint32 nfunctab;
    internal slice<byte> filetab;
    internal slice<byte> pctab; // points to the pctables.
    internal uint32 nfiletab;
    internal map<uint32, @string> funcNames; // cache the function names
    internal map<uint32, @string> strings; // interned substrings of Data, keyed by offset
    // fileMap varies depending on the version of the object file.
    // For ver12, it maps the name to the index in the file table.
    // For ver116, it maps the name to the offset in filetab.
    internal map<@string, uint32> fileMap;
}

// NOTE(rsc): This is wrong for GOARCH=arm, which uses a quantum of 4,
// but we have no idea whether we're using arm or not. This only
// matters in the old (pre-Go 1.2) symbol table format, so it's not worth
// fixing.
internal static readonly UntypedInt oldQuantum = 1;

[GoRecv] internal static (slice<byte> b, uint64 pc, nint line) parse(this ref LineTable t, uint64 targetPC, nint targetLine) {
    slice<byte> b = default!;
    uint64 pc = default!;
    nint line = default!;

    // The PC/line table can be thought of as a sequence of
    //  <pc update>* <line update>
    // batches. Each update batch results in a (pc, line) pair,
    // where line applies to every PC from pc up to but not
    // including the pc of the next pair.
    //
    // Here we process each update individually, which simplifies
    // the code, but makes the corner cases more confusing.
    (b, pc, line) = (t.Data, t.PC, t.Line);
    while (pc <= targetPC && line != targetLine && len(b) > 0) {
        var code = b[0];
        b = b[1..];
        switch (ᐧ) {
        case {} when code is 0: {
            if (len(b) < 4) {
                b = b[0..0];
                break;
            }
            var val = binary.BigEndian.Uint32(b);
            b = b[4..];
            line += ((nint)val);
            break;
        }
        case {} when code is <= 64: {
            line += ((nint)code);
            break;
        }
        case {} when code is <= 128: {
            line -= ((nint)(code - 64));
            break;
        }
        default: {
            pc += oldQuantum * ((uint64)(code - 128));
            continue;
            break;
        }}

        pc += oldQuantum;
    }
    return (b, pc, line);
}

[GoRecv] internal static ж<LineTable> Δslice(this ref LineTable t, uint64 pc) {
    (data, pc, line) = t.parse(pc, -1);
    return Ꮡ(new LineTable(Data: data, PC: pc, Line: line));
}

// PCToLine returns the line number for the given program counter.
//
// Deprecated: Use Table's PCToLine method instead.
[GoRecv] public static nint PCToLine(this ref LineTable t, uint64 pc) {
    if (t.isGo12()) {
        return t.go12PCToLine(pc);
    }
    var (_, _, line) = t.parse(pc, -1);
    return line;
}

// LineToPC returns the program counter for the given line number,
// considering only program counters before maxpc.
//
// Deprecated: Use Table's LineToPC method instead.
[GoRecv] public static uint64 LineToPC(this ref LineTable t, nint line, uint64 maxpc) {
    if (t.isGo12()) {
        return 0;
    }
    var (_, pc, line1) = t.parse(maxpc, line);
    if (line1 != line) {
        return 0;
    }
    // Subtract quantum from PC to account for post-line increment
    return pc - oldQuantum;
}

// NewLineTable returns a new PC/line table
// corresponding to the encoded data.
// Text must be the start address of the
// corresponding text segment, with the exact
// value stored in the 'runtime.text' symbol.
// This value may differ from the start
// address of the text segment if
// binary was built with cgo enabled.
public static ж<LineTable> NewLineTable(slice<byte> data, uint64 text) {
    return Ꮡ(new LineTable(Data: data, PC: text, Line: 0, funcNames: new map<uint32, @string>(), strings: new map<uint32, @string>()));
}

// Go 1.2 symbol table format.
// See golang.org/s/go12symtab.
//
// A general note about the methods here: rather than try to avoid
// index out of bounds errors, we trust Go to detect them, and then
// we recover from the panics and treat them as indicative of a malformed
// or incomplete table.
//
// The methods called by symtab.go, which begin with "go12" prefixes,
// are expected to have that recovery logic.

// isGo12 reports whether this is a Go 1.2 (or later) symbol table.
[GoRecv] internal static bool isGo12(this ref LineTable t) {
    t.parsePclnTab();
    return t.version >= ver12;
}

internal static readonly UntypedInt go12magic = /* 0xfffffffb */ 4294967291;
internal static readonly UntypedInt go116magic = /* 0xfffffffa */ 4294967290;
internal static readonly UntypedInt go118magic = /* 0xfffffff0 */ 4294967280;
internal static readonly UntypedInt go120magic = /* 0xfffffff1 */ 4294967281;

// uintptr returns the pointer-sized value encoded at b.
// The pointer size is dictated by the table being read.
[GoRecv] internal static uint64 uintptr(this ref LineTable t, slice<byte> b) {
    if (t.ptrsize == 4) {
        return ((uint64)t.binary.Uint32(b));
    }
    return t.binary.Uint64(b);
}

// parsePclnTab parses the pclntab, setting the version.
[GoRecv] internal static void parsePclnTab(this ref LineTable t) => func((defer, recover) => {
    t.mu.Lock();
    defer(t.mu.Unlock);
    if (t.version != verUnknown) {
        return;
    }
    // Note that during this function, setting the version is the last thing we do.
    // If we set the version too early, and parsing failed (likely as a panic on
    // slice lookups), we'd have a mistaken version.
    //
    // Error paths through this code will default the version to 1.1.
    t.version = ver11;
    if (!disableRecover) {
        defer(() => {
            // If we panic parsing, assume it's a Go 1.1 pclntab.
            recover();
        });
    }
    // Check header: 4-byte magic, two zeros, pc quantum, pointer size.
    if (len(t.Data) < 16 || t.Data[4] != 0 || t.Data[5] != 0 || (t.Data[6] != 1 && t.Data[6] != 2 && t.Data[6] != 4) || (t.Data[7] != 4 && t.Data[7] != 8)) {
        // pc quantum
        // pointer size
        return;
    }
    version possibleVersion = default!;
    var leMagic = binary.LittleEndian.Uint32(t.Data);
    var beMagic = binary.BigEndian.Uint32(t.Data);
    switch (ᐧ) {
    case {} when leMagic is go12magic: {
        (t.binary, possibleVersion) = (binary.LittleEndian, ver12);
        break;
    }
    case {} when beMagic is go12magic: {
        (t.binary, possibleVersion) = (binary.BigEndian, ver12);
        break;
    }
    case {} when leMagic is go116magic: {
        (t.binary, possibleVersion) = (binary.LittleEndian, ver116);
        break;
    }
    case {} when beMagic is go116magic: {
        (t.binary, possibleVersion) = (binary.BigEndian, ver116);
        break;
    }
    case {} when leMagic is go118magic: {
        (t.binary, possibleVersion) = (binary.LittleEndian, ver118);
        break;
    }
    case {} when beMagic is go118magic: {
        (t.binary, possibleVersion) = (binary.BigEndian, ver118);
        break;
    }
    case {} when leMagic is go120magic: {
        (t.binary, possibleVersion) = (binary.LittleEndian, ver120);
        break;
    }
    case {} when beMagic is go120magic: {
        (t.binary, possibleVersion) = (binary.BigEndian, ver120);
        break;
    }
    default: {
        return;
    }}

    t.version = possibleVersion;
    // quantum and ptrSize are the same between 1.2, 1.16, and 1.18
    t.quantum = ((uint32)t.Data[6]);
    t.ptrsize = ((uint32)t.Data[7]);
    var offset = (uint32 word) => t.uintptr(t.Data[(int)(8 + word * t.ptrsize)..]);
    var data = 
    var offsetʗ1 = offset;
    (uint32 word) => t.Data[(int)(offsetʗ1(word))..];
    var exprᴛ1 = possibleVersion;
    if (exprᴛ1 == ver118 || exprᴛ1 == ver120) {
        t.nfunctab = ((uint32)offset(0));
        t.nfiletab = ((uint32)offset(1));
        t.textStart = t.PC;
        t.funcnametab = data(3);
        t.cutab = data(4);
        t.filetab = data(5);
        t.pctab = data(6);
        t.funcdata = data(7);
        t.functab = data(7);
        nint functabsize = (((nint)t.nfunctab) * 2 + 1) * t.functabFieldSize();
        t.functab = t.functab[..(int)(functabsize)];
    }
    else if (exprᴛ1 == ver116) {
        t.nfunctab = ((uint32)offset(0));
        t.nfiletab = ((uint32)offset(1));
        t.funcnametab = data(2);
        t.cutab = data(3);
        t.filetab = data(4);
        t.pctab = data(5);
        t.funcdata = data(6);
        t.functab = data(6);
        nint functabsize = (((nint)t.nfunctab) * 2 + 1) * t.functabFieldSize();
        t.functab = t.functab[..(int)(functabsize)];
    }
    else if (exprᴛ1 == ver12) {
        t.nfunctab = ((uint32)t.uintptr(t.Data[8..]));
        t.funcdata = t.Data;
        t.funcnametab = t.Data;
        t.functab = t.Data[(int)(8 + t.ptrsize)..];
        t.pctab = t.Data;
        nint functabsize = (((nint)t.nfunctab) * 2 + 1) * t.functabFieldSize();
        var fileoff = t.binary.Uint32(t.functab[(int)(functabsize)..]);
        t.functab = t.functab[..(int)(functabsize)];
        t.filetab = t.Data[(int)(fileoff)..];
        t.nfiletab = t.binary.Uint32(t.filetab);
        t.filetab = t.filetab[..(int)(t.nfiletab * 4)];
    }
    else { /* default: */
        throw panic("unreachable");
    }

});

// use the start PC instead of reading from the table, which may be unrelocated

// go12Funcs returns a slice of Funcs derived from the Go 1.2+ pcln table.
[GoRecv] internal static slice<Func> go12Funcs(this ref LineTable t) => func((defer, recover) => {
    // Assume it is malformed and return nil on error.
    if (!disableRecover) {
        defer(() => {
            recover();
        });
    }
    var ft = t.funcTab();
    var funcs = new slice<Func>(ft.Count());
    var syms = new slice<Sym>(len(funcs));
    foreach (var (i, _) in funcs) {
        var f = Ꮡ(funcs, i);
        f.val.Entry = ft.pc(i);
        f.val.End = ft.pc(i + 1);
        var info = t.funcData(((uint32)i));
        f.val.LineTable = t;
        f.val.FrameSize = ((nint)info.deferreturn());
        syms[i] = new Sym(
            Value: (~f).Entry,
            Type: (rune)'T',
            Name: t.funcName(info.nameOff()),
            ΔGoType: 0,
            Func: f,
            goVersion: t.version
        );
        f.val.Sym = Ꮡ(syms, i);
    }
    return funcs;
});

// findFunc returns the funcData corresponding to the given program counter.
[GoRecv] internal static ΔfuncData findFunc(this ref LineTable t, uint64 pc) {
    ref var ft = ref heap<ΔfuncTab>(out var Ꮡft);
    ft = t.funcTab();
    if (pc < ft.pc(0) || pc >= ft.pc(ft.Count())) {
        return new ΔfuncData(nil);
    }
    nint idx = sort.Search(((nint)t.nfunctab), 
    var ftʗ1 = ft;
    (nint i) => ftʗ1.pc(i) > pc);
    idx--;
    return t.funcData(((uint32)idx));
}

// readvarint reads, removes, and returns a varint from *pp.
[GoRecv] public static uint32 readvarint(this ref LineTable t, ж<slice<byte>> Ꮡpp) {
    ref var pp = ref Ꮡpp.val;

    uint32 v = default!;
    uint32 shift = default!;
    var p = pp;
    for (shift = 0; ᐧ ; shift += 7) {
        var b = p[0];
        p = p[1..];
        v |= (uint32)(((uint32)(((uint32)b) & 127)) << (int)(shift));
        if ((byte)(b & 128) == 0) {
            break;
        }
    }
    pp = p;
    return v;
}

// funcName returns the name of the function found at off.
[GoRecv] internal static @string funcName(this ref LineTable t, uint32 off) {
    {
        @string sΔ1 = t.funcNames[off];
        var ok = t.funcNames[off]; if (ok) {
            return sΔ1;
        }
    }
    nint i = bytes.IndexByte(t.funcnametab[(int)(off)..], 0);
    @string s = ((@string)(t.funcnametab[(int)(off)..(int)(off + ((uint32)i))]));
    t.funcNames[off] = s;
    return s;
}

// stringFrom returns a Go string found at off from a position.
[GoRecv] internal static @string stringFrom(this ref LineTable t, slice<byte> arr, uint32 off) {
    {
        @string sΔ1 = t.strings[off];
        var ok = t.strings[off]; if (ok) {
            return sΔ1;
        }
    }
    nint i = bytes.IndexByte(arr[(int)(off)..], 0);
    @string s = ((@string)(arr[(int)(off)..(int)(off + ((uint32)i))]));
    t.strings[off] = s;
    return s;
}

// string returns a Go string found at off.
[GoRecv] internal static @string @string(this ref LineTable t, uint32 off) {
    return t.stringFrom(t.funcdata, off);
}

// functabFieldSize returns the size in bytes of a single functab field.
[GoRecv] internal static nint functabFieldSize(this ref LineTable t) {
    if (t.version >= ver118) {
        return 4;
    }
    return ((nint)t.ptrsize);
}

// funcTab returns t's funcTab.
[GoRecv] internal static ΔfuncTab funcTab(this ref LineTable t) {
    return new ΔfuncTab(LineTable: t, sz: t.functabFieldSize());
}

// funcTab is memory corresponding to a slice of functab structs, followed by an invalid PC.
// A functab struct is a PC and a func offset.
[GoType] partial struct ΔfuncTab {
    public partial ref ж<LineTable> LineTable { get; }
    internal nint sz; // cached result of t.functabFieldSize
}

// Count returns the number of func entries in f.
public static nint Count(this ΔfuncTab f) {
    return ((nint)f.nfunctab);
}

// pc returns the PC of the i'th func in f.
internal static uint64 pc(this ΔfuncTab f, nint i) {
    var u = f.@uint(f.functab[(int)(2 * i * f.sz)..]);
    if (f.version >= ver118) {
        u += f.textStart;
    }
    return u;
}

// funcOff returns the funcdata offset of the i'th func in f.
internal static uint64 funcOff(this ΔfuncTab f, nint i) {
    return f.@uint(f.functab[(int)((2 * i + 1) * f.sz)..]);
}

// uint returns the uint stored at b.
internal static uint64 @uint(this ΔfuncTab f, slice<byte> b) {
    if (f.sz == 4) {
        return ((uint64)f.binary.Uint32(b));
    }
    return f.binary.Uint64(b);
}

// funcData is memory corresponding to an _func struct.
[GoType] partial struct ΔfuncData {
    internal ж<LineTable> t; // LineTable this data is a part of
    internal slice<byte> data; // raw memory for the function
}

// funcData returns the ith funcData in t.functab.
[GoRecv] internal static ΔfuncData funcData(this ref LineTable t, uint32 i) {
    var data = t.funcdata[(int)(t.funcTab().funcOff(((nint)i)))..];
    return new ΔfuncData(t: t, data: data);
}

// IsZero reports whether f is the zero value.
public static bool IsZero(this ΔfuncData f) {
    return f.t == nil && f.data == default!;
}

// entryPC returns the func's entry PC.
[GoRecv] internal static uint64 entryPC(this ref ΔfuncData f) {
    // In Go 1.18, the first field of _func changed
    // from a uintptr entry PC to a uint32 entry offset.
    if (f.t.version >= ver118) {
        // TODO: support multiple text sections.
        // See runtime/symtab.go:(*moduledata).textAddr.
        return ((uint64)f.t.binary.Uint32(f.data)) + f.t.textStart;
    }
    return f.t.uintptr(f.data);
}

internal static uint32 nameOff(this ΔfuncData f) {
    return f.field(1);
}

internal static uint32 deferreturn(this ΔfuncData f) {
    return f.field(3);
}

internal static uint32 pcfile(this ΔfuncData f) {
    return f.field(5);
}

internal static uint32 pcln(this ΔfuncData f) {
    return f.field(6);
}

internal static uint32 cuOffset(this ΔfuncData f) {
    return f.field(8);
}

// field returns the nth field of the _func struct.
// It panics if n == 0 or n > 9; for n == 0, call f.entryPC.
// Most callers should use a named field accessor (just above).
internal static uint32 field(this ΔfuncData f, uint32 n) {
    if (n == 0 || n > 9) {
        throw panic("bad funcdata field");
    }
    // In Go 1.18, the first field of _func changed
    // from a uintptr entry PC to a uint32 entry offset.
    var sz0 = f.t.ptrsize;
    if (f.t.version >= ver118) {
        sz0 = 4;
    }
    var off = sz0 + (n - 1) * 4;
    // subsequent fields are 4 bytes each
    var data = f.data[(int)(off)..];
    return f.t.binary.Uint32(data);
}

// step advances to the next pc, value pair in the encoded table.
[GoRecv] public static bool step(this ref LineTable t, ж<slice<byte>> Ꮡp, ж<uint64> Ꮡpc, ж<int32> Ꮡval, bool first) {
    ref var p = ref Ꮡp.val;
    ref var pc = ref Ꮡpc.val;
    ref var val = ref Ꮡval.val;

    var uvdelta = t.readvarint(Ꮡp);
    if (uvdelta == 0 && !first) {
        return false;
    }
    if ((uint32)(uvdelta & 1) != 0){
        uvdelta = ^(uvdelta >> (int)(1));
    } else {
        uvdelta >>= (UntypedInt)(1);
    }
    var vdelta = ((int32)uvdelta);
    var pcdelta = t.readvarint(Ꮡp) * t.quantum;
    pc += ((uint64)pcdelta);
    val += vdelta;
    return true;
}

// pcvalue reports the value associated with the target pc.
// off is the offset to the beginning of the pc-value table,
// and entry is the start PC for the corresponding function.
[GoRecv] internal static int32 pcvalue(this ref LineTable t, uint32 off, uint64 entry, uint64 targetpc) {
    var p = t.pctab[(int)(off)..];
    ref var val = ref heap<int32>(out var Ꮡval);
    val = ((int32)(-1));
    ref var pc = ref heap<uint64>(out var Ꮡpc);
    pc = entry;
    while (t.step(Ꮡ(p), Ꮡpc, Ꮡval, pc == entry)) {
        if (targetpc < pc) {
            return val;
        }
    }
    return -1;
}

// findFileLine scans one function in the binary looking for a
// program counter in the given file on the given line.
// It does so by running the pc-value tables mapping program counter
// to file number. Since most functions come from a single file, these
// are usually short and quick to scan. If a file match is found, then the
// code goes to the expense of looking for a simultaneous line number match.
[GoRecv] internal static uint64 findFileLine(this ref LineTable t, uint64 entry, uint32 filetab, uint32 linetab, int32 filenum, int32 line, slice<byte> cutab) {
    if (filetab == 0 || linetab == 0) {
        return 0;
    }
    var fp = t.pctab[(int)(filetab)..];
    var fl = t.pctab[(int)(linetab)..];
    ref var fileVal = ref heap<int32>(out var ᏑfileVal);
    fileVal = ((int32)(-1));
    ref var filePC = ref heap<uint64>(out var ᏑfilePC);
    filePC = entry;
    ref var lineVal = ref heap<int32>(out var ᏑlineVal);
    lineVal = ((int32)(-1));
    ref var linePC = ref heap<uint64>(out var ᏑlinePC);
    linePC = entry;
    var fileStartPC = filePC;
    while (t.step(Ꮡ(fp), ᏑfilePC, ᏑfileVal, filePC == entry)) {
        var fileIndex = fileVal;
        if (t.version == ver116 || t.version == ver118 || t.version == ver120) {
            fileIndex = ((int32)t.binary.Uint32(cutab[(int)(fileVal * 4)..]));
        }
        if (fileIndex == filenum && fileStartPC < filePC) {
            // fileIndex is in effect starting at fileStartPC up to
            // but not including filePC, and it's the file we want.
            // Run the PC table looking for a matching line number
            // or until we reach filePC.
            var lineStartPC = linePC;
            while (linePC < filePC && t.step(Ꮡ(fl), ᏑlinePC, ᏑlineVal, linePC == entry)) {
                // lineVal is in effect until linePC, and lineStartPC < filePC.
                if (lineVal == line) {
                    if (fileStartPC <= lineStartPC) {
                        return lineStartPC;
                    }
                    if (fileStartPC < linePC) {
                        return fileStartPC;
                    }
                }
                lineStartPC = linePC;
            }
        }
        fileStartPC = filePC;
    }
    return 0;
}

// go12PCToLine maps program counter to line number for the Go 1.2+ pcln table.
[GoRecv] internal static nint /*line*/ go12PCToLine(this ref LineTable t, uint64 pc) => func((defer, recover) => {
    nint line = default!;

    defer(() => {
        if (!disableRecover && recover() != default!) {
            line = -1;
        }
    });
    var f = t.findFunc(pc);
    if (f.IsZero()) {
        return -1;
    }
    var entry = f.entryPC();
    var linetab = f.pcln();
    return ((nint)t.pcvalue(linetab, entry, pc));
});

// go12PCToFile maps program counter to file name for the Go 1.2+ pcln table.
[GoRecv] internal static @string /*file*/ go12PCToFile(this ref LineTable t, uint64 pc) => func((defer, recover) => {
    @string file = default!;

    defer(() => {
        if (!disableRecover && recover() != default!) {
            file = ""u8;
        }
    });
    var f = t.findFunc(pc);
    if (f.IsZero()) {
        return ""u8;
    }
    var entry = f.entryPC();
    var filetab = f.pcfile();
    var fno = t.pcvalue(filetab, entry, pc);
    if (t.version == ver12) {
        if (fno <= 0) {
            return ""u8;
        }
        return t.@string(t.binary.Uint32(t.filetab[(int)(4 * fno)..]));
    }
    // Go ≥ 1.16
    if (fno < 0) {
        // 0 is valid for ≥ 1.16
        return ""u8;
    }
    var cuoff = f.cuOffset();
    {
        var fnoff = t.binary.Uint32(t.cutab[(int)((cuoff + ((uint32)fno)) * 4)..]); if (fnoff != ^((uint32)0)) {
            return t.stringFrom(t.filetab, fnoff);
        }
    }
    return ""u8;
});

// go12LineToPC maps a (file, line) pair to a program counter for the Go 1.2+ pcln table.
[GoRecv] internal static uint64 /*pc*/ go12LineToPC(this ref LineTable t, @string file, nint line) => func((defer, recover) => {
    uint64 pc = default!;

    defer(() => {
        if (!disableRecover && recover() != default!) {
            pc = 0;
        }
    });
    t.initFileMap();
    var (filenum, ok) = t.fileMap[file];
    if (!ok) {
        return 0;
    }
    // Scan all functions.
    // If this turns out to be a bottleneck, we could build a map[int32][]int32
    // mapping file number to a list of functions with code from that file.
    slice<byte> cutab = default!;
    for (var i = ((uint32)0); i < t.nfunctab; i++) {
        var f = t.funcData(i);
        var entry = f.entryPC();
        var filetab = f.pcfile();
        var linetab = f.pcln();
        if (t.version == ver116 || t.version == ver118 || t.version == ver120) {
            if (f.cuOffset() == ^((uint32)0)) {
                // skip functions without compilation unit (not real function, or linker generated)
                continue;
            }
            cutab = t.cutab[(int)(f.cuOffset() * 4)..];
        }
        var pcΔ1 = t.findFileLine(entry, filetab, linetab, ((int32)filenum), ((int32)line), cutab);
        if (pcΔ1 != 0) {
            return pcΔ1;
        }
    }
    return 0;
});

// initFileMap initializes the map from file name to file number.
[GoRecv] internal static void initFileMap(this ref LineTable t) => func((defer, _) => {
    t.mu.Lock();
    defer(t.mu.Unlock);
    if (t.fileMap != default!) {
        return;
    }
    var m = new map<@string, uint32>();
    if (t.version == ver12){
        for (var i = ((uint32)1); i < t.nfiletab; i++) {
            @string s = t.@string(t.binary.Uint32(t.filetab[(int)(4 * i)..]));
            m[s] = i;
        }
    } else {
        uint32 pos = default!;
        for (var i = ((uint32)0); i < t.nfiletab; i++) {
            @string s = t.stringFrom(t.filetab, pos);
            m[s] = pos;
            pos += ((uint32)(len(s) + 1));
        }
    }
    t.fileMap = m;
});

// go12MapFiles adds to m a key for every file in the Go 1.2 LineTable.
// Every key maps to obj. That's not a very interesting map, but it provides
// a way for callers to obtain the list of files in the program.
[GoRecv] public static void go12MapFiles(this ref LineTable t, map<@string, ж<Obj>> m, ж<Obj> Ꮡobj) => func((defer, recover) => {
    ref var obj = ref Ꮡobj.val;

    if (!disableRecover) {
        defer(() => {
            recover();
        });
    }
    t.initFileMap();
    foreach (var (file, _) in t.fileMap) {
        m[file] = obj;
    }
});

// disableRecover causes this package not to swallow panics.
// This is useful when making changes.
internal const bool disableRecover = false;

} // end gosym_package
