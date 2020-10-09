// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:52:23 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\outbuf.go
using bufio = go.bufio_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using binary = go.encoding.binary_package;
using log = go.log_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
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
        //
        // It also mmaps the output file (if available). The intended usage is:
        // - Mmap the output file
        // - Write the content
        // - possibly apply any edits in the output buffer
        // - Munmap the output file
        // - possibly write more content to the file, which will not be edited later.
        public partial struct OutBuf
        {
            public ptr<sys.Arch> arch;
            public long off;
            public ptr<bufio.Writer> w;
            public slice<byte> buf; // backing store of mmap'd output file
            public ptr<os.File> f;
            public array<byte> encbuf; // temp buffer used by WriteN methods
        }

        private static void SeekSet(this ptr<OutBuf> _addr_@out, long p)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (p == @out.off)
            {
                return ;
            }

            if (@out.buf == null)
            {
                @out.Flush();
                {
                    var (_, err) = @out.f.Seek(p, 0L);

                    if (err != null)
                    {
                        Exitf("seeking to %d in %s: %v", p, @out.f.Name(), err);
                    }

                }

            }

            @out.off = p;

        }

        private static long Offset(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            return @out.off;
        }

        // Write writes the contents of v to the buffer.
        //
        // As Write is backed by a bufio.Writer, callers do not have
        // to explicitly handle the returned error as long as Flush is
        // eventually called.
        private static (long, error) Write(this ptr<OutBuf> _addr_@out, slice<byte> v)
        {
            long _p0 = default;
            error _p0 = default!;
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.buf != null)
            {
                var n = copy(@out.buf[@out.off..], v);
                @out.off += int64(n);
                return (n, error.As(null!)!);
            }

            var (n, err) = @out.w.Write(v);
            @out.off += int64(n);
            return (n, error.As(err)!);

        }

        private static void Write8(this ptr<OutBuf> _addr_@out, byte v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.buf != null)
            {
                @out.buf[@out.off] = v;
                @out.off++;
                return ;
            }

            {
                var err = @out.w.WriteByte(v);

                if (err == null)
                {
                    @out.off++;
                }

            }

        }

        // WriteByte is an alias for Write8 to fulfill the io.ByteWriter interface.
        private static error WriteByte(this ptr<OutBuf> _addr_@out, byte v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.Write8(v);
            return error.As(null!)!;
        }

        private static void Write16(this ptr<OutBuf> _addr_@out, ushort v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.arch.ByteOrder.PutUint16(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..2L]);
        }

        private static void Write32(this ptr<OutBuf> _addr_@out, uint v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.arch.ByteOrder.PutUint32(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..4L]);
        }

        private static void Write32b(this ptr<OutBuf> _addr_@out, uint v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            binary.BigEndian.PutUint32(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..4L]);
        }

        private static void Write64(this ptr<OutBuf> _addr_@out, ulong v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.arch.ByteOrder.PutUint64(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..8L]);
        }

        private static void Write64b(this ptr<OutBuf> _addr_@out, ulong v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            binary.BigEndian.PutUint64(@out.encbuf[..], v);
            @out.Write(@out.encbuf[..8L]);
        }

        private static void WriteString(this ptr<OutBuf> _addr_@out, @string s)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.buf != null)
            {
                var n = copy(@out.buf[@out.off..], s);
                if (n != len(s))
                {
                    log.Fatalf("WriteString truncated. buffer size: %d, offset: %d, len(s)=%d", len(@out.buf), @out.off, len(s));
                }

                @out.off += int64(n);
                return ;

            }

            var (n, _) = @out.w.WriteString(s);
            @out.off += int64(n);

        }

        // WriteStringN writes the first n bytes of s.
        // If n is larger than len(s) then it is padded with zero bytes.
        private static void WriteStringN(this ptr<OutBuf> _addr_@out, @string s, long n)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.WriteStringPad(s, n, zeros[..]);
        }

        // WriteStringPad writes the first n bytes of s.
        // If n is larger than len(s) then it is padded with the bytes in pad (repeated as needed).
        private static void WriteStringPad(this ptr<OutBuf> _addr_@out, @string s, long n, slice<byte> pad)
        {
            ref OutBuf @out = ref _addr_@out.val;

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

        // WriteSym writes the content of a Symbol, then changes the Symbol's content
        // to point to the output buffer that we just wrote, so we can apply further
        // edit to the symbol content.
        // If the output file is not Mmap'd, just writes the content.
        private static void WriteSym(this ptr<OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (@out.buf != null)
            {
                var start = @out.off;
                @out.Write(s.P);
                s.P = @out.buf[start..@out.off];
                s.Attr.Set(sym.AttrReadOnly, false);
            }
            else
            {
                @out.Write(s.P);
            }

        }

        private static void Flush(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            error err = default!;
            if (@out.buf != null)
            {
                err = error.As(@out.Msync())!;
            }
            else
            {
                err = error.As(@out.w.Flush())!;
            }

            if (err != null)
            {
                Exitf("flushing %s: %v", @out.f.Name(), err);
            }

        }
    }
}}}}
