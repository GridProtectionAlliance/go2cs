//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:57:35 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
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
    public static partial class unix_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct PtraceRegsMips64le
        {
            // Constructors
            public PtraceRegsMips64le(NilType _)
            {
                this.Regs = default;
                this.Lo = default;
                this.Hi = default;
                this.Epc = default;
                this.Badvaddr = default;
                this.Status = default;
                this.Cause = default;
            }

            public PtraceRegsMips64le(array<ulong> Regs = default, ulong Lo = default, ulong Hi = default, ulong Epc = default, ulong Badvaddr = default, ulong Status = default, ulong Cause = default)
            {
                this.Regs = Regs;
                this.Lo = Lo;
                this.Hi = Hi;
                this.Epc = Epc;
                this.Badvaddr = Badvaddr;
                this.Status = Status;
                this.Cause = Cause;
            }

            // Enable comparisons between nil and PtraceRegsMips64le struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(PtraceRegsMips64le value, NilType nil) => value.Equals(default(PtraceRegsMips64le));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(PtraceRegsMips64le value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, PtraceRegsMips64le value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, PtraceRegsMips64le value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator PtraceRegsMips64le(NilType nil) => default(PtraceRegsMips64le);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static PtraceRegsMips64le PtraceRegsMips64le_cast(dynamic value)
        {
            return new PtraceRegsMips64le(value.Regs, value.Lo, value.Hi, value.Epc, value.Badvaddr, value.Status, value.Cause);
        }
    }
}}}}}}