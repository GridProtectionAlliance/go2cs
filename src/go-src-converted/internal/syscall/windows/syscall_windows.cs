// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 22:13:11 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Program Files\Go\src\internal\syscall\windows\syscall_windows.go
using unsafeheader = go.@internal.unsafeheader_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.@internal.syscall;

public static partial class windows_package {

    // UTF16PtrToString is like UTF16ToString, but takes *uint16
    // as a parameter instead of []uint16.
public static @string UTF16PtrToString(ptr<ushort> _addr_p) {
    ref ushort p = ref _addr_p.val;

    if (p == null) {
        return "";
    }
    var end = @unsafe.Pointer(p);
    nint n = 0;
    while (new ptr<ptr<ptr<ushort>>>(end) != 0) {
        end = @unsafe.Pointer(uintptr(end) + @unsafe.Sizeof(p));
        n++;
    } 
    // Turn *uint16 into []uint16.
    ref slice<ushort> s = ref heap(out ptr<slice<ushort>> _addr_s);
    var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_s));
    hdr.Data = @unsafe.Pointer(p);
    hdr.Cap = n;
    hdr.Len = n; 
    // Decode []uint16 into string.
    return string(utf16.Decode(s));

}

public static readonly syscall.Errno ERROR_SHARING_VIOLATION = 32;
public static readonly syscall.Errno ERROR_LOCK_VIOLATION = 33;
public static readonly syscall.Errno ERROR_NOT_SUPPORTED = 50;
public static readonly syscall.Errno ERROR_CALL_NOT_IMPLEMENTED = 120;
public static readonly syscall.Errno ERROR_INVALID_NAME = 123;
public static readonly syscall.Errno ERROR_LOCK_FAILED = 167;
public static readonly syscall.Errno ERROR_NO_UNICODE_TRANSLATION = 1113;


public static readonly nuint GAA_FLAG_INCLUDE_PREFIX = 0x00000010;



public static readonly nint IF_TYPE_OTHER = 1;
public static readonly nint IF_TYPE_ETHERNET_CSMACD = 6;
public static readonly nint IF_TYPE_ISO88025_TOKENRING = 9;
public static readonly nint IF_TYPE_PPP = 23;
public static readonly nint IF_TYPE_SOFTWARE_LOOPBACK = 24;
public static readonly nint IF_TYPE_ATM = 37;
public static readonly nint IF_TYPE_IEEE80211 = 71;
public static readonly nint IF_TYPE_TUNNEL = 131;
public static readonly nint IF_TYPE_IEEE1394 = 144;


public partial struct SocketAddress {
    public ptr<syscall.RawSockaddrAny> Sockaddr;
    public int SockaddrLength;
}

public partial struct IpAdapterUnicastAddress {
    public uint Length;
    public uint Flags;
    public ptr<IpAdapterUnicastAddress> Next;
    public SocketAddress Address;
    public int PrefixOrigin;
    public int SuffixOrigin;
    public int DadState;
    public uint ValidLifetime;
    public uint PreferredLifetime;
    public uint LeaseLifetime;
    public byte OnLinkPrefixLength;
}

public partial struct IpAdapterAnycastAddress {
    public uint Length;
    public uint Flags;
    public ptr<IpAdapterAnycastAddress> Next;
    public SocketAddress Address;
}

public partial struct IpAdapterMulticastAddress {
    public uint Length;
    public uint Flags;
    public ptr<IpAdapterMulticastAddress> Next;
    public SocketAddress Address;
}

public partial struct IpAdapterDnsServerAdapter {
    public uint Length;
    public uint Reserved;
    public ptr<IpAdapterDnsServerAdapter> Next;
    public SocketAddress Address;
}

public partial struct IpAdapterPrefix {
    public uint Length;
    public uint Flags;
    public ptr<IpAdapterPrefix> Next;
    public SocketAddress Address;
    public uint PrefixLength;
}

