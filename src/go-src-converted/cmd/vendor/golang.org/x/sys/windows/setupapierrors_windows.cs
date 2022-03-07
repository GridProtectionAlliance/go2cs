// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 23:30:36 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\setupapierrors_windows.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public static readonly syscall.Errno ERROR_EXPECTED_SECTION_NAME = 0x20000000 | 0xC0000000 | 0;
public static readonly syscall.Errno ERROR_BAD_SECTION_NAME_LINE = 0x20000000 | 0xC0000000 | 1;
public static readonly syscall.Errno ERROR_SECTION_NAME_TOO_LONG = 0x20000000 | 0xC0000000 | 2;
public static readonly syscall.Errno ERROR_GENERAL_SYNTAX = 0x20000000 | 0xC0000000 | 3;
public static readonly syscall.Errno ERROR_WRONG_INF_STYLE = 0x20000000 | 0xC0000000 | 0x100;
public static readonly syscall.Errno ERROR_SECTION_NOT_FOUND = 0x20000000 | 0xC0000000 | 0x101;
public static readonly syscall.Errno ERROR_LINE_NOT_FOUND = 0x20000000 | 0xC0000000 | 0x102;
public static readonly syscall.Errno ERROR_NO_BACKUP = 0x20000000 | 0xC0000000 | 0x103;
public static readonly syscall.Errno ERROR_NO_ASSOCIATED_CLASS = 0x20000000 | 0xC0000000 | 0x200;
public static readonly syscall.Errno ERROR_CLASS_MISMATCH = 0x20000000 | 0xC0000000 | 0x201;
public static readonly syscall.Errno ERROR_DUPLICATE_FOUND = 0x20000000 | 0xC0000000 | 0x202;
public static readonly syscall.Errno ERROR_NO_DRIVER_SELECTED = 0x20000000 | 0xC0000000 | 0x203;
public static readonly syscall.Errno ERROR_KEY_DOES_NOT_EXIST = 0x20000000 | 0xC0000000 | 0x204;
public static readonly syscall.Errno ERROR_INVALID_DEVINST_NAME = 0x20000000 | 0xC0000000 | 0x205;
public static readonly syscall.Errno ERROR_INVALID_CLASS = 0x20000000 | 0xC0000000 | 0x206;
public static readonly syscall.Errno ERROR_DEVINST_ALREADY_EXISTS = 0x20000000 | 0xC0000000 | 0x207;
public static readonly syscall.Errno ERROR_DEVINFO_NOT_REGISTERED = 0x20000000 | 0xC0000000 | 0x208;
public static readonly syscall.Errno ERROR_INVALID_REG_PROPERTY = 0x20000000 | 0xC0000000 | 0x209;
public static readonly syscall.Errno ERROR_NO_INF = 0x20000000 | 0xC0000000 | 0x20A;
public static readonly syscall.Errno ERROR_NO_SUCH_DEVINST = 0x20000000 | 0xC0000000 | 0x20B;
public static readonly syscall.Errno ERROR_CANT_LOAD_CLASS_ICON = 0x20000000 | 0xC0000000 | 0x20C;
public static readonly syscall.Errno ERROR_INVALID_CLASS_INSTALLER = 0x20000000 | 0xC0000000 | 0x20D;
public static readonly syscall.Errno ERROR_DI_DO_DEFAULT = 0x20000000 | 0xC0000000 | 0x20E;
public static readonly syscall.Errno ERROR_DI_NOFILECOPY = 0x20000000 | 0xC0000000 | 0x20F;
public static readonly syscall.Errno ERROR_INVALID_HWPROFILE = 0x20000000 | 0xC0000000 | 0x210;
public static readonly syscall.Errno ERROR_NO_DEVICE_SELECTED = 0x20000000 | 0xC0000000 | 0x211;
public static readonly syscall.Errno ERROR_DEVINFO_LIST_LOCKED = 0x20000000 | 0xC0000000 | 0x212;
public static readonly syscall.Errno ERROR_DEVINFO_DATA_LOCKED = 0x20000000 | 0xC0000000 | 0x213;
public static readonly syscall.Errno ERROR_DI_BAD_PATH = 0x20000000 | 0xC0000000 | 0x214;
public static readonly syscall.Errno ERROR_NO_CLASSINSTALL_PARAMS = 0x20000000 | 0xC0000000 | 0x215;
public static readonly syscall.Errno ERROR_FILEQUEUE_LOCKED = 0x20000000 | 0xC0000000 | 0x216;
public static readonly syscall.Errno ERROR_BAD_SERVICE_INSTALLSECT = 0x20000000 | 0xC0000000 | 0x217;
public static readonly syscall.Errno ERROR_NO_CLASS_DRIVER_LIST = 0x20000000 | 0xC0000000 | 0x218;
public static readonly syscall.Errno ERROR_NO_ASSOCIATED_SERVICE = 0x20000000 | 0xC0000000 | 0x219;
public static readonly syscall.Errno ERROR_NO_DEFAULT_DEVICE_INTERFACE = 0x20000000 | 0xC0000000 | 0x21A;
public static readonly syscall.Errno ERROR_DEVICE_INTERFACE_ACTIVE = 0x20000000 | 0xC0000000 | 0x21B;
public static readonly syscall.Errno ERROR_DEVICE_INTERFACE_REMOVED = 0x20000000 | 0xC0000000 | 0x21C;
public static readonly syscall.Errno ERROR_BAD_INTERFACE_INSTALLSECT = 0x20000000 | 0xC0000000 | 0x21D;
public static readonly syscall.Errno ERROR_NO_SUCH_INTERFACE_CLASS = 0x20000000 | 0xC0000000 | 0x21E;
public static readonly syscall.Errno ERROR_INVALID_REFERENCE_STRING = 0x20000000 | 0xC0000000 | 0x21F;
public static readonly syscall.Errno ERROR_INVALID_MACHINENAME = 0x20000000 | 0xC0000000 | 0x220;
public static readonly syscall.Errno ERROR_REMOTE_COMM_FAILURE = 0x20000000 | 0xC0000000 | 0x221;
public static readonly syscall.Errno ERROR_MACHINE_UNAVAILABLE = 0x20000000 | 0xC0000000 | 0x222;
public static readonly syscall.Errno ERROR_NO_CONFIGMGR_SERVICES = 0x20000000 | 0xC0000000 | 0x223;
public static readonly syscall.Errno ERROR_INVALID_PROPPAGE_PROVIDER = 0x20000000 | 0xC0000000 | 0x224;
public static readonly syscall.Errno ERROR_NO_SUCH_DEVICE_INTERFACE = 0x20000000 | 0xC0000000 | 0x225;
public static readonly syscall.Errno ERROR_DI_POSTPROCESSING_REQUIRED = 0x20000000 | 0xC0000000 | 0x226;
public static readonly syscall.Errno ERROR_INVALID_COINSTALLER = 0x20000000 | 0xC0000000 | 0x227;
public static readonly syscall.Errno ERROR_NO_COMPAT_DRIVERS = 0x20000000 | 0xC0000000 | 0x228;
public static readonly syscall.Errno ERROR_NO_DEVICE_ICON = 0x20000000 | 0xC0000000 | 0x229;
public static readonly syscall.Errno ERROR_INVALID_INF_LOGCONFIG = 0x20000000 | 0xC0000000 | 0x22A;
public static readonly syscall.Errno ERROR_DI_DONT_INSTALL = 0x20000000 | 0xC0000000 | 0x22B;
public static readonly syscall.Errno ERROR_INVALID_FILTER_DRIVER = 0x20000000 | 0xC0000000 | 0x22C;
public static readonly syscall.Errno ERROR_NON_WINDOWS_NT_DRIVER = 0x20000000 | 0xC0000000 | 0x22D;
public static readonly syscall.Errno ERROR_NON_WINDOWS_DRIVER = 0x20000000 | 0xC0000000 | 0x22E;
public static readonly syscall.Errno ERROR_NO_CATALOG_FOR_OEM_INF = 0x20000000 | 0xC0000000 | 0x22F;
public static readonly syscall.Errno ERROR_DEVINSTALL_QUEUE_NONNATIVE = 0x20000000 | 0xC0000000 | 0x230;
public static readonly syscall.Errno ERROR_NOT_DISABLEABLE = 0x20000000 | 0xC0000000 | 0x231;
public static readonly syscall.Errno ERROR_CANT_REMOVE_DEVINST = 0x20000000 | 0xC0000000 | 0x232;
public static readonly syscall.Errno ERROR_INVALID_TARGET = 0x20000000 | 0xC0000000 | 0x233;
public static readonly syscall.Errno ERROR_DRIVER_NONNATIVE = 0x20000000 | 0xC0000000 | 0x234;
public static readonly syscall.Errno ERROR_IN_WOW64 = 0x20000000 | 0xC0000000 | 0x235;
public static readonly syscall.Errno ERROR_SET_SYSTEM_RESTORE_POINT = 0x20000000 | 0xC0000000 | 0x236;
public static readonly syscall.Errno ERROR_SCE_DISABLED = 0x20000000 | 0xC0000000 | 0x238;
public static readonly syscall.Errno ERROR_UNKNOWN_EXCEPTION = 0x20000000 | 0xC0000000 | 0x239;
public static readonly syscall.Errno ERROR_PNP_REGISTRY_ERROR = 0x20000000 | 0xC0000000 | 0x23A;
public static readonly syscall.Errno ERROR_REMOTE_REQUEST_UNSUPPORTED = 0x20000000 | 0xC0000000 | 0x23B;
public static readonly syscall.Errno ERROR_NOT_AN_INSTALLED_OEM_INF = 0x20000000 | 0xC0000000 | 0x23C;
public static readonly syscall.Errno ERROR_INF_IN_USE_BY_DEVICES = 0x20000000 | 0xC0000000 | 0x23D;
public static readonly syscall.Errno ERROR_DI_FUNCTION_OBSOLETE = 0x20000000 | 0xC0000000 | 0x23E;
public static readonly syscall.Errno ERROR_NO_AUTHENTICODE_CATALOG = 0x20000000 | 0xC0000000 | 0x23F;
public static readonly syscall.Errno ERROR_AUTHENTICODE_DISALLOWED = 0x20000000 | 0xC0000000 | 0x240;
public static readonly syscall.Errno ERROR_AUTHENTICODE_TRUSTED_PUBLISHER = 0x20000000 | 0xC0000000 | 0x241;
public static readonly syscall.Errno ERROR_AUTHENTICODE_TRUST_NOT_ESTABLISHED = 0x20000000 | 0xC0000000 | 0x242;
public static readonly syscall.Errno ERROR_AUTHENTICODE_PUBLISHER_NOT_TRUSTED = 0x20000000 | 0xC0000000 | 0x243;
public static readonly syscall.Errno ERROR_SIGNATURE_OSATTRIBUTE_MISMATCH = 0x20000000 | 0xC0000000 | 0x244;
public static readonly syscall.Errno ERROR_ONLY_VALIDATE_VIA_AUTHENTICODE = 0x20000000 | 0xC0000000 | 0x245;
public static readonly syscall.Errno ERROR_DEVICE_INSTALLER_NOT_READY = 0x20000000 | 0xC0000000 | 0x246;
public static readonly syscall.Errno ERROR_DRIVER_STORE_ADD_FAILED = 0x20000000 | 0xC0000000 | 0x247;
public static readonly syscall.Errno ERROR_DEVICE_INSTALL_BLOCKED = 0x20000000 | 0xC0000000 | 0x248;
public static readonly syscall.Errno ERROR_DRIVER_INSTALL_BLOCKED = 0x20000000 | 0xC0000000 | 0x249;
public static readonly syscall.Errno ERROR_WRONG_INF_TYPE = 0x20000000 | 0xC0000000 | 0x24A;
public static readonly syscall.Errno ERROR_FILE_HASH_NOT_IN_CATALOG = 0x20000000 | 0xC0000000 | 0x24B;
public static readonly syscall.Errno ERROR_DRIVER_STORE_DELETE_FAILED = 0x20000000 | 0xC0000000 | 0x24C;
public static readonly syscall.Errno ERROR_UNRECOVERABLE_STACK_OVERFLOW = 0x20000000 | 0xC0000000 | 0x300;
public static readonly syscall.Errno EXCEPTION_SPAPI_UNRECOVERABLE_STACK_OVERFLOW = ERROR_UNRECOVERABLE_STACK_OVERFLOW;
public static readonly syscall.Errno ERROR_NO_DEFAULT_INTERFACE_DEVICE = ERROR_NO_DEFAULT_DEVICE_INTERFACE;
public static readonly syscall.Errno ERROR_INTERFACE_DEVICE_ACTIVE = ERROR_DEVICE_INTERFACE_ACTIVE;
public static readonly syscall.Errno ERROR_INTERFACE_DEVICE_REMOVED = ERROR_DEVICE_INTERFACE_REMOVED;
public static readonly syscall.Errno ERROR_NO_SUCH_INTERFACE_DEVICE = ERROR_NO_SUCH_DEVICE_INTERFACE;


} // end windows_package
