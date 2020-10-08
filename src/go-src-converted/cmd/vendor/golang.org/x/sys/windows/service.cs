// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package windows -- go2cs converted at 2020 October 08 04:53:49 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\service.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        public static readonly long SC_MANAGER_CONNECT = (long)1L;
        public static readonly long SC_MANAGER_CREATE_SERVICE = (long)2L;
        public static readonly long SC_MANAGER_ENUMERATE_SERVICE = (long)4L;
        public static readonly long SC_MANAGER_LOCK = (long)8L;
        public static readonly long SC_MANAGER_QUERY_LOCK_STATUS = (long)16L;
        public static readonly long SC_MANAGER_MODIFY_BOOT_CONFIG = (long)32L;
        public static readonly ulong SC_MANAGER_ALL_ACCESS = (ulong)0xf003fUL;


        //sys    OpenSCManager(machineName *uint16, databaseName *uint16, access uint32) (handle Handle, err error) [failretval==0] = advapi32.OpenSCManagerW

        public static readonly long SERVICE_KERNEL_DRIVER = (long)1L;
        public static readonly long SERVICE_FILE_SYSTEM_DRIVER = (long)2L;
        public static readonly long SERVICE_ADAPTER = (long)4L;
        public static readonly long SERVICE_RECOGNIZER_DRIVER = (long)8L;
        public static readonly long SERVICE_WIN32_OWN_PROCESS = (long)16L;
        public static readonly long SERVICE_WIN32_SHARE_PROCESS = (long)32L;
        public static readonly var SERVICE_WIN32 = (var)SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS;
        public static readonly long SERVICE_INTERACTIVE_PROCESS = (long)256L;
        public static readonly var SERVICE_DRIVER = (var)SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER;
        public static readonly var SERVICE_TYPE_ALL = (var)SERVICE_WIN32 | SERVICE_ADAPTER | SERVICE_DRIVER | SERVICE_INTERACTIVE_PROCESS;

        public static readonly long SERVICE_BOOT_START = (long)0L;
        public static readonly long SERVICE_SYSTEM_START = (long)1L;
        public static readonly long SERVICE_AUTO_START = (long)2L;
        public static readonly long SERVICE_DEMAND_START = (long)3L;
        public static readonly long SERVICE_DISABLED = (long)4L;

        public static readonly long SERVICE_ERROR_IGNORE = (long)0L;
        public static readonly long SERVICE_ERROR_NORMAL = (long)1L;
        public static readonly long SERVICE_ERROR_SEVERE = (long)2L;
        public static readonly long SERVICE_ERROR_CRITICAL = (long)3L;

        public static readonly long SC_STATUS_PROCESS_INFO = (long)0L;

        public static readonly long SC_ACTION_NONE = (long)0L;
        public static readonly long SC_ACTION_RESTART = (long)1L;
        public static readonly long SC_ACTION_REBOOT = (long)2L;
        public static readonly long SC_ACTION_RUN_COMMAND = (long)3L;

        public static readonly long SERVICE_STOPPED = (long)1L;
        public static readonly long SERVICE_START_PENDING = (long)2L;
        public static readonly long SERVICE_STOP_PENDING = (long)3L;
        public static readonly long SERVICE_RUNNING = (long)4L;
        public static readonly long SERVICE_CONTINUE_PENDING = (long)5L;
        public static readonly long SERVICE_PAUSE_PENDING = (long)6L;
        public static readonly long SERVICE_PAUSED = (long)7L;
        public static readonly ulong SERVICE_NO_CHANGE = (ulong)0xffffffffUL;

        public static readonly long SERVICE_ACCEPT_STOP = (long)1L;
        public static readonly long SERVICE_ACCEPT_PAUSE_CONTINUE = (long)2L;
        public static readonly long SERVICE_ACCEPT_SHUTDOWN = (long)4L;
        public static readonly long SERVICE_ACCEPT_PARAMCHANGE = (long)8L;
        public static readonly long SERVICE_ACCEPT_NETBINDCHANGE = (long)16L;
        public static readonly long SERVICE_ACCEPT_HARDWAREPROFILECHANGE = (long)32L;
        public static readonly long SERVICE_ACCEPT_POWEREVENT = (long)64L;
        public static readonly long SERVICE_ACCEPT_SESSIONCHANGE = (long)128L;

        public static readonly long SERVICE_CONTROL_STOP = (long)1L;
        public static readonly long SERVICE_CONTROL_PAUSE = (long)2L;
        public static readonly long SERVICE_CONTROL_CONTINUE = (long)3L;
        public static readonly long SERVICE_CONTROL_INTERROGATE = (long)4L;
        public static readonly long SERVICE_CONTROL_SHUTDOWN = (long)5L;
        public static readonly long SERVICE_CONTROL_PARAMCHANGE = (long)6L;
        public static readonly long SERVICE_CONTROL_NETBINDADD = (long)7L;
        public static readonly long SERVICE_CONTROL_NETBINDREMOVE = (long)8L;
        public static readonly long SERVICE_CONTROL_NETBINDENABLE = (long)9L;
        public static readonly long SERVICE_CONTROL_NETBINDDISABLE = (long)10L;
        public static readonly long SERVICE_CONTROL_DEVICEEVENT = (long)11L;
        public static readonly long SERVICE_CONTROL_HARDWAREPROFILECHANGE = (long)12L;
        public static readonly long SERVICE_CONTROL_POWEREVENT = (long)13L;
        public static readonly long SERVICE_CONTROL_SESSIONCHANGE = (long)14L;

        public static readonly long SERVICE_ACTIVE = (long)1L;
        public static readonly long SERVICE_INACTIVE = (long)2L;
        public static readonly long SERVICE_STATE_ALL = (long)3L;

        public static readonly long SERVICE_QUERY_CONFIG = (long)1L;
        public static readonly long SERVICE_CHANGE_CONFIG = (long)2L;
        public static readonly long SERVICE_QUERY_STATUS = (long)4L;
        public static readonly long SERVICE_ENUMERATE_DEPENDENTS = (long)8L;
        public static readonly long SERVICE_START = (long)16L;
        public static readonly long SERVICE_STOP = (long)32L;
        public static readonly long SERVICE_PAUSE_CONTINUE = (long)64L;
        public static readonly long SERVICE_INTERROGATE = (long)128L;
        public static readonly long SERVICE_USER_DEFINED_CONTROL = (long)256L;
        public static readonly var SERVICE_ALL_ACCESS = (var)STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL;

        public static readonly long SERVICE_RUNS_IN_SYSTEM_PROCESS = (long)1L;

        public static readonly long SERVICE_CONFIG_DESCRIPTION = (long)1L;
        public static readonly long SERVICE_CONFIG_FAILURE_ACTIONS = (long)2L;
        public static readonly long SERVICE_CONFIG_DELAYED_AUTO_START_INFO = (long)3L;
        public static readonly long SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = (long)4L;
        public static readonly long SERVICE_CONFIG_SERVICE_SID_INFO = (long)5L;
        public static readonly long SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = (long)6L;
        public static readonly long SERVICE_CONFIG_PRESHUTDOWN_INFO = (long)7L;
        public static readonly long SERVICE_CONFIG_TRIGGER_INFO = (long)8L;
        public static readonly long SERVICE_CONFIG_PREFERRED_NODE = (long)9L;
        public static readonly long SERVICE_CONFIG_LAUNCH_PROTECTED = (long)12L;

        public static readonly long SERVICE_SID_TYPE_NONE = (long)0L;
        public static readonly long SERVICE_SID_TYPE_UNRESTRICTED = (long)1L;
        public static readonly long SERVICE_SID_TYPE_RESTRICTED = (long)2L | SERVICE_SID_TYPE_UNRESTRICTED;

        public static readonly long SC_ENUM_PROCESS_INFO = (long)0L;

        public static readonly long SERVICE_NOTIFY_STATUS_CHANGE = (long)2L;
        public static readonly ulong SERVICE_NOTIFY_STOPPED = (ulong)0x00000001UL;
        public static readonly ulong SERVICE_NOTIFY_START_PENDING = (ulong)0x00000002UL;
        public static readonly ulong SERVICE_NOTIFY_STOP_PENDING = (ulong)0x00000004UL;
        public static readonly ulong SERVICE_NOTIFY_RUNNING = (ulong)0x00000008UL;
        public static readonly ulong SERVICE_NOTIFY_CONTINUE_PENDING = (ulong)0x00000010UL;
        public static readonly ulong SERVICE_NOTIFY_PAUSE_PENDING = (ulong)0x00000020UL;
        public static readonly ulong SERVICE_NOTIFY_PAUSED = (ulong)0x00000040UL;
        public static readonly ulong SERVICE_NOTIFY_CREATED = (ulong)0x00000080UL;
        public static readonly ulong SERVICE_NOTIFY_DELETED = (ulong)0x00000100UL;
        public static readonly ulong SERVICE_NOTIFY_DELETE_PENDING = (ulong)0x00000200UL;


        public partial struct SERVICE_STATUS
        {
            public uint ServiceType;
            public uint CurrentState;
            public uint ControlsAccepted;
            public uint Win32ExitCode;
            public uint ServiceSpecificExitCode;
            public uint CheckPoint;
            public uint WaitHint;
        }

        public partial struct SERVICE_TABLE_ENTRY
        {
            public ptr<ushort> ServiceName;
            public System.UIntPtr ServiceProc;
        }

        public partial struct QUERY_SERVICE_CONFIG
        {
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

        public partial struct SERVICE_DESCRIPTION
        {
            public ptr<ushort> Description;
        }

        public partial struct SERVICE_DELAYED_AUTO_START_INFO
        {
            public uint IsDelayedAutoStartUp;
        }

        public partial struct SERVICE_STATUS_PROCESS
        {
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

        public partial struct ENUM_SERVICE_STATUS_PROCESS
        {
            public ptr<ushort> ServiceName;
            public ptr<ushort> DisplayName;
            public SERVICE_STATUS_PROCESS ServiceStatusProcess;
        }

        public partial struct SERVICE_NOTIFY
        {
            public uint Version;
            public System.UIntPtr NotifyCallback;
            public System.UIntPtr Context;
            public uint NotificationStatus;
            public SERVICE_STATUS_PROCESS ServiceStatus;
            public uint NotificationTriggered;
            public ptr<ushort> ServiceNames;
        }

        public partial struct SERVICE_FAILURE_ACTIONS
        {
            public uint ResetPeriod;
            public ptr<ushort> RebootMsg;
            public ptr<ushort> Command;
            public uint ActionsCount;
            public ptr<SC_ACTION> Actions;
        }

        public partial struct SC_ACTION
        {
            public uint Type;
            public uint Delay;
        }

        public partial struct QUERY_SERVICE_LOCK_STATUS
        {
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
    }
}}}}}}
