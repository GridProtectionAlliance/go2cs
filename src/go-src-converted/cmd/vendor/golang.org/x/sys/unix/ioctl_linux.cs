// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 23:26:36 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\ioctl_linux.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // IoctlRetInt performs an ioctl operation specified by req on a device
    // associated with opened file descriptor fd, and returns a non-negative
    // integer that is returned by the ioctl syscall.
public static (nint, error) IoctlRetInt(nint fd, nuint req) {
    nint _p0 = default;
    error _p0 = default!;

    var (ret, _, err) = Syscall(SYS_IOCTL, uintptr(fd), uintptr(req), 0);
    if (err != 0) {
        return (0, error.As(err)!);
    }
    return (int(ret), error.As(null!)!);

}

public static (uint, error) IoctlGetUint32(nint fd, nuint req) {
    uint _p0 = default;
    error _p0 = default!;

    ref uint value = ref heap(out ptr<uint> _addr_value);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
    return (value, error.As(err)!);
}

public static (ptr<RTCTime>, error) IoctlGetRTCTime(nint fd) {
    ptr<RTCTime> _p0 = default!;
    error _p0 = default!;

    ref RTCTime value = ref heap(out ptr<RTCTime> _addr_value);
    var err = ioctl(fd, RTC_RD_TIME, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
}

public static error IoctlSetRTCTime(nint fd, ptr<RTCTime> _addr_value) {
    ref RTCTime value = ref _addr_value.val;

    var err = ioctl(fd, RTC_SET_TIME, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

public static (ptr<RTCWkAlrm>, error) IoctlGetRTCWkAlrm(nint fd) {
    ptr<RTCWkAlrm> _p0 = default!;
    error _p0 = default!;

    ref RTCWkAlrm value = ref heap(out ptr<RTCWkAlrm> _addr_value);
    var err = ioctl(fd, RTC_WKALM_RD, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
}

public static error IoctlSetRTCWkAlrm(nint fd, ptr<RTCWkAlrm> _addr_value) {
    ref RTCWkAlrm value = ref _addr_value.val;

    var err = ioctl(fd, RTC_WKALM_SET, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

private partial struct ifreqEthtool {
    public array<byte> name;
    public unsafe.Pointer data;
}

// IoctlGetEthtoolDrvinfo fetches ethtool driver information for the network
// device specified by ifname.
public static (ptr<EthtoolDrvinfo>, error) IoctlGetEthtoolDrvinfo(nint fd, @string ifname) {
    ptr<EthtoolDrvinfo> _p0 = default!;
    error _p0 = default!;
 
    // Leave room for terminating NULL byte.
    if (len(ifname) >= IFNAMSIZ) {
        return (_addr_null!, error.As(EINVAL)!);
    }
    ref EthtoolDrvinfo value = ref heap(new EthtoolDrvinfo(Cmd:ETHTOOL_GDRVINFO,), out ptr<EthtoolDrvinfo> _addr_value);
    ref ifreqEthtool ifreq = ref heap(new ifreqEthtool(data:unsafe.Pointer(&value),), out ptr<ifreqEthtool> _addr_ifreq);
    copy(ifreq.name[..], ifname);
    var err = ioctl(fd, SIOCETHTOOL, uintptr(@unsafe.Pointer(_addr_ifreq)));
    runtime.KeepAlive(ifreq);
    return (_addr__addr_value!, error.As(err)!);

}

// IoctlGetWatchdogInfo fetches information about a watchdog device from the
// Linux watchdog API. For more information, see:
// https://www.kernel.org/doc/html/latest/watchdog/watchdog-api.html.
public static (ptr<WatchdogInfo>, error) IoctlGetWatchdogInfo(nint fd) {
    ptr<WatchdogInfo> _p0 = default!;
    error _p0 = default!;

    ref WatchdogInfo value = ref heap(out ptr<WatchdogInfo> _addr_value);
    var err = ioctl(fd, WDIOC_GETSUPPORT, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
}

// IoctlWatchdogKeepalive issues a keepalive ioctl to a watchdog device. For
// more information, see:
// https://www.kernel.org/doc/html/latest/watchdog/watchdog-api.html.
public static error IoctlWatchdogKeepalive(nint fd) {
    return error.As(ioctl(fd, WDIOC_KEEPALIVE, 0))!;
}

// IoctlFileCloneRange performs an FICLONERANGE ioctl operation to clone the
// range of data conveyed in value to the file associated with the file
// descriptor destFd. See the ioctl_ficlonerange(2) man page for details.
public static error IoctlFileCloneRange(nint destFd, ptr<FileCloneRange> _addr_value) {
    ref FileCloneRange value = ref _addr_value.val;

    var err = ioctl(destFd, FICLONERANGE, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

// IoctlFileClone performs an FICLONE ioctl operation to clone the entire file
// associated with the file description srcFd to the file associated with the
// file descriptor destFd. See the ioctl_ficlone(2) man page for details.
public static error IoctlFileClone(nint destFd, nint srcFd) {
    return error.As(ioctl(destFd, FICLONE, uintptr(srcFd)))!;
}

public partial struct FileDedupeRange {
    public ulong Src_offset;
    public ulong Src_length;
    public ushort Reserved1;
    public uint Reserved2;
    public slice<FileDedupeRangeInfo> Info;
}

public partial struct FileDedupeRangeInfo {
    public long Dest_fd;
    public ulong Dest_offset;
    public ulong Bytes_deduped;
    public int Status;
    public uint Reserved;
}

// IoctlFileDedupeRange performs an FIDEDUPERANGE ioctl operation to share the
// range of data conveyed in value from the file associated with the file
// descriptor srcFd to the value.Info destinations. See the
// ioctl_fideduperange(2) man page for details.
public static error IoctlFileDedupeRange(nint srcFd, ptr<FileDedupeRange> _addr_value) {
    ref FileDedupeRange value = ref _addr_value.val;

    var buf = make_slice<byte>(SizeofRawFileDedupeRange + len(value.Info) * SizeofRawFileDedupeRangeInfo);
    var rawrange = (RawFileDedupeRange.val)(@unsafe.Pointer(_addr_buf[0]));
    rawrange.Src_offset = value.Src_offset;
    rawrange.Src_length = value.Src_length;
    rawrange.Dest_count = uint16(len(value.Info));
    rawrange.Reserved1 = value.Reserved1;
    rawrange.Reserved2 = value.Reserved2;

    {
        var i__prev1 = i;

        foreach (var (__i) in value.Info) {
            i = __i;
            var rawinfo = (RawFileDedupeRangeInfo.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(_addr_buf[0])) + uintptr(SizeofRawFileDedupeRange) + uintptr(i * SizeofRawFileDedupeRangeInfo)));
            rawinfo.Dest_fd = value.Info[i].Dest_fd;
            rawinfo.Dest_offset = value.Info[i].Dest_offset;
            rawinfo.Bytes_deduped = value.Info[i].Bytes_deduped;
            rawinfo.Status = value.Info[i].Status;
            rawinfo.Reserved = value.Info[i].Reserved;
        }
        i = i__prev1;
    }

    var err = ioctl(srcFd, FIDEDUPERANGE, uintptr(@unsafe.Pointer(_addr_buf[0]))); 

    // Output
    {
        var i__prev1 = i;

        foreach (var (__i) in value.Info) {
            i = __i;
            rawinfo = (RawFileDedupeRangeInfo.val)(@unsafe.Pointer(uintptr(@unsafe.Pointer(_addr_buf[0])) + uintptr(SizeofRawFileDedupeRange) + uintptr(i * SizeofRawFileDedupeRangeInfo)));
            value.Info[i].Dest_fd = rawinfo.Dest_fd;
            value.Info[i].Dest_offset = rawinfo.Dest_offset;
            value.Info[i].Bytes_deduped = rawinfo.Bytes_deduped;
            value.Info[i].Status = rawinfo.Status;
            value.Info[i].Reserved = rawinfo.Reserved;
        }
        i = i__prev1;
    }

    return error.As(err)!;

}

public static error IoctlHIDGetDesc(nint fd, ptr<HIDRawReportDescriptor> _addr_value) {
    ref HIDRawReportDescriptor value = ref _addr_value.val;

    var err = ioctl(fd, HIDIOCGRDESC, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

public static (ptr<HIDRawDevInfo>, error) IoctlHIDGetRawInfo(nint fd) {
    ptr<HIDRawDevInfo> _p0 = default!;
    error _p0 = default!;

    ref HIDRawDevInfo value = ref heap(out ptr<HIDRawDevInfo> _addr_value);
    var err = ioctl(fd, HIDIOCGRAWINFO, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
}

public static (@string, error) IoctlHIDGetRawName(nint fd) {
    @string _p0 = default;
    error _p0 = default!;

    array<byte> value = new array<byte>(_HIDIOCGRAWNAME_LEN);
    var err = ioctl(fd, _HIDIOCGRAWNAME, uintptr(@unsafe.Pointer(_addr_value[0])));
    return (ByteSliceToString(value[..]), error.As(err)!);
}

public static (@string, error) IoctlHIDGetRawPhys(nint fd) {
    @string _p0 = default;
    error _p0 = default!;

    array<byte> value = new array<byte>(_HIDIOCGRAWPHYS_LEN);
    var err = ioctl(fd, _HIDIOCGRAWPHYS, uintptr(@unsafe.Pointer(_addr_value[0])));
    return (ByteSliceToString(value[..]), error.As(err)!);
}

public static (@string, error) IoctlHIDGetRawUniq(nint fd) {
    @string _p0 = default;
    error _p0 = default!;

    array<byte> value = new array<byte>(_HIDIOCGRAWUNIQ_LEN);
    var err = ioctl(fd, _HIDIOCGRAWUNIQ, uintptr(@unsafe.Pointer(_addr_value[0])));
    return (ByteSliceToString(value[..]), error.As(err)!);
}

} // end unix_package
