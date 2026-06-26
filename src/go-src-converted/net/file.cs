// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;

partial class net_package {

[GoType("@string")] partial struct fileAddr;

// BUG(mikio): On JS and Windows, the FileConn, FileListener and
// FilePacketConn functions are not implemented.
internal static @string Network(this fileAddr _) {
    return "file+net"u8;
}

internal static @string String(this fileAddr f) {
    return ((@string)f);
}

// FileConn returns a copy of the network connection corresponding to
// the open file f.
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
public static (Conn c, error err) FileConn(ж<os.File> Ꮡf) {
    Conn c = default!;
    error err = default!;

    ref var f = ref Ꮡf.val;
    (c, err) = fileConn(Ꮡf);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "file"u8, Net: "file+net"u8, Source: default!, ΔAddr: ((fileAddr)f.Name()), Err: err); err = ref Ꮡerr.val;
    }
    return (c, err);
}

// FileListener returns a copy of the network listener corresponding
// to the open file f.
// It is the caller's responsibility to close ln when finished.
// Closing ln does not affect f, and closing f does not affect ln.
public static (Listener ln, error err) FileListener(ж<os.File> Ꮡf) {
    Listener ln = default!;
    error err = default!;

    ref var f = ref Ꮡf.val;
    (ln, err) = fileListener(Ꮡf);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "file"u8, Net: "file+net"u8, Source: default!, ΔAddr: ((fileAddr)f.Name()), Err: err); err = ref Ꮡerr.val;
    }
    return (ln, err);
}

// FilePacketConn returns a copy of the packet network connection
// corresponding to the open file f.
// It is the caller's responsibility to close f when finished.
// Closing c does not affect f, and closing f does not affect c.
public static (PacketConn c, error err) FilePacketConn(ж<os.File> Ꮡf) {
    PacketConn c = default!;
    error err = default!;

    ref var f = ref Ꮡf.val;
    (c, err) = filePacketConn(Ꮡf);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "file"u8, Net: "file+net"u8, Source: default!, ΔAddr: ((fileAddr)f.Name()), Err: err); err = ref Ꮡerr.val;
    }
    return (c, err);
}

} // end net_package
