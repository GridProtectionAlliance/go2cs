// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 August 29 08:22:33 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Go\src\internal\syscall\windows\syscall_windows.go
using sync = go.sync_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class windows_package
    {
        public static readonly syscall.Errno ERROR_SHARING_VIOLATION = 32L;
        public static readonly syscall.Errno ERROR_NO_UNICODE_TRANSLATION = 1113L;

        public static readonly ulong GAA_FLAG_INCLUDE_PREFIX = 0x00000010UL;



        public static readonly long IF_TYPE_OTHER = 1L;
        public static readonly long IF_TYPE_ETHERNET_CSMACD = 6L;
        public static readonly long IF_TYPE_ISO88025_TOKENRING = 9L;
        public static readonly long IF_TYPE_PPP = 23L;
        public static readonly long IF_TYPE_SOFTWARE_LOOPBACK = 24L;
        public static readonly long IF_TYPE_ATM = 37L;
        public static readonly long IF_TYPE_IEEE80211 = 71L;
        public static readonly long IF_TYPE_TUNNEL = 131L;
        public static readonly long IF_TYPE_IEEE1394 = 144L;

        public partial struct SocketAddress
        {
            public ptr<syscall.RawSockaddrAny> Sockaddr;
            public int SockaddrLength;
        }

        public partial struct IpAdapterUnicastAddress
        {
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

        public partial struct IpAdapterAnycastAddress
        {
            public uint Length;
            public uint Flags;
            public ptr<IpAdapterAnycastAddress> Next;
            public SocketAddress Address;
        }

        public partial struct IpAdapterMulticastAddress
        {
            public uint Length;
            public uint Flags;
            public ptr<IpAdapterMulticastAddress> Next;
            public SocketAddress Address;
        }

        public partial struct IpAdapterDnsServerAdapter
        {
            public uint Length;
            public uint Reserved;
            public ptr<IpAdapterDnsServerAdapter> Next;
            public SocketAddress Address;
        }

        public partial struct IpAdapterPrefix
        {
            public uint Length;
            public uint Flags;
            public ptr<IpAdapterPrefix> Next;
            public SocketAddress Address;
            public uint PrefixLength;
        }

        public partial struct IpAdapterAddresses
        {
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

        public static readonly long IfOperStatusUp = 1L;
        public static readonly long IfOperStatusDown = 2L;
        public static readonly long IfOperStatusTesting = 3L;
        public static readonly long IfOperStatusUnknown = 4L;
        public static readonly long IfOperStatusDormant = 5L;
        public static readonly long IfOperStatusNotPresent = 6L;
        public static readonly long IfOperStatusLowerLayerDown = 7L;

        //sys    GetAdaptersAddresses(family uint32, flags uint32, reserved uintptr, adapterAddresses *IpAdapterAddresses, sizePointer *uint32) (errcode error) = iphlpapi.GetAdaptersAddresses
        //sys    GetComputerNameEx(nameformat uint32, buf *uint16, n *uint32) (err error) = GetComputerNameExW
        //sys    MoveFileEx(from *uint16, to *uint16, flags uint32) (err error) = MoveFileExW
        //sys    GetModuleFileName(module syscall.Handle, fn *uint16, len uint32) (n uint32, err error) = kernel32.GetModuleFileNameW

        public static readonly ulong WSA_FLAG_OVERLAPPED = 0x01UL;
        public static readonly ulong WSA_FLAG_NO_HANDLE_INHERIT = 0x80UL;

        public static readonly syscall.Errno WSAEMSGSIZE = 10040L;

        public static readonly ulong MSG_TRUNC = 0x0100UL;
        public static readonly ulong MSG_CTRUNC = 0x0200UL;

        private static readonly var socket_error = uintptr(~uint32(0L));

        public static syscall.GUID WSAID_WSASENDMSG = new syscall.GUID(Data1:0xa441e712,Data2:0x754f,Data3:0x43ca,Data4:[8]byte{0x84,0xa7,0x0d,0xee,0x44,0xcf,0x60,0x6d},);

        public static syscall.GUID WSAID_WSARECVMSG = new syscall.GUID(Data1:0xf689d7c8,Data2:0x6f1f,Data3:0x436b,Data4:[8]byte{0x8a,0x53,0xe5,0x4f,0xe3,0x51,0xc3,0x22},);

        private static var sendRecvMsgFunc = default;

        public partial struct WSAMsg
        {
            public ptr<syscall.RawSockaddrAny> Name;
            public int Namelen;
            public ptr<syscall.WSABuf> Buffers;
            public uint BufferCount;
            public syscall.WSABuf Control;
            public uint Flags;
        }

        //sys    WSASocket(af int32, typ int32, protocol int32, protinfo *syscall.WSAProtocolInfo, group uint32, flags uint32) (handle syscall.Handle, err error) [failretval==syscall.InvalidHandle] = ws2_32.WSASocketW

        private static error loadWSASendRecvMsg() => func((defer, _, __) =>
        {
            sendRecvMsgFunc.once.Do(() =>
            {
                syscall.Handle s = default;
                s, sendRecvMsgFunc.err = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, syscall.IPPROTO_UDP);
                if (sendRecvMsgFunc.err != null)
                {
                    return;
                }
                defer(syscall.CloseHandle(s));
                uint n = default;
                sendRecvMsgFunc.err = syscall.WSAIoctl(s, syscall.SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.Value)(@unsafe.Pointer(ref WSAID_WSARECVMSG)), uint32(@unsafe.Sizeof(WSAID_WSARECVMSG)), (byte.Value)(@unsafe.Pointer(ref sendRecvMsgFunc.recvAddr)), uint32(@unsafe.Sizeof(sendRecvMsgFunc.recvAddr)), ref n, null, 0L);
                if (sendRecvMsgFunc.err != null)
                {
                    return;
                }
                sendRecvMsgFunc.err = syscall.WSAIoctl(s, syscall.SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.Value)(@unsafe.Pointer(ref WSAID_WSASENDMSG)), uint32(@unsafe.Sizeof(WSAID_WSASENDMSG)), (byte.Value)(@unsafe.Pointer(ref sendRecvMsgFunc.sendAddr)), uint32(@unsafe.Sizeof(sendRecvMsgFunc.sendAddr)), ref n, null, 0L);
            });
            return error.As(sendRecvMsgFunc.err);
        });

        public static error WSASendMsg(syscall.Handle fd, ref WSAMsg msg, uint flags, ref uint bytesSent, ref syscall.Overlapped overlapped, ref byte croutine)
        {
            var err = loadWSASendRecvMsg();
            if (err != null)
            {
                return error.As(err);
            }
            var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.sendAddr, 6L, uintptr(fd), uintptr(@unsafe.Pointer(msg)), uintptr(flags), uintptr(@unsafe.Pointer(bytesSent)), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)));
            if (r1 == socket_error)
            {
                if (e1 != 0L)
                {
                    err = errnoErr(e1);
                }
                else
                {
                    err = syscall.EINVAL;
                }
            }
            return error.As(err);
        }

        public static error WSARecvMsg(syscall.Handle fd, ref WSAMsg msg, ref uint bytesReceived, ref syscall.Overlapped overlapped, ref byte croutine)
        {
            var err = loadWSASendRecvMsg();
            if (err != null)
            {
                return error.As(err);
            }
            var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.recvAddr, 5L, uintptr(fd), uintptr(@unsafe.Pointer(msg)), uintptr(@unsafe.Pointer(bytesReceived)), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)), 0L);
            if (r1 == socket_error)
            {
                if (e1 != 0L)
                {
                    err = errnoErr(e1);
                }
                else
                {
                    err = syscall.EINVAL;
                }
            }
            return error.As(err);
        }

        public static readonly long ComputerNameNetBIOS = 0L;
        public static readonly long ComputerNameDnsHostname = 1L;
        public static readonly long ComputerNameDnsDomain = 2L;
        public static readonly long ComputerNameDnsFullyQualified = 3L;
        public static readonly long ComputerNamePhysicalNetBIOS = 4L;
        public static readonly long ComputerNamePhysicalDnsHostname = 5L;
        public static readonly long ComputerNamePhysicalDnsDomain = 6L;
        public static readonly long ComputerNamePhysicalDnsFullyQualified = 7L;
        public static readonly long ComputerNameMax = 8L;

        public static readonly ulong MOVEFILE_REPLACE_EXISTING = 0x1UL;
        public static readonly ulong MOVEFILE_COPY_ALLOWED = 0x2UL;
        public static readonly ulong MOVEFILE_DELAY_UNTIL_REBOOT = 0x4UL;
        public static readonly ulong MOVEFILE_WRITE_THROUGH = 0x8UL;
        public static readonly ulong MOVEFILE_CREATE_HARDLINK = 0x10UL;
        public static readonly ulong MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x20UL;

        public static error Rename(@string oldpath, @string newpath)
        {
            var (from, err) = syscall.UTF16PtrFromString(oldpath);
            if (err != null)
            {
                return error.As(err);
            }
            var (to, err) = syscall.UTF16PtrFromString(newpath);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(MoveFileEx(from, to, MOVEFILE_REPLACE_EXISTING));
        }

        public static readonly long MB_ERR_INVALID_CHARS = 8L;

        //sys    GetACP() (acp uint32) = kernel32.GetACP
        //sys    GetConsoleCP() (ccp uint32) = kernel32.GetConsoleCP
        //sys    MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar
        //sys    GetCurrentThread() (pseudoHandle syscall.Handle, err error) = kernel32.GetCurrentThread



        //sys    GetACP() (acp uint32) = kernel32.GetACP
        //sys    GetConsoleCP() (ccp uint32) = kernel32.GetConsoleCP
        //sys    MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar
        //sys    GetCurrentThread() (pseudoHandle syscall.Handle, err error) = kernel32.GetCurrentThread

        public static readonly ulong STYPE_DISKTREE = 0x00UL;



        public partial struct SHARE_INFO_2
        {
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

        public static readonly ulong FILE_NAME_NORMALIZED = 0x0UL;
        public static readonly ulong FILE_NAME_OPENED = 0x8UL;

        public static readonly ulong VOLUME_NAME_DOS = 0x0UL;
        public static readonly ulong VOLUME_NAME_GUID = 0x1UL;
        public static readonly ulong VOLUME_NAME_NONE = 0x4UL;
        public static readonly ulong VOLUME_NAME_NT = 0x2UL;

        //sys    GetFinalPathNameByHandle(file syscall.Handle, filePath *uint16, filePathSize uint32, flags uint32) (n uint32, err error) = kernel32.GetFinalPathNameByHandleW

        public static error LoadGetFinalPathNameByHandle()
        {
            return error.As(procGetFinalPathNameByHandleW.Find());
        }
    }
}}}
