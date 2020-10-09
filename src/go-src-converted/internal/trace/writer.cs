// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2020 October 09 05:53:04 UTC
// import "internal/trace" ==> using trace = go.@internal.trace_package
// Original source: C:\Go\src\internal\trace\writer.go
using bytes = go.bytes_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        // Writer is a test trace writer.
        public partial struct Writer
        {
            public ref bytes.Buffer Buffer => ref Buffer_val;
        }

        public static ptr<Writer> NewWriter()
        {
            ptr<Writer> w = @new<Writer>();
            w.Write((slice<byte>)"go 1.9 trace\x00\x00\x00\x00");
            return _addr_w!;
        }

        // Emit writes an event record to the trace.
        // See Event types for valid types and required arguments.
        private static void Emit(this ptr<Writer> _addr_w, byte typ, params ulong[] args) => func((_, panic, __) =>
        {
            args = args.Clone();
            ref Writer w = ref _addr_w.val;

            var nargs = byte(len(args)) - 1L;
            if (nargs > 3L)
            {
                nargs = 3L;
            }

            byte buf = new slice<byte>(new byte[] { typ|nargs<<6 });
            if (nargs == 3L)
            {
                buf = append(buf, 0L);
            }

            foreach (var (_, a) in args)
            {
                buf = appendVarint(buf, a);
            }
            if (nargs == 3L)
            {
                buf[1L] = byte(len(buf) - 2L);
            }

            var (n, err) = w.Write(buf);
            if (n != len(buf) || err != null)
            {
                panic("failed to write");
            }

        });

        private static slice<byte> appendVarint(slice<byte> buf, ulong v)
        {
            while (v >= 0x80UL)
            {
                buf = append(buf, 0x80UL | byte(v));
                v >>= 7L;
            }

            buf = append(buf, byte(v));
            return buf;

        }
    }
}}
