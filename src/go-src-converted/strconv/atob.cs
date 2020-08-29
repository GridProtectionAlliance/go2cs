// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2020 August 29 08:16:04 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\atob.go

using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        // ParseBool returns the boolean value represented by the string.
        // It accepts 1, t, T, TRUE, true, True, 0, f, F, FALSE, false, False.
        // Any other value returns an error.
        public static (bool, error) ParseBool(@string str)
        {
            switch (str)
            {
                case "1": 

                case "t": 

                case "T": 

                case "true": 

                case "TRUE": 

                case "True": 
                    return (true, null);
                    break;
                case "0": 

                case "f": 

                case "F": 

                case "false": 

                case "FALSE": 

                case "False": 
                    return (false, null);
                    break;
            }
            return (false, syntaxError("ParseBool", str));
        }

        // FormatBool returns "true" or "false" according to the value of b
        public static @string FormatBool(bool b)
        {
            if (b)
            {
                return "true";
            }
            return "false";
        }

        // AppendBool appends "true" or "false", according to the value of b,
        // to dst and returns the extended buffer.
        public static slice<byte> AppendBool(slice<byte> dst, bool b)
        {
            if (b)
            {
                return append(dst, "true");
            }
            return append(dst, "false");
        }
    }
}
