// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:12 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_nacl.go
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        //sys    naclClose(fd int) (err error) = sys_close
        //sys    naclFstat(fd int, stat *Stat_t) (err error) = sys_fstat
        //sys    naclRead(fd int, b []byte) (n int, err error) = sys_read
        //sys    naclSeek(fd int, off *int64, whence int) (err error) = sys_lseek
        //sys    naclGetRandomBytes(b []byte) (err error) = sys_get_random_bytes
        private static readonly long direntSize = 8L + 8L + 2L + 256L;

        // native_client/src/trusted/service_runtime/include/sys/dirent.h


        // native_client/src/trusted/service_runtime/include/sys/dirent.h
        public partial struct Dirent
        {
            public long Ino;
            public long Off;
            public ushort Reclen;
            public array<byte> Name;
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            var (reclen, ok) = direntReclen(buf);
            if (!ok)
            {
                return (0L, false);
            }
            return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);
        }

        public static readonly long PathMax = 256L;

        // An Errno is an unsigned number describing an error condition.
        // It implements the error interface. The zero Errno is by convention
        // a non-error, so code to convert from Errno to error should use:
        //    err = nil
        //    if errno != 0 {
        //        err = errno
        //    }


        // An Errno is an unsigned number describing an error condition.
        // It implements the error interface. The zero Errno is by convention
        // a non-error, so code to convert from Errno to error should use:
        //    err = nil
        //    if errno != 0 {
        //        err = errno
        //    }
        public partial struct Errno // : System.UIntPtr
        {
        }

        public static @string Error(this Errno e)
        {
            if (0L <= int(e) && int(e) < len(errorstr))
            {
                var s = errorstr[e];
                if (s != "")
                {
                    return s;
                }
            }
            return "errno " + itoa(int(e));
        }

        public static bool Temporary(this Errno e)
        {
            return e == EINTR || e == EMFILE || e.Timeout();
        }

        public static bool Timeout(this Errno e)
        {
            return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
        }

        // A Signal is a number describing a process signal.
        // It implements the os.Signal interface.
        public partial struct Signal // : long
        {
        }

        private static readonly Signal _ = iota;
        public static readonly var SIGCHLD = 0;
        public static readonly var SIGINT = 1;
        public static readonly var SIGKILL = 2;
        public static readonly var SIGTRAP = 3;
        public static readonly var SIGQUIT = 4;

        public static void Signal(this Signal s)
        {
        }

        public static @string String(this Signal s)
        {
            if (0L <= s && int(s) < len(signals))
            {
                var str = signals[s];
                if (str != "")
                {
                    return str;
                }
            }
            return "signal " + itoa(int(s));
        }

        private static array<@string> signals = new array<@string>(new @string[] {  });

        // File system

        public static readonly long Stdin = 0L;
        public static readonly long Stdout = 1L;
        public static readonly long Stderr = 2L;

        // native_client/src/trusted/service_runtime/include/sys/fcntl.h
        public static readonly long O_RDONLY = 0L;
        public static readonly long O_WRONLY = 1L;
        public static readonly long O_RDWR = 2L;
        public static readonly long O_ACCMODE = 3L;

        public static readonly long O_CREAT = 0100L;
        public static readonly var O_CREATE = O_CREAT; // for ken
        public static readonly long O_TRUNC = 01000L;
        public static readonly long O_APPEND = 02000L;
        public static readonly long O_EXCL = 0200L;
        public static readonly long O_NONBLOCK = 04000L;
        public static readonly var O_NDELAY = O_NONBLOCK;
        public static readonly long O_SYNC = 010000L;
        public static readonly var O_FSYNC = O_SYNC;
        public static readonly long O_ASYNC = 020000L;

        public static readonly long O_CLOEXEC = 0L;

        public static readonly long FD_CLOEXEC = 1L;

        // native_client/src/trusted/service_runtime/include/sys/fcntl.h
        public static readonly long F_DUPFD = 0L;
        public static readonly long F_GETFD = 1L;
        public static readonly long F_SETFD = 2L;
        public static readonly long F_GETFL = 3L;
        public static readonly long F_SETFL = 4L;
        public static readonly long F_GETOWN = 5L;
        public static readonly long F_SETOWN = 6L;
        public static readonly long F_GETLK = 7L;
        public static readonly long F_SETLK = 8L;
        public static readonly long F_SETLKW = 9L;
        public static readonly long F_RGETLK = 10L;
        public static readonly long F_RSETLK = 11L;
        public static readonly long F_CNVT = 12L;
        public static readonly long F_RSETLKW = 13L;

        public static readonly long F_RDLCK = 1L;
        public static readonly long F_WRLCK = 2L;
        public static readonly long F_UNLCK = 3L;
        public static readonly long F_UNLKSYS = 4L;

        // native_client/src/trusted/service_runtime/include/bits/stat.h
        public static readonly long S_IFMT = 0000370000L;
        public static readonly long S_IFSHM_SYSV = 0000300000L;
        public static readonly long S_IFSEMA = 0000270000L;
        public static readonly long S_IFCOND = 0000260000L;
        public static readonly long S_IFMUTEX = 0000250000L;
        public static readonly long S_IFSHM = 0000240000L;
        public static readonly long S_IFBOUNDSOCK = 0000230000L;
        public static readonly long S_IFSOCKADDR = 0000220000L;
        public static readonly long S_IFDSOCK = 0000210000L;

        public static readonly long S_IFSOCK = 0000140000L;
        public static readonly long S_IFLNK = 0000120000L;
        public static readonly long S_IFREG = 0000100000L;
        public static readonly long S_IFBLK = 0000060000L;
        public static readonly long S_IFDIR = 0000040000L;
        public static readonly long S_IFCHR = 0000020000L;
        public static readonly long S_IFIFO = 0000010000L;

        public static readonly long S_UNSUP = 0000370000L;

        public static readonly long S_ISUID = 0004000L;
        public static readonly long S_ISGID = 0002000L;
        public static readonly long S_ISVTX = 0001000L;

        public static readonly long S_IREAD = 0400L;
        public static readonly long S_IWRITE = 0200L;
        public static readonly long S_IEXEC = 0100L;

        public static readonly long S_IRWXU = 0700L;
        public static readonly long S_IRUSR = 0400L;
        public static readonly long S_IWUSR = 0200L;
        public static readonly long S_IXUSR = 0100L;

        public static readonly long S_IRWXG = 070L;
        public static readonly long S_IRGRP = 040L;
        public static readonly long S_IWGRP = 020L;
        public static readonly long S_IXGRP = 010L;

        public static readonly long S_IRWXO = 07L;
        public static readonly long S_IROTH = 04L;
        public static readonly long S_IWOTH = 02L;
        public static readonly long S_IXOTH = 01L;

        // native_client/src/trusted/service_runtime/include/sys/stat.h
        // native_client/src/trusted/service_runtime/include/machine/_types.h
        public partial struct Stat_t
        {
            public long Dev;
            public ulong Ino;
            public uint Mode;
            public uint Nlink;
            public uint Uid;
            public uint Gid;
            public long Rdev;
            public long Size;
            public int Blksize;
            public int Blocks;
            public long Atime;
            public long AtimeNsec;
            public long Mtime;
            public long MtimeNsec;
            public long Ctime;
            public long CtimeNsec;
        }

        // Processes
        // Not supported on NaCl - just enough for package os.

        public static sync.RWMutex ForkLock = default;

        public partial struct WaitStatus // : uint
        {
        }

        public static bool Exited(this WaitStatus w)
        {
            return false;
        }
        public static long ExitStatus(this WaitStatus w)
        {
            return 0L;
        }
        public static bool Signaled(this WaitStatus w)
        {
            return false;
        }
        public static Signal Signal(this WaitStatus w)
        {
            return 0L;
        }
        public static bool CoreDump(this WaitStatus w)
        {
            return false;
        }
        public static bool Stopped(this WaitStatus w)
        {
            return false;
        }
        public static bool Continued(this WaitStatus w)
        {
            return false;
        }
        public static Signal StopSignal(this WaitStatus w)
        {
            return 0L;
        }
        public static long TrapCause(this WaitStatus w)
        {
            return 0L;
        }

        // XXX made up
        public partial struct Rusage
        {
            public Timeval Utime;
            public Timeval Stime;
        }

        // XXX made up
        public partial struct ProcAttr
        {
            public @string Dir;
            public slice<@string> Env;
            public slice<System.UIntPtr> Files;
            public ptr<SysProcAttr> Sys;
        }

        public partial struct SysProcAttr
        {
        }

        // System

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            return (0L, 0L, ENOSYS);
        }
        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            return (0L, 0L, ENOSYS);
        }
        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            return (0L, 0L, ENOSYS);
        }

        public static (@string, error) Sysctl(@string key)
        {
            if (key == "kern.hostname")
            {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
                return ("naclbox", null);
            }
            return ("", ENOSYS);
        }

        // Unimplemented Unix midden heap.

        public static readonly var ImplementsGetwd = false;



        public static (@string, error) Getwd()
        {
            return ("", ENOSYS);
        }
        public static long Getegid()
        {
            return 1L;
        }
        public static long Geteuid()
        {
            return 1L;
        }
        public static long Getgid()
        {
            return 1L;
        }
        public static (slice<long>, error) Getgroups()
        {
            return (new slice<long>(new long[] { 1 }), null);
        }
        public static long Getppid()
        {
            return 2L;
        }
        public static long Getpid()
        {
            return 3L;
        }
        public static error Gettimeofday(ref Timeval tv)
        {
            return error.As(ENOSYS);
        }
        public static long Getuid()
        {
            return 1L;
        }
        public static error Kill(long pid, Signal signum)
        {
            return error.As(ENOSYS);
        }
        public static (long, error) Sendfile(long outfd, long infd, ref long offset, long count)
        {
            return (0L, ENOSYS);
        }
        public static (long, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ref ProcAttr attr)
        {
            return (0L, 0L, ENOSYS);
        }
        public static (long, error) Wait4(long pid, ref WaitStatus wstatus, long options, ref Rusage rusage)
        {
            return (0L, ENOSYS);
        }
        public static (slice<byte>, error) RouteRIB(long facility, long param)
        {
            return (null, ENOSYS);
        }
        public static (slice<RoutingMessage>, error) ParseRoutingMessage(slice<byte> b)
        {
            return (null, ENOSYS);
        }
        public static (slice<Sockaddr>, error) ParseRoutingSockaddr(RoutingMessage msg)
        {
            return (null, ENOSYS);
        }
        public static (uint, error) SysctlUint32(@string name)
        {
            return (0L, ENOSYS);
        }

        public partial struct Iovec
        {
        } // dummy
    }
}
