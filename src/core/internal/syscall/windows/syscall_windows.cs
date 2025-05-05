// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using sync = sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

// CanUseLongPaths is true when the OS supports opting into
// proper long path handling without the need for fixups.
//
//go:linkname CanUseLongPaths
public static bool CanUseLongPaths;

// UTF16PtrToString is like UTF16ToString, but takes *uint16
// as a parameter instead of []uint16.
public static @string UTF16PtrToString(ж<uint16> Ꮡp) {
    ref var p = ref Ꮡp.val;

    if (p == nil) {
        return ""u8;
    }
    @unsafe.Pointer end = new @unsafe.Pointer(Ꮡp);
    nint n = 0;
    while (~(ж<uint16>)(uintptr)(end) != 0) {
        end = ((@unsafe.Pointer)(((uintptr)end) + @unsafe.Sizeof(p)));
        n++;
    }
    return syscall.UTF16ToString(@unsafe.Slice(Ꮡp, n));
}

public static readonly syscall.Errno ERROR_BAD_LENGTH = 24;
public static readonly syscall.Errno ERROR_SHARING_VIOLATION = 32;
public static readonly syscall.Errno ERROR_LOCK_VIOLATION = 33;
public static readonly syscall.Errno ERROR_NOT_SUPPORTED = 50;
public static readonly syscall.Errno ERROR_CALL_NOT_IMPLEMENTED = 120;
public static readonly syscall.Errno ERROR_INVALID_NAME = 123;
public static readonly syscall.Errno ERROR_LOCK_FAILED = 167;
public static readonly syscall.Errno ERROR_NO_UNICODE_TRANSLATION = 1113;

public static readonly UntypedInt GAA_FLAG_INCLUDE_PREFIX = /* 0x00000010 */ 16;
public static readonly UntypedInt GAA_FLAG_INCLUDE_GATEWAYS = /* 0x0080 */ 128;

public static readonly UntypedInt IF_TYPE_OTHER = 1;
public static readonly UntypedInt IF_TYPE_ETHERNET_CSMACD = 6;
public static readonly UntypedInt IF_TYPE_ISO88025_TOKENRING = 9;
public static readonly UntypedInt IF_TYPE_PPP = 23;
public static readonly UntypedInt IF_TYPE_SOFTWARE_LOOPBACK = 24;
public static readonly UntypedInt IF_TYPE_ATM = 37;
public static readonly UntypedInt IF_TYPE_IEEE80211 = 71;
public static readonly UntypedInt IF_TYPE_TUNNEL = 131;
public static readonly UntypedInt IF_TYPE_IEEE1394 = 144;

[GoType] partial struct SocketAddress {
    public ж<syscall_package.RawSockaddrAny> Sockaddr;
    public int32 SockaddrLength;
}

[GoType] partial struct IpAdapterUnicastAddress {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterUnicastAddress> Next;
    public SocketAddress Address;
    public int32 PrefixOrigin;
    public int32 SuffixOrigin;
    public int32 DadState;
    public uint32 ValidLifetime;
    public uint32 PreferredLifetime;
    public uint32 LeaseLifetime;
    public uint8 OnLinkPrefixLength;
}

[GoType] partial struct IpAdapterAnycastAddress {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterAnycastAddress> Next;
    public SocketAddress Address;
}

[GoType] partial struct IpAdapterMulticastAddress {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterMulticastAddress> Next;
    public SocketAddress Address;
}

[GoType] partial struct IpAdapterDnsServerAdapter {
    public uint32 Length;
    public uint32 Reserved;
    public ж<IpAdapterDnsServerAdapter> Next;
    public SocketAddress Address;
}

[GoType] partial struct IpAdapterPrefix {
    public uint32 Length;
    public uint32 Flags;
    public ж<IpAdapterPrefix> Next;
    public SocketAddress Address;
    public uint32 PrefixLength;
}

[GoType] partial struct IpAdapterWinsServerAddress {
    public uint32 Length;
    public uint32 Reserved;
    public ж<IpAdapterWinsServerAddress> Next;
    public SocketAddress Address;
}

[GoType] partial struct IpAdapterGatewayAddress {
    public uint32 Length;
    public uint32 Reserved;
    public ж<IpAdapterGatewayAddress> Next;
    public SocketAddress Address;
}

