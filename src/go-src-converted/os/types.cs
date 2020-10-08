// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:45:21 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\types.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Getpagesize returns the underlying system's memory page size.
        public static long Getpagesize()
        {
            return syscall.Getpagesize();
        }

        // File represents an open file descriptor.
        public partial struct File
        {
            public ref ptr<file> ptr<file> => ref ptr<file>_ptr; // os specific
        }

        // A FileInfo describes a file and is returned by Stat and Lstat.
        public partial interface FileInfo
        {
            void Name(); // base name of the file
            void Size(); // length in bytes for regular files; system-dependent for others
            void Mode(); // file mode bits
            void ModTime(); // modification time
            void IsDir(); // abbreviation for Mode().IsDir()
            void Sys(); // underlying data source (can return nil)
        }

        // A FileMode represents a file's mode and permission bits.
        // The bits have the same definition on all systems, so that
        // information about files can be moved from one system
        // to another portably. Not all bits apply to all systems.
        // The only required bit is ModeDir for directories.
        public partial struct FileMode // : uint
        {
        }

        // The defined file mode bits are the most significant bits of the FileMode.
        // The nine least-significant bits are the standard Unix rwxrwxrwx permissions.
        // The values of these bits should be considered part of the public API and
        // may be used in wire protocols or disk representations: they must not be
        // changed, although new bits might be added.
 
        // The single letters are the abbreviations
        // used by the String method's formatting.
        public static readonly FileMode ModeDir = (FileMode)1L << (int)((32L - 1L - iota)); // d: is a directory
        public static readonly var ModeAppend = (var)0; // a: append-only
        public static readonly var ModeExclusive = (var)1; // l: exclusive use
        public static readonly var ModeTemporary = (var)2; // T: temporary file; Plan 9 only
        public static readonly var ModeSymlink = (var)3; // L: symbolic link
        public static readonly var ModeDevice = (var)4; // D: device file
        public static readonly var ModeNamedPipe = (var)5; // p: named pipe (FIFO)
        public static readonly var ModeSocket = (var)6; // S: Unix domain socket
        public static readonly var ModeSetuid = (var)7; // u: setuid
        public static readonly var ModeSetgid = (var)8; // g: setgid
        public static readonly var ModeCharDevice = (var)9; // c: Unix character device, when ModeDevice is set
        public static readonly var ModeSticky = (var)10; // t: sticky
        public static readonly ModeType ModeIrregular = (ModeType)ModeDir | ModeSymlink | ModeNamedPipe | ModeSocket | ModeDevice | ModeCharDevice | ModeIrregular;

        public static readonly FileMode ModePerm = (FileMode)0777L; // Unix permission bits

        public static @string String(this FileMode m)
        {
            const @string str = (@string)"dalTLDpSugct?";

            array<byte> buf = new array<byte>(32L); // Mode is uint32.
            long w = 0L;
            {
                var i__prev1 = i;
                var c__prev1 = c;

                foreach (var (__i, __c) in str)
                {
                    i = __i;
                    c = __c;
                    if (m & (1L << (int)(uint(32L - 1L - i))) != 0L)
                    {
                        buf[w] = byte(c);
                        w++;
                    }

                }

                i = i__prev1;
                c = c__prev1;
            }

            if (w == 0L)
            {
                buf[w] = '-';
                w++;
            }

            const @string rwx = (@string)"rwxrwxrwx";

            {
                var i__prev1 = i;
                var c__prev1 = c;

                foreach (var (__i, __c) in rwx)
                {
                    i = __i;
                    c = __c;
                    if (m & (1L << (int)(uint(9L - 1L - i))) != 0L)
                    {
                        buf[w] = byte(c);
                    }
                    else
                    {
                        buf[w] = '-';
                    }

                    w++;

                }

                i = i__prev1;
                c = c__prev1;
            }

            return string(buf[..w]);

        }

        // IsDir reports whether m describes a directory.
        // That is, it tests for the ModeDir bit being set in m.
        public static bool IsDir(this FileMode m)
        {
            return m & ModeDir != 0L;
        }

        // IsRegular reports whether m describes a regular file.
        // That is, it tests that no mode type bits are set.
        public static bool IsRegular(this FileMode m)
        {
            return m & ModeType == 0L;
        }

        // Perm returns the Unix permission bits in m.
        public static FileMode Perm(this FileMode m)
        {
            return m & ModePerm;
        }

        private static @string Name(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return fs.name;
        }
        private static bool IsDir(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return fs.Mode().IsDir();
        }

        // SameFile reports whether fi1 and fi2 describe the same file.
        // For example, on Unix this means that the device and inode fields
        // of the two underlying structures are identical; on other systems
        // the decision may be based on the path names.
        // SameFile only applies to results returned by this package's Stat.
        // It returns false in other cases.
        public static bool SameFile(FileInfo fi1, FileInfo fi2)
        {
            ptr<fileStat> (fs1, ok1) = fi1._<ptr<fileStat>>();
            ptr<fileStat> (fs2, ok2) = fi2._<ptr<fileStat>>();
            if (!ok1 || !ok2)
            {
                return false;
            }

            return sameFile(fs1, fs2);

        }
    }
}
