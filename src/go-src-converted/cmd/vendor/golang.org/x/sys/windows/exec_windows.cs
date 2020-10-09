// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fork, exec, wait, etc.

// package windows -- go2cs converted at 2020 October 09 06:00:50 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\exec_windows.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        // EscapeArg rewrites command line argument s as prescribed
        // in http://msdn.microsoft.com/en-us/library/ms880421.
        // This function returns "" (2 double quotes) if s is empty.
        // Alternatively, these transformations are done:
        // - every back slash (\) is doubled, but only if immediately
        //   followed by double quote (");
        // - every double quote (") is escaped by back slash (\);
        // - finally, s is wrapped with double quotes (arg -> "arg"),
        //   but only if there is space or tab inside s.
        public static @string EscapeArg(@string s)
        {
            if (len(s) == 0L)
            {
                return "\"\"";
            }
            var n = len(s);
            var hasSpace = false;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    switch (s[i])
                    {
                        case '"': 

                        case '\\': 
                            n++;
                            break;
                        case ' ': 

                        case '\t': 
                            hasSpace = true;
                            break;
                    }

                }

                i = i__prev1;
            }
            if (hasSpace)
            {
                n += 2L;
            }
            if (n == len(s))
            {
                return s;
            }
            var qs = make_slice<byte>(n);
            long j = 0L;
            if (hasSpace)
            {
                qs[j] = '"';
                j++;
            }
            long slashes = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < len(s); i++)
                {
                    switch (s[i])
                    {
                        case '\\': 
                            slashes++;
                            qs[j] = s[i];
                            break;
                        case '"': 
                            while (slashes > 0L)
                            {
                                qs[j] = '\\';
                                j++;
                                slashes--;
                            }
                            qs[j] = '\\';
                            j++;
                            qs[j] = s[i];
                            break;
                        default: 
                            slashes = 0L;
                            qs[j] = s[i];
                            break;
                    }
                    j++;

                }

                i = i__prev1;
            }
            if (hasSpace)
            {
                while (slashes > 0L)
                {
                    qs[j] = '\\';
                    j++;
                    slashes--;
                }
                qs[j] = '"';
                j++;

            }
            return string(qs[..j]);

        }

        public static void CloseOnExec(Handle fd)
        {
            SetHandleInformation(Handle(fd), HANDLE_FLAG_INHERIT, 0L);
        }

        // FullPath retrieves the full path of the specified file.
        public static (@string, error) FullPath(@string name)
        {
            @string path = default;
            error err = default!;

            var (p, err) = UTF16PtrFromString(name);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var n = uint32(100L);
            while (true)
            {
                var buf = make_slice<ushort>(n);
                n, err = GetFullPathName(p, uint32(len(buf)), _addr_buf[0L], null);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                if (n <= uint32(len(buf)))
                {
                    return (UTF16ToString(buf[..n]), error.As(null!)!);
                }

            }


        }
    }
}}}}}}
