//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:45:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Uintreg
        {
            // Value of the Uintreg struct
            private readonly uint m_value;

            public Uintreg(uint value) => m_value = value;

            // Enable implicit conversions between uint and Uintreg struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Uintreg(uint value) => new Uintreg(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator uint(Uintreg value) => value.m_value;
            
            // Enable comparisons between nil and Uintreg struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Uintreg value, NilType nil) => value.Equals(default(Uintreg));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Uintreg value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Uintreg value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Uintreg value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Uintreg(NilType nil) => default(Uintreg);
        }
    }
}}}
