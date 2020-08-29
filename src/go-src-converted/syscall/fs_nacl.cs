// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// A simulated Unix-like file system for use within NaCl.
//
// The simulation is not particularly tied to NaCl other than the reuse
// of NaCl's definition for the Stat_t structure.
//
// The file system need never be written to disk, so it is represented as
// in-memory Go data structures, never in a serialized form.
//
// TODO: Perhaps support symlinks, although they muck everything up.

// package syscall -- go2cs converted at 2020 August 29 08:37:13 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\fs_nacl.go
using io = go.io_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        // Provided by package runtime.
        private static (long, int) now()
;

        // An fsys is a file system.
        // Since there is no I/O (everything is in memory),
        // the global lock mu protects the whole file system state,
        // and that's okay.
        private partial struct fsys
        {
            public sync.Mutex mu;
            public ptr<inode> root; // root directory
            public ptr<inode> cwd; // process current directory
            public ulong inum; // number of inodes created
            public slice<Func<(devFile, error)>> dev; // table for opening devices
        }

        // A devFile is the implementation required of device files
        // like /dev/null or /dev/random.
        private partial interface devFile
        {
            (long, error) pread(slice<byte> _p0, long _p0);
            (long, error) pwrite(slice<byte> _p0, long _p0);
        }

        // An inode is a (possibly special) file in the file system.
        private partial struct inode
        {
            public ref Stat_t Stat_t => ref Stat_t_val;
            public slice<byte> data;
            public slice<dirent> dir;
        }

        // A dirent describes a single directory entry.
        private partial struct dirent
        {
            public @string name;
            public ptr<inode> inode;
        }

        // An fsysFile is the fileImpl implementation backed by the file system.
        private partial struct fsysFile
        {
            public ref defaultFileImpl defaultFileImpl => ref defaultFileImpl_val;
            public ptr<fsys> fsys;
            public ptr<inode> inode;
            public long openmode;
            public long offset;
            public devFile dev;
        }

        // newFsys creates a new file system.
        private static ref fsys newFsys() => func((defer, _, __) =>
        {
            fsys fs = ref new fsys();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var ip = fs.newInode();
            ip.Mode = 0555L | S_IFDIR;
            fs.dirlink(ip, ".", ip);
            fs.dirlink(ip, "..", ip);
            fs.cwd = ip;
            fs.root = ip;
            return fs;
        });

        private static var fs = newFsys();
        private static Action fsinit = () =>
        {>>MARKER:FUNCTION_now_BLOCK_PREFIX<<
        };

        private static void init() => func((defer, _, __) =>
        { 
            // do not trigger loading of zipped file system here
            var oldFsinit = fsinit;
            defer(() =>
            {
                fsinit = oldFsinit;

            }());
            fsinit = () =>
            {
            }
;
            Mkdir("/dev", 0555L);
            Mkdir("/tmp", 0777L);
            mkdev("/dev/null", 0666L, openNull);
            mkdev("/dev/random", 0444L, openRandom);
            mkdev("/dev/urandom", 0444L, openRandom);
            mkdev("/dev/zero", 0666L, openZero);
            chdirEnv();
        });

        private static void chdirEnv()
        {
            var (pwd, ok) = Getenv("NACLPWD");
            if (ok)
            {
                chdir(pwd);
            }
        }

        // Except where indicated otherwise, unexported methods on fsys
        // expect fs.mu to have been locked by the caller.

        // newInode creates a new inode.
        private static ref inode newInode(this ref fsys fs)
        {
            fs.inum++;
            inode ip = ref new inode(Stat_t:Stat_t{Ino:fs.inum,Blksize:512,},);
            return ip;
        }

        // atime sets ip.Atime to the current time.
        private static void atime(this ref fsys fs, ref inode ip)
        {
            var (sec, nsec) = now();
            ip.Atime = sec;
            ip.AtimeNsec = int64(nsec);
        }

        // mtime sets ip.Mtime to the current time.
        private static void mtime(this ref fsys fs, ref inode ip)
        {
            var (sec, nsec) = now();
            ip.Mtime = sec;
            ip.MtimeNsec = int64(nsec);
        }

        // dirlookup looks for an entry in the directory dp with the given name.
        // It returns the directory entry and its index within the directory.
        private static (ref dirent, long, error) dirlookup(this ref fsys fs, ref inode dp, @string name)
        {
            fs.atime(dp);
            foreach (var (i) in dp.dir)
            {
                var de = ref dp.dir[i];
                if (de.name == name)
                {
                    fs.atime(de.inode);
                    return (de, i, null);
                }
            }
            return (null, 0L, ENOENT);
        }

        // dirlink adds to the directory dp an entry for name pointing at the inode ip.
        // If dp already contains an entry for name, that entry is overwritten.
        private static void dirlink(this ref fsys fs, ref inode dp, @string name, ref inode ip)
        {
            fs.mtime(dp);
            fs.atime(ip);
            ip.Nlink++;
            foreach (var (i) in dp.dir)
            {
                if (dp.dir[i].name == name)
                {
                    dp.dir[i] = new dirent(name,ip);
                    return;
                }
            }
            dp.dir = append(dp.dir, new dirent(name,ip));
            dp.dirSize();
        }

        private static void dirSize(this ref inode dp)
        {
            dp.Size = int64(len(dp.dir)) * (8L + 8L + 2L + 256L); // Dirent
        }

        // skipelem splits path into the first element and the remainder.
        // the returned first element contains no slashes, and the returned
        // remainder does not begin with a slash.
        private static (@string, @string) skipelem(@string path)
        {
            while (len(path) > 0L && path[0L] == '/')
            {
                path = path[1L..];
            }

            if (len(path) == 0L)
            {
                return ("", "");
            }
            long i = 0L;
            while (i < len(path) && path[i] != '/')
            {
                i++;
            }

            elem = path[..i];
            path = path[i..];
            while (len(path) > 0L && path[0L] == '/')
            {
                path = path[1L..];
            }

            return (elem, path);
        }

        // namei translates a file system path name into an inode.
        // If parent is false, the returned ip corresponds to the given name, and elem is the empty string.
        // If parent is true, the walk stops at the next-to-last element in the name,
        // so that ip is the parent directory and elem is the final element in the path.
        private static (ref inode, @string, error) namei(this ref fsys fs, @string path, bool parent)
        { 
            // Reject NUL in name.
            for (long i = 0L; i < len(path); i++)
            {
                if (path[i] == '\x00')
                {
                    return (null, "", EINVAL);
                }
            } 

            // Reject empty name.
 

            // Reject empty name.
            if (path == "")
            {
                return (null, "", EINVAL);
            }
            if (path[0L] == '/')
            {
                ip = fs.root;
            }
            else
            {
                ip = fs.cwd;
            }
            while (len(path) > 0L && path[len(path) - 1L] == '/')
            {
                path = path[..len(path) - 1L];
            }


            while (true)
            {
                var (elem, rest) = skipelem(path);
                if (elem == "")
                {
                    if (parent && ip.Mode & S_IFMT == S_IFDIR)
                    {
                        return (ip, ".", null);
                    }
                    break;
                }
                if (ip.Mode & S_IFMT != S_IFDIR)
                {
                    return (null, "", ENOTDIR);
                }
                if (len(elem) >= 256L)
                {
                    return (null, "", ENAMETOOLONG);
                }
                if (parent && rest == "")
                { 
                    // Stop one level early.
                    return (ip, elem, null);
                }
                var (de, _, err) = fs.dirlookup(ip, elem);
                if (err != null)
                {
                    return (null, "", err);
                }
                ip = de.inode;
                path = rest;
            }

            if (parent)
            {
                return (null, "", ENOTDIR);
            }
            return (ip, "", null);
        }

        // open opens or creates a file with the given name, open mode,
        // and permission mode bits.
        private static (fileImpl, error) open(this ref fsys fs, @string name, long openmode, uint mode)
        {
            var (dp, elem, err) = fs.namei(name, true);
            if (err != null)
            {
                return (null, err);
            }
            ref inode ip = default;            devFile dev = default;
            var (de, _, err) = fs.dirlookup(dp, elem);
            if (err != null)
            {
                if (openmode & O_CREATE == 0L)
                {
                    return (null, err);
                }
                ip = fs.newInode();
                ip.Mode = mode;
                fs.dirlink(dp, elem, ip);
                if (ip.Mode & S_IFMT == S_IFDIR)
                {
                    fs.dirlink(ip, ".", ip);
                    fs.dirlink(ip, "..", dp);
                }
            }
            else
            {
                ip = de.inode;
                if (openmode & (O_CREATE | O_EXCL) == O_CREATE | O_EXCL)
                {
                    return (null, EEXIST);
                }
                if (openmode & O_TRUNC != 0L)
                {
                    if (ip.Mode & S_IFMT == S_IFDIR)
                    {
                        return (null, EISDIR);
                    }
                    ip.data = null;
                }
                if (ip.Mode & S_IFMT == S_IFCHR)
                {
                    if (ip.Rdev < 0L || ip.Rdev >= int64(len(fs.dev)) || fs.dev[ip.Rdev] == null)
                    {
                        return (null, ENODEV);
                    }
                    dev, err = fs.dev[ip.Rdev]();
                    if (err != null)
                    {
                        return (null, err);
                    }
                }
            }

            if (openmode & O_ACCMODE == O_WRONLY || openmode & O_ACCMODE == O_RDWR) 
                if (ip.Mode & S_IFMT == S_IFDIR)
                {
                    return (null, EISDIR);
                }
            
            if (ip.Mode & S_IFMT == S_IFDIR) 
                if (openmode & O_ACCMODE != O_RDONLY)
                {
                    return (null, EISDIR);
                }
            else if (ip.Mode & S_IFMT == S_IFREG)             else if (ip.Mode & S_IFMT == S_IFCHR)             else 
                // TODO: some kind of special file
                return (null, EPERM);
                        fsysFile f = ref new fsysFile(fsys:fs,inode:ip,openmode:openmode,dev:dev,);
            if (openmode & O_APPEND != 0L)
            {
                f.offset = ip.Size;
            }
            return (f, null);
        }

        // fsysFile methods to implement fileImpl.

        private static error stat(this ref fsysFile _f, ref Stat_t _st) => func(_f, _st, (ref fsysFile f, ref Stat_t st, Defer defer, Panic _, Recover __) =>
        {
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            st.Value = f.inode.Stat_t;
            return error.As(null);
        });

        private static (long, error) read(this ref fsysFile _f, slice<byte> b) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            var (n, err) = f.preadLocked(b, f.offset);
            f.offset += int64(n);
            return (n, err);
        });

        public static (long, error) ReadDirent(long fd, slice<byte> buf) => func((defer, _, __) =>
        {
            var (f, err) = fdToFsysFile(fd);
            if (err != null)
            {
                return (0L, err);
            }
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            if (f.inode.Mode & S_IFMT != S_IFDIR)
            {
                return (0L, EINVAL);
            }
            var (n, err) = f.preadLocked(buf, f.offset);
            f.offset += int64(n);
            return (n, err);
        });

        private static (long, error) write(this ref fsysFile _f, slice<byte> b) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            var (n, err) = f.pwriteLocked(b, f.offset);
            f.offset += int64(n);
            return (n, err);
        });

        private static (long, error) seek(this ref fsysFile _f, long offset, long whence) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());

            if (whence == io.SeekCurrent) 
                offset += f.offset;
            else if (whence == io.SeekEnd) 
                offset += f.inode.Size;
                        if (offset < 0L)
            {
                return (0L, EINVAL);
            }
            if (offset > f.inode.Size)
            {
                return (0L, EINVAL);
            }
            f.offset = offset;
            return (offset, null);
        });

        private static (long, error) pread(this ref fsysFile _f, slice<byte> b, long offset) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            return f.preadLocked(b, offset);
        });

        private static (long, error) pwrite(this ref fsysFile _f, slice<byte> b, long offset) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            return f.pwriteLocked(b, offset);
        });

        private static (long, error) preadLocked(this ref fsysFile _f, slice<byte> b, long offset) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            if (f.openmode & O_ACCMODE == O_WRONLY)
            {
                return (0L, EINVAL);
            }
            if (offset < 0L)
            {
                return (0L, EINVAL);
            }
            if (f.dev != null)
            {
                f.fsys.atime(f.inode);
                f.fsys.mu.Unlock();
                defer(f.fsys.mu.Lock());
                return f.dev.pread(b, offset);
            }
            if (offset > f.inode.Size)
            {
                return (0L, null);
            }
            if (int64(len(b)) > f.inode.Size - offset)
            {
                b = b[..f.inode.Size - offset];
            }
            if (f.inode.Mode & S_IFMT == S_IFDIR)
            {
                if (offset % direntSize != 0L || len(b) != 0L && len(b) < direntSize)
                {
                    return (0L, EINVAL);
                }
                fs.atime(f.inode);
                long n = 0L;
                while (len(b) >= direntSize)
                {
                    var src = f.inode.dir[int(offset / direntSize)];
                    var dst = (Dirent.Value)(@unsafe.Pointer(ref b[0L]));
                    dst.Ino = int64(src.inode.Ino);
                    dst.Off = offset;
                    dst.Reclen = direntSize;
                    foreach (var (i) in dst.Name)
                    {
                        dst.Name[i] = 0L;
                    }
                    copy(dst.Name[..], src.name);
                    n += direntSize;
                    offset += direntSize;
                    b = b[direntSize..];
                }

                return (n, null);
            }
            fs.atime(f.inode);
            n = copy(b, f.inode.data[offset..]);
            return (n, null);
        });

        private static (long, error) pwriteLocked(this ref fsysFile _f, slice<byte> b, long offset) => func(_f, (ref fsysFile f, Defer defer, Panic _, Recover __) =>
        {
            if (f.openmode & O_ACCMODE == O_RDONLY)
            {
                return (0L, EINVAL);
            }
            if (offset < 0L)
            {
                return (0L, EINVAL);
            }
            if (f.dev != null)
            {
                f.fsys.atime(f.inode);
                f.fsys.mu.Unlock();
                defer(f.fsys.mu.Lock());
                return f.dev.pwrite(b, offset);
            }
            if (offset > f.inode.Size)
            {
                return (0L, EINVAL);
            }
            f.fsys.mtime(f.inode);
            var n = copy(f.inode.data[offset..], b);
            if (n < len(b))
            {
                f.inode.data = append(f.inode.data, b[n..]);
                f.inode.Size = int64(len(f.inode.data));
            }
            return (len(b), null);
        });

        // Standard Unix system calls.

        public static (long, error) Open(@string path, long openmode, uint perm) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (f, err) = fs.open(path, openmode, perm & 0777L | S_IFREG);
            if (err != null)
            {
                return (-1L, err);
            }
            return (newFD(f), null);
        });

        public static error Mkdir(@string path, uint perm) => func((defer, _, __) =>
        {
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (_, err) = fs.open(path, O_CREATE | O_EXCL, perm & 0777L | S_IFDIR);
            return error.As(err);
        });

        public static (long, error) Getcwd(slice<byte> buf)
        { 
            // Force package os to default to the old algorithm using .. and directory reads.
            return (0L, ENOSYS);
        }

        public static error Stat(@string path, ref Stat_t _st) => func(_st, (ref Stat_t st, Defer defer, Panic _, Recover __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            st.Value = ip.Stat_t;
            return error.As(null);
        });

        public static error Lstat(@string path, ref Stat_t st)
        {
            return error.As(Stat(path, st));
        }

        private static error unlink(@string path, bool isdir) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (dp, elem, err) = fs.namei(path, true);
            if (err != null)
            {
                return error.As(err);
            }
            if (elem == "." || elem == "..")
            {
                return error.As(EINVAL);
            }
            var (de, _, err) = fs.dirlookup(dp, elem);
            if (err != null)
            {
                return error.As(err);
            }
            if (isdir)
            {
                if (de.inode.Mode & S_IFMT != S_IFDIR)
                {
                    return error.As(ENOTDIR);
                }
                if (len(de.inode.dir) != 2L)
                {
                    return error.As(ENOTEMPTY);
                }
            }
            else
            {
                if (de.inode.Mode & S_IFMT == S_IFDIR)
                {
                    return error.As(EISDIR);
                }
            }
            de.inode.Nlink--;
            de.Value = dp.dir[len(dp.dir) - 1L];
            dp.dir = dp.dir[..len(dp.dir) - 1L];
            dp.dirSize();
            return error.As(null);
        });

        public static error Unlink(@string path)
        {
            return error.As(unlink(path, false));
        }

        public static error Rmdir(@string path)
        {
            return error.As(unlink(path, true));
        }

        public static error Chmod(@string path, uint mode) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            ip.Mode = ip.Mode & ~0777L | mode & 0777L;
            return error.As(null);
        });

        public static error Fchmod(long fd, uint mode) => func((defer, _, __) =>
        {
            var (f, err) = fdToFsysFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            f.inode.Mode = f.inode.Mode & ~0777L | mode & 0777L;
            return error.As(null);
        });

        public static error Chown(@string path, long uid, long gid) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            ip.Uid = uint32(uid);
            ip.Gid = uint32(gid);
            return error.As(null);
        });

        public static error Fchown(long fd, long uid, long gid) => func((defer, _, __) =>
        {
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (f, err) = fdToFsysFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            f.inode.Uid = uint32(uid);
            f.inode.Gid = uint32(gid);
            return error.As(null);
        });

        public static error Lchown(@string path, long uid, long gid)
        {
            return error.As(Chown(path, uid, gid));
        }

        public static error UtimesNano(@string path, slice<Timespec> ts) => func((defer, _, __) =>
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL);
            }
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            ip.Atime = ts[0L].Sec;
            ip.AtimeNsec = int64(ts[0L].Nsec);
            ip.Mtime = ts[1L].Sec;
            ip.MtimeNsec = int64(ts[1L].Nsec);
            return error.As(null);
        });

        public static error Link(@string path, @string link) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            var (dp, elem, err) = fs.namei(link, true);
            if (err != null)
            {
                return error.As(err);
            }
            if (ip.Mode & S_IFMT == S_IFDIR)
            {
                return error.As(EPERM);
            }
            _, _, err = fs.dirlookup(dp, elem);
            if (err == null)
            {
                return error.As(EEXIST);
            }
            fs.dirlink(dp, elem, ip);
            return error.As(null);
        });

        public static error Rename(@string from, @string to) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (fdp, felem, err) = fs.namei(from, true);
            if (err != null)
            {
                return error.As(err);
            }
            var (fde, _, err) = fs.dirlookup(fdp, felem);
            if (err != null)
            {
                return error.As(err);
            }
            var (tdp, telem, err) = fs.namei(to, true);
            if (err != null)
            {
                return error.As(err);
            }
            fs.dirlink(tdp, telem, fde.inode);
            fde.inode.Nlink--;
            fde.Value = fdp.dir[len(fdp.dir) - 1L];
            fdp.dir = fdp.dir[..len(fdp.dir) - 1L];
            fdp.dirSize();
            return error.As(null);
        });

        private static error truncate(this ref fsys fs, ref inode ip, long length)
        {
            if (length > 1e9F || ip.Mode & S_IFMT != S_IFREG)
            {
                return error.As(EINVAL);
            }
            if (length < int64(len(ip.data)))
            {
                ip.data = ip.data[..length];
            }
            else
            {
                var data = make_slice<byte>(length);
                copy(data, ip.data);
                ip.data = data;
            }
            ip.Size = int64(len(ip.data));
            return error.As(null);
        }

        public static error Truncate(@string path, long length) => func((defer, _, __) =>
        {
            fsinit();
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(fs.truncate(ip, length));
        });

        public static error Ftruncate(long fd, long length) => func((defer, _, __) =>
        {
            var (f, err) = fdToFsysFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            return error.As(f.fsys.truncate(f.inode, length));
        });

        public static error Chdir(@string path)
        {
            fsinit();
            return error.As(chdir(path));
        }

        private static error chdir(@string path) => func((defer, _, __) =>
        {
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (ip, _, err) = fs.namei(path, false);
            if (err != null)
            {
                return error.As(err);
            }
            fs.cwd = ip;
            return error.As(null);
        });

        public static error Fchdir(long fd) => func((defer, _, __) =>
        {
            var (f, err) = fdToFsysFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            f.fsys.mu.Lock();
            defer(f.fsys.mu.Unlock());
            if (f.inode.Mode & S_IFMT != S_IFDIR)
            {
                return error.As(ENOTDIR);
            }
            fs.cwd = f.inode;
            return error.As(null);
        });

        public static (long, error) Readlink(@string path, slice<byte> buf)
        {
            return (0L, ENOSYS);
        }

        public static error Symlink(@string path, @string link)
        {
            return error.As(ENOSYS);
        }

        public static error Fsync(long fd)
        {
            return error.As(null);
        }

        // Special devices.

        private static error mkdev(@string path, uint mode, Func<(devFile, error)> open)
        {
            var (f, err) = fs.open(path, O_CREATE | O_RDONLY | O_EXCL, S_IFCHR | mode);
            if (err != null)
            {
                return error.As(err);
            }
            ref fsysFile ip = f._<ref fsysFile>().inode;
            ip.Rdev = int64(len(fs.dev));
            fs.dev = append(fs.dev, open);
            return error.As(null);
        }

        private partial struct nullFile
        {
        }

        private static (devFile, error) openNull()
        {
            return (ref new nullFile(), null);
        }
        private static error close(this ref nullFile f)
        {
            return error.As(null);
        }
        private static (long, error) pread(this ref nullFile f, slice<byte> b, long offset)
        {
            return (0L, null);
        }
        private static (long, error) pwrite(this ref nullFile f, slice<byte> b, long offset)
        {
            return (len(b), null);
        }

        private partial struct zeroFile
        {
        }

        private static (devFile, error) openZero()
        {
            return (ref new zeroFile(), null);
        }
        private static error close(this ref zeroFile f)
        {
            return error.As(null);
        }
        private static (long, error) pwrite(this ref zeroFile f, slice<byte> b, long offset)
        {
            return (len(b), null);
        }

        private static (long, error) pread(this ref zeroFile f, slice<byte> b, long offset)
        {
            foreach (var (i) in b)
            {
                b[i] = 0L;
            }
            return (len(b), null);
        }

        private partial struct randomFile
        {
        }

        private static (devFile, error) openRandom()
        {
            return (new randomFile(), null);
        }

        private static error close(this randomFile f)
        {
            return error.As(null);
        }

        private static (long, error) pread(this randomFile f, slice<byte> b, long offset)
        {
            {
                var err = naclGetRandomBytes(b);

                if (err != null)
                {
                    return (0L, err);
                }

            }
            return (len(b), null);
        }

        private static (long, error) pwrite(this randomFile f, slice<byte> b, long offset)
        {
            return (0L, EPERM);
        }

        private static (ref fsysFile, error) fdToFsysFile(long fd)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (null, err);
            }
            var impl = f.impl;
            ref fsysFile (fsysf, ok) = impl._<ref fsysFile>();
            if (!ok)
            {
                return (null, EINVAL);
            }
            return (fsysf, null);
        }

        // create creates a file in the file system with the given name, mode, time, and data.
        // It is meant to be called when initializing the file system image.
        private static error create(@string name, uint mode, long sec, slice<byte> data) => func((defer, _, __) =>
        {
            fs.mu.Lock();
            defer(fs.mu.Unlock());
            var (f, err) = fs.open(name, O_CREATE | O_EXCL, mode);
            if (err != null)
            {
                if (mode & S_IFMT == S_IFDIR)
                {
                    var (ip, _, err) = fs.namei(name, false);
                    if (err == null && (ip.Mode & S_IFMT) == S_IFDIR)
                    {
                        return error.As(null); // directory already exists
                    }
                }
                return error.As(err);
            }
            ref fsysFile ip = f._<ref fsysFile>().inode;
            ip.Atime = sec;
            ip.Mtime = sec;
            ip.Ctime = sec;
            if (len(data) > 0L)
            {
                ip.Size = int64(len(data));
                ip.data = data;
            }
            return error.As(null);
        });
    }
}