public partial struct IpAdapterAddresses {
    public uint Length;
    public uint IfIndex;
    public ptr<IpAdapterAddresses> Next;
    public ptr<byte> AdapterName;
    public ptr<IpAdapterUnicastAddress> FirstUnicastAddress;
    public ptr<IpAdapterAnycastAddress> FirstAnycastAddress;
    public ptr<IpAdapterMulticastAddress> FirstMulticastAddress;
    public ptr<IpAdapterDnsServerAdapter> FirstDnsServerAddress;
    public ptr<ushort> DnsSuffix;
    public ptr<ushort> Description;
    public ptr<ushort> FriendlyName;
    public array<byte> PhysicalAddress;
    public uint PhysicalAddressLength;
    public uint Flags;
    public uint Mtu;
    public uint IfType;
    public uint OperStatus;
    public uint Ipv6IfIndex;
    public array<uint> ZoneIndices;
    public ptr<IpAdapterPrefix> FirstPrefix; /* more fields might be present here. */
}

public partial struct FILE_BASIC_INFO {
    public syscall.Filetime CreationTime;
    public syscall.Filetime LastAccessTime;
    public syscall.Filetime LastWriteTime;
    public syscall.Filetime ChangedTime;
    public uint FileAttributes;
}

public static readonly nint IfOperStatusUp = 1;
public static readonly nint IfOperStatusDown = 2;
public static readonly nint IfOperStatusTesting = 3;
public static readonly nint IfOperStatusUnknown = 4;
public static readonly nint IfOperStatusDormant = 5;
public static readonly nint IfOperStatusNotPresent = 6;
public static readonly nint IfOperStatusLowerLayerDown = 7;


//sys    GetAdaptersAddresses(family uint32, flags uint32, reserved uintptr, adapterAddresses *IpAdapterAddresses, sizePointer *uint32) (errcode error) = iphlpapi.GetAdaptersAddresses
//sys    GetComputerNameEx(nameformat uint32, buf *uint16, n *uint32) (err error) = GetComputerNameExW
//sys    MoveFileEx(from *uint16, to *uint16, flags uint32) (err error) = MoveFileExW
//sys    GetModuleFileName(module syscall.Handle, fn *uint16, len uint32) (n uint32, err error) = kernel32.GetModuleFileNameW
//sys    SetFileInformationByHandle(handle syscall.Handle, fileInformationClass uint32, buf uintptr, bufsize uint32) (err error) = kernel32.SetFileInformationByHandle

public static readonly nuint WSA_FLAG_OVERLAPPED = 0x01;
public static readonly nuint WSA_FLAG_NO_HANDLE_INHERIT = 0x80;

public static readonly syscall.Errno WSAEMSGSIZE = 10040;

public static readonly nuint MSG_PEEK = 0x2;
public static readonly nuint MSG_TRUNC = 0x0100;
public static readonly nuint MSG_CTRUNC = 0x0200;

private static readonly var socket_error = uintptr(~uint32(0));


public static syscall.GUID WSAID_WSASENDMSG = new syscall.GUID(Data1:0xa441e712,Data2:0x754f,Data3:0x43ca,Data4:[8]byte{0x84,0xa7,0x0d,0xee,0x44,0xcf,0x60,0x6d},);

public static syscall.GUID WSAID_WSARECVMSG = new syscall.GUID(Data1:0xf689d7c8,Data2:0x6f1f,Data3:0x436b,Data4:[8]byte{0x8a,0x53,0xe5,0x4f,0xe3,0x51,0xc3,0x22},);

private static var sendRecvMsgFunc = default;

public partial struct WSAMsg {
    public syscall.Pointer Name;
    public int Namelen;
    public ptr<syscall.WSABuf> Buffers;
    public uint BufferCount;
    public syscall.WSABuf Control;
    public uint Flags;
}

//sys    WSASocket(af int32, typ int32, protocol int32, protinfo *syscall.WSAProtocolInfo, group uint32, flags uint32) (handle syscall.Handle, err error) [failretval==syscall.InvalidHandle] = ws2_32.WSASocketW