[GoType] partial struct IpAdapterAddresses {
    public uint32 Length;
    public uint32 IfIndex;
    public ж<IpAdapterAddresses> Next;
    public ж<byte> AdapterName;
    public ж<IpAdapterUnicastAddress> FirstUnicastAddress;
    public ж<IpAdapterAnycastAddress> FirstAnycastAddress;
    public ж<IpAdapterMulticastAddress> FirstMulticastAddress;
    public ж<IpAdapterDnsServerAdapter> FirstDnsServerAddress;
    public ж<uint16> DnsSuffix;
    public ж<uint16> Description;
    public ж<uint16> FriendlyName;
    public array<byte> PhysicalAddress = new(syscall.MAX_ADAPTER_ADDRESS_LENGTH);
    public uint32 PhysicalAddressLength;
    public uint32 Flags;
    public uint32 Mtu;
    public uint32 IfType;
    public uint32 OperStatus;
    public uint32 Ipv6IfIndex;
    public array<uint32> ZoneIndices = new(16);
    public ж<IpAdapterPrefix> FirstPrefix;
    public uint64 TransmitLinkSpeed;
    public uint64 ReceiveLinkSpeed;
    public ж<IpAdapterWinsServerAddress> FirstWinsServerAddress;
    public ж<IpAdapterGatewayAddress> FirstGatewayAddress;
}

/* more fields might be present here. */
[GoType] partial struct SecurityAttributes {
    public uint16 Length;
    public uintptr SecurityDescriptor;
    public bool InheritHandle;
}

[GoType] partial struct FILE_BASIC_INFO {
    public int64 CreationTime;
    public int64 LastAccessTime;
    public int64 LastWriteTime;
    public int64 ChangedTime;
    public uint32 FileAttributes;
    // Pad out to 8-byte alignment.
    //
    // Without this padding, TestChmod fails due to an argument validation error
    // in SetFileInformationByHandle on windows/386.
    //
    // https://learn.microsoft.com/en-us/cpp/build/reference/zp-struct-member-alignment?view=msvc-170
    // says that “The C/C++ headers in the Windows SDK assume the platform's
    // default alignment is used.” What we see here is padding rather than
    // alignment, but maybe it is related.
    internal uint32 _;
}

public static readonly UntypedInt IfOperStatusUp = 1;
public static readonly UntypedInt IfOperStatusDown = 2;
public static readonly UntypedInt IfOperStatusTesting = 3;
public static readonly UntypedInt IfOperStatusUnknown = 4;
public static readonly UntypedInt IfOperStatusDormant = 5;
public static readonly UntypedInt IfOperStatusNotPresent = 6;
public static readonly UntypedInt IfOperStatusLowerLayerDown = 7;

//sys	GetAdaptersAddresses(family uint32, flags uint32, reserved uintptr, adapterAddresses *IpAdapterAddresses, sizePointer *uint32) (errcode error) = iphlpapi.GetAdaptersAddresses
//sys	GetComputerNameEx(nameformat uint32, buf *uint16, n *uint32) (err error) = GetComputerNameExW
//sys	MoveFileEx(from *uint16, to *uint16, flags uint32) (err error) = MoveFileExW
//sys	GetModuleFileName(module syscall.Handle, fn *uint16, len uint32) (n uint32, err error) = kernel32.GetModuleFileNameW
//sys	SetFileInformationByHandle(handle syscall.Handle, fileInformationClass uint32, buf unsafe.Pointer, bufsize uint32) (err error) = kernel32.SetFileInformationByHandle
//sys	VirtualQuery(address uintptr, buffer *MemoryBasicInformation, length uintptr) (err error) = kernel32.VirtualQuery
//sys	GetTempPath2(buflen uint32, buf *uint16) (n uint32, err error) = GetTempPath2W
public static readonly UntypedInt TH32CS_SNAPMODULE = /* 0x08 */ 8;
public static readonly UntypedInt TH32CS_SNAPMODULE32 = /* 0x10 */ 16;

public static readonly UntypedInt MAX_MODULE_NAME32 = 255;

