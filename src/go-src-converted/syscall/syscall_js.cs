// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package syscall -- go2cs converted at 2020 October 08 03:27:24 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_js.go
using oserror = go.@internal.oserror_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly long direntSize = (long)8L + 8L + 2L + 256L;



        public partial struct Dirent
        {
            public ushort Reclen;
            public array<byte> Name;
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return (1L, true);
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            var (reclen, ok) = direntReclen(buf);
            if (!ok)
            {
                return (0L, false);
            }

            return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);

        }

        public static readonly long PathMax = (long)256L;

        // An Errno is an unsigned number describing an error condition.
        // It implements the error interface. The zero Errno is by convention
        // a non-error, so code to convert from Errno to error should use:
        //    err = nil
        //    if errno != 0 {
        //        err = errno
        //    }
        //
        // Errno values can be tested against error values from the os package
        // using errors.Is. For example:
        //
        //    _, _, err := syscall.Syscall(...)
        //    if errors.Is(err, os.ErrNotExist) ...


        // An Errno is an unsigned number describing an error condition.
        // It implements the error interface. The zero Errno is by convention
        // a non-error, so code to convert from Errno to error should use:
        //    err = nil
        //    if errno != 0 {
        //        err = errno
        //    }
        //
        // Errno values can be tested against error values from the os package
        // using errors.Is. For example:
        //
        //    _, _, err := syscall.Syscall(...)
        //    if errors.Is(err, os.ErrNotExist) ...
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

        public static bool Is(this Errno e, error target)
        {

            if (target == oserror.ErrPermission) 
                return e == EACCES || e == EPERM;
            else if (target == oserror.ErrExist) 
                return e == EEXIST || e == ENOTEMPTY;
            else if (target == oserror.ErrNotExist) 
                return e == ENOENT;
                        return false;

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

        private static readonly Signal _ = (Signal)iota;
        public static readonly var SIGCHLD = (var)0;
        public static readonly var SIGINT = (var)1;
        public static readonly var SIGKILL = (var)2;
        public static readonly var SIGTRAP = (var)3;
        public static readonly var SIGQUIT = (var)4;
        public static readonly var SIGTERM = (var)5;


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

        public static readonly long Stdin = (long)0L;
        public static readonly long Stdout = (long)1L;
        public static readonly long Stderr = (long)2L;


        public static readonly long O_RDONLY = (long)0L;
        public static readonly long O_WRONLY = (long)1L;
        public static readonly long O_RDWR = (long)2L;

        public static readonly long O_CREAT = (long)0100L;
        public static readonly var O_CREATE = (var)O_CREAT;
        public static readonly long O_TRUNC = (long)01000L;
        public static readonly long O_APPEND = (long)02000L;
        public static readonly long O_EXCL = (long)0200L;
        public static readonly long O_SYNC = (long)010000L;

        public static readonly long O_CLOEXEC = (long)0L;


        public static readonly long F_DUPFD = (long)0L;
        public static readonly long F_GETFD = (long)1L;
        public static readonly long F_SETFD = (long)2L;
        public static readonly long F_GETFL = (long)3L;
        public static readonly long F_SETFL = (long)4L;
        public static readonly long F_GETOWN = (long)5L;
        public static readonly long F_SETOWN = (long)6L;
        public static readonly long F_GETLK = (long)7L;
        public static readonly long F_SETLK = (long)8L;
        public static readonly long F_SETLKW = (long)9L;
        public static readonly long F_RGETLK = (long)10L;
        public static readonly long F_RSETLK = (long)11L;
        public static readonly long F_CNVT = (long)12L;
        public static readonly long F_RSETLKW = (long)13L;

        public static readonly long F_RDLCK = (long)1L;
        public static readonly long F_WRLCK = (long)2L;
        public static readonly long F_UNLCK = (long)3L;
        public static readonly long F_UNLKSYS = (long)4L;


        public static readonly long S_IFMT = (long)0000370000L;
        public static readonly long S_IFSHM_SYSV = (long)0000300000L;
        public static readonly long S_IFSEMA = (long)0000270000L;
        public static readonly long S_IFCOND = (long)0000260000L;
        public static readonly long S_IFMUTEX = (long)0000250000L;
        public static readonly long S_IFSHM = (long)0000240000L;
        public static readonly long S_IFBOUNDSOCK = (long)0000230000L;
        public static readonly long S_IFSOCKADDR = (long)0000220000L;
        public static readonly long S_IFDSOCK = (long)0000210000L;

        public static readonly long S_IFSOCK = (long)0000140000L;
        public static readonly long S_IFLNK = (long)0000120000L;
        public static readonly long S_IFREG = (long)0000100000L;
        public static readonly long S_IFBLK = (long)0000060000L;
        public static readonly long S_IFDIR = (long)0000040000L;
        public static readonly long S_IFCHR = (long)0000020000L;
        public static readonly long S_IFIFO = (long)0000010000L;

        public static readonly long S_UNSUP = (long)0000370000L;

        public static readonly long S_ISUID = (long)0004000L;
        public static readonly long S_ISGID = (long)0002000L;
        public static readonly long S_ISVTX = (long)0001000L;

        public static readonly long S_IREAD = (long)0400L;
        public static readonly long S_IWRITE = (long)0200L;
        public static readonly long S_IEXEC = (long)0100L;

        public static readonly long S_IRWXU = (long)0700L;
        public static readonly long S_IRUSR = (long)0400L;
        public static readonly long S_IWUSR = (long)0200L;
        public static readonly long S_IXUSR = (long)0100L;

        public static readonly long S_IRWXG = (long)070L;
        public static readonly long S_IRGRP = (long)040L;
        public static readonly long S_IWGRP = (long)020L;
        public static readonly long S_IXGRP = (long)010L;

        public static readonly long S_IRWXO = (long)07L;
        public static readonly long S_IROTH = (long)04L;
        public static readonly long S_IWOTH = (long)02L;
        public static readonly long S_IXOTH = (long)01L;


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
        // Not supported - just enough for package os.

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

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            Errno err = default;

            return (0L, 0L, ENOSYS);
        }

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            Errno err = default;

            return (0L, 0L, ENOSYS);
        }

        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            Errno err = default;

            return (0L, 0L, ENOSYS);
        }

        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            Errno err = default;

            return (0L, 0L, ENOSYS);
        }

        public static (@string, error) Sysctl(@string key)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (key == "kern.hostname")
            {
                return ("js", error.As(null!)!);
            }

            return ("", error.As(ENOSYS)!);

        }

        public static readonly var ImplementsGetwd = (var)true;



        public static (@string, error) Getwd()
        {
            @string wd = default;
            error err = default!;

            array<byte> buf = new array<byte>(PathMax);
            var (n, err) = Getcwd(buf[0L..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (string(buf[..n]), error.As(null!)!);

        }

        public static long Getuid()
        {
            return jsProcess.Call("getuid").Int();
        }

        public static long Getgid()
        {
            return jsProcess.Call("getgid").Int();
        }

        public static long Geteuid()
        {
            return jsProcess.Call("geteuid").Int();
        }

        public static long Getegid()
        {
            return jsProcess.Call("getegid").Int();
        }

        public static (slice<long>, error) Getgroups() => func((defer, _, __) =>
        {
            slice<long> groups = default;
            error err = default!;

            defer(recoverErr(_addr_err));
            var array = jsProcess.Call("getgroups");
            groups = make_slice<long>(array.Length());
            foreach (var (i) in groups)
            {
                groups[i] = array.Index(i).Int();
            }
            return (groups, error.As(null!)!);

        });

        public static long Getpid()
        {
            return jsProcess.Get("pid").Int();
        }

        public static long Getppid()
        {
            return jsProcess.Get("ppid").Int();
        }

        public static long Umask(long mask)
        {
            long oldmask = default;

            return jsProcess.Call("umask", mask).Int();
        }

        public static error Gettimeofday(ptr<Timeval> _addr_tv)
        {
            ref Timeval tv = ref _addr_tv.val;

            return error.As(ENOSYS)!;
        }

        public static error Kill(long pid, Signal signum)
        {
            return error.As(ENOSYS)!;
        }
        public static (long, error) Sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            return (0L, error.As(ENOSYS)!);
        }
        public static (long, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            System.UIntPtr handle = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            return (0L, 0L, error.As(ENOSYS)!);
        }
        public static (long, error) Wait4(long pid, ptr<WaitStatus> _addr_wstatus, long options, ptr<Rusage> _addr_rusage)
        {
            long wpid = default;
            error err = default!;
            ref WaitStatus wstatus = ref _addr_wstatus.val;
            ref Rusage rusage = ref _addr_rusage.val;

            return (0L, error.As(ENOSYS)!);
        }

        public partial struct Iovec
        {
        } // dummy

        public partial struct Timespec
        {
            public long Sec;
            public long Nsec;
        }

        public partial struct Timeval
        {
            public long Sec;
            public long Usec;
        }

        private static Timespec setTimespec(long sec, long nsec)
        {
            return new Timespec(Sec:sec,Nsec:nsec);
        }

        private static Timeval setTimeval(long sec, long usec)
        {
            return new Timeval(Sec:sec,Usec:usec);
        }
    }
}