private static error loadWSASendRecvMsg() => func((defer, _, _) => {
    sendRecvMsgFunc.once.Do(() => {
        syscall.Handle s = default;
        s, sendRecvMsgFunc.err = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, syscall.IPPROTO_UDP);
        if (sendRecvMsgFunc.err != null) {
            return ;
        }
        defer(syscall.CloseHandle(s));
        ref uint n = ref heap(out ptr<uint> _addr_n);
        sendRecvMsgFunc.err = syscall.WSAIoctl(s, syscall.SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.val)(@unsafe.Pointer(_addr_WSAID_WSARECVMSG)), uint32(@unsafe.Sizeof(WSAID_WSARECVMSG)), (byte.val)(@unsafe.Pointer(_addr_sendRecvMsgFunc.recvAddr)), uint32(@unsafe.Sizeof(sendRecvMsgFunc.recvAddr)), _addr_n, null, 0);
        if (sendRecvMsgFunc.err != null) {
            return ;
        }
        sendRecvMsgFunc.err = syscall.WSAIoctl(s, syscall.SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.val)(@unsafe.Pointer(_addr_WSAID_WSASENDMSG)), uint32(@unsafe.Sizeof(WSAID_WSASENDMSG)), (byte.val)(@unsafe.Pointer(_addr_sendRecvMsgFunc.sendAddr)), uint32(@unsafe.Sizeof(sendRecvMsgFunc.sendAddr)), _addr_n, null, 0);

    });
    return error.As(sendRecvMsgFunc.err)!;

});

public static error WSASendMsg(syscall.Handle fd, ptr<WSAMsg> _addr_msg, uint flags, ptr<uint> _addr_bytesSent, ptr<syscall.Overlapped> _addr_overlapped, ptr<byte> _addr_croutine) {
    ref WSAMsg msg = ref _addr_msg.val;
    ref uint bytesSent = ref _addr_bytesSent.val;
    ref syscall.Overlapped overlapped = ref _addr_overlapped.val;
    ref byte croutine = ref _addr_croutine.val;

    var err = loadWSASendRecvMsg();
    if (err != null) {
        return error.As(err)!;
    }
    var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.sendAddr, 6, uintptr(fd), uintptr(@unsafe.Pointer(msg)), uintptr(flags), uintptr(@unsafe.Pointer(bytesSent)), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)));
    if (r1 == socket_error) {
        if (e1 != 0) {
            err = errnoErr(e1);
        }
        else
 {
            err = syscall.EINVAL;
        }
    }
    return error.As(err)!;

}

public static error WSARecvMsg(syscall.Handle fd, ptr<WSAMsg> _addr_msg, ptr<uint> _addr_bytesReceived, ptr<syscall.Overlapped> _addr_overlapped, ptr<byte> _addr_croutine) {
    ref WSAMsg msg = ref _addr_msg.val;
    ref uint bytesReceived = ref _addr_bytesReceived.val;
    ref syscall.Overlapped overlapped = ref _addr_overlapped.val;
    ref byte croutine = ref _addr_croutine.val;

    var err = loadWSASendRecvMsg();
    if (err != null) {
        return error.As(err)!;
    }
    var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.recvAddr, 5, uintptr(fd), uintptr(@unsafe.Pointer(msg)), uintptr(@unsafe.Pointer(bytesReceived)), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)), 0);
    if (r1 == socket_error) {
        if (e1 != 0) {
            err = errnoErr(e1);
        }
        else
 {
            err = syscall.EINVAL;
        }
    }
    return error.As(err)!;

}

public static readonly nint ComputerNameNetBIOS = 0;
public static readonly nint ComputerNameDnsHostname = 1;
public static readonly nint ComputerNameDnsDomain = 2;
public static readonly nint ComputerNameDnsFullyQualified = 3;
public static readonly nint ComputerNamePhysicalNetBIOS = 4;
public static readonly nint ComputerNamePhysicalDnsHostname = 5;
public static readonly nint ComputerNamePhysicalDnsDomain = 6;
public static readonly nint ComputerNamePhysicalDnsFullyQualified = 7;
public static readonly nint ComputerNameMax = 8;

