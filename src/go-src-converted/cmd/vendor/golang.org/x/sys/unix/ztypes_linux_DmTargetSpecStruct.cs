//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:30:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        public partial struct DmTargetSpec
        {
            // Constructors
            public DmTargetSpec(NilType _)
            {
                this.Sector_start = default;
                this.Length = default;
                this.Status = default;
                this.Next = default;
                this.Target_type = default;
            }

            public DmTargetSpec(ulong Sector_start = default, ulong Length = default, int Status = default, uint Next = default, array<byte> Target_type = default)
            {
                this.Sector_start = Sector_start;
                this.Length = Length;
                this.Status = Status;
                this.Next = Next;
                this.Target_type = Target_type;
            }

            // Enable comparisons between nil and DmTargetSpec struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(DmTargetSpec value, NilType nil) => value.Equals(default(DmTargetSpec));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(DmTargetSpec value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, DmTargetSpec value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, DmTargetSpec value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator DmTargetSpec(NilType nil) => default(DmTargetSpec);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static DmTargetSpec DmTargetSpec_cast(dynamic value)
        {
            return new DmTargetSpec(value.Sector_start, value.Length, value.Status, value.Next, value.Target_type);
        }
    }
}}}}}}