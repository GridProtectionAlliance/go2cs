//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:52:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using context = go.context_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using time = go.time_package;

#nullable enable

namespace go
{
    public static partial class net_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct TCPConn
        {
            // Constructors
            public TCPConn(NilType _)
            {
                this.conn = default;
            }

            public TCPConn(conn conn = default)
            {
                this.conn = conn;
            }

            // Enable comparisons between nil and TCPConn struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(TCPConn value, NilType nil) => value.Equals(default(TCPConn));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(TCPConn value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, TCPConn value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, TCPConn value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator TCPConn(NilType nil) => default(TCPConn);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static TCPConn TCPConn_cast(dynamic value)
        {
            return new TCPConn(value.conn);
        }
    }
}