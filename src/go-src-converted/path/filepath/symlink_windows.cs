// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2020 October 08 03:37:05 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\symlink_windows.go
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
            @string _p0 = default;
            error _p0 = default!;

            var (p, err) = syscall.UTF16PtrFromString(path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            ref syscall.Win32finddata data = ref heap(out ptr<syscall.Win32finddata> _addr_data);

            var (h, err) = syscall.FindFirstFile(p, _addr_data);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            syscall.FindClose(h);

            return (syscall.UTF16ToString(data.FileName[..]), error.As(null!)!);

        }

        // baseIsDotDot reports whether the last element of path is "..".
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
            @string _p0 = default;
            error _p0 = default!;

            if (path == "")
            {
                return (path, error.As(null!)!);
            }

            path = Clean(path);

            var volume = normVolumeName(path);
            path = path[len(volume)..]; 

            // skip special cases
            if (path == "." || path == "\\")
            {
                return (volume + path, error.As(null!)!);
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
                    return ("", error.As(err)!);
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

            return (volume + normPath, error.As(null!)!);

        }

        private static (@string, error) evalSymlinks(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            var (newpath, err) = walkSymlinks(path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            newpath, err = toNorm(newpath, normBase);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (newpath, error.As(null!)!);

        }
    }
}}
