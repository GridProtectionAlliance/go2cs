//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:35:06 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using context = go.context_package;
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using io = go.io_package;
using net = go.net_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        [PromotedStruct(typeof(sync.Mutex))]
        private partial struct halfConn
        {
            // Mutex structure promotion - sourced from value copy
            private readonly ptr<Mutex> m_MutexRef;

            private ref Mutex Mutex_val => ref m_MutexRef.Value;

            public ref int state => ref m_MutexRef.Value.state;

            public ref uint sema => ref m_MutexRef.Value.sema;

            // Constructors
            public halfConn(NilType _)
            {
                this.m_MutexRef = new ptr<sync.Mutex>(new sync.Mutex(nil));
                this.err = default;
                this.version = default;
                this.mac = default;
                this.seq = default;
                this.scratchBuf = default;
                this.nextMac = default;
                this.trafficSecret = default;
            }

            public halfConn(sync.Mutex Mutex = default, error err = default, ushort version = default, hash.Hash mac = default, array<byte> seq = default, array<byte> scratchBuf = default, hash.Hash nextMac = default, slice<byte> trafficSecret = default)
            {
                this.m_MutexRef = new ptr<sync.Mutex>(Mutex);
                this.err = err;
                this.version = version;
                this.mac = mac;
                this.seq = seq;
                this.scratchBuf = scratchBuf;
                this.nextMac = nextMac;
                this.trafficSecret = trafficSecret;
            }

            // Enable comparisons between nil and halfConn struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(halfConn value, NilType nil) => value.Equals(default(halfConn));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(halfConn value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, halfConn value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, halfConn value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator halfConn(NilType nil) => default(halfConn);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static halfConn halfConn_cast(dynamic value)
        {
            return new halfConn(value.Mutex, value.err, value.version, value.mac, value.seq, value.scratchBuf, value.nextMac, value.trafficSecret);
        }
    }
}}