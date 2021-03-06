//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:45:50 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;


#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct sigcontext
        {
            // Constructors
            public sigcontext(NilType _)
            {
                this.fault_address = default;
                this.regs = default;
                this.sp = default;
                this.pc = default;
                this.pstate = default;
                this._pad = default;
                this.__reserved = default;
            }

            public sigcontext(ulong fault_address = default, array<ulong> regs = default, ulong sp = default, ulong pc = default, ulong pstate = default, array<byte> _pad = default, array<byte> __reserved = default)
            {
                this.fault_address = fault_address;
                this.regs = regs;
                this.sp = sp;
                this.pc = pc;
                this.pstate = pstate;
                this._pad = _pad;
                this.__reserved = __reserved;
            }

            // Enable comparisons between nil and sigcontext struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(sigcontext value, NilType nil) => value.Equals(default(sigcontext));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(sigcontext value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, sigcontext value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, sigcontext value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sigcontext(NilType nil) => default(sigcontext);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static sigcontext sigcontext_cast(dynamic value)
        {
            return new sigcontext(value.fault_address, value.regs, value.sp, value.pc, value.pstate, value._pad, value.__reserved);
        }
    }
}