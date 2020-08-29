// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2020 August 29 08:22:30 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\symlink_windows.go
using errors = go.errors_package;
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;
using strings = go.strings_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        // normVolumeName is like VolumeName, but makes drive letter upper case.
        // result of EvalSymlinks must be unique, so we have
        // EvalSymlinks(`c:\a`) == EvalSymlinks(`C:\a`).
        private static @string normVolumeName(@string path)
        {
            var volume = VolumeName(path);

            if (len(volume) > 2L)
            { // isUNC
                return volume;
            }
            return strings.ToUpper(volume);
        }

        // normBase returns the last element of path with correct case.
        private static (@string, error) normBase(@string path)
        {
            var (p, err) = syscall.UTF16PtrFromString(path);
            if (err != null)
            {
                return ("", err);
            }
            syscall.Win32finddata data = default;

            var (h, err) = syscall.FindFirstFile(p, ref data);
            if (err != null)
            {
                return ("", err);
            }
            syscall.FindClose(h);

            return (syscall.UTF16ToString(data.FileName[..]), null);
        }

        // baseIsDotDot returns whether the last element of path is "..".
        // The given path should be 'Clean'-ed in advance.
        private static bool baseIsDotDot(@string path)
        {
            var i = strings.LastIndexByte(path, Separator);
            return path[i + 1L..] == "..";
        }

        // toNorm returns the normalized path that is guaranteed to be unique.
        // It should accept the following formats:
        //   * UNC paths                              (e.g \\server\share\foo\bar)
        //   * absolute paths                         (e.g C:\foo\bar)
        //   * relative paths begin with drive letter (e.g C:foo\bar, C:..\foo\bar, C:.., C:.)
        //   * relative paths begin with '\'          (e.g \foo\bar)
        //   * relative paths begin without '\'       (e.g foo\bar, ..\foo\bar, .., .)
        // The returned normalized path will be in the same form (of 5 listed above) as the input path.
        // If two paths A and B are indicating the same file with the same format, toNorm(A) should be equal to toNorm(B).
        // The normBase parameter should be equal to the normBase func, except for in tests.  See docs on the normBase func.
        private static (@string, error) toNorm(@string path, Func<@string, (@string, error)> normBase)
        {
            if (path == "")
            {
                return (path, null);
            }
            path = Clean(path);

            var volume = normVolumeName(path);
            path = path[len(volume)..]; 

            // skip special cases
            if (path == "." || path == "\\")
            {
                return (volume + path, null);
            }
            @string normPath = default;

            while (true)
            {
                if (baseIsDotDot(path))
                {
                    normPath = path + "\\" + normPath;

                    break;
                }
                var (name, err) = normBase(volume + path);
                if (err != null)
                {
                    return ("", err);
                }
                normPath = name + "\\" + normPath;

                var i = strings.LastIndexByte(path, Separator);
                if (i == -1L)
                {
                    break;
                }
                if (i == 0L)
                { // `\Go` or `C:\Go`
                    normPath = "\\" + normPath;

                    break;
                }
                path = path[..i];
            }


            normPath = normPath[..len(normPath) - 1L]; // remove trailing '\'

            return (volume + normPath, null);
        }

        // evalSymlinksUsingGetFinalPathNameByHandle uses Windows
        // GetFinalPathNameByHandle API to retrieve the final
        // path for the specified file.
        private static (@string, error) evalSymlinksUsingGetFinalPathNameByHandle(@string path) => func((defer, _, __) =>
        {
            var err = windows.LoadGetFinalPathNameByHandle();
            if (err != null)
            { 
                // we must be using old version of Windows
                return ("", err);
            }
            if (path == "")
            {
                return (path, null);
            } 

            // Use Windows I/O manager to dereference the symbolic link, as per
            // https://blogs.msdn.microsoft.com/oldnewthing/20100212-00/?p=14963/
            var (p, err) = syscall.UTF16PtrFromString(path);
            if (err != null)
            {
                return ("", err);
            }
            var (h, err) = syscall.CreateFile(p, 0L, 0L, null, syscall.OPEN_EXISTING, syscall.FILE_FLAG_BACKUP_SEMANTICS, 0L);
            if (err != null)
            {
                return ("", err);
            }
            defer(syscall.CloseHandle(h));

            var buf = make_slice<ushort>(100L);
            while (true)
            {
                var (n, err) = windows.GetFinalPathNameByHandle(h, ref buf[0L], uint32(len(buf)), windows.VOLUME_NAME_DOS);
                if (err != null)
                {
                    return ("", err);
                }
                if (n < uint32(len(buf)))
                {
                    break;
                }
                buf = make_slice<ushort>(n);
            }

            var s = syscall.UTF16ToString(buf);
            if (len(s) > 4L && s[..4L] == "\\\\?\\")
            {
                s = s[4L..];
                if (len(s) > 3L && s[..3L] == "UNC")
                { 
                    // return path like \\server\share\...
                    return ("\\" + s[3L..], null);
                }
                return (s, null);
            }
            return ("", errors.New("GetFinalPathNameByHandle returned unexpected path=" + s));
        });

        private static bool samefile(@string path1, @string path2)
        {
            var (fi1, err) = os.Lstat(path1);
            if (err != null)
            {
                return false;
            }
            var (fi2, err) = os.Lstat(path2);
            if (err != null)
            {
                return false;
            }
            return os.SameFile(fi1, fi2);
        }

        private static (@string, error) evalSymlinks(@string path)
        {
            var (newpath, err) = walkSymlinks(path);
            if (err != null)
            {
                var (newpath2, err2) = evalSymlinksUsingGetFinalPathNameByHandle(path);
                if (err2 == null)
                {
                    return toNorm(newpath2, normBase);
                }
                return ("", err);
            }
            newpath, err = toNorm(newpath, normBase);
            if (err != null)
            {
                (newpath2, err2) = evalSymlinksUsingGetFinalPathNameByHandle(path);
                if (err2 == null)
                {
                    return toNorm(newpath2, normBase);
                }
                return ("", err);
            }
            if (strings.ToUpper(newpath) == strings.ToUpper(path))
            { 
                // walkSymlinks did not actually walk any symlinks,
                // so we don't need to try GetFinalPathNameByHandle.
                return (newpath, null);
            }
            (newpath2, err2) = evalSymlinksUsingGetFinalPathNameByHandle(path);
            if (err2 != null)
            {
                return (newpath, null);
            }
            newpath2, err2 = toNorm(newpath2, normBase);
            if (err2 != null)
            {
                return (newpath, null);
            }
            if (samefile(newpath, newpath2))
            {
                return (newpath, null);
            }
            return (newpath2, null);
        }
    }
}}
