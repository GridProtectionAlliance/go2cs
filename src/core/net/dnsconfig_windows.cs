// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using time = time_package;
using @internal.syscall;

partial class net_package {

internal static ж<dnsConfig> /*conf*/ dnsReadConfig(@string ignoredFilename) => func((defer, _) => {
    ж<dnsConfig> conf = default!;

    conf = Ꮡ(new dnsConfig(
        ndots: 1,
        timeout: 5 * time.Second,
        attempts: 2
    ));
    var defaultNSʗ1 = defaultNS;
    defer(() => {
        if (len((~conf).servers) == 0) {
            conf.val.servers = defaultNSʗ1;
        }
    });
    (aas, err) = adapterAddresses();
    if (err != default!) {
        return conf;
    }
    foreach (var (_, aa) in aas) {
        // Only take interfaces whose OperStatus is IfOperStatusUp(0x01) into DNS configs.
        if ((~aa).OperStatus != windows.IfOperStatusUp) {
            continue;
        }
        // Only take interfaces which have at least one gateway
        if ((~aa).FirstGatewayAddress == nil) {
            continue;
        }
        for (var dns = aa.val.FirstDnsServerAddress; dns != nil; dns = dns.val.Next) {
            (sa, errΔ1) = (~dns).Address.Sockaddr.Sockaddr();
            if (errΔ1 != default!) {
                continue;
            }
            IP ip = default!;
            switch (sa.type()) {
            case ж<syscall.SockaddrInet4> sa: {
                ip = IPv4((~sa).Addr[0], (~sa).Addr[1], (~sa).Addr[2], (~sa).Addr[3]);
                break;
            }
            case ж<syscall.SockaddrInet6> sa: {
                ip = new IP(IPv6len);
                copy(ip, (~sa).Addr[..]);
                if (ip[0] == 254 && ip[1] == 192) {
                    // fec0/10 IPv6 addresses are site local anycast DNS
                    // addresses Microsoft sets by default if no other
                    // IPv6 DNS address is set. Site local anycast is
                    // deprecated since 2004, see
                    // https://datatracker.ietf.org/doc/html/rfc3879
                    continue;
                }
                break;
            }
            default: {
                var sa = sa.type();
                continue;
                break;
            }}
            // Unexpected type.
            conf.val.servers = append((~conf).servers, JoinHostPort(ip.String(), "53"u8));
        }
    }
    return conf;
});

} // end net_package
