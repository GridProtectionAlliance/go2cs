// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:44:10 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\path_unix.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        public static readonly char PathSeparator = '/'; // OS-specific path separator
        public static readonly char PathListSeparator = ':'; // OS-specific path list separator

        // IsPathSeparator reports whether c is a directory separator character.
        public static bool IsPathSeparator(byte c)
        {
            return PathSeparator == c;
        }

        // basename removes trailing slashes and the leading directory name from path name
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
    }
}
