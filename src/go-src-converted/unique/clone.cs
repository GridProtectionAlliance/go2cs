// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using stringslite = @internal.stringslite_package;
using @unsafe = unsafe_package;
using @internal;

partial class unique_package {

// clone makes a copy of value, and may update string values found in value
// with a cloned version of those strings. The purpose of explicitly cloning
// strings is to avoid accidentally giving a large string a long lifetime.
//
// Note that this will clone strings in structs and arrays found in value,
// and will clone value if it itself is a string. It will not, however, clone
// strings if value is of interface or slice type (that is, found via an
// indirection).
internal static T clone<T>(T value, ж<cloneSeq> Ꮡseq)
    where T : /* comparable */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ref var seq = ref Ꮡseq.val;

    foreach (var (_, offset) in seq.stringOffsets) {
        var ps = (ж<@string>)(uintptr)(((@unsafe.Pointer)(((uintptr)new @unsafe.Pointer(Ꮡ(value))) + offset)));
        ps.val = stringslite.Clone(ps.val);
    }
    return value;
}

// singleStringClone describes how to clone a single string.
internal static cloneSeq singleStringClone = new cloneSeq(stringOffsets: new uintptr[]{0}.slice());

// cloneSeq describes how to clone a value of a particular type.
[GoType] partial struct cloneSeq {
    internal slice<uintptr> stringOffsets;
}

// makeCloneSeq creates a cloneSeq for a type.
internal static cloneSeq makeCloneSeq(ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    if (typ == nil) {
        return new cloneSeq(nil);
    }
    if (typ.Kind() == abi.ΔString) {
        return singleStringClone;
    }
    ref var seq = ref heap(new cloneSeq(), out var Ꮡseq);
    var exprᴛ1 = typ.Kind();
    if (exprᴛ1 == abi.Struct) {
        buildStructCloneSeq(Ꮡtyp, Ꮡseq, 0);
    }
    else if (exprᴛ1 == abi.Array) {
        buildArrayCloneSeq(Ꮡtyp, Ꮡseq, 0);
    }

    return seq;
}

// buildStructCloneSeq populates a cloneSeq for an abi.Type that has Kind abi.Struct.
internal static void buildStructCloneSeq(ж<abi.Type> Ꮡtyp, ж<cloneSeq> Ꮡseq, uintptr baseOffset) {
    ref var typ = ref Ꮡtyp.val;
    ref var seq = ref Ꮡseq.val;

    var styp = typ.StructType();
    foreach (var (i, _) in (~styp).Fields) {
        var f = Ꮡ((~styp).Fields, i);
        var exprᴛ1 = (~f).Typ.Kind();
        if (exprᴛ1 == abi.ΔString) {
            seq.stringOffsets = append(seq.stringOffsets, baseOffset + (~f).Offset);
        }
        else if (exprᴛ1 == abi.Struct) {
            buildStructCloneSeq((~f).Typ, Ꮡseq, baseOffset + (~f).Offset);
        }
        else if (exprᴛ1 == abi.Array) {
            buildArrayCloneSeq((~f).Typ, Ꮡseq, baseOffset + (~f).Offset);
        }

    }
}

// buildArrayCloneSeq populates a cloneSeq for an abi.Type that has Kind abi.Array.
internal static void buildArrayCloneSeq(ж<abi.Type> Ꮡtyp, ж<cloneSeq> Ꮡseq, uintptr baseOffset) {
    ref var typ = ref Ꮡtyp.val;
    ref var seq = ref Ꮡseq.val;

    var atyp = typ.ArrayType();
    var etyp = atyp.val.Elem;
    var offset = baseOffset;
    /* for range atyp.Len {
	switch etyp.Kind() {
	case abi.String:
		seq.stringOffsets = append(seq.stringOffsets, offset)
	case abi.Struct:
		buildStructCloneSeq(etyp, seq, offset)
	case abi.Array:
		buildArrayCloneSeq(etyp, seq, offset)
	}
	offset += etyp.Size()
	align := uintptr(etyp.FieldAlign())
	offset = (offset + align - 1) &^ (align - 1)
} */
}

} // end unique_package
