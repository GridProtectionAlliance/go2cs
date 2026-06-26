// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gosym implements access to the Go symbol
// and line number tables embedded in Go binaries generated
// by the gc compilers.
namespace go.debug;

using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;
using encoding;

partial class gosym_package {

/*
 * Symbols
 */

// A Sym represents a single symbol table entry.
[GoType] partial struct Sym {
    public uint64 Value;
    public byte Type;
    public @string Name;
    public uint64 ΔGoType;
    // If this symbol is a function symbol, the corresponding Func
    public ж<Func> Func;
    internal version goVersion;
}

// Static reports whether this symbol is static (not visible outside its file).
[GoRecv] public static bool Static(this ref Sym s) {
    return s.Type >= (rune)'a';
}

// nameWithoutInst returns s.Name if s.Name has no brackets (does not reference an
// instantiated type, function, or method). If s.Name contains brackets, then it
// returns s.Name with all the contents between (and including) the outermost left
// and right bracket removed. This is useful to ignore any extra slashes or dots
// inside the brackets from the string searches below, where needed.
[GoRecv] internal static @string nameWithoutInst(this ref Sym s) {
    nint start = strings.Index(s.Name, "["u8);
    if (start < 0) {
        return s.Name;
    }
    nint end = strings.LastIndex(s.Name, "]"u8);
    if (end < 0) {
        // Malformed name, should contain closing bracket too.
        return s.Name;
    }
    return s.Name[0..(int)(start)] + s.Name[(int)(end + 1)..];
}

// PackageName returns the package part of the symbol name,
// or the empty string if there is none.
[GoRecv] public static @string PackageName(this ref Sym s) {
    @string name = s.nameWithoutInst();
    // Since go1.20, a prefix of "type:" and "go:" is a compiler-generated symbol,
    // they do not belong to any package.
    //
    // See cmd/compile/internal/base/link.go:ReservedImports variable.
    if (s.goVersion >= ver120 && (strings.HasPrefix(name, "go:"u8) || strings.HasPrefix(name, "type:"u8))) {
        return ""u8;
    }
    // For go1.18 and below, the prefix are "type." and "go." instead.
    if (s.goVersion <= ver118 && (strings.HasPrefix(name, "go."u8) || strings.HasPrefix(name, "type."u8))) {
        return ""u8;
    }
    nint pathend = strings.LastIndex(name, "/"u8);
    if (pathend < 0) {
        pathend = 0;
    }
    {
        nint i = strings.Index(name[(int)(pathend)..], "."u8); if (i != -1) {
            return name[..(int)(pathend + i)];
        }
    }
    return ""u8;
}

// ReceiverName returns the receiver type name of this symbol,
// or the empty string if there is none.  A receiver name is only detected in
// the case that s.Name is fully-specified with a package name.
[GoRecv] public static @string ReceiverName(this ref Sym s) {
    @string name = s.nameWithoutInst();
    // If we find a slash in name, it should precede any bracketed expression
    // that was removed, so pathend will apply correctly to name and s.Name.
    nint pathend = strings.LastIndex(name, "/"u8);
    if (pathend < 0) {
        pathend = 0;
    }
    // Find the first dot after pathend (or from the beginning, if there was
    // no slash in name).
    nint l = strings.Index(name[(int)(pathend)..], "."u8);
    // Find the last dot after pathend (or the beginning).
    nint r = strings.LastIndex(name[(int)(pathend)..], "."u8);
    if (l == -1 || r == -1 || l == r) {
        // There is no receiver if we didn't find two distinct dots after pathend.
        return ""u8;
    }
    // Given there is a trailing '.' that is in name, find it now in s.Name.
    // pathend+l should apply to s.Name, because it should be the dot in the
    // package name.
    r = strings.LastIndex(s.Name[(int)(pathend)..], "."u8);
    return s.Name[(int)(pathend + l + 1)..(int)(pathend + r)];
}

// BaseName returns the symbol name without the package or receiver name.
[GoRecv] public static @string BaseName(this ref Sym s) {
    @string name = s.nameWithoutInst();
    {
        nint i = strings.LastIndex(name, "."u8); if (i != -1) {
            if (s.Name != name) {
                nint brack = strings.Index(s.Name, "["u8);
                if (i > brack) {
                    // BaseName is a method name after the brackets, so
                    // recalculate for s.Name. Otherwise, i applies
                    // correctly to s.Name, since it is before the
                    // brackets.
                    i = strings.LastIndex(s.Name, "."u8);
                }
            }
            return s.Name[(int)(i + 1)..];
        }
    }
    return s.Name;
}

// A Func collects information about a single function.
[GoType] partial struct Func {
    public uint64 Entry;
    public partial ref ж<Sym> Sym { get; }
    public uint64 End;
    public slice<ж<Sym>> Params; // nil for Go 1.3 and later binaries
    public slice<ж<Sym>> Locals; // nil for Go 1.3 and later binaries
    public nint FrameSize;
    public ж<LineTable> LineTable;
    public ж<Obj> Obj;
}

// An Obj represents a collection of functions in a symbol table.
//
// The exact method of division of a binary into separate Objs is an internal detail
// of the symbol table format.
//
// In early versions of Go each source file became a different Obj.
//
// In Go 1 and Go 1.1, each package produced one Obj for all Go sources
// and one Obj per C source file.
//
// In Go 1.2, there is a single Obj for the entire program.
[GoType] partial struct Obj {
    // Funcs is a list of functions in the Obj.
    public slice<Func> Funcs;
    // In Go 1.1 and earlier, Paths is a list of symbols corresponding
    // to the source file names that produced the Obj.
    // In Go 1.2, Paths is nil.
    // Use the keys of Table.Files to obtain a list of source files.
    public slice<Sym> Paths; // meta
}

/*
 * Symbol tables
 */

// Table represents a Go symbol table. It stores all of the
// symbols decoded from the program and provides methods to translate
// between symbols, names, and addresses.
[GoType] partial struct Table {
    public slice<Sym> Syms; // nil for Go 1.3 and later binaries
    public slice<Func> Funcs;
    public map<@string, ж<Obj>> Files; // for Go 1.2 and later all files map to one Obj
    public slice<Obj> Objs;      // for Go 1.2 and later only one Obj in slice
    internal ж<LineTable> go12line; // Go 1.2 line number table
}

[GoType] partial struct sym {
    internal uint64 value;
    internal uint64 gotype;
    internal byte typ;
    internal slice<byte> name;
}

internal static slice<byte> littleEndianSymtab = new byte[]{253, 255, 255, 255, 0, 0, 0}.slice();
internal static slice<byte> bigEndianSymtab = new byte[]{255, 255, 255, 253, 0, 0, 0}.slice();
internal static slice<byte> oldLittleEndianSymtab = new byte[]{254, 255, 255, 255, 0, 0}.slice();

internal static error walksymtab(slice<byte> data, Func<sym, error> fn) {
    if (len(data) == 0) {
        // missing symtab is okay
        return default!;
    }
    binary.ByteOrder order = binary.BigEndian;
    var newTable = false;
    switch (ᐧ) {
    case {} when bytes.HasPrefix(data, oldLittleEndianSymtab): {
        data = data[6..];
        order = binary.LittleEndian;
        break;
    }
    case {} when bytes.HasPrefix(data, // Same as Go 1.0, but little endian.
 // Format was used during interim development between Go 1.0 and Go 1.1.
 // Should not be widespread, but easy to support.
 bigEndianSymtab): {
        newTable = true;
        break;
    }
    case {} when bytes.HasPrefix(data, littleEndianSymtab): {
        newTable = true;
        order = binary.LittleEndian;
        break;
    }}

    ref var ptrsz = ref heap(new nint(), out var Ꮡptrsz);
    if (newTable) {
        if (len(data) < 8) {
            return new DecodingError(len(data), "unexpected EOF", default!);
        }
        ptrsz = ((nint)data[7]);
        if (ptrsz != 4 && ptrsz != 8) {
            return new DecodingError(7, "invalid pointer size", ptrsz);
        }
        data = data[8..];
    }
    sym s = default!;
    var p = data;
    while (len(p) >= 4) {
        ref var typ = ref heap(new byte(), out var Ꮡtyp);
        if (newTable){
            // Symbol type, value, Go type.
            typ = (byte)(p[0] & 63);
            var wideValue = (byte)(p[0] & 64) != 0;
            var goType = (byte)(p[0] & 128) != 0;
            if (typ < 26){
                typ += (rune)'A';
            } else {
                typ += (rune)'a' - 26;
            }
            s.typ = typ;
            p = p[1..];
            if (wideValue){
                if (len(p) < ptrsz) {
                    return new DecodingError(len(data), "unexpected EOF", default!);
                }
                // fixed-width value
                if (ptrsz == 8){
                    s.value = order.Uint64(p[0..8]);
                    p = p[8..];
                } else {
                    s.value = ((uint64)order.Uint32(p[0..4]));
                    p = p[4..];
                }
            } else {
                // varint value
                s.value = 0;
                nuint shift = ((nuint)0);
                while (len(p) > 0 && (byte)(p[0] & 128) != 0) {
                    s.value |= (uint64)(((uint64)((byte)(p[0] & 127))) << (int)(shift));
                    shift += 7;
                    p = p[1..];
                }
                if (len(p) == 0) {
                    return new DecodingError(len(data), "unexpected EOF", default!);
                }
                s.value |= (uint64)(((uint64)p[0]) << (int)(shift));
                p = p[1..];
            }
            if (goType) {
                if (len(p) < ptrsz) {
                    return new DecodingError(len(data), "unexpected EOF", default!);
                }
                // fixed-width go type
                if (ptrsz == 8){
                    s.gotype = order.Uint64(p[0..8]);
                    p = p[8..];
                } else {
                    s.gotype = ((uint64)order.Uint32(p[0..4]));
                    p = p[4..];
                }
            }
        } else {
            // Value, symbol type.
            s.value = ((uint64)order.Uint32(p[0..4]));
            if (len(p) < 5) {
                return new DecodingError(len(data), "unexpected EOF", default!);
            }
            typ = p[4];
            if ((byte)(typ & 128) == 0) {
                return new DecodingError(len(data) - len(p) + 4, "bad symbol type", typ);
            }
            typ &= ~(byte)(128);
            s.typ = typ;
            p = p[5..];
        }
        // Name.
        nint i = default!;
        nint nnul = default!;
        for (i = 0; i < len(p); i++) {
            if (p[i] == 0) {
                nnul = 1;
                break;
            }
        }
        switch (typ) {
        case (rune)'z' or (rune)'Z': {
            p = p[(int)(i + nnul)..];
            for (i = 0; i + 2 <= len(p); i += 2) {
                if (p[i] == 0 && p[i + 1] == 0) {
                    nnul = 2;
                    break;
                }
            }
            break;
        }}

        if (len(p) < i + nnul) {
            return new DecodingError(len(data), "unexpected EOF", default!);
        }
        s.name = p[0..(int)(i)];
        i += nnul;
        p = p[(int)(i)..];
        if (!newTable) {
            if (len(p) < 4) {
                return new DecodingError(len(data), "unexpected EOF", default!);
            }
            // Go type.
            s.gotype = ((uint64)order.Uint32(p[..4]));
            p = p[4..];
        }
        fn(s);
    }
    return default!;
}

// NewTable decodes the Go symbol table (the ".gosymtab" section in ELF),
// returning an in-memory representation.
// Starting with Go 1.3, the Go symbol table no longer includes symbol data.
public static (ж<Table>, error) NewTable(slice<byte> symtab, ж<LineTable> Ꮡpcln) {
    ref var pcln = ref Ꮡpcln.val;

    nint n = default!;
    var err = walksymtab(symtab, (sym s) => {
        n++;
        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    ref var t = ref heap(new Table(), out var Ꮡt);
    if (pcln.isGo12()) {
        t.go12line = pcln;
    }
    var fname = new map<uint16, @string>();
    t.Syms = new slice<Sym>(0, n);
    nint nf = 0;
    nint nz = 0;
    var lasttyp = ((uint8)0);
    err = walksymtab(symtab, 
    var fnameʗ1 = fname;
    var tʗ1 = t;
    (sym s) => {
        nint nΔ1 = len(tʗ1.Syms);
        tʗ1.Syms = tʗ1.Syms[0..(int)(nΔ1 + 1)];
        var ts = Ꮡ(tʗ1.Syms, nΔ1);
        ts.val.Type = s.typ;
        ts.val.Value = s.value;
        ts.val.ΔGoType = s.gotype;
        ts.val.goVersion = pcln.version;
        switch (s.typ) {
        default: {
            nint w = 0;
            var b = s.name;
            for (nint iΔ3 = 0; iΔ3 < len(b); iΔ3++) {
                // rewrite name to use . instead of · (c2 b7)
                if (b[iΔ3] == 194 && iΔ3 + 1 < len(b) && b[iΔ3 + 1] == 183) {
                    iΔ3++;
                    b[i] = (rune)'.';
                }
                b[w] = b[iΔ3];
                w++;
            }
            ts.val.Name = ((@string)(s.name[0..(int)(w)]));
            break;
        }
        case (rune)'z' or (rune)'Z': {
            if (lasttyp != (rune)'z' && lasttyp != (rune)'Z') {
                nz++;
            }
            for (nint iΔ4 = 0; iΔ4 < len(s.name);  += 2) {
                ref var eltIdx = ref heap<uint16>(out var ᏑeltIdx);
                eltIdx = binary.BigEndian.Uint16(s.name[(int)(iΔ4)..(int)(iΔ4 + 2)]);
                @string elt = fnameʗ1[eltIdx];
                var ok = fnameʗ1[eltIdx];
                if (!ok) {
                    return Ꮡ(new DecodingError(-1, "bad filename code", eltIdx));
                }
                {
                    nint nΔ3 = len((~ts).Name); if (nΔ3 > 0 && (~ts).Name[nΔ3 - 1] != (rune)'/') {
                        ts.val.Name += "/"u8;
                    }
                }
                ts.val.Name += elt;
            }
            break;
        }}

        switch (s.typ) {
        case (rune)'T' or (rune)'t' or (rune)'L' or (rune)'l': {
            nf++;
            break;
        }
        case (rune)'f': {
            fnameʗ1[((uint16)s.value)] = ts.val.Name;
            break;
        }}

        lasttyp = s.typ;
        return default!;
    });
    if (err != default!) {
        return (default!, err);
    }
    t.Funcs = new slice<Func>(0, nf);
    t.Files = new map<@string, ж<Obj>>();
    ж<Obj> obj = default!;
    if (t.go12line != nil){
        // Put all functions into one Obj.
        t.Objs = new slice<Obj>(1);
        obj = Ꮡ(t.Objs, 0);
        t.go12line.go12MapFiles(t.Files, obj);
    } else {
        t.Objs = new slice<Obj>(0, nz);
    }
    // Count text symbols and attach frame sizes, parameters, and
    // locals to them. Also, find object file boundaries.
    nint lastf = 0;
    for (nint i = 0; i < len(t.Syms); i++) {
        var sym = Ꮡ(t.Syms, i);
        switch ((~sym).Type) {
        case (rune)'Z' or (rune)'z': {
            if (t.go12line != nil) {
                // path symbol
                // Go 1.2 binaries have the file information elsewhere. Ignore.
                break;
            }
            if (obj != nil) {
                // Finish the current object
                obj.val.Funcs = t.Funcs[(int)(lastf)..];
            }
            lastf = len(t.Funcs);
            nint n = len(t.Objs);
            t.Objs = t.Objs[0..(int)(n + 1)];
            obj = Ꮡ(t.Objs, n);
// Start new object

            // Count & copy path symbols
            nint endΔ2 = default!;
            for (endΔ2 = i + 1; endΔ2 < len(t.Syms); endΔ2++) {
                {
                    var c = t.Syms[end].Type; if (c != (rune)'Z' && c != (rune)'z') {
                        break;
                    }
                }
            }
            obj.val.Paths = t.Syms[(int)(i)..(int)(endΔ2)];
            i = endΔ2 - 1;
            nint depth = 0;
            foreach (var (j, _) in (~obj).Paths) {
                // loop will i++
                // Record file names
                var s = Ꮡ((~obj).Paths, j);
                if ((~s).Name == ""u8){
                    depth--;
                } else {
                    if (depth == 0) {
                        t.Files[(~s).Name] = obj;
                    }
                    depth++;
                }
            }
            break;
        }
        case (rune)'T' or (rune)'t' or (rune)'L' or (rune)'l': {
            {
                nint nΔ8 = len(t.Funcs); if (nΔ8 > 0) {
                    // text symbol
                    t.Funcs[n - 1].End = sym.val.Value;
                }
            }
            if ((~sym).Name == "runtime.etext"u8 || (~sym).Name == "etext"u8) {
                continue;
            }
            // Count parameter and local (auto) syms
            nint np = default!;
            nint na = default!;
            nint end = default!;
countloop:
            for (end = i + 1; end < len(t.Syms); end++) {
                switch (t.Syms[end].Type) {
                case (rune)'T' or (rune)'t' or (rune)'L' or (rune)'l' or (rune)'Z' or (rune)'z': {
                    goto break_countloop;
                    break;
                }
                case (rune)'p': {
                    np++;
                    break;
                }
                case (rune)'a': {
                    na++;
                    break;
                }}

continue_countloop:;
            }
break_countloop:;
            nint n = len(t.Funcs);
            t.Funcs = t.Funcs[0..(int)(n + 1)];
            var fn = Ꮡ(t.Funcs, n);
            sym.val.Func = fn;
            fn.val.Params = new slice<ж<Sym>>(0, // Fill in the function symbol
 np);
            fn.val.Locals = new slice<ж<Sym>>(0, na);
            fn.val.Sym = sym;
            fn.val.Entry = sym.val.Value;
            fn.val.Obj = obj;
            if (t.go12line != nil){
                // All functions share the same line table.
                // It knows how to narrow down to a specific
                // function quickly.
                fn.val.LineTable = t.go12line;
            } else 
            if (pcln != nil) {
                fn.val.LineTable = pcln.Δslice((~fn).Entry);
                pcln = fn.val.LineTable;
            }
            for (nint j = i; j < end; j++) {
                var s = Ꮡ(t.Syms, j);
                switch ((~s).Type) {
                case (rune)'m': {
                    fn.val.FrameSize = ((nint)(~s).Value);
                    break;
                }
                case (rune)'p': {
                    nint nΔ10 = len((~fn).Params);
                    fn.val.Params = (~fn).Params[0..(int)(nΔ10 + 1)];
                    (~fn).Params[n] = s;
                    break;
                }
                case (rune)'a': {
                    nint nΔ11 = len((~fn).Locals);
                    fn.val.Locals = (~fn).Locals[0..(int)(nΔ11 + 1)];
                    (~fn).Locals[n] = s;
                    break;
                }}

            }
            i = end - 1;
            break;
        }}

    }
    // loop will i++
    if (t.go12line != nil && nf == 0) {
        t.Funcs = t.go12line.go12Funcs();
    }
    if (obj != nil) {
        obj.val.Funcs = t.Funcs[(int)(lastf)..];
    }
    return (Ꮡt, default!);
}

// PCToFunc returns the function containing the program counter pc,
// or nil if there is no such function.
[GoRecv] public static ж<Func> PCToFunc(this ref Table t, uint64 pc) {
    var funcs = t.Funcs;
    while (len(funcs) > 0) {
        nint m = len(funcs) / 2;
        var fn = Ꮡ(funcs, m);
        switch (ᐧ) {
        case {} when pc is < (~fn).Entry: {
            funcs = funcs[0..(int)(m)];
            break;
        }
        case {} when (~fn).Entry <= pc && pc < (~fn).End: {
            return fn;
        }
        default: {
            funcs = funcs[(int)(m + 1)..];
            break;
        }}

    }
    return default!;
}

// PCToLine looks up line number information for a program counter.
// If there is no information, it returns fn == nil.
[GoRecv] public static (@string file, nint line, ж<Func> fn) PCToLine(this ref Table t, uint64 pc) {
    @string file = default!;
    nint line = default!;
    ж<Func> fn = default!;

    {
        fn = t.PCToFunc(pc); if (fn == nil) {
            return (file, line, fn);
        }
    }
    if (t.go12line != nil){
        file = t.go12line.go12PCToFile(pc);
        line = t.go12line.go12PCToLine(pc);
    } else {
        (file, line) = (~fn).Obj.lineFromAline((~fn).LineTable.PCToLine(pc));
    }
    return (file, line, fn);
}

// LineToPC looks up the first program counter on the given line in
// the named file. It returns [UnknownFileError] or [UnknownLineError] if
// there is an error looking up this line.
[GoRecv] public static (uint64 pc, ж<Func> fn, error err) LineToPC(this ref Table t, @string file, nint line) {
    uint64 pc = default!;
    ж<Func> fn = default!;
    error err = default!;

    var obj = t.Files[file];
    var ok = t.Files[file];
    if (!ok) {
        return (0, default!, ((UnknownFileError)file));
    }
    if (t.go12line != nil) {
        var pcΔ1 = t.go12line.go12LineToPC(file, line);
        if (pcΔ1 == 0) {
            return (0, default!, new UnknownLineError(file, line));
        }
        return (pcΔ1, t.PCToFunc(pcΔ1), default!);
    }
    var (abs, err) = obj.alineFromLine(file, line);
    if (err != default!) {
        return (pc, fn, err);
    }
    foreach (var (i, _) in (~obj).Funcs) {
        var f = Ꮡ((~obj).Funcs, i);
        var pcΔ2 = (~f).LineTable.LineToPC(abs, (~f).End);
        if (pcΔ2 != 0) {
            return (pcΔ2, f, default!);
        }
    }
    return (0, default!, new UnknownLineError(file, line));
}

// LookupSym returns the text, data, or bss symbol with the given name,
// or nil if no such symbol is found.
[GoRecv] public static ж<Sym> LookupSym(this ref Table t, @string name) {
    // TODO(austin) Maybe make a map
    foreach (var (i, _) in t.Syms) {
        var s = Ꮡ(t.Syms[i]);
        switch ((~s).Type) {
        case (rune)'T' or (rune)'t' or (rune)'L' or (rune)'l' or (rune)'D' or (rune)'d' or (rune)'B' or (rune)'b': {
            if ((~s).Name == name) {
                return s;
            }
            break;
        }}

    }
    return default!;
}

// LookupFunc returns the text, data, or bss symbol with the given name,
// or nil if no such symbol is found.
[GoRecv] public static ж<Func> LookupFunc(this ref Table t, @string name) {
    foreach (var (i, _) in t.Funcs) {
        var f = Ꮡ(t.Funcs[i]);
        if ((~(~f).Sym).Name == name) {
            return f;
        }
    }
    return default!;
}

// SymByAddr returns the text, data, or bss symbol starting at the given address.
[GoRecv] public static ж<Sym> SymByAddr(this ref Table t, uint64 addr) {
    foreach (var (i, _) in t.Syms) {
        var s = Ꮡ(t.Syms[i]);
        switch ((~s).Type) {
        case (rune)'T' or (rune)'t' or (rune)'L' or (rune)'l' or (rune)'D' or (rune)'d' or (rune)'B' or (rune)'b': {
            if ((~s).Value == addr) {
                return s;
            }
            break;
        }}

    }
    return default!;
}

[GoType("dyn")] partial struct lineFromAline_stackEnt {
    internal @string path;
    internal nint start;
    internal nint offset;
    internal ж<lineFromAline_stackEnt> prev;
}

/*
 * Object files
 */
// This is legacy code for Go 1.1 and earlier, which used the
// Plan 9 format for pc-line tables. This code was never quite
// correct. It's probably very close, and it's usually correct, but
// we never quite found all the corner cases.
//
// Go 1.2 and later use a simpler format, documented at golang.org/s/go12symtab.
[GoRecv] internal static (@string, nint) lineFromAline(this ref Obj o, nint aline) {
    var noPath = Ꮡ(new stackEnt("", 0, 0, nil));
    var tos = noPath;
pathloop:
    foreach (var (_, s) in o.Paths) {
        ref var val = ref heap<nint>(out var Ꮡval);
        val = ((nint)s.Value);
        switch (ᐧ) {
        case {} when val is > aline: {
            goto break_pathloop;
            break;
        }
        case {} when val is 1: {
            tos = Ꮡ(new stackEnt( // Start a new stack
s.Name, val, 0, noPath));
            break;
        }
        case {} when s.Name == ""u8: {
            if (tos == noPath) {
                // Pop
                return ("<malformed symbol table>", 0);
            }
            (~tos).prev.val.offset += val - (~tos).start;
            tos = tos.val.prev;
            break;
        }
        default: {
            tos = Ꮡ(new stackEnt( // Push
s.Name, val, 0, tos));
            break;
        }}

    }
    if (tos == noPath) {
        return ("", 0);
    }
    return ((~tos).path, aline - (~tos).start - (~tos).offset + 1);
}

[GoRecv] internal static (nint, error) alineFromLine(this ref Obj o, @string path, nint line) {
    if (line < 1) {
        return (0, new UnknownLineError(path, line));
    }
    foreach (var (i, s) in o.Paths) {
        // Find this path
        if (s.Name != path) {
            continue;
        }
        // Find this line at this stack level
        nint depth = 0;
        nint incstart = default!;
        line += ((nint)s.Value);
pathloop:
        foreach (var (_, sΔ1) in o.Paths[(int)(i)..]) {
            nint val = ((nint)sΔ1.Value);
            switch (ᐧ) {
            case {} when depth == 1 && val >= line: {
                return (line - 1, default!);
            }
            case {} when sΔ1.Name == ""u8: {
                depth--;
                if (depth == 0){
                    goto break_pathloop;
                } else 
                if (depth == 1) {
                    line += val - incstart;
                }
                break;
            }
            default: {
                if (depth == 1) {
                    incstart = val;
                }
                depth++;
                break;
            }}

        }
        return (0, new UnknownLineError(path, line));
    }
    return (0, ((UnknownFileError)path));
}

[GoType("@string")] partial struct UnknownFileError;

/*
 * Errors
 */
public static @string Error(this UnknownFileError e) {
    return "unknown file: "u8 + ((@string)e);
}

// UnknownLineError represents a failure to map a line to a program
// counter, either because the line is beyond the bounds of the file
// or because there is no code on the given line.
[GoType] partial struct UnknownLineError {
    public @string File;
    public nint Line;
}

[GoRecv] public static @string Error(this ref UnknownLineError e) {
    return "no code at "u8 + e.File + ":"u8 + strconv.Itoa(e.Line);
}

// DecodingError represents an error during the decoding of
// the symbol table.
[GoType] partial struct DecodingError {
    internal nint off;
    internal @string msg;
    internal any val;
}

[GoRecv] public static @string Error(this ref DecodingError e) {
    @string msg = e.msg;
    if (e.val != default!) {
        msg += fmt.Sprintf(" '%v'"u8, e.val);
    }
    msg += fmt.Sprintf(" at byte %#x"u8, e.off);
    return msg;
}

} // end gosym_package
