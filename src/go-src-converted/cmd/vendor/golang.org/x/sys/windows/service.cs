// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package windows -- go2cs converted at 2022 March 13 06:41:28 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\service.go
namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public static readonly nint SC_MANAGER_CONNECT = 1;
public static readonly nint SC_MANAGER_CREATE_SERVICE = 2;
public static readonly nint SC_MANAGER_ENUMERATE_SERVICE = 4;
public static readonly nint SC_MANAGER_LOCK = 8;
public static readonly nint SC_MANAGER_QUERY_LOCK_STATUS = 16;
public static readonly nint SC_MANAGER_MODIFY_BOOT_CONFIG = 32;
public static readonly nuint SC_MANAGER_ALL_ACCESS = 0xf003f;

//sys    OpenSCManager(machineName *uint16, databaseName *uint16, access uint32) (handle Handle, err error) [failretval==0] = advapi32.OpenSCManagerW

public static readonly nint SERVICE_KERNEL_DRIVER = 1;
public static readonly nint SERVICE_FILE_SYSTEM_DRIVER = 2;
public static readonly nint SERVICE_ADAPTER = 4;
public static readonly nint SERVICE_RECOGNIZER_DRIVER = 8;
public static readonly nint SERVICE_WIN32_OWN_PROCESS = 16;
public static readonly nint SERVICE_WIN32_SHARE_PROCESS = 32;
public static readonly var SERVICE_WIN32 = SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS;
public static readonly nint SERVICE_INTERACTIVE_PROCESS = 256;
public static readonly var SERVICE_DRIVER = SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER;
public static readonly var SERVICE_TYPE_ALL = SERVICE_WIN32 | SERVICE_ADAPTER | SERVICE_DRIVER | SERVICE_INTERACTIVE_PROCESS;

public static readonly nint SERVICE_BOOT_START = 0;
public static readonly nint SERVICE_SYSTEM_START = 1;
public static readonly nint SERVICE_AUTO_START = 2;
public static readonly nint SERVICE_DEMAND_START = 3;
public static readonly nint SERVICE_DISABLED = 4;

public static readonly nint SERVICE_ERROR_IGNORE = 0;
public static readonly nint SERVICE_ERROR_NORMAL = 1;
public static readonly nint SERVICE_ERROR_SEVERE = 2;
public static readonly nint SERVICE_ERROR_CRITICAL = 3;

public static readonly nint SC_STATUS_PROCESS_INFO = 0;

public static readonly nint SC_ACTION_NONE = 0;
public static readonly nint SC_ACTION_RESTART = 1;
public static readonly nint SC_ACTION_REBOOT = 2;
public static readonly nint SC_ACTION_RUN_COMMAND = 3;

public static readonly nint SERVICE_STOPPED = 1;
public static readonly nint SERVICE_START_PENDING = 2;
public static readonly nint SERVICE_STOP_PENDING = 3;
public static readonly nint SERVICE_RUNNING = 4;
public static readonly nint SERVICE_CONTINUE_PENDING = 5;
public static readonly nint SERVICE_PAUSE_PENDING = 6;
public static readonly nint SERVICE_PAUSED = 7;
public static readonly nuint SERVICE_NO_CHANGE = 0xffffffff;

public static readonly nint SERVICE_ACCEPT_STOP = 1;
public static readonly nint SERVICE_ACCEPT_PAUSE_CONTINUE = 2;
public static readonly nint SERVICE_ACCEPT_SHUTDOWN = 4;
public static readonly nint SERVICE_ACCEPT_PARAMCHANGE = 8;
public static readonly nint SERVICE_ACCEPT_NETBINDCHANGE = 16;
public static readonly nint SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 32;
public static readonly nint SERVICE_ACCEPT_POWEREVENT = 64;
public static readonly nint SERVICE_ACCEPT_SESSIONCHANGE = 128;
public static readonly nint SERVICE_ACCEPT_PRESHUTDOWN = 256;

public static readonly nint SERVICE_CONTROL_STOP = 1;
public static readonly nint SERVICE_CONTROL_PAUSE = 2;
public static readonly nint SERVICE_CONTROL_CONTINUE = 3;
public static readonly nint SERVICE_CONTROL_INTERROGATE = 4;
public static readonly nint SERVICE_CONTROL_SHUTDOWN = 5;
public static readonly nint SERVICE_CONTROL_PARAMCHANGE = 6;
public static readonly nint SERVICE_CONTROL_NETBINDADD = 7;
public static readonly nint SERVICE_CONTROL_NETBINDREMOVE = 8;
public static readonly nint SERVICE_CONTROL_NETBINDENABLE = 9;
public static readonly nint SERVICE_CONTROL_NETBINDDISABLE = 10;
public static readonly nint SERVICE_CONTROL_DEVICEEVENT = 11;
public static readonly nint SERVICE_CONTROL_HARDWAREPROFILECHANGE = 12;
public static readonly nint SERVICE_CONTROL_POWEREVENT = 13;
public static readonly nint SERVICE_CONTROL_SESSIONCHANGE = 14;
public static readonly nint SERVICE_CONTROL_PRESHUTDOWN = 15;

