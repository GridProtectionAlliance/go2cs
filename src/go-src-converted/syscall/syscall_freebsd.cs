// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// FreeBSD system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package syscall -- go2cs converted at 2020 October 09 05:01:42 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_freebsd.go
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly long _SYS_FSTAT_FREEBSD12 = (long)551L; // { int fstat(int fd, _Out_ struct stat *sb); }
        private static readonly long _SYS_FSTATAT_FREEBSD12 = (long)552L; // { int fstatat(int fd, _In_z_ char *path, _Out_ struct stat *buf, int flag); }
        private static readonly long _SYS_GETDIRENTRIES_FREEBSD12 = (long)554L; // { ssize_t getdirentries(int fd, _Out_writes_bytes_(count) char *buf, size_t count, _Out_ off_t *basep); }
        private static readonly long _SYS_STATFS_FREEBSD12 = (long)555L; // { int statfs(_In_z_ char *path, _Out_ struct statfs *buf); }
        private static readonly long _SYS_FSTATFS_FREEBSD12 = (long)556L; // { int fstatfs(int fd, _Out_ struct statfs *buf); }
        private static readonly long _SYS_GETFSSTAT_FREEBSD12 = (long)557L; // { int getfsstat(_Out_writes_bytes_opt_(bufsize) struct statfs *buf, long bufsize, int mode); }
        private static readonly long _SYS_MKNODAT_FREEBSD12 = (long)559L; // { int mknodat(int fd, _In_z_ char *path, mode_t mode, dev_t dev); }

        // See https://www.freebsd.org/doc/en_US.ISO8859-1/books/porters-handbook/versions.html.
        private static sync.Once osreldateOnce = default;        private static uint osreldate = default;

        // INO64_FIRST from /usr/src/lib/libc/sys/compat-ino64.h
        private static readonly long _ino64First = (long)1200031L;



        private static bool supportsABI(uint ver)
        {
            osreldateOnce.Do(() =>
            {
                osreldate, _ = SysctlUint32("kern.osreldate");
            });
            return osreldate >= ver;

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

            const var siz = @unsafe.Sizeof(mib[0L]); 

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

            return readInt(buf, @unsafe.Offsetof(new Dirent().Fileno), @unsafe.Sizeof(new Dirent().Fileno));
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

        public static error Pipe(slice<long> p)
        {
            return error.As(Pipe2(p, 0L))!;
        }

        //sysnb pipe2(p *[2]_C_int, flags int) (err error)

        public static error Pipe2(slice<long> p, long flags)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            var err = pipe2(_addr_pp, flags);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return error.As(err)!;

        }

        public static (ptr<IPMreqn>, error) GetsockoptIPMreqn(long fd, long level, long opt)
        {
            ptr<IPMreqn> _p0 = default!;
            error _p0 = default!;

            ref IPMreqn value = ref heap(out ptr<IPMreqn> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofIPMreqn), out ptr<var> _addr_vallen);
            var errno = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(errno)!);
        }

        public static error SetsockoptIPMreqn(long fd, long level, long opt, ptr<IPMreqn> _addr_mreq)
        {
            error err = default!;
            ref IPMreqn mreq = ref _addr_mreq.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq)))!;
        }

        public static (long, Sockaddr, error) Accept4(long fd, long flags) => func((_, panic, __) =>
        {
            long nfd = default;
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            nfd, err = accept4(fd, _addr_rsa, _addr_len, flags);
            if (err != null)
            {
                return ;
            }

            if (len > SizeofSockaddrAny)
            {
                panic("RawSockaddrAny too small");
            }

            sa, err = anyToSockaddr(_addr_rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }

            return ;

        });

        public static (long, error) Getfsstat(slice<Statfs_t> buf, long flags)
        {
            long n = default;
            error err = default!;

            unsafe.Pointer _p0 = default;            System.UIntPtr bufsize = default;            slice<statfs_freebsd11_t> oldBuf = default;            bool needsConvert = default;

            if (len(buf) > 0L)
            {
                if (supportsABI(_ino64First))
                {
                    _p0 = @unsafe.Pointer(_addr_buf[0L]);
                    bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
                }
                else
                {
                    var n = len(buf);
                    oldBuf = make_slice<statfs_freebsd11_t>(n);
                    _p0 = @unsafe.Pointer(_addr_oldBuf[0L]);
                    bufsize = @unsafe.Sizeof(new statfs_freebsd11_t()) * uintptr(n);
                    needsConvert = true;
                }

            }

            System.UIntPtr sysno = SYS_GETFSSTAT;
            if (supportsABI(_ino64First))
            {
                sysno = _SYS_GETFSSTAT_FREEBSD12;
            }

            var (r0, _, e1) = Syscall(sysno, uintptr(_p0), bufsize, uintptr(flags));
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }

            if (e1 == 0L && needsConvert)
            {
                foreach (var (i) in oldBuf)
                {
                    buf[i].convertFrom(_addr_oldBuf[i]);
                }

            }

            return ;

        }

        private static error setattrlistTimes(@string path, slice<Timespec> times)
        { 
            // used on Darwin for UtimesNano
            return error.As(ENOSYS)!;

        }

        public static error Stat(@string path, ptr<Stat_t> _addr_st)
        {
            error err = default!;
            ref Stat_t st = ref _addr_st.val;

            ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
            if (supportsABI(_ino64First))
            {
                return error.As(fstatat_freebsd12(_AT_FDCWD, path, st, 0L))!;
            }

            err = stat(path, _addr_oldStat);
            if (err != null)
            {
                return error.As(err)!;
            }

            st.convertFrom(_addr_oldStat);
            return error.As(null!)!;

        }

        public static error Lstat(@string path, ptr<Stat_t> _addr_st)
        {
            error err = default!;
            ref Stat_t st = ref _addr_st.val;

            ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
            if (supportsABI(_ino64First))
            {
                return error.As(fstatat_freebsd12(_AT_FDCWD, path, st, _AT_SYMLINK_NOFOLLOW))!;
            }

            err = lstat(path, _addr_oldStat);
            if (err != null)
            {
                return error.As(err)!;
            }

            st.convertFrom(_addr_oldStat);
            return error.As(null!)!;

        }

        public static error Fstat(long fd, ptr<Stat_t> _addr_st)
        {
            error err = default!;
            ref Stat_t st = ref _addr_st.val;

            ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
            if (supportsABI(_ino64First))
            {
                return error.As(fstat_freebsd12(fd, st))!;
            }

            err = fstat(fd, _addr_oldStat);
            if (err != null)
            {
                return error.As(err)!;
            }

            st.convertFrom(_addr_oldStat);
            return error.As(null!)!;

        }

        public static error Fstatat(long fd, @string path, ptr<Stat_t> _addr_st, long flags)
        {
            error err = default!;
            ref Stat_t st = ref _addr_st.val;

            ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
            if (supportsABI(_ino64First))
            {
                return error.As(fstatat_freebsd12(fd, path, st, flags))!;
            }

            err = fstatat(fd, path, _addr_oldStat, flags);
            if (err != null)
            {
                return error.As(err)!;
            }

            st.convertFrom(_addr_oldStat);
            return error.As(null!)!;

        }

        public static error Statfs(@string path, ptr<Statfs_t> _addr_st)
        {
            error err = default!;
            ref Statfs_t st = ref _addr_st.val;

            ref statfs_freebsd11_t oldStatfs = ref heap(out ptr<statfs_freebsd11_t> _addr_oldStatfs);
            if (supportsABI(_ino64First))
            {
                return error.As(statfs_freebsd12(path, st))!;
            }

            err = statfs(path, _addr_oldStatfs);
            if (err != null)
            {
                return error.As(err)!;
            }

            st.convertFrom(_addr_oldStatfs);
            return error.As(null!)!;

        }

        public static error Fstatfs(long fd, ptr<Statfs_t> _addr_st)
        {
            error err = default!;
            ref Statfs_t st = ref _addr_st.val;

            ref statfs_freebsd11_t oldStatfs = ref heap(out ptr<statfs_freebsd11_t> _addr_oldStatfs);
            if (supportsABI(_ino64First))
            {
                return error.As(fstatfs_freebsd12(fd, st))!;
            }

            err = fstatfs(fd, _addr_oldStatfs);
            if (err != null)
            {
                return error.As(err)!;
            }

            st.convertFrom(_addr_oldStatfs);
            return error.As(null!)!;

        }

        public static (long, error) Getdirentries(long fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep)
        {
            long n = default;
            error err = default!;
            ref System.UIntPtr basep = ref _addr_basep.val;

            if (supportsABI(_ino64First))
            {
                if (basep == null || @unsafe.Sizeof(basep) == 8L)
                {
                    return getdirentries_freebsd12(fd, buf, (uint64.val)(@unsafe.Pointer(basep)));
                } 
                // The freebsd12 syscall needs a 64-bit base. On 32-bit machines
                // we can't just use the basep passed in. See #32498.
                ref ulong @base = ref heap(uint64(basep), out ptr<ulong> _addr_@base);
                n, err = getdirentries_freebsd12(fd, buf, _addr_base);
                basep = uintptr(base);
                if (base >> (int)(32L) != 0L)
                { 
                    // We can't stuff the base back into a uintptr, so any
                    // future calls would be suspect. Generate an error.
                    // EIO is allowed by getdirentries.
                    err = EIO;

                }

                return ;

            } 

            // The old syscall entries are smaller than the new. Use 1/4 of the original
            // buffer size rounded up to DIRBLKSIZ (see /usr/src/lib/libc/sys/getdirentries.c).
            var oldBufLen = roundup(len(buf) / 4L, _dirblksiz);
            var oldBuf = make_slice<byte>(oldBufLen);
            n, err = getdirentries(fd, oldBuf, basep);
            if (err == null && n > 0L)
            {
                n = convertFromDirents11(buf, oldBuf[..n]);
            }

            return ;

        }

        public static error Mknod(@string path, uint mode, ulong dev)
        {
            error err = default!;

            long oldDev = default;
            if (supportsABI(_ino64First))
            {
                return error.As(mknodat_freebsd12(_AT_FDCWD, path, mode, dev))!;
            }

            oldDev = int(dev);
            return error.As(mknod(path, mode, oldDev))!;

        }

        // round x to the nearest multiple of y, larger or equal to x.
        //
        // from /usr/include/sys/param.h Macros for counting and rounding.
        // #define roundup(x, y)   ((((x)+((y)-1))/(y))*(y))
        private static long roundup(long x, long y)
        {
            return ((x + y - 1L) / y) * y;
        }

        private static void convertFrom(this ptr<Stat_t> _addr_s, ptr<stat_freebsd11_t> _addr_old)
        {
            ref Stat_t s = ref _addr_s.val;
            ref stat_freebsd11_t old = ref _addr_old.val;

            s.val = new Stat_t(Dev:uint64(old.Dev),Ino:uint64(old.Ino),Nlink:uint64(old.Nlink),Mode:old.Mode,Uid:old.Uid,Gid:old.Gid,Rdev:uint64(old.Rdev),Atimespec:old.Atimespec,Mtimespec:old.Mtimespec,Ctimespec:old.Ctimespec,Birthtimespec:old.Birthtimespec,Size:old.Size,Blocks:old.Blocks,Blksize:old.Blksize,Flags:old.Flags,Gen:uint64(old.Gen),);
        }

        private static void convertFrom(this ptr<Statfs_t> _addr_s, ptr<statfs_freebsd11_t> _addr_old)
        {
            ref Statfs_t s = ref _addr_s.val;
            ref statfs_freebsd11_t old = ref _addr_old.val;

            s.val = new Statfs_t(Version:_statfsVersion,Type:old.Type,Flags:old.Flags,Bsize:old.Bsize,Iosize:old.Iosize,Blocks:old.Blocks,Bfree:old.Bfree,Bavail:old.Bavail,Files:old.Files,Ffree:old.Ffree,Syncwrites:old.Syncwrites,Asyncwrites:old.Asyncwrites,Syncreads:old.Syncreads,Asyncreads:old.Asyncreads,Namemax:old.Namemax,Owner:old.Owner,Fsid:old.Fsid,);

            ref var sl = ref heap(old.Fstypename[..], out ptr<var> _addr_sl);
            var n = clen(new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)));
            copy(s.Fstypename[..], old.Fstypename[..n]);

            sl = old.Mntfromname[..];
            n = clen(new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)));
            copy(s.Mntfromname[..], old.Mntfromname[..n]);

            sl = old.Mntonname[..];
            n = clen(new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)));
            copy(s.Mntonname[..], old.Mntonname[..n]);
        }

        private static long convertFromDirents11(slice<byte> buf, slice<byte> old)
        {
            const var fixedSize = int(@unsafe.Offsetof(new Dirent().Name));
            const var oldFixedSize = int(@unsafe.Offsetof(new dirent_freebsd11().Name));

            long dstPos = 0L;
            long srcPos = 0L;
            while (dstPos + fixedSize < len(buf) && srcPos + oldFixedSize < len(old))
            {
                ref Dirent dstDirent = ref heap(out ptr<Dirent> _addr_dstDirent);
                ref dirent_freebsd11 srcDirent = ref heap(out ptr<dirent_freebsd11> _addr_srcDirent); 

                // If multiple direntries are written, sometimes when we reach the final one,
                // we may have cap of old less than size of dirent_freebsd11.
                copy(new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_srcDirent))[..], old[srcPos..]);

                var reclen = roundup(fixedSize + int(srcDirent.Namlen) + 1L, 8L);
                if (dstPos + reclen > len(buf))
                {
                    break;
                }

                dstDirent.Fileno = uint64(srcDirent.Fileno);
                dstDirent.Off = 0L;
                dstDirent.Reclen = uint16(reclen);
                dstDirent.Type = srcDirent.Type;
                dstDirent.Pad0 = 0L;
                dstDirent.Namlen = uint16(srcDirent.Namlen);
                dstDirent.Pad1 = 0L;

                copy(dstDirent.Name[..], srcDirent.Name[..srcDirent.Namlen]);
                copy(buf[dstPos..], new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_dstDirent))[..]);
                var padding = buf[dstPos + fixedSize + int(dstDirent.Namlen)..dstPos + reclen];
                foreach (var (i) in padding)
                {
                    padding[i] = 0L;
                }
                dstPos += int(dstDirent.Reclen);
                srcPos += int(srcDirent.Reclen);

            }


            return dstPos;

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
        //sys    Dup(fd int) (nfd int, err error)
        //sys    Dup2(from int, to int) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchflags(fd int, flags int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    fstat(fd int, stat *stat_freebsd11_t) (err error)
        //sys    fstat_freebsd12(fd int, stat *Stat_t) (err error) = _SYS_FSTAT_FREEBSD12
        //sys    fstatat(fd int, path string, stat *stat_freebsd11_t, flags int) (err error)
        //sys    fstatat_freebsd12(fd int, path string, stat *Stat_t, flags int) (err error) = _SYS_FSTATAT_FREEBSD12
        //sys    fstatfs(fd int, stat *statfs_freebsd11_t) (err error)
        //sys    fstatfs_freebsd12(fd int, stat *Statfs_t) (err error) = _SYS_FSTATFS_FREEBSD12
        //sys    Fsync(fd int) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sys    getdirentries(fd int, buf []byte, basep *uintptr) (n int, err error)
        //sys    getdirentries_freebsd12(fd int, buf []byte, basep *uint64) (n int, err error) = _SYS_GETDIRENTRIES_FREEBSD12
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
        //sysnb    Gettimeofday(tv *Timeval) (err error)
        //sysnb    Getuid() (uid int)
        //sys    Issetugid() (tainted bool)
        //sys    Kill(pid int, signum Signal) (err error)
        //sys    Kqueue() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Listen(s int, backlog int) (err error)
        //sys    lstat(path string, stat *stat_freebsd11_t) (err error)
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mkfifo(path string, mode uint32) (err error)
        //sys    mknod(path string, mode uint32, dev int) (err error)
        //sys    mknodat_freebsd12(fd int, path string, mode uint32, dev uint64) (err error) = _SYS_MKNODAT_FREEBSD12
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Pathconf(path string, name int) (val int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Readlink(path string, buf []byte) (n int, err error)
        //sys    Rename(from string, to string) (err error)
        //sys    Revoke(path string) (err error)
        //sys    Rmdir(path string) (err error)
        //sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = SYS_LSEEK
        //sys    Select(n int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (err error)
        //sysnb    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sys    Setlogin(name string) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tp *Timeval) (err error)
        //sysnb    Setuid(uid int) (err error)
        //sys    stat(path string, stat *stat_freebsd11_t) (err error)
        //sys    statfs(path string, stat *statfs_freebsd11_t) (err error)
        //sys    statfs_freebsd12(path string, stat *Statfs_t) (err error) = _SYS_STATFS_FREEBSD12
        //sys    Symlink(path string, link string) (err error)
        //sys    Sync() (err error)
        //sys    Truncate(path string, length int64) (err error)
        //sys    Umask(newmask int) (oldmask int)
        //sys    Undelete(path string) (err error)
        //sys    Unlink(path string) (err error)
        //sys    Unmount(path string, flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys   mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys   munmap(addr uintptr, length uintptr) (err error)
        //sys    readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_WRITE
        //sys    accept4(fd int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (nfd int, err error)
        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)
        //sys    getcwd(buf []byte) (n int, err error) = SYS___GETCWD
        //sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error) = SYS___SYSCTL
    }
}
