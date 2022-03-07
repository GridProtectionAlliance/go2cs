// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:27:19 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\types_windows.go


namespace go;

public static partial class syscall_package {

 
// Windows errors.
public static readonly Errno ERROR_FILE_NOT_FOUND = 2;
public static readonly Errno ERROR_PATH_NOT_FOUND = 3;
public static readonly Errno ERROR_ACCESS_DENIED = 5;
public static readonly Errno ERROR_NO_MORE_FILES = 18;
public static readonly Errno ERROR_HANDLE_EOF = 38;
public static readonly Errno ERROR_NETNAME_DELETED = 64;
public static readonly Errno ERROR_FILE_EXISTS = 80;
public static readonly Errno ERROR_BROKEN_PIPE = 109;
public static readonly Errno ERROR_BUFFER_OVERFLOW = 111;
public static readonly Errno ERROR_INSUFFICIENT_BUFFER = 122;
public static readonly Errno ERROR_MOD_NOT_FOUND = 126;
public static readonly Errno ERROR_PROC_NOT_FOUND = 127;
public static readonly Errno ERROR_DIR_NOT_EMPTY = 145;
public static readonly Errno ERROR_ALREADY_EXISTS = 183;
public static readonly Errno ERROR_ENVVAR_NOT_FOUND = 203;
public static readonly Errno ERROR_MORE_DATA = 234;
public static readonly Errno ERROR_OPERATION_ABORTED = 995;
public static readonly Errno ERROR_IO_PENDING = 997;
public static readonly Errno ERROR_NOT_FOUND = 1168;
public static readonly Errno ERROR_PRIVILEGE_NOT_HELD = 1314;
public static readonly Errno WSAEACCES = 10013;
public static readonly Errno WSAECONNABORTED = 10053;
public static readonly Errno WSAECONNRESET = 10054;


 
// Invented values to support what package os expects.
public static readonly nuint O_RDONLY = 0x00000;
public static readonly nuint O_WRONLY = 0x00001;
public static readonly nuint O_RDWR = 0x00002;
public static readonly nuint O_CREAT = 0x00040;
public static readonly nuint O_EXCL = 0x00080;
public static readonly nuint O_NOCTTY = 0x00100;
public static readonly nuint O_TRUNC = 0x00200;
public static readonly nuint O_NONBLOCK = 0x00800;
public static readonly nuint O_APPEND = 0x00400;
public static readonly nuint O_SYNC = 0x01000;
public static readonly nuint O_ASYNC = 0x02000;
public static readonly nuint O_CLOEXEC = 0x80000;


 
// More invented values for signals
public static readonly var SIGHUP = Signal(0x1);
public static readonly var SIGINT = Signal(0x2);
public static readonly var SIGQUIT = Signal(0x3);
public static readonly var SIGILL = Signal(0x4);
public static readonly var SIGTRAP = Signal(0x5);
public static readonly var SIGABRT = Signal(0x6);
public static readonly var SIGBUS = Signal(0x7);
public static readonly var SIGFPE = Signal(0x8);
public static readonly var SIGKILL = Signal(0x9);
public static readonly var SIGSEGV = Signal(0xb);
public static readonly var SIGPIPE = Signal(0xd);
public static readonly var SIGALRM = Signal(0xe);
public static readonly var SIGTERM = Signal(0xf);


private static array<@string> signals = new array<@string>(InitKeyedValues<@string>((1, "hangup"), (2, "interrupt"), (3, "quit"), (4, "illegal instruction"), (5, "trace/breakpoint trap"), (6, "aborted"), (7, "bus error"), (8, "floating point exception"), (9, "killed"), (10, "user defined signal 1"), (11, "segmentation fault"), (12, "user defined signal 2"), (13, "broken pipe"), (14, "alarm clock"), (15, "terminated")));

public static readonly nuint GENERIC_READ = 0x80000000;
public static readonly nuint GENERIC_WRITE = 0x40000000;
public static readonly nuint GENERIC_EXECUTE = 0x20000000;
public static readonly nuint GENERIC_ALL = 0x10000000;

public static readonly nuint FILE_LIST_DIRECTORY = 0x00000001;
public static readonly nuint FILE_APPEND_DATA = 0x00000004;
public static readonly nuint FILE_WRITE_ATTRIBUTES = 0x00000100;

public static readonly nuint FILE_SHARE_READ = 0x00000001;
public static readonly nuint FILE_SHARE_WRITE = 0x00000002;
public static readonly nuint FILE_SHARE_DELETE = 0x00000004;
public static readonly nuint FILE_ATTRIBUTE_READONLY = 0x00000001;
public static readonly nuint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
public static readonly nuint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
public static readonly nuint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
public static readonly nuint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
public static readonly nuint FILE_ATTRIBUTE_NORMAL = 0x00000080;
public static readonly nuint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;

public static readonly nuint INVALID_FILE_ATTRIBUTES = 0xffffffff;

public static readonly nint CREATE_NEW = 1;
public static readonly nint CREATE_ALWAYS = 2;
public static readonly nint OPEN_EXISTING = 3;
public static readonly nint OPEN_ALWAYS = 4;
public static readonly nint TRUNCATE_EXISTING = 5;

public static readonly nuint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
public static readonly nuint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
public static readonly nuint FILE_FLAG_OVERLAPPED = 0x40000000;

public static readonly nuint HANDLE_FLAG_INHERIT = 0x00000001;
public static readonly nuint STARTF_USESTDHANDLES = 0x00000100;
public static readonly nuint STARTF_USESHOWWINDOW = 0x00000001;
public static readonly nuint DUPLICATE_CLOSE_SOURCE = 0x00000001;
public static readonly nuint DUPLICATE_SAME_ACCESS = 0x00000002;

public static readonly nint STD_INPUT_HANDLE = -10;
public static readonly nint STD_OUTPUT_HANDLE = -11;
public static readonly nint STD_ERROR_HANDLE = -12;

public static readonly nint FILE_BEGIN = 0;
public static readonly nint FILE_CURRENT = 1;
public static readonly nint FILE_END = 2;

public static readonly nuint LANG_ENGLISH = 0x09;
public static readonly nuint SUBLANG_ENGLISH_US = 0x01;

public static readonly nint FORMAT_MESSAGE_ALLOCATE_BUFFER = 256;
public static readonly nint FORMAT_MESSAGE_IGNORE_INSERTS = 512;
public static readonly nint FORMAT_MESSAGE_FROM_STRING = 1024;
public static readonly nint FORMAT_MESSAGE_FROM_HMODULE = 2048;
public static readonly nint FORMAT_MESSAGE_FROM_SYSTEM = 4096;
public static readonly nint FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;
public static readonly nint FORMAT_MESSAGE_MAX_WIDTH_MASK = 255;

public static readonly nint MAX_PATH = 260;
public static readonly nint MAX_LONG_PATH = 32768;

public static readonly nint MAX_COMPUTERNAME_LENGTH = 15;

public static readonly nint TIME_ZONE_ID_UNKNOWN = 0;
public static readonly nint TIME_ZONE_ID_STANDARD = 1;

public static readonly nint TIME_ZONE_ID_DAYLIGHT = 2;
public static readonly nint IGNORE = 0;
public static readonly nuint INFINITE = 0xffffffff;

public static readonly nint WAIT_TIMEOUT = 258;
public static readonly nuint WAIT_ABANDONED = 0x00000080;
public static readonly nuint WAIT_OBJECT_0 = 0x00000000;
public static readonly nuint WAIT_FAILED = 0xFFFFFFFF;

public static readonly nuint CREATE_NEW_PROCESS_GROUP = 0x00000200;
public static readonly nuint CREATE_UNICODE_ENVIRONMENT = 0x00000400;

public static readonly nint PROCESS_TERMINATE = 1;
public static readonly nuint PROCESS_QUERY_INFORMATION = 0x00000400;
public static readonly nuint SYNCHRONIZE = 0x00100000;

public static readonly nuint PAGE_READONLY = 0x02;
public static readonly nuint PAGE_READWRITE = 0x04;
public static readonly nuint PAGE_WRITECOPY = 0x08;
public static readonly nuint PAGE_EXECUTE_READ = 0x20;
public static readonly nuint PAGE_EXECUTE_READWRITE = 0x40;
public static readonly nuint PAGE_EXECUTE_WRITECOPY = 0x80;

public static readonly nuint FILE_MAP_COPY = 0x01;
public static readonly nuint FILE_MAP_WRITE = 0x02;
public static readonly nuint FILE_MAP_READ = 0x04;
public static readonly nuint FILE_MAP_EXECUTE = 0x20;

public static readonly nint CTRL_C_EVENT = 0;
public static readonly nint CTRL_BREAK_EVENT = 1;
public static readonly nint CTRL_CLOSE_EVENT = 2;
public static readonly nint CTRL_LOGOFF_EVENT = 5;
public static readonly nint CTRL_SHUTDOWN_EVENT = 6;


 
// flags for CreateToolhelp32Snapshot
public static readonly nuint TH32CS_SNAPHEAPLIST = 0x01;
public static readonly nuint TH32CS_SNAPPROCESS = 0x02;
public static readonly nuint TH32CS_SNAPTHREAD = 0x04;
public static readonly nuint TH32CS_SNAPMODULE = 0x08;
public static readonly nuint TH32CS_SNAPMODULE32 = 0x10;
public static readonly var TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD;
public static readonly nuint TH32CS_INHERIT = 0x80000000;


 
// do not reorder
public static readonly nint FILE_NOTIFY_CHANGE_FILE_NAME = 1 << (int)(iota);
public static readonly var FILE_NOTIFY_CHANGE_DIR_NAME = 0;
public static readonly var FILE_NOTIFY_CHANGE_ATTRIBUTES = 1;
public static readonly var FILE_NOTIFY_CHANGE_SIZE = 2;
public static readonly var FILE_NOTIFY_CHANGE_LAST_WRITE = 3;
public static readonly var FILE_NOTIFY_CHANGE_LAST_ACCESS = 4;
public static readonly var FILE_NOTIFY_CHANGE_CREATION = 5;


 
// do not reorder
public static readonly var FILE_ACTION_ADDED = iota + 1;
public static readonly var FILE_ACTION_REMOVED = 0;
public static readonly var FILE_ACTION_MODIFIED = 1;
public static readonly var FILE_ACTION_RENAMED_OLD_NAME = 2;
public static readonly var FILE_ACTION_RENAMED_NEW_NAME = 3;


 
// wincrypt.h
public static readonly nint PROV_RSA_FULL = 1;
public static readonly nint PROV_RSA_SIG = 2;
public static readonly nint PROV_DSS = 3;
public static readonly nint PROV_FORTEZZA = 4;
public static readonly nint PROV_MS_EXCHANGE = 5;
public static readonly nint PROV_SSL = 6;
public static readonly nint PROV_RSA_SCHANNEL = 12;
public static readonly nint PROV_DSS_DH = 13;
public static readonly nint PROV_EC_ECDSA_SIG = 14;
public static readonly nint PROV_EC_ECNRA_SIG = 15;
public static readonly nint PROV_EC_ECDSA_FULL = 16;
public static readonly nint PROV_EC_ECNRA_FULL = 17;
public static readonly nint PROV_DH_SCHANNEL = 18;
public static readonly nint PROV_SPYRUS_LYNKS = 20;
public static readonly nint PROV_RNG = 21;
public static readonly nint PROV_INTEL_SEC = 22;
public static readonly nint PROV_REPLACE_OWF = 23;
public static readonly nint PROV_RSA_AES = 24;
public static readonly nuint CRYPT_VERIFYCONTEXT = 0xF0000000;
public static readonly nuint CRYPT_NEWKEYSET = 0x00000008;
public static readonly nuint CRYPT_DELETEKEYSET = 0x00000010;
public static readonly nuint CRYPT_MACHINE_KEYSET = 0x00000020;
public static readonly nuint CRYPT_SILENT = 0x00000040;
public static readonly nuint CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x00000080;

public static readonly nint USAGE_MATCH_TYPE_AND = 0;
public static readonly nint USAGE_MATCH_TYPE_OR = 1;

public static readonly nuint X509_ASN_ENCODING = 0x00000001;
public static readonly nuint PKCS_7_ASN_ENCODING = 0x00010000;

public static readonly nint CERT_STORE_PROV_MEMORY = 2;

public static readonly nint CERT_STORE_ADD_ALWAYS = 4;

public static readonly nuint CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 0x00000004;

public static readonly nuint CERT_TRUST_NO_ERROR = 0x00000000;
public static readonly nuint CERT_TRUST_IS_NOT_TIME_VALID = 0x00000001;
public static readonly nuint CERT_TRUST_IS_REVOKED = 0x00000004;
public static readonly nuint CERT_TRUST_IS_NOT_SIGNATURE_VALID = 0x00000008;
public static readonly nuint CERT_TRUST_IS_NOT_VALID_FOR_USAGE = 0x00000010;
public static readonly nuint CERT_TRUST_IS_UNTRUSTED_ROOT = 0x00000020;
public static readonly nuint CERT_TRUST_REVOCATION_STATUS_UNKNOWN = 0x00000040;
public static readonly nuint CERT_TRUST_IS_CYCLIC = 0x00000080;
public static readonly nuint CERT_TRUST_INVALID_EXTENSION = 0x00000100;
public static readonly nuint CERT_TRUST_INVALID_POLICY_CONSTRAINTS = 0x00000200;
public static readonly nuint CERT_TRUST_INVALID_BASIC_CONSTRAINTS = 0x00000400;
public static readonly nuint CERT_TRUST_INVALID_NAME_CONSTRAINTS = 0x00000800;
public static readonly nuint CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT = 0x00001000;
public static readonly nuint CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT = 0x00002000;
public static readonly nuint CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT = 0x00004000;
public static readonly nuint CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT = 0x00008000;
public static readonly nuint CERT_TRUST_IS_OFFLINE_REVOCATION = 0x01000000;
public static readonly nuint CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY = 0x02000000;
public static readonly nuint CERT_TRUST_IS_EXPLICIT_DISTRUST = 0x04000000;
public static readonly nuint CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT = 0x08000000;

public static readonly nint CERT_CHAIN_POLICY_BASE = 1;
public static readonly nint CERT_CHAIN_POLICY_AUTHENTICODE = 2;
public static readonly nint CERT_CHAIN_POLICY_AUTHENTICODE_TS = 3;
public static readonly nint CERT_CHAIN_POLICY_SSL = 4;
public static readonly nint CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5;
public static readonly nint CERT_CHAIN_POLICY_NT_AUTH = 6;
public static readonly nint CERT_CHAIN_POLICY_MICROSOFT_ROOT = 7;
public static readonly nint CERT_CHAIN_POLICY_EV = 8;

public static readonly nuint CERT_E_EXPIRED = 0x800B0101;
public static readonly nuint CERT_E_ROLE = 0x800B0103;
public static readonly nuint CERT_E_PURPOSE = 0x800B0106;
public static readonly nuint CERT_E_UNTRUSTEDROOT = 0x800B0109;
public static readonly nuint CERT_E_CN_NO_MATCH = 0x800B010F;

public static readonly nint AUTHTYPE_CLIENT = 1;
public static readonly nint AUTHTYPE_SERVER = 2;


public static slice<byte> OID_PKIX_KP_SERVER_AUTH = (slice<byte>)"1.3.6.1.5.5.7.3.1\x00";public static slice<byte> OID_SERVER_GATED_CRYPTO = (slice<byte>)"1.3.6.1.4.1.311.10.3.3\x00";public static slice<byte> OID_SGC_NETSCAPE = (slice<byte>)"2.16.840.1.113730.4.1\x00";

// Pointer represents a pointer to an arbitrary Windows type.
//
// Pointer-typed fields may point to one of many different types. It's
// up to the caller to provide a pointer to the appropriate type, cast
// to Pointer. The caller must obey the unsafe.Pointer rules while
// doing so.
public partial struct Timeval {
    public int Sec;
    public int Usec;
}

private static long Nanoseconds(this ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    return (int64(tv.Sec) * 1e6F + int64(tv.Usec)) * 1e3F;
}

public static Timeval NsecToTimeval(long nsec) {
    Timeval tv = default;

    tv.Sec = int32(nsec / 1e9F);
    tv.Usec = int32(nsec % 1e9F / 1e3F);
    return ;
}

public partial struct SecurityAttributes {
    public uint Length;
    public System.UIntPtr SecurityDescriptor;
    public uint InheritHandle;
}

public partial struct Overlapped {
    public System.UIntPtr Internal;
    public System.UIntPtr InternalHigh;
    public uint Offset;
    public uint OffsetHigh;
    public Handle HEvent;
}

public partial struct FileNotifyInformation {
    public uint NextEntryOffset;
    public uint Action;
    public uint FileNameLength;
    public ushort FileName;
}

public partial struct Filetime {
    public uint LowDateTime;
    public uint HighDateTime;
}

// Nanoseconds returns Filetime ft in nanoseconds
// since Epoch (00:00:00 UTC, January 1, 1970).
private static long Nanoseconds(this ptr<Filetime> _addr_ft) {
    ref Filetime ft = ref _addr_ft.val;
 
    // 100-nanosecond intervals since January 1, 1601
    var nsec = int64(ft.HighDateTime) << 32 + int64(ft.LowDateTime); 
    // change starting time to the Epoch (00:00:00 UTC, January 1, 1970)
    nsec -= (nint)116444736000000000L; 
    // convert into nanoseconds
    nsec *= 100;
    return nsec;

}

public static Filetime NsecToFiletime(long nsec) {
    Filetime ft = default;
 
    // convert into 100-nanosecond
    nsec /= 100; 
    // change starting time to January 1, 1601
    nsec += (nint)116444736000000000L; 
    // split into high / low
    ft.LowDateTime = uint32(nsec & 0xffffffff);
    ft.HighDateTime = uint32(nsec >> 32 & 0xffffffff);
    return ft;

}

public partial struct Win32finddata {
    public uint FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint FileSizeHigh;
    public uint FileSizeLow;
    public uint Reserved0;
    public uint Reserved1;
    public array<ushort> FileName;
    public array<ushort> AlternateFileName;
}

// This is the actual system call structure.
// Win32finddata is what we committed to in Go 1.
private partial struct win32finddata1 {
    public uint FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint FileSizeHigh;
    public uint FileSizeLow;
    public uint Reserved0;
    public uint Reserved1;
    public array<ushort> FileName;
    public array<ushort> AlternateFileName; // The Microsoft documentation for this struct¹ describes three additional
// fields: dwFileType, dwCreatorType, and wFinderFlags. However, those fields
// are empirically only present in the macOS port of the Win32 API,² and thus
// not needed for binaries built for Windows.
//
// ¹ https://docs.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-win32_find_dataw
// ² https://golang.org/issue/42637#issuecomment-760715755
}

private static void copyFindData(ptr<Win32finddata> _addr_dst, ptr<win32finddata1> _addr_src) {
    ref Win32finddata dst = ref _addr_dst.val;
    ref win32finddata1 src = ref _addr_src.val;

    dst.FileAttributes = src.FileAttributes;
    dst.CreationTime = src.CreationTime;
    dst.LastAccessTime = src.LastAccessTime;
    dst.LastWriteTime = src.LastWriteTime;
    dst.FileSizeHigh = src.FileSizeHigh;
    dst.FileSizeLow = src.FileSizeLow;
    dst.Reserved0 = src.Reserved0;
    dst.Reserved1 = src.Reserved1; 

    // The src is 1 element bigger than dst, but it must be NUL.
    copy(dst.FileName[..], src.FileName[..]);
    copy(dst.AlternateFileName[..], src.AlternateFileName[..]);

}

public partial struct ByHandleFileInformation {
    public uint FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint VolumeSerialNumber;
    public uint FileSizeHigh;
    public uint FileSizeLow;
    public uint NumberOfLinks;
    public uint FileIndexHigh;
    public uint FileIndexLow;
}

public static readonly nint GetFileExInfoStandard = 0;
public static readonly nint GetFileExMaxInfoLevel = 1;


public partial struct Win32FileAttributeData {
    public uint FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint FileSizeHigh;
    public uint FileSizeLow;
}

// ShowWindow constants
 
// winuser.h
public static readonly nint SW_HIDE = 0;
public static readonly nint SW_NORMAL = 1;
public static readonly nint SW_SHOWNORMAL = 1;
public static readonly nint SW_SHOWMINIMIZED = 2;
public static readonly nint SW_SHOWMAXIMIZED = 3;
public static readonly nint SW_MAXIMIZE = 3;
public static readonly nint SW_SHOWNOACTIVATE = 4;
public static readonly nint SW_SHOW = 5;
public static readonly nint SW_MINIMIZE = 6;
public static readonly nint SW_SHOWMINNOACTIVE = 7;
public static readonly nint SW_SHOWNA = 8;
public static readonly nint SW_RESTORE = 9;
public static readonly nint SW_SHOWDEFAULT = 10;
public static readonly nint SW_FORCEMINIMIZE = 11;


public partial struct StartupInfo {
    public uint Cb;
    public ptr<ushort> _;
    public ptr<ushort> Desktop;
    public ptr<ushort> Title;
    public uint X;
    public uint Y;
    public uint XSize;
    public uint YSize;
    public uint XCountChars;
    public uint YCountChars;
    public uint FillAttribute;
    public uint Flags;
    public ushort ShowWindow;
    public ushort _;
    public ptr<byte> _;
    public Handle StdInput;
    public Handle StdOutput;
    public Handle StdErr;
}

private partial struct _PROC_THREAD_ATTRIBUTE_LIST {
    public array<byte> _;
}

private static readonly nuint _PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
private static readonly nuint _PROC_THREAD_ATTRIBUTE_HANDLE_LIST = 0x00020002;


private partial struct _STARTUPINFOEXW {
    public ref StartupInfo StartupInfo => ref StartupInfo_val;
    public ptr<_PROC_THREAD_ATTRIBUTE_LIST> ProcThreadAttributeList;
}

private static readonly nuint _EXTENDED_STARTUPINFO_PRESENT = 0x00080000;



public partial struct ProcessInformation {
    public Handle Process;
    public Handle Thread;
    public uint ProcessId;
    public uint ThreadId;
}

public partial struct ProcessEntry32 {
    public uint Size;
    public uint Usage;
    public uint ProcessID;
    public System.UIntPtr DefaultHeapID;
    public uint ModuleID;
    public uint Threads;
    public uint ParentProcessID;
    public int PriClassBase;
    public uint Flags;
    public array<ushort> ExeFile;
}

public partial struct Systemtime {
    public ushort Year;
    public ushort Month;
    public ushort DayOfWeek;
    public ushort Day;
    public ushort Hour;
    public ushort Minute;
    public ushort Second;
    public ushort Milliseconds;
}

public partial struct Timezoneinformation {
    public int Bias;
    public array<ushort> StandardName;
    public Systemtime StandardDate;
    public int StandardBias;
    public array<ushort> DaylightName;
    public Systemtime DaylightDate;
    public int DaylightBias;
}

// Socket related.

public static readonly nint AF_UNSPEC = 0;
public static readonly nint AF_UNIX = 1;
public static readonly nint AF_INET = 2;
public static readonly nint AF_INET6 = 23;
public static readonly nint AF_NETBIOS = 17;

public static readonly nint SOCK_STREAM = 1;
public static readonly nint SOCK_DGRAM = 2;
public static readonly nint SOCK_RAW = 3;
public static readonly nint SOCK_SEQPACKET = 5;

public static readonly nint IPPROTO_IP = 0;
public static readonly nuint IPPROTO_IPV6 = 0x29;
public static readonly nint IPPROTO_TCP = 6;
public static readonly nint IPPROTO_UDP = 17;

public static readonly nuint SOL_SOCKET = 0xffff;
public static readonly nint SO_REUSEADDR = 4;
public static readonly nint SO_KEEPALIVE = 8;
public static readonly nint SO_DONTROUTE = 16;
public static readonly nint SO_BROADCAST = 32;
public static readonly nint SO_LINGER = 128;
public static readonly nuint SO_RCVBUF = 0x1002;
public static readonly nuint SO_SNDBUF = 0x1001;
public static readonly nuint SO_UPDATE_ACCEPT_CONTEXT = 0x700b;
public static readonly nuint SO_UPDATE_CONNECT_CONTEXT = 0x7010;

public static readonly nuint IOC_OUT = 0x40000000;
public static readonly nuint IOC_IN = 0x80000000;
public static readonly nuint IOC_VENDOR = 0x18000000;
public static readonly var IOC_INOUT = IOC_IN | IOC_OUT;
public static readonly nuint IOC_WS2 = 0x08000000;
public static readonly var SIO_GET_EXTENSION_FUNCTION_POINTER = IOC_INOUT | IOC_WS2 | 6;
public static readonly var SIO_KEEPALIVE_VALS = IOC_IN | IOC_VENDOR | 4;
public static readonly var SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12; 

// cf. https://support.microsoft.com/default.aspx?scid=kb;en-us;257460

public static readonly nuint IP_TOS = 0x3;
public static readonly nuint IP_TTL = 0x4;
public static readonly nuint IP_MULTICAST_IF = 0x9;
public static readonly nuint IP_MULTICAST_TTL = 0xa;
public static readonly nuint IP_MULTICAST_LOOP = 0xb;
public static readonly nuint IP_ADD_MEMBERSHIP = 0xc;
public static readonly nuint IP_DROP_MEMBERSHIP = 0xd;

public static readonly nuint IPV6_V6ONLY = 0x1b;
public static readonly nuint IPV6_UNICAST_HOPS = 0x4;
public static readonly nuint IPV6_MULTICAST_IF = 0x9;
public static readonly nuint IPV6_MULTICAST_HOPS = 0xa;
public static readonly nuint IPV6_MULTICAST_LOOP = 0xb;
public static readonly nuint IPV6_JOIN_GROUP = 0xc;
public static readonly nuint IPV6_LEAVE_GROUP = 0xd;

public static readonly nuint SOMAXCONN = 0x7fffffff;

public static readonly nint TCP_NODELAY = 1;

public static readonly nint SHUT_RD = 0;
public static readonly nint SHUT_WR = 1;
public static readonly nint SHUT_RDWR = 2;

public static readonly nint WSADESCRIPTION_LEN = 256;
public static readonly nint WSASYS_STATUS_LEN = 128;


public partial struct WSABuf {
    public uint Len;
    public ptr<byte> Buf;
}

// Invented values to support what package os expects.
public static readonly nuint S_IFMT = 0x1f000;
public static readonly nuint S_IFIFO = 0x1000;
public static readonly nuint S_IFCHR = 0x2000;
public static readonly nuint S_IFDIR = 0x4000;
public static readonly nuint S_IFBLK = 0x6000;
public static readonly nuint S_IFREG = 0x8000;
public static readonly nuint S_IFLNK = 0xa000;
public static readonly nuint S_IFSOCK = 0xc000;
public static readonly nuint S_ISUID = 0x800;
public static readonly nuint S_ISGID = 0x400;
public static readonly nuint S_ISVTX = 0x200;
public static readonly nuint S_IRUSR = 0x100;
public static readonly nuint S_IWRITE = 0x80;
public static readonly nuint S_IWUSR = 0x80;
public static readonly nuint S_IXUSR = 0x40;


public static readonly nuint FILE_TYPE_CHAR = 0x0002;
public static readonly nuint FILE_TYPE_DISK = 0x0001;
public static readonly nuint FILE_TYPE_PIPE = 0x0003;
public static readonly nuint FILE_TYPE_REMOTE = 0x8000;
public static readonly nuint FILE_TYPE_UNKNOWN = 0x0000;


public partial struct Hostent {
    public ptr<byte> Name;
    public ptr<ptr<byte>> Aliases;
    public ushort AddrType;
    public ushort Length;
    public ptr<ptr<byte>> AddrList;
}

public partial struct Protoent {
    public ptr<byte> Name;
    public ptr<ptr<byte>> Aliases;
    public ushort Proto;
}

public static readonly nuint DNS_TYPE_A = 0x0001;
public static readonly nuint DNS_TYPE_NS = 0x0002;
public static readonly nuint DNS_TYPE_MD = 0x0003;
public static readonly nuint DNS_TYPE_MF = 0x0004;
public static readonly nuint DNS_TYPE_CNAME = 0x0005;
public static readonly nuint DNS_TYPE_SOA = 0x0006;
public static readonly nuint DNS_TYPE_MB = 0x0007;
public static readonly nuint DNS_TYPE_MG = 0x0008;
public static readonly nuint DNS_TYPE_MR = 0x0009;
public static readonly nuint DNS_TYPE_NULL = 0x000a;
public static readonly nuint DNS_TYPE_WKS = 0x000b;
public static readonly nuint DNS_TYPE_PTR = 0x000c;
public static readonly nuint DNS_TYPE_HINFO = 0x000d;
public static readonly nuint DNS_TYPE_MINFO = 0x000e;
public static readonly nuint DNS_TYPE_MX = 0x000f;
public static readonly nuint DNS_TYPE_TEXT = 0x0010;
public static readonly nuint DNS_TYPE_RP = 0x0011;
public static readonly nuint DNS_TYPE_AFSDB = 0x0012;
public static readonly nuint DNS_TYPE_X25 = 0x0013;
public static readonly nuint DNS_TYPE_ISDN = 0x0014;
public static readonly nuint DNS_TYPE_RT = 0x0015;
public static readonly nuint DNS_TYPE_NSAP = 0x0016;
public static readonly nuint DNS_TYPE_NSAPPTR = 0x0017;
public static readonly nuint DNS_TYPE_SIG = 0x0018;
public static readonly nuint DNS_TYPE_KEY = 0x0019;
public static readonly nuint DNS_TYPE_PX = 0x001a;
public static readonly nuint DNS_TYPE_GPOS = 0x001b;
public static readonly nuint DNS_TYPE_AAAA = 0x001c;
public static readonly nuint DNS_TYPE_LOC = 0x001d;
public static readonly nuint DNS_TYPE_NXT = 0x001e;
public static readonly nuint DNS_TYPE_EID = 0x001f;
public static readonly nuint DNS_TYPE_NIMLOC = 0x0020;
public static readonly nuint DNS_TYPE_SRV = 0x0021;
public static readonly nuint DNS_TYPE_ATMA = 0x0022;
public static readonly nuint DNS_TYPE_NAPTR = 0x0023;
public static readonly nuint DNS_TYPE_KX = 0x0024;
public static readonly nuint DNS_TYPE_CERT = 0x0025;
public static readonly nuint DNS_TYPE_A6 = 0x0026;
public static readonly nuint DNS_TYPE_DNAME = 0x0027;
public static readonly nuint DNS_TYPE_SINK = 0x0028;
public static readonly nuint DNS_TYPE_OPT = 0x0029;
public static readonly nuint DNS_TYPE_DS = 0x002B;
public static readonly nuint DNS_TYPE_RRSIG = 0x002E;
public static readonly nuint DNS_TYPE_NSEC = 0x002F;
public static readonly nuint DNS_TYPE_DNSKEY = 0x0030;
public static readonly nuint DNS_TYPE_DHCID = 0x0031;
public static readonly nuint DNS_TYPE_UINFO = 0x0064;
public static readonly nuint DNS_TYPE_UID = 0x0065;
public static readonly nuint DNS_TYPE_GID = 0x0066;
public static readonly nuint DNS_TYPE_UNSPEC = 0x0067;
public static readonly nuint DNS_TYPE_ADDRS = 0x00f8;
public static readonly nuint DNS_TYPE_TKEY = 0x00f9;
public static readonly nuint DNS_TYPE_TSIG = 0x00fa;
public static readonly nuint DNS_TYPE_IXFR = 0x00fb;
public static readonly nuint DNS_TYPE_AXFR = 0x00fc;
public static readonly nuint DNS_TYPE_MAILB = 0x00fd;
public static readonly nuint DNS_TYPE_MAILA = 0x00fe;
public static readonly nuint DNS_TYPE_ALL = 0x00ff;
public static readonly nuint DNS_TYPE_ANY = 0x00ff;
public static readonly nuint DNS_TYPE_WINS = 0xff01;
public static readonly nuint DNS_TYPE_WINSR = 0xff02;
public static readonly nuint DNS_TYPE_NBSTAT = 0xff01;


public static readonly nuint DNS_INFO_NO_RECORDS = 0x251D;


 
// flags inside DNSRecord.Dw
public static readonly nuint DnsSectionQuestion = 0x0000;
public static readonly nuint DnsSectionAnswer = 0x0001;
public static readonly nuint DnsSectionAuthority = 0x0002;
public static readonly nuint DnsSectionAdditional = 0x0003;


public partial struct DNSSRVData {
    public ptr<ushort> Target;
    public ushort Priority;
    public ushort Weight;
    public ushort Port;
    public ushort Pad;
}

public partial struct DNSPTRData {
    public ptr<ushort> Host;
}

public partial struct DNSMXData {
    public ptr<ushort> NameExchange;
    public ushort Preference;
    public ushort Pad;
}

public partial struct DNSTXTData {
    public ushort StringCount;
    public array<ptr<ushort>> StringArray;
}

public partial struct DNSRecord {
    public ptr<DNSRecord> Next;
    public ptr<ushort> Name;
    public ushort Type;
    public ushort Length;
    public uint Dw;
    public uint Ttl;
    public uint Reserved;
    public array<byte> Data;
}

public static readonly nint TF_DISCONNECT = 1;
public static readonly nint TF_REUSE_SOCKET = 2;
public static readonly nint TF_WRITE_BEHIND = 4;
public static readonly nint TF_USE_DEFAULT_WORKER = 0;
public static readonly nint TF_USE_SYSTEM_THREAD = 16;
public static readonly nint TF_USE_KERNEL_APC = 32;


public partial struct TransmitFileBuffers {
    public System.UIntPtr Head;
    public uint HeadLength;
    public System.UIntPtr Tail;
    public uint TailLength;
}

public static readonly nint IFF_UP = 1;
public static readonly nint IFF_BROADCAST = 2;
public static readonly nint IFF_LOOPBACK = 4;
public static readonly nint IFF_POINTTOPOINT = 8;
public static readonly nint IFF_MULTICAST = 16;


public static readonly nuint SIO_GET_INTERFACE_LIST = 0x4004747F;

// TODO(mattn): SockaddrGen is union of sockaddr/sockaddr_in/sockaddr_in6_old.
// will be fixed to change variable type as suitable.



// TODO(mattn): SockaddrGen is union of sockaddr/sockaddr_in/sockaddr_in6_old.
// will be fixed to change variable type as suitable.

public partial struct SockaddrGen { // : array<byte>
}

public partial struct InterfaceInfo {
    public uint Flags;
    public SockaddrGen Address;
    public SockaddrGen BroadcastAddress;
    public SockaddrGen Netmask;
}

public partial struct IpAddressString {
    public array<byte> String;
}

public partial struct IpMaskString { // : IpAddressString
}

public partial struct IpAddrString {
    public ptr<IpAddrString> Next;
    public IpAddressString IpAddress;
    public IpMaskString IpMask;
    public uint Context;
}

public static readonly nint MAX_ADAPTER_NAME_LENGTH = 256;

public static readonly nint MAX_ADAPTER_DESCRIPTION_LENGTH = 128;

public static readonly nint MAX_ADAPTER_ADDRESS_LENGTH = 8;



public partial struct IpAdapterInfo {
    public ptr<IpAdapterInfo> Next;
    public uint ComboIndex;
    public array<byte> AdapterName;
    public array<byte> Description;
    public uint AddressLength;
    public array<byte> Address;
    public uint Index;
    public uint Type;
    public uint DhcpEnabled;
    public ptr<IpAddrString> CurrentIpAddress;
    public IpAddrString IpAddressList;
    public IpAddrString GatewayList;
    public IpAddrString DhcpServer;
    public bool HaveWins;
    public IpAddrString PrimaryWinsServer;
    public IpAddrString SecondaryWinsServer;
    public long LeaseObtained;
    public long LeaseExpires;
}

public static readonly nint MAXLEN_PHYSADDR = 8;

public static readonly nint MAX_INTERFACE_NAME_LEN = 256;

public static readonly nint MAXLEN_IFDESCR = 256;



public partial struct MibIfRow {
    public array<ushort> Name;
    public uint Index;
    public uint Type;
    public uint Mtu;
    public uint Speed;
    public uint PhysAddrLen;
    public array<byte> PhysAddr;
    public uint AdminStatus;
    public uint OperStatus;
    public uint LastChange;
    public uint InOctets;
    public uint InUcastPkts;
    public uint InNUcastPkts;
    public uint InDiscards;
    public uint InErrors;
    public uint InUnknownProtos;
    public uint OutOctets;
    public uint OutUcastPkts;
    public uint OutNUcastPkts;
    public uint OutDiscards;
    public uint OutErrors;
    public uint OutQLen;
    public uint DescrLen;
    public array<byte> Descr;
}

public partial struct CertInfo {
}

public partial struct CertContext {
    public uint EncodingType;
    public ptr<byte> EncodedCert;
    public uint Length;
    public ptr<CertInfo> CertInfo;
    public Handle Store;
}

public partial struct CertChainContext {
    public uint Size;
    public CertTrustStatus TrustStatus;
    public uint ChainCount;
    public ptr<ptr<CertSimpleChain>> Chains;
    public uint LowerQualityChainCount;
    public ptr<ptr<CertChainContext>> LowerQualityChains;
    public uint HasRevocationFreshnessTime;
    public uint RevocationFreshnessTime;
}

public partial struct CertTrustListInfo {
}

public partial struct CertSimpleChain {
    public uint Size;
    public CertTrustStatus TrustStatus;
    public uint NumElements;
    public ptr<ptr<CertChainElement>> Elements;
    public ptr<CertTrustListInfo> TrustListInfo;
    public uint HasRevocationFreshnessTime;
    public uint RevocationFreshnessTime;
}

public partial struct CertChainElement {
    public uint Size;
    public ptr<CertContext> CertContext;
    public CertTrustStatus TrustStatus;
    public ptr<CertRevocationInfo> RevocationInfo;
    public ptr<CertEnhKeyUsage> IssuanceUsage;
    public ptr<CertEnhKeyUsage> ApplicationUsage;
    public ptr<ushort> ExtendedErrorInfo;
}

public partial struct CertRevocationCrlInfo {
}

public partial struct CertRevocationInfo {
    public uint Size;
    public uint RevocationResult;
    public ptr<byte> RevocationOid;
    public Pointer OidSpecificInfo;
    public uint HasFreshnessTime;
    public uint FreshnessTime;
    public ptr<CertRevocationCrlInfo> CrlInfo;
}

public partial struct CertTrustStatus {
    public uint ErrorStatus;
    public uint InfoStatus;
}

public partial struct CertUsageMatch {
    public uint Type;
    public CertEnhKeyUsage Usage;
}

public partial struct CertEnhKeyUsage {
    public uint Length;
    public ptr<ptr<byte>> UsageIdentifiers;
}

public partial struct CertChainPara {
    public uint Size;
    public CertUsageMatch RequestedUsage;
    public CertUsageMatch RequstedIssuancePolicy;
    public uint URLRetrievalTimeout;
    public uint CheckRevocationFreshnessTime;
    public uint RevocationFreshnessTime;
    public ptr<Filetime> CacheResync;
}

public partial struct CertChainPolicyPara {
    public uint Size;
    public uint Flags;
    public Pointer ExtraPolicyPara;
}

public partial struct SSLExtraCertChainPolicyPara {
    public uint Size;
    public uint AuthType;
    public uint Checks;
    public ptr<ushort> ServerName;
}

public partial struct CertChainPolicyStatus {
    public uint Size;
    public uint Error;
    public uint ChainIndex;
    public uint ElementIndex;
    public Pointer ExtraPolicyStatus;
}

 
// do not reorder
public static readonly nuint HKEY_CLASSES_ROOT = 0x80000000 + iota;
public static readonly var HKEY_CURRENT_USER = 0;
public static readonly var HKEY_LOCAL_MACHINE = 1;
public static readonly var HKEY_USERS = 2;
public static readonly var HKEY_PERFORMANCE_DATA = 3;
public static readonly var HKEY_CURRENT_CONFIG = 4;
public static readonly KEY_QUERY_VALUE HKEY_DYN_DATA = 1;
public static readonly nint KEY_SET_VALUE = 2;
public static readonly nint KEY_CREATE_SUB_KEY = 4;
public static readonly nint KEY_ENUMERATE_SUB_KEYS = 8;
public static readonly nint KEY_NOTIFY = 16;
public static readonly nint KEY_CREATE_LINK = 32;
public static readonly nuint KEY_WRITE = 0x20006;
public static readonly nuint KEY_EXECUTE = 0x20019;
public static readonly nuint KEY_READ = 0x20019;
public static readonly nuint KEY_WOW64_64KEY = 0x0100;
public static readonly nuint KEY_WOW64_32KEY = 0x0200;
public static readonly nuint KEY_ALL_ACCESS = 0xf003f;


 
// do not reorder
public static readonly var REG_NONE = iota;
public static readonly var REG_SZ = 0;
public static readonly var REG_EXPAND_SZ = 1;
public static readonly var REG_BINARY = 2;
public static readonly var REG_DWORD_LITTLE_ENDIAN = 3;
public static readonly var REG_DWORD_BIG_ENDIAN = 4;
public static readonly var REG_LINK = 5;
public static readonly var REG_MULTI_SZ = 6;
public static readonly var REG_RESOURCE_LIST = 7;
public static readonly var REG_FULL_RESOURCE_DESCRIPTOR = 8;
public static readonly var REG_RESOURCE_REQUIREMENTS_LIST = 9;
public static readonly REG_DWORD REG_QWORD_LITTLE_ENDIAN = REG_DWORD_LITTLE_ENDIAN;
public static readonly var REG_QWORD = REG_QWORD_LITTLE_ENDIAN;


public partial struct AddrinfoW {
    public int Flags;
    public int Family;
    public int Socktype;
    public int Protocol;
    public System.UIntPtr Addrlen;
    public ptr<ushort> Canonname;
    public Pointer Addr;
    public ptr<AddrinfoW> Next;
}

public static readonly nint AI_PASSIVE = 1;
public static readonly nint AI_CANONNAME = 2;
public static readonly nint AI_NUMERICHOST = 4;


public partial struct GUID {
    public uint Data1;
    public ushort Data2;
    public ushort Data3;
    public array<byte> Data4;
}

public static GUID WSAID_CONNECTEX = new GUID(0x25a207b9,0xddf3,0x4660,[8]byte{0x8e,0xe9,0x76,0xe5,0x8c,0x74,0x06,0x3e},);

public static readonly nint FILE_SKIP_COMPLETION_PORT_ON_SUCCESS = 1;
public static readonly nint FILE_SKIP_SET_EVENT_ON_HANDLE = 2;


public static readonly nint WSAPROTOCOL_LEN = 255;
public static readonly nint MAX_PROTOCOL_CHAIN = 7;
public static readonly nint BASE_PROTOCOL = 1;
public static readonly nint LAYERED_PROTOCOL = 0;

public static readonly nuint XP1_CONNECTIONLESS = 0x00000001;
public static readonly nuint XP1_GUARANTEED_DELIVERY = 0x00000002;
public static readonly nuint XP1_GUARANTEED_ORDER = 0x00000004;
public static readonly nuint XP1_MESSAGE_ORIENTED = 0x00000008;
public static readonly nuint XP1_PSEUDO_STREAM = 0x00000010;
public static readonly nuint XP1_GRACEFUL_CLOSE = 0x00000020;
public static readonly nuint XP1_EXPEDITED_DATA = 0x00000040;
public static readonly nuint XP1_CONNECT_DATA = 0x00000080;
public static readonly nuint XP1_DISCONNECT_DATA = 0x00000100;
public static readonly nuint XP1_SUPPORT_BROADCAST = 0x00000200;
public static readonly nuint XP1_SUPPORT_MULTIPOINT = 0x00000400;
public static readonly nuint XP1_MULTIPOINT_CONTROL_PLANE = 0x00000800;
public static readonly nuint XP1_MULTIPOINT_DATA_PLANE = 0x00001000;
public static readonly nuint XP1_QOS_SUPPORTED = 0x00002000;
public static readonly nuint XP1_UNI_SEND = 0x00008000;
public static readonly nuint XP1_UNI_RECV = 0x00010000;
public static readonly nuint XP1_IFS_HANDLES = 0x00020000;
public static readonly nuint XP1_PARTIAL_MESSAGE = 0x00040000;
public static readonly nuint XP1_SAN_SUPPORT_SDP = 0x00080000;

public static readonly nuint PFL_MULTIPLE_PROTO_ENTRIES = 0x00000001;
public static readonly nuint PFL_RECOMMENDED_PROTO_ENTRY = 0x00000002;
public static readonly nuint PFL_HIDDEN = 0x00000004;
public static readonly nuint PFL_MATCHES_PROTOCOL_ZERO = 0x00000008;
public static readonly nuint PFL_NETWORKDIRECT_PROVIDER = 0x00000010;


public partial struct WSAProtocolInfo {
    public uint ServiceFlags1;
    public uint ServiceFlags2;
    public uint ServiceFlags3;
    public uint ServiceFlags4;
    public uint ProviderFlags;
    public GUID ProviderId;
    public uint CatalogEntryId;
    public WSAProtocolChain ProtocolChain;
    public int Version;
    public int AddressFamily;
    public int MaxSockAddr;
    public int MinSockAddr;
    public int SocketType;
    public int Protocol;
    public int ProtocolMaxOffset;
    public int NetworkByteOrder;
    public int SecurityScheme;
    public uint MessageSize;
    public uint ProviderReserved;
    public array<ushort> ProtocolName;
}

public partial struct WSAProtocolChain {
    public int ChainLen;
    public array<uint> ChainEntries;
}

public partial struct TCPKeepalive {
    public uint OnOff;
    public uint Time;
    public uint Interval;
}

private partial struct symbolicLinkReparseBuffer {
    public ushort SubstituteNameOffset;
    public ushort SubstituteNameLength;
    public ushort PrintNameOffset;
    public ushort PrintNameLength;
    public uint Flags;
    public array<ushort> PathBuffer;
}

private partial struct mountPointReparseBuffer {
    public ushort SubstituteNameOffset;
    public ushort SubstituteNameLength;
    public ushort PrintNameOffset;
    public ushort PrintNameLength;
    public array<ushort> PathBuffer;
}

private partial struct reparseDataBuffer {
    public uint ReparseTag;
    public ushort ReparseDataLength;
    public ushort Reserved; // GenericReparseBuffer
    public byte reparseBuffer;
}

public static readonly nuint FSCTL_GET_REPARSE_POINT = 0x900A8;
public static readonly nint MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16 * 1024;
private static readonly nuint _IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
public static readonly nuint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
public static readonly nuint SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;
private static readonly nint _SYMLINK_FLAG_RELATIVE = 1;


public static readonly nint UNIX_PATH_MAX = 108; // defined in afunix.h
 // defined in afunix.h

} // end syscall_package
