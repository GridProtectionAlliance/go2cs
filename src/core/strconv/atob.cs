// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class strconv_package {

// ParseBool returns the boolean value represented by the string.
// It accepts 1, t, T, TRUE, true, True, 0, f, F, FALSE, false, False.
// Any other value returns an error.
public static (bool, error) ParseBool(@string str) {
    var exprᴛ1 = str;
    if (exprᴛ1 == "1"u8 || exprᴛ1 == "t"u8 || exprᴛ1 == "T"u8 || exprᴛ1 == "true"u8 || exprᴛ1 == "TRUE"u8 || exprᴛ1 == "True"u8) {
        return (true, default!);
    }
    if (exprᴛ1 == "0"u8 || exprᴛ1 == "f"u8 || exprᴛ1 == "F"u8 || exprᴛ1 == "false"u8 || exprᴛ1 == "FALSE"u8 || exprᴛ1 == "False"u8) {
        return (false, default!);
    }

    return (false, ~syntaxError("ParseBool"u8, str));
}

// FormatBool returns "true" or "false" according to the value of b.
public static @string FormatBool(bool b) {
    if (b) {
        return "true"u8;
    }
    return "false"u8;
}

// AppendBool appends "true" or "false", according to the value of b,
// to dst and returns the extended buffer.
public static slice<byte> AppendBool(slice<byte> dst, bool b) {
    if (b) {
        return append(dst, "true"u8.ꓸꓸꓸ);
    }
    return append(dst, "false"u8.ꓸꓸꓸ);
}

} // end strconv_package
