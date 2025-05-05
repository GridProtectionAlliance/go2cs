// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using io = io_package;
using syscall = syscall_package;

partial class poll_package {

// SendFile wraps the TransmitFile call.
public static (int64 written, error err) SendFile(ж<FD> Ꮡfd, syscallꓸHandle src, int64 n) => func((defer, _) => {
    int64 written = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.val;
    var errʗ1 = err;
    defer(() => {
        TestHookDidSendFile(Ꮡfd, 0, written, errʗ1, written > 0);
    });
    if (fd.kind == kindPipe) {
        // TransmitFile does not work with pipes
        return (0, syscall.ESPIPE);
    }
    {
        var (ft, _) = syscall.GetFileType(src); if (ft == syscall.FILE_TYPE_PIPE) {
            return (0, syscall.ESPIPE);
        }
    }
    {
        var errΔ1 = fd.writeLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(fd.writeUnlock);
    var o = Ꮡ(fd.wop);
    o.val.handle = src;
    // TODO(brainman): skip calling syscall.Seek if OS allows it
    var (curpos, err) = syscall.Seek((~o).handle, 0, io.SeekCurrent);
    if (err != default!) {
        return (0, err);
    }
    if (n <= 0) {
        // We don't know the size of the file so infer it.
        // Find the number of bytes offset from curpos until the end of the file.
        (n, err) = syscall.Seek((~o).handle, -curpos, io.SeekEnd);
        if (err != default!) {
            return (written, err);
        }
        // Now seek back to the original position.
        {
            (_, err) = syscall.Seek((~o).handle, curpos, io.SeekStart); if (err != default!) {
                return (written, err);
            }
        }
    }
    // TransmitFile can be invoked in one call with at most
    // 2,147,483,646 bytes: the maximum value for a 32-bit integer minus 1.
    // See https://docs.microsoft.com/en-us/windows/win32/api/mswsock/nf-mswsock-transmitfile
    const int64 maxChunkSizePerCall = /* int64(0x7fffffff - 1) */ 2147483646;
    while (n > 0) {
        var chunkSize = maxChunkSizePerCall;
        if (chunkSize > n) {
            chunkSize = n;
        }
        o.val.qty = ((uint32)chunkSize);
        (~o).o.Offset = ((uint32)curpos);
        (~o).o.OffsetHigh = ((uint32)(curpos >> (int)(32)));
        var (nw, err) = execIO(o, 
        (ж<operation> o) => syscall.TransmitFile((~(~oΔ1).fd).Sysfd, (~oΔ1).handle, (~oΔ1).qty, 0, Ꮡ((~oΔ1).o), nil, syscall.TF_WRITE_BEHIND));
        if (err != default!) {
            return (written, err);
        }
        curpos += ((int64)nw);
        // Some versions of Windows (Windows 10 1803) do not set
        // file position after TransmitFile completes.
        // So just use Seek to set file position.
        {
            (_, err) = syscall.Seek((~o).handle, curpos, io.SeekStart); if (err != default!) {
                return (written, err);
            }
        }
        n -= ((int64)nw);
        written += ((int64)nw);
    }
    return (written, err);
});

} // end poll_package
