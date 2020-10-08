// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 08 03:27:53 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\types_windows.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
 
        // Windows errors.
        public static readonly Errno ERROR_FILE_NOT_FOUND = (Errno)2L;
        public static readonly Errno ERROR_PATH_NOT_FOUND = (Errno)3L;
        public static readonly Errno ERROR_ACCESS_DENIED = (Errno)5L;
        public static readonly Errno ERROR_NO_MORE_FILES = (Errno)18L;
        public static readonly Errno ERROR_HANDLE_EOF = (Errno)38L;
        public static readonly Errno ERROR_NETNAME_DELETED = (Errno)64L;
        public static readonly Errno ERROR_FILE_EXISTS = (Errno)80L;
        public static readonly Errno ERROR_BROKEN_PIPE = (Errno)109L;
        public static readonly Errno ERROR_BUFFER_OVERFLOW = (Errno)111L;
        public static readonly Errno ERROR_INSUFFICIENT_BUFFER = (Errno)122L;
        public static readonly Errno ERROR_MOD_NOT_FOUND = (Errno)126L;
        public static readonly Errno ERROR_PROC_NOT_FOUND = (Errno)127L;
        public static readonly Errno ERROR_DIR_NOT_EMPTY = (Errno)145L;
        public static readonly Errno ERROR_ALREADY_EXISTS = (Errno)183L;
        public static readonly Errno ERROR_ENVVAR_NOT_FOUND = (Errno)203L;
        public static readonly Errno ERROR_MORE_DATA = (Errno)234L;
        public static readonly Errno ERROR_OPERATION_ABORTED = (Errno)995L;
        public static readonly Errno ERROR_IO_PENDING = (Errno)997L;
        public static readonly Errno ERROR_NOT_FOUND = (Errno)1168L;
        public static readonly Errno ERROR_PRIVILEGE_NOT_HELD = (Errno)1314L;
        public static readonly Errno WSAEACCES = (Errno)10013L;
        public static readonly Errno WSAECONNABORTED = (Errno)10053L;
        public static readonly Errno WSAECONNRESET = (Errno)10054L;


 
        // Invented values to support what package os expects.
        public static readonly ulong O_RDONLY = (ulong)0x00000UL;
        public static readonly ulong O_WRONLY = (ulong)0x00001UL;
        public static readonly ulong O_RDWR = (ulong)0x00002UL;
        public static readonly ulong O_CREAT = (ulong)0x00040UL;
        public static readonly ulong O_EXCL = (ulong)0x00080UL;
        public static readonly ulong O_NOCTTY = (ulong)0x00100UL;
        public static readonly ulong O_TRUNC = (ulong)0x00200UL;
        public static readonly ulong O_NONBLOCK = (ulong)0x00800UL;
        public static readonly ulong O_APPEND = (ulong)0x00400UL;
        public static readonly ulong O_SYNC = (ulong)0x01000UL;
        public static readonly ulong O_ASYNC = (ulong)0x02000UL;
        public static readonly ulong O_CLOEXEC = (ulong)0x80000UL;


 
        // More invented values for signals
        public static readonly var SIGHUP = (var)Signal(0x1UL);
        public static readonly var SIGINT = (var)Signal(0x2UL);
        public static readonly var SIGQUIT = (var)Signal(0x3UL);
        public static readonly var SIGILL = (var)Signal(0x4UL);
        public static readonly var SIGTRAP = (var)Signal(0x5UL);
        public static readonly var SIGABRT = (var)Signal(0x6UL);
        public static readonly var SIGBUS = (var)Signal(0x7UL);
        public static readonly var SIGFPE = (var)Signal(0x8UL);
        public static readonly var SIGKILL = (var)Signal(0x9UL);
        public static readonly var SIGSEGV = (var)Signal(0xbUL);
        public static readonly var SIGPIPE = (var)Signal(0xdUL);
        public static readonly var SIGALRM = (var)Signal(0xeUL);
        public static readonly var SIGTERM = (var)Signal(0xfUL);


        private static array<@string> signals = new array<@string>(InitKeyedValues<@string>((1, "hangup"), (2, "interrupt"), (3, "quit"), (4, "illegal instruction"), (5, "trace/breakpoint trap"), (6, "aborted"), (7, "bus error"), (8, "floating point exception"), (9, "killed"), (10, "user defined signal 1"), (11, "segmentation fault"), (12, "user defined signal 2"), (13, "broken pipe"), (14, "alarm clock"), (15, "terminated")));

        public static readonly ulong GENERIC_READ = (ulong)0x80000000UL;
        public static readonly ulong GENERIC_WRITE = (ulong)0x40000000UL;
        public static readonly ulong GENERIC_EXECUTE = (ulong)0x20000000UL;
        public static readonly ulong GENERIC_ALL = (ulong)0x10000000UL;

        public static readonly ulong FILE_LIST_DIRECTORY = (ulong)0x00000001UL;
        public static readonly ulong FILE_APPEND_DATA = (ulong)0x00000004UL;
        public static readonly ulong FILE_WRITE_ATTRIBUTES = (ulong)0x00000100UL;

        public static readonly ulong FILE_SHARE_READ = (ulong)0x00000001UL;
        public static readonly ulong FILE_SHARE_WRITE = (ulong)0x00000002UL;
        public static readonly ulong FILE_SHARE_DELETE = (ulong)0x00000004UL;
        public static readonly ulong FILE_ATTRIBUTE_READONLY = (ulong)0x00000001UL;
        public static readonly ulong FILE_ATTRIBUTE_HIDDEN = (ulong)0x00000002UL;
        public static readonly ulong FILE_ATTRIBUTE_SYSTEM = (ulong)0x00000004UL;
        public static readonly ulong FILE_ATTRIBUTE_DIRECTORY = (ulong)0x00000010UL;
        public static readonly ulong FILE_ATTRIBUTE_ARCHIVE = (ulong)0x00000020UL;
        public static readonly ulong FILE_ATTRIBUTE_NORMAL = (ulong)0x00000080UL;
        public static readonly ulong FILE_ATTRIBUTE_REPARSE_POINT = (ulong)0x00000400UL;

        public static readonly ulong INVALID_FILE_ATTRIBUTES = (ulong)0xffffffffUL;

        public static readonly long CREATE_NEW = (long)1L;
        public static readonly long CREATE_ALWAYS = (long)2L;
        public static readonly long OPEN_EXISTING = (long)3L;
        public static readonly long OPEN_ALWAYS = (long)4L;
        public static readonly long TRUNCATE_EXISTING = (long)5L;

        public static readonly ulong FILE_FLAG_OPEN_REPARSE_POINT = (ulong)0x00200000UL;
        public static readonly ulong FILE_FLAG_BACKUP_SEMANTICS = (ulong)0x02000000UL;
        public static readonly ulong FILE_FLAG_OVERLAPPED = (ulong)0x40000000UL;

        public static readonly ulong HANDLE_FLAG_INHERIT = (ulong)0x00000001UL;
        public static readonly ulong STARTF_USESTDHANDLES = (ulong)0x00000100UL;
        public static readonly ulong STARTF_USESHOWWINDOW = (ulong)0x00000001UL;
        public static readonly ulong DUPLICATE_CLOSE_SOURCE = (ulong)0x00000001UL;
        public static readonly ulong DUPLICATE_SAME_ACCESS = (ulong)0x00000002UL;

        public static readonly long STD_INPUT_HANDLE = (long)-10L;
        public static readonly long STD_OUTPUT_HANDLE = (long)-11L;
        public static readonly long STD_ERROR_HANDLE = (long)-12L;

        public static readonly long FILE_BEGIN = (long)0L;
        public static readonly long FILE_CURRENT = (long)1L;
        public static readonly long FILE_END = (long)2L;

        public static readonly ulong LANG_ENGLISH = (ulong)0x09UL;
        public static readonly ulong SUBLANG_ENGLISH_US = (ulong)0x01UL;

        public static readonly long FORMAT_MESSAGE_ALLOCATE_BUFFER = (long)256L;
        public static readonly long FORMAT_MESSAGE_IGNORE_INSERTS = (long)512L;
        public static readonly long FORMAT_MESSAGE_FROM_STRING = (long)1024L;
        public static readonly long FORMAT_MESSAGE_FROM_HMODULE = (long)2048L;
        public static readonly long FORMAT_MESSAGE_FROM_SYSTEM = (long)4096L;
        public static readonly long FORMAT_MESSAGE_ARGUMENT_ARRAY = (long)8192L;
        public static readonly long FORMAT_MESSAGE_MAX_WIDTH_MASK = (long)255L;

        public static readonly long MAX_PATH = (long)260L;
        public static readonly long MAX_LONG_PATH = (long)32768L;

        public static readonly long MAX_COMPUTERNAME_LENGTH = (long)15L;

        public static readonly long TIME_ZONE_ID_UNKNOWN = (long)0L;
        public static readonly long TIME_ZONE_ID_STANDARD = (long)1L;

        public static readonly long TIME_ZONE_ID_DAYLIGHT = (long)2L;
        public static readonly long IGNORE = (long)0L;
        public static readonly ulong INFINITE = (ulong)0xffffffffUL;

        public static readonly long WAIT_TIMEOUT = (long)258L;
        public static readonly ulong WAIT_ABANDONED = (ulong)0x00000080UL;
        public static readonly ulong WAIT_OBJECT_0 = (ulong)0x00000000UL;
        public static readonly ulong WAIT_FAILED = (ulong)0xFFFFFFFFUL;

        public static readonly ulong CREATE_NEW_PROCESS_GROUP = (ulong)0x00000200UL;
        public static readonly ulong CREATE_UNICODE_ENVIRONMENT = (ulong)0x00000400UL;

        public static readonly long PROCESS_TERMINATE = (long)1L;
        public static readonly ulong PROCESS_QUERY_INFORMATION = (ulong)0x00000400UL;
        public static readonly ulong SYNCHRONIZE = (ulong)0x00100000UL;

        public static readonly ulong PAGE_READONLY = (ulong)0x02UL;
        public static readonly ulong PAGE_READWRITE = (ulong)0x04UL;
        public static readonly ulong PAGE_WRITECOPY = (ulong)0x08UL;
        public static readonly ulong PAGE_EXECUTE_READ = (ulong)0x20UL;
        public static readonly ulong PAGE_EXECUTE_READWRITE = (ulong)0x40UL;
        public static readonly ulong PAGE_EXECUTE_WRITECOPY = (ulong)0x80UL;

        public static readonly ulong FILE_MAP_COPY = (ulong)0x01UL;
        public static readonly ulong FILE_MAP_WRITE = (ulong)0x02UL;
        public static readonly ulong FILE_MAP_READ = (ulong)0x04UL;
        public static readonly ulong FILE_MAP_EXECUTE = (ulong)0x20UL;

        public static readonly long CTRL_C_EVENT = (long)0L;
        public static readonly long CTRL_BREAK_EVENT = (long)1L;
        public static readonly long CTRL_CLOSE_EVENT = (long)2L;
        public static readonly long CTRL_LOGOFF_EVENT = (long)5L;
        public static readonly long CTRL_SHUTDOWN_EVENT = (long)6L;


 
        // flags for CreateToolhelp32Snapshot
        public static readonly ulong TH32CS_SNAPHEAPLIST = (ulong)0x01UL;
        public static readonly ulong TH32CS_SNAPPROCESS = (ulong)0x02UL;
        public static readonly ulong TH32CS_SNAPTHREAD = (ulong)0x04UL;
        public static readonly ulong TH32CS_SNAPMODULE = (ulong)0x08UL;
        public static readonly ulong TH32CS_SNAPMODULE32 = (ulong)0x10UL;
        public static readonly var TH32CS_SNAPALL = (var)TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD;
        public static readonly ulong TH32CS_INHERIT = (ulong)0x80000000UL;


 
        // do not reorder
        public static readonly long FILE_NOTIFY_CHANGE_FILE_NAME = (long)1L << (int)(iota);
        public static readonly var FILE_NOTIFY_CHANGE_DIR_NAME = (var)0;
        public static readonly var FILE_NOTIFY_CHANGE_ATTRIBUTES = (var)1;
        public static readonly var FILE_NOTIFY_CHANGE_SIZE = (var)2;
        public static readonly var FILE_NOTIFY_CHANGE_LAST_WRITE = (var)3;
        public static readonly var FILE_NOTIFY_CHANGE_LAST_ACCESS = (var)4;
        public static readonly var FILE_NOTIFY_CHANGE_CREATION = (var)5;


 
        // do not reorder
        public static readonly var FILE_ACTION_ADDED = (var)iota + 1L;
        public static readonly var FILE_ACTION_REMOVED = (var)0;
        public static readonly var FILE_ACTION_MODIFIED = (var)1;
        public static readonly var FILE_ACTION_RENAMED_OLD_NAME = (var)2;
        public static readonly var FILE_ACTION_RENAMED_NEW_NAME = (var)3;


 
        // wincrypt.h
        public static readonly long PROV_RSA_FULL = (long)1L;
        public static readonly long PROV_RSA_SIG = (long)2L;
        public static readonly long PROV_DSS = (long)3L;
        public static readonly long PROV_FORTEZZA = (long)4L;
        public static readonly long PROV_MS_EXCHANGE = (long)5L;
        public static readonly long PROV_SSL = (long)6L;
        public static readonly long PROV_RSA_SCHANNEL = (long)12L;
        public static readonly long PROV_DSS_DH = (long)13L;
        public static readonly long PROV_EC_ECDSA_SIG = (long)14L;
        public static readonly long PROV_EC_ECNRA_SIG = (long)15L;
        public static readonly long PROV_EC_ECDSA_FULL = (long)16L;
        public static readonly long PROV_EC_ECNRA_FULL = (long)17L;
        public static readonly long PROV_DH_SCHANNEL = (long)18L;
        public static readonly long PROV_SPYRUS_LYNKS = (long)20L;
        public static readonly long PROV_RNG = (long)21L;
        public static readonly long PROV_INTEL_SEC = (long)22L;
        public static readonly long PROV_REPLACE_OWF = (long)23L;
        public static readonly long PROV_RSA_AES = (long)24L;
        public static readonly ulong CRYPT_VERIFYCONTEXT = (ulong)0xF0000000UL;
        public static readonly ulong CRYPT_NEWKEYSET = (ulong)0x00000008UL;
        public static readonly ulong CRYPT_DELETEKEYSET = (ulong)0x00000010UL;
        public static readonly ulong CRYPT_MACHINE_KEYSET = (ulong)0x00000020UL;
        public static readonly ulong CRYPT_SILENT = (ulong)0x00000040UL;
        public static readonly ulong CRYPT_DEFAULT_CONTAINER_OPTIONAL = (ulong)0x00000080UL;

        public static readonly long USAGE_MATCH_TYPE_AND = (long)0L;
        public static readonly long USAGE_MATCH_TYPE_OR = (long)1L;

        public static readonly ulong X509_ASN_ENCODING = (ulong)0x00000001UL;
        public static readonly ulong PKCS_7_ASN_ENCODING = (ulong)0x00010000UL;

        public static readonly long CERT_STORE_PROV_MEMORY = (long)2L;

        public static readonly long CERT_STORE_ADD_ALWAYS = (long)4L;

        public static readonly ulong CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = (ulong)0x00000004UL;

        public static readonly ulong CERT_TRUST_NO_ERROR = (ulong)0x00000000UL;
        public static readonly ulong CERT_TRUST_IS_NOT_TIME_VALID = (ulong)0x00000001UL;
        public static readonly ulong CERT_TRUST_IS_REVOKED = (ulong)0x00000004UL;
        public static readonly ulong CERT_TRUST_IS_NOT_SIGNATURE_VALID = (ulong)0x00000008UL;
        public static readonly ulong CERT_TRUST_IS_NOT_VALID_FOR_USAGE = (ulong)0x00000010UL;
        public static readonly ulong CERT_TRUST_IS_UNTRUSTED_ROOT = (ulong)0x00000020UL;
        public static readonly ulong CERT_TRUST_REVOCATION_STATUS_UNKNOWN = (ulong)0x00000040UL;
        public static readonly ulong CERT_TRUST_IS_CYCLIC = (ulong)0x00000080UL;
        public static readonly ulong CERT_TRUST_INVALID_EXTENSION = (ulong)0x00000100UL;
        public static readonly ulong CERT_TRUST_INVALID_POLICY_CONSTRAINTS = (ulong)0x00000200UL;
        public static readonly ulong CERT_TRUST_INVALID_BASIC_CONSTRAINTS = (ulong)0x00000400UL;
        public static readonly ulong CERT_TRUST_INVALID_NAME_CONSTRAINTS = (ulong)0x00000800UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT = (ulong)0x00001000UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT = (ulong)0x00002000UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT = (ulong)0x00004000UL;
        public static readonly ulong CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT = (ulong)0x00008000UL;
        public static readonly ulong CERT_TRUST_IS_OFFLINE_REVOCATION = (ulong)0x01000000UL;
        public static readonly ulong CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY = (ulong)0x02000000UL;
        public static readonly ulong CERT_TRUST_IS_EXPLICIT_DISTRUST = (ulong)0x04000000UL;
        public static readonly ulong CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT = (ulong)0x08000000UL;

        public static readonly long CERT_CHAIN_POLICY_BASE = (long)1L;
        public static readonly long CERT_CHAIN_POLICY_AUTHENTICODE = (long)2L;
        public static readonly long CERT_CHAIN_POLICY_AUTHENTICODE_TS = (long)3L;
        public static readonly long CERT_CHAIN_POLICY_SSL = (long)4L;
        public static readonly long CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = (long)5L;
        public static readonly long CERT_CHAIN_POLICY_NT_AUTH = (long)6L;
        public static readonly long CERT_CHAIN_POLICY_MICROSOFT_ROOT = (long)7L;
        public static readonly long CERT_CHAIN_POLICY_EV = (long)8L;

        public static readonly ulong CERT_E_EXPIRED = (ulong)0x800B0101UL;
        public static readonly ulong CERT_E_ROLE = (ulong)0x800B0103UL;
        public static readonly ulong CERT_E_PURPOSE = (ulong)0x800B0106UL;
        public static readonly ulong CERT_E_UNTRUSTEDROOT = (ulong)0x800B0109UL;
        public static readonly ulong CERT_E_CN_NO_MATCH = (ulong)0x800B010FUL;

        public static readonly long AUTHTYPE_CLIENT = (long)1L;
        public static readonly long AUTHTYPE_SERVER = (long)2L;


        public static slice<byte> OID_PKIX_KP_SERVER_AUTH = (slice<byte>)"1.3.6.1.5.5.7.3.1\x00";        public static slice<byte> OID_SERVER_GATED_CRYPTO = (slice<byte>)"1.3.6.1.4.1.311.10.3.3\x00";        public static slice<byte> OID_SGC_NETSCAPE = (slice<byte>)"2.16.840.1.113730.4.1\x00";

        // Pointer represents a pointer to an arbitrary Windows type.
        //
        // Pointer-typed fields may point to one of many different types. It's
        // up to the caller to provide a pointer to the appropriate type, cast
        // to Pointer. The caller must obey the unsafe.Pointer rules while
        // doing so.
        public partial struct Timeval
        {
            public int Sec;
            public int Usec;
        }

        private static long Nanoseconds(this ptr<Timeval> _addr_tv)
        {
            ref Timeval tv = ref _addr_tv.val;

            return (int64(tv.Sec) * 1e6F + int64(tv.Usec)) * 1e3F;
        }

        public static Timeval NsecToTimeval(long nsec)
        {
            Timeval tv = default;

            tv.Sec = int32(nsec / 1e9F);
            tv.Usec = int32(nsec % 1e9F / 1e3F);
            return ;
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
        private static long Nanoseconds(this ptr<Filetime> _addr_ft)
        {
            ref Filetime ft = ref _addr_ft.val;
 
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
            Filetime ft = default;
 
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

        private static void copyFindData(ptr<Win32finddata> _addr_dst, ptr<win32finddata1> _addr_src)
        {
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

        public static readonly long GetFileExInfoStandard = (long)0L;
        public static readonly long GetFileExMaxInfoLevel = (long)1L;


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
        public static readonly long SW_HIDE = (long)0L;
        public static readonly long SW_NORMAL = (long)1L;
        public static readonly long SW_SHOWNORMAL = (long)1L;
        public static readonly long SW_SHOWMINIMIZED = (long)2L;
        public static readonly long SW_SHOWMAXIMIZED = (long)3L;
        public static readonly long SW_MAXIMIZE = (long)3L;
        public static readonly long SW_SHOWNOACTIVATE = (long)4L;
        public static readonly long SW_SHOW = (long)5L;
        public static readonly long SW_MINIMIZE = (long)6L;
        public static readonly long SW_SHOWMINNOACTIVE = (long)7L;
        public static readonly long SW_SHOWNA = (long)8L;
        public static readonly long SW_RESTORE = (long)9L;
        public static readonly long SW_SHOWDEFAULT = (long)10L;
        public static readonly long SW_FORCEMINIMIZE = (long)11L;


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

        public static readonly long AF_UNSPEC = (long)0L;
        public static readonly long AF_UNIX = (long)1L;
        public static readonly long AF_INET = (long)2L;
        public static readonly long AF_INET6 = (long)23L;
        public static readonly long AF_NETBIOS = (long)17L;

        public static readonly long SOCK_STREAM = (long)1L;
        public static readonly long SOCK_DGRAM = (long)2L;
        public static readonly long SOCK_RAW = (long)3L;
        public static readonly long SOCK_SEQPACKET = (long)5L;

        public static readonly long IPPROTO_IP = (long)0L;
        public static readonly ulong IPPROTO_IPV6 = (ulong)0x29UL;
        public static readonly long IPPROTO_TCP = (long)6L;
        public static readonly long IPPROTO_UDP = (long)17L;

        public static readonly ulong SOL_SOCKET = (ulong)0xffffUL;
        public static readonly long SO_REUSEADDR = (long)4L;
        public static readonly long SO_KEEPALIVE = (long)8L;
        public static readonly long SO_DONTROUTE = (long)16L;
        public static readonly long SO_BROADCAST = (long)32L;
        public static readonly long SO_LINGER = (long)128L;
        public static readonly ulong SO_RCVBUF = (ulong)0x1002UL;
        public static readonly ulong SO_SNDBUF = (ulong)0x1001UL;
        public static readonly ulong SO_UPDATE_ACCEPT_CONTEXT = (ulong)0x700bUL;
        public static readonly ulong SO_UPDATE_CONNECT_CONTEXT = (ulong)0x7010UL;

        public static readonly ulong IOC_OUT = (ulong)0x40000000UL;
        public static readonly ulong IOC_IN = (ulong)0x80000000UL;
        public static readonly ulong IOC_VENDOR = (ulong)0x18000000UL;
        public static readonly var IOC_INOUT = (var)IOC_IN | IOC_OUT;
        public static readonly ulong IOC_WS2 = (ulong)0x08000000UL;
        public static readonly var SIO_GET_EXTENSION_FUNCTION_POINTER = (var)IOC_INOUT | IOC_WS2 | 6L;
        public static readonly var SIO_KEEPALIVE_VALS = (var)IOC_IN | IOC_VENDOR | 4L;
        public static readonly var SIO_UDP_CONNRESET = (var)IOC_IN | IOC_VENDOR | 12L; 

        // cf. https://support.microsoft.com/default.aspx?scid=kb;en-us;257460

        public static readonly ulong IP_TOS = (ulong)0x3UL;
        public static readonly ulong IP_TTL = (ulong)0x4UL;
        public static readonly ulong IP_MULTICAST_IF = (ulong)0x9UL;
        public static readonly ulong IP_MULTICAST_TTL = (ulong)0xaUL;
        public static readonly ulong IP_MULTICAST_LOOP = (ulong)0xbUL;
        public static readonly ulong IP_ADD_MEMBERSHIP = (ulong)0xcUL;
        public static readonly ulong IP_DROP_MEMBERSHIP = (ulong)0xdUL;

        public static readonly ulong IPV6_V6ONLY = (ulong)0x1bUL;
        public static readonly ulong IPV6_UNICAST_HOPS = (ulong)0x4UL;
        public static readonly ulong IPV6_MULTICAST_IF = (ulong)0x9UL;
        public static readonly ulong IPV6_MULTICAST_HOPS = (ulong)0xaUL;
        public static readonly ulong IPV6_MULTICAST_LOOP = (ulong)0xbUL;
        public static readonly ulong IPV6_JOIN_GROUP = (ulong)0xcUL;
        public static readonly ulong IPV6_LEAVE_GROUP = (ulong)0xdUL;

        public static readonly ulong SOMAXCONN = (ulong)0x7fffffffUL;

        public static readonly long TCP_NODELAY = (long)1L;

        public static readonly long SHUT_RD = (long)0L;
        public static readonly long SHUT_WR = (long)1L;
        public static readonly long SHUT_RDWR = (long)2L;

        public static readonly long WSADESCRIPTION_LEN = (long)256L;
        public static readonly long WSASYS_STATUS_LEN = (long)128L;


        public partial struct WSABuf
        {
            public uint Len;
            public ptr<byte> Buf;
        }

        // Invented values to support what package os expects.
        public static readonly ulong S_IFMT = (ulong)0x1f000UL;
        public static readonly ulong S_IFIFO = (ulong)0x1000UL;
        public static readonly ulong S_IFCHR = (ulong)0x2000UL;
        public static readonly ulong S_IFDIR = (ulong)0x4000UL;
        public static readonly ulong S_IFBLK = (ulong)0x6000UL;
        public static readonly ulong S_IFREG = (ulong)0x8000UL;
        public static readonly ulong S_IFLNK = (ulong)0xa000UL;
        public static readonly ulong S_IFSOCK = (ulong)0xc000UL;
        public static readonly ulong S_ISUID = (ulong)0x800UL;
        public static readonly ulong S_ISGID = (ulong)0x400UL;
        public static readonly ulong S_ISVTX = (ulong)0x200UL;
        public static readonly ulong S_IRUSR = (ulong)0x100UL;
        public static readonly ulong S_IWRITE = (ulong)0x80UL;
        public static readonly ulong S_IWUSR = (ulong)0x80UL;
        public static readonly ulong S_IXUSR = (ulong)0x40UL;


        public static readonly ulong FILE_TYPE_CHAR = (ulong)0x0002UL;
        public static readonly ulong FILE_TYPE_DISK = (ulong)0x0001UL;
        public static readonly ulong FILE_TYPE_PIPE = (ulong)0x0003UL;
        public static readonly ulong FILE_TYPE_REMOTE = (ulong)0x8000UL;
        public static readonly ulong FILE_TYPE_UNKNOWN = (ulong)0x0000UL;


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

        public static readonly ulong DNS_TYPE_A = (ulong)0x0001UL;
        public static readonly ulong DNS_TYPE_NS = (ulong)0x0002UL;
        public static readonly ulong DNS_TYPE_MD = (ulong)0x0003UL;
        public static readonly ulong DNS_TYPE_MF = (ulong)0x0004UL;
        public static readonly ulong DNS_TYPE_CNAME = (ulong)0x0005UL;
        public static readonly ulong DNS_TYPE_SOA = (ulong)0x0006UL;
        public static readonly ulong DNS_TYPE_MB = (ulong)0x0007UL;
        public static readonly ulong DNS_TYPE_MG = (ulong)0x0008UL;
        public static readonly ulong DNS_TYPE_MR = (ulong)0x0009UL;
        public static readonly ulong DNS_TYPE_NULL = (ulong)0x000aUL;
        public static readonly ulong DNS_TYPE_WKS = (ulong)0x000bUL;
        public static readonly ulong DNS_TYPE_PTR = (ulong)0x000cUL;
        public static readonly ulong DNS_TYPE_HINFO = (ulong)0x000dUL;
        public static readonly ulong DNS_TYPE_MINFO = (ulong)0x000eUL;
        public static readonly ulong DNS_TYPE_MX = (ulong)0x000fUL;
        public static readonly ulong DNS_TYPE_TEXT = (ulong)0x0010UL;
        public static readonly ulong DNS_TYPE_RP = (ulong)0x0011UL;
        public static readonly ulong DNS_TYPE_AFSDB = (ulong)0x0012UL;
        public static readonly ulong DNS_TYPE_X25 = (ulong)0x0013UL;
        public static readonly ulong DNS_TYPE_ISDN = (ulong)0x0014UL;
        public static readonly ulong DNS_TYPE_RT = (ulong)0x0015UL;
        public static readonly ulong DNS_TYPE_NSAP = (ulong)0x0016UL;
        public static readonly ulong DNS_TYPE_NSAPPTR = (ulong)0x0017UL;
        public static readonly ulong DNS_TYPE_SIG = (ulong)0x0018UL;
        public static readonly ulong DNS_TYPE_KEY = (ulong)0x0019UL;
        public static readonly ulong DNS_TYPE_PX = (ulong)0x001aUL;
        public static readonly ulong DNS_TYPE_GPOS = (ulong)0x001bUL;
        public static readonly ulong DNS_TYPE_AAAA = (ulong)0x001cUL;
        public static readonly ulong DNS_TYPE_LOC = (ulong)0x001dUL;
        public static readonly ulong DNS_TYPE_NXT = (ulong)0x001eUL;
        public static readonly ulong DNS_TYPE_EID = (ulong)0x001fUL;
        public static readonly ulong DNS_TYPE_NIMLOC = (ulong)0x0020UL;
        public static readonly ulong DNS_TYPE_SRV = (ulong)0x0021UL;
        public static readonly ulong DNS_TYPE_ATMA = (ulong)0x0022UL;
        public static readonly ulong DNS_TYPE_NAPTR = (ulong)0x0023UL;
        public static readonly ulong DNS_TYPE_KX = (ulong)0x0024UL;
        public static readonly ulong DNS_TYPE_CERT = (ulong)0x0025UL;
        public static readonly ulong DNS_TYPE_A6 = (ulong)0x0026UL;
        public static readonly ulong DNS_TYPE_DNAME = (ulong)0x0027UL;
        public static readonly ulong DNS_TYPE_SINK = (ulong)0x0028UL;
        public static readonly ulong DNS_TYPE_OPT = (ulong)0x0029UL;
        public static readonly ulong DNS_TYPE_DS = (ulong)0x002BUL;
        public static readonly ulong DNS_TYPE_RRSIG = (ulong)0x002EUL;
        public static readonly ulong DNS_TYPE_NSEC = (ulong)0x002FUL;
        public static readonly ulong DNS_TYPE_DNSKEY = (ulong)0x0030UL;
        public static readonly ulong DNS_TYPE_DHCID = (ulong)0x0031UL;
        public static readonly ulong DNS_TYPE_UINFO = (ulong)0x0064UL;
        public static readonly ulong DNS_TYPE_UID = (ulong)0x0065UL;
        public static readonly ulong DNS_TYPE_GID = (ulong)0x0066UL;
        public static readonly ulong DNS_TYPE_UNSPEC = (ulong)0x0067UL;
        public static readonly ulong DNS_TYPE_ADDRS = (ulong)0x00f8UL;
        public static readonly ulong DNS_TYPE_TKEY = (ulong)0x00f9UL;
        public static readonly ulong DNS_TYPE_TSIG = (ulong)0x00faUL;
        public static readonly ulong DNS_TYPE_IXFR = (ulong)0x00fbUL;
        public static readonly ulong DNS_TYPE_AXFR = (ulong)0x00fcUL;
        public static readonly ulong DNS_TYPE_MAILB = (ulong)0x00fdUL;
        public static readonly ulong DNS_TYPE_MAILA = (ulong)0x00feUL;
        public static readonly ulong DNS_TYPE_ALL = (ulong)0x00ffUL;
        public static readonly ulong DNS_TYPE_ANY = (ulong)0x00ffUL;
        public static readonly ulong DNS_TYPE_WINS = (ulong)0xff01UL;
        public static readonly ulong DNS_TYPE_WINSR = (ulong)0xff02UL;
        public static readonly ulong DNS_TYPE_NBSTAT = (ulong)0xff01UL;


        public static readonly ulong DNS_INFO_NO_RECORDS = (ulong)0x251DUL;


 
        // flags inside DNSRecord.Dw
        public static readonly ulong DnsSectionQuestion = (ulong)0x0000UL;
        public static readonly ulong DnsSectionAnswer = (ulong)0x0001UL;
        public static readonly ulong DnsSectionAuthority = (ulong)0x0002UL;
        public static readonly ulong DnsSectionAdditional = (ulong)0x0003UL;


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
            public array<ptr<ushort>> StringArray;
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

        public static readonly long TF_DISCONNECT = (long)1L;
        public static readonly long TF_REUSE_SOCKET = (long)2L;
        public static readonly long TF_WRITE_BEHIND = (long)4L;
        public static readonly long TF_USE_DEFAULT_WORKER = (long)0L;
        public static readonly long TF_USE_SYSTEM_THREAD = (long)16L;
        public static readonly long TF_USE_KERNEL_APC = (long)32L;


        public partial struct TransmitFileBuffers
        {
            public System.UIntPtr Head;
            public uint HeadLength;
            public System.UIntPtr Tail;
            public uint TailLength;
        }

        public static readonly long IFF_UP = (long)1L;
        public static readonly long IFF_BROADCAST = (long)2L;
        public static readonly long IFF_LOOPBACK = (long)4L;
        public static readonly long IFF_POINTTOPOINT = (long)8L;
        public static readonly long IFF_MULTICAST = (long)16L;


        public static readonly ulong SIO_GET_INTERFACE_LIST = (ulong)0x4004747FUL;

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

        public static readonly long MAX_ADAPTER_NAME_LENGTH = (long)256L;

        public static readonly long MAX_ADAPTER_DESCRIPTION_LENGTH = (long)128L;

        public static readonly long MAX_ADAPTER_ADDRESS_LENGTH = (long)8L;



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

        public static readonly long MAXLEN_PHYSADDR = (long)8L;

        public static readonly long MAX_INTERFACE_NAME_LEN = (long)256L;

        public static readonly long MAXLEN_IFDESCR = (long)256L;



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

        public partial struct CertInfo
        {
        }

        public partial struct CertContext
        {
            public uint EncodingType;
            public ptr<byte> EncodedCert;
            public uint Length;
            public ptr<CertInfo> CertInfo;
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

        public partial struct CertTrustListInfo
        {
        }

        public partial struct CertSimpleChain
        {
            public uint Size;
            public CertTrustStatus TrustStatus;
            public uint NumElements;
            public ptr<ptr<CertChainElement>> Elements;
            public ptr<CertTrustListInfo> TrustListInfo;
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

        public partial struct CertRevocationCrlInfo
        {
        }

        public partial struct CertRevocationInfo
        {
            public uint Size;
            public uint RevocationResult;
            public ptr<byte> RevocationOid;
            public Pointer OidSpecificInfo;
            public uint HasFreshnessTime;
            public uint FreshnessTime;
            public ptr<CertRevocationCrlInfo> CrlInfo;
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
            public Pointer ExtraPolicyPara;
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
            public Pointer ExtraPolicyStatus;
        }

 
        // do not reorder
        public static readonly ulong HKEY_CLASSES_ROOT = (ulong)0x80000000UL + iota;
        public static readonly var HKEY_CURRENT_USER = (var)0;
        public static readonly var HKEY_LOCAL_MACHINE = (var)1;
        public static readonly var HKEY_USERS = (var)2;
        public static readonly var HKEY_PERFORMANCE_DATA = (var)3;
        public static readonly var HKEY_CURRENT_CONFIG = (var)4;
        public static readonly KEY_QUERY_VALUE HKEY_DYN_DATA = (KEY_QUERY_VALUE)1L;
        public static readonly long KEY_SET_VALUE = (long)2L;
        public static readonly long KEY_CREATE_SUB_KEY = (long)4L;
        public static readonly long KEY_ENUMERATE_SUB_KEYS = (long)8L;
        public static readonly long KEY_NOTIFY = (long)16L;
        public static readonly long KEY_CREATE_LINK = (long)32L;
        public static readonly ulong KEY_WRITE = (ulong)0x20006UL;
        public static readonly ulong KEY_EXECUTE = (ulong)0x20019UL;
        public static readonly ulong KEY_READ = (ulong)0x20019UL;
        public static readonly ulong KEY_WOW64_64KEY = (ulong)0x0100UL;
        public static readonly ulong KEY_WOW64_32KEY = (ulong)0x0200UL;
        public static readonly ulong KEY_ALL_ACCESS = (ulong)0xf003fUL;


 
        // do not reorder
        public static readonly var REG_NONE = (var)iota;
        public static readonly var REG_SZ = (var)0;
        public static readonly var REG_EXPAND_SZ = (var)1;
        public static readonly var REG_BINARY = (var)2;
        public static readonly var REG_DWORD_LITTLE_ENDIAN = (var)3;
        public static readonly var REG_DWORD_BIG_ENDIAN = (var)4;
        public static readonly var REG_LINK = (var)5;
        public static readonly var REG_MULTI_SZ = (var)6;
        public static readonly var REG_RESOURCE_LIST = (var)7;
        public static readonly var REG_FULL_RESOURCE_DESCRIPTOR = (var)8;
        public static readonly var REG_RESOURCE_REQUIREMENTS_LIST = (var)9;
        public static readonly REG_DWORD REG_QWORD_LITTLE_ENDIAN = (REG_DWORD)REG_DWORD_LITTLE_ENDIAN;
        public static readonly var REG_QWORD = (var)REG_QWORD_LITTLE_ENDIAN;


        public partial struct AddrinfoW
        {
            public int Flags;
            public int Family;
            public int Socktype;
            public int Protocol;
            public System.UIntPtr Addrlen;
            public ptr<ushort> Canonname;
            public Pointer Addr;
            public ptr<AddrinfoW> Next;
        }

        public static readonly long AI_PASSIVE = (long)1L;
        public static readonly long AI_CANONNAME = (long)2L;
        public static readonly long AI_NUMERICHOST = (long)4L;


        public partial struct GUID
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            public array<byte> Data4;
        }

        public static GUID WSAID_CONNECTEX = new GUID(0x25a207b9,0xddf3,0x4660,[8]byte{0x8e,0xe9,0x76,0xe5,0x8c,0x74,0x06,0x3e},);

        public static readonly long FILE_SKIP_COMPLETION_PORT_ON_SUCCESS = (long)1L;
        public static readonly long FILE_SKIP_SET_EVENT_ON_HANDLE = (long)2L;


        public static readonly long WSAPROTOCOL_LEN = (long)255L;
        public static readonly long MAX_PROTOCOL_CHAIN = (long)7L;
        public static readonly long BASE_PROTOCOL = (long)1L;
        public static readonly long LAYERED_PROTOCOL = (long)0L;

        public static readonly ulong XP1_CONNECTIONLESS = (ulong)0x00000001UL;
        public static readonly ulong XP1_GUARANTEED_DELIVERY = (ulong)0x00000002UL;
        public static readonly ulong XP1_GUARANTEED_ORDER = (ulong)0x00000004UL;
        public static readonly ulong XP1_MESSAGE_ORIENTED = (ulong)0x00000008UL;
        public static readonly ulong XP1_PSEUDO_STREAM = (ulong)0x00000010UL;
        public static readonly ulong XP1_GRACEFUL_CLOSE = (ulong)0x00000020UL;
        public static readonly ulong XP1_EXPEDITED_DATA = (ulong)0x00000040UL;
        public static readonly ulong XP1_CONNECT_DATA = (ulong)0x00000080UL;
        public static readonly ulong XP1_DISCONNECT_DATA = (ulong)0x00000100UL;
        public static readonly ulong XP1_SUPPORT_BROADCAST = (ulong)0x00000200UL;
        public static readonly ulong XP1_SUPPORT_MULTIPOINT = (ulong)0x00000400UL;
        public static readonly ulong XP1_MULTIPOINT_CONTROL_PLANE = (ulong)0x00000800UL;
        public static readonly ulong XP1_MULTIPOINT_DATA_PLANE = (ulong)0x00001000UL;
        public static readonly ulong XP1_QOS_SUPPORTED = (ulong)0x00002000UL;
        public static readonly ulong XP1_UNI_SEND = (ulong)0x00008000UL;
        public static readonly ulong XP1_UNI_RECV = (ulong)0x00010000UL;
        public static readonly ulong XP1_IFS_HANDLES = (ulong)0x00020000UL;
        public static readonly ulong XP1_PARTIAL_MESSAGE = (ulong)0x00040000UL;
        public static readonly ulong XP1_SAN_SUPPORT_SDP = (ulong)0x00080000UL;

        public static readonly ulong PFL_MULTIPLE_PROTO_ENTRIES = (ulong)0x00000001UL;
        public static readonly ulong PFL_RECOMMENDED_PROTO_ENTRY = (ulong)0x00000002UL;
        public static readonly ulong PFL_HIDDEN = (ulong)0x00000004UL;
        public static readonly ulong PFL_MATCHES_PROTOCOL_ZERO = (ulong)0x00000008UL;
        public static readonly ulong PFL_NETWORKDIRECT_PROVIDER = (ulong)0x00000010UL;


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

        public static readonly ulong FSCTL_GET_REPARSE_POINT = (ulong)0x900A8UL;
        public static readonly long MAXIMUM_REPARSE_DATA_BUFFER_SIZE = (long)16L * 1024L;
        private static readonly ulong _IO_REPARSE_TAG_MOUNT_POINT = (ulong)0xA0000003UL;
        public static readonly ulong IO_REPARSE_TAG_SYMLINK = (ulong)0xA000000CUL;
        public static readonly ulong SYMBOLIC_LINK_FLAG_DIRECTORY = (ulong)0x1UL;
        private static readonly long _SYMLINK_FLAG_RELATIVE = (long)1L;


        public static readonly long UNIX_PATH_MAX = (long)108L; // defined in afunix.h
 // defined in afunix.h
    }
}
