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
        public partial struct RawSockaddrIUCV
        {
            // Constructors
            public RawSockaddrIUCV(NilType _)
            {
                this.Family = default;
                this.Port = default;
                this.Addr = default;
                this.Nodeid = default;
                this.User_id = default;
                this.Name = default;
            }

            public RawSockaddrIUCV(ushort Family = default, ushort Port = default, uint Addr = default, array<sbyte> Nodeid = default, array<sbyte> User_id = default, array<sbyte> Name = default)
            {
                this.Family = Family;
                this.Port = Port;
                this.Addr = Addr;
                this.Nodeid = Nodeid;
                this.User_id = User_id;
                this.Name = Name;
            }

            // Enable comparisons between nil and RawSockaddrIUCV struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(RawSockaddrIUCV value, NilType nil) => value.Equals(default(RawSockaddrIUCV));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(RawSockaddrIUCV value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, RawSockaddrIUCV value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, RawSockaddrIUCV value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RawSockaddrIUCV(NilType nil) => default(RawSockaddrIUCV);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static RawSockaddrIUCV RawSockaddrIUCV_cast(dynamic value)
        {
            return new RawSockaddrIUCV(value.Family, value.Port, value.Addr, value.Nodeid, value.User_id, value.Name);
        }
    }
}}}}}}