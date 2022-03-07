// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:27:19 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\types_windows_arm.go


namespace go;

public static partial class syscall_package {

public partial struct WSAData {
    public ushort Version;
    public ushort HighVersion;
    public array<byte> Description;
    public array<byte> SystemStatus;
    public ushort MaxSockets;
    public ushort MaxUdpDg;
    public ptr<byte> VendorInfo;
}

public partial struct Servent {
    public ptr<byte> Name;
    public ptr<ptr<byte>> Aliases;
    public ushort Port;
    public ptr<byte> Proto;
}

} // end syscall_package
