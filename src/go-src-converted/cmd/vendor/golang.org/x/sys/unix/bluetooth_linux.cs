// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Bluetooth sockets and messages

// package unix -- go2cs converted at 2022 March 06 23:26:29 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\bluetooth_linux.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Bluetooth Protocols
public static readonly nint BTPROTO_L2CAP = 0;
public static readonly nint BTPROTO_HCI = 1;
public static readonly nint BTPROTO_SCO = 2;
public static readonly nint BTPROTO_RFCOMM = 3;
public static readonly nint BTPROTO_BNEP = 4;
public static readonly nint BTPROTO_CMTP = 5;
public static readonly nint BTPROTO_HIDP = 6;
public static readonly nint BTPROTO_AVDTP = 7;


public static readonly nint HCI_CHANNEL_RAW = 0;
public static readonly nint HCI_CHANNEL_USER = 1;
public static readonly nint HCI_CHANNEL_MONITOR = 2;
public static readonly nint HCI_CHANNEL_CONTROL = 3;
public static readonly nint HCI_CHANNEL_LOGGING = 4;


// Socketoption Level
public static readonly nuint SOL_BLUETOOTH = 0x112;
public static readonly nuint SOL_HCI = 0x0;
public static readonly nuint SOL_L2CAP = 0x6;
public static readonly nuint SOL_RFCOMM = 0x12;
public static readonly nuint SOL_SCO = 0x11;


} // end unix_package
