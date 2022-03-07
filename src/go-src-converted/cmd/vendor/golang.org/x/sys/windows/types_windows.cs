// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 23:30:43 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\types_windows.go
using net = go.net_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

    // NTStatus corresponds with NTSTATUS, error values returned by ntdll.dll and
    // other native functions.
public partial struct NTStatus { // : uint
}

 
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
public static readonly nuint FILE_ATTRIBUTE_DEVICE = 0x00000040;
public static readonly nuint FILE_ATTRIBUTE_NORMAL = 0x00000080;
public static readonly nuint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
public static readonly nuint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
public static readonly nuint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
public static readonly nuint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
public static readonly nuint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
public static readonly nuint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
public static readonly nuint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
public static readonly nuint FILE_ATTRIBUTE_INTEGRITY_STREAM = 0x00008000;
public static readonly nuint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;
public static readonly nuint FILE_ATTRIBUTE_NO_SCRUB_DATA = 0x00020000;
public static readonly nuint FILE_ATTRIBUTE_RECALL_ON_OPEN = 0x00040000;
public static readonly nuint FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x00400000;

public static readonly nuint INVALID_FILE_ATTRIBUTES = 0xffffffff;

public static readonly nint CREATE_NEW = 1;
public static readonly nint CREATE_ALWAYS = 2;
public static readonly nint OPEN_EXISTING = 3;
public static readonly nint OPEN_ALWAYS = 4;
public static readonly nint TRUNCATE_EXISTING = 5;

public static readonly nuint FILE_FLAG_OPEN_REQUIRING_OPLOCK = 0x00040000;
public static readonly nuint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
public static readonly nuint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
public static readonly nuint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
public static readonly nuint FILE_FLAG_SESSION_AWARE = 0x00800000;
public static readonly nuint FILE_FLAG_POSIX_SEMANTICS = 0x01000000;
public static readonly nuint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
public static readonly nuint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
public static readonly nuint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
public static readonly nuint FILE_FLAG_RANDOM_ACCESS = 0x10000000;
public static readonly nuint FILE_FLAG_NO_BUFFERING = 0x20000000;
public static readonly nuint FILE_FLAG_OVERLAPPED = 0x40000000;
public static readonly nuint FILE_FLAG_WRITE_THROUGH = 0x80000000;

public static readonly nuint HANDLE_FLAG_INHERIT = 0x00000001;
public static readonly nuint STARTF_USESTDHANDLES = 0x00000100;
public static readonly nuint STARTF_USESHOWWINDOW = 0x00000001;
public static readonly nuint DUPLICATE_CLOSE_SOURCE = 0x00000001;
public static readonly nuint DUPLICATE_SAME_ACCESS = 0x00000002;

public static readonly nint STD_INPUT_HANDLE = -10 & (1 << 32 - 1);
public static readonly nint STD_OUTPUT_HANDLE = -11 & (1 << 32 - 1);
public static readonly nint STD_ERROR_HANDLE = -12 & (1 << 32 - 1);

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

public static readonly nuint WAIT_ABANDONED = 0x00000080;
public static readonly nuint WAIT_OBJECT_0 = 0x00000000;
public static readonly nuint WAIT_FAILED = 0xFFFFFFFF; 

// Access rights for process.
public static readonly nuint PROCESS_CREATE_PROCESS = 0x0080;
public static readonly nuint PROCESS_CREATE_THREAD = 0x0002;
public static readonly nuint PROCESS_DUP_HANDLE = 0x0040;
public static readonly nuint PROCESS_QUERY_INFORMATION = 0x0400;
public static readonly nuint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
public static readonly nuint PROCESS_SET_INFORMATION = 0x0200;
public static readonly nuint PROCESS_SET_QUOTA = 0x0100;
public static readonly nuint PROCESS_SUSPEND_RESUME = 0x0800;
public static readonly nuint PROCESS_TERMINATE = 0x0001;
public static readonly nuint PROCESS_VM_OPERATION = 0x0008;
public static readonly nuint PROCESS_VM_READ = 0x0010;
public static readonly nuint PROCESS_VM_WRITE = 0x0020; 

// Access rights for thread.
public static readonly nuint THREAD_DIRECT_IMPERSONATION = 0x0200;
public static readonly nuint THREAD_GET_CONTEXT = 0x0008;
public static readonly nuint THREAD_IMPERSONATE = 0x0100;
public static readonly nuint THREAD_QUERY_INFORMATION = 0x0040;
public static readonly nuint THREAD_QUERY_LIMITED_INFORMATION = 0x0800;
public static readonly nuint THREAD_SET_CONTEXT = 0x0010;
public static readonly nuint THREAD_SET_INFORMATION = 0x0020;
public static readonly nuint THREAD_SET_LIMITED_INFORMATION = 0x0400;
public static readonly nuint THREAD_SET_THREAD_TOKEN = 0x0080;
public static readonly nuint THREAD_SUSPEND_RESUME = 0x0002;
public static readonly nuint THREAD_TERMINATE = 0x0001;

public static readonly nuint FILE_MAP_COPY = 0x01;
public static readonly nuint FILE_MAP_WRITE = 0x02;
public static readonly nuint FILE_MAP_READ = 0x04;
public static readonly nuint FILE_MAP_EXECUTE = 0x20;

public static readonly nint CTRL_C_EVENT = 0;
public static readonly nint CTRL_BREAK_EVENT = 1;
public static readonly nint CTRL_CLOSE_EVENT = 2;
public static readonly nint CTRL_LOGOFF_EVENT = 5;
public static readonly nint CTRL_SHUTDOWN_EVENT = 6; 

// Windows reserves errors >= 1<<29 for application use.
public static readonly nint APPLICATION_ERROR = 1 << 29;


 
// Process creation flags.
public static readonly nuint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
public static readonly nuint CREATE_DEFAULT_ERROR_MODE = 0x04000000;
public static readonly nuint CREATE_NEW_CONSOLE = 0x00000010;
public static readonly nuint CREATE_NEW_PROCESS_GROUP = 0x00000200;
public static readonly nuint CREATE_NO_WINDOW = 0x08000000;
public static readonly nuint CREATE_PROTECTED_PROCESS = 0x00040000;
public static readonly nuint CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000;
public static readonly nuint CREATE_SEPARATE_WOW_VDM = 0x00000800;
public static readonly nuint CREATE_SHARED_WOW_VDM = 0x00001000;
public static readonly nuint CREATE_SUSPENDED = 0x00000004;
public static readonly nuint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
public static readonly nuint DEBUG_ONLY_THIS_PROCESS = 0x00000002;
public static readonly nuint DEBUG_PROCESS = 0x00000001;
public static readonly nuint DETACHED_PROCESS = 0x00000008;
public static readonly nuint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
public static readonly nuint INHERIT_PARENT_AFFINITY = 0x00010000;


 
// attributes for ProcThreadAttributeList
public static readonly nuint PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
public static readonly nuint PROC_THREAD_ATTRIBUTE_HANDLE_LIST = 0x00020002;
public static readonly nuint PROC_THREAD_ATTRIBUTE_GROUP_AFFINITY = 0x00030003;
public static readonly nuint PROC_THREAD_ATTRIBUTE_PREFERRED_NODE = 0x00020004;
public static readonly nuint PROC_THREAD_ATTRIBUTE_IDEAL_PROCESSOR = 0x00030005;
public static readonly nuint PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;
public static readonly nuint PROC_THREAD_ATTRIBUTE_UMS_THREAD = 0x00030006;
public static readonly nuint PROC_THREAD_ATTRIBUTE_PROTECTION_LEVEL = 0x0002000b;


 
// flags for CreateToolhelp32Snapshot
public static readonly nuint TH32CS_SNAPHEAPLIST = 0x01;
public static readonly nuint TH32CS_SNAPPROCESS = 0x02;
public static readonly nuint TH32CS_SNAPTHREAD = 0x04;
public static readonly nuint TH32CS_SNAPMODULE = 0x08;
public static readonly nuint TH32CS_SNAPMODULE32 = 0x10;
public static readonly var TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPMODULE | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD;
public static readonly nuint TH32CS_INHERIT = 0x80000000;


 
// filters for ReadDirectoryChangesW and FindFirstChangeNotificationW
public static readonly nuint FILE_NOTIFY_CHANGE_FILE_NAME = 0x001;
public static readonly nuint FILE_NOTIFY_CHANGE_DIR_NAME = 0x002;
public static readonly nuint FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x004;
public static readonly nuint FILE_NOTIFY_CHANGE_SIZE = 0x008;
public static readonly nuint FILE_NOTIFY_CHANGE_LAST_WRITE = 0x010;
public static readonly nuint FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x020;
public static readonly nuint FILE_NOTIFY_CHANGE_CREATION = 0x040;
public static readonly nuint FILE_NOTIFY_CHANGE_SECURITY = 0x100;


 
// do not reorder
public static readonly var FILE_ACTION_ADDED = iota + 1;
public static readonly var FILE_ACTION_REMOVED = 0;
public static readonly var FILE_ACTION_MODIFIED = 1;
public static readonly var FILE_ACTION_RENAMED_OLD_NAME = 2;
public static readonly var FILE_ACTION_RENAMED_NEW_NAME = 3;


 
// wincrypt.h
/* certenrolld_begin -- PROV_RSA_*/
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

/* dwFlags definitions for CryptAcquireContext */
public static readonly nuint CRYPT_VERIFYCONTEXT = 0xF0000000;
public static readonly nuint CRYPT_NEWKEYSET = 0x00000008;
public static readonly nuint CRYPT_DELETEKEYSET = 0x00000010;
public static readonly nuint CRYPT_MACHINE_KEYSET = 0x00000020;
public static readonly nuint CRYPT_SILENT = 0x00000040;
public static readonly nuint CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x00000080; 

/* Flags for PFXImportCertStore */
public static readonly nuint CRYPT_EXPORTABLE = 0x00000001;
public static readonly nuint CRYPT_USER_PROTECTED = 0x00000002;
public static readonly nuint CRYPT_USER_KEYSET = 0x00001000;
public static readonly nuint PKCS12_PREFER_CNG_KSP = 0x00000100;
public static readonly nuint PKCS12_ALWAYS_CNG_KSP = 0x00000200;
public static readonly nuint PKCS12_ALLOW_OVERWRITE_KEY = 0x00004000;
public static readonly nuint PKCS12_NO_PERSIST_KEY = 0x00008000;
public static readonly nuint PKCS12_INCLUDE_EXTENDED_PROPERTIES = 0x00000010; 

/* Flags for CryptAcquireCertificatePrivateKey */
public static readonly nuint CRYPT_ACQUIRE_CACHE_FLAG = 0x00000001;
public static readonly nuint CRYPT_ACQUIRE_USE_PROV_INFO_FLAG = 0x00000002;
public static readonly nuint CRYPT_ACQUIRE_COMPARE_KEY_FLAG = 0x00000004;
public static readonly nuint CRYPT_ACQUIRE_NO_HEALING = 0x00000008;
public static readonly nuint CRYPT_ACQUIRE_SILENT_FLAG = 0x00000040;
public static readonly nuint CRYPT_ACQUIRE_WINDOW_HANDLE_FLAG = 0x00000080;
public static readonly nuint CRYPT_ACQUIRE_NCRYPT_KEY_FLAGS_MASK = 0x00070000;
public static readonly nuint CRYPT_ACQUIRE_ALLOW_NCRYPT_KEY_FLAG = 0x00010000;
public static readonly nuint CRYPT_ACQUIRE_PREFER_NCRYPT_KEY_FLAG = 0x00020000;
public static readonly nuint CRYPT_ACQUIRE_ONLY_NCRYPT_KEY_FLAG = 0x00040000; 

/* pdwKeySpec for CryptAcquireCertificatePrivateKey */
public static readonly nint AT_KEYEXCHANGE = 1;
public static readonly nint AT_SIGNATURE = 2;
public static readonly nuint CERT_NCRYPT_KEY_SPEC = 0xFFFFFFFF; 

/* Default usage match type is AND with value zero */
public static readonly nint USAGE_MATCH_TYPE_AND = 0;
public static readonly nint USAGE_MATCH_TYPE_OR = 1; 

/* msgAndCertEncodingType values for CertOpenStore function */
public static readonly nuint X509_ASN_ENCODING = 0x00000001;
public static readonly nuint PKCS_7_ASN_ENCODING = 0x00010000; 

