//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using xcoff = go.@internal.xcoff_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace go {
namespace @internal
{
    public static partial class gccgoimporter_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct PackageInit
        {
            // Constructors
            public PackageInit(NilType _)
            {
                this.Name = default;
                this.InitFunc = default;
                this.Priority = default;
            }

            public PackageInit(@string Name = default, @string InitFunc = default, nint Priority = default)
            {
                this.Name = Name;
                this.InitFunc = InitFunc;
                this.Priority = Priority;
            }

            // Enable comparisons between nil and PackageInit struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(PackageInit value, NilType nil) => value.Equals(default(PackageInit));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(PackageInit value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, PackageInit value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, PackageInit value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator PackageInit(NilType nil) => default(PackageInit);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static PackageInit PackageInit_cast(dynamic value)
        {
            return new PackageInit(value.Name, value.InitFunc, value.Priority);
        }
    }
}}}