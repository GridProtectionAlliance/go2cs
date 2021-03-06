//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:55:45 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using binary = go.encoding.binary_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto {
namespace ed25519 {
namespace @internal
{
    public static partial class edwards25519_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct CompletedGroupElement
        {
            // Constructors
            public CompletedGroupElement(NilType _)
            {
                this.X = default;
                this.Y = default;
                this.Z = default;
                this.T = default;
            }

            public CompletedGroupElement(FieldElement X = default, FieldElement Y = default, FieldElement Z = default, FieldElement T = default)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
                this.T = T;
            }

            // Enable comparisons between nil and CompletedGroupElement struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(CompletedGroupElement value, NilType nil) => value.Equals(default(CompletedGroupElement));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(CompletedGroupElement value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, CompletedGroupElement value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, CompletedGroupElement value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CompletedGroupElement(NilType nil) => default(CompletedGroupElement);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static CompletedGroupElement CompletedGroupElement_cast(dynamic value)
        {
            return new CompletedGroupElement(value.X, value.Y, value.Z, value.T);
        }
    }
}}}}}}}}