//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:21:23 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct ElfEhdr
        {
            // Value of the ElfEhdr struct
            private readonly elf.Header64 m_value;
            
            public ElfEhdr(elf.Header64 value) => m_value = value;

            // Enable implicit conversions between elf.Header64 and ElfEhdr struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ElfEhdr(elf.Header64 value) => new ElfEhdr(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator elf.Header64(ElfEhdr value) => value.m_value;
            
            // Enable comparisons between nil and ElfEhdr struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ElfEhdr value, NilType nil) => value.Equals(default(ElfEhdr));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ElfEhdr value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ElfEhdr value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ElfEhdr value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ElfEhdr(NilType nil) => default(ElfEhdr);
        }
    }
}}}}