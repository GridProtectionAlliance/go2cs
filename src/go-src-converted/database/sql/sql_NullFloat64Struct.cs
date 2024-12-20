//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:43:37 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using context = go.context_package;
using driver = go.database.sql.driver_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using go;

#nullable enable

namespace go {
namespace database
{
    public static partial class sql_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct NullFloat64
        {
            // Constructors
            public NullFloat64(NilType _)
            {
                this.Float64 = default;
                this.Valid = default;
            }

            public NullFloat64(double Float64 = default, bool Valid = default)
            {
                this.Float64 = Float64;
                this.Valid = Valid;
            }

            // Enable comparisons between nil and NullFloat64 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NullFloat64 value, NilType nil) => value.Equals(default(NullFloat64));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NullFloat64 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, NullFloat64 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, NullFloat64 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NullFloat64(NilType nil) => default(NullFloat64);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static NullFloat64 NullFloat64_cast(dynamic value)
        {
            return new NullFloat64(value.Float64, value.Valid);
        }
    }
}}