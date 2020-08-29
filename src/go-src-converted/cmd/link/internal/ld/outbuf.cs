// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:04:21 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\outbuf.go
using bufio = go.bufio_package;
using sys = go.cmd.@internal.sys_package;
using binary = go.encoding.binary_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // OutBuf is a buffered file writer.
        //
        // It is simlar to the Writer in cmd/internal/bio with a few small differences.
        //
        // First, it tracks the output architecture and uses it to provide
        // endian helpers.
        //
        // Second, it provides a very cheap offset counter that doesn't require
        // any system calls to read the value.
        public partial struct OutBuf
        {
            public ptr<sys.Arch> arch;
            public long off;
            public ptr<bufio.Writer> w;
            public ptr<os.File> f;
            public array<byte> encbuf; // temp buffer used by WriteN methods
        }

        private static void SeekSet(this ref OutBuf @out, long p)
        {
            if (p == @out.off)
            {
                return;
            }
            @out.Flush();
            {
                var (_, err) = @out.f.Seek(p, 0L);

                if (err != null)
                {
                    Exitf("seeking to %d in %s: %v", p, @out.f.Name(), err);
                }

            }
            @out.off = p;
        }

        private static long Offset(this ref OutBuf @out)
        {
            return @out.off;
        }

        // Write writes the contents of v to the buffer.
        //
        // As Write is backed by a bufio.Writer, callers do not have
        // to explicitly handle the returned error as long as Flush is
        // eventually called.
        private static (long, error) Write(this ref OutBuf @out, slice<byte> v)
        {
            var (n, err) = @out.w.Write(v);
            @out.off += int64(n);
            return (n, err);
        }

        private static void Write8(this ref OutBuf @out, byte v)
        {
            {
                var err = @out.w.WriteByte(v);

                if (err == null)
                {
                    @out.off++;
                }

            }
        }

        private static void Write16(this ref OutBuf @out, ushort v)
        {
            @out.arch.ByteOrder.PutUint16(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..2L]);
        }

        private static void Write32(this ref OutBuf @out, uint v)
        {
            @out.arch.ByteOrder.PutUint32(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..4L]);
        }

        private static void Write32b(this ref OutBuf @out, uint v)
        {
            binary.BigEndian.PutUint32(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..4L]);
        }

        private static void Write64(this ref OutBuf @out, ulong v)
        {
            @out.arch.ByteOrder.PutUint64(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..8L]);
        }

        private static void Write64b(this ref OutBuf @out, ulong v)
        {
            binary.BigEndian.PutUint64(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..8L]);
        }

        private static void WriteString(this ref OutBuf @out, @string s)
        {
            var (n, _) = @out.w.WriteString(s);
            @out.off += int64(n);
        }

        // WriteStringN writes the first n bytes of s.
        // If n is larger than len(s) then it is padded with zero bytes.
        private static void WriteStringN(this ref OutBuf @out, @string s, long n)
        {
            @out.WriteStringPad(s, n, zeros[..]);
        }

        // WriteStringPad writes the first n bytes of s.
        // If n is larger than len(s) then it is padded with the bytes in pad (repeated as needed).
        private static void WriteStringPad(this ref OutBuf @out, @string s, long n, slice<byte> pad)
        {
            if (len(s) >= n)
            {
                @out.WriteString(s[..n]);
            }
            else
            {
                @out.WriteString(s);
                n -= len(s);
                while (n > len(pad))
                {
                    @out.Write(pad);
                    n -= len(pad);

                }

                @out.Write(pad[..n]);
            }
        }

        private static void Flush(this ref OutBuf @out)
        {
            {
                var err = @out.w.Flush();

                if (err != null)
                {
                    Exitf("flushing %s: %v", @out.f.Name(), err);
                }

            }
        }
    }
}}}}
