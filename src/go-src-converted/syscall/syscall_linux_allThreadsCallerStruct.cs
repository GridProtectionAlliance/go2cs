//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:27:02 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using itoa = go.@internal.itoa_package;
using @unsafe = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class syscall_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct allThreadsCaller
        {
            // Constructors
            public allThreadsCaller(NilType _)
            {
                this.trap = default;
                this.a1 = default;
                this.a2 = default;
                this.a3 = default;
                this.a4 = default;
                this.a5 = default;
                this.a6 = default;
                this.r1 = default;
                this.r2 = default;
                this.err = default;
            }

            public allThreadsCaller(System.UIntPtr trap = default, System.UIntPtr a1 = default, System.UIntPtr a2 = default, System.UIntPtr a3 = default, System.UIntPtr a4 = default, System.UIntPtr a5 = default, System.UIntPtr a6 = default, System.UIntPtr r1 = default, System.UIntPtr r2 = default, Errno err = default)
            {
                this.trap = trap;
                this.a1 = a1;
                this.a2 = a2;
                this.a3 = a3;
                this.a4 = a4;
                this.a5 = a5;
                this.a6 = a6;
                this.r1 = r1;
                this.r2 = r2;
                this.err = err;
            }

            // Enable comparisons between nil and allThreadsCaller struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(allThreadsCaller value, NilType nil) => value.Equals(default(allThreadsCaller));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(allThreadsCaller value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, allThreadsCaller value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, allThreadsCaller value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator allThreadsCaller(NilType nil) => default(allThreadsCaller);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static allThreadsCaller allThreadsCaller_cast(dynamic value)
        {
            return new allThreadsCaller(value.trap, value.a1, value.a2, value.a3, value.a4, value.a5, value.a6, value.r1, value.r2, value.err);
        }
    }
}