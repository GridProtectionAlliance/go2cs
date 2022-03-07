// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 23:30:34 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\memory_windows.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public static readonly nuint MEM_COMMIT = 0x00001000;
public static readonly nuint MEM_RESERVE = 0x00002000;
public static readonly nuint MEM_DECOMMIT = 0x00004000;
public static readonly nuint MEM_RELEASE = 0x00008000;
public static readonly nuint MEM_RESET = 0x00080000;
public static readonly nuint MEM_TOP_DOWN = 0x00100000;
public static readonly nuint MEM_WRITE_WATCH = 0x00200000;
public static readonly nuint MEM_PHYSICAL = 0x00400000;
public static readonly nuint MEM_RESET_UNDO = 0x01000000;
public static readonly nuint MEM_LARGE_PAGES = 0x20000000;

public static readonly nuint PAGE_NOACCESS = 0x00000001;
public static readonly nuint PAGE_READONLY = 0x00000002;
public static readonly nuint PAGE_READWRITE = 0x00000004;
public static readonly nuint PAGE_WRITECOPY = 0x00000008;
public static readonly nuint PAGE_EXECUTE = 0x00000010;
public static readonly nuint PAGE_EXECUTE_READ = 0x00000020;
public static readonly nuint PAGE_EXECUTE_READWRITE = 0x00000040;
public static readonly nuint PAGE_EXECUTE_WRITECOPY = 0x00000080;
public static readonly nuint PAGE_GUARD = 0x00000100;
public static readonly nuint PAGE_NOCACHE = 0x00000200;
public static readonly nuint PAGE_WRITECOMBINE = 0x00000400;
public static readonly nuint PAGE_TARGETS_INVALID = 0x40000000;
public static readonly nuint PAGE_TARGETS_NO_UPDATE = 0x40000000;

public static readonly nuint QUOTA_LIMITS_HARDWS_MIN_DISABLE = 0x00000002;
public static readonly nuint QUOTA_LIMITS_HARDWS_MIN_ENABLE = 0x00000001;
public static readonly nuint QUOTA_LIMITS_HARDWS_MAX_DISABLE = 0x00000008;
public static readonly nuint QUOTA_LIMITS_HARDWS_MAX_ENABLE = 0x00000004;


} // end windows_package
