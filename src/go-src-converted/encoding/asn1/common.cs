// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package asn1 -- go2cs converted at 2020 August 29 08:29:46 UTC
// import "encoding/asn1" ==> using asn1 = go.encoding.asn1_package
// Original source: C:\Go\src\encoding\asn1\common.go
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class asn1_package
    {
        // ASN.1 objects have metadata preceding them:
        //   the tag: the type of the object
        //   a flag denoting if this object is compound or not
        //   the class type: the namespace of the tag
        //   the length of the object, in bytes

        // Here are some standard tags and classes

        // ASN.1 tags represent the type of the following object.
        public static readonly long TagBoolean = 1L;
        public static readonly long TagInteger = 2L;
        public static readonly long TagBitString = 3L;
        public static readonly long TagOctetString = 4L;
        public static readonly long TagNull = 5L;
        public static readonly long TagOID = 6L;
        public static readonly long TagEnum = 10L;
        public static readonly long TagUTF8String = 12L;
        public static readonly long TagSequence = 16L;
        public static readonly long TagSet = 17L;
        public static readonly long TagNumericString = 18L;
        public static readonly long TagPrintableString = 19L;
        public static readonly long TagT61String = 20L;
        public static readonly long TagIA5String = 22L;
        public static readonly long TagUTCTime = 23L;
        public static readonly long TagGeneralizedTime = 24L;
        public static readonly long TagGeneralString = 27L;

        // ASN.1 class types represent the namespace of the tag.
        public static readonly long ClassUniversal = 0L;
        public static readonly long ClassApplication = 1L;
        public static readonly long ClassContextSpecific = 2L;
        public static readonly long ClassPrivate = 3L;

        private partial struct tagAndLength
        {
            public long @class;
            public long tag;
            public long length;
            public bool isCompound;
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
        private partial struct fieldParameters
        {
            public bool optional; // true iff the field is OPTIONAL
            public bool @explicit; // true iff an EXPLICIT tag is in use.
            public bool application; // true iff an APPLICATION tag is in use.
            public ptr<long> defaultValue; // a default value for INTEGER typed fields (maybe nil).
            public ptr<long> tag; // the EXPLICIT or IMPLICIT tag (maybe nil).
            public long stringType; // the string tag to use when marshaling.
            public long timeType; // the time tag to use when marshaling.
            public bool set; // true iff this should be encoded as a SET
            public bool omitEmpty; // true iff this should be omitted if empty when marshaling.

// Invariants:
//   if explicit is set, tag is non-nil.
        }

        // Given a tag string with the format specified in the package comment,
        // parseFieldParameters will parse it into a fieldParameters structure,
        // ignoring unknown parts of the string.
        private static fieldParameters parseFieldParameters(@string str)
        {
            foreach (var (_, part) in strings.Split(str, ","))
            {

                if (part == "optional") 
                    ret.optional = true;
                else if (part == "explicit") 
                    ret.@explicit = true;
                    if (ret.tag == null)
                    {
                        ret.tag = @new<int>();
                    }
                else if (part == "generalized") 
                    ret.timeType = TagGeneralizedTime;
                else if (part == "utc") 
                    ret.timeType = TagUTCTime;
                else if (part == "ia5") 
                    ret.stringType = TagIA5String;
                else if (part == "printable") 
                    ret.stringType = TagPrintableString;
                else if (part == "numeric") 
                    ret.stringType = TagNumericString;
                else if (part == "utf8") 
                    ret.stringType = TagUTF8String;
                else if (strings.HasPrefix(part, "default:")) 
                    var (i, err) = strconv.ParseInt(part[8L..], 10L, 64L);
                    if (err == null)
                    {
                        ret.defaultValue = @new<int64>();
                        ret.defaultValue.Value = i;
                    }
                else if (strings.HasPrefix(part, "tag:")) 
                    (i, err) = strconv.Atoi(part[4L..]);
                    if (err == null)
                    {
                        ret.tag = @new<int>();
                        ret.tag.Value = i;
                    }
                else if (part == "set") 
                    ret.set = true;
                else if (part == "application") 
                    ret.application = true;
                    if (ret.tag == null)
                    {
                        ret.tag = @new<int>();
                    }
                else if (part == "omitempty") 
                    ret.omitEmpty = true;
                            }
            return;
        }

        // Given a reflected Go type, getUniversalType returns the default tag number
        // and expected compound flag.
        private static (bool, long, bool, bool) getUniversalType(reflect.Type t)
        {

            if (t == rawValueType) 
                return (true, -1L, false, true);
            else if (t == objectIdentifierType) 
                return (false, TagOID, false, true);
            else if (t == bitStringType) 
                return (false, TagBitString, false, true);
            else if (t == timeType) 
                return (false, TagUTCTime, false, true);
            else if (t == enumeratedType) 
                return (false, TagEnum, false, true);
            else if (t == bigIntType) 
                return (false, TagInteger, false, true);
            
            if (t.Kind() == reflect.Bool) 
                return (false, TagBoolean, false, true);
            else if (t.Kind() == reflect.Int || t.Kind() == reflect.Int8 || t.Kind() == reflect.Int16 || t.Kind() == reflect.Int32 || t.Kind() == reflect.Int64) 
                return (false, TagInteger, false, true);
            else if (t.Kind() == reflect.Struct) 
                return (false, TagSequence, true, true);
            else if (t.Kind() == reflect.Slice) 
                if (t.Elem().Kind() == reflect.Uint8)
                {
                    return (false, TagOctetString, false, true);
                }
                if (strings.HasSuffix(t.Name(), "SET"))
                {
                    return (false, TagSet, true, true);
                }
                return (false, TagSequence, true, true);
            else if (t.Kind() == reflect.String) 
                return (false, TagPrintableString, false, true);
                        return (false, 0L, false, false);
        }
    }
}}
