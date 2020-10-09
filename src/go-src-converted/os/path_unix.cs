// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 09 05:07:17 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\path_unix.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        public static readonly char PathSeparator = (char)'/'; // OS-specific path separator
        public static readonly char PathListSeparator = (char)':'; // OS-specific path list separator

        // IsPathSeparator reports whether c is a directory separator character.
        public static bool IsPathSeparator(byte c)
        {
            return PathSeparator == c;
        }

        // basename removes trailing slashes and the leading directory name from path name.
        private static @string basename(@string name)
        {
            var i = len(name) - 1L; 
            // Remove trailing slashes
            while (i > 0L && name[i] == '/')
            {
                name = name[..i];
                i--;
            } 
            // Remove leading directory name
 
            // Remove leading directory name
            i--;

            while (i >= 0L)
            {
                if (name[i] == '/')
                {
                    name = name[i + 1L..];
                    break;
                i--;
                }

            }


            return name;

        }

        // splitPath returns the base name and parent directory.
        private static (@string, @string) splitPath(@string path)
        {
            @string _p0 = default;
            @string _p0 = default;
 
            // if no better parent is found, the path is relative from "here"
            @string dirname = "."; 

            // Remove all but one leading slash.
            while (len(path) > 1L && path[0L] == '/' && path[1L] == '/')
            {
                path = path[1L..];
            }


            var i = len(path) - 1L; 

            // Remove trailing slashes.
            while (i > 0L && path[i] == '/')
            {
                path = path[..i];
                i--;
            } 

            // if no slashes in path, base is path
 

            // if no slashes in path, base is path
            var basename = path; 

            // Remove leading directory path
            i--;

            while (i >= 0L)
            {
                if (path[i] == '/')
                {
                    if (i == 0L)
                    {
                        dirname = path[..1L];
                i--;
                    }
                    else
                    {
                        dirname = path[..i];
                    }

                    basename = path[i + 1L..];
                    break;

                }

            }


            return (dirname, basename);

        }

        private static @string fixRootDirectory(@string p)
        {
            return p;
        }
    }
}
