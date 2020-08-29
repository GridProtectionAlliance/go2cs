// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2020 August 29 08:22:29 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\symlink.go
using errors = go.errors_package;
using os = go.os_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        // isRoot returns true if path is root of file system
        // (`/` on unix and `/`, `\`, `c:\` or `c:/` on windows).
        private static bool isRoot(@string path)
        {
            if (runtime.GOOS != "windows")
            {
                return path == "/";
            }
            switch (len(path))
            {
                case 1L: 
                    return os.IsPathSeparator(path[0L]);
                    break;
                case 3L: 
                    return path[1L] == ':' && os.IsPathSeparator(path[2L]);
                    break;
            }
            return false;
        }

        // isDriveLetter returns true if path is Windows drive letter (like "c:").
        private static bool isDriveLetter(@string path)
        {
            if (runtime.GOOS != "windows")
            {
                return false;
            }
            return len(path) == 2L && path[1L] == ':';
        }

        private static (@string, bool, error) walkLink(@string path, ref long linksWalked)
        {
            if (linksWalked > 255L.Value)
            {
                return ("", false, errors.New("EvalSymlinks: too many links"));
            }
            var (fi, err) = os.Lstat(path);
            if (err != null)
            {
                return ("", false, err);
            }
            if (fi.Mode() & os.ModeSymlink == 0L)
            {
                return (path, false, null);
            }
            newpath, err = os.Readlink(path);
            if (err != null)
            {
                return ("", false, err);
            }
            linksWalked.Value++;
            return (newpath, true, null);
        }

        private static (@string, error) walkLinks(@string path, ref long linksWalked)
        {
            {
                var (dir, file) = Split(path);


                if (dir == "") 
                    var (newpath, _, err) = walkLink(file, linksWalked);
                    return (newpath, err);
                else if (file == "") 
                    if (isDriveLetter(dir))
                    {
                        return (dir, null);
                    }
                    if (os.IsPathSeparator(dir[len(dir) - 1L]))
                    {
                        if (isRoot(dir))
                        {
                            return (dir, null);
                        }
                        return walkLinks(dir[..len(dir) - 1L], linksWalked);
                    }
                    (newpath, _, err) = walkLink(dir, linksWalked);
                    return (newpath, err);
                else 
                    var (newdir, err) = walkLinks(dir, linksWalked);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    var (newpath, islink, err) = walkLink(Join(newdir, file), linksWalked);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    if (!islink)
                    {
                        return (newpath, null);
                    }
                    if (IsAbs(newpath) || os.IsPathSeparator(newpath[0L]))
                    {
                        return (newpath, null);
                    }
                    return (Join(newdir, newpath), null);

            }
        }

        private static (@string, error) walkSymlinks(@string path)
        {
            if (path == "")
            {
                return (path, null);
            }
            long linksWalked = default; // to protect against cycles
            while (true)
            {
                var i = linksWalked;
                var (newpath, err) = walkLinks(path, ref linksWalked);
                if (err != null)
                {
                    return ("", err);
                }
                if (runtime.GOOS == "windows")
                { 
                    // walkLinks(".", ...) always returns "." on unix.
                    // But on windows it returns symlink target, if current
                    // directory is a symlink. Stop the walk, if symlink
                    // target is not absolute path, and return "."
                    // to the caller (just like unix does).
                    // Same for "C:.".
                    if (path[volumeNameLen(path)..] == "." && !IsAbs(newpath))
                    {
                        return (path, null);
                    }
                }
                if (i == linksWalked)
                {
                    return (Clean(newpath), null);
                }
                path = newpath;
            }

        }
    }
}}
