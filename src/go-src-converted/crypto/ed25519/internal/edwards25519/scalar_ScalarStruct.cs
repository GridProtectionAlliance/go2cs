//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:31:41 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using subtle = go.crypto.subtle_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using go;

#nullable enable

namespace go {
namespace crypto {
namespace ed25519 {
namespace @internal
{
    public static partial class edwards25519_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Scalar
        {
            // Constructors
            public Scalar(NilType _)
            {
                this.s = default;
            }

            public Scalar(array<byte> s = default)
            {
                this.s = s;
            }

            // Enable comparisons between nil and Scalar struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Scalar value, NilType nil) => value.Equals(default(Scalar));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Scalar value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Scalar value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Scalar value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Scalar(NilType nil) => default(Scalar);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Scalar Scalar_cast(dynamic value)
        {
            return new Scalar(value.s);
        }
    }
}}}}