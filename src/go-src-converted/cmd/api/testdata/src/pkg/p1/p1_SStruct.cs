//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:54:11 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ptwo = go.p2_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace api {
namespace testdata {
namespace src {
namespace pkg
{
    public static partial class p1_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct s
        {
            // Constructors
            public s(NilType _)
            {
            }
            // Enable comparisons between nil and s struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(s value, NilType nil) => value.Equals(default(s));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(s value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, s value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, s value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator s(NilType nil) => default(s);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static s s_cast(dynamic value)
        {
            return new s();
        }
    }
}}}}}}