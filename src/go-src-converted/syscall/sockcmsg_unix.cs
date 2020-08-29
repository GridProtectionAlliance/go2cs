// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// Socket control messages

// package syscall -- go2cs converted at 2020 August 29 08:37:40 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sockcmsg_unix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class syscall_package
    {
        // Round the length of a raw sockaddr up to align it properly.
        private static long cmsgAlignOf(long salen)
        {
            var salign = sizeofPtr; 
            // NOTE: It seems like 64-bit Darwin, DragonFly BSD and
            // Solaris kernels still require 32-bit aligned access to
            // network subsystem.
            if (darwin64Bit || dragonfly64Bit || solaris64Bit)
            {
                salign = 4L;
            }
            return (salen + salign - 1L) & ~(salign - 1L);
        }

        // CmsgLen returns the value to store in the Len field of the Cmsghdr
        // structure, taking into account any necessary alignment.
        public static long CmsgLen(long datalen)
        {
            return cmsgAlignOf(SizeofCmsghdr) + datalen;
        }

        // CmsgSpace returns the number of bytes an ancillary element with
        // payload of the passed data length occupies.
        public static long CmsgSpace(long datalen)
        {
            return cmsgAlignOf(SizeofCmsghdr) + cmsgAlignOf(datalen);
        }

        private static unsafe.Pointer cmsgData(ref Cmsghdr h)
        {
            return @unsafe.Pointer(uintptr(@unsafe.Pointer(h)) + uintptr(cmsgAlignOf(SizeofCmsghdr)));
        }

        // SocketControlMessage represents a socket control message.
        public partial struct SocketControlMessage
        {
            public Cmsghdr Header;
            public slice<byte> Data;
        }

        // ParseSocketControlMessage parses b as an array of socket control
        // messages.
        public static (slice<SocketControlMessage>, error) ParseSocketControlMessage(slice<byte> b)
        {
            slice<SocketControlMessage> msgs = default;
            long i = 0L;
            while (i + CmsgLen(0L) <= len(b))
            {
                var (h, dbuf, err) = socketControlMessageHeaderAndData(b[i..]);
                if (err != null)
                {
                    return (null, err);
                }
                SocketControlMessage m = new SocketControlMessage(Header:*h,Data:dbuf);
                msgs = append(msgs, m);
                i += cmsgAlignOf(int(h.Len));
            }

            return (msgs, null);
        }

        private static (ref Cmsghdr, slice<byte>, error) socketControlMessageHeaderAndData(slice<byte> b)
        {
            var h = (Cmsghdr.Value)(@unsafe.Pointer(ref b[0L]));
            if (h.Len < SizeofCmsghdr || uint64(h.Len) > uint64(len(b)))
            {
                return (null, null, EINVAL);
            }
            return (h, b[cmsgAlignOf(SizeofCmsghdr)..h.Len], null);
        }

        // UnixRights encodes a set of open file descriptors into a socket
        // control message for sending to another process.
        public static slice<byte> UnixRights(params long[] fds)
        {
            fds = fds.Clone();

            var datalen = len(fds) * 4L;
            var b = make_slice<byte>(CmsgSpace(datalen));
            var h = (Cmsghdr.Value)(@unsafe.Pointer(ref b[0L]));
            h.Level = SOL_SOCKET;
            h.Type = SCM_RIGHTS;
            h.SetLen(CmsgLen(datalen));
            var data = cmsgData(h);
            foreach (var (_, fd) in fds)
            {
                (int32.Value)(data).Value;

                int32(fd);
                data = @unsafe.Pointer(uintptr(data) + 4L);
            }
            return b;
        }

        // ParseUnixRights decodes a socket control message that contains an
        // integer array of open file descriptors from another process.
        public static (slice<long>, error) ParseUnixRights(ref SocketControlMessage m)
        {
            if (m.Header.Level != SOL_SOCKET)
            {
                return (null, EINVAL);
            }
            if (m.Header.Type != SCM_RIGHTS)
            {
                return (null, EINVAL);
            }
            var fds = make_slice<long>(len(m.Data) >> (int)(2L));
            {
                long i = 0L;
                long j = 0L;

                while (i < len(m.Data))
                {
                    fds[j] = int(@unsafe.Pointer(ref m.Data[i]).Value);
                    j++;
                    i += 4L;
                }

            }
            return (fds, null);
        }
    }
}
