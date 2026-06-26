// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using slicereader = @internal.coverage.slicereader_package;
using uleb128 = @internal.coverage.uleb128_package;
using io = io_package;

partial class stringtab_package {

// This package implements string table writer and reader utilities,
// for use in emitting and reading/decoding coverage meta-data and
// counter-data files.

// Writer implements a string table writing utility.
[GoType] partial struct Writer {
    internal map<@string, uint32> stab;
    internal slice<@string> strs;
    internal slice<byte> tmp;
    internal bool frozen;
}

// InitWriter initializes a stringtab.Writer.
[GoRecv] public static void InitWriter(this ref Writer stw) {
    stw.stab = new map<@string, uint32>();
    stw.tmp = new slice<byte>(64);
}

// Nentries returns the number of strings interned so far.
[GoRecv] public static uint32 Nentries(this ref Writer stw) {
    return ((uint32)len(stw.strs));
}

// Lookup looks up string 's' in the writer's table, adding
// a new entry if need be, and returning an index into the table.
[GoRecv] public static uint32 Lookup(this ref Writer stw, @string s) {
    {
        var (idxΔ1, ok) = stw.stab[s]; if (ok) {
            return idxΔ1;
        }
    }
    if (stw.frozen) {
        throw panic("internal error: string table previously frozen");
    }
    var idx = ((uint32)len(stw.strs));
    stw.stab[s] = idx;
    stw.strs = append(stw.strs, s);
    return idx;
}

// Size computes the memory in bytes needed for the serialized
// version of a stringtab.Writer.
[GoRecv] public static uint32 Size(this ref Writer stw) {
    var rval = ((uint32)0);
    stw.tmp = stw.tmp[..0];
    stw.tmp = uleb128.AppendUleb128(stw.tmp, ((nuint)len(stw.strs)));
    rval += ((uint32)len(stw.tmp));
    foreach (var (_, s) in stw.strs) {
        stw.tmp = stw.tmp[..0];
        nuint slen = ((nuint)len(s));
        stw.tmp = uleb128.AppendUleb128(stw.tmp, slen);
        rval += ((uint32)len(stw.tmp)) + ((uint32)slen);
    }
    return rval;
}

// Write writes the string table in serialized form to the specified
// io.Writer.
[GoRecv] public static error Write(this ref Writer stw, io.Writer w) {
    var wr128 = (nuint v) => {
        stw.tmp = stw.tmp[..0];
        stw.tmp = uleb128.AppendUleb128(stw.tmp, v);
        {
            var (nw, err) = w.Write(stw.tmp); if (err != default!){
                return fmt.Errorf("writing string table: %v"u8, err);
            } else 
            if (nw != len(stw.tmp)) {
                return fmt.Errorf("short write emitting stringtab uleb"u8);
            }
        }
        return default!;
    };
    {
        var err = wr128(((nuint)len(stw.strs))); if (err != default!) {
            return err;
        }
    }
    foreach (var (_, s) in stw.strs) {
        {
            var err = wr128(((nuint)len(s))); if (err != default!) {
                return err;
            }
        }
        {
            var (nw, err) = w.Write(slice<byte>(s)); if (err != default!){
                return fmt.Errorf("writing string table: %v"u8, err);
            } else 
            if (nw != len(slice<byte>(s))) {
                return fmt.Errorf("short write emitting stringtab"u8);
            }
        }
    }
    return default!;
}

// Freeze sends a signal to the writer that no more additions are
// allowed, only lookups of existing strings (if a lookup triggers
// addition, a panic will result). Useful as a mechanism for
// "finalizing" a string table prior to writing it out.
[GoRecv] public static void Freeze(this ref Writer stw) {
    stw.frozen = true;
}

// Reader is a helper for reading a string table previously
// serialized by a Writer.Write call.
[GoType] partial struct Reader {
    internal ж<@internal.coverage.slicereader_package.Reader> r;
    internal slice<@string> strs;
}

// NewReader creates a stringtab.Reader to read the contents
// of a string table from 'r'.
public static ж<Reader> NewReader(ж<slicereader.Reader> Ꮡr) {
    ref var r = ref Ꮡr.val;

    var str = Ꮡ(new Reader(
        r: r
    ));
    return str;
}

// Read reads/decodes a string table using the reader provided.
[GoRecv] public static void Read(this ref Reader str) {
    nint numEntries = ((nint)str.r.ReadULEB128());
    str.strs = new slice<@string>(0, numEntries);
    for (nint idx = 0; idx < numEntries; idx++) {
        var slen = str.r.ReadULEB128();
        str.strs = append(str.strs, str.r.ReadString(((int64)slen)));
    }
}

// Entries returns the number of decoded entries in a string table.
[GoRecv] public static nint Entries(this ref Reader str) {
    return len(str.strs);
}

// Get returns string 'idx' within the string table.
[GoRecv] public static @string Get(this ref Reader str, uint32 idx) {
    return str.strs[idx];
}

} // end stringtab_package
