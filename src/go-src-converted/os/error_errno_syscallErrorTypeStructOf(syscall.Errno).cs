//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:27:54 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class os_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct syscallErrorType
        {
            // Value of the syscallErrorType struct
            private readonly syscall.Errno m_value;
            
            public syscallErrorType(syscall.Errno value) => m_value = value;

            // Enable implicit conversions between syscall.Errno and syscallErrorType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator syscallErrorType(syscall.Errno value) => new syscallErrorType(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator syscall.Errno(syscallErrorType value) => value.m_value;
            
            // Enable comparisons between nil and syscallErrorType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(syscallErrorType value, NilType nil) => value.Equals(default(syscallErrorType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(syscallErrorType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, syscallErrorType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, syscallErrorType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator syscallErrorType(NilType nil) => default(syscallErrorType);
        }
    }
}
