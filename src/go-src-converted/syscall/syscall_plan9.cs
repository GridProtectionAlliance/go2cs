// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9 system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package syscall -- go2cs converted at 2020 October 09 05:01:55 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_plan9.go
using oserror = go.@internal.oserror_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly var ImplementsGetwd = true;

        private static readonly long bitSize16 = (long)2L;

        // ErrorString implements Error's String method by returning itself.
        //
        // ErrorString values can be tested against error values from the os package
        // using errors.Is. For example:
        //
        //    _, _, err := syscall.Syscall(...)
        //    if errors.Is(err, os.ErrNotExist) ...


        // ErrorString implements Error's String method by returning itself.
        //
        // ErrorString values can be tested against error values from the os package
        // using errors.Is. For example:
        //
        //    _, _, err := syscall.Syscall(...)
        //    if errors.Is(err, os.ErrNotExist) ...
        public partial struct ErrorString // : @string
        {
        }

        public static @string Error(this ErrorString e)
        {
            return string(e);
        }

        // NewError converts s to an ErrorString, which satisfies the Error interface.
        public static error NewError(@string s)
        {
            return error.As(ErrorString(s))!;
        }

        public static bool Is(this ErrorString e, error target)
        {

            if (target == oserror.ErrPermission) 
                return checkErrMessageContent(e, "permission denied");
            else if (target == oserror.ErrExist) 
                return checkErrMessageContent(e, "exists", "is a directory");
            else if (target == oserror.ErrNotExist) 
                return checkErrMessageContent(e, "does not exist", "not found", "has been removed", "no parent");
                        return false;

        }

        // checkErrMessageContent checks if err message contains one of msgs.
        private static bool checkErrMessageContent(ErrorString e, params @string[] msgs)
        {
            msgs = msgs.Clone();

            foreach (var (_, msg) in msgs)
            {
                if (contains(string(e), msg))
                {
                    return true;
                }

            }
            return false;

        }

        // contains is a local version of strings.Contains. It knows len(sep) > 1.
        private static bool contains(@string s, @string sep)
        {
            var n = len(sep);
            var c = sep[0L];
            for (long i = 0L; i + n <= len(s); i++)
            {
                if (s[i] == c && s[i..i + n] == sep)
                {
                    return true;
                }

            }

            return false;

        }

        public static bool Temporary(this ErrorString e)
        {
            return e == EINTR || e == EMFILE || e.Timeout();
        }

        public static bool Timeout(this ErrorString e)
        {
            return e == EBUSY || e == ETIMEDOUT;
        }

        private static @string emptystring = default;

        // A Note is a string describing a process note.
        // It implements the os.Signal interface.
        public partial struct Note // : @string
        {
        }

        public static void Signal(this Note n)
        {
        }

        public static @string String(this Note n)
        {
            return string(n);
        }

        public static long Stdin = 0L;        public static long Stdout = 1L;        public static long Stderr = 2L;

        // For testing: clients can set this flag to force
        // creation of IPv6 sockets to return EAFNOSUPPORT.
        public static bool SocketDisableIPv6 = default;

        public static (System.UIntPtr, System.UIntPtr, ErrorString) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, ErrorString) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        public static (System.UIntPtr, System.UIntPtr, System.UIntPtr) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, System.UIntPtr) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        //go:nosplit
        private static ulong atoi(slice<byte> b)
        {
            ulong n = default;

            n = 0L;
            for (long i = 0L; i < len(b); i++)
            {>>MARKER:FUNCTION_RawSyscall6_BLOCK_PREFIX<<
                n = n * 10L + uint(b[i] - '0');
            }

            return ;

        }

        private static @string cstring(slice<byte> s)
        {
            foreach (var (i) in s)
            {
                if (s[i] == 0L)
                {>>MARKER:FUNCTION_RawSyscall_BLOCK_PREFIX<<
                    return string(s[0L..i]);
                }

            }
            return string(s);

        }

        private static @string errstr()
        {
            array<byte> buf = new array<byte>(ERRMAX);

            RawSyscall(SYS_ERRSTR, uintptr(@unsafe.Pointer(_addr_buf[0L])), uintptr(len(buf)), 0L);

            buf[len(buf) - 1L] = 0L;
            return cstring(buf[..]);
        }

        private static (ulong, error) readnum(@string path) => func((defer, _, __) =>
        {
            ulong _p0 = default;
            error _p0 = default!;

            array<byte> b = new array<byte>(12L);

            var (fd, e) = Open(path, O_RDONLY);
            if (e != null)
            {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
                return (0L, error.As(e)!);
            }

            defer(Close(fd));

            var (n, e) = Pread(fd, b[..], 0L);

            if (e != null)
            {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
                return (0L, error.As(e)!);
            }

            long m = 0L;
            while (m < n && b[m] == ' ')
            {
                m++;
            }


            return (atoi(b[m..n - 1L]), error.As(null!)!);

        });

        public static long Getpid()
        {
            long pid = default;

            var (n, _) = readnum("#c/pid");
            return int(n);
        }

        public static long Getppid()
        {
            long ppid = default;

            var (n, _) = readnum("#c/ppid");
            return int(n);
        }

        public static (long, error) Read(long fd, slice<byte> p)
        {
            long n = default;
            error err = default!;

            return Pread(fd, p, -1L);
        }

        public static (long, error) Write(long fd, slice<byte> p)
        {
            long n = default;
            error err = default!;

            if (faketime && (fd == 1L || fd == 2L))
            {
                n = faketimeWrite(fd, p);
                if (n < 0L)
                {
                    return (0L, error.As(ErrorString("error"))!);
                }

                return (n, error.As(null!)!);

            }

            return Pwrite(fd, p, -1L);

        }

        private static long ioSync = default;

        //sys    fd2path(fd int, buf []byte) (err error)
        public static (@string, error) Fd2path(long fd)
        {
            @string path = default;
            error err = default!;

            array<byte> buf = new array<byte>(512L);

            var e = fd2path(fd, buf[..]);
            if (e != null)
            {
                return ("", error.As(e)!);
            }

            return (cstring(buf[..]), error.As(null!)!);

        }

        //sys    pipe(p *[2]int32) (err error)
        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(NewError("bad arg in system call"))!;
            }

            ref array<int> pp = ref heap(new array<int>(2L), out ptr<array<int>> _addr_pp);
            err = pipe(_addr_pp);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return ;

        }

        // Underlying system call writes to newoffset via pointer.
        // Implemented in assembly to avoid allocation.
        private static (long, @string) seek(System.UIntPtr placeholder, long fd, long offset, long whence)
