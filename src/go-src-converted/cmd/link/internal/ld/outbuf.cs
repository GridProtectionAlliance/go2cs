// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:50:12 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\outbuf.go
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using log = go.log_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // If fallocate is not supported on this platform, return this error.
        // Note this is the same error returned by filesystems that don't support
        // fallocate, and that is intentional. The error is ignored where needed, and
        // OutBuf writes to heap memory.
        private static readonly @string fallocateNotSupportedErr = (@string)"operation not supported";

        private static readonly long outbufMode = (long)0775L;

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
        // Third, it also mmaps the output file (if available). The intended usage is:
        // - Mmap the output file
        // - Write the content
        // - possibly apply any edits in the output buffer
        // - possibly write more content to the file. These writes take place in a heap
        //   backed buffer that will get synced to disk.
        // - Munmap the output file
        //
        // And finally, it provides a mechanism by which you can multithread the
        // writing of output files. This mechanism is accomplished by copying a OutBuf,
        // and using it in the thread/goroutine.
        //
        // Parallel OutBuf is intended to be used like:
        //
        //  func write(out *OutBuf) {
        //    var wg sync.WaitGroup
        //    for i := 0; i < 10; i++ {
        //      wg.Add(1)
        //      view, err := out.View(start[i])
        //      if err != nil {
        //         // handle output
        //         continue
        //      }
        //      go func(out *OutBuf, i int) {
        //        // do output
        //        wg.Done()
        //      }(view, i)
        //    }
        //    wg.Wait()
        //  }


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
        // Third, it also mmaps the output file (if available). The intended usage is:
        // - Mmap the output file
        // - Write the content
        // - possibly apply any edits in the output buffer
        // - possibly write more content to the file. These writes take place in a heap
        //   backed buffer that will get synced to disk.
        // - Munmap the output file
        //
        // And finally, it provides a mechanism by which you can multithread the
        // writing of output files. This mechanism is accomplished by copying a OutBuf,
        // and using it in the thread/goroutine.
        //
        // Parallel OutBuf is intended to be used like:
        //
        //  func write(out *OutBuf) {
        //    var wg sync.WaitGroup
        //    for i := 0; i < 10; i++ {
        //      wg.Add(1)
        //      view, err := out.View(start[i])
        //      if err != nil {
        //         // handle output
        //         continue
        //      }
        //      go func(out *OutBuf, i int) {
        //        // do output
        //        wg.Done()
        //      }(view, i)
        //    }
        //    wg.Wait()
        //  }
        public partial struct OutBuf
        {
            public ptr<sys.Arch> arch;
            public long off;
            public slice<byte> buf; // backing store of mmap'd output file
            public slice<byte> heap; // backing store for non-mmapped data

            public @string name;
            public ptr<os.File> f;
            public array<byte> encbuf; // temp buffer used by WriteN methods
            public bool isView; // true if created from View()
        }

        private static error Open(this ptr<OutBuf> _addr_@out, @string name)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.f != null)
            {
                return error.As(errors.New("cannot open more than one file"))!;
            }

            var (f, err) = os.OpenFile(name, os.O_RDWR | os.O_CREATE | os.O_TRUNC, outbufMode);
            if (err != null)
            {
                return error.As(err)!;
            }

            @out.off = 0L;
            @out.name = name;
            @out.f = f;
            return error.As(null!)!;

        }

        public static ptr<OutBuf> NewOutBuf(ptr<sys.Arch> _addr_arch)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return addr(new OutBuf(arch:arch,));
        }

        private static var viewError = errors.New("output not mmapped");

        private static (ptr<OutBuf>, error) View(this ptr<OutBuf> _addr_@out, ulong start)
        {
            ptr<OutBuf> _p0 = default!;
            error _p0 = default!;
            ref OutBuf @out = ref _addr_@out.val;

            return (addr(new OutBuf(arch:out.arch,name:out.name,buf:out.buf,heap:out.heap,off:int64(start),isView:true,)), error.As(null!)!);
        }

        private static var viewCloseError = errors.New("cannot Close OutBuf from View");

        private static error Close(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.isView)
            {
                return error.As(viewCloseError)!;
            }

            if (@out.isMmapped())
            {
                @out.copyHeap();
                @out.munmap();
            }

            if (@out.f == null)
            {
                return error.As(null!)!;
            }

            if (len(@out.heap) != 0L)
            {
                {
                    var (_, err) = @out.f.Write(@out.heap);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            }

            {
                var err = @out.f.Close();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            @out.f = null;
            return error.As(null!)!;

        }

        // isMmapped returns true if the OutBuf is mmaped.
        private static bool isMmapped(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            return len(@out.buf) != 0L;
        }

        // copyHeap copies the heap to the mmapped section of memory, returning true if
        // a copy takes place.
        private static bool copyHeap(this ptr<OutBuf> _addr_@out) => func((_, panic, __) =>
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (!@out.isMmapped())
            { // only valuable for mmapped OutBufs.
                return false;

            }

            if (@out.isView)
            {
                panic("can't copyHeap a view");
            }

            var bufLen = len(@out.buf);
            var heapLen = len(@out.heap);
            var total = uint64(bufLen + heapLen);
            @out.munmap();
            if (heapLen != 0L)
            {
                {
                    var err = @out.Mmap(total);

                    if (err != null)
                    {
                        panic(err);
                    }

                }

                copy(@out.buf[bufLen..], @out.heap[..heapLen]);
                @out.heap = @out.heap[..0L];

            }

            return true;

        });

        // maxOutBufHeapLen limits the growth of the heap area.
        private static readonly long maxOutBufHeapLen = (long)10L << (int)(20L);

        // writeLoc determines the write location if a buffer is mmaped.
        // We maintain two write buffers, an mmapped section, and a heap section for
        // writing. When the mmapped section is full, we switch over the heap memory
        // for writing.


        // writeLoc determines the write location if a buffer is mmaped.
        // We maintain two write buffers, an mmapped section, and a heap section for
        // writing. When the mmapped section is full, we switch over the heap memory
        // for writing.
        private static (long, slice<byte>) writeLoc(this ptr<OutBuf> _addr_@out, long lenToWrite) => func((_, panic, __) =>
        {
            long _p0 = default;
            slice<byte> _p0 = default;
            ref OutBuf @out = ref _addr_@out.val;
 
            // See if we have enough space in the mmaped area.
            var bufLen = int64(len(@out.buf));
            if (@out.off + lenToWrite <= bufLen)
            {
                return (@out.off, @out.buf);
            } 

            // Not enough space in the mmaped area, write to heap area instead.
            var heapPos = @out.off - bufLen;
            var heapLen = int64(len(@out.heap));
            var lenNeeded = heapPos + lenToWrite;
            if (lenNeeded > heapLen)
            { // do we need to grow the heap storage?
                // The heap variables aren't protected by a mutex. For now, just bomb if you
                // try to use OutBuf in parallel. (Note this probably could be fixed.)
                if (@out.isView)
                {
                    panic("cannot write to heap in parallel");
                } 
                // See if our heap would grow to be too large, and if so, copy it to the end
                // of the mmapped area.
                if (heapLen > maxOutBufHeapLen && @out.copyHeap())
                {
                    heapPos = 0L;
                    heapLen = 0L;
                    lenNeeded = lenToWrite;

                }

                @out.heap = append(@out.heap, make_slice<byte>(lenNeeded - heapLen));

            }

            return (heapPos, @out.heap);

        });

        private static void SeekSet(this ptr<OutBuf> _addr_@out, long p)
        {
            ref OutBuf @out = ref _addr_@out.val;

            @out.off = p;
        }

        private static long Offset(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            return @out.off;
        }

        // Write writes the contents of v to the buffer.
        private static (long, error) Write(this ptr<OutBuf> _addr_@out, slice<byte> v)
        {
            long _p0 = default;
            error _p0 = default!;
            ref OutBuf @out = ref _addr_@out.val;

            var n = len(v);
            var (pos, buf) = @out.writeLoc(int64(n));
            copy(buf[pos..], v);
            @out.off += int64(n);
            return (n, error.As(null!)!);
        }

        private static void Write8(this ptr<OutBuf> _addr_@out, byte v)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var (pos, buf) = @out.writeLoc(1L);
            buf[pos] = v;
            @out.off++;
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

            var (pos, buf) = @out.writeLoc(int64(len(s)));
            var n = copy(buf[pos..], s);
            if (n != len(s))
            {
                log.Fatalf("WriteString truncated. buffer size: %d, offset: %d, len(s)=%d", len(@out.buf), @out.off, len(s));
            }

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
        private static void WriteSym(this ptr<OutBuf> _addr_@out, ptr<loader.Loader> _addr_ldr, loader.Sym s)
        {
            ref OutBuf @out = ref _addr_@out.val;
            ref loader.Loader ldr = ref _addr_ldr.val;

            var P = ldr.Data(s);
            var n = int64(len(P));
            var (pos, buf) = @out.writeLoc(n);
            copy(buf[pos..], P);
            @out.off += n;
            ldr.SetOutData(s, buf[pos..pos + n]);
        }
    }
}}}}
