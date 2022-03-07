// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2022 March 06 22:15:55 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\interface.go


namespace go.golang.org.x.net;

public static partial class route_package {

    // An InterfaceMessage represents an interface message.
public partial struct InterfaceMessage {
    public nint Version; // message version
    public nint Type; // message type
    public nint Flags; // interface flags
    public nint Index; // interface index
    public @string Name; // interface name
    public slice<Addr> Addrs; // addresses

    public nint extOff; // offset of header extension
    public slice<byte> raw; // raw message
}

// An InterfaceAddrMessage represents an interface address message.
public partial struct InterfaceAddrMessage {
    public nint Version; // message version
    public nint Type; // message type
    public nint Flags; // interface flags
    public nint Index; // interface index
    public slice<Addr> Addrs; // addresses

    public slice<byte> raw; // raw message
}

// Sys implements the Sys method of Message interface.
private static slice<Sys> Sys(this ptr<InterfaceAddrMessage> _addr_m) {
    ref InterfaceAddrMessage m = ref _addr_m.val;

    return null;
}

// An InterfaceMulticastAddrMessage represents an interface multicast
// address message.
public partial struct InterfaceMulticastAddrMessage {
    public nint Version; // message version
    public nint Type; // message type
    public nint Flags; // interface flags
    public nint Index; // interface index
    public slice<Addr> Addrs; // addresses

    public slice<byte> raw; // raw message
}

// Sys implements the Sys method of Message interface.
private static slice<Sys> Sys(this ptr<InterfaceMulticastAddrMessage> _addr_m) {
    ref InterfaceMulticastAddrMessage m = ref _addr_m.val;

    return null;
}

// An InterfaceAnnounceMessage represents an interface announcement
// message.
public partial struct InterfaceAnnounceMessage {
    public nint Version; // message version
    public nint Type; // message type
    public nint Index; // interface index
    public @string Name; // interface name
    public nint What; // what type of announcement

    public slice<byte> raw; // raw message
}

// Sys implements the Sys method of Message interface.
private static slice<Sys> Sys(this ptr<InterfaceAnnounceMessage> _addr_m) {
    ref InterfaceAnnounceMessage m = ref _addr_m.val;

    return null;
}

} // end route_package
