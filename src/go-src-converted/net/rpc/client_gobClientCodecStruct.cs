//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 08:36:32 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using bufio = go.bufio_package;
using gob = go.encoding.gob_package;
using errors = go.errors_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using sync = go.sync_package;
using go;

namespace go {
namespace net
{
    public static partial class rpc_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct gobClientCodec
        {
            // Constructors
            public gobClientCodec(NilType _)
            {
                this.rwc = default;
                this.dec = default;
                this.enc = default;
                this.encBuf = default;
            }

            public gobClientCodec(io.ReadWriteCloser rwc = default, ref ptr<gob.Decoder> dec = default, ref ptr<gob.Encoder> enc = default, ref ptr<bufio.Writer> encBuf = default)
            {
                this.rwc = rwc;
                this.dec = dec;
                this.enc = enc;
                this.encBuf = encBuf;
            }

            // Enable comparisons between nil and gobClientCodec struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(gobClientCodec value, NilType nil) => value.Equals(default(gobClientCodec));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(gobClientCodec value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, gobClientCodec value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, gobClientCodec value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator gobClientCodec(NilType nil) => default(gobClientCodec);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static gobClientCodec gobClientCodec_cast(dynamic value)
        {
            return new gobClientCodec(value.rwc, ref value.dec, ref value.enc, ref value.encBuf);
        }
    }
}}