public static readonly nint SERVICE_ACTIVE = 1;
public static readonly nint SERVICE_INACTIVE = 2;
public static readonly nint SERVICE_STATE_ALL = 3;

public static readonly nint SERVICE_QUERY_CONFIG = 1;
public static readonly nint SERVICE_CHANGE_CONFIG = 2;
public static readonly nint SERVICE_QUERY_STATUS = 4;
public static readonly nint SERVICE_ENUMERATE_DEPENDENTS = 8;
public static readonly nint SERVICE_START = 16;
public static readonly nint SERVICE_STOP = 32;
public static readonly nint SERVICE_PAUSE_CONTINUE = 64;
public static readonly nint SERVICE_INTERROGATE = 128;
public static readonly nint SERVICE_USER_DEFINED_CONTROL = 256;
public static readonly var SERVICE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL;

public static readonly nint SERVICE_RUNS_IN_SYSTEM_PROCESS = 1;

public static readonly nint SERVICE_CONFIG_DESCRIPTION = 1;
public static readonly nint SERVICE_CONFIG_FAILURE_ACTIONS = 2;
public static readonly nint SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3;
public static readonly nint SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 4;
public static readonly nint SERVICE_CONFIG_SERVICE_SID_INFO = 5;
public static readonly nint SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = 6;
public static readonly nint SERVICE_CONFIG_PRESHUTDOWN_INFO = 7;
public static readonly nint SERVICE_CONFIG_TRIGGER_INFO = 8;
public static readonly nint SERVICE_CONFIG_PREFERRED_NODE = 9;
public static readonly nint SERVICE_CONFIG_LAUNCH_PROTECTED = 12;

public static readonly nint SERVICE_SID_TYPE_NONE = 0;
public static readonly nint SERVICE_SID_TYPE_UNRESTRICTED = 1;
public static readonly nint SERVICE_SID_TYPE_RESTRICTED = 2 | SERVICE_SID_TYPE_UNRESTRICTED;

public static readonly nint SC_ENUM_PROCESS_INFO = 0;

public static readonly nint SERVICE_NOTIFY_STATUS_CHANGE = 2;
public static readonly nuint SERVICE_NOTIFY_STOPPED = 0x00000001;
public static readonly nuint SERVICE_NOTIFY_START_PENDING = 0x00000002;
public static readonly nuint SERVICE_NOTIFY_STOP_PENDING = 0x00000004;
public static readonly nuint SERVICE_NOTIFY_RUNNING = 0x00000008;
public static readonly nuint SERVICE_NOTIFY_CONTINUE_PENDING = 0x00000010;
public static readonly nuint SERVICE_NOTIFY_PAUSE_PENDING = 0x00000020;
public static readonly nuint SERVICE_NOTIFY_PAUSED = 0x00000040;
public static readonly nuint SERVICE_NOTIFY_CREATED = 0x00000080;
public static readonly nuint SERVICE_NOTIFY_DELETED = 0x00000100;
public static readonly nuint SERVICE_NOTIFY_DELETE_PENDING = 0x00000200;

public static readonly nint SC_EVENT_DATABASE_CHANGE = 0;
public static readonly nint SC_EVENT_PROPERTY_CHANGE = 1;
public static readonly nint SC_EVENT_STATUS_CHANGE = 2;

public partial struct SERVICE_STATUS {
    public uint ServiceType;
    public uint CurrentState;
    public uint ControlsAccepted;
    public uint Win32ExitCode;
    public uint ServiceSpecificExitCode;
    public uint CheckPoint;
    public uint WaitHint;
}

public partial struct SERVICE_TABLE_ENTRY {
    public ptr<ushort> ServiceName;
    public System.UIntPtr ServiceProc;
}

public partial struct QUERY_SERVICE_CONFIG {
    public uint ServiceType;
    public uint StartType;
    public uint ErrorControl;
    public ptr<ushort> BinaryPathName;
    public ptr<ushort> LoadOrderGroup;
    public uint TagId;
    public ptr<ushort> Dependencies;
    public ptr<ushort> ServiceStartName;
    public ptr<ushort> DisplayName;
}

public partial struct SERVICE_DESCRIPTION {
    public ptr<ushort> Description;
}

public partial struct SERVICE_DELAYED_AUTO_START_INFO {
    public uint IsDelayedAutoStartUp;
}

