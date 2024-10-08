//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:37:39 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using httptrace = go.net.http.httptrace_package;
using @internal = go.net.http.@internal_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using httpguts = go.golang.org.x.net.http.httpguts_package;
using go;

#nullable enable

namespace go {
namespace net
{
    public static partial class http_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct transferReader
        {
            // Constructors
            public transferReader(NilType _)
            {
                this.Header = default;
                this.StatusCode = default;
                this.RequestMethod = default;
                this.ProtoMajor = default;
                this.ProtoMinor = default;
                this.Body = default;
                this.ContentLength = default;
                this.Chunked = default;
                this.Close = default;
                this.Trailer = default;
            }

            public transferReader(Header Header = default, nint StatusCode = default, @string RequestMethod = default, nint ProtoMajor = default, nint ProtoMinor = default, io.ReadCloser Body = default, long ContentLength = default, bool Chunked = default, bool Close = default, Header Trailer = default)
            {
                this.Header = Header;
                this.StatusCode = StatusCode;
                this.RequestMethod = RequestMethod;
                this.ProtoMajor = ProtoMajor;
                this.ProtoMinor = ProtoMinor;
                this.Body = Body;
                this.ContentLength = ContentLength;
                this.Chunked = Chunked;
                this.Close = Close;
                this.Trailer = Trailer;
            }

            // Enable comparisons between nil and transferReader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(transferReader value, NilType nil) => value.Equals(default(transferReader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(transferReader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, transferReader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, transferReader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator transferReader(NilType nil) => default(transferReader);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static transferReader transferReader_cast(dynamic value)
        {
            return new transferReader(value.Header, value.StatusCode, value.RequestMethod, value.ProtoMajor, value.ProtoMinor, value.Body, value.ContentLength, value.Chunked, value.Close, value.Trailer);
        }
    }
}}