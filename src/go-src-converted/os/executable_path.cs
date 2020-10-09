// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix openbsd

// package os -- go2cs converted at 2020 October 09 05:07:04 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_path.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // We query the working directory at init, to use it later to search for the
        // executable file
        // errWd will be checked later, if we need to use initWd


        private static (@string, error) executable()
        {
            @string _p0 = default;
            error _p0 = default!;

            @string exePath = default;
            if (len(Args) == 0L || Args[0L] == "")
            {
                return ("", error.As(ErrNotExist)!);
            }

            if (IsPathSeparator(Args[0L][0L]))
            { 
                // Args[0] is an absolute path, so it is the executable.
                // Note that we only need to worry about Unix paths here.
                exePath = Args[0L];

            }
            else
            {
                for (long i = 1L; i < len(Args[0L]); i++)
                {
                    if (IsPathSeparator(Args[0L][i]))
                    { 
                        // Args[0] is a relative path: prepend the
                        // initial working directory.
                        if (errWd != null)
                        {
                            return ("", error.As(errWd)!);
                        }

                        exePath = initWd + string(PathSeparator) + Args[0L];
                        break;

                    }

                }


            }

            if (exePath != "")
            {
                {
                    var err = isExecutable(exePath);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                }

                return (exePath, error.As(null!)!);

            } 
            // Search for executable in $PATH.
            foreach (var (_, dir) in splitPathList(Getenv("PATH")))
            {
                if (len(dir) == 0L)
                {
                    dir = ".";
                }

                if (!IsPathSeparator(dir[0L]))
                {
                    if (errWd != null)
                    {
                        return ("", error.As(errWd)!);
                    }

                    dir = initWd + string(PathSeparator) + dir;

                }

                exePath = dir + string(PathSeparator) + Args[0L];

                if (isExecutable(exePath) == null) 
                    return (exePath, error.As(null!)!);
                else if (isExecutable(exePath) == ErrPermission) 
                    return ("", error.As(ErrPermission)!);
                
            }
            return ("", error.As(ErrNotExist)!);

        }

        // isExecutable returns an error if a given file is not an executable.
        private static error isExecutable(@string path)
        {
            var (stat, err) = Stat(path);
            if (err != null)
            {
                return error.As(err)!;
            }

            var mode = stat.Mode();
            if (!mode.IsRegular())
            {
                return error.As(ErrPermission)!;
            }

            if ((mode & 0111L) == 0L)
            {
                return error.As(ErrPermission)!;
            }

            return error.As(null!)!;

        }

        // splitPathList splits a path list.
        // This is based on genSplit from strings/strings.go
        private static slice<@string> splitPathList(@string pathList)
        {
            if (pathList == "")
            {
                return null;
            }

            long n = 1L;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(pathList); i++)
                {
                    if (pathList[i] == PathListSeparator)
                    {
                        n++;
                    }

                }


                i = i__prev1;
            }
            long start = 0L;
            var a = make_slice<@string>(n);
            long na = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i + 1L <= len(pathList) && na + 1L < n; i++)
                {
                    if (pathList[i] == PathListSeparator)
                    {
                        a[na] = pathList[start..i];
                        na++;
                        start = i + 1L;
                    }

                }


                i = i__prev1;
            }
            a[na] = pathList[start..];
            return a[..na + 1L];

        }
    }
}
