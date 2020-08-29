// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package str -- go2cs converted at 2020 August 29 10:00:48 UTC
// import "cmd/go/internal/str" ==> using str = go.cmd.go.@internal.str_package
// Original source: C:\Go\src\cmd\go\internal\str\path.go
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
        // HasFilePathPrefix reports whether the filesystem path s begins with the
        // elements in prefix.
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
            else if (len(s) > len(prefix)) 
                if (prefix != "" && prefix[len(prefix) - 1L] == filepath.Separator)
                {
                    return strings.HasPrefix(s, prefix);
                }
                return s[len(prefix)] == filepath.Separator && s[..len(prefix)] == prefix;
            else 
                return false;
                    }
    }
}}}}
