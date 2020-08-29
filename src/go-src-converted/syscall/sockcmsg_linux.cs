// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Socket control messages

// package syscall -- go2cs converted at 2020 August 29 08:37:38 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sockcmsg_linux.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class syscall_package
    {
        // UnixCredentials encodes credentials into a socket control message
        // for sending to another process. This can be used for
        // authentication.
        public static slice<byte> UnixCredentials(ref Ucred ucred)
        {
            var b = make_slice<byte>(CmsgSpace(SizeofUcred));
            var h = (Cmsghdr.Value)(@unsafe.Pointer(ref b[0L]));
            h.Level = SOL_SOCKET;
            h.Type = SCM_CREDENTIALS;
            h.SetLen(CmsgLen(SizeofUcred));
            ((Ucred.Value)(cmsgData(h))).Value = ucred.Value;
            return b;
        }

        // ParseUnixCredentials decodes a socket control message that contains
        // credentials in a Ucred structure. To receive such a message, the
        // SO_PASSCRED option must be enabled on the socket.
        public static (ref Ucred, error) ParseUnixCredentials(ref SocketControlMessage m)
        {
            if (m.Header.Level != SOL_SOCKET)
            {
                return (null, EINVAL);
            }
            if (m.Header.Type != SCM_CREDENTIALS)
            {
                return (null, EINVAL);
            }
            if (uintptr(len(m.Data)) < @unsafe.Sizeof(new Ucred()))
            {
                return (null, EINVAL);
            }
            *(*Ucred) ucred = @unsafe.Pointer(ref m.Data[0L]).Value;
            return (ref ucred, null);
        }
    }
}
