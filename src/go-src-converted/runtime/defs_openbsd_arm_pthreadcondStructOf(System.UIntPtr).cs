//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:08:35 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct pthreadcond
        {
            // Value of the pthreadcond struct
            private readonly System.UIntPtr m_value;
            
            public pthreadcond(System.UIntPtr value) => m_value = value;

            // Enable implicit conversions between System.UIntPtr and pthreadcond struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator pthreadcond(System.UIntPtr value) => new pthreadcond(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator System.UIntPtr(pthreadcond value) => value.m_value;
            
            // Enable comparisons between nil and pthreadcond struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(pthreadcond value, NilType nil) => value.Equals(default(pthreadcond));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(pthreadcond value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, pthreadcond value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, pthreadcond value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator pthreadcond(NilType nil) => default(pthreadcond);
        }
    }
}