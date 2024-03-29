//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:45:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using errors = go.errors_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net {
namespace dns
{
    public static partial class dnsmessage_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Builder
        {
            // Constructors
            public Builder(NilType _)
            {
                this.msg = default;
                this.section = default;
                this.header = default;
                this.start = default;
                this.compression = default;
            }

            public Builder(slice<byte> msg = default, section section = default, header header = default, nint start = default, map<@string, nint> compression = default)
            {
                this.msg = msg;
                this.section = section;
                this.header = header;
                this.start = start;
                this.compression = compression;
            }

            // Enable comparisons between nil and Builder struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Builder value, NilType nil) => value.Equals(default(Builder));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Builder value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Builder value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Builder value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Builder(NilType nil) => default(Builder);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Builder Builder_cast(dynamic value)
        {
            return new Builder(value.msg, value.section, value.header, value.start, value.compression);
        }
    }
}}}}}}