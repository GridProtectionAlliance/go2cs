//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:40 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using a = go.a_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class c_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct A
        {
            // Constructors
            public A(NilType _)
            {
            }
            // Enable comparisons between nil and A struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(A value, NilType nil) => value.Equals(default(A));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(A value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, A value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, A value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A(NilType nil) => default(A);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static A A_cast(dynamic value)
        {
            return new A();
        }
    }
}}