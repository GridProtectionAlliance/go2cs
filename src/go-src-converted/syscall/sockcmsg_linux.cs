// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Socket control messages

// package syscall -- go2cs converted at 2020 October 08 03:27:02 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sockcmsg_linux.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // UnixCredentials encodes credentials into a socket control message
        // for sending to another process. This can be used for
        // authentication.
        public static slice<byte> UnixCredentials(ptr<Ucred> _addr_ucred)
        {
            ref Ucred ucred = ref _addr_ucred.val;

            var b = make_slice<byte>(CmsgSpace(SizeofUcred));
            var h = (Cmsghdr.val)(@unsafe.Pointer(_addr_b[0L]));
            h.Level = SOL_SOCKET;
            h.Type = SCM_CREDENTIALS;
            h.SetLen(CmsgLen(SizeofUcred)) * (Ucred.val)(h.data(0L));

            ucred;
            return b;

        }

        // ParseUnixCredentials decodes a socket control message that contains
        // credentials in a Ucred structure. To receive such a message, the
        // SO_PASSCRED option must be enabled on the socket.
        public static (ptr<Ucred>, error) ParseUnixCredentials(ptr<SocketControlMessage> _addr_m)
        {
            ptr<Ucred> _p0 = default!;
            error _p0 = default!;
            ref SocketControlMessage m = ref _addr_m.val;

            if (m.Header.Level != SOL_SOCKET)
            {
                return (_addr_null!, error.As(EINVAL)!);
            }

            if (m.Header.Type != SCM_CREDENTIALS)
            {
                return (_addr_null!, error.As(EINVAL)!);
            }

            if (uintptr(len(m.Data)) < @unsafe.Sizeof(new Ucred()))
            {
                return (_addr_null!, error.As(EINVAL)!);
            }

            ptr<ptr<Ucred>> ucred = new ptr<ptr<ptr<Ucred>>>(@unsafe.Pointer(_addr_m.Data[0L]));
            return (_addr__addr_ucred!, error.As(null!)!);

        }
    }
}
