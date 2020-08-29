// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:08:49 UTC
// Original source: C:\Go\src\cmd\vet\buildtag.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static slice<byte> nl = (slice<byte>)"\n";        private static slice<byte> slashSlash = (slice<byte>)"//";        private static slice<byte> plusBuild = (slice<byte>)"+build";

        // checkBuildTag checks that build tags are in the correct location and well-formed.
        private static void checkBuildTag(@string name, slice<byte> data)
        {
            if (!vet("buildtags"))
            {
                return;
            }
            var lines = bytes.SplitAfter(data, nl); 

            // Determine cutpoint where +build comments are no longer valid.
            // They are valid in leading // comments in the file followed by
            // a blank line.
            long cutoff = default;
            {
                var i__prev1 = i;
                var line__prev1 = line;

                foreach (var (__i, __line) in lines)
                {
                    i = __i;
                    line = __line;
                    line = bytes.TrimSpace(line);
                    if (len(line) == 0L)
                    {
                        cutoff = i;
                        continue;
                    }
                    if (bytes.HasPrefix(line, slashSlash))
                    {
                        continue;
                    }
                    break;
                }

                i = i__prev1;
                line = line__prev1;
            }

            {
                var i__prev1 = i;
                var line__prev1 = line;

                foreach (var (__i, __line) in lines)
                {
                    i = __i;
                    line = __line;
                    line = bytes.TrimSpace(line);
                    if (!bytes.HasPrefix(line, slashSlash))
                    {
                        continue;
                    }
                    var text = bytes.TrimSpace(line[2L..]);
                    if (bytes.HasPrefix(text, plusBuild))
                    {
                        var fields = bytes.Fields(text);
                        if (!bytes.Equal(fields[0L], plusBuild))
                        { 
                            // Comment is something like +buildasdf not +build.
                            fmt.Fprintf(os.Stderr, "%s:%d: possible malformed +build comment\n", name, i + 1L);
                            setExit(1L);
                            continue;
                        }
                        if (i >= cutoff)
                        {
                            fmt.Fprintf(os.Stderr, "%s:%d: +build comment must appear before package clause and be followed by a blank line\n", name, i + 1L);
                            setExit(1L);
                            continue;
                        } 
                        // Check arguments.
Args:
                        foreach (var (_, arg) in fields[1L..])
                        {
                            foreach (var (_, elem) in strings.Split(string(arg), ","))
                            {
                                if (strings.HasPrefix(elem, "!!"))
                                {
                                    fmt.Fprintf(os.Stderr, "%s:%d: invalid double negative in build constraint: %s\n", name, i + 1L, arg);
                                    setExit(1L);
                                    _breakArgs = true;
                                    break;
                                }
                                elem = strings.TrimPrefix(elem, "!");
                                foreach (var (_, c) in elem)
                                {
                                    if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.')
                                    {
                                        fmt.Fprintf(os.Stderr, "%s:%d: invalid non-alphanumeric build constraint: %s\n", name, i + 1L, arg);
                                        setExit(1L);
                                        _breakArgs = true;
                                        break;
                                    }
                                }
                            }
                        }
                        continue;
                    } 
                    // Comment with +build but not at beginning.
                    if (bytes.Contains(line, plusBuild) && i < cutoff)
                    {
                        fmt.Fprintf(os.Stderr, "%s:%d: possible malformed +build comment\n", name, i + 1L);
                        setExit(1L);
                        continue;
                    }
                }

                i = i__prev1;
                line = line__prev1;
            }

        }
    }
}
