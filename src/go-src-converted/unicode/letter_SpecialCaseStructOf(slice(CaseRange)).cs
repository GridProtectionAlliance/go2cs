//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:49:32 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class unicode_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct SpecialCase
        {
            // Value of the SpecialCase struct
            private readonly slice<CaseRange> m_value;

            public SpecialCase(slice<CaseRange> value) => m_value = value;

            // Enable implicit conversions between slice<CaseRange> and SpecialCase struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SpecialCase(slice<CaseRange> value) => new SpecialCase(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<CaseRange>(SpecialCase value) => value.m_value;
            
            // Enable comparisons between nil and SpecialCase struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(SpecialCase value, NilType nil) => value.Equals(default(SpecialCase));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(SpecialCase value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, SpecialCase value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, SpecialCase value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SpecialCase(NilType nil) => default(SpecialCase);
        }
    }
}
