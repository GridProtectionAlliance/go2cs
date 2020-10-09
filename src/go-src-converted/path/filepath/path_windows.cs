// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2020 October 09 04:49:47 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\path_windows.go
using strings = go.strings_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        private static bool isSlash(byte c)
        {
            return c == '\\' || c == '/';
        }

        // reservedNames lists reserved Windows names. Search for PRN in
        // https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file
        // for details.
        private static @string reservedNames = new slice<@string>(new @string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" });

        // isReservedName returns true, if path is Windows reserved name.
        // See reservedNames for the full list.
        private static bool isReservedName(@string path)
        {
            if (len(path) == 0L)
            {
                return false;
            }

            foreach (var (_, reserved) in reservedNames)
            {
                if (strings.EqualFold(path, reserved))
                {
                    return true;
                }

            }
            return false;

        }

        // IsAbs reports whether the path is absolute.
        public static bool IsAbs(@string path)
        {
            bool b = default;

            if (isReservedName(path))
            {
                return true;
            }

            var l = volumeNameLen(path);
            if (l == 0L)
            {
                return false;
            }

            path = path[l..];
            if (path == "")
            {
                return false;
            }

            return isSlash(path[0L]);

        }

        // volumeNameLen returns length of the leading volume name on Windows.
        // It returns 0 elsewhere.
        private static long volumeNameLen(@string path)
        {
            if (len(path) < 2L)
            {
                return 0L;
            } 
            // with drive letter
            var c = path[0L];
            if (path[1L] == ':' && ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z'))
            {
                return 2L;
            } 
            // is it UNC? https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
            {
                var l = len(path);

                if (l >= 5L && isSlash(path[0L]) && isSlash(path[1L]) && !isSlash(path[2L]) && path[2L] != '.')
                { 
                    // first, leading `\\` and next shouldn't be `\`. its server name.
                    for (long n = 3L; n < l - 1L; n++)
                    { 
                        // second, next '\' shouldn't be repeated.
                        if (isSlash(path[n]))
                        {
                            n++; 
                            // third, following something characters. its share name.
                            if (!isSlash(path[n]))
                            {
                                if (path[n] == '.')
                                {
                                    break;
                                }

                                while (n < l)
                                {
                                    if (isSlash(path[n]))
                                    {
                                        break;
                                    n++;
                                    }

                                }

                                return n;

                            }

                            break;

                        }

                    }


                }

            }

            return 0L;

        }

        // HasPrefix exists for historical compatibility and should not be used.
        //
        // Deprecated: HasPrefix does not respect path boundaries and
        // does not ignore case when required.
        public static bool HasPrefix(@string p, @string prefix)
        {
            if (strings.HasPrefix(p, prefix))
            {
                return true;
            }

            return strings.HasPrefix(strings.ToLower(p), strings.ToLower(prefix));

        }

        private static slice<@string> splitList(@string path)
        { 
            // The same implementation is used in LookPath in os/exec;
            // consider changing os/exec when changing this.

            if (path == "")
            {
                return new slice<@string>(new @string[] {  });
            } 

            // Split path, respecting but preserving quotes.
            @string list = new slice<@string>(new @string[] {  });
            long start = 0L;
            var quo = false;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(path); i++)
                {
                    {
                        var c = path[i];


                        if (c == '"') 
                            quo = !quo;
                        else if (c == ListSeparator && !quo) 
                            list = append(list, path[start..i]);
                            start = i + 1L;

                    }

                }


                i = i__prev1;
            }
            list = append(list, path[start..]); 

            // Remove quotes.
            {
                long i__prev1 = i;

                foreach (var (__i, __s) in list)
                {
                    i = __i;
                    s = __s;
                    list[i] = strings.ReplaceAll(s, "\"", "");
                }

                i = i__prev1;
            }

            return list;

        }

        private static (@string, error) abs(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (path == "")
            { 
                // syscall.FullPath returns an error on empty path, because it's not a valid path.
                // To implement Abs behavior of returning working directory on empty string input,
                // special-case empty path by changing it to "." path. See golang.org/issue/24441.
                path = ".";

            }

            var (fullPath, err) = syscall.FullPath(path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (Clean(fullPath), error.As(null!)!);

        }

        private static @string join(slice<@string> elem)
        {
            foreach (var (i, e) in elem)
            {
                if (e != "")
                {
                    return joinNonEmpty(elem[i..]);
                }

            }
            return "";

        }

        // joinNonEmpty is like join, but it assumes that the first element is non-empty.
        private static @string joinNonEmpty(slice<@string> elem)
        {
            if (len(elem[0L]) == 2L && elem[0L][1L] == ':')
            { 
                // First element is drive letter without terminating slash.
                // Keep path relative to current directory on that drive.
                // Skip empty elements.
                long i = 1L;
                while (i < len(elem))
                {
                    if (elem[i] != "")
                    {
                        break;
                    i++;
                    }

                }

                return Clean(elem[0L] + strings.Join(elem[i..], string(Separator)));

            } 
            // The following logic prevents Join from inadvertently creating a
            // UNC path on Windows. Unless the first element is a UNC path, Join
            // shouldn't create a UNC path. See golang.org/issue/9167.
            var p = Clean(strings.Join(elem, string(Separator)));
            if (!isUNC(p))
            {
                return p;
            } 
            // p == UNC only allowed when the first element is a UNC path.
            var head = Clean(elem[0L]);
            if (isUNC(head))
            {
                return p;
            } 
            // head + tail == UNC, but joining two non-UNC paths should not result
            // in a UNC path. Undo creation of UNC path.
            var tail = Clean(strings.Join(elem[1L..], string(Separator)));
            if (head[len(head) - 1L] == Separator)
            {
                return head + tail;
            }

            return head + string(Separator) + tail;

        }

        // isUNC reports whether path is a UNC path.
        private static bool isUNC(@string path)
        {
            return volumeNameLen(path) > 2L;
        }

        private static bool sameWord(@string a, @string b)
        {
            return strings.EqualFold(a, b);
        }
    }
}}
