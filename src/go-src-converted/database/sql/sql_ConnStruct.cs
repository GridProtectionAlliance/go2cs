//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:05:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
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
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Conn
        {
            // Constructors
            public Conn(NilType _)
            {
                this.db = default;
                this.closemu = default;
                this.dc = default;
                this.done = default;
            }

            public Conn(ref ptr<DB> db = default, sync.RWMutex closemu = default, ref ptr<driverConn> dc = default, int done = default)
            {
                this.db = db;
                this.closemu = closemu;
                this.dc = dc;
                this.done = done;
            }

            // Enable comparisons between nil and Conn struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Conn value, NilType nil) => value.Equals(default(Conn));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Conn value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Conn value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Conn value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Conn(NilType nil) => default(Conn);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Conn Conn_cast(dynamic value)
        {
            return new Conn(ref value.db, value.closemu, ref value.dc, value.done);
        }
    }
}}