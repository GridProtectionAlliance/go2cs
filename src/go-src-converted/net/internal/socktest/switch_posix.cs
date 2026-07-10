// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go.net.@internal;

using fmt = fmt_package;
using syscall = syscall_package;

partial class socktest_package {

internal static @string familyString(nint family) {
    var exprᴛ1 = family;
    if (exprᴛ1 == syscall.AF_INET) {
        return "inet4"u8;
    }
    if (exprᴛ1 == syscall.AF_INET6) {
        return "inet6"u8;
    }
    if (exprᴛ1 == syscall.AF_UNIX) {
        return "local"u8;
    }
    { /* default: */
        return fmt.Sprintf("%d"u8, family);
    }

}

internal static @string typeString(nint sotype) {
    @string s = default!;
    var exprᴛ1 = (nint)(sotype & 0xff);
    if (exprᴛ1 == syscall.SOCK_STREAM) {
        s = "stream"u8;
    }
    else if (exprᴛ1 == syscall.SOCK_DGRAM) {
        s = "datagram"u8;
    }
    else if (exprᴛ1 == syscall.SOCK_RAW) {
        s = "raw"u8;
    }
    else if (exprᴛ1 == syscall.SOCK_SEQPACKET) {
        s = "seqpacket"u8;
    }
    else { /* default: */
        s = fmt.Sprintf("%d"u8, (nint)(sotype & 0xff));
    }

    {
        nuint flags = (nuint)((nuint)sotype & (nuint)~(nuint)0xff); if (flags != 0) {
            s += fmt.Sprintf("|%#x"u8, flags);
        }
    }
    return s;
}

internal static @string protocolString(nint proto) {
    var exprᴛ1 = proto;
    if (exprᴛ1 is 0) {
        return "default"u8;
    }
    if (exprᴛ1 == syscall.IPPROTO_TCP) {
        return "tcp"u8;
    }
    if (exprᴛ1 == syscall.IPPROTO_UDP) {
        return "udp"u8;
    }
    { /* default: */
        return fmt.Sprintf("%d"u8, proto);
    }

}

} // end socktest_package
