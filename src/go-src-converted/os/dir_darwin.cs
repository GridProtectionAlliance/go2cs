// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:06:57 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\dir_darwin.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Auxiliary information if the File describes a directory
        private partial struct dirInfo
        {
            public System.UIntPtr dir; // Pointer to DIR structure from dirent.h
        }

        private static void close(this ptr<dirInfo> _addr_d)
        {
            ref dirInfo d = ref _addr_d.val;

            if (d.dir == 0L)
            {
                return ;
            }

            closedir(d.dir);
            d.dir = 0L;

        }

        private static (slice<@string>, error) readdirnames(this ptr<File> _addr_f, long n)
        {
            slice<@string> names = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            if (f.dirinfo == null)
            {
                var (dir, call, errno) = f.pfd.OpenDir();
                if (errno != null)
                {
                    return (null, error.As(wrapSyscallError(call, errno))!);
                }

                f.dirinfo = addr(new dirInfo(dir:dir,));

            }

            var d = f.dirinfo;

            var size = n;
            if (size <= 0L)
            {
                size = 100L;
                n = -1L;
            }

            names = make_slice<@string>(0L, size);
            ref syscall.Dirent dirent = ref heap(out ptr<syscall.Dirent> _addr_dirent);
            ptr<syscall.Dirent> entptr;
            while (len(names) < size || n == -1L)
            {
                {
                    var res = readdir_r(d.dir, _addr_dirent, _addr_entptr);

                    if (res != 0L)
                    {
                        return (names, error.As(wrapSyscallError("readdir", syscall.Errno(res)))!);
                    }

                }

                if (entptr == null)
                { // EOF
                    break;

                }

                if (dirent.Ino == 0L)
                {
                    continue;
                }

                ptr<array<byte>> name = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_dirent.Name))[..];
                foreach (var (i, c) in name)
                {
                    if (c == 0L)
                    {
                        name = name[..i];
                        break;
                    }

                } 
                // Check for useless names before allocating a string.
                if (string(name) == "." || string(name) == "..")
                {
                    continue;
                }

                names = append(names, string(name));
                runtime.KeepAlive(f);

            }

            if (n >= 0L && len(names) == 0L)
            {
                return (names, error.As(io.EOF)!);
            }

            return (names, error.As(null!)!);

        }

        // Implemented in syscall/syscall_darwin.go.

        //go:linkname closedir syscall.closedir
        private static error closedir(System.UIntPtr dir)
;

        //go:linkname readdir_r syscall.readdir_r
        private static long readdir_r(System.UIntPtr dir, ptr<syscall.Dirent> entry, ptr<ptr<syscall.Dirent>> result)
;
    }
}
