//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:53:12 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using sort = go.sort_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct MethodSet
        {
            // Constructors
            public MethodSet(NilType _)
            {
                this.list = default;
            }

            public MethodSet(slice<ptr<Selection>> list = default)
            {
                this.list = list;
            }

            // Enable comparisons between nil and MethodSet struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(MethodSet value, NilType nil) => value.Equals(default(MethodSet));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(MethodSet value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, MethodSet value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, MethodSet value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MethodSet(NilType nil) => default(MethodSet);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static MethodSet MethodSet_cast(dynamic value)
        {
            return new MethodSet(value.list);
        }
    }
}}