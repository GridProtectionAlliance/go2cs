// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class syscall_package {

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
public static readonly Errno WSAENOPROTOOPT = 10042;
public static readonly Errno WSAECONNABORTED = 10053;
public static readonly Errno WSAECONNRESET = 10054;

public static readonly UntypedInt O_RDONLY = /* 0x00000 */ 0;
public static readonly UntypedInt O_WRONLY = /* 0x00001 */ 1;
public static readonly UntypedInt O_RDWR = /* 0x00002 */ 2;
public static readonly UntypedInt O_CREAT = /* 0x00040 */ 64;
public static readonly UntypedInt O_EXCL = /* 0x00080 */ 128;
public static readonly UntypedInt O_NOCTTY = /* 0x00100 */ 256;
public static readonly UntypedInt O_TRUNC = /* 0x00200 */ 512;
public static readonly UntypedInt O_NONBLOCK = /* 0x00800 */ 2048;
public static readonly UntypedInt O_APPEND = /* 0x00400 */ 1024;
public static readonly UntypedInt O_SYNC = /* 0x01000 */ 4096;
public static readonly UntypedInt O_ASYNC = /* 0x02000 */ 8192;
public static readonly UntypedInt O_CLOEXEC = /* 0x80000 */ 524288;

public static readonly ΔSignal SIGHUP = /* Signal(0x1) */ 1;
public static readonly ΔSignal SIGINT = /* Signal(0x2) */ 2;
public static readonly ΔSignal SIGQUIT = /* Signal(0x3) */ 3;
public static readonly ΔSignal SIGILL = /* Signal(0x4) */ 4;
public static readonly ΔSignal SIGTRAP = /* Signal(0x5) */ 5;
public static readonly ΔSignal SIGABRT = /* Signal(0x6) */ 6;
public static readonly ΔSignal SIGBUS = /* Signal(0x7) */ 7;
public static readonly ΔSignal SIGFPE = /* Signal(0x8) */ 8;
public static readonly ΔSignal SIGKILL = /* Signal(0x9) */ 9;
public static readonly ΔSignal SIGSEGV = /* Signal(0xb) */ 11;
public static readonly ΔSignal SIGPIPE = /* Signal(0xd) */ 13;
public static readonly ΔSignal SIGALRM = /* Signal(0xe) */ 14;
public static readonly ΔSignal SIGTERM = /* Signal(0xf) */ 15;

internal static array<@string> signals = new array<@string>(16){
    [1] = "hangup"u8,
    [2] = "interrupt"u8,
    [3] = "quit"u8,
    [4] = "illegal instruction"u8,
    [5] = "trace/breakpoint trap"u8,
    [6] = "aborted"u8,
    [7] = "bus error"u8,
    [8] = "floating point exception"u8,
    [9] = "killed"u8,
    [10] = "user defined signal 1"u8,
    [11] = "segmentation fault"u8,
    [12] = "user defined signal 2"u8,
    [13] = "broken pipe"u8,
    [14] = "alarm clock"u8,
    [15] = "terminated"u8
};

public static readonly UntypedInt GENERIC_READ = /* 0x80000000 */ 2147483648;
public static readonly UntypedInt GENERIC_WRITE = /* 0x40000000 */ 1073741824;
public static readonly UntypedInt GENERIC_EXECUTE = /* 0x20000000 */ 536870912;
public static readonly UntypedInt GENERIC_ALL = /* 0x10000000 */ 268435456;
public static readonly UntypedInt FILE_LIST_DIRECTORY = /* 0x00000001 */ 1;
public static readonly UntypedInt FILE_APPEND_DATA = /* 0x00000004 */ 4;
public static readonly UntypedInt FILE_WRITE_ATTRIBUTES = /* 0x00000100 */ 256;
public static readonly UntypedInt FILE_SHARE_READ = /* 0x00000001 */ 1;
public static readonly UntypedInt FILE_SHARE_WRITE = /* 0x00000002 */ 2;
public static readonly UntypedInt FILE_SHARE_DELETE = /* 0x00000004 */ 4;
public static readonly UntypedInt FILE_ATTRIBUTE_READONLY = /* 0x00000001 */ 1;
public static readonly UntypedInt FILE_ATTRIBUTE_HIDDEN = /* 0x00000002 */ 2;
public static readonly UntypedInt FILE_ATTRIBUTE_SYSTEM = /* 0x00000004 */ 4;
public static readonly UntypedInt FILE_ATTRIBUTE_DIRECTORY = /* 0x00000010 */ 16;
public static readonly UntypedInt FILE_ATTRIBUTE_ARCHIVE = /* 0x00000020 */ 32;
public static readonly UntypedInt FILE_ATTRIBUTE_NORMAL = /* 0x00000080 */ 128;
public static readonly UntypedInt FILE_ATTRIBUTE_REPARSE_POINT = /* 0x00000400 */ 1024;
public static readonly UntypedInt INVALID_FILE_ATTRIBUTES = /* 0xffffffff */ 4294967295;
public static readonly UntypedInt CREATE_NEW = 1;
public static readonly UntypedInt CREATE_ALWAYS = 2;
public static readonly UntypedInt OPEN_EXISTING = 3;
public static readonly UntypedInt OPEN_ALWAYS = 4;
public static readonly UntypedInt TRUNCATE_EXISTING = 5;
public static readonly UntypedInt FILE_FLAG_OPEN_REPARSE_POINT = /* 0x00200000 */ 2097152;
public static readonly UntypedInt FILE_FLAG_BACKUP_SEMANTICS = /* 0x02000000 */ 33554432;
public static readonly UntypedInt FILE_FLAG_OVERLAPPED = /* 0x40000000 */ 1073741824;
public static readonly UntypedInt HANDLE_FLAG_INHERIT = /* 0x00000001 */ 1;
public static readonly UntypedInt STARTF_USESTDHANDLES = /* 0x00000100 */ 256;
public static readonly UntypedInt STARTF_USESHOWWINDOW = /* 0x00000001 */ 1;
public static readonly UntypedInt DUPLICATE_CLOSE_SOURCE = /* 0x00000001 */ 1;
public static readonly UntypedInt DUPLICATE_SAME_ACCESS = /* 0x00000002 */ 2;
public static readonly UntypedInt STD_INPUT_HANDLE = -10;
public static readonly UntypedInt STD_OUTPUT_HANDLE = -11;
public static readonly UntypedInt STD_ERROR_HANDLE = -12;
public static readonly UntypedInt FILE_BEGIN = 0;
public static readonly UntypedInt FILE_CURRENT = 1;
public static readonly UntypedInt FILE_END = 2;
public static readonly UntypedInt LANG_ENGLISH = /* 0x09 */ 9;
public static readonly UntypedInt SUBLANG_ENGLISH_US = /* 0x01 */ 1;
public static readonly UntypedInt FORMAT_MESSAGE_ALLOCATE_BUFFER = 256;
public static readonly UntypedInt FORMAT_MESSAGE_IGNORE_INSERTS = 512;
public static readonly UntypedInt FORMAT_MESSAGE_FROM_STRING = 1024;
public static readonly UntypedInt FORMAT_MESSAGE_FROM_HMODULE = 2048;
public static readonly UntypedInt FORMAT_MESSAGE_FROM_SYSTEM = 4096;
public static readonly UntypedInt FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;
public static readonly UntypedInt FORMAT_MESSAGE_MAX_WIDTH_MASK = 255;
public static readonly UntypedInt MAX_PATH = 260;
public static readonly UntypedInt MAX_LONG_PATH = 32768;
public static readonly UntypedInt MAX_COMPUTERNAME_LENGTH = 15;
public static readonly UntypedInt TIME_ZONE_ID_UNKNOWN = 0;
public static readonly UntypedInt TIME_ZONE_ID_STANDARD = 1;
public static readonly UntypedInt TIME_ZONE_ID_DAYLIGHT = 2;
public static readonly UntypedInt IGNORE = 0;
public static readonly UntypedInt INFINITE = /* 0xffffffff */ 4294967295;
public static readonly UntypedInt WAIT_TIMEOUT = 258;
public static readonly UntypedInt WAIT_ABANDONED = /* 0x00000080 */ 128;
public static readonly UntypedInt WAIT_OBJECT_0 = /* 0x00000000 */ 0;
public static readonly UntypedInt WAIT_FAILED = /* 0xFFFFFFFF */ 4294967295;
public static readonly UntypedInt CREATE_NEW_PROCESS_GROUP = /* 0x00000200 */ 512;
public static readonly UntypedInt CREATE_UNICODE_ENVIRONMENT = /* 0x00000400 */ 1024;
public static readonly UntypedInt PROCESS_TERMINATE = 1;
public static readonly UntypedInt PROCESS_QUERY_INFORMATION = /* 0x00000400 */ 1024;
public static readonly UntypedInt SYNCHRONIZE = /* 0x00100000 */ 1048576;
public static readonly UntypedInt PAGE_READONLY = /* 0x02 */ 2;
public static readonly UntypedInt PAGE_READWRITE = /* 0x04 */ 4;
public static readonly UntypedInt PAGE_WRITECOPY = /* 0x08 */ 8;
public static readonly UntypedInt PAGE_EXECUTE_READ = /* 0x20 */ 32;
public static readonly UntypedInt PAGE_EXECUTE_READWRITE = /* 0x40 */ 64;
public static readonly UntypedInt PAGE_EXECUTE_WRITECOPY = /* 0x80 */ 128;
public static readonly UntypedInt FILE_MAP_COPY = /* 0x01 */ 1;
public static readonly UntypedInt FILE_MAP_WRITE = /* 0x02 */ 2;
public static readonly UntypedInt FILE_MAP_READ = /* 0x04 */ 4;
public static readonly UntypedInt FILE_MAP_EXECUTE = /* 0x20 */ 32;
public static readonly UntypedInt CTRL_C_EVENT = 0;
public static readonly UntypedInt CTRL_BREAK_EVENT = 1;
public static readonly UntypedInt CTRL_CLOSE_EVENT = 2;
public static readonly UntypedInt CTRL_LOGOFF_EVENT = 5;
public static readonly UntypedInt CTRL_SHUTDOWN_EVENT = 6;

