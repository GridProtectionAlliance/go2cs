//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:36:18 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct gState
        {
            // Value of the gState struct
            private readonly nint m_value;
            
            public gState(nint value) => m_value = value;

            // Enable implicit conversions between nint and gState struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator gState(nint value) => new gState(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(gState value) => value.m_value;
            
            // Enable comparisons between nil and gState struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(gState value, NilType nil) => value.Equals(default(gState));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(gState value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, gState value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, gState value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator gState(NilType nil) => default(gState);
        }
    }
}
