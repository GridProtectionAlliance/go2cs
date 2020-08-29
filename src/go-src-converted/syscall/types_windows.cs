// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:38:32 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\types_windows.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
 
        // Windows errors.
        public static readonly Errno ERROR_FILE_NOT_FOUND = 2L;
        public static readonly Errno ERROR_PATH_NOT_FOUND = 3L;
        public static readonly Errno ERROR_ACCESS_DENIED = 5L;
        public static readonly Errno ERROR_NO_MORE_FILES = 18L;
        public static readonly Errno ERROR_HANDLE_EOF = 38L;
        public static readonly Errno ERROR_NETNAME_DELETED = 64L;
        public static readonly Errno ERROR_FILE_EXISTS = 80L;
        public static readonly Errno ERROR_BROKEN_PIPE = 109L;
        public static readonly Errno ERROR_BUFFER_OVERFLOW = 111L;
        public static readonly Errno ERROR_INSUFFICIENT_BUFFER = 122L;
        public static readonly Errno ERROR_MOD_NOT_FOUND = 126L;
        public static readonly Errno ERROR_PROC_NOT_FOUND = 127L;
        public static readonly Errno ERROR_DIR_NOT_EMPTY = 145L;
        public static readonly Errno ERROR_ALREADY_EXISTS = 183L;
        public static readonly Errno ERROR_ENVVAR_NOT_FOUND = 203L;
        public static readonly Errno ERROR_MORE_DATA = 234L;
        public static readonly Errno ERROR_OPERATION_ABORTED = 995L;
        public static readonly Errno ERROR_IO_PENDING = 997L;
        public static readonly Errno ERROR_NOT_FOUND = 1168L;
        public static readonly Errno ERROR_PRIVILEGE_NOT_HELD = 1314L;
        public static readonly Errno WSAEACCES = 10013L;
        public static readonly Errno WSAECONNABORTED = 10053L;
        public static readonly Errno WSAECONNRESET = 10054L;

 
        // Invented values to support what package os expects.
        public static readonly ulong O_RDONLY = 0x00000UL;
        public static readonly ulong O_WRONLY = 0x00001UL;
        public static readonly ulong O_RDWR = 0x00002UL;
        public static readonly ulong O_CREAT = 0x00040UL;
        public static readonly ulong O_EXCL = 0x00080UL;
        public static readonly ulong O_NOCTTY = 0x00100UL;
        public static readonly ulong O_TRUNC = 0x00200UL;
        public static readonly ulong O_NONBLOCK = 0x00800UL;
        public static readonly ulong O_APPEND = 0x00400UL;
        public static readonly ulong O_SYNC = 0x01000UL;
        public static readonly ulong O_ASYNC = 0x02000UL;
        public static readonly ulong O_CLOEXEC = 0x80000UL;

 
        // More invented values for signals
        public static readonly var SIGHUP = Signal(0x1UL);
        public static readonly var SIGINT = Signal(0x2UL);
        public static readonly var SIGQUIT = Signal(0x3UL);
        public static readonly var SIGILL = Signal(0x4UL);
        public static readonly var SIGTRAP = Signal(0x5UL);
        public static readonly var SIGABRT = Signal(0x6UL);
        public static readonly var SIGBUS = Signal(0x7UL);
        public static readonly var SIGFPE = Signal(0x8UL);
        public static readonly var SIGKILL = Signal(0x9UL);
        public static readonly var SIGSEGV = Signal(0xbUL);
        public static readonly var SIGPIPE = Signal(0xdUL);
        public static readonly var SIGALRM = Signal(0xeUL);
        public static readonly var SIGTERM = Signal(0xfUL);

        private static array<@string> signals = new array<@string>(InitKeyedValues<@string>((1, "hangup"), (2, "interrupt"), (3, "quit"), (4, "illegal instruction"), (5, "trace/breakpoint trap"), (6, "aborted"), (7, "bus error"), (8, "floating point exception"), (9, "killed"), (10, "user defined signal 1"), (11, "segmentation fault"), (12, "user defined signal 2"), (13, "broken pipe"), (14, "alarm clock"), (15, "terminated")));

        public static readonly ulong GENERIC_READ = 0x80000000UL;
        public static readonly ulong GENERIC_WRITE = 0x40000000UL;
        public static readonly ulong GENERIC_EXECUTE = 0x20000000UL;
        public static readonly ulong GENERIC_ALL = 0x10000000UL;

        public static readonly ulong FILE_LIST_DIRECTORY = 0x00000001UL;
        public static readonly ulong FILE_APPEND_DATA = 0x00000004UL;
        public static readonly ulong FILE_WRITE_ATTRIBUTES = 0x00000100UL;

        public static readonly ulong FILE_SHARE_READ = 0x00000001UL;
        public static readonly ulong FILE_SHARE_WRITE = 0x00000002UL;
        public static readonly ulong FILE_SHARE_DELETE = 0x00000004UL;
        public static readonly ulong FILE_ATTRIBUTE_READONLY = 0x00000001UL;
        public static readonly ulong FILE_ATTRIBUTE_HIDDEN = 0x00000002UL;
        public static readonly ulong FILE_ATTRIBUTE_SYSTEM = 0x00000004UL;
        public static readonly ulong FILE_ATTRIBUTE_DIRECTORY = 0x00000010UL;
        public static readonly ulong FILE_ATTRIBUTE_ARCHIVE = 0x00000020UL;
        public static readonly ulong FILE_ATTRIBUTE_NORMAL = 0x00000080UL;
        public static readonly ulong FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400UL;

        public static readonly ulong INVALID_FILE_ATTRIBUTES = 0xffffffffUL;

        public static readonly long CREATE_NEW = 1L;
        public static readonly long CREATE_ALWAYS = 2L;
        public static readonly long OPEN_EXISTING = 3L;
        public static readonly long OPEN_ALWAYS = 4L;
        public static readonly long TRUNCATE_EXISTING = 5L;

        public static readonly ulong FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000UL;
        public static readonly ulong FILE_FLAG_BACKUP_SEMANTICS = 0x02000000UL;
        public static readonly ulong FILE_FLAG_OVERLAPPED = 0x40000000UL;

        public static readonly ulong HANDLE_FLAG_INHERIT = 0x00000001UL;
        public static readonly ulong STARTF_USESTDHANDLES = 0x00000100UL;
        public static readonly ulong STARTF_USESHOWWINDOW = 0x00000001UL;
        public static readonly ulong DUPLICATE_CLOSE_SOURCE = 0x00000001UL;
        public static readonly ulong DUPLICATE_SAME_ACCESS = 0x00000002UL;

        public static readonly long STD_INPUT_HANDLE = -10L;
        public static readonly long STD_OUTPUT_HANDLE = -11L;
        public static readonly long STD_ERROR_HANDLE = -12L;

        public static readonly long FILE_BEGIN = 0L;
        public static readonly long FILE_CURRENT = 1L;
        public static readonly long FILE_END = 2L;

        public static readonly ulong LANG_ENGLISH = 0x09UL;
        public static readonly ulong SUBLANG_ENGLISH_US = 0x01UL;

        public static readonly long FORMAT_MESSAGE_ALLOCATE_BUFFER = 256L;
        public static readonly long FORMAT_MESSAGE_IGNORE_INSERTS = 512L;
        public static readonly long FORMAT_MESSAGE_FROM_STRING = 1024L;
        public static readonly long FORMAT_MESSAGE_FROM_HMODULE = 2048L;
        public static readonly long FORMAT_MESSAGE_FROM_SYSTEM = 4096L;
        public static readonly long FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192L;
        public static readonly long FORMAT_MESSAGE_MAX_WIDTH_MASK = 255L;

        public static readonly long MAX_PATH = 260L;
        public static readonly long MAX_LONG_PATH = 32768L;

        public static readonly long MAX_COMPUTERNAME_LENGTH = 15L;

        public static readonly long TIME_ZONE_ID_UNKNOWN = 0L;
        public static readonly long TIME_ZONE_ID_STANDARD = 1L;

        public static readonly long TIME_ZONE_ID_DAYLIGHT = 2L;
        public static readonly long IGNORE = 0L;
        public static readonly ulong INFINITE = 0xffffffffUL;

        public static readonly long WAIT_TIMEOUT = 258L;
        public static readonly ulong WAIT_ABANDONED = 0x00000080UL;
        public static readonly ulong WAIT_OBJECT_0 = 0x00000000UL;
        public static readonly ulong WAIT_FAILED = 0xFFFFFFFFUL;

        public static readonly ulong CREATE_NEW_PROCESS_GROUP = 0x00000200UL;
        public static readonly ulong CREATE_UNICODE_ENVIRONMENT = 0x00000400UL;

        public static readonly long PROCESS_TERMINATE = 1L;
        public static readonly ulong PROCESS_QUERY_INFORMATION = 0x00000400UL;
        public static readonly ulong SYNCHRONIZE = 0x00100000UL;

        public static readonly ulong PAGE_READONLY = 0x02UL;
        public static readonly ulong PAGE_READWRITE = 0x04UL;
        public static readonly ulong PAGE_WRITECOPY = 0x08UL;
        public static readonly ulong PAGE_EXECUTE_READ = 0x20UL;
        public static readonly ulong PAGE_EXECUTE_READWRITE = 0x40UL;
        public static readonly ulong PAGE_EXECUTE_WRITECOPY = 0x80UL;

        public static readonly ulong FILE_MAP_COPY = 0x01UL;
        public static readonly ulong FILE_MAP_WRITE = 0x02UL;
        public static readonly ulong FILE_MAP_READ = 0x04UL;
        public static readonly ulong FILE_MAP_EXECUTE = 0x20UL;

        public static readonly long CTRL_C_EVENT = 0L;
        public static readonly long CTRL_BREAK_EVENT = 1L;

 
        // flags for CreateToolhelp32Snapshot
        public static readonly ulong TH32CS_SNAPHEAPLIST = 0x01UL;
        public static readonly ulong TH32CS_SNAPPROCESS = 0x02UL;
        public static readonly ulong TH32CS_SNAPTHREAD = 0x04UL;
        public static readonly ulong TH32CS_SNAPMODULE = 0x08UL;
        public static readonly ulong TH32CS_SNAPMODULE32 = 0x10UL;
        public static readonly var TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD;
        public static readonly ulong TH32CS_INHERIT = 0x80000000UL;

 
        // do not reorder
        public static readonly long FILE_NOTIFY_CHANGE_FILE_NAME = 1L << (int)(iota);
        public static readonly var FILE_NOTIFY_CHANGE_DIR_NAME = 0;
        public static readonly var FILE_NOTIFY_CHANGE_ATTRIBUTES = 1;
        public static readonly var FILE_NOTIFY_CHANGE_SIZE = 2;
        public static readonly var FILE_NOTIFY_CHANGE_LAST_WRITE = 3;
        public static readonly var FILE_NOTIFY_CHANGE_LAST_ACCESS = 4;
        public static readonly var FILE_NOTIFY_CHANGE_CREATION = 5;

 
        // do not reorder
        public static readonly var FILE_ACTION_ADDED = iota + 1L;
        public static readonly var FILE_ACTION_REMOVED = 0;
        public static readonly var FILE_ACTION_MODIFIED = 1;
        public static readonly var FILE_ACTION_RENAMED_OLD_NAME = 2;
        public static readonly var FILE_ACTION_RENAMED_NEW_NAME = 3;

 
        // wincrypt.h
        public static readonly long PROV_RSA_FULL = 1L;
        public static readonly long PROV_RSA_SIG = 2L;
        public static readonly long PROV_DSS = 3L;
        public static readonly long PROV_FORTEZZA = 4L;
        public static readonly long PROV_MS_EXCHANGE = 5L;
        public static readonly long PROV_SSL = 6L;
        public static readonly long PROV_RSA_SCHANNEL = 12L;
        public static readonly long PROV_DSS_DH = 13L;
        public static readonly long PROV_EC_ECDSA_SIG = 14L;
        public static readonly long PROV_EC_ECNRA_SIG = 15L;
        public static readonly long PROV_EC_ECDSA_FULL = 16L;
        public static readonly long PROV_EC_ECNRA_FULL = 17L;
        public static readonly long PROV_DH_SCHANNEL = 18L;
        public static readonly long PROV_SPYRUS_LYNKS = 20L;
        public static readonly long PROV_RNG = 21L;
        public static readonly long PROV_INTEL_SEC = 22L;
        public static readonly long PROV_REPLACE_OWF = 23L;
        public static readonly long PROV_RSA_AES = 24L;
        public static readonly ulong CRYPT_VERIFYCONTEXT = 0xF0000000UL;
        public static readonly ulong CRYPT_NEWKEYSET = 0x00000008UL;
        public static readonly ulong CRYPT_DELETEKEYSET = 0x00000010UL;
        public static readonly ulong CRYPT_MACHINE_KEYSET = 0x00000020UL;
        public static readonly ulong CRYPT_SILENT = 0x00000040UL;
        public static readonly ulong CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x00000080UL;

        public static readonly long USAGE_MATCH_TYPE_AND = 0L;
        public static readonly long USAGE_MATCH_TYPE_OR = 1L;

        public static readonly ulong X509_ASN_ENCODING = 0x00000001UL;
        public static readonly ulong PKCS_7_ASN_ENCODING = 0x00010000UL;

        public static readonly long CERT_STORE_PROV_MEMORY = 2L;

        public static readonly long CERT_STORE_ADD_ALWAYS = 4L;

        public static readonly ulong CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 0x00000004UL;

        public static readonly ulong CERT_TRUST_NO_ERROR = 0x00000000UL;
        public static readonly ulong CERT_TRUST_IS_NOT_TIME_VALID = 0x00000001UL;
        public static readonly ulong CERT_TRUST_IS_REVOKED = 0x00000004UL;
        public static readonly ulong CERT_TRUST_IS_NOT_SIGNATURE_VALID = 0x00000008UL;
        public static readonly ulong CERT_TRUST_IS_NOT_VALID_FOR_USAGE = 0x00000010UL;
        public static readonly ulong CERT_TRUST_IS_UNTRUSTED_ROOT = 0x00000020UL;
        public static readonly ulong CERT_TRUST_REVOCATION_STATUS_UNKNOWN = 0x00000040UL;
        public static readonly ulong CERT_TRUST_IS_CYCLIC = 0x00000080UL;
        public static readonly ulong CERT_TRUST_INVALID_EXTENSION = 0x00000100UL;
        public static readonly ulong CERT_TRUST_INVALID_POLICY_CONSTRAINTS = 0x00000200UL;
        public static readonly ulong CERT_TRUST_INVALID_BASIC_CONSTRAINTS = 0x00000400UL;
        public static readonly ulong CERT_TRUST_INVALID_NAME_CONSTRAINTS = 0x00000800UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT = 0x00001000UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT = 0x00002000UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT = 0x00004000UL;
        public static readonly ulong CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT = 0x00008000UL;
        public static readonly ulong CERT_TRUST_IS_OFFLINE_REVOCATION = 0x01000000UL;
        public static readonly ulong CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY = 0x02000000UL;
        public static readonly ulong CERT_TRUST_IS_EXPLICIT_DISTRUST = 0x04000000UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT = 0x08000000UL;

        public static readonly long CERT_CHAIN_POLICY_BASE = 1L;
        public static readonly long CERT_CHAIN_POLICY_AUTHENTICODE = 2L;
        public static readonly long CERT_CHAIN_POLICY_AUTHENTICODE_TS = 3L;
        public static readonly long CERT_CHAIN_POLICY_SSL = 4L;
        public static readonly long CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5L;
        public static readonly long CERT_CHAIN_POLICY_NT_AUTH = 6L;
        public static readonly long CERT_CHAIN_POLICY_MICROSOFT_ROOT = 7L;
        public static readonly long CERT_CHAIN_POLICY_EV = 8L;

        public static readonly ulong CERT_E_EXPIRED = 0x800B0101UL;
        public static readonly ulong CERT_E_ROLE = 0x800B0103UL;
        public static readonly ulong CERT_E_PURPOSE = 0x800B0106UL;
        public static readonly ulong CERT_E_UNTRUSTEDROOT = 0x800B0109UL;
        public static readonly ulong CERT_E_CN_NO_MATCH = 0x800B010FUL;

        public static readonly long AUTHTYPE_CLIENT = 1L;
        public static readonly long AUTHTYPE_SERVER = 2L;

        public static slice<byte> OID_PKIX_KP_SERVER_AUTH = (slice<byte>)"1.3.6.1.5.5.7.3.1\x00";        public static slice<byte> OID_SERVER_GATED_CRYPTO = (slice<byte>)"1.3.6.1.4.1.311.10.3.3\x00";        public static slice<byte> OID_SGC_NETSCAPE = (slice<byte>)"2.16.840.1.113730.4.1\x00";

        // Invented values to support what package os expects.
        public partial struct Timeval
        {
            public int Sec;
            public int Usec;
        }

        private static long Nanoseconds(this ref Timeval tv)
        {
            return (int64(tv.Sec) * 1e6F + int64(tv.Usec)) * 1e3F;
        }

        public static Timeval NsecToTimeval(long nsec)
        {
            tv.Sec = int32(nsec / 1e9F);
            tv.Usec = int32(nsec % 1e9F / 1e3F);
            return;
        }

        public partial struct SecurityAttributes
        {
            public uint Length;
            public System.UIntPtr SecurityDescriptor;
            public uint InheritHandle;
        }

        public partial struct Overlapped
        {
            public System.UIntPtr Internal;
            public System.UIntPtr InternalHigh;
            public uint Offset;
            public uint OffsetHigh;
            public Handle HEvent;
        }

        public partial struct FileNotifyInformation
        {
            public uint NextEntryOffset;
            public uint Action;
            public uint FileNameLength;
            public ushort FileName;
        }

        public partial struct Filetime
        {
            public uint LowDateTime;
            public uint HighDateTime;
        }

        // Nanoseconds returns Filetime ft in nanoseconds
        // since Epoch (00:00:00 UTC, January 1, 1970).
        private static long Nanoseconds(this ref Filetime ft)
        { 
            // 100-nanosecond intervals since January 1, 1601
            var nsec = int64(ft.HighDateTime) << (int)(32L) + int64(ft.LowDateTime); 
            // change starting time to the Epoch (00:00:00 UTC, January 1, 1970)
            nsec -= 116444736000000000L; 
            // convert into nanoseconds
            nsec *= 100L;
            return nsec;
        }

        public static Filetime NsecToFiletime(long nsec)
        { 
            // convert into 100-nanosecond
            nsec /= 100L; 
            // change starting time to January 1, 1601
            nsec += 116444736000000000L; 
            // split into high / low
            ft.LowDateTime = uint32(nsec & 0xffffffffUL);
            ft.HighDateTime = uint32(nsec >> (int)(32L) & 0xffffffffUL);
            return ft;
        }

        public partial struct Win32finddata
        {
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
        private partial struct win32finddata1
        {
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

        private static void copyFindData(ref Win32finddata dst, ref win32finddata1 src)
        {
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

        public partial struct ByHandleFileInformation
        {
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

        public static readonly long GetFileExInfoStandard = 0L;
        public static readonly long GetFileExMaxInfoLevel = 1L;

        public partial struct Win32FileAttributeData
        {
            public uint FileAttributes;
            public Filetime CreationTime;
            public Filetime LastAccessTime;
            public Filetime LastWriteTime;
            public uint FileSizeHigh;
            public uint FileSizeLow;
        }

        // ShowWindow constants
 
        // winuser.h
        public static readonly long SW_HIDE = 0L;
        public static readonly long SW_NORMAL = 1L;
        public static readonly long SW_SHOWNORMAL = 1L;
        public static readonly long SW_SHOWMINIMIZED = 2L;
        public static readonly long SW_SHOWMAXIMIZED = 3L;
        public static readonly long SW_MAXIMIZE = 3L;
        public static readonly long SW_SHOWNOACTIVATE = 4L;
        public static readonly long SW_SHOW = 5L;
        public static readonly long SW_MINIMIZE = 6L;
        public static readonly long SW_SHOWMINNOACTIVE = 7L;
        public static readonly long SW_SHOWNA = 8L;
        public static readonly long SW_RESTORE = 9L;
        public static readonly long SW_SHOWDEFAULT = 10L;
        public static readonly long SW_FORCEMINIMIZE = 11L;

        public partial struct StartupInfo
        {
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

        public partial struct ProcessInformation
        {
            public Handle Process;
            public Handle Thread;
            public uint ProcessId;
            public uint ThreadId;
        }

        public partial struct ProcessEntry32
        {
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

        public partial struct Systemtime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;
        }

        public partial struct Timezoneinformation
        {
            public int Bias;
            public array<ushort> StandardName;
            public Systemtime StandardDate;
            public int StandardBias;
            public array<ushort> DaylightName;
            public Systemtime DaylightDate;
            public int DaylightBias;
        }

        // Socket related.

        public static readonly long AF_UNSPEC = 0L;
        public static readonly long AF_UNIX = 1L;
        public static readonly long AF_INET = 2L;
        public static readonly long AF_INET6 = 23L;
        public static readonly long AF_NETBIOS = 17L;

        public static readonly long SOCK_STREAM = 1L;
        public static readonly long SOCK_DGRAM = 2L;
        public static readonly long SOCK_RAW = 3L;
        public static readonly long SOCK_SEQPACKET = 5L;

        public static readonly long IPPROTO_IP = 0L;
        public static readonly ulong IPPROTO_IPV6 = 0x29UL;
        public static readonly long IPPROTO_TCP = 6L;
        public static readonly long IPPROTO_UDP = 17L;

        public static readonly ulong SOL_SOCKET = 0xffffUL;
        public static readonly long SO_REUSEADDR = 4L;
        public static readonly long SO_KEEPALIVE = 8L;
        public static readonly long SO_DONTROUTE = 16L;
        public static readonly long SO_BROADCAST = 32L;
        public static readonly long SO_LINGER = 128L;
        public static readonly ulong SO_RCVBUF = 0x1002UL;
        public static readonly ulong SO_SNDBUF = 0x1001UL;
        public static readonly ulong SO_UPDATE_ACCEPT_CONTEXT = 0x700bUL;
        public static readonly ulong SO_UPDATE_CONNECT_CONTEXT = 0x7010UL;

        public static readonly ulong IOC_OUT = 0x40000000UL;
        public static readonly ulong IOC_IN = 0x80000000UL;
        public static readonly ulong IOC_VENDOR = 0x18000000UL;
        public static readonly var IOC_INOUT = IOC_IN | IOC_OUT;
        public static readonly ulong IOC_WS2 = 0x08000000UL;
        public static readonly var SIO_GET_EXTENSION_FUNCTION_POINTER = IOC_INOUT | IOC_WS2 | 6L;
        public static readonly var SIO_KEEPALIVE_VALS = IOC_IN | IOC_VENDOR | 4L;
        public static readonly var SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12L; 

        // cf. http://support.microsoft.com/default.aspx?scid=kb;en-us;257460

        public static readonly ulong IP_TOS = 0x3UL;
        public static readonly ulong IP_TTL = 0x4UL;
        public static readonly ulong IP_MULTICAST_IF = 0x9UL;
        public static readonly ulong IP_MULTICAST_TTL = 0xaUL;
        public static readonly ulong IP_MULTICAST_LOOP = 0xbUL;
        public static readonly ulong IP_ADD_MEMBERSHIP = 0xcUL;
        public static readonly ulong IP_DROP_MEMBERSHIP = 0xdUL;

        public static readonly ulong IPV6_V6ONLY = 0x1bUL;
        public static readonly ulong IPV6_UNICAST_HOPS = 0x4UL;
        public static readonly ulong IPV6_MULTICAST_IF = 0x9UL;
        public static readonly ulong IPV6_MULTICAST_HOPS = 0xaUL;
        public static readonly ulong IPV6_MULTICAST_LOOP = 0xbUL;
        public static readonly ulong IPV6_JOIN_GROUP = 0xcUL;
        public static readonly ulong IPV6_LEAVE_GROUP = 0xdUL;

        public static readonly ulong SOMAXCONN = 0x7fffffffUL;

        public static readonly long TCP_NODELAY = 1L;

        public static readonly long SHUT_RD = 0L;
        public static readonly long SHUT_WR = 1L;
        public static readonly long SHUT_RDWR = 2L;

        public static readonly long WSADESCRIPTION_LEN = 256L;
        public static readonly long WSASYS_STATUS_LEN = 128L;

        public partial struct WSABuf
        {
            public uint Len;
            public ptr<byte> Buf;
        }

        // Invented values to support what package os expects.
        public static readonly ulong S_IFMT = 0x1f000UL;
        public static readonly ulong S_IFIFO = 0x1000UL;
        public static readonly ulong S_IFCHR = 0x2000UL;
        public static readonly ulong S_IFDIR = 0x4000UL;
        public static readonly ulong S_IFBLK = 0x6000UL;
        public static readonly ulong S_IFREG = 0x8000UL;
        public static readonly ulong S_IFLNK = 0xa000UL;
        public static readonly ulong S_IFSOCK = 0xc000UL;
        public static readonly ulong S_ISUID = 0x800UL;
        public static readonly ulong S_ISGID = 0x400UL;
        public static readonly ulong S_ISVTX = 0x200UL;
        public static readonly ulong S_IRUSR = 0x100UL;
        public static readonly ulong S_IWRITE = 0x80UL;
        public static readonly ulong S_IWUSR = 0x80UL;
        public static readonly ulong S_IXUSR = 0x40UL;

        public static readonly ulong FILE_TYPE_CHAR = 0x0002UL;
        public static readonly ulong FILE_TYPE_DISK = 0x0001UL;
        public static readonly ulong FILE_TYPE_PIPE = 0x0003UL;
        public static readonly ulong FILE_TYPE_REMOTE = 0x8000UL;
        public static readonly ulong FILE_TYPE_UNKNOWN = 0x0000UL;

        public partial struct Hostent
        {
            public ptr<byte> Name;
            public ptr<ptr<byte>> Aliases;
            public ushort AddrType;
            public ushort Length;
            public ptr<ptr<byte>> AddrList;
        }

        public partial struct Protoent
        {
            public ptr<byte> Name;
            public ptr<ptr<byte>> Aliases;
            public ushort Proto;
        }

        public static readonly ulong DNS_TYPE_A = 0x0001UL;
        public static readonly ulong DNS_TYPE_NS = 0x0002UL;
        public static readonly ulong DNS_TYPE_MD = 0x0003UL;
        public static readonly ulong DNS_TYPE_MF = 0x0004UL;
        public static readonly ulong DNS_TYPE_CNAME = 0x0005UL;
        public static readonly ulong DNS_TYPE_SOA = 0x0006UL;
        public static readonly ulong DNS_TYPE_MB = 0x0007UL;
        public static readonly ulong DNS_TYPE_MG = 0x0008UL;
        public static readonly ulong DNS_TYPE_MR = 0x0009UL;
        public static readonly ulong DNS_TYPE_NULL = 0x000aUL;
        public static readonly ulong DNS_TYPE_WKS = 0x000bUL;
        public static readonly ulong DNS_TYPE_PTR = 0x000cUL;
        public static readonly ulong DNS_TYPE_HINFO = 0x000dUL;
        public static readonly ulong DNS_TYPE_MINFO = 0x000eUL;
        public static readonly ulong DNS_TYPE_MX = 0x000fUL;
        public static readonly ulong DNS_TYPE_TEXT = 0x0010UL;
        public static readonly ulong DNS_TYPE_RP = 0x0011UL;
        public static readonly ulong DNS_TYPE_AFSDB = 0x0012UL;
        public static readonly ulong DNS_TYPE_X25 = 0x0013UL;
        public static readonly ulong DNS_TYPE_ISDN = 0x0014UL;
        public static readonly ulong DNS_TYPE_RT = 0x0015UL;
        public static readonly ulong DNS_TYPE_NSAP = 0x0016UL;
        public static readonly ulong DNS_TYPE_NSAPPTR = 0x0017UL;
        public static readonly ulong DNS_TYPE_SIG = 0x0018UL;
        public static readonly ulong DNS_TYPE_KEY = 0x0019UL;
        public static readonly ulong DNS_TYPE_PX = 0x001aUL;
        public static readonly ulong DNS_TYPE_GPOS = 0x001bUL;
        public static readonly ulong DNS_TYPE_AAAA = 0x001cUL;
        public static readonly ulong DNS_TYPE_LOC = 0x001dUL;
        public static readonly ulong DNS_TYPE_NXT = 0x001eUL;
        public static readonly ulong DNS_TYPE_EID = 0x001fUL;
        public static readonly ulong DNS_TYPE_NIMLOC = 0x0020UL;
        public static readonly ulong DNS_TYPE_SRV = 0x0021UL;
        public static readonly ulong DNS_TYPE_ATMA = 0x0022UL;
        public static readonly ulong DNS_TYPE_NAPTR = 0x0023UL;
        public static readonly ulong DNS_TYPE_KX = 0x0024UL;
        public static readonly ulong DNS_TYPE_CERT = 0x0025UL;
        public static readonly ulong DNS_TYPE_A6 = 0x0026UL;
        public static readonly ulong DNS_TYPE_DNAME = 0x0027UL;
        public static readonly ulong DNS_TYPE_SINK = 0x0028UL;
        public static readonly ulong DNS_TYPE_OPT = 0x0029UL;
        public static readonly ulong DNS_TYPE_DS = 0x002BUL;
        public static readonly ulong DNS_TYPE_RRSIG = 0x002EUL;
        public static readonly ulong DNS_TYPE_NSEC = 0x002FUL;
        public static readonly ulong DNS_TYPE_DNSKEY = 0x0030UL;
        public static readonly ulong DNS_TYPE_DHCID = 0x0031UL;
        public static readonly ulong DNS_TYPE_UINFO = 0x0064UL;
        public static readonly ulong DNS_TYPE_UID = 0x0065UL;
        public static readonly ulong DNS_TYPE_GID = 0x0066UL;
        public static readonly ulong DNS_TYPE_UNSPEC = 0x0067UL;
        public static readonly ulong DNS_TYPE_ADDRS = 0x00f8UL;
        public static readonly ulong DNS_TYPE_TKEY = 0x00f9UL;
        public static readonly ulong DNS_TYPE_TSIG = 0x00faUL;
        public static readonly ulong DNS_TYPE_IXFR = 0x00fbUL;
        public static readonly ulong DNS_TYPE_AXFR = 0x00fcUL;
        public static readonly ulong DNS_TYPE_MAILB = 0x00fdUL;
        public static readonly ulong DNS_TYPE_MAILA = 0x00feUL;
        public static readonly ulong DNS_TYPE_ALL = 0x00ffUL;
        public static readonly ulong DNS_TYPE_ANY = 0x00ffUL;
        public static readonly ulong DNS_TYPE_WINS = 0xff01UL;
        public static readonly ulong DNS_TYPE_WINSR = 0xff02UL;
        public static readonly ulong DNS_TYPE_NBSTAT = 0xff01UL;

        public static readonly ulong DNS_INFO_NO_RECORDS = 0x251DUL;

 
        // flags inside DNSRecord.Dw
        public static readonly ulong DnsSectionQuestion = 0x0000UL;
        public static readonly ulong DnsSectionAnswer = 0x0001UL;
        public static readonly ulong DnsSectionAuthority = 0x0002UL;
        public static readonly ulong DnsSectionAdditional = 0x0003UL;

        public partial struct DNSSRVData
        {
            public ptr<ushort> Target;
            public ushort Priority;
            public ushort Weight;
            public ushort Port;
            public ushort Pad;
        }

        public partial struct DNSPTRData
        {
            public ptr<ushort> Host;
        }

        public partial struct DNSMXData
        {
            public ptr<ushort> NameExchange;
            public ushort Preference;
            public ushort Pad;
        }

        public partial struct DNSTXTData
        {
            public ushort StringCount;
            public array<ref ushort> StringArray;
        }

        public partial struct DNSRecord
        {
            public ptr<DNSRecord> Next;
            public ptr<ushort> Name;
            public ushort Type;
            public ushort Length;
            public uint Dw;
            public uint Ttl;
            public uint Reserved;
            public array<byte> Data;
        }

        public static readonly long TF_DISCONNECT = 1L;
        public static readonly long TF_REUSE_SOCKET = 2L;
        public static readonly long TF_WRITE_BEHIND = 4L;
        public static readonly long TF_USE_DEFAULT_WORKER = 0L;
        public static readonly long TF_USE_SYSTEM_THREAD = 16L;
        public static readonly long TF_USE_KERNEL_APC = 32L;

        public partial struct TransmitFileBuffers
        {
            public System.UIntPtr Head;
            public uint HeadLength;
            public System.UIntPtr Tail;
            public uint TailLength;
        }

        public static readonly long IFF_UP = 1L;
        public static readonly long IFF_BROADCAST = 2L;
        public static readonly long IFF_LOOPBACK = 4L;
        public static readonly long IFF_POINTTOPOINT = 8L;
        public static readonly long IFF_MULTICAST = 16L;

        public static readonly ulong SIO_GET_INTERFACE_LIST = 0x4004747FUL;

        // TODO(mattn): SockaddrGen is union of sockaddr/sockaddr_in/sockaddr_in6_old.
        // will be fixed to change variable type as suitable.



        // TODO(mattn): SockaddrGen is union of sockaddr/sockaddr_in/sockaddr_in6_old.
        // will be fixed to change variable type as suitable.

        public partial struct SockaddrGen // : array<byte>
        {
        }

        public partial struct InterfaceInfo
        {
            public uint Flags;
            public SockaddrGen Address;
            public SockaddrGen BroadcastAddress;
            public SockaddrGen Netmask;
        }

        public partial struct IpAddressString
        {
            public array<byte> String;
        }

        public partial struct IpMaskString // : IpAddressString
        {
        }

        public partial struct IpAddrString
        {
            public ptr<IpAddrString> Next;
            public IpAddressString IpAddress;
            public IpMaskString IpMask;
            public uint Context;
        }

        public static readonly long MAX_ADAPTER_NAME_LENGTH = 256L;

        public static readonly long MAX_ADAPTER_DESCRIPTION_LENGTH = 128L;

        public static readonly long MAX_ADAPTER_ADDRESS_LENGTH = 8L;



        public partial struct IpAdapterInfo
        {
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

        public static readonly long MAXLEN_PHYSADDR = 8L;

        public static readonly long MAX_INTERFACE_NAME_LEN = 256L;

        public static readonly long MAXLEN_IFDESCR = 256L;



        public partial struct MibIfRow
        {
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

        public partial struct CertContext
        {
            public uint EncodingType;
            public ptr<byte> EncodedCert;
            public uint Length;
            public System.UIntPtr CertInfo;
            public Handle Store;
        }

        public partial struct CertChainContext
        {
            public uint Size;
            public CertTrustStatus TrustStatus;
            public uint ChainCount;
            public ptr<ptr<CertSimpleChain>> Chains;
            public uint LowerQualityChainCount;
            public ptr<ptr<CertChainContext>> LowerQualityChains;
            public uint HasRevocationFreshnessTime;
            public uint RevocationFreshnessTime;
        }

        public partial struct CertSimpleChain
        {
            public uint Size;
            public CertTrustStatus TrustStatus;
            public uint NumElements;
            public ptr<ptr<CertChainElement>> Elements;
            public System.UIntPtr TrustListInfo;
            public uint HasRevocationFreshnessTime;
            public uint RevocationFreshnessTime;
        }

        public partial struct CertChainElement
        {
            public uint Size;
            public ptr<CertContext> CertContext;
            public CertTrustStatus TrustStatus;
            public ptr<CertRevocationInfo> RevocationInfo;
            public ptr<CertEnhKeyUsage> IssuanceUsage;
            public ptr<CertEnhKeyUsage> ApplicationUsage;
            public ptr<ushort> ExtendedErrorInfo;
        }

        public partial struct CertRevocationInfo
        {
            public uint Size;
            public uint RevocationResult;
            public ptr<byte> RevocationOid;
            public System.UIntPtr OidSpecificInfo;
            public uint HasFreshnessTime;
            public uint FreshnessTime;
            public System.UIntPtr CrlInfo; // *CertRevocationCrlInfo
        }

        public partial struct CertTrustStatus
        {
            public uint ErrorStatus;
            public uint InfoStatus;
        }

        public partial struct CertUsageMatch
        {
            public uint Type;
            public CertEnhKeyUsage Usage;
        }

        public partial struct CertEnhKeyUsage
        {
            public uint Length;
            public ptr<ptr<byte>> UsageIdentifiers;
        }

        public partial struct CertChainPara
        {
            public uint Size;
            public CertUsageMatch RequestedUsage;
            public CertUsageMatch RequstedIssuancePolicy;
            public uint URLRetrievalTimeout;
            public uint CheckRevocationFreshnessTime;
            public uint RevocationFreshnessTime;
            public ptr<Filetime> CacheResync;
        }

        public partial struct CertChainPolicyPara
        {
            public uint Size;
            public uint Flags;
            public System.UIntPtr ExtraPolicyPara;
        }

        public partial struct SSLExtraCertChainPolicyPara
        {
            public uint Size;
            public uint AuthType;
            public uint Checks;
            public ptr<ushort> ServerName;
        }

        public partial struct CertChainPolicyStatus
        {
            public uint Size;
            public uint Error;
            public uint ChainIndex;
            public uint ElementIndex;
            public System.UIntPtr ExtraPolicyStatus;
        }

 
        // do not reorder
        public static readonly ulong HKEY_CLASSES_ROOT = 0x80000000UL + iota;
        public static readonly var HKEY_CURRENT_USER = 0;
        public static readonly var HKEY_LOCAL_MACHINE = 1;
        public static readonly var HKEY_USERS = 2;
        public static readonly var HKEY_PERFORMANCE_DATA = 3;
        public static readonly var HKEY_CURRENT_CONFIG = 4;
        public static readonly KEY_QUERY_VALUE HKEY_DYN_DATA = 1L;
        public static readonly long KEY_SET_VALUE = 2L;
        public static readonly long KEY_CREATE_SUB_KEY = 4L;
        public static readonly long KEY_ENUMERATE_SUB_KEYS = 8L;
        public static readonly long KEY_NOTIFY = 16L;
        public static readonly long KEY_CREATE_LINK = 32L;
        public static readonly ulong KEY_WRITE = 0x20006UL;
        public static readonly ulong KEY_EXECUTE = 0x20019UL;
        public static readonly ulong KEY_READ = 0x20019UL;
        public static readonly ulong KEY_WOW64_64KEY = 0x0100UL;
        public static readonly ulong KEY_WOW64_32KEY = 0x0200UL;
        public static readonly ulong KEY_ALL_ACCESS = 0xf003fUL;

 
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

        public partial struct AddrinfoW
        {
            public int Flags;
            public int Family;
            public int Socktype;
            public int Protocol;
            public System.UIntPtr Addrlen;
            public ptr<ushort> Canonname;
            public System.UIntPtr Addr;
            public ptr<AddrinfoW> Next;
        }

        public static readonly long AI_PASSIVE = 1L;
        public static readonly long AI_CANONNAME = 2L;
        public static readonly long AI_NUMERICHOST = 4L;

        public partial struct GUID
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            public array<byte> Data4;
        }

        public static GUID WSAID_CONNECTEX = new GUID(0x25a207b9,0xddf3,0x4660,[8]byte{0x8e,0xe9,0x76,0xe5,0x8c,0x74,0x06,0x3e},);

        public static readonly long FILE_SKIP_COMPLETION_PORT_ON_SUCCESS = 1L;
        public static readonly long FILE_SKIP_SET_EVENT_ON_HANDLE = 2L;

        public static readonly long WSAPROTOCOL_LEN = 255L;
        public static readonly long MAX_PROTOCOL_CHAIN = 7L;
        public static readonly long BASE_PROTOCOL = 1L;
        public static readonly long LAYERED_PROTOCOL = 0L;

        public static readonly ulong XP1_CONNECTIONLESS = 0x00000001UL;
        public static readonly ulong XP1_GUARANTEED_DELIVERY = 0x00000002UL;
        public static readonly ulong XP1_GUARANTEED_ORDER = 0x00000004UL;
        public static readonly ulong XP1_MESSAGE_ORIENTED = 0x00000008UL;
        public static readonly ulong XP1_PSEUDO_STREAM = 0x00000010UL;
        public static readonly ulong XP1_GRACEFUL_CLOSE = 0x00000020UL;
        public static readonly ulong XP1_EXPEDITED_DATA = 0x00000040UL;
        public static readonly ulong XP1_CONNECT_DATA = 0x00000080UL;
        public static readonly ulong XP1_DISCONNECT_DATA = 0x00000100UL;
        public static readonly ulong XP1_SUPPORT_BROADCAST = 0x00000200UL;
        public static readonly ulong XP1_SUPPORT_MULTIPOINT = 0x00000400UL;
        public static readonly ulong XP1_MULTIPOINT_CONTROL_PLANE = 0x00000800UL;
        public static readonly ulong XP1_MULTIPOINT_DATA_PLANE = 0x00001000UL;
        public static readonly ulong XP1_QOS_SUPPORTED = 0x00002000UL;
        public static readonly ulong XP1_UNI_SEND = 0x00008000UL;
        public static readonly ulong XP1_UNI_RECV = 0x00010000UL;
        public static readonly ulong XP1_IFS_HANDLES = 0x00020000UL;
        public static readonly ulong XP1_PARTIAL_MESSAGE = 0x00040000UL;
        public static readonly ulong XP1_SAN_SUPPORT_SDP = 0x00080000UL;

        public static readonly ulong PFL_MULTIPLE_PROTO_ENTRIES = 0x00000001UL;
        public static readonly ulong PFL_RECOMMENDED_PROTO_ENTRY = 0x00000002UL;
        public static readonly ulong PFL_HIDDEN = 0x00000004UL;
        public static readonly ulong PFL_MATCHES_PROTOCOL_ZERO = 0x00000008UL;
        public static readonly ulong PFL_NETWORKDIRECT_PROVIDER = 0x00000010UL;

        public partial struct WSAProtocolInfo
        {
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

        public partial struct WSAProtocolChain
        {
            public int ChainLen;
            public array<uint> ChainEntries;
        }

        public partial struct TCPKeepalive
        {
            public uint OnOff;
            public uint Time;
            public uint Interval;
        }

        private partial struct symbolicLinkReparseBuffer
        {
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            public uint Flags;
            public array<ushort> PathBuffer;
        }

        private partial struct mountPointReparseBuffer
        {
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            public array<ushort> PathBuffer;
        }

        private partial struct reparseDataBuffer
        {
            public uint ReparseTag;
            public ushort ReparseDataLength;
            public ushort Reserved; // GenericReparseBuffer
            public byte reparseBuffer;
        }

        public static readonly ulong FSCTL_GET_REPARSE_POINT = 0x900A8UL;
        public static readonly long MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16L * 1024L;
        private static readonly ulong _IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003UL;
        public static readonly ulong IO_REPARSE_TAG_SYMLINK = 0xA000000CUL;
        public static readonly ulong SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1UL;
        private static readonly long _SYMLINK_FLAG_RELATIVE = 1L;
    }
}
