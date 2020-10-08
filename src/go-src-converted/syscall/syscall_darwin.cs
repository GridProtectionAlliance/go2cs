// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Darwin system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package syscall -- go2cs converted at 2020 October 08 03:27:12 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_darwin.go
using errorspkg = go.errors_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly var ImplementsGetwd = (var)true;



        public static (@string, error) Getwd()
        {
            @string _p0 = default;
            error _p0 = default!;

            var buf = make_slice<byte>(2048L);
            var (attrs, err) = getAttrList(".", new attrList(CommonAttr:attrCmnFullpath), buf, 0L);
            if (err == null && len(attrs) == 1L && len(attrs[0L]) >= 2L)
            {
                var wd = string(attrs[0L]); 
                // Sanity check that it's an absolute path and ends
                // in a null byte, which we then strip.
                if (wd[0L] == '/' && wd[len(wd) - 1L] == 0L)
                {
                    return (wd[..len(wd) - 1L], error.As(null!)!);
                }

            } 
            // If pkg/os/getwd.go gets ENOTSUP, it will fall back to the
            // slow algorithm.
            return ("", error.As(ENOTSUP)!);

        }

        public partial struct SockaddrDatalink
        {
            public byte Len;
            public byte Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<sbyte> Data;
            public RawSockaddrDatalink raw;
        }

        // Translate "kern.hostname" to []_C_int{0,1,2,3}.
        private static (slice<_C_int>, error) nametomib(@string name)
        {
            slice<_C_int> mib = default;
            error err = default!;

            const var siz = (var)@unsafe.Sizeof(mib[0L]); 

            // NOTE(rsc): It seems strange to set the buffer to have
            // size CTL_MAXNAME+2 but use only CTL_MAXNAME
            // as the size. I don't know why the +2 is here, but the
            // kernel uses +2 for its own implementation of this function.
            // I am scared that if we don't include the +2 here, the kernel
            // will silently write 2 words farther than we specify
            // and we'll get memory corruption.
 

            // NOTE(rsc): It seems strange to set the buffer to have
            // size CTL_MAXNAME+2 but use only CTL_MAXNAME
            // as the size. I don't know why the +2 is here, but the
            // kernel uses +2 for its own implementation of this function.
            // I am scared that if we don't include the +2 here, the kernel
            // will silently write 2 words farther than we specify
            // and we'll get memory corruption.
            array<_C_int> buf = new array<_C_int>(CTL_MAXNAME + 2L);
            ref var n = ref heap(uintptr(CTL_MAXNAME) * siz, out ptr<var> _addr_n);

            var p = (byte.val)(@unsafe.Pointer(_addr_buf[0L]));
            var (bytes, err) = ByteSliceFromString(name);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // Magic sysctl: "setting" 0.3 to a string name
            // lets you read back the array of integers form.
            err = sysctl(new slice<_C_int>(new _C_int[] { 0, 3 }), p, _addr_n, _addr_bytes[0L], uintptr(len(name)));

            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (buf[0L..n / siz], error.As(null!)!);

        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
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

            return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
        }

        public static error PtraceAttach(long pid)
        {
            error err = default!;

            return error.As(ptrace(PT_ATTACH, pid, 0L, 0L))!;
        }
        public static error PtraceDetach(long pid)
        {
            error err = default!;

            return error.As(ptrace(PT_DETACH, pid, 0L, 0L))!;
        }

        private static readonly long attrBitMapCount = (long)5L;
        private static readonly ulong attrCmnModtime = (ulong)0x00000400UL;
        private static readonly ulong attrCmnAcctime = (ulong)0x00001000UL;
        private static readonly ulong attrCmnFullpath = (ulong)0x08000000UL;


        private partial struct attrList
        {
            public ushort bitmapCount;
            public ushort _;
            public uint CommonAttr;
            public uint VolAttr;
            public uint DirAttr;
            public uint FileAttr;
            public uint Forkattr;
        }

        private static (slice<slice<byte>>, error) getAttrList(@string path, attrList attrList, slice<byte> attrBuf, ulong options)
        {
            slice<slice<byte>> attrs = default;
            error err = default!;

            if (len(attrBuf) < 4L)
            {
                return (null, error.As(errorspkg.New("attrBuf too small"))!);
            }

            attrList.bitmapCount = attrBitMapCount;

            ptr<byte> _p0;
            _p0, err = BytePtrFromString(path);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (_, _, e1) = syscall6(funcPC(libc_getattrlist_trampoline), uintptr(@unsafe.Pointer(_p0)), uintptr(@unsafe.Pointer(_addr_attrList)), uintptr(@unsafe.Pointer(_addr_attrBuf[0L])), uintptr(len(attrBuf)), uintptr(options), 0L);
            if (e1 != 0L)
            {
                return (null, error.As(e1)!);
            }

            ptr<ptr<uint>> size = new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_attrBuf[0L])); 

            // dat is the section of attrBuf that contains valid data,
            // without the 4 byte length header. All attribute offsets
            // are relative to dat.
            var dat = attrBuf;
            if (int(size) < len(attrBuf))
            {
                dat = dat[..size];
            }

            dat = dat[4L..]; // remove length prefix

            {
                var i = uint32(0L);

                while (int(i) < len(dat))
                {
                    var header = dat[i..];
                    if (len(header) < 8L)
                    {
                        return (attrs, error.As(errorspkg.New("truncated attribute header"))!);
                    }

                    ptr<ptr<int>> datOff = new ptr<ptr<ptr<int>>>(@unsafe.Pointer(_addr_header[0L]));
                    ptr<ptr<uint>> attrLen = new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_header[4L]));
                    if (datOff < 0L || uint32(datOff) + attrLen > uint32(len(dat)))
                    {
                        return (attrs, error.As(errorspkg.New("truncated results; attrBuf too small"))!);
                    }

                    var end = uint32(datOff) + attrLen;
                    attrs = append(attrs, dat[datOff..end]);
                    i = end;
                    {
                        var r = i % 4L;

                        if (r != 0L)
                        {
                            i += (4L - r);
                        }

                    }

                }

            }
            return ;

        }

        private static void libc_getattrlist_trampoline()
