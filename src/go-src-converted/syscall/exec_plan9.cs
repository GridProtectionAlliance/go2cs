// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fork, exec, wait, etc.

// package syscall -- go2cs converted at 2020 October 08 03:26:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\exec_plan9.go
using runtime = go.runtime_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class syscall_package
    {
        // ForkLock is not used on plan9.
        public static sync.RWMutex ForkLock = default;

        // gstringb reads a non-empty string from b, prefixed with a 16-bit length in little-endian order.
        // It returns the string as a byte slice, or nil if b is too short to contain the length or
        // the full string.
        //go:nosplit
        private static slice<byte> gstringb(slice<byte> b)
        {
            if (len(b) < 2L)
            {
                return null;
            }

            var (n, b) = gbit16(b);
            if (int(n) > len(b))
            {
                return null;
            }

            return b[..n];

        }

        // Offset of the name field in a 9P directory entry - see UnmarshalDir() in dir_plan9.go
        private static readonly long nameOffset = (long)39L;

        // gdirname returns the first filename from a buffer of directory entries,
        // and a slice containing the remaining directory entries.
        // If the buffer doesn't start with a valid directory entry, the returned name is nil.
        //go:nosplit


        // gdirname returns the first filename from a buffer of directory entries,
        // and a slice containing the remaining directory entries.
        // If the buffer doesn't start with a valid directory entry, the returned name is nil.
        //go:nosplit
        private static (slice<byte>, slice<byte>) gdirname(slice<byte> buf)
        {
            slice<byte> name = default;
            slice<byte> rest = default;

            if (len(buf) < 2L)
            {
                return ;
            }

            var (size, buf) = gbit16(buf);
            if (size < STATFIXLEN || int(size) > len(buf))
            {
                return ;
            }

            name = gstringb(buf[nameOffset..size]);
            rest = buf[size..];
            return ;

        }

        // StringSlicePtr converts a slice of strings to a slice of pointers
        // to NUL-terminated byte arrays. If any string contains a NUL byte
        // this function panics instead of returning an error.
        //
        // Deprecated: Use SlicePtrFromStrings instead.
        public static slice<ptr<byte>> StringSlicePtr(slice<@string> ss)
        {
            var bb = make_slice<ptr<byte>>(len(ss) + 1L);
            for (long i = 0L; i < len(ss); i++)
            {
                bb[i] = StringBytePtr(ss[i]);
            }

            bb[len(ss)] = null;
            return bb;

        }

        // SlicePtrFromStrings converts a slice of strings to a slice of
        // pointers to NUL-terminated byte arrays. If any string contains
        // a NUL byte, it returns (nil, EINVAL).
        public static (slice<ptr<byte>>, error) SlicePtrFromStrings(slice<@string> ss)
        {
            slice<ptr<byte>> _p0 = default;
            error _p0 = default!;

            error err = default!;
            var bb = make_slice<ptr<byte>>(len(ss) + 1L);
            for (long i = 0L; i < len(ss); i++)
            {
                bb[i], err = BytePtrFromString(ss[i]);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            bb[len(ss)] = null;
            return (bb, error.As(null!)!);

        }

        // readdirnames returns the names of files inside the directory represented by dirfd.
        private static (slice<@string>, error) readdirnames(long dirfd)
        {
            slice<@string> names = default;
            error err = default!;

            names = make_slice<@string>(0L, 100L);
            array<byte> buf = new array<byte>(STATMAX);

            while (true)
            {
                var (n, e) = Read(dirfd, buf[..]);
                if (e != null)
                {
                    return (null, error.As(e)!);
                }

                if (n == 0L)
                {
                    break;
                }

                {
                    var b = buf[..n];

                    while (len(b) > 0L)
                    {
                        slice<byte> s = default;
                        s, b = gdirname(b);
                        if (s == null)
                        {
                            return (null, error.As(ErrBadStat)!);
                        }

                        names = append(names, string(s));

                    }

                }

            }

            return ;

        }

        // name of the directory containing names and control files for all open file descriptors


        // forkAndExecInChild forks the process, calling dup onto 0..len(fd)
        // and finally invoking exec(argv0, argvv, envv) in the child.
        // If a dup or exec fails, it writes the error string to pipe.
        // (The pipe write end is close-on-exec so if exec succeeds, it will be closed.)
        //
        // In the child, this function must not acquire any locks, because
        // they might have been locked at the time of the fork. This means
        // no rescheduling, no malloc calls, and no new stack segments.
        // The calls to RawSyscall are okay because they are assembly
        // functions that do not grow the stack.
        //go:norace
        private static (long, error) forkAndExecInChild(ptr<byte> _addr_argv0, slice<ptr<byte>> argv, slice<envItem> envv, ptr<byte> _addr_dir, ptr<ProcAttr> _addr_attr, long pipe, long rflag)
        {
            long pid = default;
            error err = default!;
            ref byte argv0 = ref _addr_argv0.val;
            ref byte dir = ref _addr_dir.val;
            ref ProcAttr attr = ref _addr_attr.val;
 
            // Declare all variables at top in case any
            // declarations require heap allocation (e.g., errbuf).
            System.UIntPtr r1 = default;            long nextfd = default;            long i = default;            long clearenv = default;            long envfd = default;            array<byte> errbuf = new array<byte>(ERRMAX);            array<byte> statbuf = new array<byte>(STATMAX);            long dupdevfd = default; 

            // Guard against side effects of shuffling fds below.
            // Make sure that nextfd is beyond any currently open files so
            // that we can't run the risk of overwriting any of them.
            var fd = make_slice<long>(len(attr.Files));
            nextfd = len(attr.Files);
            {
                long i__prev1 = i;

                foreach (var (__i, __ufd) in attr.Files)
                {
                    i = __i;
                    ufd = __ufd;
                    if (nextfd < int(ufd))
                    {
                        nextfd = int(ufd);
                    }

                    fd[i] = int(ufd);

                }

                i = i__prev1;
            }

            nextfd++;

            if (envv != null)
            {
                clearenv = RFCENVG;
            } 

            // About to call fork.
            // No more allocation or calls of non-assembly functions.
            r1, _, _ = RawSyscall(SYS_RFORK, uintptr(RFPROC | RFFDG | RFREND | clearenv | rflag), 0L, 0L);

            if (r1 != 0L)
            {
                if (int32(r1) == -1L)
                {
                    return (0L, error.As(NewError(errstr()))!);
                } 
                // parent; return PID
                return (int(r1), error.As(null!)!);

            } 

            // Fork succeeded, now in child.

            // Close fds we don't need.
            r1, _, _ = RawSyscall(SYS_OPEN, uintptr(@unsafe.Pointer(dupdev)), uintptr(O_RDONLY), 0L);
            dupdevfd = int(r1);
            if (dupdevfd == -1L)
            {
                goto childerror;
            }

dirloop:
            while (true)
            {
                r1, _, _ = RawSyscall6(SYS_PREAD, uintptr(dupdevfd), uintptr(@unsafe.Pointer(_addr_statbuf[0L])), uintptr(len(statbuf)), ~uintptr(0L), ~uintptr(0L), 0L);
                var n = int(r1);
                switch (n)
                {
                    case -1L: 
                        goto childerror;
                        break;
                    case 0L: 
                        _breakdirloop = true;
                        break;
                        break;
                }
                {
                    var b = statbuf[..n];

                    while (len(b) > 0L)
                    {
                        slice<byte> s = default;
                        s, b = gdirname(b);
                        if (s == null)
                        {
                            copy(errbuf[..], ErrBadStat.Error());
                            goto childerror1;
                        }

                        if (s[len(s) - 1L] == 'l')
                        { 
                            // control file for descriptor <N> is named <N>ctl
                            continue;

                        }

                        closeFdExcept(int(atoi(s)), pipe, dupdevfd, fd);

                    }

                }

            }
            RawSyscall(SYS_CLOSE, uintptr(dupdevfd), 0L, 0L); 

            // Write new environment variables.
            if (envv != null)
            {
                for (i = 0L; i < len(envv); i++)
                {
                    r1, _, _ = RawSyscall(SYS_CREATE, uintptr(@unsafe.Pointer(envv[i].name)), uintptr(O_WRONLY), uintptr(0666L));

                    if (int32(r1) == -1L)
                    {
                        goto childerror;
                    }

                    envfd = int(r1);

                    r1, _, _ = RawSyscall6(SYS_PWRITE, uintptr(envfd), uintptr(@unsafe.Pointer(envv[i].value)), uintptr(envv[i].nvalue), ~uintptr(0L), ~uintptr(0L), 0L);

                    if (int32(r1) == -1L || int(r1) != envv[i].nvalue)
                    {
                        goto childerror;
                    }

                    r1, _, _ = RawSyscall(SYS_CLOSE, uintptr(envfd), 0L, 0L);

                    if (int32(r1) == -1L)
                    {
                        goto childerror;
                    }

                }


            } 

            // Chdir
            if (dir != null)
            {
                r1, _, _ = RawSyscall(SYS_CHDIR, uintptr(@unsafe.Pointer(dir)), 0L, 0L);
                if (int32(r1) == -1L)
                {
                    goto childerror;
                }

            } 

            // Pass 1: look for fd[i] < i and move those up above len(fd)
            // so that pass 2 won't stomp on an fd it needs later.
            if (pipe < nextfd)
            {
                r1, _, _ = RawSyscall(SYS_DUP, uintptr(pipe), uintptr(nextfd), 0L);
                if (int32(r1) == -1L)
                {
                    goto childerror;
                }

                pipe = nextfd;
                nextfd++;

            }

            for (i = 0L; i < len(fd); i++)
            {
                if (fd[i] >= 0L && fd[i] < int(i))
                {
                    if (nextfd == pipe)
                    { // don't stomp on pipe
                        nextfd++;

                    }

                    r1, _, _ = RawSyscall(SYS_DUP, uintptr(fd[i]), uintptr(nextfd), 0L);
                    if (int32(r1) == -1L)
                    {
                        goto childerror;
                    }

                    fd[i] = nextfd;
                    nextfd++;

                }

            } 

            // Pass 2: dup fd[i] down onto i.
 

            // Pass 2: dup fd[i] down onto i.
            for (i = 0L; i < len(fd); i++)
            {
                if (fd[i] == -1L)
                {
                    RawSyscall(SYS_CLOSE, uintptr(i), 0L, 0L);
                    continue;
                }

                if (fd[i] == int(i))
                {
                    continue;
                }

                r1, _, _ = RawSyscall(SYS_DUP, uintptr(fd[i]), uintptr(i), 0L);
                if (int32(r1) == -1L)
                {
                    goto childerror;
                }

            } 

            // Pass 3: close fd[i] if it was moved in the previous pass.
 

            // Pass 3: close fd[i] if it was moved in the previous pass.
            for (i = 0L; i < len(fd); i++)
            {
                if (fd[i] >= 0L && fd[i] != int(i))
                {
                    RawSyscall(SYS_CLOSE, uintptr(fd[i]), 0L, 0L);
                }

            } 

            // Time to exec.
 

            // Time to exec.
            r1, _, _ = RawSyscall(SYS_EXEC, uintptr(@unsafe.Pointer(argv0)), uintptr(@unsafe.Pointer(_addr_argv[0L])), 0L);

childerror:
            RawSyscall(SYS_ERRSTR, uintptr(@unsafe.Pointer(_addr_errbuf[0L])), uintptr(len(errbuf)), 0L);
childerror1:
            errbuf[len(errbuf) - 1L] = 0L;
            i = 0L;
            while (i < len(errbuf) && errbuf[i] != 0L)
            {
                i++;
            }


            RawSyscall6(SYS_PWRITE, uintptr(pipe), uintptr(@unsafe.Pointer(_addr_errbuf[0L])), uintptr(i), ~uintptr(0L), ~uintptr(0L), 0L);

            while (true)
            {
                RawSyscall(SYS_EXITS, 0L, 0L, 0L);
            }


        }

        // close the numbered file descriptor, unless it is fd1, fd2, or a member of fds.
        //go:nosplit
        private static void closeFdExcept(long n, long fd1, long fd2, slice<long> fds)
        {
            if (n == fd1 || n == fd2)
            {
                return ;
            }

            foreach (var (_, fd) in fds)
            {
                if (n == fd)
                {
                    return ;
                }

            }
            RawSyscall(SYS_CLOSE, uintptr(n), 0L, 0L);

        }

        private static error cexecPipe(slice<long> p)
        {
            var e = Pipe(p);
            if (e != null)
            {
                return error.As(e)!;
            }

            var (fd, e) = Open("#d/" + itoa(p[1L]), O_CLOEXEC);
            if (e != null)
            {
                Close(p[0L]);
                Close(p[1L]);
                return error.As(e)!;
            }

            Close(fd);
            return error.As(null!)!;

        }

        private partial struct envItem
        {
            public ptr<byte> name;
            public ptr<byte> value;
            public long nvalue;
        }

        public partial struct ProcAttr
        {
            public @string Dir; // Current working directory.
            public slice<@string> Env; // Environment.
            public slice<System.UIntPtr> Files; // File descriptors.
            public ptr<SysProcAttr> Sys;
        }

        public partial struct SysProcAttr
        {
            public long Rfork; // additional flags to pass to rfork
        }

        private static ProcAttr zeroProcAttr = default;
        private static SysProcAttr zeroSysProcAttr = default;

        private static (long, error) forkExec(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            array<long> p = new array<long>(2L);            long n = default;            array<byte> errbuf = new array<byte>(ERRMAX);            ref Waitmsg wmsg = ref heap(out ptr<Waitmsg> _addr_wmsg);

            if (attr == null)
            {
                attr = _addr_zeroProcAttr;
            }

            var sys = attr.Sys;
            if (sys == null)
            {
                sys = _addr_zeroSysProcAttr;
            }

            p[0L] = -1L;
            p[1L] = -1L; 

            // Convert args to C form.
            var (argv0p, err) = BytePtrFromString(argv0);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            var (argvp, err) = SlicePtrFromStrings(argv);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            var destDir = attr.Dir;
            if (destDir == "")
            {
                wdmu.Lock();
                destDir = wdStr;
                wdmu.Unlock();
            }

            ptr<byte> dir;
            if (destDir != "")
            {
                dir, err = BytePtrFromString(destDir);
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            slice<envItem> envvParsed = default;
            if (attr.Env != null)
            {
                envvParsed = make_slice<envItem>(0L, len(attr.Env));
                foreach (var (_, v) in attr.Env)
                {
                    long i = 0L;
                    while (i < len(v) && v[i] != '=')
                    {
                        i++;
                    }


                    var (envname, err) = BytePtrFromString("/env/" + v[..i]);
                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    }

                    var envvalue = make_slice<byte>(len(v) - i);
                    copy(envvalue, v[i + 1L..]);
                    envvParsed = append(envvParsed, new envItem(envname,&envvalue[0],len(v)-i));

                }

            } 

            // Allocate child status pipe close on exec.
            var e = cexecPipe(p[..]);

            if (e != null)
            {
                return (0L, error.As(e)!);
            } 

            // Kick off child.
            pid, err = forkAndExecInChild(_addr_argv0p, argvp, envvParsed, dir, _addr_attr, p[1L], sys.Rfork);

            if (err != null)
            {
                if (p[0L] >= 0L)
                {
                    Close(p[0L]);
                    Close(p[1L]);
                }

                return (0L, error.As(err)!);

            } 

            // Read child error status from pipe.
            Close(p[1L]);
            n, err = Read(p[0L], errbuf[..]);
            Close(p[0L]);

            if (err != null || n != 0L)
            {
                if (n > 0L)
                {
                    err = NewError(string(errbuf[..n]));
                }
                else if (err == null)
                {
                    err = NewError("failed to read exec status");
                } 

                // Child failed; wait for it to exit, to make sure
                // the zombies don't accumulate.
                while (wmsg.Pid != pid)
                {
                    Await(_addr_wmsg);
                }

                return (0L, error.As(err)!);

            } 

            // Read got EOF, so pipe closed on exec, so exec succeeded.
            return (pid, error.As(null!)!);

        }

        private partial struct waitErr
        {
            public ref Waitmsg Waitmsg => ref Waitmsg_val;
            public error err;
        }

        private static var procs = default;

        // startProcess starts a new goroutine, tied to the OS
        // thread, which runs the process and subsequently waits
        // for it to finish, communicating the process stats back
        // to any goroutines that may have been waiting on it.
        //
        // Such a dedicated goroutine is needed because on
        // Plan 9, only the parent thread can wait for a child,
        // whereas goroutines tend to jump OS threads (e.g.,
        // between starting a process and running Wait(), the
        // goroutine may have been rescheduled).
        private static (long, error) startProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            private partial struct forkRet
            {
                public long pid;
                public error err;
            }

            var forkc = make_channel<forkRet>(1L);
            go_(() => () =>
            {
                runtime.LockOSThread();
                forkRet ret = default;

                ret.pid, ret.err = forkExec(argv0, argv, _addr_attr); 
                // If fork fails there is nothing to wait for.
                if (ret.err != null || ret.pid == 0L)
                {
                    forkc.Send(ret);
                    return ;
                }

                var waitc = make_channel<ptr<waitErr>>(1L); 

                // Mark that the process is running.
                procs.Lock();
                if (procs.waits == null)
                {
                    procs.waits = make_map<long, channel<ptr<waitErr>>>();
                }

                procs.waits[ret.pid] = waitc;
                procs.Unlock();

                forkc.Send(ret);

                ref waitErr w = ref heap(out ptr<waitErr> _addr_w);
                while (w.err == null && w.Pid != ret.pid)
                {
                    w.err = Await(_addr_w.Waitmsg);
                }

                waitc.Send(_addr_w);
                close(waitc);

            }());
            ret = forkc.Receive();
            return (ret.pid, error.As(ret.err)!);

        }

        // Combination of fork and exec, careful to be thread safe.
        public static (long, error) ForkExec(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            return startProcess(argv0, argv, _addr_attr);
        }

        // StartProcess wraps ForkExec for package os.
        public static (long, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            System.UIntPtr handle = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            pid, err = startProcess(argv0, argv, _addr_attr);
            return (pid, 0L, error.As(err)!);
        }

        // Ordinary exec.
        public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv)
        {
            error err = default!;

            if (envv != null)
            {
                var (r1, _, _) = RawSyscall(SYS_RFORK, RFCENVG, 0L, 0L);
                if (int32(r1) == -1L)
                {
                    return error.As(NewError(errstr()))!;
                }

                foreach (var (_, v) in envv)
                {
                    long i = 0L;
                    while (i < len(v) && v[i] != '=')
                    {
                        i++;
                    }


                    var (fd, e) = Create("/env/" + v[..i], O_WRONLY, 0666L);
                    if (e != null)
                    {
                        return error.As(e)!;
                    }

                    _, e = Write(fd, (slice<byte>)v[i + 1L..]);
                    if (e != null)
                    {
                        Close(fd);
                        return error.As(e)!;
                    }

                    Close(fd);

                }

            }

            var (argv0p, err) = BytePtrFromString(argv0);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (argvp, err) = SlicePtrFromStrings(argv);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (_, _, e1) = Syscall(SYS_EXEC, uintptr(@unsafe.Pointer(argv0p)), uintptr(@unsafe.Pointer(_addr_argvp[0L])), 0L);

            return error.As(e1)!;

        }

        // WaitProcess waits until the pid of a
        // running process is found in the queue of
        // wait messages. It is used in conjunction
        // with ForkExec/StartProcess to wait for a
        // running process to exit.
        public static error WaitProcess(long pid, ptr<Waitmsg> _addr_w)
        {
            error err = default!;
            ref Waitmsg w = ref _addr_w.val;

            procs.Lock();
            var ch = procs.waits[pid];
            procs.Unlock();

            ptr<waitErr> wmsg;
            if (ch != null)
            {
                wmsg = ch.Receive();
                procs.Lock();
                if (procs.waits[pid] == ch)
                {
                    delete(procs.waits, pid);
                }

                procs.Unlock();

            }

            if (wmsg == null)
            { 
                // ch was missing or ch is closed
                return error.As(NewError("process not found"))!;

            }

            if (wmsg.err != null)
            {
                return error.As(wmsg.err)!;
            }

            if (w != null)
            {
                w = wmsg.Waitmsg;
            }

            return error.As(null!)!;

        }
    }
}
