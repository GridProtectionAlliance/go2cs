// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using errors = errors_package;
using sync = sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfow
[GoType] partial struct _OSVERSIONINFOW {
    internal uint32 osVersionInfoSize;
    internal uint32 majorVersion;
    internal uint32 minorVersion;
    internal uint32 buildNumber;
    internal uint32 platformId;
    internal array<uint16> csdVersion = new(128);
}

// According to documentation, RtlGetVersion function always succeeds.
//sys	rtlGetVersion(info *_OSVERSIONINFOW) = ntdll.RtlGetVersion

// version retrieves the major, minor, and build version numbers
// of the current Windows OS from the RtlGetVersion API.
internal static (uint32 major, uint32 minor, uint32 build) version() {
    uint32 major = default!;
    uint32 minor = default!;
    uint32 build = default!;

    ref var info = ref heap<_OSVERSIONINFOW>(out var Ꮡinfo);
    info = new _OSVERSIONINFOW(nil);
    info.osVersionInfoSize = ((uint32)@unsafe.Sizeof(info));
    rtlGetVersion(Ꮡinfo);
    return (info.majorVersion, info.minorVersion, info.buildNumber);
}

internal static bool supportTCPKeepAliveIdle;
internal static bool supportTCPKeepAliveInterval;
internal static bool supportTCPKeepAliveCount;

// Fallback to checking the Windows version.
internal static Action initTCPKeepAlive = sync.OnceFunc(() => {
    var (s, err) = WSASocket(syscall.AF_INET, syscall.SOCK_STREAM, syscall.IPPROTO_TCP, nil, 0, WSA_FLAG_NO_HANDLE_INHERIT);
    if (err != default!) {
        var (major, _, build) = version();
        var supportTCPKeepAliveIdle = major >= 10 && build >= 16299;
        var supportTCPKeepAliveInterval = major >= 10 && build >= 16299;
        var supportTCPKeepAliveCount = major >= 10 && build >= 15063;
        return (major, minor, build);
    }
    deferǃ(syscall.Closesocket, s, defer);
    internal static Func<nint, bool> optSupported = (nint opt) => {
        var err = syscall.SetsockoptInt(s, syscall.IPPROTO_TCP, opt, 1);
        return !errors.Is(err, syscall.WSAENOPROTOOPT);
    };

    var supportTCPKeepAliveIdle = optSupported(TCP_KEEPIDLE);
    var supportTCPKeepAliveInterval = optSupported(TCP_KEEPINTVL);
    var supportTCPKeepAliveCount = optSupported(TCP_KEEPCNT);
});

// SupportTCPKeepAliveInterval indicates whether TCP_KEEPIDLE is supported.
// The minimal requirement is Windows 10.0.16299.
public static bool SupportTCPKeepAliveIdle() {
    initTCPKeepAlive();
    return supportTCPKeepAliveIdle;
}

// SupportTCPKeepAliveInterval indicates whether TCP_KEEPINTVL is supported.
// The minimal requirement is Windows 10.0.16299.
public static bool SupportTCPKeepAliveInterval() {
    initTCPKeepAlive();
    return supportTCPKeepAliveInterval;
}

// SupportTCPKeepAliveCount indicates whether TCP_KEEPCNT is supported.
// supports TCP_KEEPCNT.
// The minimal requirement is Windows 10.0.15063.
public static bool SupportTCPKeepAliveCount() {
    initTCPKeepAlive();
    return supportTCPKeepAliveCount;
}

// SupportTCPInitialRTONoSYNRetransmissions indicates whether the current
// Windows version supports the TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS.
// The minimal requirement is Windows 10.0.16299.
public static Func<bool> SupportTCPInitialRTONoSYNRetransmissions = sync.OnceValue(() => {
    var (major, _, build) = version();
    return major >= 10 && build >= 16299;
});

// First call to get the required buffer size in bytes.
// Ignore the error, it will always fail.
// Second call to get the actual protocols.
// SupportUnixSocket indicates whether the current Windows version supports
// Unix Domain Sockets.
// The minimal requirement is Windows 10.0.17063.
public static Func<bool> SupportUnixSocket = sync.OnceValue(() => {
    internal static uint32 size;

    (_, _) = syscall.WSAEnumProtocols(nil, nil, Ꮡ(size));
    var n = ((int32)size) / ((int32)@unsafe.Sizeof(new syscall.WSAProtocolInfo(nil)));
    var buf = new slice<syscall.WSAProtocolInfo>(n);
    var (n, err) = syscall.WSAEnumProtocols(nil, Ꮡ(buf, 0), Ꮡ(size));
    if (err != default!) {
        return false;
    }
    for (var i = ((int32)0); i < n; i++) {
        if (buf[i].AddressFamily == syscall.AF_UNIX) {
            return true;
        }
    }
    return false;
});

} // end windows_package
