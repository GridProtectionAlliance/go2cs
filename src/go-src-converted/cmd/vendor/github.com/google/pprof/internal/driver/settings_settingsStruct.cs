//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:23:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using url = go.net.url_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct settings
        {
            // Constructors
            public settings(NilType _)
            {
                this.Configs = default;
            }

            public settings(slice<namedConfig> Configs = default)
            {
                this.Configs = Configs;
            }

            // Enable comparisons between nil and settings struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(settings value, NilType nil) => value.Equals(default(settings));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(settings value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, settings value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, settings value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator settings(NilType nil) => default(settings);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static settings settings_cast(dynamic value)
        {
            return new settings(value.Configs);
        }
    }
}}}}}}}