public partial struct SERVICE_STATUS_PROCESS {
    public uint ServiceType;
    public uint CurrentState;
    public uint ControlsAccepted;
    public uint Win32ExitCode;
    public uint ServiceSpecificExitCode;
    public uint CheckPoint;
    public uint WaitHint;
    public uint ProcessId;
    public uint ServiceFlags;
}

public partial struct ENUM_SERVICE_STATUS_PROCESS {
    public ptr<ushort> ServiceName;
    public ptr<ushort> DisplayName;
    public SERVICE_STATUS_PROCESS ServiceStatusProcess;
}

public partial struct SERVICE_NOTIFY {
    public uint Version;
    public System.UIntPtr NotifyCallback;
    public System.UIntPtr Context;
    public uint NotificationStatus;
    public SERVICE_STATUS_PROCESS ServiceStatus;
    public uint NotificationTriggered;
    public ptr<ushort> ServiceNames;
}

public partial struct SERVICE_FAILURE_ACTIONS {
    public uint ResetPeriod;
    public ptr<ushort> RebootMsg;
    public ptr<ushort> Command;
    public uint ActionsCount;
    public ptr<SC_ACTION> Actions;
}

public partial struct SC_ACTION {
    public uint Type;
    public uint Delay;
}

public partial struct QUERY_SERVICE_LOCK_STATUS {
    public uint IsLocked;
    public ptr<ushort> LockOwner;
    public uint LockDuration;
}

//sys    CloseServiceHandle(handle Handle) (err error) = advapi32.CloseServiceHandle
//sys    CreateService(mgr Handle, serviceName *uint16, displayName *uint16, access uint32, srvType uint32, startType uint32, errCtl uint32, pathName *uint16, loadOrderGroup *uint16, tagId *uint32, dependencies *uint16, serviceStartName *uint16, password *uint16) (handle Handle, err error) [failretval==0] = advapi32.CreateServiceW
//sys    OpenService(mgr Handle, serviceName *uint16, access uint32) (handle Handle, err error) [failretval==0] = advapi32.OpenServiceW
//sys    DeleteService(service Handle) (err error) = advapi32.DeleteService
//sys    StartService(service Handle, numArgs uint32, argVectors **uint16) (err error) = advapi32.StartServiceW
//sys    QueryServiceStatus(service Handle, status *SERVICE_STATUS) (err error) = advapi32.QueryServiceStatus
//sys    QueryServiceLockStatus(mgr Handle, lockStatus *QUERY_SERVICE_LOCK_STATUS, bufSize uint32, bytesNeeded *uint32) (err error) = advapi32.QueryServiceLockStatusW
//sys    ControlService(service Handle, control uint32, status *SERVICE_STATUS) (err error) = advapi32.ControlService
//sys    StartServiceCtrlDispatcher(serviceTable *SERVICE_TABLE_ENTRY) (err error) = advapi32.StartServiceCtrlDispatcherW
//sys    SetServiceStatus(service Handle, serviceStatus *SERVICE_STATUS) (err error) = advapi32.SetServiceStatus
//sys    ChangeServiceConfig(service Handle, serviceType uint32, startType uint32, errorControl uint32, binaryPathName *uint16, loadOrderGroup *uint16, tagId *uint32, dependencies *uint16, serviceStartName *uint16, password *uint16, displayName *uint16) (err error) = advapi32.ChangeServiceConfigW
//sys    QueryServiceConfig(service Handle, serviceConfig *QUERY_SERVICE_CONFIG, bufSize uint32, bytesNeeded *uint32) (err error) = advapi32.QueryServiceConfigW
//sys    ChangeServiceConfig2(service Handle, infoLevel uint32, info *byte) (err error) = advapi32.ChangeServiceConfig2W
//sys    QueryServiceConfig2(service Handle, infoLevel uint32, buff *byte, buffSize uint32, bytesNeeded *uint32) (err error) = advapi32.QueryServiceConfig2W
//sys    EnumServicesStatusEx(mgr Handle, infoLevel uint32, serviceType uint32, serviceState uint32, services *byte, bufSize uint32, bytesNeeded *uint32, servicesReturned *uint32, resumeHandle *uint32, groupName *uint16) (err error) = advapi32.EnumServicesStatusExW
//sys    QueryServiceStatusEx(service Handle, infoLevel uint32, buff *byte, buffSize uint32, bytesNeeded *uint32) (err error) = advapi32.QueryServiceStatusEx
//sys    NotifyServiceStatusChange(service Handle, notifyMask uint32, notifier *SERVICE_NOTIFY) (ret error) = advapi32.NotifyServiceStatusChangeW
//sys    SubscribeServiceChangeNotifications(service Handle, eventType uint32, callback uintptr, callbackCtx uintptr, subscription *uintptr) (ret error) = sechost.SubscribeServiceChangeNotifications?
//sys    UnsubscribeServiceChangeNotifications(subscription uintptr) = sechost.UnsubscribeServiceChangeNotifications?

} // end windows_package
