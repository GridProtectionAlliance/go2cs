// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs defs_solaris.go

// package lif -- go2cs converted at 2020 August 29 10:12:19 UTC
// import "vendor/golang_org/x/net/lif" ==> using lif = go.vendor.golang_org.x.net.lif_package
// Original source: C:\Go\src\vendor\golang_org\x\net\lif\zsys_solaris_amd64.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        private static readonly ulong sysAF_UNSPEC = 0x0UL;
        private static readonly ulong sysAF_INET = 0x2UL;
        private static readonly ulong sysAF_INET6 = 0x1aUL;

        private static readonly ulong sysSOCK_DGRAM = 0x1UL;

        private partial struct sockaddrStorage
        {
            public ushort Family;
            public array<sbyte> X_ss_pad1;
            public double X_ss_align;
            public array<sbyte> X_ss_pad2;
        }

        private static readonly ulong sysLIFC_NOXMIT = 0x1UL;
        private static readonly ulong sysLIFC_EXTERNAL_SOURCE = 0x2UL;
        private static readonly ulong sysLIFC_TEMPORARY = 0x4UL;
        private static readonly ulong sysLIFC_ALLZONES = 0x8UL;
        private static readonly ulong sysLIFC_UNDER_IPMP = 0x10UL;
        private static readonly ulong sysLIFC_ENABLED = 0x20UL;

        private static readonly ulong sysSIOCGLIFADDR = -0x3f87968fUL;
        private static readonly ulong sysSIOCGLIFDSTADDR = -0x3f87968dUL;
        private static readonly ulong sysSIOCGLIFFLAGS = -0x3f87968bUL;
        private static readonly ulong sysSIOCGLIFMTU = -0x3f879686UL;
        private static readonly ulong sysSIOCGLIFNETMASK = -0x3f879683UL;
        private static readonly ulong sysSIOCGLIFMETRIC = -0x3f879681UL;
        private static readonly ulong sysSIOCGLIFNUM = -0x3ff3967eUL;
        private static readonly ulong sysSIOCGLIFINDEX = -0x3f87967bUL;
        private static readonly ulong sysSIOCGLIFSUBNET = -0x3f879676UL;
        private static readonly ulong sysSIOCGLIFLNKINFO = -0x3f879674UL;
        private static readonly ulong sysSIOCGLIFCONF = -0x3fef965bUL;
        private static readonly ulong sysSIOCGLIFHWADDR = -0x3f879640UL;

        private static readonly ulong sysIFF_UP = 0x1UL;
        private static readonly ulong sysIFF_BROADCAST = 0x2UL;
        private static readonly ulong sysIFF_DEBUG = 0x4UL;
        private static readonly ulong sysIFF_LOOPBACK = 0x8UL;
        private static readonly ulong sysIFF_POINTOPOINT = 0x10UL;
        private static readonly ulong sysIFF_NOTRAILERS = 0x20UL;
        private static readonly ulong sysIFF_RUNNING = 0x40UL;
        private static readonly ulong sysIFF_NOARP = 0x80UL;
        private static readonly ulong sysIFF_PROMISC = 0x100UL;
        private static readonly ulong sysIFF_ALLMULTI = 0x200UL;
        private static readonly ulong sysIFF_INTELLIGENT = 0x400UL;
        private static readonly ulong sysIFF_MULTICAST = 0x800UL;
        private static readonly ulong sysIFF_MULTI_BCAST = 0x1000UL;
        private static readonly ulong sysIFF_UNNUMBERED = 0x2000UL;
        private static readonly ulong sysIFF_PRIVATE = 0x8000UL;

        private static readonly ulong sizeofLifnum = 0xcUL;
        private static readonly ulong sizeofLifreq = 0x178UL;
        private static readonly ulong sizeofLifconf = 0x18UL;
        private static readonly ulong sizeofLifIfinfoReq = 0x10UL;

        private partial struct lifnum
        {
            public ushort Family;
            public array<byte> Pad_cgo_0;
            public int Flags;
            public int Count;
        }

        private partial struct lifreq
        {
            public array<sbyte> Name;
            public array<byte> Lifru1;
            public uint Type;
            public array<byte> Lifru;
        }

        private partial struct lifconf
        {
            public ushort Family;
            public array<byte> Pad_cgo_0;
            public int Flags;
            public int Len;
            public array<byte> Pad_cgo_1;
            public array<byte> Lifcu;
        }

        private partial struct lifIfinfoReq
        {
            public byte Maxhops;
            public array<byte> Pad_cgo_0;
            public uint Reachtime;
            public uint Reachretrans;
            public uint Maxmtu;
        }

        private static readonly ulong sysIFT_IPV4 = 0xc8UL;
        private static readonly ulong sysIFT_IPV6 = 0xc9UL;
        private static readonly ulong sysIFT_6TO4 = 0xcaUL;
    }
}}}}}
