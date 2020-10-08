// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:32 UTC
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
            slice<Interface> _p0 = default;
            error _p0 = default!;

            if (ifindex == 0L)
            {
                var (n, err) = interfaceCount();
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                var ifcs = make_slice<Interface>(n);
                foreach (var (i) in ifcs)
                {
                    var (ifc, err) = readInterface(i);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }
                    ifcs[i] = ifc.val;

                }                return (ifcs, error.As(null!)!);

            }
            (ifc, err) = readInterface(ifindex - 1L);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            return (new slice<Interface>(new Interface[] { *ifc }), error.As(null!)!);

        }

        private static (ptr<Interface>, error) readInterface(long i) => func((defer, _, __) =>
        {
            ptr<Interface> _p0 = default!;
            error _p0 = default!;

            ptr<Interface> ifc = addr(new Interface(Index:i+1,Name:netdir+"/ipifc/"+itoa(i),));

            var ifcstat = ifc.Name + "/status";
            var (ifcstatf, err) = open(ifcstat);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            defer(ifcstatf.close());

            var (line, ok) = ifcstatf.readLine();
            if (!ok)
            {
                return (_addr_null!, error.As(errors.New("invalid interface status file: " + ifcstat))!);
            }

            var fields = getFields(line);
            if (len(fields) < 4L)
            {
                return (_addr_null!, error.As(errors.New("invalid interface status file: " + ifcstat))!);
            }

            var device = fields[1L];
            var mtustr = fields[3L];

            var (mtu, _, ok) = dtoi(mtustr);
            if (!ok)
            {
                return (_addr_null!, error.As(errors.New("invalid status file of interface: " + ifcstat))!);
            }

            ifc.MTU = mtu; 

            // Not a loopback device ("/dev/null") or packet interface (e.g. "pkt2")
            if (stringsHasPrefix(device, netdir + "/"))
            {
                var (deviceaddrf, err) = open(device + "/addr");
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                defer(deviceaddrf.close());

                line, ok = deviceaddrf.readLine();
                if (!ok)
                {
                    return (_addr_null!, error.As(errors.New("invalid address file for interface: " + device + "/addr"))!);
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

            return (_addr_ifc!, error.As(null!)!);

        });

        private static (long, error) interfaceCount() => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;

            var (d, err) = os.Open(netdir + "/ipifc");
            if (err != null)
            {
                return (-1L, error.As(err)!);
            }

            defer(d.Close());

            var (names, err) = d.Readdirnames(0L);
            if (err != null)
            {
                return (-1L, error.As(err)!);
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
            return (c, error.As(null!)!);

        });

        // If the ifi is nil, interfaceAddrTable returns addresses for all
        // network interfaces. Otherwise it returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceAddrTable(ptr<Interface> _addr_ifi) => func((defer, _, __) =>
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            slice<Interface> ifcs = default;
            if (ifi == null)
            {
                error err = default!;
                ifcs, err = interfaceTable(0L);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }
            else
            {
                ifcs = new slice<Interface>(new Interface[] { *ifi });
            }

            slice<Addr> addrs = default;
            foreach (var (_, ifc) in ifcs)
            {
                var status = ifc.Name + "/status";
                var (statusf, err) = open(status);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                defer(statusf.close()); 

                // Read but ignore first line as it only contains the table header.
                // See https://9p.io/magic/man2html/3/ip
                {
                    var (_, ok) = statusf.readLine();

                    if (!ok)
                    {
                        return (null, error.As(errors.New("cannot read header line for interface: " + status))!);
                    }

                }


                {
                    var (line, ok) = statusf.readLine();

                    while (ok)
                    {
                        var fields = getFields(line);
                        if (len(fields) < 1L)
                        {
                            return (null, error.As(errors.New("cannot parse IP address for interface: " + status))!);
                        line, ok = statusf.readLine();
                        }

                        var addr = fields[0L];
                        var ip = ParseIP(addr);
                        if (ip == null)
                        {
                            return (null, error.As(errors.New("cannot parse IP address for interface: " + status))!);
                        } 

                        // The mask is represented as CIDR relative to the IPv6 address.
                        // Plan 9 internal representation is always IPv6.
                        var maskfld = fields[1L];
                        maskfld = maskfld[1L..];
                        var (pfxlen, _, ok) = dtoi(maskfld);
                        if (!ok)
                        {
                            return (null, error.As(errors.New("cannot parse network mask for interface: " + status))!);
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

                        addrs = append(addrs, addr(new IPNet(IP:ip,Mask:mask)));

                    }

                }

            }
            return (addrs, error.As(null!)!);

        });

        // interfaceMulticastAddrTable returns addresses for a specific
        // interface.
        private static (slice<Addr>, error) interfaceMulticastAddrTable(ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            return (null, error.As(null!)!);
        }
    }
}
