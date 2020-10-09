// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package str -- go2cs converted at 2020 October 09 05:45:28 UTC
// import "cmd/go/internal/str" ==> using str = go.cmd.go.@internal.str_package
// Original source: C:\Go\src\cmd\go\internal\str\path.go
using path = go.path_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class str_package
    {
        // HasPathPrefix reports whether the slash-separated path s
        // begins with the elements in prefix.
        public static bool HasPathPrefix(@string s, @string prefix)
        {
            if (len(s) == len(prefix))
            {
                return s == prefix;
            }
            if (prefix == "")
            {
                return true;
            }
            if (len(s) > len(prefix))
            {
                if (prefix[len(prefix) - 1L] == '/' || s[len(prefix)] == '/')
                {
                    return s[..len(prefix)] == prefix;
                }
            }
            return false;

        }

        // HasFilePathPrefix reports whether the filesystem path s
        // begins with the elements in prefix.
        public static bool HasFilePathPrefix(@string s, @string prefix)
        {
            var sv = strings.ToUpper(filepath.VolumeName(s));
            var pv = strings.ToUpper(filepath.VolumeName(prefix));
            s = s[len(sv)..];
            prefix = prefix[len(pv)..];

            if (sv != pv) 
                return false;
            else if (len(s) == len(prefix)) 
                return s == prefix;
            else if (prefix == "") 
                return true;
            else if (len(s) > len(prefix)) 
                if (prefix[len(prefix) - 1L] == filepath.Separator)
                {
                    return strings.HasPrefix(s, prefix);
                }

                return s[len(prefix)] == filepath.Separator && s[..len(prefix)] == prefix;
            else 
                return false;
            
        }

        // GlobsMatchPath reports whether any path prefix of target
        // matches one of the glob patterns (as defined by path.Match)
        // in the comma-separated globs list.
        // It ignores any empty or malformed patterns in the list.
        public static bool GlobsMatchPath(@string globs, @string target)
        {
            while (globs != "")
            { 
                // Extract next non-empty glob in comma-separated list.
                @string glob = default;
                {
                    var i__prev1 = i;

                    var i = strings.Index(globs, ",");

                    if (i >= 0L)
                    {
                        glob = globs[..i];
                        globs = globs[i + 1L..];

                    }
                    else
                    {
                        glob = globs;
                        globs = "";

                    }

                    i = i__prev1;

                }

                if (glob == "")
                {
                    continue;
                } 

                // A glob with N+1 path elements (N slashes) needs to be matched
                // against the first N+1 path elements of target,
                // which end just before the N+1'th slash.
                var n = strings.Count(glob, "/");
                var prefix = target; 
                // Walk target, counting slashes, truncating at the N+1'th slash.
                {
                    var i__prev2 = i;

                    for (i = 0L; i < len(target); i++)
                    {
                        if (target[i] == '/')
                        {
                            if (n == 0L)
                            {
                                prefix = target[..i];
                                break;
                            }

                            n--;

                        }

                    }


                    i = i__prev2;
                }
                if (n > 0L)
                { 
                    // Not enough prefix elements.
                    continue;

                }

                var (matched, _) = path.Match(glob, prefix);
                if (matched)
                {
                    return true;
                }

            }

            return false;

        }
    }
}}}}
