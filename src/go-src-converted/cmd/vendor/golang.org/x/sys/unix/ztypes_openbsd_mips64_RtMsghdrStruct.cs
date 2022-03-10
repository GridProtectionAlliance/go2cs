//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:30:31 UTC
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
        public partial struct RtMsghdr
        {
            // Constructors
            public RtMsghdr(NilType _)
            {
                this.Msglen = default;
                this.Version = default;
                this.Type = default;
                this.Hdrlen = default;
                this.Index = default;
                this.Tableid = default;
                this.Priority = default;
                this.Mpls = default;
                this.Addrs = default;
                this.Flags = default;
                this.Fmask = default;
                this.Pid = default;
                this.Seq = default;
                this.Errno = default;
                this.Inits = default;
                this.Rmx = default;
            }

            public RtMsghdr(ushort Msglen = default, byte Version = default, byte Type = default, ushort Hdrlen = default, ushort Index = default, ushort Tableid = default, byte Priority = default, byte Mpls = default, int Addrs = default, int Flags = default, int Fmask = default, int Pid = default, int Seq = default, int Errno = default, uint Inits = default, RtMetrics Rmx = default)
            {
                this.Msglen = Msglen;
                this.Version = Version;
                this.Type = Type;
                this.Hdrlen = Hdrlen;
                this.Index = Index;
                this.Tableid = Tableid;
                this.Priority = Priority;
                this.Mpls = Mpls;
                this.Addrs = Addrs;
                this.Flags = Flags;
                this.Fmask = Fmask;
                this.Pid = Pid;
                this.Seq = Seq;
                this.Errno = Errno;
                this.Inits = Inits;
                this.Rmx = Rmx;
            }

            // Enable comparisons between nil and RtMsghdr struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(RtMsghdr value, NilType nil) => value.Equals(default(RtMsghdr));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(RtMsghdr value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, RtMsghdr value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, RtMsghdr value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator RtMsghdr(NilType nil) => default(RtMsghdr);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static RtMsghdr RtMsghdr_cast(dynamic value)
        {
            return new RtMsghdr(value.Msglen, value.Version, value.Type, value.Hdrlen, value.Index, value.Tableid, value.Priority, value.Mpls, value.Addrs, value.Flags, value.Fmask, value.Pid, value.Seq, value.Errno, value.Inits, value.Rmx);
        }
    }
}}}}}}