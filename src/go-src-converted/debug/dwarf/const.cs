// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Constants
namespace go.debug;

partial class dwarf_package {

[GoType("num:uint32")] partial struct Attr;

//go:generate stringer -type Attr -trimprefix=Attr
public static readonly Attr AttrSibling = 0x01;
public static readonly Attr AttrLocation = 0x02;
public static readonly Attr AttrName = 0x03;
public static readonly Attr AttrOrdering = 0x09;
public static readonly Attr AttrByteSize = 0x0B;
public static readonly Attr AttrBitOffset = 0x0C;
public static readonly Attr AttrBitSize = 0x0D;
public static readonly Attr AttrStmtList = 0x10;
public static readonly Attr AttrLowpc = 0x11;
public static readonly Attr AttrHighpc = 0x12;
public static readonly Attr AttrLanguage = 0x13;
public static readonly Attr AttrDiscr = 0x15;
public static readonly Attr AttrDiscrValue = 0x16;
public static readonly Attr AttrVisibility = 0x17;
public static readonly Attr AttrImport = 0x18;
public static readonly Attr AttrStringLength = 0x19;
public static readonly Attr AttrCommonRef = 0x1A;
public static readonly Attr AttrCompDir = 0x1B;
public static readonly Attr AttrConstValue = 0x1C;
public static readonly Attr AttrContainingType = 0x1D;
public static readonly Attr AttrDefaultValue = 0x1E;
public static readonly Attr AttrInline = 0x20;
public static readonly Attr AttrIsOptional = 0x21;
public static readonly Attr AttrLowerBound = 0x22;
public static readonly Attr AttrProducer = 0x25;
public static readonly Attr AttrPrototyped = 0x27;
public static readonly Attr AttrReturnAddr = 0x2A;
public static readonly Attr AttrStartScope = 0x2C;
public static readonly Attr AttrStrideSize = 0x2E;
public static readonly Attr AttrUpperBound = 0x2F;
public static readonly Attr AttrAbstractOrigin = 0x31;
public static readonly Attr AttrAccessibility = 0x32;
public static readonly Attr AttrAddrClass = 0x33;
public static readonly Attr AttrArtificial = 0x34;
public static readonly Attr AttrBaseTypes = 0x35;
public static readonly Attr AttrCalling = 0x36;
public static readonly Attr AttrCount = 0x37;
public static readonly Attr AttrDataMemberLoc = 0x38;
public static readonly Attr AttrDeclColumn = 0x39;
public static readonly Attr AttrDeclFile = 0x3A;
public static readonly Attr AttrDeclLine = 0x3B;
public static readonly Attr AttrDeclaration = 0x3C;
public static readonly Attr AttrDiscrList = 0x3D;
public static readonly Attr AttrEncoding = 0x3E;
public static readonly Attr AttrExternal = 0x3F;
public static readonly Attr AttrFrameBase = 0x40;
public static readonly Attr AttrFriend = 0x41;
public static readonly Attr AttrIdentifierCase = 0x42;
public static readonly Attr AttrMacroInfo = 0x43;
public static readonly Attr AttrNamelistItem = 0x44;
public static readonly Attr AttrPriority = 0x45;
public static readonly Attr AttrSegment = 0x46;
public static readonly Attr AttrSpecification = 0x47;
public static readonly Attr AttrStaticLink = 0x48;
public static readonly Attr AttrType = 0x49;
public static readonly Attr AttrUseLocation = 0x4A;
public static readonly Attr AttrVarParam = 0x4B;
public static readonly Attr AttrVirtuality = 0x4C;
public static readonly Attr AttrVtableElemLoc = 0x4D;
public static readonly Attr AttrAllocated = 0x4E;
public static readonly Attr AttrAssociated = 0x4F;
public static readonly Attr AttrDataLocation = 0x50;
public static readonly Attr AttrStride = 0x51;
public static readonly Attr AttrEntrypc = 0x52;
public static readonly Attr AttrUseUTF8 = 0x53;
public static readonly Attr AttrExtension = 0x54;
public static readonly Attr AttrRanges = 0x55;
public static readonly Attr AttrTrampoline = 0x56;
public static readonly Attr AttrCallColumn = 0x57;
public static readonly Attr AttrCallFile = 0x58;
public static readonly Attr AttrCallLine = 0x59;
public static readonly Attr AttrDescription = 0x5A;
public static readonly Attr AttrBinaryScale = 0x5B;
public static readonly Attr AttrDecimalScale = 0x5C;
public static readonly Attr AttrSmall = 0x5D;
public static readonly Attr AttrDecimalSign = 0x5E;
public static readonly Attr AttrDigitCount = 0x5F;
public static readonly Attr AttrPictureString = 0x60;
public static readonly Attr AttrMutable = 0x61;
public static readonly Attr AttrThreadsScaled = 0x62;
public static readonly Attr AttrExplicit = 0x63;
public static readonly Attr AttrObjectPointer = 0x64;
public static readonly Attr AttrEndianity = 0x65;
public static readonly Attr AttrElemental = 0x66;
public static readonly Attr AttrPure = 0x67;
public static readonly Attr AttrRecursive = 0x68;
public static readonly Attr AttrSignature = 0x69;
public static readonly Attr AttrMainSubprogram = 0x6A;
public static readonly Attr AttrDataBitOffset = 0x6B;
public static readonly Attr AttrConstExpr = 0x6C;
public static readonly Attr AttrEnumClass = 0x6D;
public static readonly Attr AttrLinkageName = 0x6E;
public static readonly Attr AttrStringLengthBitSize = 0x6F;
public static readonly Attr AttrStringLengthByteSize = 0x70;
public static readonly Attr AttrRank = 0x71;
public static readonly Attr AttrStrOffsetsBase = 0x72;
public static readonly Attr AttrAddrBase = 0x73;
public static readonly Attr AttrRnglistsBase = 0x74;
public static readonly Attr AttrDwoName = 0x76;
public static readonly Attr AttrReference = 0x77;
public static readonly Attr AttrRvalueReference = 0x78;
public static readonly Attr AttrMacros = 0x79;
public static readonly Attr AttrCallAllCalls = 0x7A;
public static readonly Attr AttrCallAllSourceCalls = 0x7B;
public static readonly Attr AttrCallAllTailCalls = 0x7C;
public static readonly Attr AttrCallReturnPC = 0x7D;
public static readonly Attr AttrCallValue = 0x7E;
public static readonly Attr AttrCallOrigin = 0x7F;
public static readonly Attr AttrCallParameter = 0x80;
public static readonly Attr AttrCallPC = 0x81;
public static readonly Attr AttrCallTailCall = 0x82;
public static readonly Attr AttrCallTarget = 0x83;
public static readonly Attr AttrCallTargetClobbered = 0x84;
public static readonly Attr AttrCallDataLocation = 0x85;
public static readonly Attr AttrCallDataValue = 0x86;
public static readonly Attr AttrNoreturn = 0x87;
public static readonly Attr AttrAlignment = 0x88;
public static readonly Attr AttrExportSymbols = 0x89;
public static readonly Attr AttrDeleted = 0x8A;
public static readonly Attr AttrDefaulted = 0x8B;
public static readonly Attr AttrLoclistsBase = 0x8C;

public static @string GoString(this Attr a) {
    {
        var (str, ok) = _Attr_map[a, ꟷ]; if (ok) {
            return "dwarf.Attr"u8 + str;
        }
    }
    return "dwarf."u8 + a.String();
}

[GoType("num:uint32")] partial struct format;

internal static readonly format formAddr = 0x01;
internal static readonly format formDwarfBlock2 = 0x03;
internal static readonly format formDwarfBlock4 = 0x04;
internal static readonly format formData2 = 0x05;
internal static readonly format formData4 = 0x06;
internal static readonly format formData8 = 0x07;
internal static readonly format formString = 0x08;
internal static readonly format formDwarfBlock = 0x09;
internal static readonly format formDwarfBlock1 = 0x0A;
internal static readonly format formData1 = 0x0B;
internal static readonly format formFlag = 0x0C;
internal static readonly format formSdata = 0x0D;
internal static readonly format formStrp = 0x0E;
internal static readonly format formUdata = 0x0F;
internal static readonly format formRefAddr = 0x10;
internal static readonly format formRef1 = 0x11;
internal static readonly format formRef2 = 0x12;
internal static readonly format formRef4 = 0x13;
internal static readonly format formRef8 = 0x14;
internal static readonly format formRefUdata = 0x15;
internal static readonly format formIndirect = 0x16;
internal static readonly format formSecOffset = 0x17;
internal static readonly format formExprloc = 0x18;
internal static readonly format formFlagPresent = 0x19;
internal static readonly format formRefSig8 = 0x20;
internal static readonly format formStrx = 0x1A;
internal static readonly format formAddrx = 0x1B;
internal static readonly format formRefSup4 = 0x1C;
internal static readonly format formStrpSup = 0x1D;
internal static readonly format formData16 = 0x1E;
internal static readonly format formLineStrp = 0x1F;
internal static readonly format formImplicitConst = 0x21;
internal static readonly format formLoclistx = 0x22;
internal static readonly format formRnglistx = 0x23;
internal static readonly format formRefSup8 = 0x24;
internal static readonly format formStrx1 = 0x25;
internal static readonly format formStrx2 = 0x26;
internal static readonly format formStrx3 = 0x27;
internal static readonly format formStrx4 = 0x28;
internal static readonly format formAddrx1 = 0x29;
internal static readonly format formAddrx2 = 0x2A;
internal static readonly format formAddrx3 = 0x2B;
internal static readonly format formAddrx4 = 0x2C;
internal static readonly format formGnuRefAlt = 0x1f20;
internal static readonly format formGnuStrpAlt = 0x1f21;

[GoType("num:uint32")] partial struct Tag;

//go:generate stringer -type Tag -trimprefix=Tag
public static readonly Tag TagArrayType = 0x01;
public static readonly Tag TagClassType = 0x02;
public static readonly Tag TagEntryPoint = 0x03;
public static readonly Tag TagEnumerationType = 0x04;
public static readonly Tag TagFormalParameter = 0x05;
public static readonly Tag TagImportedDeclaration = 0x08;
public static readonly Tag TagLabel = 0x0A;
public static readonly Tag TagLexDwarfBlock = 0x0B;
public static readonly Tag TagMember = 0x0D;
public static readonly Tag TagPointerType = 0x0F;
public static readonly Tag TagReferenceType = 0x10;
public static readonly Tag TagCompileUnit = 0x11;
public static readonly Tag TagStringType = 0x12;
public static readonly Tag TagStructType = 0x13;
public static readonly Tag TagSubroutineType = 0x15;
public static readonly Tag TagTypedef = 0x16;
public static readonly Tag TagUnionType = 0x17;
public static readonly Tag TagUnspecifiedParameters = 0x18;
public static readonly Tag TagVariant = 0x19;
public static readonly Tag TagCommonDwarfBlock = 0x1A;
public static readonly Tag TagCommonInclusion = 0x1B;
public static readonly Tag TagInheritance = 0x1C;
public static readonly Tag TagInlinedSubroutine = 0x1D;
public static readonly Tag TagModule = 0x1E;
public static readonly Tag TagPtrToMemberType = 0x1F;
public static readonly Tag TagSetType = 0x20;
public static readonly Tag TagSubrangeType = 0x21;
public static readonly Tag TagWithStmt = 0x22;
public static readonly Tag TagAccessDeclaration = 0x23;
public static readonly Tag TagBaseType = 0x24;
public static readonly Tag TagCatchDwarfBlock = 0x25;
public static readonly Tag TagConstType = 0x26;
public static readonly Tag TagConstant = 0x27;
public static readonly Tag TagEnumerator = 0x28;
public static readonly Tag TagFileType = 0x29;
public static readonly Tag TagFriend = 0x2A;
public static readonly Tag TagNamelist = 0x2B;
public static readonly Tag TagNamelistItem = 0x2C;
public static readonly Tag TagPackedType = 0x2D;
public static readonly Tag TagSubprogram = 0x2E;
public static readonly Tag TagTemplateTypeParameter = 0x2F;
public static readonly Tag TagTemplateValueParameter = 0x30;
public static readonly Tag TagThrownType = 0x31;
public static readonly Tag TagTryDwarfBlock = 0x32;
public static readonly Tag TagVariantPart = 0x33;
public static readonly Tag TagVariable = 0x34;
public static readonly Tag TagVolatileType = 0x35;
public static readonly Tag TagDwarfProcedure = 0x36;
public static readonly Tag TagRestrictType = 0x37;
public static readonly Tag TagInterfaceType = 0x38;
public static readonly Tag TagNamespace = 0x39;
public static readonly Tag TagImportedModule = 0x3A;
public static readonly Tag TagUnspecifiedType = 0x3B;
public static readonly Tag TagPartialUnit = 0x3C;
public static readonly Tag TagImportedUnit = 0x3D;
public static readonly Tag TagMutableType = 0x3E;      // Later removed from DWARF.
public static readonly Tag TagCondition = 0x3F;
public static readonly Tag TagSharedType = 0x40;
public static readonly Tag TagTypeUnit = 0x41;
public static readonly Tag TagRvalueReferenceType = 0x42;
public static readonly Tag TagTemplateAlias = 0x43;
public static readonly Tag TagCoarrayType = 0x44;
public static readonly Tag TagGenericSubrange = 0x45;
public static readonly Tag TagDynamicType = 0x46;
public static readonly Tag TagAtomicType = 0x47;
public static readonly Tag TagCallSite = 0x48;
public static readonly Tag TagCallSiteParameter = 0x49;
public static readonly Tag TagSkeletonUnit = 0x4A;
public static readonly Tag TagImmutableType = 0x4B;

public static @string GoString(this Tag t) {
    if (t <= TagTemplateAlias) {
        return "dwarf.Tag"u8 + t.String();
    }
    return "dwarf."u8 + t.String();
}

// Location expression operators.
// The debug info encodes value locations like 8(R3)
// as a sequence of these op codes.
// This package does not implement full expressions;
// the opPlusUconst operator is expected by the type parser.
internal static readonly UntypedInt opAddr = 0x03; /* 1 op, const addr */

internal static readonly UntypedInt opDeref = 0x06;

internal static readonly UntypedInt opConst1u = 0x08; /* 1 op, 1 byte const */

internal static readonly UntypedInt opConst1s = 0x09; /*	" signed */

internal static readonly UntypedInt opConst2u = 0x0A; /* 1 op, 2 byte const  */

internal static readonly UntypedInt opConst2s = 0x0B; /*	" signed */

internal static readonly UntypedInt opConst4u = 0x0C; /* 1 op, 4 byte const */

internal static readonly UntypedInt opConst4s = 0x0D; /*	" signed */

internal static readonly UntypedInt opConst8u = 0x0E; /* 1 op, 8 byte const */

internal static readonly UntypedInt opConst8s = 0x0F; /*	" signed */

internal static readonly UntypedInt opConstu = 0x10; /* 1 op, LEB128 const */

internal static readonly UntypedInt opConsts = 0x11; /*	" signed */

internal static readonly UntypedInt opDup = 0x12;

internal static readonly UntypedInt opDrop = 0x13;

internal static readonly UntypedInt opOver = 0x14;

internal static readonly UntypedInt opPick = 0x15; /* 1 op, 1 byte stack index */

internal static readonly UntypedInt opSwap = 0x16;

internal static readonly UntypedInt opRot = 0x17;

internal static readonly UntypedInt opXderef = 0x18;

internal static readonly UntypedInt opAbs = 0x19;

internal static readonly UntypedInt opAnd = 0x1A;

internal static readonly UntypedInt opDiv = 0x1B;

internal static readonly UntypedInt opMinus = 0x1C;

internal static readonly UntypedInt opMod = 0x1D;

internal static readonly UntypedInt opMul = 0x1E;

internal static readonly UntypedInt opNeg = 0x1F;

internal static readonly UntypedInt opNot = 0x20;

internal static readonly UntypedInt opOr = 0x21;

internal static readonly UntypedInt opPlus = 0x22;

internal static readonly UntypedInt opPlusUconst = 0x23; /* 1 op, ULEB128 addend */

internal static readonly UntypedInt opShl = 0x24;

internal static readonly UntypedInt opShr = 0x25;

internal static readonly UntypedInt opShra = 0x26;

internal static readonly UntypedInt opXor = 0x27;

internal static readonly UntypedInt opSkip = 0x2F; /* 1 op, signed 2-byte constant */

internal static readonly UntypedInt opBra = 0x28; /* 1 op, signed 2-byte constant */

internal static readonly UntypedInt opEq = 0x29;

internal static readonly UntypedInt opGe = 0x2A;

internal static readonly UntypedInt opGt = 0x2B;

internal static readonly UntypedInt opLe = 0x2C;

internal static readonly UntypedInt opLt = 0x2D;

internal static readonly UntypedInt opNe = 0x2E;

internal static readonly UntypedInt opLit0 = 0x30;

internal static readonly UntypedInt opReg0 = 0x50;

internal static readonly UntypedInt opBreg0 = 0x70; /* 1 op, signed LEB128 constant */

internal static readonly UntypedInt opRegx = 0x90; /* 1 op, ULEB128 register */

internal static readonly UntypedInt opFbreg = 0x91; /* 1 op, SLEB128 offset */

internal static readonly UntypedInt opBregx = 0x92; /* 2 op, ULEB128 reg; SLEB128 off */

internal static readonly UntypedInt opPiece = 0x93; /* 1 op, ULEB128 size of piece */

internal static readonly UntypedInt opDerefSize = 0x94; /* 1-byte size of data retrieved */

internal static readonly UntypedInt opXderefSize = 0x95; /* 1-byte size of data retrieved */

internal static readonly UntypedInt opNop = 0x96;

internal static readonly UntypedInt opPushObjAddr = 0x97;

internal static readonly UntypedInt opCall2 = 0x98; /* 2-byte offset of DIE */

internal static readonly UntypedInt opCall4 = 0x99; /* 4-byte offset of DIE */

internal static readonly UntypedInt opCallRef = 0x9A; /* 4- or 8- byte offset of DIE */

internal static readonly UntypedInt opFormTLSAddress = 0x9B;

internal static readonly UntypedInt opCallFrameCFA = 0x9C;

internal static readonly UntypedInt opBitPiece = 0x9D;

internal static readonly UntypedInt opImplicitValue = 0x9E;

internal static readonly UntypedInt opStackValue = 0x9F;

internal static readonly UntypedInt opImplicitPointer = 0xA0;

internal static readonly UntypedInt opAddrx = 0xA1;

internal static readonly UntypedInt opConstx = 0xA2;

internal static readonly UntypedInt opEntryValue = 0xA3;

internal static readonly UntypedInt opConstType = 0xA4;

internal static readonly UntypedInt opRegvalType = 0xA5;

internal static readonly UntypedInt opDerefType = 0xA6;

internal static readonly UntypedInt opXderefType = 0xA7;

internal static readonly UntypedInt opConvert = 0xA8;

internal static readonly UntypedInt opReinterpret = 0xA9;

/* 0xE0-0xFF reserved for user-specific */

// Basic type encodings -- the value for AttrEncoding in a TagBaseType Entry.
internal static readonly UntypedInt encAddress = 0x01;

internal static readonly UntypedInt encBoolean = 0x02;

internal static readonly UntypedInt encComplexFloat = 0x03;

internal static readonly UntypedInt encFloat = 0x04;

internal static readonly UntypedInt encSigned = 0x05;

internal static readonly UntypedInt encSignedChar = 0x06;

internal static readonly UntypedInt encUnsigned = 0x07;

internal static readonly UntypedInt encUnsignedChar = 0x08;

internal static readonly UntypedInt encImaginaryFloat = 0x09;

internal static readonly UntypedInt encPackedDecimal = 0x0A;

internal static readonly UntypedInt encNumericString = 0x0B;

internal static readonly UntypedInt encEdited = 0x0C;

internal static readonly UntypedInt encSignedFixed = 0x0D;

internal static readonly UntypedInt encUnsignedFixed = 0x0E;

internal static readonly UntypedInt encDecimalFloat = 0x0F;

internal static readonly UntypedInt encUTF = 0x10;

internal static readonly UntypedInt encUCS = 0x11;

internal static readonly UntypedInt encASCII = 0x12;

// Statement program standard opcode encodings.
internal static readonly UntypedInt lnsCopy = 1;

internal static readonly UntypedInt lnsAdvancePC = 2;

internal static readonly UntypedInt lnsAdvanceLine = 3;

internal static readonly UntypedInt lnsSetFile = 4;

internal static readonly UntypedInt lnsSetColumn = 5;

internal static readonly UntypedInt lnsNegateStmt = 6;

internal static readonly UntypedInt lnsSetBasicBlock = 7;

internal static readonly UntypedInt lnsConstAddPC = 8;

internal static readonly UntypedInt lnsFixedAdvancePC = 9;

internal static readonly UntypedInt lnsSetPrologueEnd = 10;

internal static readonly UntypedInt lnsSetEpilogueBegin = 11;

internal static readonly UntypedInt lnsSetISA = 12;

// Statement program extended opcode encodings.
internal static readonly UntypedInt lneEndSequence = 1;

internal static readonly UntypedInt lneSetAddress = 2;

internal static readonly UntypedInt lneDefineFile = 3;

internal static readonly UntypedInt lneSetDiscriminator = 4;

// Line table directory and file name entry formats.
// These are new in DWARF 5.
internal static readonly UntypedInt lnctPath = 0x01;

internal static readonly UntypedInt lnctDirectoryIndex = 0x02;

internal static readonly UntypedInt lnctTimestamp = 0x03;

internal static readonly UntypedInt lnctSize = 0x04;

internal static readonly UntypedInt lnctMD5 = 0x05;

// Location list entry codes.
// These are new in DWARF 5.
internal static readonly UntypedInt lleEndOfList = 0x00;

internal static readonly UntypedInt lleBaseAddressx = 0x01;

internal static readonly UntypedInt lleStartxEndx = 0x02;

internal static readonly UntypedInt lleStartxLength = 0x03;

internal static readonly UntypedInt lleOffsetPair = 0x04;

internal static readonly UntypedInt lleDefaultLocation = 0x05;

internal static readonly UntypedInt lleBaseAddress = 0x06;

internal static readonly UntypedInt lleStartEnd = 0x07;

internal static readonly UntypedInt lleStartLength = 0x08;

// Unit header unit type encodings.
// These are new in DWARF 5.
internal static readonly UntypedInt utCompile = 0x01;

internal static readonly UntypedInt utType = 0x02;

internal static readonly UntypedInt utPartial = 0x03;

internal static readonly UntypedInt utSkeleton = 0x04;

internal static readonly UntypedInt utSplitCompile = 0x05;

internal static readonly UntypedInt utSplitType = 0x06;

// Opcodes for DWARFv5 debug_rnglists section.
internal static readonly UntypedInt rleEndOfList = 0x0;

internal static readonly UntypedInt rleBaseAddressx = 0x1;

internal static readonly UntypedInt rleStartxEndx = 0x2;

internal static readonly UntypedInt rleStartxLength = 0x3;

internal static readonly UntypedInt rleOffsetPair = 0x4;

internal static readonly UntypedInt rleBaseAddress = 0x5;

internal static readonly UntypedInt rleStartEnd = 0x6;

internal static readonly UntypedInt rleStartLength = 0x7;

} // end dwarf_package