;

        //go:linkname libc_getattrlist libc_getattrlist
        //go:cgo_import_dynamic libc_getattrlist getattrlist "/usr/lib/libSystem.B.dylib"

        //sysnb pipe(p *[2]int32) (err error)

        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {>>MARKER:FUNCTION_libc_getattrlist_trampoline_BLOCK_PREFIX<<
                return error.As(EINVAL)!;
            }

            ref array<int> q = ref heap(new array<int>(2L), out ptr<array<int>> _addr_q);
            err = pipe(_addr_q);
            p[0L] = int(q[0L]);
            p[1L] = int(q[1L]);
            return ;

        }

        public static (long, error) Getfsstat(slice<Statfs_t> buf, long flags)
        {
            long n = default;
            error err = default!;

            unsafe.Pointer _p0 = default;
            System.UIntPtr bufsize = default;
            if (len(buf) > 0L)
            {
                _p0 = @unsafe.Pointer(_addr_buf[0L]);
                bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
            }

            var (r0, _, e1) = syscall(funcPC(libc_getfsstat_trampoline), uintptr(_p0), bufsize, uintptr(flags));
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        private static void libc_getfsstat_trampoline()
;

        //go:linkname libc_getfsstat libc_getfsstat
        //go:cgo_import_dynamic libc_getfsstat getfsstat "/usr/lib/libSystem.B.dylib"

        private static error setattrlistTimes(@string path, slice<Timespec> times)
        {
            var (_p0, err) = BytePtrFromString(path);
            if (err != null)
            {>>MARKER:FUNCTION_libc_getfsstat_trampoline_BLOCK_PREFIX<<
                return error.As(err)!;
            }

            ref attrList attrList = ref heap(out ptr<attrList> _addr_attrList);
            attrList.bitmapCount = attrBitMapCount;
            attrList.CommonAttr = attrCmnModtime | attrCmnAcctime; 

            // order is mtime, atime: the opposite of Chtimes
            ref array<Timespec> attributes = ref heap(new array<Timespec>(new Timespec[] { times[1], times[0] }), out ptr<array<Timespec>> _addr_attributes);
            const long options = (long)0L;

            var (_, _, e1) = syscall6(funcPC(libc_setattrlist_trampoline), uintptr(@unsafe.Pointer(_p0)), uintptr(@unsafe.Pointer(_addr_attrList)), uintptr(@unsafe.Pointer(_addr_attributes)), uintptr(@unsafe.Sizeof(attributes)), uintptr(options), 0L);
            if (e1 != 0L)
            {
                return error.As(e1)!;
            }

            return error.As(null!)!;

        }

        private static void libc_setattrlist_trampoline()
;

        //go:linkname libc_setattrlist libc_setattrlist
        //go:cgo_import_dynamic libc_setattrlist setattrlist "/usr/lib/libSystem.B.dylib"

        private static error utimensat(long dirfd, @string path, ptr<array<Timespec>> _addr_times, long flag)
        {
            ref array<Timespec> times = ref _addr_times.val;
 
            // Darwin doesn't support SYS_UTIMENSAT
            return error.As(ENOSYS)!;

        }

        /*
         * Wrapped
         */

        //sys    kill(pid int, signum int, posix int) (err error)

        public static error Kill(long pid, Signal signum)
        {
            error err = default!;

            return error.As(kill(pid, int(signum), 1L))!;
        }

        /*
         * Exposed directly
         */
        //sys    Access(path string, mode uint32) (err error)
        //sys    Adjtime(delta *Timeval, olddelta *Timeval) (err error)
        //sys    Chdir(path string) (err error)
        //sys    Chflags(path string, flags int) (err error)
        //sys    Chmod(path string, mode uint32) (err error)
        //sys    Chown(path string, uid int, gid int) (err error)
        //sys    Chroot(path string) (err error)
        //sys    Close(fd int) (err error)
        //sys    closedir(dir uintptr) (err error)
        //sys    Dup(fd int) (nfd int, err error)
        //sys    Dup2(from int, to int) (err error)
        //sys    Exchangedata(path1 string, path2 string, options int) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchflags(fd int, flags int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    Fsync(fd int) (err error)
        //  Fsync is not called for os.File.Sync(). Please see internal/poll/fd_fsync_darwin.go
        //sys    Ftruncate(fd int, length int64) (err error)
        //sys    Getdtablesize() (size int)
        //sysnb    Getegid() (egid int)
        //sysnb    Geteuid() (uid int)
        //sysnb    Getgid() (gid int)
        //sysnb    Getpgid(pid int) (pgid int, err error)
        //sysnb    Getpgrp() (pgrp int)
        //sysnb    Getpid() (pid int)
        //sysnb    Getppid() (ppid int)
        //sys    Getpriority(which int, who int) (prio int, err error)
        //sysnb    Getrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Getrusage(who int, rusage *Rusage) (err error)
        //sysnb    Getsid(pid int) (sid int, err error)
        //sysnb    Getuid() (uid int)
        //sysnb    Issetugid() (tainted bool)
        //sys    Kqueue() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Listen(s int, backlog int) (err error)
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mkfifo(path string, mode uint32) (err error)
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Mlock(b []byte) (err error)
        //sys    Mlockall(flags int) (err error)
        //sys    Mprotect(b []byte, prot int) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Munlockall() (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Pathconf(path string, name int) (val int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    readdir_r(dir uintptr, entry *Dirent, result **Dirent) (res Errno)
        //sys    Readlink(path string, buf []byte) (n int, err error)
        //sys    Rename(from string, to string) (err error)
        //sys    Revoke(path string) (err error)
        //sys    Rmdir(path string) (err error)
        //sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = SYS_lseek
        //sys    Select(n int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (err error)
        //sys    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sys    Setlogin(name string) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sys    Setprivexec(flag int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tp *Timeval) (err error)
        //sysnb    Setuid(uid int) (err error)
        //sys    Symlink(path string, link string) (err error)
        //sys    Sync() (err error)
        //sys    Truncate(path string, length int64) (err error)
        //sys    Umask(newmask int) (oldmask int)
        //sys    Undelete(path string) (err error)
        //sys    Unlink(path string) (err error)
        //sys    Unmount(path string, flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    writev(fd int, iovecs []Iovec) (cnt uintptr, err error)
        //sys   mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys   munmap(addr uintptr, length uintptr) (err error)
        //sysnb fork() (pid int, err error)
        //sysnb ioctl(fd int, req int, arg int) (err error)
        //sysnb ioctlPtr(fd int, req uint, arg unsafe.Pointer) (err error) = SYS_ioctl
        //sysnb execve(path *byte, argv **byte, envp **byte) (err error)
        //sysnb exit(res int) (err error)
        //sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error)
        //sys    fcntlPtr(fd int, cmd int, arg unsafe.Pointer) (val int, err error) = SYS_fcntl
        //sys   unlinkat(fd int, path string, flags int) (err error)
        //sys   openat(fd int, path string, flags int, perm uint32) (fdret int, err error)

        private static void init()
        {
            execveDarwin = execve;
        }

        private static (System.UIntPtr, error) fdopendir(long fd)
        {
            System.UIntPtr dir = default;
            error err = default!;

            var (r0, _, e1) = syscallPtr(funcPC(libc_fdopendir_trampoline), uintptr(fd), 0L, 0L);
            dir = uintptr(r0);
            if (e1 != 0L)
            {>>MARKER:FUNCTION_libc_setattrlist_trampoline_BLOCK_PREFIX<<
                err = errnoErr(e1);
            }

            return ;

        }

        private static void libc_fdopendir_trampoline()
;

        //go:linkname libc_fdopendir libc_fdopendir
        //go:cgo_import_dynamic libc_fdopendir fdopendir "/usr/lib/libSystem.B.dylib"

        private static (long, error) readlen(long fd, ptr<byte> _addr_buf, long nbuf)
        {
            long n = default;
            error err = default!;
            ref byte buf = ref _addr_buf.val;

            var (r0, _, e1) = syscall(funcPC(libc_read_trampoline), uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf));
            n = int(r0);
            if (e1 != 0L)
            {>>MARKER:FUNCTION_libc_fdopendir_trampoline_BLOCK_PREFIX<<
                err = errnoErr(e1);
            }

            return ;

        }

        private static (long, error) writelen(long fd, ptr<byte> _addr_buf, long nbuf)
        {
            long n = default;
            error err = default!;
            ref byte buf = ref _addr_buf.val;

            var (r0, _, e1) = syscall(funcPC(libc_write_trampoline), uintptr(fd), uintptr(@unsafe.Pointer(buf)), uintptr(nbuf));
            n = int(r0);
            if (e1 != 0L)
            {
                err = errnoErr(e1);
            }

            return ;

        }

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
            {
                return (0L, error.As(err)!);
            } 

            // We need to duplicate the incoming file descriptor
            // because the caller expects to retain control of it, but
            // fdopendir expects to take control of its argument.
            // Just Dup'ing the file descriptor is not enough, as the
            // result shares underlying state. Use openat to make a really
            // new file descriptor referring to the same directory.
            var (fd2, err) = openat(fd, ".", O_RDONLY, 0L);
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
                ref struct{ptrunsafe.Pointersizintcapint} s = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{ptrunsafe.Pointersizintcapint}{ptr:unsafe.Pointer(&entry),siz:reclen,cap:reclen}, out ptr<struct{ptrunsafe.Pointersizintcapint}> _addr_s);
                copy(buf, new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_s)));
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

        // Implemented in the runtime package (runtime/sys_darwin.go)
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) rawSyscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        private static (System.UIntPtr, System.UIntPtr, Errno) syscallPtr(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;

        // Find the entry point for f. See comments in runtime/proc.go for the
        // function of the same name.
        //go:nosplit
        private static System.UIntPtr funcPC(Action f)
        {
            return new ptr<ptr<ptr<ptr<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_f));
        }
    }
}