;

        public static (long, error) Seek(long fd, long offset, long whence)
        {
            long newoffset = default;
            error err = default!;

            var (newoffset, e) = seek(0L, fd, offset, whence);

            if (newoffset == -1L)
            {>>MARKER:FUNCTION_seek_BLOCK_PREFIX<<
                err = NewError(e);
            }

            return ;

        }

        public static error Mkdir(@string path, uint mode)
        {
            error err = default!;
 
            // If path exists and is not a directory, Create will fail silently.
            // Work around this by rejecting Mkdir if path exists.
            var statbuf = make_slice<byte>(bitSize16); 
            // Remove any trailing slashes from path, otherwise the Stat will
            // fail with ENOTDIR.
            var n = len(path);
            while (n > 1L && path[n - 1L] == '/')
            {
                n--;
            }

            _, err = Stat(path[0L..n], statbuf);
            if (err == null)
            {
                return error.As(EEXIST)!;
            }

            var (fd, err) = Create(path, O_RDONLY, DMDIR | mode);

            if (fd != -1L)
            {
                Close(fd);
            }

            return ;

        }

        public partial struct Waitmsg
        {
            public long Pid;
            public array<uint> Time;
            public @string Msg;
        }

        public static bool Exited(this Waitmsg w)
        {
            return true;
        }
        public static bool Signaled(this Waitmsg w)
        {
            return false;
        }

        public static long ExitStatus(this Waitmsg w)
        {
            if (len(w.Msg) == 0L)
            { 
                // a normal exit returns no message
                return 0L;

            }

            return 1L;

        }

        //sys    await(s []byte) (n int, err error)
        public static error Await(ptr<Waitmsg> _addr_w)
        {
            error err = default!;
            ref Waitmsg w = ref _addr_w.val;

            array<byte> buf = new array<byte>(512L);
            array<slice<byte>> f = new array<slice<byte>>(5L);

            var (n, err) = await(buf[..]);

            if (err != null || w == null)
            {
                return ;
            }

            long nf = 0L;
            long p = 0L;
            for (long i = 0L; i < n && nf < len(f) - 1L; i++)
            {
                if (buf[i] == ' ')
                {
                    f[nf] = buf[p..i];
                    p = i + 1L;
                    nf++;
                }

            }

            f[nf] = buf[p..];
            nf++;

            if (nf != len(f))
            {
                return error.As(NewError("invalid wait message"))!;
            }

            w.Pid = int(atoi(f[0L]));
            w.Time[0L] = uint32(atoi(f[1L]));
            w.Time[1L] = uint32(atoi(f[2L]));
            w.Time[2L] = uint32(atoi(f[3L]));
            w.Msg = cstring(f[4L]);
            if (w.Msg == "''")
            { 
                // await() returns '' for no error
                w.Msg = "";

            }

            return ;

        }

        public static error Unmount(@string name, @string old)
        {
            error err = default!;

            fixwd(name, old);
            var (oldp, err) = BytePtrFromString(old);
            if (err != null)
            {
                return error.As(err)!;
            }

            var oldptr = uintptr(@unsafe.Pointer(oldp));

            System.UIntPtr r0 = default;
            ErrorString e = default; 

            // bind(2) man page: If name is zero, everything bound or mounted upon old is unbound or unmounted.
            if (name == "")
            {
                r0, _, e = Syscall(SYS_UNMOUNT, _zero, oldptr, 0L);
            }
            else
            {
                var (namep, err) = BytePtrFromString(name);
                if (err != null)
                {
                    return error.As(err)!;
                }

                r0, _, e = Syscall(SYS_UNMOUNT, uintptr(@unsafe.Pointer(namep)), oldptr, 0L);

            }

            if (int32(r0) == -1L)
            {
                err = e;
            }

            return ;

        }

        public static error Fchdir(long fd)
        {
            error err = default!;

            var (path, err) = Fd2path(fd);

            if (err != null)
            {
                return ;
            }

            return error.As(Chdir(path))!;

        }

        public partial struct Timespec
        {
            public int Sec;
            public int Nsec;
        }

        public partial struct Timeval
        {
            public int Sec;
            public int Usec;
        }

        public static Timeval NsecToTimeval(long nsec)
        {
            Timeval tv = default;

            nsec += 999L; // round up to microsecond
            tv.Usec = int32(nsec % 1e9F / 1e3F);
            tv.Sec = int32(nsec / 1e9F);
            return ;

        }

        private static long nsec()
        {
            ref long scratch = ref heap(out ptr<long> _addr_scratch);

            var (r0, _, _) = Syscall(SYS_NSEC, uintptr(@unsafe.Pointer(_addr_scratch)), 0L, 0L); 
            // TODO(aram): remove hack after I fix _nsec in the pc64 kernel.
            if (r0 == 0L)
            {
                return scratch;
            }

            return int64(r0);

        }

        public static error Gettimeofday(ptr<Timeval> _addr_tv)
        {
            ref Timeval tv = ref _addr_tv.val;

            var nsec = nsec();
            tv = NsecToTimeval(nsec);
            return error.As(null!)!;
        }

        public static long Getegid()
        {
            long egid = default;

            return -1L;
        }
        public static long Geteuid()
        {
            long euid = default;

            return -1L;
        }
        public static long Getgid()
        {
            long gid = default;

            return -1L;
        }
        public static long Getuid()
        {
            long uid = default;

            return -1L;
        }

        public static (slice<long>, error) Getgroups()
        {
            slice<long> gids = default;
            error err = default!;

            return (make_slice<long>(0L), error.As(null!)!);
        }

        //sys    open(path string, mode int) (fd int, err error)
        public static (long, error) Open(@string path, long mode)
        {
            long fd = default;
            error err = default!;

            fixwd(path);
            return open(path, mode);
        }

        //sys    create(path string, mode int, perm uint32) (fd int, err error)
        public static (long, error) Create(@string path, long mode, uint perm)
        {
            long fd = default;
            error err = default!;

            fixwd(path);
            return create(path, mode, perm);
        }

        //sys    remove(path string) (err error)
        public static error Remove(@string path)
        {
            fixwd(path);
            return error.As(remove(path))!;
        }

        //sys    stat(path string, edir []byte) (n int, err error)
        public static (long, error) Stat(@string path, slice<byte> edir)
        {
            long n = default;
            error err = default!;

            fixwd(path);
            return stat(path, edir);
        }

        //sys    bind(name string, old string, flag int) (err error)
        public static error Bind(@string name, @string old, long flag)
        {
            error err = default!;

            fixwd(name, old);
            return error.As(bind(name, old, flag))!;
        }

        //sys    mount(fd int, afd int, old string, flag int, aname string) (err error)
        public static error Mount(long fd, long afd, @string old, long flag, @string aname)
        {
            error err = default!;

            fixwd(old);
            return error.As(mount(fd, afd, old, flag, aname))!;
        }

        //sys    wstat(path string, edir []byte) (err error)
        public static error Wstat(@string path, slice<byte> edir)
        {
            error err = default!;

            fixwd(path);
            return error.As(wstat(path, edir))!;
        }

        //sys    chdir(path string) (err error)
        //sys    Dup(oldfd int, newfd int) (fd int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    Close(fd int) (err error)
        //sys    Fstat(fd int, edir []byte) (n int, err error)
        //sys    Fwstat(fd int, edir []byte) (err error)
    }
}