/* storeProvider values for CertOpenStore function */
public static readonly nint CERT_STORE_PROV_MSG = 1;
public static readonly nint CERT_STORE_PROV_MEMORY = 2;
public static readonly nint CERT_STORE_PROV_FILE = 3;
public static readonly nint CERT_STORE_PROV_REG = 4;
public static readonly nint CERT_STORE_PROV_PKCS7 = 5;
public static readonly nint CERT_STORE_PROV_SERIALIZED = 6;
public static readonly nint CERT_STORE_PROV_FILENAME_A = 7;
public static readonly nint CERT_STORE_PROV_FILENAME_W = 8;
public static readonly var CERT_STORE_PROV_FILENAME = CERT_STORE_PROV_FILENAME_W;
public static readonly nint CERT_STORE_PROV_SYSTEM_A = 9;
public static readonly nint CERT_STORE_PROV_SYSTEM_W = 10;
public static readonly var CERT_STORE_PROV_SYSTEM = CERT_STORE_PROV_SYSTEM_W;
public static readonly nint CERT_STORE_PROV_COLLECTION = 11;
public static readonly nint CERT_STORE_PROV_SYSTEM_REGISTRY_A = 12;
public static readonly nint CERT_STORE_PROV_SYSTEM_REGISTRY_W = 13;
public static readonly var CERT_STORE_PROV_SYSTEM_REGISTRY = CERT_STORE_PROV_SYSTEM_REGISTRY_W;
public static readonly nint CERT_STORE_PROV_PHYSICAL_W = 14;
public static readonly var CERT_STORE_PROV_PHYSICAL = CERT_STORE_PROV_PHYSICAL_W;
public static readonly nint CERT_STORE_PROV_SMART_CARD_W = 15;
public static readonly var CERT_STORE_PROV_SMART_CARD = CERT_STORE_PROV_SMART_CARD_W;
public static readonly nint CERT_STORE_PROV_LDAP_W = 16;
public static readonly var CERT_STORE_PROV_LDAP = CERT_STORE_PROV_LDAP_W;
public static readonly nint CERT_STORE_PROV_PKCS12 = 17; 

/* store characteristics (low WORD of flag) for CertOpenStore function */
public static readonly nuint CERT_STORE_NO_CRYPT_RELEASE_FLAG = 0x00000001;
public static readonly nuint CERT_STORE_SET_LOCALIZED_NAME_FLAG = 0x00000002;
public static readonly nuint CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 0x00000004;
public static readonly nuint CERT_STORE_DELETE_FLAG = 0x00000010;
public static readonly nuint CERT_STORE_UNSAFE_PHYSICAL_FLAG = 0x00000020;
public static readonly nuint CERT_STORE_SHARE_STORE_FLAG = 0x00000040;
public static readonly nuint CERT_STORE_SHARE_CONTEXT_FLAG = 0x00000080;
public static readonly nuint CERT_STORE_MANIFOLD_FLAG = 0x00000100;
public static readonly nuint CERT_STORE_ENUM_ARCHIVED_FLAG = 0x00000200;
public static readonly nuint CERT_STORE_UPDATE_KEYID_FLAG = 0x00000400;
public static readonly nuint CERT_STORE_BACKUP_RESTORE_FLAG = 0x00000800;
public static readonly nuint CERT_STORE_MAXIMUM_ALLOWED_FLAG = 0x00001000;
public static readonly nuint CERT_STORE_CREATE_NEW_FLAG = 0x00002000;
public static readonly nuint CERT_STORE_OPEN_EXISTING_FLAG = 0x00004000;
public static readonly nuint CERT_STORE_READONLY_FLAG = 0x00008000; 

/* store locations (high WORD of flag) for CertOpenStore function */
public static readonly nuint CERT_SYSTEM_STORE_CURRENT_USER = 0x00010000;
public static readonly nuint CERT_SYSTEM_STORE_LOCAL_MACHINE = 0x00020000;
public static readonly nuint CERT_SYSTEM_STORE_CURRENT_SERVICE = 0x00040000;
public static readonly nuint CERT_SYSTEM_STORE_SERVICES = 0x00050000;
public static readonly nuint CERT_SYSTEM_STORE_USERS = 0x00060000;
public static readonly nuint CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY = 0x00070000;
public static readonly nuint CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY = 0x00080000;
public static readonly nuint CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE = 0x00090000;
public static readonly nuint CERT_SYSTEM_STORE_UNPROTECTED_FLAG = 0x40000000;
public static readonly nuint CERT_SYSTEM_STORE_RELOCATE_FLAG = 0x80000000; 

/* Miscellaneous high-WORD flags for CertOpenStore function */
public static readonly nuint CERT_REGISTRY_STORE_REMOTE_FLAG = 0x00010000;
public static readonly nuint CERT_REGISTRY_STORE_SERIALIZED_FLAG = 0x00020000;
public static readonly nuint CERT_REGISTRY_STORE_ROAMING_FLAG = 0x00040000;
public static readonly nuint CERT_REGISTRY_STORE_MY_IE_DIRTY_FLAG = 0x00080000;
public static readonly nuint CERT_REGISTRY_STORE_LM_GPT_FLAG = 0x01000000;
public static readonly nuint CERT_REGISTRY_STORE_CLIENT_GPT_FLAG = 0x80000000;
public static readonly nuint CERT_FILE_STORE_COMMIT_ENABLE_FLAG = 0x00010000;
public static readonly nuint CERT_LDAP_STORE_SIGN_FLAG = 0x00010000;
public static readonly nuint CERT_LDAP_STORE_AREC_EXCLUSIVE_FLAG = 0x00020000;
public static readonly nuint CERT_LDAP_STORE_OPENED_FLAG = 0x00040000;
public static readonly nuint CERT_LDAP_STORE_UNBIND_FLAG = 0x00080000; 

/* addDisposition values for CertAddCertificateContextToStore function */
public static readonly nint CERT_STORE_ADD_NEW = 1;
public static readonly nint CERT_STORE_ADD_USE_EXISTING = 2;
public static readonly nint CERT_STORE_ADD_REPLACE_EXISTING = 3;
public static readonly nint CERT_STORE_ADD_ALWAYS = 4;
public static readonly nint CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES = 5;
public static readonly nint CERT_STORE_ADD_NEWER = 6;
public static readonly nint CERT_STORE_ADD_NEWER_INHERIT_PROPERTIES = 7; 

/* ErrorStatus values for CertTrustStatus struct */
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
public static readonly nuint CERT_TRUST_IS_PARTIAL_CHAIN = 0x00010000;
public static readonly nuint CERT_TRUST_CTL_IS_NOT_TIME_VALID = 0x00020000;
public static readonly nuint CERT_TRUST_CTL_IS_NOT_SIGNATURE_VALID = 0x00040000;
public static readonly nuint CERT_TRUST_CTL_IS_NOT_VALID_FOR_USAGE = 0x00080000;
public static readonly nuint CERT_TRUST_HAS_WEAK_SIGNATURE = 0x00100000;
public static readonly nuint CERT_TRUST_IS_OFFLINE_REVOCATION = 0x01000000;
public static readonly nuint CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY = 0x02000000;
public static readonly nuint CERT_TRUST_IS_EXPLICIT_DISTRUST = 0x04000000;
public static readonly nuint CERT_TRUST_HAS_NOT_SUPPORTED_CRITICAL_EXT = 0x08000000; 

/* InfoStatus values for CertTrustStatus struct */
public static readonly nuint CERT_TRUST_HAS_EXACT_MATCH_ISSUER = 0x00000001;
public static readonly nuint CERT_TRUST_HAS_KEY_MATCH_ISSUER = 0x00000002;
public static readonly nuint CERT_TRUST_HAS_NAME_MATCH_ISSUER = 0x00000004;
public static readonly nuint CERT_TRUST_IS_SELF_SIGNED = 0x00000008;
public static readonly nuint CERT_TRUST_HAS_PREFERRED_ISSUER = 0x00000100;
public static readonly nuint CERT_TRUST_HAS_ISSUANCE_CHAIN_POLICY = 0x00000400;
public static readonly nuint CERT_TRUST_HAS_VALID_NAME_CONSTRAINTS = 0x00000400;
public static readonly nuint CERT_TRUST_IS_PEER_TRUSTED = 0x00000800;
public static readonly nuint CERT_TRUST_HAS_CRL_VALIDITY_EXTENDED = 0x00001000;
public static readonly nuint CERT_TRUST_IS_FROM_EXCLUSIVE_TRUST_STORE = 0x00002000;
public static readonly nuint CERT_TRUST_IS_CA_TRUSTED = 0x00004000;
public static readonly nuint CERT_TRUST_IS_COMPLEX_CHAIN = 0x00010000; 

/* Certificate Information Flags */
public static readonly nint CERT_INFO_VERSION_FLAG = 1;
public static readonly nint CERT_INFO_SERIAL_NUMBER_FLAG = 2;
public static readonly nint CERT_INFO_SIGNATURE_ALGORITHM_FLAG = 3;
public static readonly nint CERT_INFO_ISSUER_FLAG = 4;
public static readonly nint CERT_INFO_NOT_BEFORE_FLAG = 5;
public static readonly nint CERT_INFO_NOT_AFTER_FLAG = 6;
public static readonly nint CERT_INFO_SUBJECT_FLAG = 7;
public static readonly nint CERT_INFO_SUBJECT_PUBLIC_KEY_INFO_FLAG = 8;
public static readonly nint CERT_INFO_ISSUER_UNIQUE_ID_FLAG = 9;
public static readonly nint CERT_INFO_SUBJECT_UNIQUE_ID_FLAG = 10;
public static readonly nint CERT_INFO_EXTENSION_FLAG = 11; 

