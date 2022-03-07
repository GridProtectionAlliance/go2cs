// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package windows -- go2cs converted at 2022 March 06 23:30:33 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\eventlog.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public static readonly nint EVENTLOG_SUCCESS = 0;
public static readonly nint EVENTLOG_ERROR_TYPE = 1;
public static readonly nint EVENTLOG_WARNING_TYPE = 2;
public static readonly nint EVENTLOG_INFORMATION_TYPE = 4;
public static readonly nint EVENTLOG_AUDIT_SUCCESS = 8;
public static readonly nint EVENTLOG_AUDIT_FAILURE = 16;


//sys    RegisterEventSource(uncServerName *uint16, sourceName *uint16) (handle Handle, err error) [failretval==0] = advapi32.RegisterEventSourceW
//sys    DeregisterEventSource(handle Handle) (err error) = advapi32.DeregisterEventSource
//sys    ReportEvent(log Handle, etype uint16, category uint16, eventId uint32, usrSId uintptr, numStrings uint16, dataSize uint32, strings **uint16, rawData *byte) (err error) = advapi32.ReportEventW

} // end windows_package
