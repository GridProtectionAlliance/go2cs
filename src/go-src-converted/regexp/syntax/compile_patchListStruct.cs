//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:37:55 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using unicode = go.unicode_package;
using go;

#nullable enable

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct patchList
        {
            // Constructors
            public patchList(NilType _)
            {
                this.head = default;
                this.tail = default;
            }

            public patchList(uint head = default, uint tail = default)
            {
                this.head = head;
                this.tail = tail;
            }

            // Enable comparisons between nil and patchList struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(patchList value, NilType nil) => value.Equals(default(patchList));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(patchList value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, patchList value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, patchList value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator patchList(NilType nil) => default(patchList);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static patchList patchList_cast(dynamic value)
        {
            return new patchList(value.head, value.tail);
        }
    }
}}