[GoType] partial struct ModuleEntry32 {
    public uint32 Size;
    public uint32 ModuleID;
    public uint32 ProcessID;
    public uint32 GlblcntUsage;
    public uint32 ProccntUsage;
    public uintptr ModBaseAddr;
    public uint32 ModBaseSize;
    public syscall_package.ΔHandle ModuleHandle;
    public array<uint16> Module = new(MAX_MODULE_NAME32 + 1);
    public array<uint16> ExePath = new(syscall.MAX_PATH);
}

public const uintptr SizeofModuleEntry32 = /* unsafe.Sizeof(ModuleEntry32{}) */ 1080;

//sys	Module32First(snapshot syscall.Handle, moduleEntry *ModuleEntry32) (err error) = kernel32.Module32FirstW
//sys	Module32Next(snapshot syscall.Handle, moduleEntry *ModuleEntry32) (err error) = kernel32.Module32NextW
public static readonly UntypedInt WSA_FLAG_OVERLAPPED = /* 0x01 */ 1;
public static readonly UntypedInt WSA_FLAG_NO_HANDLE_INHERIT = /* 0x80 */ 128;
public static readonly syscall.Errno WSAEINVAL = 10022;
public static readonly syscall.Errno WSAEMSGSIZE = 10040;
public static readonly syscall.Errno WSAEAFNOSUPPORT = 10047;
public static readonly UntypedInt MSG_PEEK = /* 0x2 */ 2;
public static readonly UntypedInt MSG_TRUNC = /* 0x0100 */ 256;
public static readonly UntypedInt MSG_CTRUNC = /* 0x0200 */ 512;
internal const uintptr socket_error = /* uintptr(^uint32(0)) */ 4294967295;

public static syscall.GUID WSAID_WSASENDMSG = new syscall.GUID(
    Data1: (nint)2755782418L,
    Data2: 30031,
    Data3: 17354,
    Data4: new byte[]{132, 167, 13, 238, 68, 207, 96, 109}.array()
);

public static syscall.GUID WSAID_WSARECVMSG = new syscall.GUID(
    Data1: (nint)4136228808L,
    Data2: 28447,
    Data3: 17259,
    Data4: new byte[]{138, 83, 229, 79, 227, 81, 195, 34}.array()
);


[GoType("dyn")] partial struct sendRecvMsgFuncᴛ1 {
    internal sync_package.Once once;
    internal uintptr sendAddr;
    internal uintptr recvAddr;
    internal error err;
}
internal static sendRecvMsgFuncᴛ1 sendRecvMsgFunc;

[GoType] partial struct WSAMsg {
    public syscall_package.Pointer Name;
    public int32 Namelen;
    public ж<syscall_package.WSABuf> Buffers;
    public uint32 BufferCount;
    public syscall_package.WSABuf Control;
    public uint32 Flags;
}

//sys	WSASocket(af int32, typ int32, protocol int32, protinfo *syscall.WSAProtocolInfo, group uint32, flags uint32) (handle syscall.Handle, err error) [failretval==syscall.InvalidHandle] = ws2_32.WSASocketW
//sys	WSAGetOverlappedResult(h syscall.Handle, o *syscall.Overlapped, bytes *uint32, wait bool, flags *uint32) (err error) = ws2_32.WSAGetOverlappedResult
internal static error loadWSASendRecvMsg() => func((defer, _) => {
    sendRecvMsgFunc.once.Do(
    var WSAID_WSARECVMSGʗ2 = WSAID_WSARECVMSG;
    var WSAID_WSASENDMSGʗ2 = WSAID_WSASENDMSG;
    var sendRecvMsgFuncʗ2 = sendRecvMsgFunc;
    () => {
        syscallꓸHandle s = default!;
        (s, sendRecvMsgFuncʗ2.err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, syscall.IPPROTO_UDP);
        if (sendRecvMsgFuncʗ2.err != default!) {
            return;
        }
        deferǃ(syscall.CloseHandle, s, defer);
        ref var n = ref heap(new uint32(), out var Ꮡn);
        sendRecvMsgFunc.err = syscall.WSAIoctl(s,
            syscall.SIO_GET_EXTENSION_FUNCTION_POINTER,
            (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(WSAID_WSARECVMSG))),
            ((uint32)@unsafe.Sizeof(WSAID_WSARECVMSG)),
            (ж<byte>)(uintptr)(((@unsafe.Pointer)(ᏑsendRecvMsgFunc.of(sendRecvMsgFuncᴛ1.ᏑrecvAddr)))),
            ((uint32)@unsafe.Sizeof(sendRecvMsgFunc.recvAddr)),
            Ꮡn, nil, 0);
        if (sendRecvMsgFunc.err != default!) {
            return;
        }
        sendRecvMsgFunc.err = syscall.WSAIoctl(s,
            syscall.SIO_GET_EXTENSION_FUNCTION_POINTER,
            (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(WSAID_WSASENDMSG))),
            ((uint32)@unsafe.Sizeof(WSAID_WSASENDMSG)),
            (ж<byte>)(uintptr)(((@unsafe.Pointer)(ᏑsendRecvMsgFunc.of(sendRecvMsgFuncᴛ1.ᏑsendAddr)))),
            ((uint32)@unsafe.Sizeof(sendRecvMsgFunc.sendAddr)),
            Ꮡn, nil, 0);
    });
    return sendRecvMsgFunc.err;
});

