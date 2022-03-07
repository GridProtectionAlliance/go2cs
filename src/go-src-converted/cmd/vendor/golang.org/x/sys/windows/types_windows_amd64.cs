// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 23:30:43 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\types_windows_amd64.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public partial struct WSAData {
    public ushort Version;
    public ushort HighVersion;
    public ushort MaxSockets;
    public ushort MaxUdpDg;
    public ptr<byte> VendorInfo;
    public array<byte> Description;
    public array<byte> SystemStatus;
}

public partial struct Servent {
    public ptr<byte> Name;
    public ptr<ptr<byte>> Aliases;
    public ptr<byte> Proto;
    public ushort Port;
}

public partial struct JOBOBJECT_BASIC_LIMIT_INFORMATION {
    public long PerProcessUserTimeLimit;
    public long PerJobUserTimeLimit;
    public uint LimitFlags;
    public System.UIntPtr MinimumWorkingSetSize;
    public System.UIntPtr MaximumWorkingSetSize;
    public uint ActiveProcessLimit;
    public System.UIntPtr Affinity;
    public uint PriorityClass;
    public uint SchedulingClass;
}

} // end windows_package
