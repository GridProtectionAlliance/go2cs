//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:23 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace @internal
{
    public static partial class execabs_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Cmd
        {
            // Value of the Cmd struct
            private readonly exec.Cmd m_value;
            
            public Cmd(exec.Cmd value) => m_value = value;

            // Enable implicit conversions between exec.Cmd and Cmd struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Cmd(exec.Cmd value) => new Cmd(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator exec.Cmd(Cmd value) => value.m_value;
            
            // Enable comparisons between nil and Cmd struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Cmd value, NilType nil) => value.Equals(default(Cmd));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Cmd value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Cmd value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Cmd value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Cmd(NilType nil) => default(Cmd);
        }
    }
}}
