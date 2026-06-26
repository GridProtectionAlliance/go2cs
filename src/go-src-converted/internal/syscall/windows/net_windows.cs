// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
using _ = unsafe_package;

partial class windows_package {

//go:linkname WSASendtoInet4 syscall.wsaSendtoInet4
//go:noescape
public static partial error /*err*/ WSASendtoInet4(syscallꓸHandle s, ж<syscall.WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, ж<syscall.SockaddrInet4> to, ж<syscall.Overlapped> overlapped, ж<byte> croutine);

//go:linkname WSASendtoInet6 syscall.wsaSendtoInet6
//go:noescape
public static partial error /*err*/ WSASendtoInet6(syscallꓸHandle s, ж<syscall.WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, ж<syscall.SockaddrInet6> to, ж<syscall.Overlapped> overlapped, ж<byte> croutine);

public static readonly UntypedInt SIO_TCP_INITIAL_RTO = /* syscall.IOC_IN | syscall.IOC_VENDOR | 17 */ 2550136849;
public const uint16 TCP_INITIAL_RTO_UNSPECIFIED_RTT = /* ^uint16(0) */ 65535;
public const uint8 TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS = /* ^uint8(1) */ 254;

[GoType] partial struct TCP_INITIAL_RTO_PARAMETERS {
    public uint16 Rtt;
    public uint8 MaxSynRetransmissions;
}

} // end windows_package
