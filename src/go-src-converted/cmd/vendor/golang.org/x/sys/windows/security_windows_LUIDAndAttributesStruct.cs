//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 08 04:53:48 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using go;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct LUIDAndAttributes
        {
            // Constructors
            public LUIDAndAttributes(NilType _)
            {
                this.Luid = default;
                this.Attributes = default;
            }

            public LUIDAndAttributes(LUID Luid = default, uint Attributes = default)
            {
                this.Luid = Luid;
                this.Attributes = Attributes;
            }

            // Enable comparisons between nil and LUIDAndAttributes struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(LUIDAndAttributes value, NilType nil) => value.Equals(default(LUIDAndAttributes));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(LUIDAndAttributes value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, LUIDAndAttributes value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, LUIDAndAttributes value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator LUIDAndAttributes(NilType nil) => default(LUIDAndAttributes);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static LUIDAndAttributes LUIDAndAttributes_cast(dynamic value)
        {
            return new LUIDAndAttributes(value.Luid, value.Attributes);
        }
    }
}}}}}}