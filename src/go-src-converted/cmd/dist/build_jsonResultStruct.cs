//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:28:47 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

#nullable enable

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct jsonResult
        {
            // Constructors
            public jsonResult(NilType _)
            {
                this.GOOS = default;
                this.GOARCH = default;
                this.CgoSupported = default;
                this.FirstClass = default;
            }

            public jsonResult(@string GOOS = default, @string GOARCH = default, bool CgoSupported = default, bool FirstClass = default)
            {
                this.GOOS = GOOS;
                this.GOARCH = GOARCH;
                this.CgoSupported = CgoSupported;
                this.FirstClass = FirstClass;
            }

            // Enable comparisons between nil and jsonResult struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(jsonResult value, NilType nil) => value.Equals(default(jsonResult));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(jsonResult value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, jsonResult value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, jsonResult value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator jsonResult(NilType nil) => default(jsonResult);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static jsonResult jsonResult_cast(dynamic value)
        {
            return new jsonResult(value.GOOS, value.GOARCH, value.CgoSupported, value.FirstClass);
        }
    }
}