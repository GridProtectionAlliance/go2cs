//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:44:31 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using syscall = go.syscall_package;
using go;

#nullable enable

namespace go {
namespace @internal {
namespace syscall {
namespace windows
{
    public static partial class registry_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct KeyInfo
        {
            // Constructors
            public KeyInfo(NilType _)
            {
                this.SubKeyCount = default;
                this.MaxSubKeyLen = default;
                this.ValueCount = default;
                this.MaxValueNameLen = default;
                this.MaxValueLen = default;
                this.lastWriteTime = default;
            }

            public KeyInfo(uint SubKeyCount = default, uint MaxSubKeyLen = default, uint ValueCount = default, uint MaxValueNameLen = default, uint MaxValueLen = default, syscall.Filetime lastWriteTime = default)
            {
                this.SubKeyCount = SubKeyCount;
                this.MaxSubKeyLen = MaxSubKeyLen;
                this.ValueCount = ValueCount;
                this.MaxValueNameLen = MaxValueNameLen;
                this.MaxValueLen = MaxValueLen;
                this.lastWriteTime = lastWriteTime;
            }

            // Enable comparisons between nil and KeyInfo struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(KeyInfo value, NilType nil) => value.Equals(default(KeyInfo));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(KeyInfo value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, KeyInfo value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, KeyInfo value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator KeyInfo(NilType nil) => default(KeyInfo);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static KeyInfo KeyInfo_cast(dynamic value)
        {
            return new KeyInfo(value.SubKeyCount, value.MaxSubKeyLen, value.ValueCount, value.MaxValueNameLen, value.MaxValueLen, value.lastWriteTime);
        }
    }
}}}}