// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// DWARF debug information entry parser.
// An entry is a sequence of data items of a given format.
// The first word in the entry is an index into what DWARF
// calls the ``abbreviation table.''  An abbreviation is really
// just a type descriptor: it's an array of attribute tag/value format pairs.
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using strconv = strconv_package;
using encoding;

partial class dwarf_package {

// a single entry's description: a sequence of attributes
[GoType] partial struct abbrev {
    internal Tag tag;
    internal bool children;
    internal slice<afield> field;
}

[GoType] partial struct afield {
    internal Attr attr;
    internal format fmt;
    internal Class @class;
    internal int64 val; // for formImplicitConst
}
/* visitMapType: map[uint32]abbrev */

// parseAbbrev returns the abbreviation table that starts at byte off
// in the .debug_abbrev section.
[GoRecv] internal static (abbrevTable, error) parseAbbrev(this ref Data d, uint64 off, nint vers) {
    {
        var mΔ1 = d.abbrevCache[off];
        var ok = d.abbrevCache[off]; if (ok) {
            return (mΔ1, default!);
        }
    }
    var data = d.abbrev;
    if (off > ((uint64)len(data))){
        data = default!;
    } else {
        data = data[(int)(off)..];
    }
    ref var b = ref heap<buf>(out var Ꮡb);
    b = makeBuf(d, new unknownFormat(nil), "abbrev"u8, 0, data);
    // Error handling is simplified by the buf getters
    // returning an endless stream of 0s after an error.
    var m = new abbrevTable();
    while (ᐧ) {
        // Table ends with id == 0.
        var id = ((uint32)b.@uint());
        if (id == 0) {
            break;
        }
        // Walk over attributes, counting.
        nint n = 0;
        var b1 = b;
        // Read from copy of b.
        b1.@uint();
        b1.uint8();
        while (ᐧ) {
            var tag = b1.@uint();
            var fmt = b1.@uint();
            if (tag == 0 && fmt == 0) {
                break;
            }
            if (((format)fmt) == formImplicitConst) {
                b1.@int();
            }
            n++;
        }
        if (b1.err != default!) {
            return (default!, b1.err);
        }
        // Walk over attributes again, this time writing them down.
        abbrev a = default!;
        a.tag = ((Tag)b.@uint());
        a.children = b.uint8() != 0;
        a.field = new slice<afield>(n);
        foreach (var (i, _) in a.field) {
            a.field[i].attr = ((Attr)b.@uint());
            a.field[i].fmt = ((format)b.@uint());
            a.field[i].@class = formToClass(a.field[i].fmt, a.field[i].attr, vers, Ꮡb);
            if (a.field[i].fmt == formImplicitConst) {
                a.field[i].val = b.@int();
            }
        }
        b.@uint();
        b.@uint();
        m[id] = a;
    }
    if (b.err != default!) {
        return (default!, b.err);
    }
    d.abbrevCache[off] = m;
    return (m, default!);
}

// attrIsExprloc indicates attributes that allow exprloc values that
// are encoded as block values in DWARF 2 and 3. See DWARF 4, Figure
// 20.
internal static map<Attr, bool> attrIsExprloc = new map<Attr, bool>{
    [AttrLocation] = true,
    [AttrByteSize] = true,
    [AttrBitOffset] = true,
    [AttrBitSize] = true,
    [AttrStringLength] = true,
    [AttrLowerBound] = true,
    [AttrReturnAddr] = true,
    [AttrStrideSize] = true,
    [AttrUpperBound] = true,
    [AttrCount] = true,
    [AttrDataMemberLoc] = true,
    [AttrFrameBase] = true,
    [AttrSegment] = true,
    [AttrStaticLink] = true,
    [AttrUseLocation] = true,
    [AttrVtableElemLoc] = true,
    [AttrAllocated] = true,
    [AttrAssociated] = true,
    [AttrDataLocation] = true,
    [AttrStride] = true
};

// The following are new in DWARF 5.
// attrPtrClass indicates the *ptr class of attributes that have
// encoding formSecOffset in DWARF 4 or formData* in DWARF 2 and 3.
internal static dwarf.Class attrPtrClass = new map<Attr, Class>{
    [AttrLocation] = ClassLocListPtr,
    [AttrStmtList] = ClassLinePtr,
    [AttrStringLength] = ClassLocListPtr,
    [AttrReturnAddr] = ClassLocListPtr,
    [AttrStartScope] = ClassRangeListPtr,
    [AttrDataMemberLoc] = ClassLocListPtr,
    [AttrFrameBase] = ClassLocListPtr,
    [AttrMacroInfo] = ClassMacPtr,
    [AttrSegment] = ClassLocListPtr,
    [AttrStaticLink] = ClassLocListPtr,
    [AttrUseLocation] = ClassLocListPtr,
    [AttrVtableElemLoc] = ClassLocListPtr,
    [AttrRanges] = ClassRangeListPtr,
    [AttrStrOffsetsBase] = ClassStrOffsetsPtr,
    [AttrAddrBase] = ClassAddrPtr,
    [AttrRnglistsBase] = ClassRngListsPtr,
    [AttrLoclistsBase] = ClassLocListPtr
};

// formToClass returns the DWARF 4 Class for the given form. If the
// DWARF version is less then 4, it will disambiguate some forms
// depending on the attribute.
internal static Class formToClass(format form, Attr attr, nint vers, ж<buf> Ꮡb) {
    ref var b = ref Ꮡb.val;

    var exprᴛ1 = form;
    { /* default: */
        b.error("cannot determine class of unknown attribute form"u8);
        return 0;
    }
    if (exprᴛ1 == formIndirect) {
        return ClassUnknown;
    }
    if (exprᴛ1 == formAddr || exprᴛ1 == formAddrx || exprᴛ1 == formAddrx1 || exprᴛ1 == formAddrx2 || exprᴛ1 == formAddrx3 || exprᴛ1 == formAddrx4) {
        return ClassAddress;
    }
    if (exprᴛ1 == formDwarfBlock1 || exprᴛ1 == formDwarfBlock2 || exprᴛ1 == formDwarfBlock4 || exprᴛ1 == formDwarfBlock) {
        if (attrIsExprloc[attr]) {
            // In DWARF 2 and 3, ClassExprLoc was encoded as a
            // block. DWARF 4 distinguishes ClassBlock and
            // ClassExprLoc, but there are no attributes that can
            // be both, so we also promote ClassBlock values in
            // DWARF 4 that should be ClassExprLoc in case
            // producers get this wrong.
            return ClassExprLoc;
        }
        return ClassBlock;
    }
    if (exprᴛ1 == formData1 || exprᴛ1 == formData2 || exprᴛ1 == formData4 || exprᴛ1 == formData8 || exprᴛ1 == formSdata || exprᴛ1 == formUdata || exprᴛ1 == formData16 || exprᴛ1 == formImplicitConst) {
        {
            Class @class = attrPtrClass[attr];
            var ok = attrPtrClass[attr]; if (vers < 4 && ok) {
                // In DWARF 2 and 3, ClassPtr was encoded as a
                // constant. Unlike ClassExprLoc/ClassBlock, some
                // DWARF 4 attributes need to distinguish Class*Ptr
                // from ClassConstant, so we only do this promotion
                // for versions 2 and 3.
                return @class;
            }
        }
        return ClassConstant;
    }
    if (exprᴛ1 == formFlag || exprᴛ1 == formFlagPresent) {
        return ClassFlag;
    }
    if (exprᴛ1 == formRefAddr || exprᴛ1 == formRef1 || exprᴛ1 == formRef2 || exprᴛ1 == formRef4 || exprᴛ1 == formRef8 || exprᴛ1 == formRefUdata || exprᴛ1 == formRefSup4 || exprᴛ1 == formRefSup8) {
        return ClassReference;
    }
    if (exprᴛ1 == formRefSig8) {
        return ClassReferenceSig;
    }
    if (exprᴛ1 == formString || exprᴛ1 == formStrp || exprᴛ1 == formStrx || exprᴛ1 == formStrpSup || exprᴛ1 == formLineStrp || exprᴛ1 == formStrx1 || exprᴛ1 == formStrx2 || exprᴛ1 == formStrx3 || exprᴛ1 == formStrx4) {
        return ClassString;
    }
    if (exprᴛ1 == formSecOffset) {
        {
            Class @class = attrPtrClass[attr];
            var ok = attrPtrClass[attr]; if (ok) {
                // DWARF 4 defines four *ptr classes, but doesn't
                // distinguish them in the encoding. Disambiguate
                // these classes using the attribute.
                return @class;
            }
        }
        return ClassUnknown;
    }
    if (exprᴛ1 == formExprloc) {
        return ClassExprLoc;
    }
    if (exprᴛ1 == formGnuRefAlt) {
        return ClassReferenceAlt;
    }
    if (exprᴛ1 == formGnuStrpAlt) {
        return ClassStringAlt;
    }
    if (exprᴛ1 == formLoclistx) {
        return ClassLocList;
    }
    if (exprᴛ1 == formRnglistx) {
        return ClassRngList;
    }

}

// An entry is a sequence of attribute/value pairs.
[GoType] partial struct Entry {
    public Offset Offset; // offset of Entry in DWARF info
    public Tag Tag;    // tag (kind of Entry)
    public bool Children;   // whether Entry is followed by children
    public slice<Field> Field;
}

// A Field is a single attribute/value pair in an [Entry].
//
// A value can be one of several "attribute classes" defined by DWARF.
// The Go types corresponding to each class are:
//
//	DWARF class       Go type        Class
//	-----------       -------        -----
//	address           uint64         ClassAddress
//	block             []byte         ClassBlock
//	constant          int64          ClassConstant
//	flag              bool           ClassFlag
//	reference
//	  to info         dwarf.Offset   ClassReference
//	  to type unit    uint64         ClassReferenceSig
//	string            string         ClassString
//	exprloc           []byte         ClassExprLoc
//	lineptr           int64          ClassLinePtr
//	loclistptr        int64          ClassLocListPtr
//	macptr            int64          ClassMacPtr
//	rangelistptr      int64          ClassRangeListPtr
//
// For unrecognized or vendor-defined attributes, [Class] may be
// [ClassUnknown].
[GoType] partial struct Field {
    public Attr Attr;
    public any Val;
    public Class Class;
}

[GoType("num:nint")] partial struct Class;

public static readonly Class ClassUnknown = /* iota */ 0;
public static readonly Class ClassAddress = 1;
public static readonly Class ClassBlock = 2;
public static readonly Class ClassConstant = 3;
public static readonly Class ClassExprLoc = 4;
public static readonly Class ClassFlag = 5;
public static readonly Class ClassLinePtr = 6;
public static readonly Class ClassLocListPtr = 7;
public static readonly Class ClassMacPtr = 8;
public static readonly Class ClassRangeListPtr = 9;
public static readonly Class ClassReference = 10;
public static readonly Class ClassReferenceSig = 11;
public static readonly Class ClassString = 12;
public static readonly Class ClassReferenceAlt = 13;
public static readonly Class ClassStringAlt = 14;
public static readonly Class ClassAddrPtr = 15;
public static readonly Class ClassLocList = 16;
public static readonly Class ClassRngList = 17;
public static readonly Class ClassRngListsPtr = 18;
public static readonly Class ClassStrOffsetsPtr = 19;

//go:generate stringer -type=Class
public static @string GoString(this Class i) {
    return "dwarf."u8 + i.String();
}

// Val returns the value associated with attribute [Attr] in [Entry],
// or nil if there is no such attribute.
//
// A common idiom is to merge the check for nil return with
// the check that the value has the expected dynamic type, as in:
//
//	v, ok := e.Val(AttrSibling).(int64)
[GoRecv] public static any Val(this ref Entry e, Attr a) {
    {
        var f = e.AttrField(a); if (f != nil) {
            return (~f).Val;
        }
    }
    return default!;
}

// AttrField returns the [Field] associated with attribute [Attr] in
// [Entry], or nil if there is no such attribute.
[GoRecv] public static ж<Field> AttrField(this ref Entry e, Attr a) {
    foreach (var (i, f) in e.Field) {
        if (f.Attr == a) {
            return Ꮡ(e.Field[i]);
        }
    }
    return default!;
}

[GoType("num:uint32")] partial struct Offset;

// If we are currently parsing the compilation unit,
// we can't evaluate Addrx or Strx until we've seen the
// relevant base entry.
[GoType("dyn")] partial struct entry_delayed {
    internal nint idx;
    internal uint64 off;
    internal format fmt;
}

// Entry reads a single entry from buf, decoding
// according to the given abbreviation table.
[GoRecv] internal static ж<Entry> entry(this ref buf b, ж<Entry> Ꮡcu, abbrevTable atab, Offset ubase, nint vers) {
    ref var cu = ref Ꮡcu.val;

    ref var offΔ1 = ref heap<Offset>(out var ᏑoffΔ1);
    offΔ1 = b.off;
    var id = ((uint32)b.@uint());
    if (id == 0) {
        return Ꮡ(new Entry(nil));
    }
    var (a, ok) = atab[id];
    if (!ok) {
        b.error("unknown abbreviation table index"u8);
        return default!;
    }
    var e = Ꮡ(new Entry(
        Offset: offΔ1,
        Tag: a.tag,
        Children: a.children,
        Field: new slice<Field>(len(a.field))
    ));
    slice<delayed> delay = default!;
    var resolveStrx = (uint64 strBase, uint64 off) => {
         += strBaseΔ1;
        if (((uint64)((nint)offΔ2)) != offΔ2) {
            b.error("DW_FORM_strx offset out of range"u8);
        }
        ref var b1Δ1 = ref heap<buf>(out var Ꮡb1Δ1);
        b1Δ1 = makeBuf(b.dwarf, b.format, "str_offsets"u8, 0, b.dwarf.strOffsets);
        b1Δ1.skip(((nint)offΔ2));
        var (is64, _) = b.format.dwarf64();
        if (is64){
             = b1Δ1.uint64();
        } else {
             = ((uint64)b1Δ1.uint32());
        }
        if (b1Δ1.err != default!) {
            b.err = b1Δ1.err;
            return ""u8;
        }
        if (((uint64)((nint)offΔ2)) != offΔ2) {
            b.error("DW_FORM_strx indirect offset out of range"u8);
        }
         = makeBuf(b.dwarf, b.format, "str"u8, 0, b.dwarf.str);
        b1Δ1.skip(((nint)offΔ2));
        @string valΔ1 = b1Δ1.@string();
        if (b1Δ1.err != default!) {
            b.err = b1Δ1.err;
        }
        return valΔ1;
    };
    var resolveRnglistx = (uint64 rnglistsBase, uint64 off) => {
        var (is64, _) = b.format.dwarf64();
        if (is64){
             *= 8;
        } else {
             *= 4;
        }
         += rnglistsBaseΔ1;
        if (((uint64)((nint)offΔ3)) != offΔ3) {
            b.error("DW_FORM_rnglistx offset out of range"u8);
        }
        ref var b1Δ2 = ref heap<buf>(out var Ꮡb1Δ2);
        b1Δ2 = makeBuf(b.dwarf, b.format, "rnglists"u8, 0, b.dwarf.rngLists);
        b1Δ2.skip(((nint)offΔ3));
        if (is64){
             = b1Δ2.uint64();
        } else {
             = ((uint64)b1Δ2.uint32());
        }
        if (b1Δ2.err != default!) {
            b.err = b1Δ2.err;
            return 0;
        }
        if (((uint64)((nint)offΔ3)) != offΔ3) {
            b.error("DW_FORM_rnglistx indirect offset out of range"u8);
        }
        return rnglistsBaseΔ1 + offΔ3;
    };
    foreach (var (i, _) in (~e).Field) {
        (~e).Field[i].Attr = a.field[i].attr;
        (~e).Field[i].Class = a.field[i].@class;
        var fmt = a.field[i].fmt;
        if (fmt == formIndirect) {
            fmt = ((format)b.@uint());
            (~e).Field[i].Class = formToClass(fmt, a.field[i].attr, vers, b);
        }
        any val = default!;
        var exprᴛ1 = fmt;
        { /* default: */
            b.error("unknown entry attr format 0x"u8 + strconv.FormatInt(((int64)fmt), 16));
        }
        else if (exprᴛ1 == formAddr) {
            val = b.addr();
        }
        else if (exprᴛ1 == formAddrx || exprᴛ1 == formAddrx1 || exprᴛ1 == formAddrx2 || exprᴛ1 == formAddrx3 || exprᴛ1 == formAddrx4) {
// address
            uint64 offΔ6 = default!;
            var exprᴛ2 = fmt;
            if (exprᴛ2 == formAddrx) {
                off = b.@uint();
            }
            else if (exprᴛ2 == formAddrx1) {
                off = ((uint64)b.uint8());
            }
            else if (exprᴛ2 == formAddrx2) {
                off = ((uint64)b.uint16());
            }
            else if (exprᴛ2 == formAddrx3) {
                off = ((uint64)b.uint24());
            }
            else if (exprᴛ2 == formAddrx4) {
                off = ((uint64)b.uint32());
            }

            if (b.dwarf.addr == default!) {
                b.error("DW_FORM_addrx with no .debug_addr section"u8);
            }
            if (b.err != default!) {
                return default!;
            }
            // We have to adjust by the offset of the
            // compilation unit. This won't work if the
            // program uses Reader.Seek to skip over the
            // unit. Not much we can do about that.
            int64 addrBase = default!;
            if (cu != nil){
                (addrBase, _) = cu.Val(AttrAddrBase)._<int64>(ᐧ);
            } else 
            if (a.tag == TagCompileUnit) {
                delay = append(delay, new delayed(i, offΔ6, formAddrx));
                break;
            }
            error err = default!;
            (val, err) = b.dwarf.debugAddr(b.format, ((uint64)addrBase), offΔ6);
            if (err != default!) {
                if (b.err == default!) {
                    b.err = err;
                }
                return default!;
            }
        }
        if (exprᴛ1 == formDwarfBlock1) {
            val = b.bytes(((nint)b.uint8()));
        }
        else if (exprᴛ1 == formDwarfBlock2) {
            val = b.bytes(((nint)b.uint16()));
        }
        else if (exprᴛ1 == formDwarfBlock4) {
            val = b.bytes(((nint)b.uint32()));
        }
        else if (exprᴛ1 == formDwarfBlock) {
            val = b.bytes(((nint)b.@uint()));
        }
        else if (exprᴛ1 == formData1) {
            val = ((int64)b.uint8());
        }
        else if (exprᴛ1 == formData2) {
            val = ((int64)b.uint16());
        }
        else if (exprᴛ1 == formData4) {
            val = ((int64)b.uint32());
        }
        else if (exprᴛ1 == formData8) {
            val = ((int64)b.uint64());
        }
        else if (exprᴛ1 == formData16) {
            val = b.bytes(16);
        }
        else if (exprᴛ1 == formSdata) {
            val = ((int64)b.@int());
        }
        else if (exprᴛ1 == formUdata) {
            val = ((int64)b.@uint());
        }
        else if (exprᴛ1 == formImplicitConst) {
            val = a.field[i].val;
        }
        else if (exprᴛ1 == formFlag) {
            val = b.uint8() == 1;
        }
        else if (exprᴛ1 == formFlagPresent) {
            val = true;
        }
        else if (exprᴛ1 == formRefAddr) {
            nint versΔ2 = b.format.version();
            if (versΔ2 == 0){
                // block
                // constant
                // flag
                // New in DWARF 4.
                // The attribute is implicitly indicated as present, and no value is
                // encoded in the debugging information entry itself.
                // reference to other entry
                b.error("unknown version for DW_FORM_ref_addr"u8);
            } else 
            if (versΔ2 == 2){
                val = ((Offset)b.addr());
            } else {
                var (is64, known) = b.format.dwarf64();
                if (!known){
                    b.error("unknown size for DW_FORM_ref_addr"u8);
                } else 
                if (is64){
                    val = ((Offset)b.uint64());
                } else {
                    val = ((Offset)b.uint32());
                }
            }
        }
        else if (exprᴛ1 == formRef1) {
            val = ((Offset)b.uint8()) + ubase;
        }
        else if (exprᴛ1 == formRef2) {
            val = ((Offset)b.uint16()) + ubase;
        }
        else if (exprᴛ1 == formRef4) {
            val = ((Offset)b.uint32()) + ubase;
        }
        else if (exprᴛ1 == formRef8) {
            val = ((Offset)b.uint64()) + ubase;
        }
        else if (exprᴛ1 == formRefUdata) {
            val = ((Offset)b.@uint()) + ubase;
        }
        else if (exprᴛ1 == formString) {
            val = b.@string();
        }
        else if (exprᴛ1 == formStrp || exprᴛ1 == formLineStrp) {
// string
            uint64 offΔ7 = default!;        // offset into .debug_str
            var (is64, known) = b.format.dwarf64();
            if (!known){
                b.error("unknown size for DW_FORM_strp/line_strp"u8);
            } else 
            if (is64){
                off = b.uint64();
            } else {
                off = ((uint64)b.uint32());
            }
            if (((uint64)((nint)offΔ7)) != offΔ7) {
                b.error("DW_FORM_strp/line_strp offset out of range"u8);
            }
            if (b.err != default!) {
                return default!;
            }
            buf b1 = default!;
            if (fmt == formStrp){
                b1 = makeBuf(b.dwarf, b.format, "str"u8, 0, b.dwarf.str);
            } else {
                if (len(b.dwarf.lineStr) == 0) {
                    b.error("DW_FORM_line_strp with no .debug_line_str section"u8);
                    return default!;
                }
                b1 = makeBuf(b.dwarf, b.format, "line_str"u8, 0, b.dwarf.lineStr);
            }
            b1.skip(((nint)offΔ7));
            val = b1.@string();
            if (b1.err != default!) {
                b.err = b1.err;
                return default!;
            }
        }
        if (exprᴛ1 == formStrx || exprᴛ1 == formStrx1 || exprᴛ1 == formStrx2 || exprᴛ1 == formStrx3 || exprᴛ1 == formStrx4) {
            uint64 offΔ8 = default!;
            var exprᴛ3 = fmt;
            if (exprᴛ3 == formStrx) {
                offΔ8 = b.@uint();
            }
            else if (exprᴛ3 == formStrx1) {
                offΔ8 = ((uint64)b.uint8());
            }
            else if (exprᴛ3 == formStrx2) {
                offΔ8 = ((uint64)b.uint16());
            }
            else if (exprᴛ3 == formStrx3) {
                offΔ8 = ((uint64)b.uint24());
            }
            else if (exprᴛ3 == formStrx4) {
                offΔ8 = ((uint64)b.uint32());
            }

            if (len(b.dwarf.strOffsets) == 0) {
                b.error("DW_FORM_strx with no .debug_str_offsets section"u8);
            }
            var (is64, known) = b.format.dwarf64();
            if (!known) {
                b.error("unknown offset size for DW_FORM_strx"u8);
            }
            if (b.err != default!) {
                return default!;
            }
            if (is64){
                offΔ8 *= 8;
            } else {
                offΔ8 *= 4;
            }
            // We have to adjust by the offset of the
            // compilation unit. This won't work if the
            // program uses Reader.Seek to skip over the
            // unit. Not much we can do about that.
            int64 strBase = default!;
            if (cu != nil){
                (strBase, _) = cu.Val(AttrStrOffsetsBase)._<int64>(ᐧ);
            } else 
            if (a.tag == TagCompileUnit) {
                delay = append(delay, new delayed(i, offΔ8, formStrx));
                break;
            }
            val = resolveStrx(((uint64)strBase), offΔ8);
        }
        else if (exprᴛ1 == formStrpSup) {
            var (is64, known) = b.format.dwarf64();
            if (!known){
                b.error("unknown size for DW_FORM_strp_sup"u8);
            } else 
            if (is64){
                val = b.uint64();
            } else {
                val = b.uint32();
            }
        }
        else if (exprᴛ1 == formSecOffset || exprᴛ1 == formGnuRefAlt || exprᴛ1 == formGnuStrpAlt) {
            var (is64, known) = b.format.dwarf64();
            if (!known){
                // lineptr, loclistptr, macptr, rangelistptr
                // New in DWARF 4, but clang can generate them with -gdwarf-2.
                // Section reference, replacing use of formData4 and formData8.
                b.error("unknown size for form 0x"u8 + strconv.FormatInt(((int64)fmt), 16));
            } else 
            if (is64){
                val = ((int64)b.uint64());
            } else {
                val = ((int64)b.uint32());
            }
        }
        else if (exprᴛ1 == formExprloc) {
            val = b.bytes(((nint)b.@uint()));
        }
        else if (exprᴛ1 == formRefSig8) {
            val = b.uint64();
        }
        else if (exprᴛ1 == formRefSup4) {
            val = b.uint32();
        }
        else if (exprᴛ1 == formRefSup8) {
            val = b.uint64();
        }
        else if (exprᴛ1 == formLoclistx) {
            val = b.@uint();
        }
        else if (exprᴛ1 == formRnglistx) {
            var offΔ9 = b.@uint();
// exprloc
// New in DWARF 4.
// reference
// New in DWARF 4.
// 64-bit type signature.
// loclist
// rnglist

            // We have to adjust by the rnglists_base of
            // the compilation unit. This won't work if
            // the program uses Reader.Seek to skip over
            // the unit. Not much we can do about that.
            int64 rnglistsBase = default!;
            if (cu != nil){
                (rnglistsBase, _) = cu.Val(AttrRnglistsBase)._<int64>(ᐧ);
            } else 
            if (a.tag == TagCompileUnit) {
                delay = append(delay, new delayed(i, offΔ9, formRnglistx));
                break;
            }
            val = resolveRnglistx(((uint64)rnglistsBase), offΔ9);
        }

        (~e).Field[i].Val = val;
    }
    if (b.err != default!) {
        return default!;
    }
    foreach (var (_, del) in delay) {
        var exprᴛ4 = del.fmt;
        if (exprᴛ4 == formAddrx) {
            var (addrBase, _) = e.Val(AttrAddrBase)._<int64>(ᐧ);
            var (val, err) = b.dwarf.debugAddr(b.format, ((uint64)addrBase), del.off);
            if (err != default!) {
                b.err = err;
                return default!;
            }
            (~e).Field[del.idx].Val = val;
        }
        else if (exprᴛ4 == formStrx) {
            var (strBase, _) = e.Val(AttrStrOffsetsBase)._<int64>(ᐧ);
            (~e).Field[del.idx].Val = resolveStrx(((uint64)strBase), del.off);
            if (b.err != default!) {
                return default!;
            }
        }
        if (exprᴛ4 == formRnglistx) {
            var (rnglistsBase, _) = e.Val(AttrRnglistsBase)._<int64>(ᐧ);
            (~e).Field[del.idx].Val = resolveRnglistx(((uint64)rnglistsBase), del.off);
            if (b.err != default!) {
                return default!;
            }
        }

    }
    return e;
}

// A Reader allows reading [Entry] structures from a DWARF “info” section.
// The [Entry] structures are arranged in a tree. The [Reader.Next] function
// return successive entries from a pre-order traversal of the tree.
// If an entry has children, its Children field will be true, and the children
// follow, terminated by an [Entry] with [Tag] 0.
[GoType] partial struct ΔReader {
    internal buf b;
    internal ж<Data> d;
    internal error err;
    internal nint unit;
    internal bool lastUnit;   // set if last entry returned by Next is TagCompileUnit/TagPartialUnit
    internal bool lastChildren;   // .Children of last entry returned by Next
    internal Offset lastSibling; // .Val(AttrSibling) of last entry returned by Next
    internal ж<Entry> cu; // current compilation unit
}

// Reader returns a new Reader for [Data].
// The reader is positioned at byte offset 0 in the DWARF “info” section.
[GoRecv] public static ж<ΔReader> Reader(this ref Data d) {
    var r = Ꮡ(new ΔReader(d: d));
    r.Seek(0);
    return r;
}

// AddressSize returns the size in bytes of addresses in the current compilation
// unit.
[GoRecv] public static nint AddressSize(this ref ΔReader r) {
    return r.d.unit[r.unit].asize;
}

// ByteOrder returns the byte order in the current compilation unit.
[GoRecv] public static binary.ByteOrder ByteOrder(this ref ΔReader r) {
    return r.b.order;
}

// Seek positions the [Reader] at offset off in the encoded entry stream.
// Offset 0 can be used to denote the first entry.
[GoRecv] public static void Seek(this ref ΔReader r, Offset off) {
    var d = r.d;
    r.err = default!;
    r.lastChildren = false;
    if (off == 0) {
        if (len((~d).unit) == 0) {
            return;
        }
        var uΔ1 = Ꮡ((~d).unit, 0);
        r.unit = 0;
        r.b = makeBuf(r.d, ~uΔ1, "info"u8, (~uΔ1).off, (~uΔ1).data);
        r.cu = default!;
        return;
    }
    nint i = d.offsetToUnit(off);
    if (i == -1) {
        r.err = errors.New("offset out of range"u8);
        return;
    }
    if (i != r.unit) {
        r.cu = default!;
    }
    var u = Ꮡ((~d).unit, i);
    r.unit = i;
    r.b = makeBuf(r.d, ~u, "info"u8, off, (~u).data[(int)(off - (~u).off)..]);
}

// maybeNextUnit advances to the next unit if this one is finished.
[GoRecv] internal static void maybeNextUnit(this ref ΔReader r) {
    while (len(r.b.data) == 0 && r.unit + 1 < len(r.d.unit)) {
        r.nextUnit();
    }
}

// nextUnit advances to the next unit.
[GoRecv] internal static void nextUnit(this ref ΔReader r) {
    r.unit++;
    var u = Ꮡ(r.d.unit[r.unit]);
    r.b = makeBuf(r.d, ~u, "info"u8, (~u).off, (~u).data);
    r.cu = default!;
}

// Next reads the next entry from the encoded entry stream.
// It returns nil, nil when it reaches the end of the section.
// It returns an error if the current offset is invalid or the data at the
// offset cannot be decoded as a valid [Entry].
[GoRecv] public static (ж<Entry>, error) Next(this ref ΔReader r) {
    if (r.err != default!) {
        return (default!, r.err);
    }
    r.maybeNextUnit();
    if (len(r.b.data) == 0) {
        return (default!, default!);
    }
    var u = Ꮡ(r.d.unit[r.unit]);
    var e = r.b.entry(r.cu, (~u).atable, (~u).@base, (~u).vers);
    if (r.b.err != default!) {
        r.err = r.b.err;
        return (default!, r.err);
    }
    r.lastUnit = false;
    if (e != nil){
        r.lastChildren = e.val.Children;
        if (r.lastChildren) {
            (r.lastSibling, _) = e.Val(AttrSibling)._<Offset>(ᐧ);
        }
        if ((~e).Tag == TagCompileUnit || (~e).Tag == TagPartialUnit) {
            r.lastUnit = true;
            r.cu = e;
        }
    } else {
        r.lastChildren = false;
    }
    return (e, default!);
}

// SkipChildren skips over the child entries associated with
// the last [Entry] returned by [Reader.Next]. If that [Entry] did not have
// children or [Reader.Next] has not been called, SkipChildren is a no-op.
[GoRecv] public static void SkipChildren(this ref ΔReader r) {
    if (r.err != default! || !r.lastChildren) {
        return;
    }
    // If the last entry had a sibling attribute,
    // that attribute gives the offset of the next
    // sibling, so we can avoid decoding the
    // child subtrees.
    if (r.lastSibling >= r.b.off) {
        r.Seek(r.lastSibling);
        return;
    }
    if (r.lastUnit && r.unit + 1 < len(r.d.unit)) {
        r.nextUnit();
        return;
    }
    while (ᐧ) {
        (e, err) = r.Next();
        if (err != default! || e == nil || (~e).Tag == 0) {
            break;
        }
        if ((~e).Children) {
            r.SkipChildren();
        }
    }
}

// clone returns a copy of the reader. This is used by the typeReader
// interface.
[GoRecv] internal static typeReader clone(this ref ΔReader r) {
    return ~r.d.Reader();
}

// offset returns the current buffer offset. This is used by the
// typeReader interface.
[GoRecv] internal static Offset offset(this ref ΔReader r) {
    return r.b.off;
}

// SeekPC returns the [Entry] for the compilation unit that includes pc,
// and positions the reader to read the children of that unit.  If pc
// is not covered by any unit, SeekPC returns [ErrUnknownPC] and the
// position of the reader is undefined.
//
// Because compilation units can describe multiple regions of the
// executable, in the worst case SeekPC must search through all the
// ranges in all the compilation units. Each call to SeekPC starts the
// search at the compilation unit of the last call, so in general
// looking up a series of PCs will be faster if they are sorted. If
// the caller wishes to do repeated fast PC lookups, it should build
// an appropriate index using the Ranges method.
[GoRecv] public static (ж<Entry>, error) SeekPC(this ref ΔReader r, uint64 pc) {
    nint unit = r.unit;
    for (nint i = 0; i < len(r.d.unit); i++) {
        if (unit >= len(r.d.unit)) {
            unit = 0;
        }
        r.err = default!;
        r.lastChildren = false;
        r.unit = unit;
        r.cu = default!;
        var u = Ꮡ(r.d.unit[unit]);
        r.b = makeBuf(r.d, ~u, "info"u8, (~u).off, (~u).data);
        (e, err) = r.Next();
        if (err != default!) {
            return (default!, err);
        }
        if (e == nil || (~e).Tag == 0) {
            return (default!, ErrUnknownPC);
        }
        (ranges, err) = r.d.Ranges(e);
        if (err != default!) {
            return (default!, err);
        }
        foreach (var (_, pcs) in ranges) {
            if (pcs[0] <= pc && pc < pcs[1]) {
                return (e, default!);
            }
        }
        unit++;
    }
    return (default!, ErrUnknownPC);
}

// Ranges returns the PC ranges covered by e, a slice of [low,high) pairs.
// Only some entry types, such as [TagCompileUnit] or [TagSubprogram], have PC
// ranges; for others, this will return nil with no error.
[GoRecv] public static (slice<array<uint64>>, error) Ranges(this ref Data d, ж<Entry> Ꮡe) {
    ref var e = ref Ꮡe.val;

    slice<array<uint64>> ret = default!;
    var (low, lowOK) = e.Val(AttrLowpc)._<uint64>(ᐧ);
    uint64 high = default!;
    bool highOK = default!;
    var highField = e.AttrField(AttrHighpc);
    if (highField != nil) {
        var exprᴛ1 = (~highField).Class;
        if (exprᴛ1 == ClassAddress) {
            (high, highOK) = (~highField).Val._<uint64>(ᐧ);
        }
        else if (exprᴛ1 == ClassConstant) {
            var (off, ok) = (~highField).Val._<int64>(ᐧ);
            if (ok) {
                high = low + ((uint64)off);
                highOK = true;
            }
        }

    }
    if (lowOK && highOK) {
        ret = append(ret, new uint64[]{low, high}.array());
    }
    ж<unit> u = default!;
    {
        nint uidx = d.offsetToUnit(e.Offset); if (uidx >= 0 && uidx < len(d.unit)) {
            u = Ꮡ(d.unit[uidx]);
        }
    }
    if (u != nil && (~u).vers >= 5 && d.rngLists != default!) {
        // DWARF version 5 and later
        var field = e.AttrField(AttrRanges);
        if (field == nil) {
            return (ret, default!);
        }
        var exprᴛ2 = (~field).Class;
        if (exprᴛ2 == ClassRangeListPtr) {
            var (rangesΔ2, rangesOKΔ2) = (~field).Val._<int64>(ᐧ);
            if (!rangesOKΔ2) {
                return (ret, default!);
            }
            var (cu, @base, err) = d.baseAddressForEntry(Ꮡe);
            if (err != default!) {
                return (default!, err);
            }
            return d.dwarf5Ranges(u, cu, @base, rangesΔ2, ret);
        }
        if (exprᴛ2 == ClassRngList) {
            var (rnglist, ok) = (~field).Val._<uint64>(ᐧ);
            if (!ok) {
                return (ret, default!);
            }
            var (cu, @base, err) = d.baseAddressForEntry(Ꮡe);
            if (err != default!) {
                return (default!, err);
            }
            return d.dwarf5Ranges(u, cu, @base, ((int64)rnglist), ret);
        }
        { /* default: */
            return (ret, default!);
        }

    }
    // DWARF version 2 through 4
    var (ranges, rangesOK) = e.Val(AttrRanges)._<int64>(ᐧ);
    if (rangesOK && d.ranges != default!) {
        var (_, @base, err) = d.baseAddressForEntry(Ꮡe);
        if (err != default!) {
            return (default!, err);
        }
        return d.dwarf2Ranges(u, @base, ranges, ret);
    }
    return (ret, default!);
}

// baseAddressForEntry returns the initial base address to be used when
// looking up the range list of entry e.
// DWARF specifies that this should be the lowpc attribute of the enclosing
// compilation unit, however comments in gdb/dwarf2read.c say that some
// versions of GCC use the entrypc attribute, so we check that too.
[GoRecv] public static (ж<Entry>, uint64, error) baseAddressForEntry(this ref Data d, ж<Entry> Ꮡe) {
    ref var e = ref Ꮡe.val;

    ж<Entry> cu = default!;
    if (e.Tag == TagCompileUnit){
        cu = e;
    } else {
        nint i = d.offsetToUnit(e.Offset);
        if (i == -1) {
            return (default!, 0, errors.New("no unit for entry"u8));
        }
        var u = Ꮡ(d.unit[i]);
        var b = makeBuf(d, ~u, "info"u8, (~u).off, (~u).data);
        cu = b.entry(nil, (~u).atable, (~u).@base, (~u).vers);
        if (b.err != default!) {
            return (default!, 0, b.err);
        }
    }
    {
        var (cuEntry, cuEntryOK) = cu.Val(AttrEntrypc)._<uint64>(ᐧ); if (cuEntryOK){
            return (cu, cuEntry, default!);
        } else 
        {
            var (cuLow, cuLowOK) = cu.Val(AttrLowpc)._<uint64>(ᐧ); if (cuLowOK) {
                return (cu, cuLow, default!);
            }
        }
    }
    return (cu, 0, default!);
}

[GoRecv] public static (slice<array<uint64>>, error) dwarf2Ranges(this ref Data d, ж<unit> Ꮡu, uint64 @base, int64 ranges, slice<array<uint64>> ret) {
    ref var u = ref Ꮡu.val;

    if (ranges < 0 || ranges > ((int64)len(d.ranges))) {
        return (default!, fmt.Errorf("invalid range offset %d (max %d)"u8, ranges, len(d.ranges)));
    }
    var buf = makeBuf(d, ~u, "ranges"u8, ((Offset)ranges), d.ranges[(int)(ranges)..]);
    while (len(buf.data) > 0) {
        var low = buf.addr();
        var high = buf.addr();
        if (low == 0 && high == 0) {
            break;
        }
        if (low == ^((uint64)0) >> (int)(((nuint)((8 - u.addrsize()) * 8)))){
            @base = high;
        } else {
            ret = append(ret, new uint64[]{@base + low, @base + high}.array());
        }
    }
    return (ret, default!);
}

// dwarf5Ranges interprets a debug_rnglists sequence, see DWARFv5 section
// 2.17.3 (page 53).
[GoRecv] public static (slice<array<uint64>>, error) dwarf5Ranges(this ref Data d, ж<unit> Ꮡu, ж<Entry> Ꮡcu, uint64 @base, int64 ranges, slice<array<uint64>> ret) {
    ref var u = ref Ꮡu.val;
    ref var cu = ref Ꮡcu.val;

    if (ranges < 0 || ranges > ((int64)len(d.rngLists))) {
        return (default!, fmt.Errorf("invalid rnglist offset %d (max %d)"u8, ranges, len(d.ranges)));
    }
    int64 addrBase = default!;
    if (cu != nil) {
        (addrBase, _) = cu.Val(AttrAddrBase)._<int64>(ᐧ);
    }
    var buf = makeBuf(d, ~u, "rnglists"u8, 0, d.rngLists);
    buf.skip(((nint)ranges));
    while (ᐧ) {
        var opcode = buf.uint8();
        switch (opcode) {
        case rleEndOfList: {
            if (buf.err != default!) {
                return (default!, buf.err);
            }
            return (ret, default!);
        }
        case rleBaseAddressx: {
            var baseIdx = buf.@uint();
            error err = default!;
            (@base, err) = d.debugAddr(~u, ((uint64)addrBase), baseIdx);
            if (err != default!) {
                return (default!, err);
            }
            break;
        }
        case rleStartxEndx: {
            var startIdx = buf.@uint();
            var endIdx = buf.@uint();
            var (start, err) = d.debugAddr(~u, ((uint64)addrBase), startIdx);
            if (err != default!) {
                return (default!, err);
            }
            var (end, err) = d.debugAddr(~u, ((uint64)addrBase), endIdx);
            if (err != default!) {
                return (default!, err);
            }
            ret = append(ret, new uint64[]{start, end}.array());
            break;
        }
        case rleStartxLength: {
            var startIdx = buf.@uint();
            var len = buf.@uint();
            var (start, err) = d.debugAddr(~u, ((uint64)addrBase), startIdx);
            if (err != default!) {
                return (default!, err);
            }
            ret = append(ret, new uint64[]{start, start + len}.array());
            break;
        }
        case rleOffsetPair: {
            var off1 = buf.@uint();
            var off2 = buf.@uint();
            ret = append(ret, new uint64[]{@base + off1, @base + off2}.array());
            break;
        }
        case rleBaseAddress: {
            @base = buf.addr();
            break;
        }
        case rleStartEnd: {
            var start = buf.addr();
            var end = buf.addr();
            ret = append(ret, new uint64[]{start, end}.array());
            break;
        }
        case rleStartLength: {
            var start = buf.addr();
            var len = buf.@uint();
            ret = append(ret, new uint64[]{start, start + len}.array());
            break;
        }}

    }
}

// debugAddr returns the address at idx in debug_addr
[GoRecv] internal static (uint64, error) debugAddr(this ref Data d, dataFormat format, uint64 addrBase, uint64 idx) {
    var off = idx * ((uint64)format.addrsize()) + addrBase;
    if (((uint64)((nint)off)) != off) {
        return (0, errors.New("offset out of range"u8));
    }
    var b = makeBuf(d, format, "addr"u8, 0, d.addr);
    b.skip(((nint)off));
    var val = b.addr();
    if (b.err != default!) {
        return (0, b.err);
    }
    return (val, default!);
}

} // end dwarf_package
