// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2022 March 13 06:47:05 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\norm\input.go
namespace go.vendor.golang.org.x.text.unicode;

using utf8 = unicode.utf8_package;

public static partial class norm_package {

private partial struct input {
    public @string str;
    public slice<byte> bytes;
}

private static input inputBytes(slice<byte> str) {
    return new input(bytes:str);
}

private static input inputString(@string str) {
    return new input(str:str);
}

private static void setBytes(this ptr<input> _addr_@in, slice<byte> str) {
    ref input @in = ref _addr_@in.val;

    @in.str = "";
    @in.bytes = str;
}

private static void setString(this ptr<input> _addr_@in, @string str) {
    ref input @in = ref _addr_@in.val;

    @in.str = str;
    @in.bytes = null;
}

private static byte _byte(this ptr<input> _addr_@in, nint p) {
    ref input @in = ref _addr_@in.val;

    if (@in.bytes == null) {
        return @in.str[p];
    }
    return @in.bytes[p];
}

private static nint skipASCII(this ptr<input> _addr_@in, nint p, nint max) {
    ref input @in = ref _addr_@in.val;

    if (@in.bytes == null) {
        while (p < max && @in.str[p] < utf8.RuneSelf) {
            p++;
        }
    else
    } {
        while (p < max && @in.bytes[p] < utf8.RuneSelf) {
            p++;
        }
    }
    return p;
}

private static nint skipContinuationBytes(this ptr<input> _addr_@in, nint p) {
    ref input @in = ref _addr_@in.val;

    if (@in.bytes == null) {
        while (p < len(@in.str) && !utf8.RuneStart(@in.str[p])) {
            p++;
        }
    else
    } {
        while (p < len(@in.bytes) && !utf8.RuneStart(@in.bytes[p])) {
            p++;
        }
    }
    return p;
}

private static slice<byte> appendSlice(this ptr<input> _addr_@in, slice<byte> buf, nint b, nint e) {
    ref input @in = ref _addr_@in.val;

    if (@in.bytes != null) {
        return append(buf, @in.bytes[(int)b..(int)e]);
    }
    for (var i = b; i < e; i++) {
        buf = append(buf, @in.str[i]);
    }
    return buf;
}

private static nint copySlice(this ptr<input> _addr_@in, slice<byte> buf, nint b, nint e) {
    ref input @in = ref _addr_@in.val;

    if (@in.bytes == null) {
        return copy(buf, @in.str[(int)b..(int)e]);
    }
    return copy(buf, @in.bytes[(int)b..(int)e]);
}

private static (ushort, nint) charinfoNFC(this ptr<input> _addr_@in, nint p) {
    ushort _p0 = default;
    nint _p0 = default;
    ref input @in = ref _addr_@in.val;

    if (@in.bytes == null) {
        return nfcData.lookupString(@in.str[(int)p..]);
    }
    return nfcData.lookup(@in.bytes[(int)p..]);
}

private static (ushort, nint) charinfoNFKC(this ptr<input> _addr_@in, nint p) {
    ushort _p0 = default;
    nint _p0 = default;
    ref input @in = ref _addr_@in.val;

    if (@in.bytes == null) {
        return nfkcData.lookupString(@in.str[(int)p..]);
    }
    return nfkcData.lookup(@in.bytes[(int)p..]);
}

private static int hangul(this ptr<input> _addr_@in, nint p) {
    int r = default;
    ref input @in = ref _addr_@in.val;

    nint size = default;
    if (@in.bytes == null) {
        if (!isHangulString(@in.str[(int)p..])) {
            return 0;
        }
        r, size = utf8.DecodeRuneInString(@in.str[(int)p..]);
    }
    else
 {
        if (!isHangul(@in.bytes[(int)p..])) {
            return 0;
        }
        r, size = utf8.DecodeRune(@in.bytes[(int)p..]);
    }
    if (size != hangulUTF8Size) {
        return 0;
    }
    return r;
}

} // end norm_package
