// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:36 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\error_plan9.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static bool isExist(error err)
        {
            return checkErrMessageContent(err, "exists", "is a directory");
        }

        private static bool isNotExist(error err)
        {
            return checkErrMessageContent(err, "does not exist", "not found", "has been removed", "no parent");
        }

        private static bool isPermission(error err)
        {
            return checkErrMessageContent(err, "permission denied");
        }

        // checkErrMessageContent checks if err message contains one of msgs.
        private static bool checkErrMessageContent(error err, params @string[] msgs)
        {
            msgs = msgs.Clone();

            if (err == null)
            {
                return false;
            }
            err = underlyingError(err);
            foreach (var (_, msg) in msgs)
            {
                if (contains(err.Error(), msg))
                {
                    return true;
                }
            }
            return false;
        }

        // contains is a local version of strings.Contains. It knows len(sep) > 1.
        private static bool contains(@string s, @string sep)
        {
            var n = len(sep);
            var c = sep[0L];
            for (long i = 0L; i + n <= len(s); i++)
            {
                if (s[i] == c && s[i..i + n] == sep)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
