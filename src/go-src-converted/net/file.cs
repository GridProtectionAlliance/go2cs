// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:15:46 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\file.go
using os = go.os_package;

namespace go;

public static partial class net_package {

    // BUG(mikio): On JS and Windows, the FileConn, FileListener and
    // FilePacketConn functions are not implemented.
private partial struct fileAddr { // : @string
}

private static @string Network(this fileAddr _p0) {
    return "file+net";
}
private static @string String(this fileAddr f) {
    return string(f);
}

// FileConn returns a copy of the network connection corresponding to
// the open file f.
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
public static (Conn, error) FileConn(ptr<os.File> _addr_f) {
    Conn c = default;
    error err = default!;
    ref os.File f = ref _addr_f.val;

    c, err = fileConn(f);
    if (err != null) {
        err = addr(new OpError(Op:"file",Net:"file+net",Source:nil,Addr:fileAddr(f.Name()),Err:err));
    }
    return ;

}

// FileListener returns a copy of the network listener corresponding
// to the open file f.
// It is the caller's responsibility to close ln when finished.
// Closing ln does not affect f, and closing f does not affect ln.
public static (Listener, error) FileListener(ptr<os.File> _addr_f) {
    Listener ln = default;
    error err = default!;
    ref os.File f = ref _addr_f.val;

    ln, err = fileListener(f);
    if (err != null) {
        err = addr(new OpError(Op:"file",Net:"file+net",Source:nil,Addr:fileAddr(f.Name()),Err:err));
    }
    return ;

}

// FilePacketConn returns a copy of the packet network connection
// corresponding to the open file f.
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
public static (PacketConn, error) FilePacketConn(ptr<os.File> _addr_f) {
    PacketConn c = default;
    error err = default!;
    ref os.File f = ref _addr_f.val;

    c, err = filePacketConn(f);
    if (err != null) {
        err = addr(new OpError(Op:"file",Net:"file+net",Source:nil,Addr:fileAddr(f.Name()),Err:err));
    }
    return ;

}

} // end net_package
