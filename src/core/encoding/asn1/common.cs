// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;

partial class asn1_package {

// ASN.1 objects have metadata preceding them:
//   the tag: the type of the object
//   a flag denoting if this object is compound or not
//   the class type: the namespace of the tag
//   the length of the object, in bytes
// Here are some standard tags and classes

// ASN.1 tags represent the type of the following object.
public static readonly UntypedInt TagBoolean = 1;

public static readonly UntypedInt TagInteger = 2;

public static readonly UntypedInt TagBitString = 3;

public static readonly UntypedInt TagOctetString = 4;

public static readonly UntypedInt TagNull = 5;

public static readonly UntypedInt TagOID = 6;

public static readonly UntypedInt TagEnum = 10;

public static readonly UntypedInt TagUTF8String = 12;

public static readonly UntypedInt TagSequence = 16;

public static readonly UntypedInt TagSet = 17;

public static readonly UntypedInt TagNumericString = 18;

public static readonly UntypedInt TagPrintableString = 19;

public static readonly UntypedInt TagT61String = 20;

public static readonly UntypedInt TagIA5String = 22;

public static readonly UntypedInt TagUTCTime = 23;

public static readonly UntypedInt TagGeneralizedTime = 24;

public static readonly UntypedInt TagGeneralString = 27;

public static readonly UntypedInt TagBMPString = 30;

// ASN.1 class types represent the namespace of the tag.
public static readonly UntypedInt ClassUniversal = 0;

public static readonly UntypedInt ClassApplication = 1;

public static readonly UntypedInt ClassContextSpecific = 2;

public static readonly UntypedInt ClassPrivate = 3;

[GoType] partial struct tagAndLength {
    internal nint @class;
    internal nint tag;
    internal nint length;
    internal bool isCompound;
}

// ASN.1 has IMPLICIT and EXPLICIT tags, which can be translated as "instead
// of" and "in addition to". When not specified, every primitive type has a
// default tag in the UNIVERSAL class.
//
// For example: a BIT STRING is tagged [UNIVERSAL 3] by default (although ASN.1
// doesn't actually have a UNIVERSAL keyword). However, by saying [IMPLICIT
// CONTEXT-SPECIFIC 42], that means that the tag is replaced by another.
//
// On the other hand, if it said [EXPLICIT CONTEXT-SPECIFIC 10], then an
// /additional/ tag would wrap the default tag. This explicit tag will have the
// compound flag set.
//
// (This is used in order to remove ambiguity with optional elements.)
//
// You can layer EXPLICIT and IMPLICIT tags to an arbitrary depth, however we
// don't support that here. We support a single layer of EXPLICIT or IMPLICIT
// tagging with tag strings on the fields of a structure.

// fieldParameters is the parsed representation of tag string from a structure field.
[GoType] partial struct fieldParameters {
    internal bool optional;   // true iff the field is OPTIONAL
    internal bool @explicit;   // true iff an EXPLICIT tag is in use.
    internal bool application;   // true iff an APPLICATION tag is in use.
    internal bool @private;   // true iff a PRIVATE tag is in use.
    internal ж<int64> defaultValue; // a default value for INTEGER typed fields (maybe nil).
    internal ж<nint> tag; // the EXPLICIT or IMPLICIT tag (maybe nil).
    internal nint stringType;   // the string tag to use when marshaling.
    internal nint timeType;   // the time tag to use when marshaling.
    internal bool set;   // true iff this should be encoded as a SET
    internal bool omitEmpty;   // true iff this should be omitted if empty when marshaling.
}

// Invariants:
//   if explicit is set, tag is non-nil.

// Given a tag string with the format specified in the package comment,
// parseFieldParameters will parse it into a fieldParameters structure,
// ignoring unknown parts of the string.
internal static fieldParameters /*ret*/ parseFieldParameters(@string str) {
    fieldParameters ret = default!;

    @string part = default!;
    while (len(str) > 0) {
        (part, str, _) = strings.Cut(str, ","u8);
        switch (ᐧ) {
        case {} when part == "optional"u8: {
            ret.optional = true;
            break;
        }
        case {} when part == "explicit"u8: {
            ret.@explicit = true;
            if (ret.tag == nil) {
                ret.tag = @new<nint>();
            }
            break;
        }
        case {} when part == "generalized"u8: {
            ret.timeType = TagGeneralizedTime;
            break;
        }
        case {} when part == "utc"u8: {
            ret.timeType = TagUTCTime;
            break;
        }
        case {} when part == "ia5"u8: {
            ret.stringType = TagIA5String;
            break;
        }
        case {} when part == "printable"u8: {
            ret.stringType = TagPrintableString;
            break;
        }
        case {} when part == "numeric"u8: {
            ret.stringType = TagNumericString;
            break;
        }
        case {} when part == "utf8"u8: {
            ret.stringType = TagUTF8String;
            break;
        }
        case {} when strings.HasPrefix(part, "default:"u8): {
            var (i, err) = strconv.ParseInt(part[8..], 10, 64);
            if (err == default!) {
                ret.defaultValue = @new<int64>();
                ret.defaultValue.val = i;
            }
            break;
        }
        case {} when strings.HasPrefix(part, "tag:"u8): {
            var (i, err) = strconv.Atoi(part[4..]);
            if (err == default!) {
                ret.tag = @new<nint>();
                ret.tag.val = i;
            }
            break;
        }
        case {} when part == "set"u8: {
            ret.set = true;
            break;
        }
        case {} when part == "application"u8: {
            ret.application = true;
            if (ret.tag == nil) {
                ret.tag = @new<nint>();
            }
            break;
        }
        case {} when part == "private"u8: {
            ret.@private = true;
            if (ret.tag == nil) {
                ret.tag = @new<nint>();
            }
            break;
        }
        case {} when part == "omitempty"u8: {
            ret.omitEmpty = true;
            break;
        }}

    }
    return ret;
}

// Given a reflected Go type, getUniversalType returns the default tag number
// and expected compound flag.
internal static (bool matchAny, nint tagNumber, bool isCompound, bool ok) getUniversalType(reflectꓸType t) {
    bool matchAny = default!;
    nint tagNumber = default!;
    bool isCompound = default!;
    bool ok = default!;

    var exprᴛ1 = t;
    if (exprᴛ1 == rawValueType) {
        return (true, -1, false, true);
    }
    if (exprᴛ1 == objectIdentifierType) {
        return (false, TagOID, false, true);
    }
    if (exprᴛ1 == bitStringType) {
        return (false, TagBitString, false, true);
    }
    if (exprᴛ1 == timeType) {
        return (false, TagUTCTime, false, true);
    }
    if (exprᴛ1 == enumeratedType) {
        return (false, TagEnum, false, true);
    }
    if (exprᴛ1 == bigIntType) {
        return (false, TagInteger, false, true);
    }

    var exprᴛ2 = t.Kind();
    if (exprᴛ2 == reflect.ΔBool) {
        return (false, TagBoolean, false, true);
    }
    if (exprᴛ2 == reflect.ΔInt || exprᴛ2 == reflect.Int8 || exprᴛ2 == reflect.Int16 || exprᴛ2 == reflect.Int32 || exprᴛ2 == reflect.Int64) {
        return (false, TagInteger, false, true);
    }
    if (exprᴛ2 == reflect.Struct) {
        return (false, TagSequence, true, true);
    }
    if (exprᴛ2 == reflect.ΔSlice) {
        if (t.Elem().Kind() == reflect.Uint8) {
            return (false, TagOctetString, false, true);
        }
        if (strings.HasSuffix(t.Name(), "SET"u8)) {
            return (false, TagSet, true, true);
        }
        return (false, TagSequence, true, true);
    }
    if (exprᴛ2 == reflect.ΔString) {
        return (false, TagPrintableString, false, true);
    }

    return (false, 0, false, false);
}

} // end asn1_package