public static readonly UntypedInt TH32CS_SNAPHEAPLIST = /* 0x01 */ 1;
public static readonly UntypedInt TH32CS_SNAPPROCESS = /* 0x02 */ 2;
public static readonly UntypedInt TH32CS_SNAPTHREAD = /* 0x04 */ 4;
public static readonly UntypedInt TH32CS_SNAPMODULE = /* 0x08 */ 8;
public static readonly UntypedInt TH32CS_SNAPMODULE32 = /* 0x10 */ 16;
public static readonly UntypedInt TH32CS_SNAPALL = /* TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD */ 15;
public static readonly UntypedInt TH32CS_INHERIT = /* 0x80000000 */ 2147483648;

public static readonly UntypedInt FILE_NOTIFY_CHANGE_FILE_NAME = /* 1 << iota */ 1;
public static readonly UntypedInt FILE_NOTIFY_CHANGE_DIR_NAME = 2;
public static readonly UntypedInt FILE_NOTIFY_CHANGE_ATTRIBUTES = 4;
public static readonly UntypedInt FILE_NOTIFY_CHANGE_SIZE = 8;
public static readonly UntypedInt FILE_NOTIFY_CHANGE_LAST_WRITE = 16;
public static readonly UntypedInt FILE_NOTIFY_CHANGE_LAST_ACCESS = 32;
public static readonly UntypedInt FILE_NOTIFY_CHANGE_CREATION = 64;

public static readonly UntypedInt FILE_ACTION_ADDED = /* iota + 1 */ 1;
public static readonly UntypedInt FILE_ACTION_REMOVED = 2;
public static readonly UntypedInt FILE_ACTION_MODIFIED = 3;
public static readonly UntypedInt FILE_ACTION_RENAMED_OLD_NAME = 4;
public static readonly UntypedInt FILE_ACTION_RENAMED_NEW_NAME = 5;