public static error WSASendMsg(syscallꓸHandle fd, ж<WSAMsg> Ꮡmsg, uint32 flags, ж<uint32> ᏑbytesSent, ж<syscall.Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
    ref var msg = ref Ꮡmsg.val;
    ref var bytesSent = ref ᏑbytesSent.val;
    ref var overlapped = ref Ꮡoverlapped.val;
    ref var croutine = ref Ꮡcroutine.val;

    var err = loadWSASendRecvMsg();
    if (err != default!) {
        return err;
    }
    var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.sendAddr, 6, ((uintptr)fd), ((uintptr)new @unsafe.Pointer(Ꮡmsg)), ((uintptr)flags), ((uintptr)new @unsafe.Pointer(ᏑbytesSent)), ((uintptr)new @unsafe.Pointer(Ꮡoverlapped)), ((uintptr)new @unsafe.Pointer(Ꮡcroutine)));
    if (r1 == socket_error) {
        if (e1 != 0){
            err = errnoErr(e1);
        } else {
            err = syscall.EINVAL;
        }
    }
    return err;
}

public static error WSARecvMsg(syscallꓸHandle fd, ж<WSAMsg> Ꮡmsg, ж<uint32> ᏑbytesReceived, ж<syscall.Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
    ref var msg = ref Ꮡmsg.val;
    ref var bytesReceived = ref ᏑbytesReceived.val;
    ref var overlapped = ref Ꮡoverlapped.val;
    ref var croutine = ref Ꮡcroutine.val;

    var err = loadWSASendRecvMsg();
    if (err != default!) {
        return err;
    }
    var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.recvAddr, 5, ((uintptr)fd), ((uintptr)new @unsafe.Pointer(Ꮡmsg)), ((uintptr)new @unsafe.Pointer(ᏑbytesReceived)), ((uintptr)new @unsafe.Pointer(Ꮡoverlapped)), ((uintptr)new @unsafe.Pointer(Ꮡcroutine)), 0);
    if (r1 == socket_error) {
        if (e1 != 0){
            err = errnoErr(e1);
        } else {
            err = syscall.EINVAL;
        }
    }
    return err;
}

public static readonly UntypedInt ComputerNameNetBIOS = 0;
public static readonly UntypedInt ComputerNameDnsHostname = 1;
public static readonly UntypedInt ComputerNameDnsDomain = 2;
public static readonly UntypedInt ComputerNameDnsFullyQualified = 3;
public static readonly UntypedInt ComputerNamePhysicalNetBIOS = 4;
public static readonly UntypedInt ComputerNamePhysicalDnsHostname = 5;
public static readonly UntypedInt ComputerNamePhysicalDnsDomain = 6;
public static readonly UntypedInt ComputerNamePhysicalDnsFullyQualified = 7;
public static readonly UntypedInt ComputerNameMax = 8;
public static readonly UntypedInt MOVEFILE_REPLACE_EXISTING = /* 0x1 */ 1;
public static readonly UntypedInt MOVEFILE_COPY_ALLOWED = /* 0x2 */ 2;
public static readonly UntypedInt MOVEFILE_DELAY_UNTIL_REBOOT = /* 0x4 */ 4;
public static readonly UntypedInt MOVEFILE_WRITE_THROUGH = /* 0x8 */ 8;
public static readonly UntypedInt MOVEFILE_CREATE_HARDLINK = /* 0x10 */ 16;
public static readonly UntypedInt MOVEFILE_FAIL_IF_NOT_TRACKABLE = /* 0x20 */ 32;

