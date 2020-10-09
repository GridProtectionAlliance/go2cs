// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Bluetooth sockets and messages

// package unix -- go2cs converted at 2020 October 09 05:56:11 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\bluetooth_linux.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // Bluetooth Protocols
        public static readonly long BTPROTO_L2CAP = (long)0L;
        public static readonly long BTPROTO_HCI = (long)1L;
        public static readonly long BTPROTO_SCO = (long)2L;
        public static readonly long BTPROTO_RFCOMM = (long)3L;
        public static readonly long BTPROTO_BNEP = (long)4L;
        public static readonly long BTPROTO_CMTP = (long)5L;
        public static readonly long BTPROTO_HIDP = (long)6L;
        public static readonly long BTPROTO_AVDTP = (long)7L;


        public static readonly long HCI_CHANNEL_RAW = (long)0L;
        public static readonly long HCI_CHANNEL_USER = (long)1L;
        public static readonly long HCI_CHANNEL_MONITOR = (long)2L;
        public static readonly long HCI_CHANNEL_CONTROL = (long)3L;
        public static readonly long HCI_CHANNEL_LOGGING = (long)4L;


        // Socketoption Level
        public static readonly ulong SOL_BLUETOOTH = (ulong)0x112UL;
        public static readonly ulong SOL_HCI = (ulong)0x0UL;
        public static readonly ulong SOL_L2CAP = (ulong)0x6UL;
        public static readonly ulong SOL_RFCOMM = (ulong)0x12UL;
        public static readonly ulong SOL_SCO = (ulong)0x11UL;

    }
}}}}}}
