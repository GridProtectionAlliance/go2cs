// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Socket control messages

// package syscall -- go2cs converted at 2020 October 09 05:01:32 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sockcmsg_unix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
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

        private static unsafe.Pointer data(this ptr<Cmsghdr> _addr_h, System.UIntPtr offset)
        {
            ref Cmsghdr h = ref _addr_h.val;

            return @unsafe.Pointer(uintptr(@unsafe.Pointer(h)) + uintptr(cmsgAlignOf(SizeofCmsghdr)) + offset);
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
            slice<SocketControlMessage> _p0 = default;
            error _p0 = default!;

            slice<SocketControlMessage> msgs = default;
            long i = 0L;
            while (i + CmsgLen(0L) <= len(b))
            {
                var (h, dbuf, err) = socketControlMessageHeaderAndData(b[i..]);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                SocketControlMessage m = new SocketControlMessage(Header:*h,Data:dbuf);
                msgs = append(msgs, m);
                i += cmsgAlignOf(int(h.Len));

            }

            return (msgs, error.As(null!)!);

        }

        private static (ptr<Cmsghdr>, slice<byte>, error) socketControlMessageHeaderAndData(slice<byte> b)
        {
            ptr<Cmsghdr> _p0 = default!;
            slice<byte> _p0 = default;
            error _p0 = default!;

            var h = (Cmsghdr.val)(@unsafe.Pointer(_addr_b[0L]));
            if (h.Len < SizeofCmsghdr || uint64(h.Len) > uint64(len(b)))
            {
                return (_addr_null!, null, error.As(EINVAL)!);
            }

            return (_addr_h!, b[cmsgAlignOf(SizeofCmsghdr)..h.Len], error.As(null!)!);

        }

        // UnixRights encodes a set of open file descriptors into a socket
        // control message for sending to another process.
        public static slice<byte> UnixRights(params long[] fds)
        {
            fds = fds.Clone();

            var datalen = len(fds) * 4L;
            var b = make_slice<byte>(CmsgSpace(datalen));
            var h = (Cmsghdr.val)(@unsafe.Pointer(_addr_b[0L]));
            h.Level = SOL_SOCKET;
            h.Type = SCM_RIGHTS;
            h.SetLen(CmsgLen(datalen));
            foreach (var (i, fd) in fds)
            {
                (int32.val)(h.data(4L * uintptr(i))).val;

                int32(fd);

            }
            return b;

        }

        // ParseUnixRights decodes a socket control message that contains an
        // integer array of open file descriptors from another process.
        public static (slice<long>, error) ParseUnixRights(ptr<SocketControlMessage> _addr_m)
        {
            slice<long> _p0 = default;
            error _p0 = default!;
            ref SocketControlMessage m = ref _addr_m.val;

            if (m.Header.Level != SOL_SOCKET)
            {
                return (null, error.As(EINVAL)!);
            }

            if (m.Header.Type != SCM_RIGHTS)
            {
                return (null, error.As(EINVAL)!);
            }

            var fds = make_slice<long>(len(m.Data) >> (int)(2L));
            {
                long i = 0L;
                long j = 0L;

                while (i < len(m.Data))
                {
                    fds[j] = int(new ptr<ptr<ptr<int>>>(@unsafe.Pointer(_addr_m.Data[i])));
                    j++;
                    i += 4L;
                }

            }
            return (fds, error.As(null!)!);

        }
    }
}