/* dwFindType for CertFindCertificateInStore  */
public static readonly nuint CERT_COMPARE_MASK = 0xFFFF;
public static readonly nint CERT_COMPARE_SHIFT = 16;
public static readonly nint CERT_COMPARE_ANY = 0;
public static readonly nint CERT_COMPARE_SHA1_HASH = 1;
public static readonly nint CERT_COMPARE_NAME = 2;
public static readonly nint CERT_COMPARE_ATTR = 3;
public static readonly nint CERT_COMPARE_MD5_HASH = 4;
public static readonly nint CERT_COMPARE_PROPERTY = 5;
public static readonly nint CERT_COMPARE_PUBLIC_KEY = 6;
public static readonly var CERT_COMPARE_HASH = CERT_COMPARE_SHA1_HASH;
public static readonly nint CERT_COMPARE_NAME_STR_A = 7;
public static readonly nint CERT_COMPARE_NAME_STR_W = 8;
public static readonly nint CERT_COMPARE_KEY_SPEC = 9;
public static readonly nint CERT_COMPARE_ENHKEY_USAGE = 10;
public static readonly var CERT_COMPARE_CTL_USAGE = CERT_COMPARE_ENHKEY_USAGE;
public static readonly nint CERT_COMPARE_SUBJECT_CERT = 11;
public static readonly nint CERT_COMPARE_ISSUER_OF = 12;
public static readonly nint CERT_COMPARE_EXISTING = 13;
public static readonly nint CERT_COMPARE_SIGNATURE_HASH = 14;
public static readonly nint CERT_COMPARE_KEY_IDENTIFIER = 15;
public static readonly nint CERT_COMPARE_CERT_ID = 16;
public static readonly nint CERT_COMPARE_CROSS_CERT_DIST_POINTS = 17;
public static readonly nint CERT_COMPARE_PUBKEY_MD5_HASH = 18;
public static readonly nint CERT_COMPARE_SUBJECT_INFO_ACCESS = 19;
public static readonly nint CERT_COMPARE_HASH_STR = 20;
public static readonly nint CERT_COMPARE_HAS_PRIVATE_KEY = 21;
public static readonly var CERT_FIND_ANY = (CERT_COMPARE_ANY << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_SHA1_HASH = (CERT_COMPARE_SHA1_HASH << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_MD5_HASH = (CERT_COMPARE_MD5_HASH << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_SIGNATURE_HASH = (CERT_COMPARE_SIGNATURE_HASH << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_KEY_IDENTIFIER = (CERT_COMPARE_KEY_IDENTIFIER << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_HASH = CERT_FIND_SHA1_HASH;
public static readonly var CERT_FIND_PROPERTY = (CERT_COMPARE_PROPERTY << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_PUBLIC_KEY = (CERT_COMPARE_PUBLIC_KEY << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_SUBJECT_NAME = (CERT_COMPARE_NAME << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_SUBJECT_FLAG);
public static readonly var CERT_FIND_SUBJECT_ATTR = (CERT_COMPARE_ATTR << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_SUBJECT_FLAG);
public static readonly var CERT_FIND_ISSUER_NAME = (CERT_COMPARE_NAME << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_ISSUER_FLAG);
public static readonly var CERT_FIND_ISSUER_ATTR = (CERT_COMPARE_ATTR << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_ISSUER_FLAG);
public static readonly var CERT_FIND_SUBJECT_STR_A = (CERT_COMPARE_NAME_STR_A << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_SUBJECT_FLAG);
public static readonly var CERT_FIND_SUBJECT_STR_W = (CERT_COMPARE_NAME_STR_W << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_SUBJECT_FLAG);
public static readonly var CERT_FIND_SUBJECT_STR = CERT_FIND_SUBJECT_STR_W;
public static readonly var CERT_FIND_ISSUER_STR_A = (CERT_COMPARE_NAME_STR_A << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_ISSUER_FLAG);
public static readonly var CERT_FIND_ISSUER_STR_W = (CERT_COMPARE_NAME_STR_W << (int)(CERT_COMPARE_SHIFT) | CERT_INFO_ISSUER_FLAG);
public static readonly var CERT_FIND_ISSUER_STR = CERT_FIND_ISSUER_STR_W;
public static readonly var CERT_FIND_KEY_SPEC = (CERT_COMPARE_KEY_SPEC << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_ENHKEY_USAGE = (CERT_COMPARE_ENHKEY_USAGE << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_CTL_USAGE = CERT_FIND_ENHKEY_USAGE;
public static readonly var CERT_FIND_SUBJECT_CERT = (CERT_COMPARE_SUBJECT_CERT << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_ISSUER_OF = (CERT_COMPARE_ISSUER_OF << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_EXISTING = (CERT_COMPARE_EXISTING << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_CERT_ID = (CERT_COMPARE_CERT_ID << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_CROSS_CERT_DIST_POINTS = (CERT_COMPARE_CROSS_CERT_DIST_POINTS << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_PUBKEY_MD5_HASH = (CERT_COMPARE_PUBKEY_MD5_HASH << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_SUBJECT_INFO_ACCESS = (CERT_COMPARE_SUBJECT_INFO_ACCESS << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_HASH_STR = (CERT_COMPARE_HASH_STR << (int)(CERT_COMPARE_SHIFT));
public static readonly var CERT_FIND_HAS_PRIVATE_KEY = (CERT_COMPARE_HAS_PRIVATE_KEY << (int)(CERT_COMPARE_SHIFT));
public static readonly nuint CERT_FIND_OPTIONAL_ENHKEY_USAGE_FLAG = 0x1;
public static readonly nuint CERT_FIND_EXT_ONLY_ENHKEY_USAGE_FLAG = 0x2;
public static readonly nuint CERT_FIND_PROP_ONLY_ENHKEY_USAGE_FLAG = 0x4;
public static readonly nuint CERT_FIND_NO_ENHKEY_USAGE_FLAG = 0x8;
public static readonly nuint CERT_FIND_OR_ENHKEY_USAGE_FLAG = 0x10;
public static readonly nuint CERT_FIND_VALID_ENHKEY_USAGE_FLAG = 0x20;
public static readonly var CERT_FIND_OPTIONAL_CTL_USAGE_FLAG = CERT_FIND_OPTIONAL_ENHKEY_USAGE_FLAG;
public static readonly var CERT_FIND_EXT_ONLY_CTL_USAGE_FLAG = CERT_FIND_EXT_ONLY_ENHKEY_USAGE_FLAG;
public static readonly var CERT_FIND_PROP_ONLY_CTL_USAGE_FLAG = CERT_FIND_PROP_ONLY_ENHKEY_USAGE_FLAG;
public static readonly var CERT_FIND_NO_CTL_USAGE_FLAG = CERT_FIND_NO_ENHKEY_USAGE_FLAG;
public static readonly var CERT_FIND_OR_CTL_USAGE_FLAG = CERT_FIND_OR_ENHKEY_USAGE_FLAG;
public static readonly var CERT_FIND_VALID_CTL_USAGE_FLAG = CERT_FIND_VALID_ENHKEY_USAGE_FLAG; 

/* policyOID values for CertVerifyCertificateChainPolicy function */
public static readonly nint CERT_CHAIN_POLICY_BASE = 1;
public static readonly nint CERT_CHAIN_POLICY_AUTHENTICODE = 2;
public static readonly nint CERT_CHAIN_POLICY_AUTHENTICODE_TS = 3;
public static readonly nint CERT_CHAIN_POLICY_SSL = 4;
public static readonly nint CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5;
public static readonly nint CERT_CHAIN_POLICY_NT_AUTH = 6;
public static readonly nint CERT_CHAIN_POLICY_MICROSOFT_ROOT = 7;
public static readonly nint CERT_CHAIN_POLICY_EV = 8;
public static readonly nint CERT_CHAIN_POLICY_SSL_F12 = 9; 

/* flag for dwFindType CertFindChainInStore  */
public static readonly nint CERT_CHAIN_FIND_BY_ISSUER = 1; 

/* dwFindFlags for CertFindChainInStore when dwFindType == CERT_CHAIN_FIND_BY_ISSUER */
public static readonly nuint CERT_CHAIN_FIND_BY_ISSUER_COMPARE_KEY_FLAG = 0x0001;
public static readonly nuint CERT_CHAIN_FIND_BY_ISSUER_COMPLEX_CHAIN_FLAG = 0x0002;
public static readonly nuint CERT_CHAIN_FIND_BY_ISSUER_CACHE_ONLY_URL_FLAG = 0x0004;
public static readonly nuint CERT_CHAIN_FIND_BY_ISSUER_LOCAL_MACHINE_FLAG = 0x0008;
public static readonly nuint CERT_CHAIN_FIND_BY_ISSUER_NO_KEY_FLAG = 0x4000;
public static readonly nuint CERT_CHAIN_FIND_BY_ISSUER_CACHE_ONLY_FLAG = 0x8000; 

/* Certificate Store close flags */
public static readonly nuint CERT_CLOSE_STORE_FORCE_FLAG = 0x00000001;
public static readonly nuint CERT_CLOSE_STORE_CHECK_FLAG = 0x00000002; 

/* CryptQueryObject object type */
public static readonly nint CERT_QUERY_OBJECT_FILE = 1;
public static readonly nint CERT_QUERY_OBJECT_BLOB = 2; 

/* CryptQueryObject content type flags */
public static readonly nint CERT_QUERY_CONTENT_CERT = 1;
public static readonly nint CERT_QUERY_CONTENT_CTL = 2;
public static readonly nint CERT_QUERY_CONTENT_CRL = 3;
public static readonly nint CERT_QUERY_CONTENT_SERIALIZED_STORE = 4;
public static readonly nint CERT_QUERY_CONTENT_SERIALIZED_CERT = 5;
public static readonly nint CERT_QUERY_CONTENT_SERIALIZED_CTL = 6;
public static readonly nint CERT_QUERY_CONTENT_SERIALIZED_CRL = 7;
public static readonly nint CERT_QUERY_CONTENT_PKCS7_SIGNED = 8;
public static readonly nint CERT_QUERY_CONTENT_PKCS7_UNSIGNED = 9;
public static readonly nint CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED = 10;
public static readonly nint CERT_QUERY_CONTENT_PKCS10 = 11;
public static readonly nint CERT_QUERY_CONTENT_PFX = 12;
public static readonly nint CERT_QUERY_CONTENT_CERT_PAIR = 13;
public static readonly nint CERT_QUERY_CONTENT_PFX_AND_LOAD = 14;
public static readonly nint CERT_QUERY_CONTENT_FLAG_CERT = (1 << (int)(CERT_QUERY_CONTENT_CERT));
public static readonly nint CERT_QUERY_CONTENT_FLAG_CTL = (1 << (int)(CERT_QUERY_CONTENT_CTL));
public static readonly nint CERT_QUERY_CONTENT_FLAG_CRL = (1 << (int)(CERT_QUERY_CONTENT_CRL));
public static readonly nint CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE = (1 << (int)(CERT_QUERY_CONTENT_SERIALIZED_STORE));
public static readonly nint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT = (1 << (int)(CERT_QUERY_CONTENT_SERIALIZED_CERT));
public static readonly nint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL = (1 << (int)(CERT_QUERY_CONTENT_SERIALIZED_CTL));
public static readonly nint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL = (1 << (int)(CERT_QUERY_CONTENT_SERIALIZED_CRL));
public static readonly nint CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED = (1 << (int)(CERT_QUERY_CONTENT_PKCS7_SIGNED));
public static readonly nint CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED = (1 << (int)(CERT_QUERY_CONTENT_PKCS7_UNSIGNED));
public static readonly nint CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED = (1 << (int)(CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED));
public static readonly nint CERT_QUERY_CONTENT_FLAG_PKCS10 = (1 << (int)(CERT_QUERY_CONTENT_PKCS10));
public static readonly nint CERT_QUERY_CONTENT_FLAG_PFX = (1 << (int)(CERT_QUERY_CONTENT_PFX));
public static readonly nint CERT_QUERY_CONTENT_FLAG_CERT_PAIR = (1 << (int)(CERT_QUERY_CONTENT_CERT_PAIR));
public static readonly nint CERT_QUERY_CONTENT_FLAG_PFX_AND_LOAD = (1 << (int)(CERT_QUERY_CONTENT_PFX_AND_LOAD));
public static readonly var CERT_QUERY_CONTENT_FLAG_ALL = (CERT_QUERY_CONTENT_FLAG_CERT | CERT_QUERY_CONTENT_FLAG_CTL | CERT_QUERY_CONTENT_FLAG_CRL | CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL | CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED | CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED | CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED | CERT_QUERY_CONTENT_FLAG_PKCS10 | CERT_QUERY_CONTENT_FLAG_PFX | CERT_QUERY_CONTENT_FLAG_CERT_PAIR);
public static readonly var CERT_QUERY_CONTENT_FLAG_ALL_ISSUER_CERT = (CERT_QUERY_CONTENT_FLAG_CERT | CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE | CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT | CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED | CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED); 

/* CryptQueryObject format type flags */
public static readonly nint CERT_QUERY_FORMAT_BINARY = 1;
public static readonly nint CERT_QUERY_FORMAT_BASE64_ENCODED = 2;
public static readonly nint CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3;
public static readonly nint CERT_QUERY_FORMAT_FLAG_BINARY = (1 << (int)(CERT_QUERY_FORMAT_BINARY));
public static readonly nint CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED = (1 << (int)(CERT_QUERY_FORMAT_BASE64_ENCODED));
public static readonly nint CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED = (1 << (int)(CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED));
public static readonly var CERT_QUERY_FORMAT_FLAG_ALL = (CERT_QUERY_FORMAT_FLAG_BINARY | CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED | CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED); 

/* CertGetNameString name types */
public static readonly nint CERT_NAME_EMAIL_TYPE = 1;
public static readonly nint CERT_NAME_RDN_TYPE = 2;
public static readonly nint CERT_NAME_ATTR_TYPE = 3;
public static readonly nint CERT_NAME_SIMPLE_DISPLAY_TYPE = 4;
public static readonly nint CERT_NAME_FRIENDLY_DISPLAY_TYPE = 5;
public static readonly nint CERT_NAME_DNS_TYPE = 6;
public static readonly nint CERT_NAME_URL_TYPE = 7;
public static readonly nint CERT_NAME_UPN_TYPE = 8; 

/* CertGetNameString flags */
public static readonly nuint CERT_NAME_ISSUER_FLAG = 0x1;
public static readonly nuint CERT_NAME_DISABLE_IE4_UTF8_FLAG = 0x10000;
public static readonly nuint CERT_NAME_SEARCH_ALL_NAMES_FLAG = 0x2;
public static readonly nuint CERT_NAME_STR_ENABLE_PUNYCODE_FLAG = 0x00200000; 

/* AuthType values for SSLExtraCertChainPolicyPara struct */
public static readonly nint AUTHTYPE_CLIENT = 1;
public static readonly nint AUTHTYPE_SERVER = 2; 

/* Checks values for SSLExtraCertChainPolicyPara struct */
public static readonly nuint SECURITY_FLAG_IGNORE_REVOCATION = 0x00000080;
public static readonly nuint SECURITY_FLAG_IGNORE_UNKNOWN_CA = 0x00000100;
public static readonly nuint SECURITY_FLAG_IGNORE_WRONG_USAGE = 0x00000200;
public static readonly nuint SECURITY_FLAG_IGNORE_CERT_CN_INVALID = 0x00001000;
public static readonly nuint SECURITY_FLAG_IGNORE_CERT_DATE_INVALID = 0x00002000; 

/* Flags for Crypt[Un]ProtectData */
public static readonly nuint CRYPTPROTECT_UI_FORBIDDEN = 0x1;
public static readonly nuint CRYPTPROTECT_LOCAL_MACHINE = 0x4;
public static readonly nuint CRYPTPROTECT_CRED_SYNC = 0x8;
public static readonly nuint CRYPTPROTECT_AUDIT = 0x10;
public static readonly nuint CRYPTPROTECT_NO_RECOVERY = 0x20;
public static readonly nuint CRYPTPROTECT_VERIFY_PROTECTION = 0x40;
public static readonly nuint CRYPTPROTECT_CRED_REGENERATE = 0x80; 

/* Flags for CryptProtectPromptStruct */
public static readonly nint CRYPTPROTECT_PROMPT_ON_UNPROTECT = 1;
public static readonly nint CRYPTPROTECT_PROMPT_ON_PROTECT = 2;
public static readonly nint CRYPTPROTECT_PROMPT_RESERVED = 4;
public static readonly nint CRYPTPROTECT_PROMPT_STRONG = 8;
public static readonly nint CRYPTPROTECT_PROMPT_REQUIRE_STRONG = 16;


 
// flags for SetErrorMode
public static readonly nuint SEM_FAILCRITICALERRORS = 0x0001;
public static readonly nuint SEM_NOALIGNMENTFAULTEXCEPT = 0x0004;
public static readonly nuint SEM_NOGPFAULTERRORBOX = 0x0002;
public static readonly nuint SEM_NOOPENFILEERRORBOX = 0x8000;


 
// Priority class.
public static readonly nuint ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000;
public static readonly nuint BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
public static readonly nuint HIGH_PRIORITY_CLASS = 0x00000080;
public static readonly nuint IDLE_PRIORITY_CLASS = 0x00000040;
public static readonly nuint NORMAL_PRIORITY_CLASS = 0x00000020;
public static readonly nuint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;
public static readonly nuint PROCESS_MODE_BACKGROUND_END = 0x00200000;
public static readonly nuint REALTIME_PRIORITY_CLASS = 0x00000100;


/* wintrust.h constants for WinVerifyTrustEx */
public static readonly nint WTD_UI_ALL = 1;
public static readonly nint WTD_UI_NONE = 2;
public static readonly nint WTD_UI_NOBAD = 3;
public static readonly nint WTD_UI_NOGOOD = 4;

public static readonly nint WTD_REVOKE_NONE = 0;
public static readonly nint WTD_REVOKE_WHOLECHAIN = 1;

public static readonly nint WTD_CHOICE_FILE = 1;
public static readonly nint WTD_CHOICE_CATALOG = 2;
public static readonly nint WTD_CHOICE_BLOB = 3;
public static readonly nint WTD_CHOICE_SIGNER = 4;
public static readonly nint WTD_CHOICE_CERT = 5;

public static readonly nuint WTD_STATEACTION_IGNORE = 0x00000000;
public static readonly nuint WTD_STATEACTION_VERIFY = 0x00000010;
public static readonly nuint WTD_STATEACTION_CLOSE = 0x00000002;
public static readonly nuint WTD_STATEACTION_AUTO_CACHE = 0x00000003;
public static readonly nuint WTD_STATEACTION_AUTO_CACHE_FLUSH = 0x00000004;

public static readonly nuint WTD_USE_IE4_TRUST_FLAG = 0x1;
public static readonly nuint WTD_NO_IE4_CHAIN_FLAG = 0x2;
public static readonly nuint WTD_NO_POLICY_USAGE_FLAG = 0x4;
public static readonly nuint WTD_REVOCATION_CHECK_NONE = 0x10;
public static readonly nuint WTD_REVOCATION_CHECK_END_CERT = 0x20;
public static readonly nuint WTD_REVOCATION_CHECK_CHAIN = 0x40;
public static readonly nuint WTD_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x80;
public static readonly nuint WTD_SAFER_FLAG = 0x100;
public static readonly nuint WTD_HASH_ONLY_FLAG = 0x200;
public static readonly nuint WTD_USE_DEFAULT_OSVER_CHECK = 0x400;
public static readonly nuint WTD_LIFETIME_SIGNING_FLAG = 0x800;
public static readonly nuint WTD_CACHE_ONLY_URL_RETRIEVAL = 0x1000;
public static readonly nuint WTD_DISABLE_MD2_MD4 = 0x2000;
public static readonly nuint WTD_MOTW = 0x4000;

public static readonly nint WTD_UICONTEXT_EXECUTE = 0;
public static readonly nint WTD_UICONTEXT_INSTALL = 1;


public static slice<byte> OID_PKIX_KP_SERVER_AUTH = (slice<byte>)"1.3.6.1.5.5.7.3.1\x00";public static slice<byte> OID_SERVER_GATED_CRYPTO = (slice<byte>)"1.3.6.1.4.1.311.10.3.3\x00";public static slice<byte> OID_SGC_NETSCAPE = (slice<byte>)"2.16.840.1.113730.4.1\x00";public static GUID WINTRUST_ACTION_GENERIC_VERIFY_V2 = new GUID(Data1:0xaac56b,Data2:0xcd44,Data3:0x11d0,Data4:[8]byte{0x8c,0xc2,0x0,0xc0,0x4f,0xc2,0x95,0xee},);

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
// ¹ https://docs.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-win32_find_dataw describe
// ² https://golang.org/issue/42637#issuecomment-760715755.
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

public partial struct StartupInfoEx {
    public ref StartupInfo StartupInfo => ref StartupInfo_val;
    public ptr<ProcThreadAttributeList> ProcThreadAttributeList;
}

// ProcThreadAttributeList is a placeholder type to represent a PROC_THREAD_ATTRIBUTE_LIST.
//
// To create a *ProcThreadAttributeList, use NewProcThreadAttributeList, and
// free its memory using ProcThreadAttributeList.Delete.
public partial struct ProcThreadAttributeList {
    public array<unsafe.Pointer> _;
}

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

public partial struct ThreadEntry32 {
    public uint Size;
    public uint Usage;
    public uint ThreadID;
    public uint OwnerProcessID;
    public int BasePri;
    public int DeltaPri;
    public uint Flags;
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
public static readonly nint AF_NETBIOS = 17;
public static readonly nint AF_INET6 = 23;
public static readonly nint AF_IRDA = 26;
public static readonly nint AF_BTH = 32;

public static readonly nint SOCK_STREAM = 1;
public static readonly nint SOCK_DGRAM = 2;
public static readonly nint SOCK_RAW = 3;
public static readonly nint SOCK_RDM = 4;
public static readonly nint SOCK_SEQPACKET = 5;

public static readonly nint IPPROTO_IP = 0;
public static readonly nint IPPROTO_ICMP = 1;
public static readonly nint IPPROTO_IGMP = 2;
public static readonly nint BTHPROTO_RFCOMM = 3;
public static readonly nint IPPROTO_TCP = 6;
public static readonly nint IPPROTO_UDP = 17;
public static readonly nint IPPROTO_IPV6 = 41;
public static readonly nint IPPROTO_ICMPV6 = 58;
public static readonly nint IPPROTO_RM = 113;

public static readonly nuint SOL_SOCKET = 0xffff;
public static readonly nint SO_REUSEADDR = 4;
public static readonly nint SO_KEEPALIVE = 8;
public static readonly nint SO_DONTROUTE = 16;
public static readonly nint SO_BROADCAST = 32;
public static readonly nint SO_LINGER = 128;
public static readonly nuint SO_RCVBUF = 0x1002;
public static readonly nuint SO_RCVTIMEO = 0x1006;
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

// cf. http://support.microsoft.com/default.aspx?scid=kb;en-us;257460

public static readonly nuint IP_HDRINCL = 0x2;
public static readonly nuint IP_TOS = 0x3;
public static readonly nuint IP_TTL = 0x4;
public static readonly nuint IP_MULTICAST_IF = 0x9;
public static readonly nuint IP_MULTICAST_TTL = 0xa;
public static readonly nuint IP_MULTICAST_LOOP = 0xb;
public static readonly nuint IP_ADD_MEMBERSHIP = 0xc;
public static readonly nuint IP_DROP_MEMBERSHIP = 0xd;
public static readonly nuint IP_PKTINFO = 0x13;

public static readonly nuint IPV6_V6ONLY = 0x1b;
public static readonly nuint IPV6_UNICAST_HOPS = 0x4;
public static readonly nuint IPV6_MULTICAST_IF = 0x9;
public static readonly nuint IPV6_MULTICAST_HOPS = 0xa;
public static readonly nuint IPV6_MULTICAST_LOOP = 0xb;
public static readonly nuint IPV6_JOIN_GROUP = 0xc;
public static readonly nuint IPV6_LEAVE_GROUP = 0xd;
public static readonly nuint IPV6_PKTINFO = 0x13;

public static readonly nuint MSG_OOB = 0x1;
public static readonly nuint MSG_PEEK = 0x2;
public static readonly nuint MSG_DONTROUTE = 0x4;
public static readonly nuint MSG_WAITALL = 0x8;

public static readonly nuint MSG_TRUNC = 0x0100;
public static readonly nuint MSG_CTRUNC = 0x0200;
public static readonly nuint MSG_BCAST = 0x0400;
public static readonly nuint MSG_MCAST = 0x0800;

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

public partial struct WSAMsg {
    public ptr<syscall.RawSockaddrAny> Name;
    public int Namelen;
    public ptr<WSABuf> Buffers;
    public uint BufferCount;
    public WSABuf Control;
    public uint Flags;
}

// Flags for WSASocket
public static readonly nuint WSA_FLAG_OVERLAPPED = 0x01;
public static readonly nuint WSA_FLAG_MULTIPOINT_C_ROOT = 0x02;
public static readonly nuint WSA_FLAG_MULTIPOINT_C_LEAF = 0x04;
public static readonly nuint WSA_FLAG_MULTIPOINT_D_ROOT = 0x08;
public static readonly nuint WSA_FLAG_MULTIPOINT_D_LEAF = 0x10;
public static readonly nuint WSA_FLAG_ACCESS_SYSTEM_SECURITY = 0x40;
public static readonly nuint WSA_FLAG_NO_HANDLE_INHERIT = 0x80;
public static readonly nuint WSA_FLAG_REGISTERED_IO = 0x100;


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
    public uint Version;
    public CryptIntegerBlob SerialNumber;
    public CryptAlgorithmIdentifier SignatureAlgorithm;
    public CertNameBlob Issuer;
    public Filetime NotBefore;
    public Filetime NotAfter;
    public CertNameBlob Subject;
    public CertPublicKeyInfo SubjectPublicKeyInfo;
    public CryptBitBlob IssuerUniqueId;
    public CryptBitBlob SubjectUniqueId;
    public uint CountExtensions;
    public ptr<CertExtension> Extensions;
}

public partial struct CertExtension {
    public ptr<byte> ObjId;
    public int Critical;
    public CryptObjidBlob Value;
}

public partial struct CryptAlgorithmIdentifier {
    public ptr<byte> ObjId;
    public CryptObjidBlob Parameters;
}

public partial struct CertPublicKeyInfo {
    public CryptAlgorithmIdentifier Algorithm;
    public CryptBitBlob PublicKey;
}

public partial struct DataBlob {
    public uint Size;
    public ptr<byte> Data;
}
public partial struct CryptIntegerBlob { // : DataBlob
}
public partial struct CryptUintBlob { // : DataBlob
}
public partial struct CryptObjidBlob { // : DataBlob
}
public partial struct CertNameBlob { // : DataBlob
}
public partial struct CertRdnValueBlob { // : DataBlob
}
public partial struct CertBlob { // : DataBlob
}
public partial struct CrlBlob { // : DataBlob
}
public partial struct CryptDataBlob { // : DataBlob
}
public partial struct CryptHashBlob { // : DataBlob
}
public partial struct CryptDigestBlob { // : DataBlob
}
public partial struct CryptDerBlob { // : DataBlob
}
public partial struct CryptAttrBlob { // : DataBlob
}

public partial struct CryptBitBlob {
    public uint Size;
    public ptr<byte> Data;
    public uint UnusedBits;
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

public partial struct CertPolicyInfo {
    public ptr<byte> Identifier;
    public uint CountQualifiers;
    public ptr<CertPolicyQualifierInfo> Qualifiers;
}

public partial struct CertPoliciesInfo {
    public uint Count;
    public ptr<CertPolicyInfo> PolicyInfos;
}

public partial struct CertPolicyQualifierInfo {
}

public partial struct CertStrongSignPara {
    public uint Size;
    public uint InfoChoice;
    public unsafe.Pointer InfoOrSerializedInfoOrOID;
}

public partial struct CryptProtectPromptStruct {
    public uint Size;
    public uint PromptFlags;
    public HWND App;
    public ptr<ushort> Prompt;
}

public partial struct CertChainFindByIssuerPara {
    public uint Size;
    public ptr<byte> UsageIdentifier;
    public uint KeySpec;
    public uint AcquirePrivateKeyFlags;
    public uint IssuerCount;
    public Pointer Issuer;
    public Pointer FindCallback;
    public Pointer FindArg;
    public ptr<uint> IssuerChainIndex;
    public ptr<uint> IssuerElementIndex;
}

public partial struct WinTrustData {
    public uint Size;
    public System.UIntPtr PolicyCallbackData;
    public System.UIntPtr SIPClientData;
    public uint UIChoice;
    public uint RevocationChecks;
    public uint UnionChoice;
    public unsafe.Pointer FileOrCatalogOrBlobOrSgnrOrCert;
    public uint StateAction;
    public Handle StateData;
    public ptr<ushort> URLReference;
    public uint ProvFlags;
    public uint UIContext;
    public ptr<WinTrustSignatureSettings> SignatureSettings;
}

public partial struct WinTrustFileInfo {
    public uint Size;
    public ptr<ushort> FilePath;
    public Handle File;
    public ptr<GUID> KnownSubject;
}

public partial struct WinTrustSignatureSettings {
    public uint Size;
    public uint Index;
    public uint Flags;
    public uint SecondarySigs;
    public uint VerifiedSigIndex;
    public ptr<CertStrongSignPara> CryptoPolicy;
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


public static readonly nuint EVENT_MODIFY_STATE = 0x0002;
public static readonly var EVENT_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3;

public static readonly nuint MUTANT_QUERY_STATE = 0x0001;
public static readonly var MUTANT_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | MUTANT_QUERY_STATE;

public static readonly nuint SEMAPHORE_MODIFY_STATE = 0x0002;
public static readonly var SEMAPHORE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3;

public static readonly nuint TIMER_QUERY_STATE = 0x0001;
public static readonly nuint TIMER_MODIFY_STATE = 0x0002;
public static readonly var TIMER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | TIMER_QUERY_STATE | TIMER_MODIFY_STATE;

public static readonly var MUTEX_MODIFY_STATE = MUTANT_QUERY_STATE;
public static readonly var MUTEX_ALL_ACCESS = MUTANT_ALL_ACCESS;

public static readonly nuint CREATE_EVENT_MANUAL_RESET = 0x1;
public static readonly nuint CREATE_EVENT_INITIAL_SET = 0x2;
public static readonly nuint CREATE_MUTEX_INITIAL_OWNER = 0x1;


public partial struct AddrinfoW {
    public int Flags;
    public int Family;
    public int Socktype;
    public int Protocol;
    public System.UIntPtr Addrlen;
    public ptr<ushort> Canonname;
    public System.UIntPtr Addr;
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

public static GUID WSAID_WSASENDMSG = new GUID(0xa441e712,0x754f,0x43ca,[8]byte{0x84,0xa7,0x0d,0xee,0x44,0xcf,0x60,0x6d},);

public static GUID WSAID_WSARECVMSG = new GUID(0xf689d7c8,0x6f1f,0x436b,[8]byte{0x8a,0x53,0xe5,0x4f,0xe3,0x51,0xc3,0x22},);

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
public static readonly nuint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
public static readonly nuint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
public static readonly nuint SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;


public static readonly nint ComputerNameNetBIOS = 0;
public static readonly nint ComputerNameDnsHostname = 1;
public static readonly nint ComputerNameDnsDomain = 2;
public static readonly nint ComputerNameDnsFullyQualified = 3;
public static readonly nint ComputerNamePhysicalNetBIOS = 4;
public static readonly nint ComputerNamePhysicalDnsHostname = 5;
public static readonly nint ComputerNamePhysicalDnsDomain = 6;
public static readonly nint ComputerNamePhysicalDnsFullyQualified = 7;
public static readonly nint ComputerNameMax = 8;


// For MessageBox()
public static readonly nuint MB_OK = 0x00000000;
public static readonly nuint MB_OKCANCEL = 0x00000001;
public static readonly nuint MB_ABORTRETRYIGNORE = 0x00000002;
public static readonly nuint MB_YESNOCANCEL = 0x00000003;
public static readonly nuint MB_YESNO = 0x00000004;
public static readonly nuint MB_RETRYCANCEL = 0x00000005;
public static readonly nuint MB_CANCELTRYCONTINUE = 0x00000006;
public static readonly nuint MB_ICONHAND = 0x00000010;
public static readonly nuint MB_ICONQUESTION = 0x00000020;
public static readonly nuint MB_ICONEXCLAMATION = 0x00000030;
public static readonly nuint MB_ICONASTERISK = 0x00000040;
public static readonly nuint MB_USERICON = 0x00000080;
public static readonly var MB_ICONWARNING = MB_ICONEXCLAMATION;
public static readonly var MB_ICONERROR = MB_ICONHAND;
public static readonly var MB_ICONINFORMATION = MB_ICONASTERISK;
public static readonly var MB_ICONSTOP = MB_ICONHAND;
public static readonly nuint MB_DEFBUTTON1 = 0x00000000;
public static readonly nuint MB_DEFBUTTON2 = 0x00000100;
public static readonly nuint MB_DEFBUTTON3 = 0x00000200;
public static readonly nuint MB_DEFBUTTON4 = 0x00000300;
public static readonly nuint MB_APPLMODAL = 0x00000000;
public static readonly nuint MB_SYSTEMMODAL = 0x00001000;
public static readonly nuint MB_TASKMODAL = 0x00002000;
public static readonly nuint MB_HELP = 0x00004000;
public static readonly nuint MB_NOFOCUS = 0x00008000;
public static readonly nuint MB_SETFOREGROUND = 0x00010000;
public static readonly nuint MB_DEFAULT_DESKTOP_ONLY = 0x00020000;
public static readonly nuint MB_TOPMOST = 0x00040000;
public static readonly nuint MB_RIGHT = 0x00080000;
public static readonly nuint MB_RTLREADING = 0x00100000;
public static readonly nuint MB_SERVICE_NOTIFICATION = 0x00200000;


public static readonly nuint MOVEFILE_REPLACE_EXISTING = 0x1;
public static readonly nuint MOVEFILE_COPY_ALLOWED = 0x2;
public static readonly nuint MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;
public static readonly nuint MOVEFILE_WRITE_THROUGH = 0x8;
public static readonly nuint MOVEFILE_CREATE_HARDLINK = 0x10;
public static readonly nuint MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x20;


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

// IP returns an IPv4 or IPv6 address, or nil if the underlying SocketAddress is neither.
private static net.IP IP(this ptr<SocketAddress> _addr_addr) {
    ref SocketAddress addr = ref _addr_addr.val;

    if (uintptr(addr.SockaddrLength) >= @unsafe.Sizeof(new RawSockaddrInet4()) && addr.Sockaddr.Addr.Family == AF_INET) {
        return (RawSockaddrInet4.val)(@unsafe.Pointer(addr.Sockaddr)).Addr[..];
    }
    else if (uintptr(addr.SockaddrLength) >= @unsafe.Sizeof(new RawSockaddrInet6()) && addr.Sockaddr.Addr.Family == AF_INET6) {
        return (RawSockaddrInet6.val)(@unsafe.Pointer(addr.Sockaddr)).Addr[..];
    }
    return null;

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

public static readonly nint IfOperStatusUp = 1;
public static readonly nint IfOperStatusDown = 2;
public static readonly nint IfOperStatusTesting = 3;
public static readonly nint IfOperStatusUnknown = 4;
public static readonly nint IfOperStatusDormant = 5;
public static readonly nint IfOperStatusNotPresent = 6;
public static readonly nint IfOperStatusLowerLayerDown = 7;


// Console related constants used for the mode parameter to SetConsoleMode. See
// https://docs.microsoft.com/en-us/windows/console/setconsolemode for details.

public static readonly nuint ENABLE_PROCESSED_INPUT = 0x1;
public static readonly nuint ENABLE_LINE_INPUT = 0x2;
public static readonly nuint ENABLE_ECHO_INPUT = 0x4;
public static readonly nuint ENABLE_WINDOW_INPUT = 0x8;
public static readonly nuint ENABLE_MOUSE_INPUT = 0x10;
public static readonly nuint ENABLE_INSERT_MODE = 0x20;
public static readonly nuint ENABLE_QUICK_EDIT_MODE = 0x40;
public static readonly nuint ENABLE_EXTENDED_FLAGS = 0x80;
public static readonly nuint ENABLE_AUTO_POSITION = 0x100;
public static readonly nuint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x200;

public static readonly nuint ENABLE_PROCESSED_OUTPUT = 0x1;
public static readonly nuint ENABLE_WRAP_AT_EOL_OUTPUT = 0x2;
public static readonly nuint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x4;
public static readonly nuint DISABLE_NEWLINE_AUTO_RETURN = 0x8;
public static readonly nuint ENABLE_LVB_GRID_WORLDWIDE = 0x10;


public partial struct Coord {
    public short X;
    public short Y;
}

public partial struct SmallRect {
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}

// Used with GetConsoleScreenBuffer to retrieve information about a console
// screen buffer. See
// https://docs.microsoft.com/en-us/windows/console/console-screen-buffer-info-str
// for details.

public partial struct ConsoleScreenBufferInfo {
    public Coord Size;
    public Coord CursorPosition;
    public ushort Attributes;
    public SmallRect Window;
    public Coord MaximumWindowSize;
}

public static readonly nint UNIX_PATH_MAX = 108; // defined in afunix.h

 // defined in afunix.h

 
// flags for JOBOBJECT_BASIC_LIMIT_INFORMATION.LimitFlags
public static readonly nuint JOB_OBJECT_LIMIT_ACTIVE_PROCESS = 0x00000008;
public static readonly nuint JOB_OBJECT_LIMIT_AFFINITY = 0x00000010;
public static readonly nuint JOB_OBJECT_LIMIT_BREAKAWAY_OK = 0x00000800;
public static readonly nuint JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION = 0x00000400;
public static readonly nuint JOB_OBJECT_LIMIT_JOB_MEMORY = 0x00000200;
public static readonly nuint JOB_OBJECT_LIMIT_JOB_TIME = 0x00000004;
public static readonly nuint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;
public static readonly nuint JOB_OBJECT_LIMIT_PRESERVE_JOB_TIME = 0x00000040;
public static readonly nuint JOB_OBJECT_LIMIT_PRIORITY_CLASS = 0x00000020;
public static readonly nuint JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x00000100;
public static readonly nuint JOB_OBJECT_LIMIT_PROCESS_TIME = 0x00000002;
public static readonly nuint JOB_OBJECT_LIMIT_SCHEDULING_CLASS = 0x00000080;
public static readonly nuint JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK = 0x00001000;
public static readonly nuint JOB_OBJECT_LIMIT_SUBSET_AFFINITY = 0x00004000;
public static readonly nuint JOB_OBJECT_LIMIT_WORKINGSET = 0x00000001;


public partial struct IO_COUNTERS {
    public ulong ReadOperationCount;
    public ulong WriteOperationCount;
    public ulong OtherOperationCount;
    public ulong ReadTransferCount;
    public ulong WriteTransferCount;
    public ulong OtherTransferCount;
}

public partial struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
    public IO_COUNTERS IoInfo;
    public System.UIntPtr ProcessMemoryLimit;
    public System.UIntPtr JobMemoryLimit;
    public System.UIntPtr PeakProcessMemoryUsed;
    public System.UIntPtr PeakJobMemoryUsed;
}

 
// UIRestrictionsClass
public static readonly nuint JOB_OBJECT_UILIMIT_DESKTOP = 0x00000040;
public static readonly nuint JOB_OBJECT_UILIMIT_DISPLAYSETTINGS = 0x00000010;
public static readonly nuint JOB_OBJECT_UILIMIT_EXITWINDOWS = 0x00000080;
public static readonly nuint JOB_OBJECT_UILIMIT_GLOBALATOMS = 0x00000020;
public static readonly nuint JOB_OBJECT_UILIMIT_HANDLES = 0x00000001;
public static readonly nuint JOB_OBJECT_UILIMIT_READCLIPBOARD = 0x00000002;
public static readonly nuint JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS = 0x00000008;
public static readonly nuint JOB_OBJECT_UILIMIT_WRITECLIPBOARD = 0x00000004;


public partial struct JOBOBJECT_BASIC_UI_RESTRICTIONS {
    public uint UIRestrictionsClass;
}

 
// JobObjectInformationClass
public static readonly nint JobObjectAssociateCompletionPortInformation = 7;
public static readonly nint JobObjectBasicLimitInformation = 2;
public static readonly nint JobObjectBasicUIRestrictions = 4;
public static readonly nint JobObjectCpuRateControlInformation = 15;
public static readonly nint JobObjectEndOfJobTimeInformation = 6;
public static readonly nint JobObjectExtendedLimitInformation = 9;
public static readonly nint JobObjectGroupInformation = 11;
public static readonly nint JobObjectGroupInformationEx = 14;
public static readonly nint JobObjectLimitViolationInformation2 = 35;
public static readonly nint JobObjectNetRateControlInformation = 32;
public static readonly nint JobObjectNotificationLimitInformation = 12;
public static readonly nint JobObjectNotificationLimitInformation2 = 34;
public static readonly nint JobObjectSecurityLimitInformation = 5;


public static readonly nuint KF_FLAG_DEFAULT = 0x00000000;
public static readonly nuint KF_FLAG_FORCE_APP_DATA_REDIRECTION = 0x00080000;
public static readonly nuint KF_FLAG_RETURN_FILTER_REDIRECTION_TARGET = 0x00040000;
public static readonly nuint KF_FLAG_FORCE_PACKAGE_REDIRECTION = 0x00020000;
public static readonly nuint KF_FLAG_NO_PACKAGE_REDIRECTION = 0x00010000;
public static readonly nuint KF_FLAG_FORCE_APPCONTAINER_REDIRECTION = 0x00020000;
public static readonly nuint KF_FLAG_NO_APPCONTAINER_REDIRECTION = 0x00010000;
public static readonly nuint KF_FLAG_CREATE = 0x00008000;
public static readonly nuint KF_FLAG_DONT_VERIFY = 0x00004000;
public static readonly nuint KF_FLAG_DONT_UNEXPAND = 0x00002000;
public static readonly nuint KF_FLAG_NO_ALIAS = 0x00001000;
public static readonly nuint KF_FLAG_INIT = 0x00000800;
public static readonly nuint KF_FLAG_DEFAULT_PATH = 0x00000400;
public static readonly nuint KF_FLAG_NOT_PARENT_RELATIVE = 0x00000200;
public static readonly nuint KF_FLAG_SIMPLE_IDLIST = 0x00000100;
public static readonly nuint KF_FLAG_ALIAS_ONLY = 0x80000000;


public partial struct OsVersionInfoEx {
    public uint osVersionInfoSize;
    public uint MajorVersion;
    public uint MinorVersion;
    public uint BuildNumber;
    public uint PlatformId;
    public array<ushort> CsdVersion;
    public ushort ServicePackMajor;
    public ushort ServicePackMinor;
    public ushort SuiteMask;
    public byte ProductType;
    public byte _;
}

public static readonly nuint EWX_LOGOFF = 0x00000000;
public static readonly nuint EWX_SHUTDOWN = 0x00000001;
public static readonly nuint EWX_REBOOT = 0x00000002;
public static readonly nuint EWX_FORCE = 0x00000004;
public static readonly nuint EWX_POWEROFF = 0x00000008;
public static readonly nuint EWX_FORCEIFHUNG = 0x00000010;
public static readonly nuint EWX_QUICKRESOLVE = 0x00000020;
public static readonly nuint EWX_RESTARTAPPS = 0x00000040;
public static readonly nuint EWX_HYBRID_SHUTDOWN = 0x00400000;
public static readonly nuint EWX_BOOTOPTIONS = 0x01000000;

public static readonly nuint SHTDN_REASON_FLAG_COMMENT_REQUIRED = 0x01000000;
public static readonly nuint SHTDN_REASON_FLAG_DIRTY_PROBLEM_ID_REQUIRED = 0x02000000;
public static readonly nuint SHTDN_REASON_FLAG_CLEAN_UI = 0x04000000;
public static readonly nuint SHTDN_REASON_FLAG_DIRTY_UI = 0x08000000;
public static readonly nuint SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000;
public static readonly nuint SHTDN_REASON_FLAG_PLANNED = 0x80000000;
public static readonly nuint SHTDN_REASON_MAJOR_OTHER = 0x00000000;
public static readonly nuint SHTDN_REASON_MAJOR_NONE = 0x00000000;
public static readonly nuint SHTDN_REASON_MAJOR_HARDWARE = 0x00010000;
public static readonly nuint SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000;
public static readonly nuint SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000;
public static readonly nuint SHTDN_REASON_MAJOR_APPLICATION = 0x00040000;
public static readonly nuint SHTDN_REASON_MAJOR_SYSTEM = 0x00050000;
public static readonly nuint SHTDN_REASON_MAJOR_POWER = 0x00060000;
public static readonly nuint SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000;
public static readonly nuint SHTDN_REASON_MINOR_OTHER = 0x00000000;
public static readonly nuint SHTDN_REASON_MINOR_NONE = 0x000000ff;
public static readonly nuint SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001;
public static readonly nuint SHTDN_REASON_MINOR_INSTALLATION = 0x00000002;
public static readonly nuint SHTDN_REASON_MINOR_UPGRADE = 0x00000003;
public static readonly nuint SHTDN_REASON_MINOR_RECONFIG = 0x00000004;
public static readonly nuint SHTDN_REASON_MINOR_HUNG = 0x00000005;
public static readonly nuint SHTDN_REASON_MINOR_UNSTABLE = 0x00000006;
public static readonly nuint SHTDN_REASON_MINOR_DISK = 0x00000007;
public static readonly nuint SHTDN_REASON_MINOR_PROCESSOR = 0x00000008;
public static readonly nuint SHTDN_REASON_MINOR_NETWORKCARD = 0x00000009;
public static readonly nuint SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a;
public static readonly nuint SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b;
public static readonly nuint SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c;
public static readonly nuint SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d;
public static readonly nuint SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e;
public static readonly nuint SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F;
public static readonly nuint SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010;
public static readonly nuint SHTDN_REASON_MINOR_HOTFIX = 0x00000011;
public static readonly nuint SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012;
public static readonly nuint SHTDN_REASON_MINOR_SECURITY = 0x00000013;
public static readonly nuint SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014;
public static readonly nuint SHTDN_REASON_MINOR_WMI = 0x00000015;
public static readonly nuint SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016;
public static readonly nuint SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017;
public static readonly nuint SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018;
public static readonly nuint SHTDN_REASON_MINOR_MMC = 0x00000019;
public static readonly nuint SHTDN_REASON_MINOR_SYSTEMRESTORE = 0x0000001a;
public static readonly nuint SHTDN_REASON_MINOR_TERMSRV = 0x00000020;
public static readonly nuint SHTDN_REASON_MINOR_DC_PROMOTION = 0x00000021;
public static readonly nuint SHTDN_REASON_MINOR_DC_DEMOTION = 0x00000022;
public static readonly var SHTDN_REASON_UNKNOWN = SHTDN_REASON_MINOR_NONE;
public static readonly var SHTDN_REASON_LEGACY_API = SHTDN_REASON_MAJOR_LEGACY_API | SHTDN_REASON_FLAG_PLANNED;
public static readonly nuint SHTDN_REASON_VALID_BIT_MASK = 0xc0ffffff;

public static readonly nuint SHUTDOWN_NORETRY = 0x1;


// Flags used for GetModuleHandleEx
public static readonly nint GET_MODULE_HANDLE_EX_FLAG_PIN = 1;
public static readonly nint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 2;
public static readonly nint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 4;


// MUI function flag values
public static readonly nuint MUI_LANGUAGE_ID = 0x4;
public static readonly nuint MUI_LANGUAGE_NAME = 0x8;
public static readonly nuint MUI_MERGE_SYSTEM_FALLBACK = 0x10;
public static readonly nuint MUI_MERGE_USER_FALLBACK = 0x20;
public static readonly var MUI_UI_FALLBACK = MUI_MERGE_SYSTEM_FALLBACK | MUI_MERGE_USER_FALLBACK;
public static readonly nuint MUI_THREAD_LANGUAGES = 0x40;
public static readonly nuint MUI_CONSOLE_FILTER = 0x100;
public static readonly nuint MUI_COMPLEX_SCRIPT_FILTER = 0x200;
public static readonly nuint MUI_RESET_FILTERS = 0x001;
public static readonly nuint MUI_USER_PREFERRED_UI_LANGUAGES = 0x10;
public static readonly nuint MUI_USE_INSTALLED_LANGUAGES = 0x20;
public static readonly nuint MUI_USE_SEARCH_ALL_LANGUAGES = 0x40;
public static readonly nuint MUI_LANG_NEUTRAL_PE_FILE = 0x100;
public static readonly nuint MUI_NON_LANG_NEUTRAL_FILE = 0x200;
public static readonly nuint MUI_MACHINE_LANGUAGE_SETTINGS = 0x400;
public static readonly nuint MUI_FILETYPE_NOT_LANGUAGE_NEUTRAL = 0x001;
public static readonly nuint MUI_FILETYPE_LANGUAGE_NEUTRAL_MAIN = 0x002;
public static readonly nuint MUI_FILETYPE_LANGUAGE_NEUTRAL_MUI = 0x004;
public static readonly nuint MUI_QUERY_TYPE = 0x001;
public static readonly nuint MUI_QUERY_CHECKSUM = 0x002;
public static readonly nuint MUI_QUERY_LANGUAGE_NAME = 0x004;
public static readonly nuint MUI_QUERY_RESOURCE_TYPES = 0x008;
public static readonly nuint MUI_FILEINFO_VERSION = 0x001;

public static readonly nuint MUI_FULL_LANGUAGE = 0x01;
public static readonly nuint MUI_PARTIAL_LANGUAGE = 0x02;
public static readonly nuint MUI_LIP_LANGUAGE = 0x04;
public static readonly nuint MUI_LANGUAGE_INSTALLED = 0x20;
public static readonly nuint MUI_LANGUAGE_LICENSED = 0x40;


// FILE_INFO_BY_HANDLE_CLASS constants for SetFileInformationByHandle/GetFileInformationByHandleEx
public static readonly nint FileBasicInfo = 0;
public static readonly nint FileStandardInfo = 1;
public static readonly nint FileNameInfo = 2;
public static readonly nint FileRenameInfo = 3;
public static readonly nint FileDispositionInfo = 4;
public static readonly nint FileAllocationInfo = 5;
public static readonly nint FileEndOfFileInfo = 6;
public static readonly nint FileStreamInfo = 7;
public static readonly nint FileCompressionInfo = 8;
public static readonly nint FileAttributeTagInfo = 9;
public static readonly nint FileIdBothDirectoryInfo = 10;
public static readonly nint FileIdBothDirectoryRestartInfo = 11;
public static readonly nint FileIoPriorityHintInfo = 12;
public static readonly nint FileRemoteProtocolInfo = 13;
public static readonly nint FileFullDirectoryInfo = 14;
public static readonly nint FileFullDirectoryRestartInfo = 15;
public static readonly nint FileStorageInfo = 16;
public static readonly nint FileAlignmentInfo = 17;
public static readonly nint FileIdInfo = 18;
public static readonly nint FileIdExtdDirectoryInfo = 19;
public static readonly nint FileIdExtdDirectoryRestartInfo = 20;
public static readonly nint FileDispositionInfoEx = 21;
public static readonly nint FileRenameInfoEx = 22;
public static readonly nint FileCaseSensitiveInfo = 23;
public static readonly nint FileNormalizedNameInfo = 24;


// LoadLibrary flags for determining from where to search for a DLL
public static readonly nuint DONT_RESOLVE_DLL_REFERENCES = 0x1;
public static readonly nuint LOAD_LIBRARY_AS_DATAFILE = 0x2;
public static readonly nuint LOAD_WITH_ALTERED_SEARCH_PATH = 0x8;
public static readonly nuint LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x10;
public static readonly nuint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20;
public static readonly nuint LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x40;
public static readonly nuint LOAD_LIBRARY_REQUIRE_SIGNED_TARGET = 0x80;
public static readonly nuint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x100;
public static readonly nuint LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x200;
public static readonly nuint LOAD_LIBRARY_SEARCH_USER_DIRS = 0x400;
public static readonly nuint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x800;
public static readonly nuint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x1000;
public static readonly nuint LOAD_LIBRARY_SAFE_CURRENT_DIRS = 0x00002000;
public static readonly nuint LOAD_LIBRARY_SEARCH_SYSTEM32_NO_FORWARDER = 0x00004000;
public static readonly nuint LOAD_LIBRARY_OS_INTEGRITY_CONTINUITY = 0x00008000;


// RegNotifyChangeKeyValue notifyFilter flags.
 
// REG_NOTIFY_CHANGE_NAME notifies the caller if a subkey is added or deleted.
public static readonly nuint REG_NOTIFY_CHANGE_NAME = 0x00000001; 

// REG_NOTIFY_CHANGE_ATTRIBUTES notifies the caller of changes to the attributes of the key, such as the security descriptor information.
public static readonly nuint REG_NOTIFY_CHANGE_ATTRIBUTES = 0x00000002; 

// REG_NOTIFY_CHANGE_LAST_SET notifies the caller of changes to a value of the key. This can include adding or deleting a value, or changing an existing value.
public static readonly nuint REG_NOTIFY_CHANGE_LAST_SET = 0x00000004; 

// REG_NOTIFY_CHANGE_SECURITY notifies the caller of changes to the security descriptor of the key.
public static readonly nuint REG_NOTIFY_CHANGE_SECURITY = 0x00000008; 

// REG_NOTIFY_THREAD_AGNOSTIC indicates that the lifetime of the registration must not be tied to the lifetime of the thread issuing the RegNotifyChangeKeyValue call. Note: This flag value is only supported in Windows 8 and later.
public static readonly nuint REG_NOTIFY_THREAD_AGNOSTIC = 0x10000000;


public partial struct CommTimeouts {
    public uint ReadIntervalTimeout;
    public uint ReadTotalTimeoutMultiplier;
    public uint ReadTotalTimeoutConstant;
    public uint WriteTotalTimeoutMultiplier;
    public uint WriteTotalTimeoutConstant;
}

// NTUnicodeString is a UTF-16 string for NT native APIs, corresponding to UNICODE_STRING.
public partial struct NTUnicodeString {
    public ushort Length;
    public ushort MaximumLength;
    public ptr<ushort> Buffer;
}

// NTString is an ANSI string for NT native APIs, corresponding to STRING.
public partial struct NTString {
    public ushort Length;
    public ushort MaximumLength;
    public ptr<byte> Buffer;
}

public partial struct LIST_ENTRY {
    public ptr<LIST_ENTRY> Flink;
    public ptr<LIST_ENTRY> Blink;
}

public partial struct LDR_DATA_TABLE_ENTRY {
    public array<System.UIntPtr> reserved1;
    public LIST_ENTRY InMemoryOrderLinks;
    public array<System.UIntPtr> reserved2;
    public System.UIntPtr DllBase;
    public array<System.UIntPtr> reserved3;
    public NTUnicodeString FullDllName;
    public array<byte> reserved4;
    public array<System.UIntPtr> reserved5;
    public System.UIntPtr reserved6;
    public uint TimeDateStamp;
}

public partial struct PEB_LDR_DATA {
    public array<byte> reserved1;
    public array<System.UIntPtr> reserved2;
    public LIST_ENTRY InMemoryOrderModuleList;
}

public partial struct CURDIR {
    public NTUnicodeString DosPath;
    public Handle Handle;
}

public partial struct RTL_DRIVE_LETTER_CURDIR {
    public ushort Flags;
    public ushort Length;
    public uint TimeStamp;
    public NTString DosPath;
}

public partial struct RTL_USER_PROCESS_PARAMETERS {
    public uint MaximumLength;
    public uint Length;
    public uint Flags;
    public uint DebugFlags;
    public Handle ConsoleHandle;
    public uint ConsoleFlags;
    public Handle StandardInput;
    public Handle StandardOutput;
    public Handle StandardError;
    public CURDIR CurrentDirectory;
    public NTUnicodeString DllPath;
    public NTUnicodeString ImagePathName;
    public NTUnicodeString CommandLine;
    public unsafe.Pointer Environment;
    public uint StartingX;
    public uint StartingY;
    public uint CountX;
    public uint CountY;
    public uint CountCharsX;
    public uint CountCharsY;
    public uint FillAttribute;
    public uint WindowFlags;
    public uint ShowWindowFlags;
    public NTUnicodeString WindowTitle;
    public NTUnicodeString DesktopInfo;
    public NTUnicodeString ShellInfo;
    public NTUnicodeString RuntimeData;
    public array<RTL_DRIVE_LETTER_CURDIR> CurrentDirectories;
    public System.UIntPtr EnvironmentSize;
    public System.UIntPtr EnvironmentVersion;
    public unsafe.Pointer PackageDependencyData;
    public uint ProcessGroupId;
    public uint LoaderThreads;
    public NTUnicodeString RedirectionDllName;
    public NTUnicodeString HeapPartitionName;
    public System.UIntPtr DefaultThreadpoolCpuSetMasks;
    public uint DefaultThreadpoolCpuSetMaskCount;
}

public partial struct PEB {
    public array<byte> reserved1;
    public byte BeingDebugged;
    public byte BitField;
    public System.UIntPtr reserved3;
    public System.UIntPtr ImageBaseAddress;
    public ptr<PEB_LDR_DATA> Ldr;
    public ptr<RTL_USER_PROCESS_PARAMETERS> ProcessParameters;
    public array<System.UIntPtr> reserved4;
    public System.UIntPtr AtlThunkSListPtr;
    public System.UIntPtr reserved5;
    public uint reserved6;
    public System.UIntPtr reserved7;
    public uint reserved8;
    public uint AtlThunkSListPtr32;
    public array<System.UIntPtr> reserved9;
    public array<byte> reserved10;
    public System.UIntPtr PostProcessInitRoutine;
    public array<byte> reserved11;
    public array<System.UIntPtr> reserved12;
    public uint SessionId;
}

public partial struct OBJECT_ATTRIBUTES {
    public uint Length;
    public Handle RootDirectory;
    public ptr<NTUnicodeString> ObjectName;
    public uint Attributes;
    public ptr<SECURITY_DESCRIPTOR> SecurityDescriptor;
    public ptr<SECURITY_QUALITY_OF_SERVICE> SecurityQoS;
}

// Values for the Attributes member of OBJECT_ATTRIBUTES.
public static readonly nuint OBJ_INHERIT = 0x00000002;
public static readonly nuint OBJ_PERMANENT = 0x00000010;
public static readonly nuint OBJ_EXCLUSIVE = 0x00000020;
public static readonly nuint OBJ_CASE_INSENSITIVE = 0x00000040;
public static readonly nuint OBJ_OPENIF = 0x00000080;
public static readonly nuint OBJ_OPENLINK = 0x00000100;
public static readonly nuint OBJ_KERNEL_HANDLE = 0x00000200;
public static readonly nuint OBJ_FORCE_ACCESS_CHECK = 0x00000400;
public static readonly nuint OBJ_IGNORE_IMPERSONATED_DEVICEMAP = 0x00000800;
public static readonly nuint OBJ_DONT_REPARSE = 0x00001000;
public static readonly nuint OBJ_VALID_ATTRIBUTES = 0x00001FF2;


public partial struct IO_STATUS_BLOCK {
    public NTStatus Status;
    public System.UIntPtr Information;
}

public partial struct RTLP_CURDIR_REF {
    public int RefCount;
    public Handle Handle;
}

public partial struct RTL_RELATIVE_NAME {
    public NTUnicodeString RelativeName;
    public Handle ContainingDirectory;
    public ptr<RTLP_CURDIR_REF> CurDirRef;
}

 
// CreateDisposition flags for NtCreateFile and NtCreateNamedPipeFile.
public static readonly nuint FILE_SUPERSEDE = 0x00000000;
public static readonly nuint FILE_OPEN = 0x00000001;
public static readonly nuint FILE_CREATE = 0x00000002;
public static readonly nuint FILE_OPEN_IF = 0x00000003;
public static readonly nuint FILE_OVERWRITE = 0x00000004;
public static readonly nuint FILE_OVERWRITE_IF = 0x00000005;
public static readonly nuint FILE_MAXIMUM_DISPOSITION = 0x00000005; 

// CreateOptions flags for NtCreateFile and NtCreateNamedPipeFile.
public static readonly nuint FILE_DIRECTORY_FILE = 0x00000001;
public static readonly nuint FILE_WRITE_THROUGH = 0x00000002;
public static readonly nuint FILE_SEQUENTIAL_ONLY = 0x00000004;
public static readonly nuint FILE_NO_INTERMEDIATE_BUFFERING = 0x00000008;
public static readonly nuint FILE_SYNCHRONOUS_IO_ALERT = 0x00000010;
public static readonly nuint FILE_SYNCHRONOUS_IO_NONALERT = 0x00000020;
public static readonly nuint FILE_NON_DIRECTORY_FILE = 0x00000040;
public static readonly nuint FILE_CREATE_TREE_CONNECTION = 0x00000080;
public static readonly nuint FILE_COMPLETE_IF_OPLOCKED = 0x00000100;
public static readonly nuint FILE_NO_EA_KNOWLEDGE = 0x00000200;
public static readonly nuint FILE_OPEN_REMOTE_INSTANCE = 0x00000400;
public static readonly nuint FILE_RANDOM_ACCESS = 0x00000800;
public static readonly nuint FILE_DELETE_ON_CLOSE = 0x00001000;
public static readonly nuint FILE_OPEN_BY_FILE_ID = 0x00002000;
public static readonly nuint FILE_OPEN_FOR_BACKUP_INTENT = 0x00004000;
public static readonly nuint FILE_NO_COMPRESSION = 0x00008000;
public static readonly nuint FILE_OPEN_REQUIRING_OPLOCK = 0x00010000;
public static readonly nuint FILE_DISALLOW_EXCLUSIVE = 0x00020000;
public static readonly nuint FILE_RESERVE_OPFILTER = 0x00100000;
public static readonly nuint FILE_OPEN_REPARSE_POINT = 0x00200000;
public static readonly nuint FILE_OPEN_NO_RECALL = 0x00400000;
public static readonly nuint FILE_OPEN_FOR_FREE_SPACE_QUERY = 0x00800000; 

// Parameter constants for NtCreateNamedPipeFile.

public static readonly nuint FILE_PIPE_BYTE_STREAM_TYPE = 0x00000000;
public static readonly nuint FILE_PIPE_MESSAGE_TYPE = 0x00000001;

public static readonly nuint FILE_PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000;
public static readonly nuint FILE_PIPE_REJECT_REMOTE_CLIENTS = 0x00000002;

public static readonly nuint FILE_PIPE_TYPE_VALID_MASK = 0x00000003;

public static readonly nuint FILE_PIPE_BYTE_STREAM_MODE = 0x00000000;
public static readonly nuint FILE_PIPE_MESSAGE_MODE = 0x00000001;

public static readonly nuint FILE_PIPE_QUEUE_OPERATION = 0x00000000;
public static readonly nuint FILE_PIPE_COMPLETE_OPERATION = 0x00000001;

public static readonly nuint FILE_PIPE_INBOUND = 0x00000000;
public static readonly nuint FILE_PIPE_OUTBOUND = 0x00000001;
public static readonly nuint FILE_PIPE_FULL_DUPLEX = 0x00000002;

public static readonly nuint FILE_PIPE_DISCONNECTED_STATE = 0x00000001;
public static readonly nuint FILE_PIPE_LISTENING_STATE = 0x00000002;
public static readonly nuint FILE_PIPE_CONNECTED_STATE = 0x00000003;
public static readonly nuint FILE_PIPE_CLOSING_STATE = 0x00000004;

public static readonly nuint FILE_PIPE_CLIENT_END = 0x00000000;
public static readonly nuint FILE_PIPE_SERVER_END = 0x00000001;


// ProcessInformationClasses for NtQueryInformationProcess and NtSetInformationProcess.
public static readonly var ProcessBasicInformation = iota;
public static readonly var ProcessQuotaLimits = 0;
public static readonly var ProcessIoCounters = 1;
public static readonly var ProcessVmCounters = 2;
public static readonly var ProcessTimes = 3;
public static readonly var ProcessBasePriority = 4;
public static readonly var ProcessRaisePriority = 5;
public static readonly var ProcessDebugPort = 6;
public static readonly var ProcessExceptionPort = 7;
public static readonly var ProcessAccessToken = 8;
public static readonly var ProcessLdtInformation = 9;
public static readonly var ProcessLdtSize = 10;
public static readonly var ProcessDefaultHardErrorMode = 11;
public static readonly var ProcessIoPortHandlers = 12;
public static readonly var ProcessPooledUsageAndLimits = 13;
public static readonly var ProcessWorkingSetWatch = 14;
public static readonly var ProcessUserModeIOPL = 15;
public static readonly var ProcessEnableAlignmentFaultFixup = 16;
public static readonly var ProcessPriorityClass = 17;
public static readonly var ProcessWx86Information = 18;
public static readonly var ProcessHandleCount = 19;
public static readonly var ProcessAffinityMask = 20;
public static readonly var ProcessPriorityBoost = 21;
public static readonly var ProcessDeviceMap = 22;
public static readonly var ProcessSessionInformation = 23;
public static readonly var ProcessForegroundInformation = 24;
public static readonly var ProcessWow64Information = 25;
public static readonly var ProcessImageFileName = 26;
public static readonly var ProcessLUIDDeviceMapsEnabled = 27;
public static readonly var ProcessBreakOnTermination = 28;
public static readonly var ProcessDebugObjectHandle = 29;
public static readonly var ProcessDebugFlags = 30;
public static readonly var ProcessHandleTracing = 31;
public static readonly var ProcessIoPriority = 32;
public static readonly var ProcessExecuteFlags = 33;
public static readonly var ProcessTlsInformation = 34;
public static readonly var ProcessCookie = 35;
public static readonly var ProcessImageInformation = 36;
public static readonly var ProcessCycleTime = 37;
public static readonly var ProcessPagePriority = 38;
public static readonly var ProcessInstrumentationCallback = 39;
public static readonly var ProcessThreadStackAllocation = 40;
public static readonly var ProcessWorkingSetWatchEx = 41;
public static readonly var ProcessImageFileNameWin32 = 42;
public static readonly var ProcessImageFileMapping = 43;
public static readonly var ProcessAffinityUpdateMode = 44;
public static readonly var ProcessMemoryAllocationMode = 45;
public static readonly var ProcessGroupInformation = 46;
public static readonly var ProcessTokenVirtualizationEnabled = 47;
public static readonly var ProcessConsoleHostProcess = 48;
public static readonly var ProcessWindowInformation = 49;
public static readonly var ProcessHandleInformation = 50;
public static readonly var ProcessMitigationPolicy = 51;
public static readonly var ProcessDynamicFunctionTableInformation = 52;
public static readonly var ProcessHandleCheckingMode = 53;
public static readonly var ProcessKeepAliveCount = 54;
public static readonly var ProcessRevokeFileHandles = 55;
public static readonly var ProcessWorkingSetControl = 56;
public static readonly var ProcessHandleTable = 57;
public static readonly var ProcessCheckStackExtentsMode = 58;
public static readonly var ProcessCommandLineInformation = 59;
public static readonly var ProcessProtectionInformation = 60;
public static readonly var ProcessMemoryExhaustion = 61;
public static readonly var ProcessFaultInformation = 62;
public static readonly var ProcessTelemetryIdInformation = 63;
public static readonly var ProcessCommitReleaseInformation = 64;
public static readonly var ProcessDefaultCpuSetsInformation = 65;
public static readonly var ProcessAllowedCpuSetsInformation = 66;
public static readonly var ProcessSubsystemProcess = 67;
public static readonly var ProcessJobMemoryInformation = 68;
public static readonly var ProcessInPrivate = 69;
public static readonly var ProcessRaiseUMExceptionOnInvalidHandleClose = 70;
public static readonly var ProcessIumChallengeResponse = 71;
public static readonly var ProcessChildProcessInformation = 72;
public static readonly var ProcessHighGraphicsPriorityInformation = 73;
public static readonly var ProcessSubsystemInformation = 74;
public static readonly var ProcessEnergyValues = 75;
public static readonly var ProcessActivityThrottleState = 76;
public static readonly var ProcessActivityThrottlePolicy = 77;
public static readonly var ProcessWin32kSyscallFilterInformation = 78;
public static readonly var ProcessDisableSystemAllowedCpuSets = 79;
public static readonly var ProcessWakeInformation = 80;
public static readonly var ProcessEnergyTrackingState = 81;
public static readonly var ProcessManageWritesToExecutableMemory = 82;
public static readonly var ProcessCaptureTrustletLiveDump = 83;
public static readonly var ProcessTelemetryCoverage = 84;
public static readonly var ProcessEnclaveInformation = 85;
public static readonly var ProcessEnableReadWriteVmLogging = 86;
public static readonly var ProcessUptimeInformation = 87;
public static readonly var ProcessImageSection = 88;
public static readonly var ProcessDebugAuthInformation = 89;
public static readonly var ProcessSystemResourceManagement = 90;
public static readonly var ProcessSequenceNumber = 91;
public static readonly var ProcessLoaderDetour = 92;
public static readonly var ProcessSecurityDomainInformation = 93;
public static readonly var ProcessCombineSecurityDomainsInformation = 94;
public static readonly var ProcessEnableLogging = 95;
public static readonly var ProcessLeapSecondInformation = 96;
public static readonly var ProcessFiberShadowStackAllocation = 97;
public static readonly var ProcessFreeFiberShadowStackAllocation = 98;
public static readonly var ProcessAltSystemCallInformation = 99;
public static readonly var ProcessDynamicEHContinuationTargets = 100;
public static readonly var ProcessDynamicEnforcedCetCompatibleRanges = 101;


public partial struct PROCESS_BASIC_INFORMATION {
    public NTStatus ExitStatus;
    public ptr<PEB> PebBaseAddress;
    public System.UIntPtr AffinityMask;
    public int BasePriority;
    public System.UIntPtr UniqueProcessId;
    public System.UIntPtr InheritedFromUniqueProcessId;
}

// Constants for LocalAlloc flags.
public static readonly nuint LMEM_FIXED = 0x0;
public static readonly nuint LMEM_MOVEABLE = 0x2;
public static readonly nuint LMEM_NOCOMPACT = 0x10;
public static readonly nuint LMEM_NODISCARD = 0x20;
public static readonly nuint LMEM_ZEROINIT = 0x40;
public static readonly nuint LMEM_MODIFY = 0x80;
public static readonly nuint LMEM_DISCARDABLE = 0xf00;
public static readonly nuint LMEM_VALID_FLAGS = 0xf72;
public static readonly nuint LMEM_INVALID_HANDLE = 0x8000;
public static readonly var LHND = LMEM_MOVEABLE | LMEM_ZEROINIT;
public static readonly var LPTR = LMEM_FIXED | LMEM_ZEROINIT;
public static readonly var NONZEROLHND = LMEM_MOVEABLE;
public static readonly var NONZEROLPTR = LMEM_FIXED;


// Constants for the CreateNamedPipe-family of functions.
public static readonly nuint PIPE_ACCESS_INBOUND = 0x1;
public static readonly nuint PIPE_ACCESS_OUTBOUND = 0x2;
public static readonly nuint PIPE_ACCESS_DUPLEX = 0x3;

public static readonly nuint PIPE_CLIENT_END = 0x0;
public static readonly nuint PIPE_SERVER_END = 0x1;

public static readonly nuint PIPE_WAIT = 0x0;
public static readonly nuint PIPE_NOWAIT = 0x1;
public static readonly nuint PIPE_READMODE_BYTE = 0x0;
public static readonly nuint PIPE_READMODE_MESSAGE = 0x2;
public static readonly nuint PIPE_TYPE_BYTE = 0x0;
public static readonly nuint PIPE_TYPE_MESSAGE = 0x4;
public static readonly nuint PIPE_ACCEPT_REMOTE_CLIENTS = 0x0;
public static readonly nuint PIPE_REJECT_REMOTE_CLIENTS = 0x8;

public static readonly nint PIPE_UNLIMITED_INSTANCES = 255;


// Constants for security attributes when opening named pipes.
public static readonly var SECURITY_ANONYMOUS = SecurityAnonymous << 16;
public static readonly var SECURITY_IDENTIFICATION = SecurityIdentification << 16;
public static readonly var SECURITY_IMPERSONATION = SecurityImpersonation << 16;
public static readonly var SECURITY_DELEGATION = SecurityDelegation << 16;

public static readonly nuint SECURITY_CONTEXT_TRACKING = 0x40000;
public static readonly nuint SECURITY_EFFECTIVE_ONLY = 0x80000;

public static readonly nuint SECURITY_SQOS_PRESENT = 0x100000;
public static readonly nuint SECURITY_VALID_SQOS_FLAGS = 0x1f0000;


// ResourceID represents a 16-bit resource identifier, traditionally created with the MAKEINTRESOURCE macro.
public partial struct ResourceID { // : ushort
}

// ResourceIDOrString must be either a ResourceID, to specify a resource or resource type by ID,
// or a string, to specify a resource or resource type by name.
public partial interface ResourceIDOrString {
}

// Predefined resource names and types.
 
// Predefined names.
public static ResourceID CREATEPROCESS_MANIFEST_RESOURCE_ID = 1;public static ResourceID ISOLATIONAWARE_MANIFEST_RESOURCE_ID = 2;public static ResourceID ISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID = 3;public static ResourceID ISOLATIONPOLICY_MANIFEST_RESOURCE_ID = 4;public static ResourceID ISOLATIONPOLICY_BROWSER_MANIFEST_RESOURCE_ID = 5;public static ResourceID MINIMUM_RESERVED_MANIFEST_RESOURCE_ID = 1;public static ResourceID MAXIMUM_RESERVED_MANIFEST_RESOURCE_ID = 16;public static ResourceID RT_CURSOR = 1;public static ResourceID RT_BITMAP = 2;public static ResourceID RT_ICON = 3;public static ResourceID RT_MENU = 4;public static ResourceID RT_DIALOG = 5;public static ResourceID RT_STRING = 6;public static ResourceID RT_FONTDIR = 7;public static ResourceID RT_FONT = 8;public static ResourceID RT_ACCELERATOR = 9;public static ResourceID RT_RCDATA = 10;public static ResourceID RT_MESSAGETABLE = 11;public static ResourceID RT_GROUP_CURSOR = 12;public static ResourceID RT_GROUP_ICON = 14;public static ResourceID RT_VERSION = 16;public static ResourceID RT_DLGINCLUDE = 17;public static ResourceID RT_PLUGPLAY = 19;public static ResourceID RT_VXD = 20;public static ResourceID RT_ANICURSOR = 21;public static ResourceID RT_ANIICON = 22;public static ResourceID RT_HTML = 23;public static ResourceID RT_MANIFEST = 24;

public partial struct COAUTHIDENTITY {
    public ptr<ushort> User;
    public uint UserLength;
    public ptr<ushort> Domain;
    public uint DomainLength;
    public ptr<ushort> Password;
    public uint PasswordLength;
    public uint Flags;
}

public partial struct COAUTHINFO {
    public uint AuthnSvc;
    public uint AuthzSvc;
    public ptr<ushort> ServerPrincName;
    public uint AuthnLevel;
    public uint ImpersonationLevel;
    public ptr<COAUTHIDENTITY> AuthIdentityData;
    public uint Capabilities;
}

public partial struct COSERVERINFO {
    public uint Reserved1;
    public ptr<ushort> Aame;
    public ptr<COAUTHINFO> AuthInfo;
    public uint Reserved2;
}

public partial struct BIND_OPTS3 {
    public uint CbStruct;
    public uint Flags;
    public uint Mode;
    public uint TickCountDeadline;
    public uint TrackFlags;
    public uint ClassContext;
    public uint Locale;
    public ptr<COSERVERINFO> ServerInfo;
    public HWND Hwnd;
}

public static readonly nuint CLSCTX_INPROC_SERVER = 0x1;
public static readonly nuint CLSCTX_INPROC_HANDLER = 0x2;
public static readonly nuint CLSCTX_LOCAL_SERVER = 0x4;
public static readonly nuint CLSCTX_INPROC_SERVER16 = 0x8;
public static readonly nuint CLSCTX_REMOTE_SERVER = 0x10;
public static readonly nuint CLSCTX_INPROC_HANDLER16 = 0x20;
public static readonly nuint CLSCTX_RESERVED1 = 0x40;
public static readonly nuint CLSCTX_RESERVED2 = 0x80;
public static readonly nuint CLSCTX_RESERVED3 = 0x100;
public static readonly nuint CLSCTX_RESERVED4 = 0x200;
public static readonly nuint CLSCTX_NO_CODE_DOWNLOAD = 0x400;
public static readonly nuint CLSCTX_RESERVED5 = 0x800;
public static readonly nuint CLSCTX_NO_CUSTOM_MARSHAL = 0x1000;
public static readonly nuint CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000;
public static readonly nuint CLSCTX_NO_FAILURE_LOG = 0x4000;
public static readonly nuint CLSCTX_DISABLE_AAA = 0x8000;
public static readonly nuint CLSCTX_ENABLE_AAA = 0x10000;
public static readonly nuint CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000;
public static readonly nuint CLSCTX_ACTIVATE_32_BIT_SERVER = 0x40000;
public static readonly nuint CLSCTX_ACTIVATE_64_BIT_SERVER = 0x80000;
public static readonly nuint CLSCTX_ENABLE_CLOAKING = 0x100000;
public static readonly nuint CLSCTX_APPCONTAINER = 0x400000;
public static readonly nuint CLSCTX_ACTIVATE_AAA_AS_IU = 0x800000;
public static readonly nuint CLSCTX_PS_DLL = 0x80000000;

public static readonly nuint COINIT_MULTITHREADED = 0x0;
public static readonly nuint COINIT_APARTMENTTHREADED = 0x2;
public static readonly nuint COINIT_DISABLE_OLE1DDE = 0x4;
public static readonly nuint COINIT_SPEED_OVER_MEMORY = 0x8;


// Flag for QueryFullProcessImageName.
public static readonly nint PROCESS_NAME_NATIVE = 1;


} // end windows_package