public static readonly nuint MOVEFILE_REPLACE_EXISTING = 0x1;
public static readonly nuint MOVEFILE_COPY_ALLOWED = 0x2;
public static readonly nuint MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;
public static readonly nuint MOVEFILE_WRITE_THROUGH = 0x8;
public static readonly nuint MOVEFILE_CREATE_HARDLINK = 0x10;
public static readonly nuint MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x20;


public static error Rename(@string oldpath, @string newpath) {
    var (from, err) = syscall.UTF16PtrFromString(oldpath);
    if (err != null) {
        return error.As(err)!;
    }
    var (to, err) = syscall.UTF16PtrFromString(newpath);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(MoveFileEx(from, to, MOVEFILE_REPLACE_EXISTING))!;

}

//sys LockFileEx(file syscall.Handle, flags uint32, reserved uint32, bytesLow uint32, bytesHigh uint32, overlapped *syscall.Overlapped) (err error) = kernel32.LockFileEx
//sys UnlockFileEx(file syscall.Handle, reserved uint32, bytesLow uint32, bytesHigh uint32, overlapped *syscall.Overlapped) (err error) = kernel32.UnlockFileEx

public static readonly nuint LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;
public static readonly nuint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;


public static readonly nint MB_ERR_INVALID_CHARS = 8;

//sys    GetACP() (acp uint32) = kernel32.GetACP
//sys    GetConsoleCP() (ccp uint32) = kernel32.GetConsoleCP
//sys    MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar
//sys    GetCurrentThread() (pseudoHandle syscall.Handle, err error) = kernel32.GetCurrentThread



//sys    GetACP() (acp uint32) = kernel32.GetACP
//sys    GetConsoleCP() (ccp uint32) = kernel32.GetConsoleCP
//sys    MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar
//sys    GetCurrentThread() (pseudoHandle syscall.Handle, err error) = kernel32.GetCurrentThread

public static readonly nuint STYPE_DISKTREE = 0x00;



public partial struct SHARE_INFO_2 {
    public ptr<ushort> Netname;
    public uint Type;
    public ptr<ushort> Remark;
    public uint Permissions;
    public uint MaxUses;
    public uint CurrentUses;
    public ptr<ushort> Path;
    public ptr<ushort> Passwd;
}

//sys  NetShareAdd(serverName *uint16, level uint32, buf *byte, parmErr *uint16) (neterr error) = netapi32.NetShareAdd
//sys  NetShareDel(serverName *uint16, netName *uint16, reserved uint32) (neterr error) = netapi32.NetShareDel

public static readonly nuint FILE_NAME_NORMALIZED = 0x0;
public static readonly nuint FILE_NAME_OPENED = 0x8;

public static readonly nuint VOLUME_NAME_DOS = 0x0;
public static readonly nuint VOLUME_NAME_GUID = 0x1;
public static readonly nuint VOLUME_NAME_NONE = 0x4;
public static readonly nuint VOLUME_NAME_NT = 0x2;


//sys    GetFinalPathNameByHandle(file syscall.Handle, filePath *uint16, filePathSize uint32, flags uint32) (n uint32, err error) = kernel32.GetFinalPathNameByHandleW

public static error LoadGetFinalPathNameByHandle() {
    return error.As(procGetFinalPathNameByHandleW.Find())!;
}

//sys    CreateEnvironmentBlock(block **uint16, token syscall.Token, inheritExisting bool) (err error) = userenv.CreateEnvironmentBlock
//sys    DestroyEnvironmentBlock(block *uint16) (err error) = userenv.DestroyEnvironmentBlock

//sys    RtlGenRandom(buf []byte) (err error) = advapi32.SystemFunction036

} // end windows_package
