//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:38:17 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using http = go.net.http_package;
using cgi = go.net.http.cgi_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;
using go;

#nullable enable

namespace go {
namespace net {
namespace http
{
    public static partial class fcgi_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct envVarsContextKey
        {
            // Constructors
            public envVarsContextKey(NilType _)
            {
            }
            // Enable comparisons between nil and envVarsContextKey struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(envVarsContextKey value, NilType nil) => value.Equals(default(envVarsContextKey));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(envVarsContextKey value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, envVarsContextKey value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, envVarsContextKey value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator envVarsContextKey(NilType nil) => default(envVarsContextKey);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static envVarsContextKey envVarsContextKey_cast(dynamic value)
        {
            return new envVarsContextKey();
        }
    }
}}}