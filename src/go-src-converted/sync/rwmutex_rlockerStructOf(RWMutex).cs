//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:01:10 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class sync_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct rlocker
        {
            // Value of the rlocker struct
            private readonly RWMutex m_value;

            public rlocker(RWMutex value) => m_value = value;

            // Enable implicit conversions between RWMutex and rlocker struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator rlocker(RWMutex value) => new rlocker(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RWMutex(rlocker value) => value.m_value;
            
            // Enable comparisons between nil and rlocker struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(rlocker value, NilType nil) => value.Equals(default(rlocker));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(rlocker value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, rlocker value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, rlocker value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator rlocker(NilType nil) => default(rlocker);
        }
    }
}