public static readonly UntypedInt PROV_RSA_FULL = 1;
public static readonly UntypedInt PROV_RSA_SIG = 2;
public static readonly UntypedInt PROV_DSS = 3;
public static readonly UntypedInt PROV_FORTEZZA = 4;
public static readonly UntypedInt PROV_MS_EXCHANGE = 5;
public static readonly UntypedInt PROV_SSL = 6;
public static readonly UntypedInt PROV_RSA_SCHANNEL = 12;
public static readonly UntypedInt PROV_DSS_DH = 13;
public static readonly UntypedInt PROV_EC_ECDSA_SIG = 14;
public static readonly UntypedInt PROV_EC_ECNRA_SIG = 15;
public static readonly UntypedInt PROV_EC_ECDSA_FULL = 16;
public static readonly UntypedInt PROV_EC_ECNRA_FULL = 17;
public static readonly UntypedInt PROV_DH_SCHANNEL = 18;
public static readonly UntypedInt PROV_SPYRUS_LYNKS = 20;
public static readonly UntypedInt PROV_RNG = 21;
public static readonly UntypedInt PROV_INTEL_SEC = 22;
public static readonly UntypedInt PROV_REPLACE_OWF = 23;
public static readonly UntypedInt PROV_RSA_AES = 24;
public static readonly UntypedInt CRYPT_VERIFYCONTEXT = /* 0xF0000000 */ 4026531840;
public static readonly UntypedInt CRYPT_NEWKEYSET = /* 0x00000008 */ 8;
public static readonly UntypedInt CRYPT_DELETEKEYSET = /* 0x00000010 */ 16;
public static readonly UntypedInt CRYPT_MACHINE_KEYSET = /* 0x00000020 */ 32;
public static readonly UntypedInt CRYPT_SILENT = /* 0x00000040 */ 64;
public static readonly UntypedInt CRYPT_DEFAULT_CONTAINER_OPTIONAL = /* 0x00000080 */ 128;
public static readonly UntypedInt USAGE_MATCH_TYPE_AND = 0;
public static readonly UntypedInt USAGE_MATCH_TYPE_OR = 1;
public static readonly UntypedInt X509_ASN_ENCODING = /* 0x00000001 */ 1;
public static readonly UntypedInt PKCS_7_ASN_ENCODING = /* 0x00010000 */ 65536;
public static readonly UntypedInt CERT_STORE_PROV_MEMORY = 2;
public static readonly UntypedInt CERT_STORE_ADD_ALWAYS = 4;
public static readonly UntypedInt CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = /* 0x00000004 */ 4;
public static readonly UntypedInt CERT_TRUST_NO_ERROR = /* 0x00000000 */ 0;
public static readonly UntypedInt CERT_TRUST_IS_NOT_TIME_VALID = /* 0x00000001 */ 1;
public static readonly UntypedInt CERT_TRUST_IS_REVOKED = /* 0x00000004 */ 4;
public static readonly UntypedInt CERT_TRUST_IS_NOT_SIGNATURE_VALID = /* 0x00000008 */ 8;
public static readonly UntypedInt CERT_TRUST_IS_NOT_VALID_FOR_USAGE = /* 0x00000010 */ 16;
public static readonly UntypedInt CERT_TRUST_IS_UNTRUSTED_ROOT = /* 0x00000020 */ 32;
public static readonly UntypedInt CERT_TRUST_REVOCATION_STATUS_UNKNOWN = /* 0x00000040 */ 64;
public static readonly UntypedInt CERT_TRUST_IS_CYCLIC = /* 0x00000080 */ 128;
public static readonly UntypedInt CERT_TRUST_INVALID_EXTENSION = /* 0x00000100 */ 256;
public static readonly UntypedInt CERT_TRUST_INVALID_POLICY_CONSTRAINTS = /* 0x00000200 */ 512;
public static readonly UntypedInt CERT_TRUST_INVALID_BASIC_CONSTRAINTS = /* 0x00000400 */ 1024;
public static readonly UntypedInt CERT_TRUST_INVALID_NAME_CONSTRAINTS = /* 0x00000800 */ 2048;
public static readonly UntypedInt CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT = /* 0x00001000 */ 4096;
public static readonly UntypedInt CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT = /* 0x00002000 */ 8192;
public static readonly UntypedInt CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT = /* 0x00004000 */ 16384;
public static readonly UntypedInt CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT = /* 0x00008000 */ 32768;
public static readonly UntypedInt CERT_TRUST_IS_OFFLINE_REVOCATION = /* 0x01000000 */ 16777216;
public static readonly UntypedInt CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY = /* 0x02000000 */ 33554432;
public static readonly UntypedInt CERT_TRUST_IS_EXPLICIT_DISTRUST = /* 0x04000000 */ 67108864;
public static readonly UntypedInt CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT = /* 0x08000000 */ 134217728;
public static readonly UntypedInt CERT_CHAIN_POLICY_BASE = 1;
public static readonly UntypedInt CERT_CHAIN_POLICY_AUTHENTICODE = 2;
public static readonly UntypedInt CERT_CHAIN_POLICY_AUTHENTICODE_TS = 3;
public static readonly UntypedInt CERT_CHAIN_POLICY_SSL = 4;
public static readonly UntypedInt CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5;
public static readonly UntypedInt CERT_CHAIN_POLICY_NT_AUTH = 6;
public static readonly UntypedInt CERT_CHAIN_POLICY_MICROSOFT_ROOT = 7;
public static readonly UntypedInt CERT_CHAIN_POLICY_EV = 8;
public static readonly UntypedInt CERT_E_EXPIRED = /* 0x800B0101 */ 2148204801;
public static readonly UntypedInt CERT_E_ROLE = /* 0x800B0103 */ 2148204803;
public static readonly UntypedInt CERT_E_PURPOSE = /* 0x800B0106 */ 2148204806;
public static readonly UntypedInt CERT_E_UNTRUSTEDROOT = /* 0x800B0109 */ 2148204809;
public static readonly UntypedInt CERT_E_CN_NO_MATCH = /* 0x800B010F */ 2148204815;
public static readonly UntypedInt AUTHTYPE_CLIENT = 1;
public static readonly UntypedInt AUTHTYPE_SERVER = 2;

public static slice<byte> OID_PKIX_KP_SERVER_AUTH = slice<byte>("1.3.6.1.5.5.7.3.1\x00");
public static slice<byte> OID_SERVER_GATED_CRYPTO = slice<byte>("1.3.6.1.4.1.311.10.3.3\x00");
public static slice<byte> OID_SGC_NETSCAPE = slice<byte>("2.16.840.1.113730.4.1\x00");

[GoType("dyn")] partial struct Δtype {
}
Δtype.val
// Invented values to support what package os expects.
[GoType] partial struct Timeval {
    public int32 Sec;
    public int32 Usec;
}

[GoRecv] public static int64 Nanoseconds(this ref Timeval tv) {
    return (((int64)tv.Sec) * 1e6F + ((int64)tv.Usec)) * 1e3F;
}

public static Timeval /*tv*/ NsecToTimeval(int64 nsec) {
    Timeval tv = default!;

    tv.Sec = ((int32)(nsec / 1e9F));
    tv.Usec = ((int32)(nsec % 1e9F / 1e3F));
    return tv;
}

[GoType] partial struct SecurityAttributes {
    public uint32 Length;
    public uintptr SecurityDescriptor;
    public uint32 InheritHandle;
}

[GoType] partial struct Overlapped {
    public uintptr Internal;
    public uintptr InternalHigh;
    public uint32 Offset;
    public uint32 OffsetHigh;
    public ΔHandle HEvent;
}

[GoType] partial struct FileNotifyInformation {
    public uint32 NextEntryOffset;
    public uint32 Action;
    public uint32 FileNameLength;
    public uint16 FileName;
}

[GoType] partial struct Filetime {
    public uint32 LowDateTime;
    public uint32 HighDateTime;
}

// Nanoseconds returns Filetime ft in nanoseconds
// since Epoch (00:00:00 UTC, January 1, 1970).
[GoRecv] public static int64 Nanoseconds(this ref Filetime ft) {
    // 100-nanosecond intervals since January 1, 1601
    var nsec = ((int64)ft.HighDateTime) << (int)(32) + ((int64)ft.LowDateTime);
    // change starting time to the Epoch (00:00:00 UTC, January 1, 1970)
    nsec -= (nint)116444736000000000L;
    // convert into nanoseconds
    nsec *= 100;
    return nsec;
}

public static Filetime /*ft*/ NsecToFiletime(int64 nsec) {
    Filetime ft = default!;

    // convert into 100-nanosecond
    nsec /= 100;
    // change starting time to January 1, 1601
    nsec += (nint)116444736000000000L;
    // split into high / low
    ft.LowDateTime = ((uint32)((int64)(nsec & (nint)4294967295L)));
    ft.HighDateTime = ((uint32)((int64)(nsec >> (int)(32) & (nint)4294967295L)));
    return ft;
}

[GoType] partial struct Win32finddata {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    public uint32 Reserved0;
    public uint32 Reserved1;
    public array<uint16> FileName = new(MAX_PATH - 1);
    public array<uint16> AlternateFileName = new(13);
}

// This is the actual system call structure.
// Win32finddata is what we committed to in Go 1.
[GoType] partial struct win32finddata1 {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    public uint32 Reserved0;
    public uint32 Reserved1;
    public array<uint16> FileName = new(MAX_PATH);
    public array<uint16> AlternateFileName = new(14);
}

