//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:00:52 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct SID
        {
            // Constructors
            public SID(NilType _)
            {
            }
            // Enable comparisons between nil and SID struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(SID value, NilType nil) => value.Equals(default(SID));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(SID value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, SID value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, SID value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SID(NilType nil) => default(SID);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static SID SID_cast(dynamic value)
        {
            return new SID();
        }
    }
}}}}}}