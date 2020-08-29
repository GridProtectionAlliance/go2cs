// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2020 August 29 08:35:55 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\tags.go
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        // tagOptions is the string following a comma in a struct field's "json"
        // tag, or the empty string. It does not include the leading comma.
        private partial struct tagOptions // : @string
        {
        }

        // parseTag splits a struct field's json tag into its name and
        // comma-separated options.
        private static (@string, tagOptions) parseTag(@string tag)
        {
            {
                var idx = strings.Index(tag, ",");

                if (idx != -1L)
                {
                    return (tag[..idx], tagOptions(tag[idx + 1L..]));
                }

            }
            return (tag, tagOptions(""));
        }

        // Contains reports whether a comma-separated list of options
        // contains a particular substr flag. substr must be surrounded by a
        // string boundary or commas.
        private static bool Contains(this tagOptions o, @string optionName)
        {
            if (len(o) == 0L)
            {
                return false;
            }
            var s = string(o);
            while (s != "")
            {
                @string next = default;
                var i = strings.Index(s, ",");
                if (i >= 0L)
                {
                    s = s[..i];
                    next = s[i + 1L..];
                }
                if (s == optionName)
                {
                    return true;
                }
                s = next;
            }

            return false;
        }
    }
}}
