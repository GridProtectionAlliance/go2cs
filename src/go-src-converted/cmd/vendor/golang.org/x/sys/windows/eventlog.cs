// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package windows -- go2cs converted at 2020 October 09 06:00:49 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\eventlog.go

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
        public static readonly long EVENTLOG_SUCCESS = (long)0L;
        public static readonly long EVENTLOG_ERROR_TYPE = (long)1L;
        public static readonly long EVENTLOG_WARNING_TYPE = (long)2L;
        public static readonly long EVENTLOG_INFORMATION_TYPE = (long)4L;
        public static readonly long EVENTLOG_AUDIT_SUCCESS = (long)8L;
        public static readonly long EVENTLOG_AUDIT_FAILURE = (long)16L;


        //sys    RegisterEventSource(uncServerName *uint16, sourceName *uint16) (handle Handle, err error) [failretval==0] = advapi32.RegisterEventSourceW
        //sys    DeregisterEventSource(handle Handle) (err error) = advapi32.DeregisterEventSource
        //sys    ReportEvent(log Handle, etype uint16, category uint16, eventId uint32, usrSId uintptr, numStrings uint16, dataSize uint32, strings **uint16, rawData *byte) (err error) = advapi32.ReportEventW
    }
}}}}}}
