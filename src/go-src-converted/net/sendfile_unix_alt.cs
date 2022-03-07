// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || solaris
// +build dragonfly freebsd solaris

// package net -- go2cs converted at 2022 March 06 22:16:32 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sendfile_unix_alt.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using System;


namespace go;

public static partial class net_package {

    // sendFile copies the contents of r to c using the sendfile
    // system call to minimize copies.
    //
    // if handled == true, sendFile returns the number of bytes copied and any
    // non-EOF error.
    //
    // if handled == false, sendFile performed no work.
private static (long, error, bool) sendFile(ptr<netFD> _addr_c, io.Reader r) {
    long written = default;
    error err = default!;
    bool handled = default;
    ref netFD c = ref _addr_c.val;
 
    // FreeBSD, DragonFly and Solaris use 0 as the "until EOF" value.
    // If you pass in more bytes than the file contains, it will
    // loop back to the beginning ad nauseam until it's sent
    // exactly the number of bytes told to. As such, we need to
    // know exactly how many bytes to send.
    long remain = 0;

    ptr<io.LimitedReader> (lr, ok) = r._<ptr<io.LimitedReader>>();
    if (ok) {
        (remain, r) = (lr.N, lr.R);        if (remain <= 0) {
            return (0, error.As(null!)!, true);
        }
    }
    ptr<os.File> (f, ok) = r._<ptr<os.File>>();
    if (!ok) {
        return (0, error.As(null!)!, false);
    }
    if (remain == 0) {
        var (fi, err) = f.Stat();
        if (err != null) {
            return (0, error.As(err)!, false);
        }
        remain = fi.Size();

    }
    var (pos, err) = f.Seek(0, io.SeekCurrent);
    if (err != null) {
        return (0, error.As(err)!, false);
    }
    var (sc, err) = f.SyscallConn();
    if (err != null) {
        return (0, error.As(null!)!, false);
    }
    error werr = default!;
    err = sc.Read(fd => {
        written, werr = poll.SendFile(_addr_c.pfd, int(fd), pos, remain);
        return true;
    });
    if (err == null) {
        err = werr;
    }
    if (lr != null) {
        lr.N = remain - written;
    }
    var (_, err1) = f.Seek(written, io.SeekCurrent);
    if (err1 != null && err == null) {
        return (written, error.As(err1)!, written > 0);
    }
    return (written, error.As(wrapSyscallError("sendfile", err))!, written > 0);

}

} // end net_package
