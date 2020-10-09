// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2020 October 09 04:49:47 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\symlink.go
using errors = go.errors_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        private static (@string, error) walkSymlinks(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            var volLen = volumeNameLen(path);
            var pathSeparator = string(os.PathSeparator);

            if (volLen < len(path) && os.IsPathSeparator(path[volLen]))
            {
                volLen++;
            }
            var vol = path[..volLen];
            var dest = vol;
            long linksWalked = 0L;
            {
                var start = volLen;
                var end = volLen;

                while (start < len(path))
                {
                    while (start < len(path) && os.IsPathSeparator(path[start]))
                    {
                        start++;
                    start = end;
                    }
                    end = start;
                    while (end < len(path) && !os.IsPathSeparator(path[end]))
                    {
                        end++;
                    } 

                    // On Windows, "." can be a symlink.
                    // We look it up, and use the value if it is absolute.
                    // If not, we just return ".".
                    var isWindowsDot = runtime.GOOS == "windows" && path[volumeNameLen(path)..] == "."; 

                    // The next path component is in path[start:end].
                    if (end == start)
                    { 
                        // No more path components.
                        break;

                    }
                    else if (path[start..end] == "." && !isWindowsDot)
                    { 
                        // Ignore path component ".".
                        continue;

                    }
                    else if (path[start..end] == "..")
                    { 
                        // Back up to previous component if possible.
                        // Note that volLen includes any leading slash.

                        // Set r to the index of the last slash in dest,
                        // after the volume.
                        long r = default;
                        for (r = len(dest) - 1L; r >= volLen; r--)
                        {
                            if (os.IsPathSeparator(dest[r]))
                            {
                                break;
                            }
                        }
                        if (r < volLen || dest[r + 1L..] == "..")
                        { 
                            // Either path has no slashes
                            // (it's empty or just "C:")
                            // or it ends in a ".." we had to keep.
                            // Either way, keep this "..".
                            if (len(dest) > volLen)
                            {
                                dest += pathSeparator;
                            }
                            dest += "..";

                        }
                        else
                        { 
                            // Discard everything since the last slash.
                            dest = dest[..r];

                        }
                        continue;

                    }
                    if (len(dest) > volumeNameLen(dest) && !os.IsPathSeparator(dest[len(dest) - 1L]))
                    {
                        dest += pathSeparator;
                    }
                    dest += path[start..end]; 

                    // Resolve symlink.

                    var (fi, err) = os.Lstat(dest);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }
                    if (fi.Mode() & os.ModeSymlink == 0L)
                    {
                        if (!fi.Mode().IsDir() && end < len(path))
                        {
                            return ("", error.As(syscall.ENOTDIR)!);
                        }
                        continue;

                    }
                    linksWalked++;
                    if (linksWalked > 255L)
                    {
                        return ("", error.As(errors.New("EvalSymlinks: too many links"))!);
                    }
                    var (link, err) = os.Readlink(dest);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }
                    if (isWindowsDot && !IsAbs(link))
                    { 
                        // On Windows, if "." is a relative symlink,
                        // just return ".".
                        break;

                    }
                    path = link + path[end..];

                    var v = volumeNameLen(link);
                    if (v > 0L)
                    { 
                        // Symlink to drive name is an absolute path.
                        if (v < len(link) && os.IsPathSeparator(link[v]))
                        {
                            v++;
                        }
                        vol = link[..v];
                        dest = vol;
                        end = len(vol);

                    }
                    else if (len(link) > 0L && os.IsPathSeparator(link[0L]))
                    { 
                        // Symlink to absolute path.
                        dest = link[..1L];
                        end = 1L;

                    }
                    else
                    { 
                        // Symlink to relative path; replace last
                        // path component in dest.
                        r = default;
                        for (r = len(dest) - 1L; r >= volLen; r--)
                        {
                            if (os.IsPathSeparator(dest[r]))
                            {
                                break;
                            }
                        }
                        if (r < volLen)
                        {
                            dest = vol;
                        }
                        else
                        {
                            dest = dest[..r];
                        }
                        end = 0L;

                    }
                }
            }
            return (Clean(dest), error.As(null!)!);

        }
    }
}}