public static error Rename(@string oldpath, @string newpath) {
    (from, err) = syscall.UTF16PtrFromString(oldpath);
    if (err != default!) {
        return err;
    }
    (to, err) = syscall.UTF16PtrFromString(newpath);
    if (err != default!) {
        return err;
    }
    return MoveFileEx(from, to, MOVEFILE_REPLACE_EXISTING);
}

//sys LockFileEx(file syscall.Handle, flags uint32, reserved uint32, bytesLow uint32, bytesHigh uint32, overlapped *syscall.Overlapped) (err error) = kernel32.LockFileEx
//sys UnlockFileEx(file syscall.Handle, reserved uint32, bytesLow uint32, bytesHigh uint32, overlapped *syscall.Overlapped) (err error) = kernel32.UnlockFileEx
public static readonly UntypedInt LOCKFILE_FAIL_IMMEDIATELY = /* 0x00000001 */ 1;
public static readonly UntypedInt LOCKFILE_EXCLUSIVE_LOCK = /* 0x00000002 */ 2;

public static readonly UntypedInt MB_ERR_INVALID_CHARS = 8;

//sys	GetACP() (acp uint32) = kernel32.GetACP
//sys	GetConsoleCP() (ccp uint32) = kernel32.GetConsoleCP
//sys	MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar
//sys	GetCurrentThread() (pseudoHandle syscall.Handle, err error) = kernel32.GetCurrentThread

// Constants from lmshare.h
public static readonly UntypedInt STYPE_DISKTREE = /* 0x00 */ 0;

public static readonly UntypedInt STYPE_TEMPORARY = /* 0x40000000 */ 1073741824;

[GoType] partial struct SHARE_INFO_2 {
    public ж<uint16> Netname;
    public uint32 Type;
    public ж<uint16> Remark;
    public uint32 Permissions;
    public uint32 MaxUses;
    public uint32 CurrentUses;
    public ж<uint16> Path;
    public ж<uint16> Passwd;
}

//sys  NetShareAdd(serverName *uint16, level uint32, buf *byte, parmErr *uint16) (neterr error) = netapi32.NetShareAdd
//sys  NetShareDel(serverName *uint16, netName *uint16, reserved uint32) (neterr error) = netapi32.NetShareDel
public static readonly UntypedInt FILE_NAME_NORMALIZED = /* 0x0 */ 0;
public static readonly UntypedInt FILE_NAME_OPENED = /* 0x8 */ 8;
public static readonly UntypedInt VOLUME_NAME_DOS = /* 0x0 */ 0;
public static readonly UntypedInt VOLUME_NAME_GUID = /* 0x1 */ 1;
public static readonly UntypedInt VOLUME_NAME_NONE = /* 0x4 */ 4;
public static readonly UntypedInt VOLUME_NAME_NT = /* 0x2 */ 2;

//sys	GetFinalPathNameByHandle(file syscall.Handle, filePath *uint16, filePathSize uint32, flags uint32) (n uint32, err error) = kernel32.GetFinalPathNameByHandleW
public static error ErrorLoadingGetTempPath2() {
    return procGetTempPath2W.Find();
}

