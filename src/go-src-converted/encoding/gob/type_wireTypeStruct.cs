//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:39:45 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using encoding = go.encoding_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct wireType
        {
            // Constructors
            public wireType(NilType _)
            {
                this.ArrayT = default;
                this.SliceT = default;
                this.StructT = default;
                this.MapT = default;
                this.GobEncoderT = default;
                this.BinaryMarshalerT = default;
                this.TextMarshalerT = default;
            }

            public wireType(ref ptr<arrayType> ArrayT = default, ref ptr<sliceType> SliceT = default, ref ptr<structType> StructT = default, ref ptr<mapType> MapT = default, ref ptr<gobEncoderType> GobEncoderT = default, ref ptr<gobEncoderType> BinaryMarshalerT = default, ref ptr<gobEncoderType> TextMarshalerT = default)
            {
                this.ArrayT = ArrayT;
                this.SliceT = SliceT;
                this.StructT = StructT;
                this.MapT = MapT;
                this.GobEncoderT = GobEncoderT;
                this.BinaryMarshalerT = BinaryMarshalerT;
                this.TextMarshalerT = TextMarshalerT;
            }

            // Enable comparisons between nil and wireType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(wireType value, NilType nil) => value.Equals(default(wireType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(wireType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, wireType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, wireType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator wireType(NilType nil) => default(wireType);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static wireType wireType_cast(dynamic value)
        {
            return new wireType(ref value.ArrayT, ref value.SliceT, ref value.StructT, ref value.MapT, ref value.GobEncoderT, ref value.BinaryMarshalerT, ref value.TextMarshalerT);
        }
    }
}}