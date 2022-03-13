// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2022 March 13 05:28:17 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Program Files\Go\src\path\filepath\symlink.go
namespace go.path;

using errors = errors_package;
using fs = io.fs_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;

public static partial class filepath_package {

private static (@string, error) walkSymlinks(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    var volLen = volumeNameLen(path);
    var pathSeparator = string(os.PathSeparator);

    if (volLen < len(path) && os.IsPathSeparator(path[volLen])) {
        volLen++;
    }
    var vol = path[..(int)volLen];
    var dest = vol;
    nint linksWalked = 0;
    {
        var start = volLen;
        var end = volLen;

        while (start < len(path)) {
            while (start < len(path) && os.IsPathSeparator(path[start])) {
                start++;
            start = end;
            }
            end = start;
            while (end < len(path) && !os.IsPathSeparator(path[end])) {
                end++;
            } 

            // On Windows, "." can be a symlink.
            // We look it up, and use the value if it is absolute.
            // If not, we just return ".".
            var isWindowsDot = runtime.GOOS == "windows" && path[(int)volumeNameLen(path)..] == "."; 

            // The next path component is in path[start:end].
            if (end == start) { 
                // No more path components.
                break;
            }
            else if (path[(int)start..(int)end] == "." && !isWindowsDot) { 
                // Ignore path component ".".
                continue;
            }
            else if (path[(int)start..(int)end] == "..") { 
                // Back up to previous component if possible.
                // Note that volLen includes any leading slash.

                // Set r to the index of the last slash in dest,
                // after the volume.
                nint r = default;
                for (r = len(dest) - 1; r >= volLen; r--) {
                    if (os.IsPathSeparator(dest[r])) {
                        break;
                    }
                }
                if (r < volLen || dest[(int)r + 1..] == "..") { 
                    // Either path has no slashes
                    // (it's empty or just "C:")
                    // or it ends in a ".." we had to keep.
                    // Either way, keep this "..".
                    if (len(dest) > volLen) {
                        dest += pathSeparator;
                    }
                    dest += "..";
                }
                else
 { 
                    // Discard everything since the last slash.
                    dest = dest[..(int)r];
                }
                continue;
            }
            if (len(dest) > volumeNameLen(dest) && !os.IsPathSeparator(dest[len(dest) - 1])) {
                dest += pathSeparator;
            }
            dest += path[(int)start..(int)end]; 

            // Resolve symlink.

            var (fi, err) = os.Lstat(dest);
            if (err != null) {
                return ("", error.As(err)!);
            }
            if (fi.Mode() & fs.ModeSymlink == 0) {
                if (!fi.Mode().IsDir() && end < len(path)) {
                    return ("", error.As(syscall.ENOTDIR)!);
                }
                continue;
            }
            linksWalked++;
            if (linksWalked > 255) {
                return ("", error.As(errors.New("EvalSymlinks: too many links"))!);
            }
            var (link, err) = os.Readlink(dest);
            if (err != null) {
                return ("", error.As(err)!);
            }
            if (isWindowsDot && !IsAbs(link)) { 
                // On Windows, if "." is a relative symlink,
                // just return ".".
                break;
            }
            path = link + path[(int)end..];

            var v = volumeNameLen(link);
            if (v > 0) { 
                // Symlink to drive name is an absolute path.
                if (v < len(link) && os.IsPathSeparator(link[v])) {
                    v++;
                }
                vol = link[..(int)v];
                dest = vol;
                end = len(vol);
            }
            else if (len(link) > 0 && os.IsPathSeparator(link[0])) { 
                // Symlink to absolute path.
                dest = link[..(int)1];
                end = 1;
            }
            else
 { 
                // Symlink to relative path; replace last
                // path component in dest.
                r = default;
                for (r = len(dest) - 1; r >= volLen; r--) {
                    if (os.IsPathSeparator(dest[r])) {
                        break;
                    }
                }
                if (r < volLen) {
                    dest = vol;
                }
                else
 {
                    dest = dest[..(int)r];
                }
                end = 0;
            }
        }
    }
    return (Clean(dest), error.As(null!)!);
}

} // end filepath_package