//sys	CreateEnvironmentBlock(block **uint16, token syscall.Token, inheritExisting bool) (err error) = userenv.CreateEnvironmentBlock
//sys	DestroyEnvironmentBlock(block *uint16) (err error) = userenv.DestroyEnvironmentBlock
//sys	CreateEvent(eventAttrs *SecurityAttributes, manualReset uint32, initialState uint32, name *uint16) (handle syscall.Handle, err error) = kernel32.CreateEventW
//sys	ProcessPrng(buf []byte) (err error) = bcryptprimitives.ProcessPrng
[GoType] partial struct FILE_ID_BOTH_DIR_INFO {
    public uint32 NextEntryOffset;
    public uint32 FileIndex;
    public syscall_package.Filetime CreationTime;
    public syscall_package.Filetime LastAccessTime;
    public syscall_package.Filetime LastWriteTime;
    public syscall_package.Filetime ChangeTime;
    public uint64 EndOfFile;
    public uint64 AllocationSize;
    public uint32 FileAttributes;
    public uint32 FileNameLength;
    public uint32 EaSize;
    public uint32 ShortNameLength;
    public array<uint16> ShortName = new(12);
    public uint64 FileID;
    public array<uint16> FileName = new(1);
}

[GoType] partial struct FILE_FULL_DIR_INFO {
    public uint32 NextEntryOffset;
    public uint32 FileIndex;
    public syscall_package.Filetime CreationTime;
    public syscall_package.Filetime LastAccessTime;
    public syscall_package.Filetime LastWriteTime;
    public syscall_package.Filetime ChangeTime;
    public uint64 EndOfFile;
    public uint64 AllocationSize;
    public uint32 FileAttributes;
    public uint32 FileNameLength;
    public uint32 EaSize;
    public array<uint16> FileName = new(1);
}

//sys	GetVolumeInformationByHandle(file syscall.Handle, volumeNameBuffer *uint16, volumeNameSize uint32, volumeNameSerialNumber *uint32, maximumComponentLength *uint32, fileSystemFlags *uint32, fileSystemNameBuffer *uint16, fileSystemNameSize uint32) (err error) = GetVolumeInformationByHandleW
//sys	GetVolumeNameForVolumeMountPoint(volumeMountPoint *uint16, volumeName *uint16, bufferlength uint32) (err error) = GetVolumeNameForVolumeMountPointW
//sys	RtlLookupFunctionEntry(pc uintptr, baseAddress *uintptr, table *byte) (ret uintptr) = kernel32.RtlLookupFunctionEntry
//sys	RtlVirtualUnwind(handlerType uint32, baseAddress uintptr, pc uintptr, entry uintptr, ctxt uintptr, data *uintptr, frame *uintptr, ctxptrs *byte) (ret uintptr) = kernel32.RtlVirtualUnwind
[GoType] partial struct SERVICE_STATUS {
    public uint32 ServiceType;
    public uint32 CurrentState;
    public uint32 ControlsAccepted;
    public uint32 Win32ExitCode;
    public uint32 ServiceSpecificExitCode;
    public uint32 CheckPoint;
    public uint32 WaitHint;
}

public static readonly UntypedInt SERVICE_RUNNING = 4;
public static readonly UntypedInt SERVICE_QUERY_STATUS = 4;

//sys    OpenService(mgr syscall.Handle, serviceName *uint16, access uint32) (handle syscall.Handle, err error) = advapi32.OpenServiceW
//sys	QueryServiceStatus(hService syscall.Handle, lpServiceStatus *SERVICE_STATUS) (err error)  = advapi32.QueryServiceStatus
//sys    OpenSCManager(machineName *uint16, databaseName *uint16, access uint32) (handle syscall.Handle, err error)  [failretval==0] = advapi32.OpenSCManagerW
public static (@string, error) FinalPath(syscallꓸHandle h, uint32 flags) {
    var buf = new slice<uint16>(100);
    while (ᐧ) {
        var (n, err) = GetFinalPathNameByHandle(h, Ꮡ(buf, 0), ((uint32)len(buf)), flags);
        if (err != default!) {
            return ("", err);
        }
        if (n < ((uint32)len(buf))) {
            break;
        }
        buf = new slice<uint16>(n);
    }
    return (syscall.UTF16ToString(buf), default!);
}

// QueryPerformanceCounter retrieves the current value of performance counter.
//
//go:linkname QueryPerformanceCounter
public static partial int64 QueryPerformanceCounter();

// Implemented in runtime package.

// QueryPerformanceFrequency retrieves the frequency of the performance counter.
// The returned value is represented as counts per second.
//
//go:linkname QueryPerformanceFrequency
public static partial int64 QueryPerformanceFrequency();

// Implemented in runtime package.

} // end windows_package
