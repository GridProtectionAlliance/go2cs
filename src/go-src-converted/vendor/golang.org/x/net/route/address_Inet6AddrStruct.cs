//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:46:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using runtime = go.runtime_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Inet6Addr
        {
            // Constructors
            public Inet6Addr(NilType _)
            {
                this.IP = default;
                this.ZoneID = default;
            }

            public Inet6Addr(array<byte> IP = default, nint ZoneID = default)
            {
                this.IP = IP;
                this.ZoneID = ZoneID;
            }

            // Enable comparisons between nil and Inet6Addr struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Inet6Addr value, NilType nil) => value.Equals(default(Inet6Addr));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Inet6Addr value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Inet6Addr value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Inet6Addr value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Inet6Addr(NilType nil) => default(Inet6Addr);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Inet6Addr Inet6Addr_cast(dynamic value)
        {
            return new Inet6Addr(value.IP, value.ZoneID);
        }
    }
}}}}}