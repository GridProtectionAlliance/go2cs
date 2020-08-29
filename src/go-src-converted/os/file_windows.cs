// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file_windows.go
using poll = go.@internal.poll_package;
using windows = go.@internal.syscall.windows_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // file is the real representation of *File.
        // The extra level of indirection ensures that no clients of os
        // can overwrite this data, which could cause the finalizer
        // to close the wrong file descriptor.
        private partial struct file
        {
            public poll.FD pfd;
            public @string name;
            public ptr<dirInfo> dirinfo; // nil unless directory being read
        }

        // Fd returns the Windows handle referencing the open file.
        // The handle is valid only until f.Close is called or f is garbage collected.
        // On Unix systems this will cause the SetDeadline methods to stop working.
        private static System.UIntPtr Fd(this ref File file)
        {
            if (file == null)
            {
                return uintptr(syscall.InvalidHandle);
            }
            return uintptr(file.pfd.Sysfd);
        }

        // newFile returns a new File with the given file handle and name.
        // Unlike NewFile, it does not check that h is syscall.InvalidHandle.
        private static ref File newFile(syscall.Handle h, @string name, @string kind)
        {
            if (kind == "file")
            {
                uint m = default;
                if (syscall.GetConsoleMode(h, ref m) == null)
                {
                    kind = "console";
                }
            }
            File f = ref new File(&file{pfd:poll.FD{Sysfd:h,IsStream:true,ZeroReadIsEOF:true,},name:name,});
            runtime.SetFinalizer(f.file, ref file); 

            // Ignore initialization errors.
            // Assume any problems will show up in later I/O.
            f.pfd.Init(kind, false);

            return f;
        }

        // newConsoleFile creates new File that will be used as console.
        private static ref File newConsoleFile(syscall.Handle h, @string name)
        {
            return newFile(h, name, "console");
        }

        // NewFile returns a new File with the given file descriptor and
        // name. The returned value will be nil if fd is not a valid file
        // descriptor.
        public static ref File NewFile(System.UIntPtr fd, @string name)
        {
            var h = syscall.Handle(fd);
            if (h == syscall.InvalidHandle)
            {
                return null;
            }
            return newFile(h, name, "file");
        }

        // Auxiliary information if the File describes a directory
        private partial struct dirInfo
        {
            public syscall.Win32finddata data;
            public bool needdata;
            public @string path;
            public bool isempty; // set if FindFirstFile returns ERROR_FILE_NOT_FOUND
        }

        private static void epipecheck(ref File file, error e)
        {
        }

        public static readonly @string DevNull = "NUL";



        private static bool isdir(this ref file f)
        {
            return f != null && f.dirinfo != null;
        }

        private static (ref File, error) openFile(@string name, long flag, FileMode perm)
        {
            var (r, e) = syscall.Open(fixLongPath(name), flag | syscall.O_CLOEXEC, syscallMode(perm));
            if (e != null)
            {
                return (null, e);
            }
            return (newFile(r, name, "file"), null);
        }

        private static (ref File, error) openDir(@string name)
        {
            @string mask = default;

            var path = fixLongPath(name);

            if (len(path) == 2L && path[1L] == ':' || (len(path) > 0L && path[len(path) - 1L] == '\\'))
            { // it is a drive letter, like C:
                mask = path + "*";
            }
            else
            {
                mask = path + "\\*";
            }
            var (maskp, e) = syscall.UTF16PtrFromString(mask);
            if (e != null)
            {
                return (null, e);
            }
            ptr<dirInfo> d = @new<dirInfo>();
            var (r, e) = syscall.FindFirstFile(maskp, ref d.data);
            if (e != null)
            { 
                // FindFirstFile returns ERROR_FILE_NOT_FOUND when
                // no matching files can be found. Then, if directory
                // exists, we should proceed.
                if (e != syscall.ERROR_FILE_NOT_FOUND)
                {
                    return (null, e);
                }
                syscall.Win32FileAttributeData fa = default;
                var (pathp, e) = syscall.UTF16PtrFromString(path);
                if (e != null)
                {
                    return (null, e);
                }
                e = syscall.GetFileAttributesEx(pathp, syscall.GetFileExInfoStandard, (byte.Value)(@unsafe.Pointer(ref fa)));
                if (e != null)
                {
                    return (null, e);
                }
                if (fa.FileAttributes & syscall.FILE_ATTRIBUTE_DIRECTORY == 0L)
                {
                    return (null, e);
                }
                d.isempty = true;
            }
            d.path = path;
            if (!isAbs(d.path))
            {
                d.path, e = syscall.FullPath(d.path);
                if (e != null)
                {
                    return (null, e);
                }
            }
            var f = newFile(r, name, "dir");
            f.dirinfo = d;
            return (f, null);
        }

        // openFileNolog is the Windows implementation of OpenFile.
        private static (ref File, error) openFileNolog(@string name, long flag, FileMode perm)
        {
            if (name == "")
            {
                return (null, ref new PathError("open",name,syscall.ENOENT));
            }
            var (r, errf) = openFile(name, flag, perm);
            if (errf == null)
            {
                return (r, null);
            }
            var (r, errd) = openDir(name);
            if (errd == null)
            {
                if (flag & O_WRONLY != 0L || flag & O_RDWR != 0L)
                {
                    r.Close();
                    return (null, ref new PathError("open",name,syscall.EISDIR));
                }
                return (r, null);
            }
            return (null, ref new PathError("open",name,errf));
        }

        // Close closes the File, rendering it unusable for I/O.
        // It returns an error, if any.
        private static error Close(this ref File file)
        {
            if (file == null)
            {
                return error.As(ErrInvalid);
            }
            return error.As(file.file.close());
        }

        private static error close(this ref file file)
        {
            if (file == null)
            {
                return error.As(syscall.EINVAL);
            }
            if (file.isdir() && file.dirinfo.isempty)
            { 
                // "special" empty directories
                return error.As(null);
            }
            error err = default;
            {
                var e = file.pfd.Close();

                if (e != null)
                {
                    if (e == poll.ErrFileClosing)
                    {
                        e = ErrClosed;
                    }
                    err = error.As(ref new PathError("close",file.name,e));
                } 

                // no need for a finalizer anymore

            } 

            // no need for a finalizer anymore
            runtime.SetFinalizer(file, null);
            return error.As(err);
        }

        // read reads up to len(b) bytes from the File.
        // It returns the number of bytes read and an error, if any.
        private static (long, error) read(this ref File f, slice<byte> b)
        {
            n, err = f.pfd.Read(b);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // pread reads len(b) bytes from the File starting at byte offset off.
        // It returns the number of bytes read and the error, if any.
        // EOF is signaled by a zero count with err set to 0.
        private static (long, error) pread(this ref File f, slice<byte> b, long off)
        {
            n, err = f.pfd.Pread(b, off);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // write writes len(b) bytes to the File.
        // It returns the number of bytes written and an error, if any.
        private static (long, error) write(this ref File f, slice<byte> b)
        {
            n, err = f.pfd.Write(b);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // pwrite writes len(b) bytes to the File starting at byte offset off.
        // It returns the number of bytes written and an error, if any.
        private static (long, error) pwrite(this ref File f, slice<byte> b, long off)
        {
            n, err = f.pfd.Pwrite(b, off);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // seek sets the offset for the next Read or Write on file to offset, interpreted
        // according to whence: 0 means relative to the origin of the file, 1 means
        // relative to the current offset, and 2 means relative to the end.
        // It returns the new offset and an error, if any.
        private static (long, error) seek(this ref File f, long offset, long whence)
        {
            ret, err = f.pfd.Seek(offset, whence);
            runtime.KeepAlive(f);
            return (ret, err);
        }

        // Truncate changes the size of the named file.
        // If the file is a symbolic link, it changes the size of the link's target.
        public static error Truncate(@string name, long size) => func((defer, _, __) =>
        {
            var (f, e) = OpenFile(name, O_WRONLY | O_CREATE, 0666L);
            if (e != null)
            {
                return error.As(e);
            }
            defer(f.Close());
            var e1 = f.Truncate(size);
            if (e1 != null)
            {
                return error.As(e1);
            }
            return error.As(null);
        });

        // Remove removes the named file or directory.
        // If there is an error, it will be of type *PathError.
        public static error Remove(@string name)
        {
            var (p, e) = syscall.UTF16PtrFromString(fixLongPath(name));
            if (e != null)
            {
                return error.As(ref new PathError("remove",name,e));
            } 

            // Go file interface forces us to know whether
            // name is a file or directory. Try both.
            e = syscall.DeleteFile(p);
            if (e == null)
            {
                return error.As(null);
            }
            var e1 = syscall.RemoveDirectory(p);
            if (e1 == null)
            {
                return error.As(null);
            } 

            // Both failed: figure out which error to return.
            if (e1 != e)
            {
                var (a, e2) = syscall.GetFileAttributes(p);
                if (e2 != null)
                {
                    e = e2;
                }
                else
                {
                    if (a & syscall.FILE_ATTRIBUTE_DIRECTORY != 0L)
                    {
                        e = e1;
                    }
                    else if (a & syscall.FILE_ATTRIBUTE_READONLY != 0L)
                    {
                        e1 = syscall.SetFileAttributes(p, a & ~syscall.FILE_ATTRIBUTE_READONLY);

                        if (e1 == null)
                        {
                            e = syscall.DeleteFile(p);

                            if (e == null)
                            {
                                return error.As(null);
                            }
                        }
                    }
                }
            }
            return error.As(ref new PathError("remove",name,e));
        }

        private static error rename(@string oldname, @string newname)
        {
            var e = windows.Rename(fixLongPath(oldname), fixLongPath(newname));
            if (e != null)
            {
                return error.As(ref new LinkError("rename",oldname,newname,e));
            }
            return error.As(null);
        }

        // Pipe returns a connected pair of Files; reads from r return bytes written to w.
        // It returns the files and an error, if any.
        public static (ref File, ref File, error) Pipe()
        {
            array<syscall.Handle> p = new array<syscall.Handle>(2L);
            var e = syscall.CreatePipe(ref p[0L], ref p[1L], null, 0L);
            if (e != null)
            {
                return (null, null, NewSyscallError("pipe", e));
            }
            return (newFile(p[0L], "|0", "file"), newFile(p[1L], "|1", "file"), null);
        }

        private static @string tempDir()
        {
            var n = uint32(syscall.MAX_PATH);
            while (true)
            {
                var b = make_slice<ushort>(n);
                n, _ = syscall.GetTempPath(uint32(len(b)), ref b[0L]);
                if (n > uint32(len(b)))
                {
                    continue;
                }
                if (n > 0L && b[n - 1L] == '\\')
                {
                    n--;
                }
                return string(utf16.Decode(b[..n]));
            }

        }

        // Link creates newname as a hard link to the oldname file.
        // If there is an error, it will be of type *LinkError.
        public static error Link(@string oldname, @string newname)
        {
            var (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
            if (err != null)
            {
                return error.As(ref new LinkError("link",oldname,newname,err));
            }
            var (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
            if (err != null)
            {
                return error.As(ref new LinkError("link",oldname,newname,err));
            }
            err = syscall.CreateHardLink(n, o, 0L);
            if (err != null)
            {
                return error.As(ref new LinkError("link",oldname,newname,err));
            }
            return error.As(null);
        }

        // Symlink creates newname as a symbolic link to oldname.
        // If there is an error, it will be of type *LinkError.
        public static error Symlink(@string oldname, @string newname)
        { 
            // CreateSymbolicLink is not supported before Windows Vista
            if (syscall.LoadCreateSymbolicLink() != null)
            {
                return error.As(ref new LinkError("symlink",oldname,newname,syscall.EWINDOWS));
            } 

            // '/' does not work in link's content
            oldname = fromSlash(oldname); 

            // need the exact location of the oldname when its relative to determine if its a directory
            var destpath = oldname;
            if (!isAbs(oldname))
            {
                destpath = dirname(newname) + "\\" + oldname;
            }
            var (fi, err) = Lstat(destpath);
            var isdir = err == null && fi.IsDir();

            var (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
            if (err != null)
            {
                return error.As(ref new LinkError("symlink",oldname,newname,err));
            }
            var (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
            if (err != null)
            {
                return error.As(ref new LinkError("symlink",oldname,newname,err));
            }
            uint flags = default;
            if (isdir)
            {
                flags |= syscall.SYMBOLIC_LINK_FLAG_DIRECTORY;
            }
            err = syscall.CreateSymbolicLink(n, o, flags);
            if (err != null)
            {
                return error.As(ref new LinkError("symlink",oldname,newname,err));
            }
            return error.As(null);
        }
    }
}
