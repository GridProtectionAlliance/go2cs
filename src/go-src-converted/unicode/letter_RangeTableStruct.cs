//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:28:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;


#nullable enable

namespace go
{
    public static partial class unicode_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct RangeTable
        {
            // Constructors
            public RangeTable(NilType _)
            {
                this.R16 = default;
                this.R32 = default;
                this.LatinOffset = default;
            }

            public RangeTable(slice<Range16> R16 = default, slice<Range32> R32 = default, nint LatinOffset = default)
            {
                this.R16 = R16;
                this.R32 = R32;
                this.LatinOffset = LatinOffset;
            }

            // Enable comparisons between nil and RangeTable struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(RangeTable value, NilType nil) => value.Equals(default(RangeTable));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(RangeTable value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, RangeTable value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, RangeTable value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RangeTable(NilType nil) => default(RangeTable);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static RangeTable RangeTable_cast(dynamic value)
        {
            return new RangeTable(value.R16, value.R32, value.LatinOffset);
        }
    }
}