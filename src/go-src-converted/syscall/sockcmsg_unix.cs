// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Socket control messages

// package syscall -- go2cs converted at 2022 March 13 05:40:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\sockcmsg_unix.go
namespace go;

using @unsafe = @unsafe_package;


// CmsgLen returns the value to store in the Len field of the Cmsghdr
// structure, taking into account any necessary alignment.

public static partial class syscall_package {

public static nint CmsgLen(nint datalen) {
    return cmsgAlignOf(SizeofCmsghdr) + datalen;
}

// CmsgSpace returns the number of bytes an ancillary element with
// payload of the passed data length occupies.
public static nint CmsgSpace(nint datalen) {
    return cmsgAlignOf(SizeofCmsghdr) + cmsgAlignOf(datalen);
}

private static unsafe.Pointer data(this ptr<Cmsghdr> _addr_h, System.UIntPtr offset) {
    ref Cmsghdr h = ref _addr_h.val;

    return @unsafe.Pointer(uintptr(@unsafe.Pointer(h)) + uintptr(cmsgAlignOf(SizeofCmsghdr)) + offset);
}

// SocketControlMessage represents a socket control message.
public partial struct SocketControlMessage {
    public Cmsghdr Header;
    public slice<byte> Data;
}

// ParseSocketControlMessage parses b as an array of socket control
// messages.
public static (slice<SocketControlMessage>, error) ParseSocketControlMessage(slice<byte> b) {
    slice<SocketControlMessage> _p0 = default;
    error _p0 = default!;

    slice<SocketControlMessage> msgs = default;
    nint i = 0;
    while (i + CmsgLen(0) <= len(b)) {
        var (h, dbuf, err) = socketControlMessageHeaderAndData(b[(int)i..]);
        if (err != null) {
            return (null, error.As(err)!);
        }
        SocketControlMessage m = new SocketControlMessage(Header:*h,Data:dbuf);
        msgs = append(msgs, m);
        i += cmsgAlignOf(int(h.Len));
    }
    return (msgs, error.As(null!)!);
}

private static (ptr<Cmsghdr>, slice<byte>, error) socketControlMessageHeaderAndData(slice<byte> b) {
    ptr<Cmsghdr> _p0 = default!;
    slice<byte> _p0 = default;
    error _p0 = default!;

    var h = (Cmsghdr.val)(@unsafe.Pointer(_addr_b[0]));
    if (h.Len < SizeofCmsghdr || uint64(h.Len) > uint64(len(b))) {
        return (_addr_null!, null, error.As(EINVAL)!);
    }
    return (_addr_h!, b[(int)cmsgAlignOf(SizeofCmsghdr)..(int)h.Len], error.As(null!)!);
}

// UnixRights encodes a set of open file descriptors into a socket
// control message for sending to another process.
public static slice<byte> UnixRights(params nint[] fds) {
    fds = fds.Clone();

    var datalen = len(fds) * 4;
    var b = make_slice<byte>(CmsgSpace(datalen));
    var h = (Cmsghdr.val)(@unsafe.Pointer(_addr_b[0]));
    h.Level = SOL_SOCKET;
    h.Type = SCM_RIGHTS;
    h.SetLen(CmsgLen(datalen));
    foreach (var (i, fd) in fds) {
        (int32.val).val;

        (h.data(4 * uintptr(i))) = int32(fd);
    }    return b;
}

// ParseUnixRights decodes a socket control message that contains an
// integer array of open file descriptors from another process.
public static (slice<nint>, error) ParseUnixRights(ptr<SocketControlMessage> _addr_m) {
    slice<nint> _p0 = default;
    error _p0 = default!;
    ref SocketControlMessage m = ref _addr_m.val;

    if (m.Header.Level != SOL_SOCKET) {
        return (null, error.As(EINVAL)!);
    }
    if (m.Header.Type != SCM_RIGHTS) {
        return (null, error.As(EINVAL)!);
    }
    var fds = make_slice<nint>(len(m.Data) >> 2);
    {
        nint i = 0;
        nint j = 0;

        while (i < len(m.Data)) {
            fds[j] = int(new ptr<ptr<ptr<int>>>(@unsafe.Pointer(_addr_m.Data[i])));
            j++;
            i += 4;
        }
    }
    return (fds, error.As(null!)!);
}

} // end syscall_package
