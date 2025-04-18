//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:43:28 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using sha1 = go.crypto.sha1_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using unsafeheader = go.@internal.unsafeheader_package;
using io = go.io_package;
using @unsafe = go.@unsafe_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class goobj_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Reader
        {
            // Constructors
            public Reader(NilType _)
            {
                this.b = default;
                this.@readonly = default;
                this.rd = default;
                this.start = default;
                this.h = default;
            }

            public Reader(slice<byte> b = default, bool @readonly = default, io.ReaderAt rd = default, uint start = default, Header h = default)
            {
                this.b = b;
                this.@readonly = @readonly;
                this.rd = rd;
                this.start = start;
                this.h = h;
            }

            // Enable comparisons between nil and Reader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Reader value, NilType nil) => value.Equals(default(Reader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Reader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Reader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Reader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Reader(NilType nil) => default(Reader);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Reader Reader_cast(dynamic value)
        {
            return new Reader(value.b, value.@readonly, value.rd, value.start, value.h);
        }
    }
}}}