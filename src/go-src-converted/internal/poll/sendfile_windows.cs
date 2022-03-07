// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:13:19 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\sendfile_windows.go
using io = go.io_package;
using syscall = go.syscall_package;
using System;


namespace go.@internal;

public static partial class poll_package {

    // SendFile wraps the TransmitFile call.
public static (long, error) SendFile(ptr<FD> _addr_fd, syscall.Handle src, long n) => func((defer, _, _) => {
    long written = default;
    error err = default!;
    ref FD fd = ref _addr_fd.val;

    if (fd.kind == kindPipe) { 
        // TransmitFile does not work with pipes
        return (0, error.As(syscall.ESPIPE)!);

    }
    {
        var err = fd.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    defer(fd.writeUnlock());

    var o = _addr_fd.wop;
    o.handle = src; 

    // TODO(brainman): skip calling syscall.Seek if OS allows it
    var (curpos, err) = syscall.Seek(o.handle, 0, io.SeekCurrent);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (n <= 0) { // We don't know the size of the file so infer it.
        // Find the number of bytes offset from curpos until the end of the file.
        n, err = syscall.Seek(o.handle, -curpos, io.SeekEnd);
        if (err != null) {
            return ;
        }
        _, err = syscall.Seek(o.handle, curpos, io.SeekStart);

        if (err != null) {
            return ;
        }
    }
    const var maxChunkSizePerCall = int64(0x7fffffff - 1);



    while (n > 0) {
        var chunkSize = maxChunkSizePerCall;
        if (chunkSize > n) {
            chunkSize = n;
        }
        o.qty = uint32(chunkSize);
        o.o.Offset = uint32(curpos);
        o.o.OffsetHigh = uint32(curpos >> 32);

        var (nw, err) = execIO(o, o => {
            return syscall.TransmitFile(o.fd.Sysfd, o.handle, o.qty, 0, _addr_o.o, null, syscall.TF_WRITE_BEHIND);
        });
        if (err != null) {
            return (written, error.As(err)!);
        }
        curpos += int64(nw); 

        // Some versions of Windows (Windows 10 1803) do not set
        // file position after TransmitFile completes.
        // So just use Seek to set file position.
        _, err = syscall.Seek(o.handle, curpos, io.SeekStart);

        if (err != null) {
            return (written, error.As(err)!);
        }
        n -= int64(nw);
        written += int64(nw);

    }

    return ;

});

} // end poll_package
