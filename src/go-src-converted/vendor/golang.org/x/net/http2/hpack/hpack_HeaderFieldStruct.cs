//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:45:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net {
namespace http2
{
    public static partial class hpack_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct HeaderField
        {
            // Constructors
            public HeaderField(NilType _)
            {
                this.Name = default;
                this.Value = default;
                this.Sensitive = default;
            }

            public HeaderField(@string Name = default, @string Value = default, bool Sensitive = default)
            {
                this.Name = Name;
                this.Value = Value;
                this.Sensitive = Sensitive;
            }

            // Enable comparisons between nil and HeaderField struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(HeaderField value, NilType nil) => value.Equals(default(HeaderField));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(HeaderField value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, HeaderField value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, HeaderField value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator HeaderField(NilType nil) => default(HeaderField);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static HeaderField HeaderField_cast(dynamic value)
        {
            return new HeaderField(value.Name, value.Value, value.Sensitive);
        }
    }
}}}}}}