// The Microsoft documentation for this struct¹ describes three additional
// fields: dwFileType, dwCreatorType, and wFinderFlags. However, those fields
// are empirically only present in the macOS port of the Win32 API,² and thus
// not needed for binaries built for Windows.
//
// ¹ https://docs.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-win32_find_dataw
// ² https://golang.org/issue/42637#issuecomment-760715755
internal static void copyFindData(ж<Win32finddata> Ꮡdst, ж<win32finddata1> Ꮡsrc) {
    ref var dst = ref Ꮡdst.val;
    ref var src = ref Ꮡsrc.val;

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

[GoType] partial struct ByHandleFileInformation {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 VolumeSerialNumber;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
    public uint32 NumberOfLinks;
    public uint32 FileIndexHigh;
    public uint32 FileIndexLow;
}

public static readonly UntypedInt GetFileExInfoStandard = 0;
public static readonly UntypedInt GetFileExMaxInfoLevel = 1;

[GoType] partial struct Win32FileAttributeData {
    public uint32 FileAttributes;
    public Filetime CreationTime;
    public Filetime LastAccessTime;
    public Filetime LastWriteTime;
    public uint32 FileSizeHigh;
    public uint32 FileSizeLow;
}

// ShowWindow constants
public static readonly UntypedInt SW_HIDE = 0;

public static readonly UntypedInt SW_NORMAL = 1;

public static readonly UntypedInt SW_SHOWNORMAL = 1;

public static readonly UntypedInt SW_SHOWMINIMIZED = 2;

public static readonly UntypedInt SW_SHOWMAXIMIZED = 3;

public static readonly UntypedInt SW_MAXIMIZE = 3;

public static readonly UntypedInt SW_SHOWNOACTIVATE = 4;

public static readonly UntypedInt SW_SHOW = 5;

public static readonly UntypedInt SW_MINIMIZE = 6;

public static readonly UntypedInt SW_SHOWMINNOACTIVE = 7;

public static readonly UntypedInt SW_SHOWNA = 8;

public static readonly UntypedInt SW_RESTORE = 9;

public static readonly UntypedInt SW_SHOWDEFAULT = 10;

public static readonly UntypedInt SW_FORCEMINIMIZE = 11;

[GoType] partial struct StartupInfo {
    public uint32 Cb;
    internal ж<uint16> _;
    public ж<uint16> Desktop;
    public ж<uint16> Title;
    public uint32 X;
    public uint32 Y;
    public uint32 XSize;
    public uint32 YSize;
    public uint32 XCountChars;
    public uint32 YCountChars;
    public uint32 FillAttribute;
    public uint32 Flags;
    public uint16 ShowWindow;
    internal uint16 __;
    internal ж<byte> ___;
    public ΔHandle StdInput;
    public ΔHandle StdOutput;
    public ΔHandle StdErr;
}

[GoType] partial struct _PROC_THREAD_ATTRIBUTE_LIST {
    internal array<byte> _ = new(1);
}

internal static readonly UntypedInt _PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = /* 0x00020000 */ 131072;
internal static readonly UntypedInt _PROC_THREAD_ATTRIBUTE_HANDLE_LIST = /* 0x00020002 */ 131074;

[GoType] partial struct _STARTUPINFOEXW {
    public partial ref StartupInfo StartupInfo { get; }
    public ж<_PROC_THREAD_ATTRIBUTE_LIST> ProcThreadAttributeList;
}

internal static readonly UntypedInt _EXTENDED_STARTUPINFO_PRESENT = /* 0x00080000 */ 524288;

[GoType] partial struct ProcessInformation {
    public ΔHandle Process;
    public ΔHandle Thread;
    public uint32 ProcessId;
    public uint32 ThreadId;
}

[GoType] partial struct ProcessEntry32 {
    public uint32 Size;
    public uint32 Usage;
    public uint32 ProcessID;
    public uintptr DefaultHeapID;
    public uint32 ModuleID;
    public uint32 Threads;
    public uint32 ParentProcessID;
    public int32 PriClassBase;
    public uint32 Flags;
    public array<uint16> ExeFile = new(MAX_PATH);
}

[GoType] partial struct Systemtime {
    public uint16 Year;
    public uint16 Month;
    public uint16 DayOfWeek;
    public uint16 Day;
    public uint16 Hour;
    public uint16 Minute;
    public uint16 Second;
    public uint16 Milliseconds;
}

[GoType] partial struct Timezoneinformation {
    public int32 Bias;
    public array<uint16> StandardName = new(32);
    public Systemtime StandardDate;
    public int32 StandardBias;
    public array<uint16> DaylightName = new(32);
    public Systemtime DaylightDate;
    public int32 DaylightBias;
}

// Socket related.
public static readonly UntypedInt AF_UNSPEC = 0;
public static readonly UntypedInt AF_UNIX = 1;
public static readonly UntypedInt AF_INET = 2;
public static readonly UntypedInt AF_INET6 = 23;
public static readonly UntypedInt AF_NETBIOS = 17;
public static readonly UntypedInt SOCK_STREAM = 1;
public static readonly UntypedInt SOCK_DGRAM = 2;
public static readonly UntypedInt SOCK_RAW = 3;
public static readonly UntypedInt SOCK_SEQPACKET = 5;
public static readonly UntypedInt IPPROTO_IP = 0;
public static readonly UntypedInt IPPROTO_IPV6 = /* 0x29 */ 41;
public static readonly UntypedInt IPPROTO_TCP = 6;
public static readonly UntypedInt IPPROTO_UDP = 17;
public static readonly UntypedInt SOL_SOCKET = /* 0xffff */ 65535;
public static readonly UntypedInt SO_REUSEADDR = 4;
public static readonly UntypedInt SO_KEEPALIVE = 8;
public static readonly UntypedInt SO_DONTROUTE = 16;
public static readonly UntypedInt SO_BROADCAST = 32;
public static readonly UntypedInt SO_LINGER = 128;
public static readonly UntypedInt SO_RCVBUF = /* 0x1002 */ 4098;
public static readonly UntypedInt SO_SNDBUF = /* 0x1001 */ 4097;
public static readonly UntypedInt SO_UPDATE_ACCEPT_CONTEXT = /* 0x700b */ 28683;
public static readonly UntypedInt SO_UPDATE_CONNECT_CONTEXT = /* 0x7010 */ 28688;
public static readonly UntypedInt IOC_OUT = /* 0x40000000 */ 1073741824;
public static readonly UntypedInt IOC_IN = /* 0x80000000 */ 2147483648;
public static readonly UntypedInt IOC_VENDOR = /* 0x18000000 */ 402653184;
public static readonly UntypedInt IOC_INOUT = /* IOC_IN | IOC_OUT */ 3221225472;
public static readonly UntypedInt IOC_WS2 = /* 0x08000000 */ 134217728;
public static readonly UntypedInt SIO_GET_EXTENSION_FUNCTION_POINTER = /* IOC_INOUT | IOC_WS2 | 6 */ 3355443206;
public static readonly UntypedInt SIO_KEEPALIVE_VALS = /* IOC_IN | IOC_VENDOR | 4 */ 2550136836;
public static readonly UntypedInt SIO_UDP_CONNRESET = /* IOC_IN | IOC_VENDOR | 12 */ 2550136844;
// cf. https://learn.microsoft.com/en-US/troubleshoot/windows/win32/header-library-requirement-socket-ipproto-ip
public static readonly UntypedInt IP_TOS = /* 0x3 */ 3;
public static readonly UntypedInt IP_TTL = /* 0x4 */ 4;
public static readonly UntypedInt IP_MULTICAST_IF = /* 0x9 */ 9;
public static readonly UntypedInt IP_MULTICAST_TTL = /* 0xa */ 10;
public static readonly UntypedInt IP_MULTICAST_LOOP = /* 0xb */ 11;
public static readonly UntypedInt IP_ADD_MEMBERSHIP = /* 0xc */ 12;
public static readonly UntypedInt IP_DROP_MEMBERSHIP = /* 0xd */ 13;
public static readonly UntypedInt IPV6_V6ONLY = /* 0x1b */ 27;
public static readonly UntypedInt IPV6_UNICAST_HOPS = /* 0x4 */ 4;
public static readonly UntypedInt IPV6_MULTICAST_IF = /* 0x9 */ 9;
public static readonly UntypedInt IPV6_MULTICAST_HOPS = /* 0xa */ 10;
public static readonly UntypedInt IPV6_MULTICAST_LOOP = /* 0xb */ 11;
public static readonly UntypedInt IPV6_JOIN_GROUP = /* 0xc */ 12;
public static readonly UntypedInt IPV6_LEAVE_GROUP = /* 0xd */ 13;
public static readonly UntypedInt SOMAXCONN = /* 0x7fffffff */ 2147483647;
public static readonly UntypedInt TCP_NODELAY = 1;
public static readonly UntypedInt SHUT_RD = 0;
public static readonly UntypedInt SHUT_WR = 1;
public static readonly UntypedInt SHUT_RDWR = 2;
public static readonly UntypedInt WSADESCRIPTION_LEN = 256;
public static readonly UntypedInt WSASYS_STATUS_LEN = 128;

[GoType] partial struct WSABuf {
    public uint32 Len;
    public ж<byte> Buf;
}

// Invented values to support what package os expects.
public static readonly UntypedInt S_IFMT = /* 0x1f000 */ 126976;

public static readonly UntypedInt S_IFIFO = /* 0x1000 */ 4096;

public static readonly UntypedInt S_IFCHR = /* 0x2000 */ 8192;

public static readonly UntypedInt S_IFDIR = /* 0x4000 */ 16384;

public static readonly UntypedInt S_IFBLK = /* 0x6000 */ 24576;

public static readonly UntypedInt S_IFREG = /* 0x8000 */ 32768;

public static readonly UntypedInt S_IFLNK = /* 0xa000 */ 40960;

public static readonly UntypedInt S_IFSOCK = /* 0xc000 */ 49152;

public static readonly UntypedInt S_ISUID = /* 0x800 */ 2048;

public static readonly UntypedInt S_ISGID = /* 0x400 */ 1024;

public static readonly UntypedInt S_ISVTX = /* 0x200 */ 512;

public static readonly UntypedInt S_IRUSR = /* 0x100 */ 256;

public static readonly UntypedInt S_IWRITE = /* 0x80 */ 128;

public static readonly UntypedInt S_IWUSR = /* 0x80 */ 128;

public static readonly UntypedInt S_IXUSR = /* 0x40 */ 64;

public static readonly UntypedInt FILE_TYPE_CHAR = /* 0x0002 */ 2;
public static readonly UntypedInt FILE_TYPE_DISK = /* 0x0001 */ 1;
public static readonly UntypedInt FILE_TYPE_PIPE = /* 0x0003 */ 3;
public static readonly UntypedInt FILE_TYPE_REMOTE = /* 0x8000 */ 32768;
public static readonly UntypedInt FILE_TYPE_UNKNOWN = /* 0x0000 */ 0;

[GoType] partial struct Hostent {
    public ж<byte> Name;
    public ж<ж<byte>> Aliases;
    public uint16 AddrType;
    public uint16 Length;
    public ж<ж<byte>> AddrList;
}

[GoType] partial struct Protoent {
    public ж<byte> Name;
    public ж<ж<byte>> Aliases;
    public uint16 Proto;
}

public static readonly UntypedInt DNS_TYPE_A = /* 0x0001 */ 1;
public static readonly UntypedInt DNS_TYPE_NS = /* 0x0002 */ 2;
public static readonly UntypedInt DNS_TYPE_MD = /* 0x0003 */ 3;
public static readonly UntypedInt DNS_TYPE_MF = /* 0x0004 */ 4;
public static readonly UntypedInt DNS_TYPE_CNAME = /* 0x0005 */ 5;
public static readonly UntypedInt DNS_TYPE_SOA = /* 0x0006 */ 6;
public static readonly UntypedInt DNS_TYPE_MB = /* 0x0007 */ 7;
public static readonly UntypedInt DNS_TYPE_MG = /* 0x0008 */ 8;
public static readonly UntypedInt DNS_TYPE_MR = /* 0x0009 */ 9;
public static readonly UntypedInt DNS_TYPE_NULL = /* 0x000a */ 10;
public static readonly UntypedInt DNS_TYPE_WKS = /* 0x000b */ 11;
public static readonly UntypedInt DNS_TYPE_PTR = /* 0x000c */ 12;
public static readonly UntypedInt DNS_TYPE_HINFO = /* 0x000d */ 13;
public static readonly UntypedInt DNS_TYPE_MINFO = /* 0x000e */ 14;
public static readonly UntypedInt DNS_TYPE_MX = /* 0x000f */ 15;
public static readonly UntypedInt DNS_TYPE_TEXT = /* 0x0010 */ 16;
public static readonly UntypedInt DNS_TYPE_RP = /* 0x0011 */ 17;
public static readonly UntypedInt DNS_TYPE_AFSDB = /* 0x0012 */ 18;
public static readonly UntypedInt DNS_TYPE_X25 = /* 0x0013 */ 19;
public static readonly UntypedInt DNS_TYPE_ISDN = /* 0x0014 */ 20;
public static readonly UntypedInt DNS_TYPE_RT = /* 0x0015 */ 21;
public static readonly UntypedInt DNS_TYPE_NSAP = /* 0x0016 */ 22;
public static readonly UntypedInt DNS_TYPE_NSAPPTR = /* 0x0017 */ 23;
public static readonly UntypedInt DNS_TYPE_SIG = /* 0x0018 */ 24;
public static readonly UntypedInt DNS_TYPE_KEY = /* 0x0019 */ 25;
public static readonly UntypedInt DNS_TYPE_PX = /* 0x001a */ 26;
public static readonly UntypedInt DNS_TYPE_GPOS = /* 0x001b */ 27;
public static readonly UntypedInt DNS_TYPE_AAAA = /* 0x001c */ 28;
public static readonly UntypedInt DNS_TYPE_LOC = /* 0x001d */ 29;
public static readonly UntypedInt DNS_TYPE_NXT = /* 0x001e */ 30;
public static readonly UntypedInt DNS_TYPE_EID = /* 0x001f */ 31;
public static readonly UntypedInt DNS_TYPE_NIMLOC = /* 0x0020 */ 32;
public static readonly UntypedInt DNS_TYPE_SRV = /* 0x0021 */ 33;
public static readonly UntypedInt DNS_TYPE_ATMA = /* 0x0022 */ 34;
public static readonly UntypedInt DNS_TYPE_NAPTR = /* 0x0023 */ 35;
public static readonly UntypedInt DNS_TYPE_KX = /* 0x0024 */ 36;
public static readonly UntypedInt DNS_TYPE_CERT = /* 0x0025 */ 37;
public static readonly UntypedInt DNS_TYPE_A6 = /* 0x0026 */ 38;
public static readonly UntypedInt DNS_TYPE_DNAME = /* 0x0027 */ 39;
public static readonly UntypedInt DNS_TYPE_SINK = /* 0x0028 */ 40;
public static readonly UntypedInt DNS_TYPE_OPT = /* 0x0029 */ 41;
public static readonly UntypedInt DNS_TYPE_DS = /* 0x002B */ 43;
public static readonly UntypedInt DNS_TYPE_RRSIG = /* 0x002E */ 46;
public static readonly UntypedInt DNS_TYPE_NSEC = /* 0x002F */ 47;
public static readonly UntypedInt DNS_TYPE_DNSKEY = /* 0x0030 */ 48;
public static readonly UntypedInt DNS_TYPE_DHCID = /* 0x0031 */ 49;
public static readonly UntypedInt DNS_TYPE_UINFO = /* 0x0064 */ 100;
public static readonly UntypedInt DNS_TYPE_UID = /* 0x0065 */ 101;
public static readonly UntypedInt DNS_TYPE_GID = /* 0x0066 */ 102;
public static readonly UntypedInt DNS_TYPE_UNSPEC = /* 0x0067 */ 103;
public static readonly UntypedInt DNS_TYPE_ADDRS = /* 0x00f8 */ 248;
public static readonly UntypedInt DNS_TYPE_TKEY = /* 0x00f9 */ 249;
public static readonly UntypedInt DNS_TYPE_TSIG = /* 0x00fa */ 250;
public static readonly UntypedInt DNS_TYPE_IXFR = /* 0x00fb */ 251;
public static readonly UntypedInt DNS_TYPE_AXFR = /* 0x00fc */ 252;
public static readonly UntypedInt DNS_TYPE_MAILB = /* 0x00fd */ 253;
public static readonly UntypedInt DNS_TYPE_MAILA = /* 0x00fe */ 254;
public static readonly UntypedInt DNS_TYPE_ALL = /* 0x00ff */ 255;
public static readonly UntypedInt DNS_TYPE_ANY = /* 0x00ff */ 255;
public static readonly UntypedInt DNS_TYPE_WINS = /* 0xff01 */ 65281;
public static readonly UntypedInt DNS_TYPE_WINSR = /* 0xff02 */ 65282;
public static readonly UntypedInt DNS_TYPE_NBSTAT = /* 0xff01 */ 65281;

public static readonly UntypedInt DNS_INFO_NO_RECORDS = /* 0x251D */ 9501;

public static readonly UntypedInt DnsSectionQuestion = /* 0x0000 */ 0;
public static readonly UntypedInt DnsSectionAnswer = /* 0x0001 */ 1;
public static readonly UntypedInt DnsSectionAuthority = /* 0x0002 */ 2;
public static readonly UntypedInt DnsSectionAdditional = /* 0x0003 */ 3;

[GoType] partial struct DNSSRVData {
    public ж<uint16> Target;
    public uint16 Priority;
    public uint16 Weight;
    public uint16 Port;
    public uint16 Pad;
}

[GoType] partial struct DNSPTRData {
    public ж<uint16> Host;
}

[GoType] partial struct DNSMXData {
    public ж<uint16> NameExchange;
    public uint16 Preference;
    public uint16 Pad;
}

[GoType] partial struct DNSTXTData {
    public uint16 StringCount;
    public array<ж<uint16>> StringArray = new(1);
}

[GoType] partial struct DNSRecord {
    public ж<DNSRecord> Next;
    public ж<uint16> Name;
    public uint16 Type;
    public uint16 Length;
    public uint32 Dw;
    public uint32 Ttl;
    public uint32 Reserved;
    public array<byte> Data = new(40);
}

public static readonly UntypedInt TF_DISCONNECT = 1;
public static readonly UntypedInt TF_REUSE_SOCKET = 2;
public static readonly UntypedInt TF_WRITE_BEHIND = 4;
public static readonly UntypedInt TF_USE_DEFAULT_WORKER = 0;
public static readonly UntypedInt TF_USE_SYSTEM_THREAD = 16;
public static readonly UntypedInt TF_USE_KERNEL_APC = 32;

[GoType] partial struct TransmitFileBuffers {
    public uintptr Head;
    public uint32 HeadLength;
    public uintptr Tail;
    public uint32 TailLength;
}

public static readonly UntypedInt IFF_UP = 1;
public static readonly UntypedInt IFF_BROADCAST = 2;
public static readonly UntypedInt IFF_LOOPBACK = 4;
public static readonly UntypedInt IFF_POINTTOPOINT = 8;
public static readonly UntypedInt IFF_MULTICAST = 16;

public static readonly UntypedInt SIO_GET_INTERFACE_LIST = /* 0x4004747F */ 1074033791;

[GoType("[24]byte")] partial struct SockaddrGen;

// TODO(mattn): SockaddrGen is union of sockaddr/sockaddr_in/sockaddr_in6_old.
// will be fixed to change variable type as suitable.
[GoType] partial struct InterfaceInfo {
    public uint32 Flags;
    public SockaddrGen Address;
    public SockaddrGen BroadcastAddress;
    public SockaddrGen Netmask;
}

[GoType] partial struct IpAddressString {
    public array<byte> String = new(16);
}

[GoType("struct{String <16>byte}")] partial struct IpMaskString;

[GoType] partial struct IpAddrString {
    public ж<IpAddrString> Next;
    public IpAddressString IpAddress;
    public IpMaskString IpMask;
    public uint32 Context;
}

public static readonly UntypedInt MAX_ADAPTER_NAME_LENGTH = 256;

public static readonly UntypedInt MAX_ADAPTER_DESCRIPTION_LENGTH = 128;

public static readonly UntypedInt MAX_ADAPTER_ADDRESS_LENGTH = 8;

[GoType] partial struct IpAdapterInfo {
    public ж<IpAdapterInfo> Next;
    public uint32 ComboIndex;
    public array<byte> AdapterName = new(MAX_ADAPTER_NAME_LENGTH + 4);
    public array<byte> Description = new(MAX_ADAPTER_DESCRIPTION_LENGTH + 4);
    public uint32 AddressLength;
    public array<byte> Address = new(MAX_ADAPTER_ADDRESS_LENGTH);
    public uint32 Index;
    public uint32 Type;
    public uint32 DhcpEnabled;
    public ж<IpAddrString> CurrentIpAddress;
    public IpAddrString IpAddressList;
    public IpAddrString GatewayList;
    public IpAddrString DhcpServer;
    public bool HaveWins;
    public IpAddrString PrimaryWinsServer;
    public IpAddrString SecondaryWinsServer;
    public int64 LeaseObtained;
    public int64 LeaseExpires;
}

public static readonly UntypedInt MAXLEN_PHYSADDR = 8;

public static readonly UntypedInt MAX_INTERFACE_NAME_LEN = 256;

public static readonly UntypedInt MAXLEN_IFDESCR = 256;

[GoType] partial struct MibIfRow {
    public array<uint16> Name = new(MAX_INTERFACE_NAME_LEN);
    public uint32 Index;
    public uint32 Type;
    public uint32 Mtu;
    public uint32 Speed;
    public uint32 PhysAddrLen;
    public array<byte> PhysAddr = new(MAXLEN_PHYSADDR);
    public uint32 AdminStatus;
    public uint32 OperStatus;
    public uint32 LastChange;
    public uint32 InOctets;
    public uint32 InUcastPkts;
    public uint32 InNUcastPkts;
    public uint32 InDiscards;
    public uint32 InErrors;
    public uint32 InUnknownProtos;
    public uint32 OutOctets;
    public uint32 OutUcastPkts;
    public uint32 OutNUcastPkts;
    public uint32 OutDiscards;
    public uint32 OutErrors;
    public uint32 OutQLen;
    public uint32 DescrLen;
    public array<byte> Descr = new(MAXLEN_IFDESCR);
}

[GoType] partial struct CertInfo {
}

// Not implemented
[GoType] partial struct CertContext {
    public uint32 EncodingType;
    public ж<byte> EncodedCert;
    public uint32 Length;
    public ж<CertInfo> CertInfo;
    public ΔHandle Store;
}

[GoType] partial struct CertChainContext {
    public uint32 Size;
    public CertTrustStatus TrustStatus;
    public uint32 ChainCount;
    public ж<ж<CertSimpleChain>> Chains;
    public uint32 LowerQualityChainCount;
    public ж<ж<CertChainContext>> LowerQualityChains;
    public uint32 HasRevocationFreshnessTime;
    public uint32 RevocationFreshnessTime;
}

[GoType] partial struct CertTrustListInfo {
}

// Not implemented
[GoType] partial struct CertSimpleChain {
    public uint32 Size;
    public CertTrustStatus TrustStatus;
    public uint32 NumElements;
    public ж<ж<CertChainElement>> Elements;
    public ж<CertTrustListInfo> TrustListInfo;
    public uint32 HasRevocationFreshnessTime;
    public uint32 RevocationFreshnessTime;
}

[GoType] partial struct CertChainElement {
    public uint32 Size;
    public ж<CertContext> CertContext;
    public CertTrustStatus TrustStatus;
    public ж<CertRevocationInfo> RevocationInfo;
    public ж<CertEnhKeyUsage> IssuanceUsage;
    public ж<CertEnhKeyUsage> ApplicationUsage;
    public ж<uint16> ExtendedErrorInfo;
}

[GoType] partial struct CertRevocationCrlInfo {
}

// Not implemented
[GoType] partial struct CertRevocationInfo {
    public uint32 Size;
    public uint32 RevocationResult;
    public ж<byte> RevocationOid;
    public Pointer OidSpecificInfo;
    public uint32 HasFreshnessTime;
    public uint32 FreshnessTime;
    public ж<CertRevocationCrlInfo> CrlInfo;
}

[GoType] partial struct CertTrustStatus {
    public uint32 ErrorStatus;
    public uint32 InfoStatus;
}

[GoType] partial struct CertUsageMatch {
    public uint32 Type;
    public CertEnhKeyUsage Usage;
}

[GoType] partial struct CertEnhKeyUsage {
    public uint32 Length;
    public ж<ж<byte>> UsageIdentifiers;
}

[GoType] partial struct CertChainPara {
    public uint32 Size;
    public CertUsageMatch RequestedUsage;
    public CertUsageMatch RequstedIssuancePolicy;
    public uint32 URLRetrievalTimeout;
    public uint32 CheckRevocationFreshnessTime;
    public uint32 RevocationFreshnessTime;
    public ж<Filetime> CacheResync;
}

[GoType] partial struct CertChainPolicyPara {
    public uint32 Size;
    public uint32 Flags;
    public Pointer ExtraPolicyPara;
}

[GoType] partial struct SSLExtraCertChainPolicyPara {
    public uint32 Size;
    public uint32 AuthType;
    public uint32 Checks;
    public ж<uint16> ServerName;
}

[GoType] partial struct CertChainPolicyStatus {
    public uint32 Size;
    public uint32 Error;
    public uint32 ChainIndex;
    public uint32 ElementIndex;
    public Pointer ExtraPolicyStatus;
}

public static readonly UntypedInt HKEY_CLASSES_ROOT = /* 0x80000000 + iota */ 2147483648;
public static readonly UntypedInt HKEY_CURRENT_USER = 2147483649;
public static readonly UntypedInt HKEY_LOCAL_MACHINE = 2147483650;
public static readonly UntypedInt HKEY_USERS = 2147483651;
public static readonly UntypedInt HKEY_PERFORMANCE_DATA = 2147483652;
public static readonly UntypedInt HKEY_CURRENT_CONFIG = 2147483653;
public static readonly UntypedInt HKEY_DYN_DATA = 2147483654;
public static readonly UntypedInt KEY_QUERY_VALUE = 1;
public static readonly UntypedInt KEY_SET_VALUE = 2;
public static readonly UntypedInt KEY_CREATE_SUB_KEY = 4;
public static readonly UntypedInt KEY_ENUMERATE_SUB_KEYS = 8;
public static readonly UntypedInt KEY_NOTIFY = 16;
public static readonly UntypedInt KEY_CREATE_LINK = 32;
public static readonly UntypedInt KEY_WRITE = /* 0x20006 */ 131078;
public static readonly UntypedInt KEY_EXECUTE = /* 0x20019 */ 131097;
public static readonly UntypedInt KEY_READ = /* 0x20019 */ 131097;
public static readonly UntypedInt KEY_WOW64_64KEY = /* 0x0100 */ 256;
public static readonly UntypedInt KEY_WOW64_32KEY = /* 0x0200 */ 512;
public static readonly UntypedInt KEY_ALL_ACCESS = /* 0xf003f */ 983103;

public static readonly UntypedInt REG_NONE = iota;
public static readonly UntypedInt REG_SZ = 1;
public static readonly UntypedInt REG_EXPAND_SZ = 2;
public static readonly UntypedInt REG_BINARY = 3;
public static readonly UntypedInt REG_DWORD_LITTLE_ENDIAN = 4;
public static readonly UntypedInt REG_DWORD_BIG_ENDIAN = 5;
public static readonly UntypedInt REG_LINK = 6;
public static readonly UntypedInt REG_MULTI_SZ = 7;
public static readonly UntypedInt REG_RESOURCE_LIST = 8;
public static readonly UntypedInt REG_FULL_RESOURCE_DESCRIPTOR = 9;
public static readonly UntypedInt REG_RESOURCE_REQUIREMENTS_LIST = 10;
public static readonly UntypedInt REG_QWORD_LITTLE_ENDIAN = 11;
public static readonly UntypedInt REG_DWORD = /* REG_DWORD_LITTLE_ENDIAN */ 4;
public static readonly UntypedInt REG_QWORD = /* REG_QWORD_LITTLE_ENDIAN */ 11;

[GoType] partial struct AddrinfoW {
    public int32 Flags;
    public int32 Family;
    public int32 Socktype;
    public int32 Protocol;
    public uintptr Addrlen;
    public ж<uint16> Canonname;
    public Pointer Addr;
    public ж<AddrinfoW> Next;
}

public static readonly UntypedInt AI_PASSIVE = 1;
public static readonly UntypedInt AI_CANONNAME = 2;
public static readonly UntypedInt AI_NUMERICHOST = 4;

[GoType] partial struct GUID {
    public uint32 Data1;
    public uint16 Data2;
    public uint16 Data3;
    public array<byte> Data4 = new(8);
}

public static GUID WSAID_CONNECTEX = new GUID(
    631375801,
    56819,
    18016,
    new byte[]{142, 233, 118, 229, 140, 116, 6, 62}.array()
);

public static readonly UntypedInt FILE_SKIP_COMPLETION_PORT_ON_SUCCESS = 1;
public static readonly UntypedInt FILE_SKIP_SET_EVENT_ON_HANDLE = 2;

public static readonly UntypedInt WSAPROTOCOL_LEN = 255;
public static readonly UntypedInt MAX_PROTOCOL_CHAIN = 7;
public static readonly UntypedInt BASE_PROTOCOL = 1;
public static readonly UntypedInt LAYERED_PROTOCOL = 0;
public static readonly UntypedInt XP1_CONNECTIONLESS = /* 0x00000001 */ 1;
public static readonly UntypedInt XP1_GUARANTEED_DELIVERY = /* 0x00000002 */ 2;
public static readonly UntypedInt XP1_GUARANTEED_ORDER = /* 0x00000004 */ 4;
public static readonly UntypedInt XP1_MESSAGE_ORIENTED = /* 0x00000008 */ 8;
public static readonly UntypedInt XP1_PSEUDO_STREAM = /* 0x00000010 */ 16;
public static readonly UntypedInt XP1_GRACEFUL_CLOSE = /* 0x00000020 */ 32;
public static readonly UntypedInt XP1_EXPEDITED_DATA = /* 0x00000040 */ 64;
public static readonly UntypedInt XP1_CONNECT_DATA = /* 0x00000080 */ 128;
public static readonly UntypedInt XP1_DISCONNECT_DATA = /* 0x00000100 */ 256;
public static readonly UntypedInt XP1_SUPPORT_BROADCAST = /* 0x00000200 */ 512;
public static readonly UntypedInt XP1_SUPPORT_MULTIPOINT = /* 0x00000400 */ 1024;
public static readonly UntypedInt XP1_MULTIPOINT_CONTROL_PLANE = /* 0x00000800 */ 2048;
public static readonly UntypedInt XP1_MULTIPOINT_DATA_PLANE = /* 0x00001000 */ 4096;
public static readonly UntypedInt XP1_QOS_SUPPORTED = /* 0x00002000 */ 8192;
public static readonly UntypedInt XP1_UNI_SEND = /* 0x00008000 */ 32768;
public static readonly UntypedInt XP1_UNI_RECV = /* 0x00010000 */ 65536;
public static readonly UntypedInt XP1_IFS_HANDLES = /* 0x00020000 */ 131072;
public static readonly UntypedInt XP1_PARTIAL_MESSAGE = /* 0x00040000 */ 262144;
public static readonly UntypedInt XP1_SAN_SUPPORT_SDP = /* 0x00080000 */ 524288;
public static readonly UntypedInt PFL_MULTIPLE_PROTO_ENTRIES = /* 0x00000001 */ 1;
public static readonly UntypedInt PFL_RECOMMENDED_PROTO_ENTRY = /* 0x00000002 */ 2;
public static readonly UntypedInt PFL_HIDDEN = /* 0x00000004 */ 4;
public static readonly UntypedInt PFL_MATCHES_PROTOCOL_ZERO = /* 0x00000008 */ 8;
public static readonly UntypedInt PFL_NETWORKDIRECT_PROVIDER = /* 0x00000010 */ 16;

[GoType] partial struct WSAProtocolInfo {
    public uint32 ServiceFlags1;
    public uint32 ServiceFlags2;
    public uint32 ServiceFlags3;
    public uint32 ServiceFlags4;
    public uint32 ProviderFlags;
    public GUID ProviderId;
    public uint32 CatalogEntryId;
    public WSAProtocolChain ProtocolChain;
    public int32 Version;
    public int32 AddressFamily;
    public int32 MaxSockAddr;
    public int32 MinSockAddr;
    public int32 SocketType;
    public int32 Protocol;
    public int32 ProtocolMaxOffset;
    public int32 NetworkByteOrder;
    public int32 SecurityScheme;
    public uint32 MessageSize;
    public uint32 ProviderReserved;
    public array<uint16> ProtocolName = new(WSAPROTOCOL_LEN + 1);
}

[GoType] partial struct WSAProtocolChain {
    public int32 ChainLen;
    public array<uint32> ChainEntries = new(MAX_PROTOCOL_CHAIN);
}

[GoType] partial struct TCPKeepalive {
    public uint32 OnOff;
    public uint32 Time;
    public uint32 Interval;
}

[GoType] partial struct symbolicLinkReparseBuffer {
    public uint16 SubstituteNameOffset;
    public uint16 SubstituteNameLength;
    public uint16 PrintNameOffset;
    public uint16 PrintNameLength;
    public uint32 Flags;
    public array<uint16> PathBuffer = new(1);
}

[GoType] partial struct mountPointReparseBuffer {
    public uint16 SubstituteNameOffset;
    public uint16 SubstituteNameLength;
    public uint16 PrintNameOffset;
    public uint16 PrintNameLength;
    public array<uint16> PathBuffer = new(1);
}

[GoType] partial struct reparseDataBuffer {
    public uint32 ReparseTag;
    public uint16 ReparseDataLength;
    public uint16 Reserved;
    // GenericReparseBuffer
    internal byte reparseBuffer;
}

public static readonly UntypedInt FSCTL_GET_REPARSE_POINT = /* 0x900A8 */ 589992;
public static readonly UntypedInt MAXIMUM_REPARSE_DATA_BUFFER_SIZE = /* 16 * 1024 */ 16384;
internal static readonly UntypedInt _IO_REPARSE_TAG_MOUNT_POINT = /* 0xA0000003 */ 2684354563;
public static readonly UntypedInt IO_REPARSE_TAG_SYMLINK = /* 0xA000000C */ 2684354572;
public static readonly UntypedInt SYMBOLIC_LINK_FLAG_DIRECTORY = /* 0x1 */ 1;
internal static readonly UntypedInt _SYMLINK_FLAG_RELATIVE = 1;

public static readonly UntypedInt UNIX_PATH_MAX = 108; // defined in afunix.h

} // end syscall_package
