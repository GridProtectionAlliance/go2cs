// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,go1.13

// package unix -- go2cs converted at 2020 October 08 04:46:47 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin.1_13.go
using @unsafe = go.@unsafe_package;

using unsafeheader = go.golang.org.x.sys.@internal.unsafeheader_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        //sys    closedir(dir uintptr) (err error)
        //sys    readdir_r(dir uintptr, entry *Dirent, result **Dirent) (res Errno)
        private static (System.UIntPtr, error) fdopendir(long fd)
        {
            System.UIntPtr dir = default;
            error err = default!;

            var (r0, _, e1) = syscall_syscallPtr(funcPC(libc_fdopendir_trampoline), uintptr(fd), 0L, 0L);
            dir = uintptr(r0);
            if (e1 != 0L)
            {
                err = errnoErr(e1);
            }
            return ;

        }

        private static void libc_fdopendir_trampoline()
;

        //go:linkname libc_fdopendir libc_fdopendir
        //go:cgo_import_dynamic libc_fdopendir fdopendir "/usr/lib/libSystem.B.dylib"

        public static (long, error) Getdirentries(long fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref System.UIntPtr basep = ref _addr_basep.val;
 
            // Simulate Getdirentries using fdopendir/readdir_r/closedir.
            // We store the number of entries to skip in the seek
            // offset of fd. See issue #31368.
            // It's not the full required semantics, but should handle the case
            // of calling Getdirentries or ReadDirent repeatedly.
            // It won't handle assigning the results of lseek to *basep, or handle
            // the directory being edited underfoot.
            var (skip, err) = Seek(fd, 0L, 1L);
            if (err != null)
            {>>MARKER:FUNCTION_libc_fdopendir_trampoline_BLOCK_PREFIX<<
                return (0L, error.As(err)!);
            } 

            // We need to duplicate the incoming file descriptor
            // because the caller expects to retain control of it, but
            // fdopendir expects to take control of its argument.
            // Just Dup'ing the file descriptor is not enough, as the
            // result shares underlying state. Use Openat to make a really
            // new file descriptor referring to the same directory.
            var (fd2, err) = Openat(fd, ".", O_RDONLY, 0L);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            var (d, err) = fdopendir(fd2);
            if (err != null)
            {
                Close(fd2);
                return (0L, error.As(err)!);
            }

            defer(closedir(d));

            long cnt = default;
            while (true)
            {
                ref Dirent entry = ref heap(out ptr<Dirent> _addr_entry);
                ptr<Dirent> entryp;
                var e = readdir_r(d, _addr_entry, _addr_entryp);
                if (e != 0L)
                {
                    return (n, error.As(errnoErr(e))!);
                }

                if (entryp == null)
                {
                    break;
                }

                if (skip > 0L)
                {
                    skip--;
                    cnt++;
                    continue;
                }

                var reclen = int(entry.Reclen);
                if (reclen > len(buf))
                { 
                    // Not enough room. Return for now.
                    // The counter will let us know where we should start up again.
                    // Note: this strategy for suspending in the middle and
                    // restarting is O(n^2) in the length of the directory. Oh well.
                    break;

                } 

                // Copy entry into return buffer.
                ref slice<byte> s = ref heap(out ptr<slice<byte>> _addr_s);
                var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_s));
                hdr.Data = @unsafe.Pointer(_addr_entry);
                hdr.Cap = reclen;
                hdr.Len = reclen;
                copy(buf, s);

                buf = buf[reclen..];
                n += reclen;
                cnt++;

            } 
            // Set the seek offset of the input fd to record
            // how many files we've already returned.
 
            // Set the seek offset of the input fd to record
            // how many files we've already returned.
            _, err = Seek(fd, cnt, 0L);
            if (err != null)
            {
                return (n, error.As(err)!);
            }

            return (n, error.As(null!)!);

        });
    }
}}}}}}
