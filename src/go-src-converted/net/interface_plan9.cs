// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:36 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface_plan9.go
using errors = go.errors_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // If the ifindex is zero, interfaceTable returns mappings of all
        // network interfaces. Otherwise it returns a mapping of a specific
        // interface.
        private static (slice<Interface>, error) interfaceTable(long ifindex)
        {
            if (ifindex == 0L)
            {
                var (n, err) = interfaceCount();
                if (err != null)
                {
                    return (null, err);
                }
                var ifcs = make_slice<Interface>(n);
                foreach (var (i) in ifcs)
                {
                    var (ifc, err) = readInterface(i);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    ifcs[i] = ifc.Value;
                }                return (ifcs, null);
            }
            (ifc, err) = readInterface(ifindex - 1L);
            if (err != null)
            {
                return (null, err);
            }
            return (new slice<Interface>(new Interface[] { *ifc }), null);
        }

        private static (ref Interface, error) readInterface(long i) => func((defer, _, __) =>
        {
            Interface ifc = ref new Interface(Index:i+1,Name:netdir+"/ipifc/"+itoa(i),);

            var ifcstat = ifc.Name + "/status";
            var (ifcstatf, err) = open(ifcstat);
            if (err != null)
            {
                return (null, err);
            }
            defer(ifcstatf.close());

            var (line, ok) = ifcstatf.readLine();
            if (!ok)
            {
                return (null, errors.New("invalid interface status file: " + ifcstat));
            }
            var fields = getFields(line);
            if (len(fields) < 4L)
            {
                return (null, errors.New("invalid interface status file: " + ifcstat));
            }
            var device = fields[1L];
            var mtustr = fields[3L];

            var (mtu, _, ok) = dtoi(mtustr);
            if (!ok)
            {
                return (null, errors.New("invalid status file of interface: " + ifcstat));
            }
            ifc.MTU = mtu; 

            // Not a loopback device
            if (device != "/dev/null")
            {
                var (deviceaddrf, err) = open(device + "/addr");
                if (err != null)
                {
                    return (null, err);
                }
                defer(deviceaddrf.close());

                line, ok = deviceaddrf.readLine();
                if (!ok)
                {
                    return (null, errors.New("invalid address file for interface: " + device + "/addr"));
                }
                if (len(line) > 0L && len(line) % 2L == 0L)
                {
                    ifc.HardwareAddr = make_slice<byte>(len(line) / 2L);
                    bool ok = default;
                    foreach (var (i) in ifc.HardwareAddr)
                    {
                        var j = (i + 1L) * 2L;
                        ifc.HardwareAddr[i], ok = xtoi2(line[i * 2L..j], 0L);
                        if (!ok)
                        {
                            ifc.HardwareAddr = ifc.HardwareAddr[..i];
                            break;
                        }
                    }
                }
            else

                ifc.Flags = FlagUp | FlagBroadcast | FlagMulticast;
            }            {
                ifc.Flags = FlagUp | FlagMulticast | FlagLoopback;
            }
            return (ifc, null);
        });

        private static (long, error) interfaceCount() => func((defer, _, __) =>
        {
            var (d, err) = os.Open(netdir + "/ipifc");
            if (err != null)
            {
                return (-1L, err);
            }
            defer(d.Close());

            var (names, err) = d.Readdirnames(0L);
            if (err != null)
            {
                return (-1L, err);
            } 

            // Assumes that numbered files in ipifc are strictly
            // the incrementing numbered directories for the
            // interfaces
            long c = 0L;
            foreach (var (_, name) in names)
            {
                {
                    var (_, _, ok) = dtoi(name);

                    if (!ok)
                    {
                        continue;
                    }

                }
                c++;
            }
            return (c, null);
        });

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ref Interface _ifi) => func(_ifi, (ref Interface ifi, Defer defer, Panic _, Recover __) =>
        {
            slice<Interface> ifcs = default;
            if (ifi == null)
            {
                error err = default;
                ifcs, err = interfaceTable(0L);
                if (err != null)
                {
                    return (null, err);
                }
            }
            else
            {
                ifcs = new slice<Interface>(new Interface[] { *ifi });
            }
            var addrs = make_slice<Addr>(len(ifcs));
            foreach (var (i, ifc) in ifcs)
            {
                var status = ifc.Name + "/status";
                var (statusf, err) = open(status);
                if (err != null)
                {
                    return (null, err);
                }
                defer(statusf.close());

                var (line, ok) = statusf.readLine();
                line, ok = statusf.readLine();
                if (!ok)
                {
                    return (null, errors.New("cannot parse IP address for interface: " + status));
                } 

                // This assumes only a single address for the interface.
                var fields = getFields(line);
                if (len(fields) < 1L)
                {
                    return (null, errors.New("cannot parse IP address for interface: " + status));
                }
                var addr = fields[0L];
                var ip = ParseIP(addr);
                if (ip == null)
                {
                    return (null, errors.New("cannot parse IP address for interface: " + status));
                } 

                // The mask is represented as CIDR relative to the IPv6 address.
                // Plan 9 internal representation is always IPv6.
                var maskfld = fields[1L];
                maskfld = maskfld[1L..];
                var (pfxlen, _, ok) = dtoi(maskfld);
                if (!ok)
                {
                    return (null, errors.New("cannot parse network mask for interface: " + status));
                }
                IPMask mask = default;
                if (ip.To4() != null)
                { // IPv4 or IPv6 IPv4-mapped address
                    mask = CIDRMask(pfxlen - 8L * len(v4InV6Prefix), 8L * IPv4len);
                }
                if (ip.To16() != null && ip.To4() == null)
                { // IPv6 address
                    mask = CIDRMask(pfxlen, 8L * IPv6len);
                }
                addrs[i] = ref new IPNet(IP:ip,Mask:mask);
            }
            return (addrs, null);
        });

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ref Interface ifi)
        {
            return (null, null);
        }
    }
}
