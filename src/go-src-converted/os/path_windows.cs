// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:53 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\path_windows.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        public static readonly char PathSeparator = (char)'\\'; // OS-specific path separator
        public static readonly char PathListSeparator = (char)';'; // OS-specific path list separator

        // IsPathSeparator reports whether c is a directory separator character.
        public static bool IsPathSeparator(byte c)
        { 
            // NOTE: Windows accept / as path separator.
            return c == '\\' || c == '/';

        }

        // basename removes trailing slashes and the leading
        // directory name and drive letter from path name.
        private static @string basename(@string name)
        { 
            // Remove drive letter
            if (len(name) == 2L && name[1L] == ':')
            {
                name = ".";
            }
            else if (len(name) > 2L && name[1L] == ':')
            {
                name = name[2L..];
            }

            var i = len(name) - 1L; 
            // Remove trailing slashes
            while (i > 0L && (name[i] == '/' || name[i] == '\\'))
            {
                name = name[..i];
                i--;
            } 
            // Remove leading directory name
 
            // Remove leading directory name
            i--;

            while (i >= 0L)
            {
                if (name[i] == '/' || name[i] == '\\')
                {
                    name = name[i + 1L..];
                    break;
                i--;
                }

            }

            return name;

        }

        private static bool isAbs(@string path)
        {
            bool b = default;

            var v = volumeName(path);
            if (v == "")
            {
                return false;
            }

            path = path[len(v)..];
            if (path == "")
            {
                return false;
            }

            return IsPathSeparator(path[0L]);

        }

        private static @string volumeName(@string path)
        {
            @string v = default;

            if (len(path) < 2L)
            {
                return "";
            } 
            // with drive letter
            var c = path[0L];
            if (path[1L] == ':' && ('0' <= c && c <= '9' || 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z'))
            {
                return path[..2L];
            } 
            // is it UNC
            {
                var l = len(path);

                if (l >= 5L && IsPathSeparator(path[0L]) && IsPathSeparator(path[1L]) && !IsPathSeparator(path[2L]) && path[2L] != '.')
                { 
                    // first, leading `\\` and next shouldn't be `\`. its server name.
                    for (long n = 3L; n < l - 1L; n++)
                    { 
                        // second, next '\' shouldn't be repeated.
                        if (IsPathSeparator(path[n]))
                        {
                            n++; 
                            // third, following something characters. its share name.
                            if (!IsPathSeparator(path[n]))
                            {
                                if (path[n] == '.')
                                {
                                    break;
                                }

                                while (n < l)
                                {
                                    if (IsPathSeparator(path[n]))
                                    {
                                        break;
                                    n++;
                                    }

                                }

                                return path[..n];

                            }

                            break;

                        }

                    }


                }

            }

            return "";

        }

        private static @string fromSlash(@string path)
        { 
            // Replace each '/' with '\\' if present
            slice<byte> pathbuf = default;
            long lastSlash = default;
            foreach (var (i, b) in path)
            {
                if (b == '/')
                {
                    if (pathbuf == null)
                    {
                        pathbuf = make_slice<byte>(len(path));
                    }

                    copy(pathbuf[lastSlash..], path[lastSlash..i]);
                    pathbuf[i] = '\\';
                    lastSlash = i + 1L;

                }

            }
            if (pathbuf == null)
            {
                return path;
            }

            copy(pathbuf[lastSlash..], path[lastSlash..]);
            return string(pathbuf);

        }

        private static @string dirname(@string path)
        {
            var vol = volumeName(path);
            var i = len(path) - 1L;
            while (i >= len(vol) && !IsPathSeparator(path[i]))
            {
                i--;
            }

            var dir = path[len(vol)..i + 1L];
            var last = len(dir) - 1L;
            if (last > 0L && IsPathSeparator(dir[last]))
            {
                dir = dir[..last];
            }

            if (dir == "")
            {
                dir = ".";
            }

            return vol + dir;

        }

        // fixLongPath returns the extended-length (\\?\-prefixed) form of
        // path when needed, in order to avoid the default 260 character file
        // path limit imposed by Windows. If path is not easily converted to
        // the extended-length form (for example, if path is a relative path
        // or contains .. elements), or is short enough, fixLongPath returns
        // path unmodified.
        //
        // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx#maxpath
        private static @string fixLongPath(@string path)
        { 
            // Do nothing (and don't allocate) if the path is "short".
            // Empirically (at least on the Windows Server 2013 builder),
            // the kernel is arbitrarily okay with < 248 bytes. That
            // matches what the docs above say:
            // "When using an API to create a directory, the specified
            // path cannot be so long that you cannot append an 8.3 file
            // name (that is, the directory name cannot exceed MAX_PATH
            // minus 12)." Since MAX_PATH is 260, 260 - 12 = 248.
            //
            // The MSDN docs appear to say that a normal path that is 248 bytes long
            // will work; empirically the path must be less then 248 bytes long.
            if (len(path) < 248L)
            { 
                // Don't fix. (This is how Go 1.7 and earlier worked,
                // not automatically generating the \\?\ form)
                return path;

            } 

            // The extended form begins with \\?\, as in
            // \\?\c:\windows\foo.txt or \\?\UNC\server\share\foo.txt.
            // The extended form disables evaluation of . and .. path
            // elements and disables the interpretation of / as equivalent
            // to \. The conversion here rewrites / to \ and elides
            // . elements as well as trailing or duplicate separators. For
            // simplicity it avoids the conversion entirely for relative
            // paths or paths containing .. elements. For now,
            // \\server\share paths are not converted to
            // \\?\UNC\server\share paths because the rules for doing so
            // are less well-specified.
            if (len(path) >= 2L && path[..2L] == "\\\\")
            { 
                // Don't canonicalize UNC paths.
                return path;

            }

            if (!isAbs(path))
            { 
                // Relative path
                return path;

            }

            const @string prefix = (@string)"\\\\?";



            var pathbuf = make_slice<byte>(len(prefix) + len(path) + len("\\"));
            copy(pathbuf, prefix);
            var n = len(path);
            long r = 0L;
            var w = len(prefix);
            while (r < n)
            {

                if (IsPathSeparator(path[r])) 
                    // empty block
                    r++;
                else if (path[r] == '.' && (r + 1L == n || IsPathSeparator(path[r + 1L]))) 
                    // /./
                    r++;
                else if (r + 1L < n && path[r] == '.' && path[r + 1L] == '.' && (r + 2L == n || IsPathSeparator(path[r + 2L]))) 
                    // /../ is currently unhandled
                    return path;
                else 
                    pathbuf[w] = '\\';
                    w++;
                    while (r < n && !IsPathSeparator(path[r]))
                    {
                        pathbuf[w] = path[r];
                        w++;
                        r++;
                    }
                
            } 
            // A drive's root directory needs a trailing \
 
            // A drive's root directory needs a trailing \
            if (w == len("\\\\?\\c:"))
            {
                pathbuf[w] = '\\';
                w++;
            }

            return string(pathbuf[..w]);

        }

        // fixRootDirectory fixes a reference to a drive's root directory to
        // have the required trailing slash.
        private static @string fixRootDirectory(@string p)
        {
            if (len(p) == len("\\\\?\\c:"))
            {
                if (IsPathSeparator(p[0L]) && IsPathSeparator(p[1L]) && p[2L] == '?' && IsPathSeparator(p[3L]) && p[5L] == ':')
                {
                    return p + "\\";
                }

            }

            return p;

        }
    }
}
