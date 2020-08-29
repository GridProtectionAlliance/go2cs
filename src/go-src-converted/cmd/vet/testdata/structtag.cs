// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for canonical struct tags.

// package testdata -- go2cs converted at 2020 August 29 10:10:39 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\structtag.go
using xml = go.encoding.xml_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public partial struct StructTagTest
        {
            [Description("hello")]
            public long A; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag pair"
            [Description("\tx:\"y\"")]
            public long B; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag key"
            [Description("x:\"y\"\tx:\"y\"")]
            public long C; // ERROR "not compatible with reflect.StructTag.Get"
            [Description("x:`y`")]
            public long D; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag value"
            [Description("ct\brl:\"char\"")]
            public long E; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag pair"
            [Description(":\"emptykey\"")]
            public long F; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag key"
            [Description("x:\"noEndQuote")]
            public long G; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag value"
            [Description("x:\"trunc\\x0\"")]
            public long H; // ERROR "not compatible with reflect.StructTag.Get: bad syntax for struct tag value"
            [Description("x:\"foo\",y:\"bar\"")]
            public long I; // ERROR "not compatible with reflect.StructTag.Get: key:.value. pairs not separated by spaces"
            [Description("x:\"foo\"y:\"bar\"")]
            public long J; // ERROR "not compatible with reflect.StructTag.Get: key:.value. pairs not separated by spaces"
            [Description("x:\"y\" u:\"v\" w:\"\"")]
            public long OK0;
            [Description("x:\"y:z\" u:\"v\" w:\"\"")]
            public long OK1; // note multiple colons.
            [Description("k0:\"values contain spaces\" k1:\"literal\ttabs\" k2:\"and\\tescaped\\tabs\"")]
            public long OK2;
            [Description("under_scores:\"and\" CAPS:\"ARE_OK\"")]
            public long OK3;
        }

        public partial struct UnexportedEncodingTagTest
        {
            [Description("json:\"xx\"")]
            public long x; // ERROR "struct field x has json tag but is not exported"
            [Description("xml:\"yy\"")]
            public long y; // ERROR "struct field y has xml tag but is not exported"
            public long z;
            [Description("json:\"aa\" xml:\"bb\"")]
            public long A;
        }

        private partial struct unexp
        {
        }

        public partial struct JSONEmbeddedField
        {
            [Description("is:\"embedded\"")]
            public ref UnexportedEncodingTagTest UnexportedEncodingTagTest => ref UnexportedEncodingTagTest_val;
            [Description("is:\"embedded,notexported\" json:\"unexp\"")]
            public ref unexp unexp => ref unexp_val; // OK for now, see issue 7363
        }

        public partial struct AnonymousJSON
        {
        }
        public partial struct AnonymousXML
        {
        }

        public partial struct DuplicateJSONFields
        {
            [Description("json:\"a\"")]
            public long JSON;
            [Description("json:\"a\"")]
            public long DuplicateJSON; // ERROR "struct field DuplicateJSON repeats json tag .a. also at testdata/structtag.go:46"
            [Description("json:\"-\"")]
            public long IgnoredJSON;
            [Description("json:\"-\"")]
            public long OtherIgnoredJSON;
            [Description("json:\",omitempty\"")]
            public long OmitJSON;
            [Description("json:\",omitempty\"")]
            public long OtherOmitJSON;
            [Description("json:\"a,omitempty\"")]
            public long DuplicateOmitJSON; // ERROR "struct field DuplicateOmitJSON repeats json tag .a. also at testdata/structtag.go:46"
            [Description("foo:\"a\"")]
            public long NonJSON;
            [Description("foo:\"a\"")]
            public long DuplicateNonJSON;
            [Description("json:\"a\"")]
            public ref AnonymousJSON AnonymousJSON => ref AnonymousJSON_val; // ERROR "struct field AnonymousJSON repeats json tag .a. also at testdata/structtag.go:46"

            [Description("xml:\"a\"")]
            public long XML;
            [Description("xml:\"a\"")]
            public long DuplicateXML; // ERROR "struct field DuplicateXML repeats xml tag .a. also at testdata/structtag.go:60"
            [Description("xml:\"-\"")]
            public long IgnoredXML;
            [Description("xml:\"-\"")]
            public long OtherIgnoredXML;
            [Description("xml:\",omitempty\"")]
            public long OmitXML;
            [Description("xml:\",omitempty\"")]
            public long OtherOmitXML;
            [Description("xml:\"a,omitempty\"")]
            public long DuplicateOmitXML; // ERROR "struct field DuplicateOmitXML repeats xml tag .a. also at testdata/structtag.go:60"
            [Description("foo:\"a\"")]
            public long NonXML;
            [Description("foo:\"a\"")]
            public long DuplicateNonXML;
            [Description("xml:\"a\"")]
            public ref AnonymousXML AnonymousXML => ref AnonymousXML_val; // ERROR "struct field AnonymousXML repeats xml tag .a. also at testdata/structtag.go:60"
        }

        public partial struct UnexpectedSpacetest
        {
            [Description("json:\"a,omitempty\"")]
            public long A;
            [Description("json:\"b, omitempty\"")]
            public long B; // ERROR "suspicious space in struct tag value"
            [Description("json:\"c ,omitempty\"")]
            public long C;
            [Description("json:\"d,omitempty, string\"")]
            public long D; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"e local\"")]
            public long E;
            [Description("xml:\"f \"")]
            public long F; // ERROR "suspicious space in struct tag value"
            [Description("xml:\" g\"")]
            public long G; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"h ,omitempty\"")]
            public long H; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"i, omitempty\"")]
            public long I; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"j local ,omitempty\"")]
            public long J; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"k local, omitempty\"")]
            public long K; // ERROR "suspicious space in struct tag value"
            [Description("xml:\" l local,omitempty\"")]
            public long L; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"m  local,omitempty\"")]
            public long M; // ERROR "suspicious space in struct tag value"
            [Description("xml:\" \"")]
            public long N; // ERROR "suspicious space in struct tag value"
            [Description("xml:\"\"")]
            public long O;
            [Description("xml:\",\"")]
            public long P;
            [Description("foo:\" doesn\'t care \"")]
            public long Q;
        }
    }
}}}
