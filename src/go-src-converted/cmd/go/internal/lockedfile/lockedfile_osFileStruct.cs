//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:30:05 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using runtime = go.runtime_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class lockedfile_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct osFile
        {
            // Constructors
            public osFile(NilType _)
            {
                this.File> = default;
            }

            public osFile(ref ptr<os.File> File> = default)
            {
                this.File> = File>;
            }

            // Enable comparisons between nil and osFile struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(osFile value, NilType nil) => value.Equals(default(osFile));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(osFile value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, osFile value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, osFile value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator osFile(NilType nil) => default(osFile);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static osFile osFile_cast(dynamic value)
        {
            return new osFile(ref value.File>);
        }
    }
}}}}