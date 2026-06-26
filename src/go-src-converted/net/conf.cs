// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using godebug = @internal.godebug_package;
using stringslite = @internal.stringslite_package;
using fs = io.fs_package;
using os = os_package;
using runtime = runtime_package;
using sync = sync_package;
using syscall = syscall_package;
using @internal;
using io;

partial class net_package {

// The net package's name resolution is rather complicated.
// There are two main approaches, go and cgo.
// The cgo resolver uses C functions like getaddrinfo.
// The go resolver reads system files directly and
// sends DNS packets directly to servers.
//
// The netgo build tag prefers the go resolver.
// The netcgo build tag prefers the cgo resolver.
//
// The netgo build tag also prohibits the use of the cgo tool.
// However, on Darwin, Plan 9, and Windows the cgo resolver is still available.
// On those systems the cgo resolver does not require the cgo tool.
// (The term "cgo resolver" was locked in by GODEBUG settings
// at a time when the cgo resolver did require the cgo tool.)
//
// Adding netdns=go to GODEBUG will prefer the go resolver.
// Adding netdns=cgo to GODEBUG will prefer the cgo resolver.
//
// The Resolver struct has a PreferGo field that user code
// may set to prefer the go resolver. It is documented as being
// equivalent to adding netdns=go to GODEBUG.
//
// When deciding which resolver to use, we first check the PreferGo field.
// If that is not set, we check the GODEBUG setting.
// If that is not set, we check the netgo or netcgo build tag.
// If none of those are set, we normally prefer the go resolver by default.
// However, if the cgo resolver is available,
// there is a complex set of conditions for which we prefer the cgo resolver.
//
// Other files define the netGoBuildTag, netCgoBuildTag, and cgoAvailable
// constants.

// conf is used to determine name resolution configuration.
[GoType] partial struct conf {
    internal bool netGo; // prefer go approach, based on build tag and GODEBUG
    internal bool netCgo; // prefer cgo approach, based on build tag and GODEBUG
    internal nint dnsDebugLevel; // from GODEBUG
    internal bool preferCgo; // if no explicit preference, use cgo
    internal @string goos;  // copy of runtime.GOOS, used for testing
    internal mdnsTest mdnsTest; // assume /etc/mdns.allow exists, for testing
}

[GoType("num:nint")] partial struct mdnsTest;

internal static readonly mdnsTest mdnsFromSystem = /* iota */ 0;
internal static readonly mdnsTest mdnsAssumeExists = 1;
internal static readonly mdnsTest mdnsAssumeDoesNotExist = 2;

internal static sync.Once confOnce; // guards init of confVal via initConfVal
internal static ж<conf> confVal = Ꮡ(new conf(goos: runtime.GOOS));

// systemConf returns the machine's network configuration.
internal static ж<conf> systemConf() {
    confOnce.Do(initConfVal);
    return confVal;
}

// initConfVal initializes confVal based on the environment
// that will not change during program execution.
internal static void initConfVal() => func((defer, _) => {
    var (dnsMode, debugLevel) = goDebugNetDNS();
    confVal.val.netGo = netGoBuildTag || dnsMode == "go"u8;
    confVal.val.netCgo = netCgoBuildTag || dnsMode == "cgo"u8;
    confVal.val.dnsDebugLevel = debugLevel;
    if ((~confVal).dnsDebugLevel > 0) {
        defer(() => {
            if ((~confVal).dnsDebugLevel > 1) {
                println("go package net: confVal.netCgo =", (~confVal).netCgo, " netGo =", (~confVal).netGo);
            }
            switch (ᐧ) {
            case {} when (~confVal).netGo: {
                if (netGoBuildTag){
                    println("go package net: built with netgo build tag; using Go's DNS resolver");
                } else {
                    println("go package net: GODEBUG setting forcing use of Go's resolver");
                }
                break;
            }
            case {} when !cgoAvailable: {
                println("go package net: cgo resolver not supported; using Go's DNS resolver");
                break;
            }
            case {} when (~confVal).netCgo || (~confVal).preferCgo: {
                println("go package net: using cgo DNS resolver");
                break;
            }
            default: {
                println("go package net: dynamic selection of DNS resolver");
                break;
            }}

        });
    }
    // The remainder of this function sets preferCgo based on
    // conditions that will not change during program execution.
    // By default, prefer the go resolver.
    confVal.val.preferCgo = false;
    // If the cgo resolver is not available, we can't prefer it.
    if (!cgoAvailable) {
        return;
    }
    // Some operating systems always prefer the cgo resolver.
    if (goosPrefersCgo()) {
        confVal.val.preferCgo = true;
        return;
    }
    // The remaining checks are specific to Unix systems.
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        return;
    }

    // If any environment-specified resolver options are specified,
    // prefer the cgo resolver.
    // Note that LOCALDOMAIN can change behavior merely by being
    // specified with the empty string.
    var (_, localDomainDefined) = syscall.Getenv("LOCALDOMAIN"u8);
    if (localDomainDefined || os.Getenv("RES_OPTIONS"u8) != ""u8 || os.Getenv("HOSTALIASES"u8) != ""u8) {
        confVal.val.preferCgo = true;
        return;
    }
    // OpenBSD apparently lets you override the location of resolv.conf
    // with ASR_CONFIG. If we notice that, defer to libc.
    if (runtime.GOOS == "openbsd"u8 && os.Getenv("ASR_CONFIG"u8) != ""u8) {
        confVal.val.preferCgo = true;
        return;
    }
});

// goosPrefersCgo reports whether the GOOS value passed in prefers
// the cgo resolver.
internal static bool goosPrefersCgo() {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
        return true;
    }
    if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
        return true;
    }
    if (exprᴛ1 == "android"u8) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// Historically on Windows and Plan 9 we prefer the
// cgo resolver (which doesn't use the cgo tool) rather than
// the go resolver. This is because originally these
// systems did not support the go resolver.
// Keep it this way for better compatibility.
// Perhaps we can revisit this some day.
// Darwin pops up annoying dialog boxes if programs try to
// do their own DNS requests, so prefer cgo.
// DNS requests don't work on Android, so prefer the cgo resolver.
// Issue #10714.

// mustUseGoResolver reports whether a DNS lookup of any sort is
// required to use the go resolver. The provided Resolver is optional.
// This will report true if the cgo resolver is not available.
[GoRecv] internal static bool mustUseGoResolver(this ref conf c, ж<Resolver> Ꮡr) {
    ref var r = ref Ꮡr.val;

    if (!cgoAvailable) {
        return true;
    }
    if (runtime.GOOS == "plan9"u8) {
        // TODO(bradfitz): for now we only permit use of the PreferGo
        // implementation when there's a non-nil Resolver with a
        // non-nil Dialer. This is a sign that the code is trying
        // to use their DNS-speaking net.Conn (such as an in-memory
        // DNS cache) and they don't want to actually hit the network.
        // Once we add support for looking the default DNS servers
        // from plan9, though, then we can relax this.
        if (r == nil || r.Dial == default!) {
            return false;
        }
    }
    return c.netGo || r.preferGo();
}

// addrLookupOrder determines which strategy to use to resolve addresses.
// The provided Resolver is optional. nil means to not consider its options.
// It also returns dnsConfig when it was used to determine the lookup order.
[GoRecv] internal static (ΔhostLookupOrder ret, ж<dnsConfig> dnsConf) addrLookupOrder(this ref conf c, ж<Resolver> Ꮡr, @string addr) => func((defer, _) => {
    ΔhostLookupOrder ret = default!;
    ж<dnsConfig> dnsConf = default!;

    ref var r = ref Ꮡr.val;
    if (c.dnsDebugLevel > 1) {
        defer(() => {
            print("go package net: addrLookupOrder(", addr, ") = ", ret.String(), "\n");
        });
    }
    return c.lookupOrder(Ꮡr, ""u8);
});

// hostLookupOrder determines which strategy to use to resolve hostname.
// The provided Resolver is optional. nil means to not consider its options.
// It also returns dnsConfig when it was used to determine the lookup order.
[GoRecv] internal static (ΔhostLookupOrder ret, ж<dnsConfig> dnsConf) hostLookupOrder(this ref conf c, ж<Resolver> Ꮡr, @string hostname) => func((defer, _) => {
    ΔhostLookupOrder ret = default!;
    ж<dnsConfig> dnsConf = default!;

    ref var r = ref Ꮡr.val;
    if (c.dnsDebugLevel > 1) {
        defer(() => {
            print("go package net: hostLookupOrder(", hostname, ") = ", ret.String(), "\n");
        });
    }
    return c.lookupOrder(Ꮡr, hostname);
});

[GoRecv] internal static (ΔhostLookupOrder ret, ж<dnsConfig> dnsConf) lookupOrder(this ref conf c, ж<Resolver> Ꮡr, @string hostname) {
    ΔhostLookupOrder ret = default!;
    ж<dnsConfig> dnsConf = default!;

    ref var r = ref Ꮡr.val;
    // fallbackOrder is the order we return if we can't figure it out.
    ΔhostLookupOrder fallbackOrder = default!;
    bool canUseCgo = default!;
    if (c.mustUseGoResolver(Ꮡr)){
        // Go resolver was explicitly requested
        // or cgo resolver is not available.
        // Figure out the order below.
        fallbackOrder = hostLookupFilesDNS;
        canUseCgo = false;
    } else 
    if (c.netCgo){
        // Cgo resolver was explicitly requested.
        return (hostLookupCgo, default!);
    } else 
    if (c.preferCgo){
        // Given a choice, we prefer the cgo resolver.
        return (hostLookupCgo, default!);
    } else {
        // Neither resolver was explicitly requested
        // and we have no preference.
        if (bytealg.IndexByteString(hostname, (rune)'\\') != -1 || bytealg.IndexByteString(hostname, (rune)'%') != -1) {
            // Don't deal with special form hostnames
            // with backslashes or '%'.
            return (hostLookupCgo, default!);
        }
        // If something is unrecognized, use cgo.
        fallbackOrder = hostLookupCgo;
        canUseCgo = true;
    }
    // On systems that don't use /etc/resolv.conf or /etc/nsswitch.conf, we are done.
    var exprᴛ1 = c.goos;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "android"u8 || exprᴛ1 == "ios"u8) {
        return (fallbackOrder, default!);
    }

    // Try to figure out the order to use for searches.
    // If we don't recognize something, use fallbackOrder.
    // That will use cgo unless the Go resolver was explicitly requested.
    // If we do figure out the order, return something other
    // than fallbackOrder to use the Go resolver with that order.
    dnsConf = getSystemDNSConfig();
    if (canUseCgo && (~dnsConf).err != default! && !errors.Is((~dnsConf).err, fs.ErrNotExist) && !errors.Is((~dnsConf).err, fs.ErrPermission)) {
        // We can't read the resolv.conf file, so use cgo if we can.
        return (hostLookupCgo, dnsConf);
    }
    if (canUseCgo && (~dnsConf).unknownOpt) {
        // We didn't recognize something in resolv.conf,
        // so use cgo if we can.
        return (hostLookupCgo, dnsConf);
    }
    // OpenBSD is unique and doesn't use nsswitch.conf.
    // It also doesn't support mDNS.
    if (c.goos == "openbsd"u8) {
        // OpenBSD's resolv.conf manpage says that a
        // non-existent resolv.conf means "lookup" defaults
        // to only "files", without DNS lookups.
        if (errors.Is((~dnsConf).err, fs.ErrNotExist)) {
            return (hostLookupFiles, dnsConf);
        }
        var lookup = dnsConf.val.lookup;
        if (len(lookup) == 0) {
            // https://www.openbsd.org/cgi-bin/man.cgi/OpenBSD-current/man5/resolv.conf.5
            // "If the lookup keyword is not used in the
            // system's resolv.conf file then the assumed
            // order is 'bind file'"
            return (hostLookupDNSFiles, dnsConf);
        }
        if (len(lookup) < 1 || len(lookup) > 2) {
            // We don't recognize this format.
            return (fallbackOrder, dnsConf);
        }
        var exprᴛ2 = lookup[0];
        if (exprᴛ2 == "bind"u8) {
            if (len(lookup) == 2) {
                if (lookup[1] == "file") {
                    return (hostLookupDNSFiles, dnsConf);
                }
                // Unrecognized.
                return (fallbackOrder, dnsConf);
            }
            return (hostLookupDNS, dnsConf);
        }
        if (exprᴛ2 == "file"u8) {
            if (len(lookup) == 2) {
                if (lookup[1] == "bind") {
                    return (hostLookupFilesDNS, dnsConf);
                }
                // Unrecognized.
                return (fallbackOrder, dnsConf);
            }
            return (hostLookupFiles, dnsConf);
        }
        { /* default: */
            return (fallbackOrder, dnsConf);
        }

    }
    // Unrecognized.
    // We always return before this point.
    // The code below is for non-OpenBSD.
    // Canonicalize the hostname by removing any trailing dot.
    hostname = stringslite.TrimSuffix(hostname, "."u8);
    var nss = getSystemNSS();
    var srcs = (~nss).sources["hosts"u8];
    // If /etc/nsswitch.conf doesn't exist or doesn't specify any
    // sources for "hosts", assume Go's DNS will work fine.
    if (errors.Is((~nss).err, fs.ErrNotExist) || ((~nss).err == default! && len(srcs) == 0)) {
        if (canUseCgo && c.goos == "solaris"u8) {
            // illumos defaults to
            // "nis [NOTFOUND=return] files",
            // which the go resolver doesn't support.
            return (hostLookupCgo, dnsConf);
        }
        return (hostLookupFilesDNS, dnsConf);
    }
    if ((~nss).err != default!) {
        // We failed to parse or open nsswitch.conf, so
        // we have nothing to base an order on.
        return (fallbackOrder, dnsConf);
    }
    bool hasDNSSource = default!;
    bool hasDNSSourceChecked = default!;
    bool filesSource = default!;
    bool dnsSource = default!;
    @string first = default!;
    foreach (var (i, src) in srcs) {
        if (src.source == "files"u8 || src.source == "dns"u8) {
            if (canUseCgo && !src.standardCriteria()) {
                // non-standard; let libc deal with it.
                return (hostLookupCgo, dnsConf);
            }
            if (src.source == "files"u8){
                filesSource = true;
            } else {
                hasDNSSource = true;
                hasDNSSourceChecked = true;
                dnsSource = true;
            }
            if (first == ""u8) {
                first = src.source;
            }
            continue;
        }
        if (canUseCgo) {
            switch (ᐧ) {
            case {} when hostname != ""u8 && src.source == "myhostname"u8: {
                if (isLocalhost(hostname) || isGateway(hostname) || isOutbound(hostname)) {
                    // Let the cgo resolver handle myhostname
                    // if we are looking up the local hostname.
                    return (hostLookupCgo, dnsConf);
                }
                var (hn, err) = getHostname();
                if (err != default! || stringsEqualFold(hostname, hn)) {
                    return (hostLookupCgo, dnsConf);
                }
                continue;
                break;
            }
            case {} when hostname != ""u8 && stringslite.HasPrefix(src.source, "mdns"u8): {
                if (stringsHasSuffixFold(hostname, ".local"u8)) {
                    // Per RFC 6762, the ".local" TLD is special. And
                    // because Go's native resolver doesn't do mDNS or
                    // similar local resolution mechanisms, assume that
                    // libc might (via Avahi, etc) and use cgo.
                    return (hostLookupCgo, dnsConf);
                }
                // We don't parse mdns.allow files. They're rare. If one
                // exists, it might list other TLDs (besides .local) or even
                // '*', so just let libc deal with it.
                bool haveMDNSAllow = default!;
                var exprᴛ3 = c.mdnsTest;
                if (exprᴛ3 == mdnsFromSystem) {
                    (_, err) = os.Stat("/etc/mdns.allow"u8);
                    if (err != default! && !errors.Is(err, fs.ErrNotExist)) {
                        // Let libc figure out what is going on.
                        return (hostLookupCgo, dnsConf);
                    }
                    haveMDNSAllow = err == default!;
                }
                else if (exprᴛ3 == mdnsAssumeExists) {
                    haveMDNSAllow = true;
                }
                else if (exprᴛ3 == mdnsAssumeDoesNotExist) {
                    haveMDNSAllow = false;
                }

                if (haveMDNSAllow) {
                    return (hostLookupCgo, dnsConf);
                }
                continue;
                break;
            }
            default: {
                return (hostLookupCgo, dnsConf);
            }}

        }
        // Some source we don't know how to deal with.
        if (!hasDNSSourceChecked) {
            hasDNSSourceChecked = true;
            foreach (var (_, v) in srcs[(int)(i + 1)..]) {
                if (v.source == "dns"u8) {
                    hasDNSSource = true;
                    break;
                }
            }
        }
        // If we saw a source we don't recognize, which can only
        // happen if we can't use the cgo resolver, treat it as DNS,
        // but only when there is no dns in all other sources.
        if (!hasDNSSource) {
            dnsSource = true;
            if (first == ""u8) {
                first = "dns"u8;
            }
        }
    }
    // Cases where Go can handle it without cgo and C thread overhead,
    // or where the Go resolver has been forced.
    switch (ᐧ) {
    case {} when filesSource && dnsSource: {
        if (first == "files"u8){
            return (hostLookupFilesDNS, dnsConf);
        } else {
            return (hostLookupDNSFiles, dnsConf);
        }
        break;
    }
    case {} when filesSource: {
        return (hostLookupFiles, dnsConf);
    }
    case {} when dnsSource: {
        return (hostLookupDNS, dnsConf);
    }}

    // Something weird. Fallback to the default.
    return (fallbackOrder, dnsConf);
}

internal static ж<godebug.Setting> netdns = godebug.New("netdns"u8);

// goDebugNetDNS parses the value of the GODEBUG "netdns" value.
// The netdns value can be of the form:
//
//	1       // debug level 1
//	2       // debug level 2
//	cgo     // use cgo for DNS lookups
//	go      // use go for DNS lookups
//	cgo+1   // use cgo for DNS lookups + debug level 1
//	1+cgo   // same
//	cgo+2   // same, but debug level 2
//
// etc.
internal static (@string dnsMode, nint debugLevel) goDebugNetDNS() {
    @string dnsMode = default!;
    nint debugLevel = default!;

    @string goDebug = netdns.Value();
    var parsePart = (@string s) => {
        if (s == ""u8) {
            return (dnsMode, debugLevel);
        }
        if ((rune)'0' <= s[0] && s[0] <= (rune)'9'){
            (debugLevel, _, _) = dtoi(s);
        } else {
            dnsMode = s;
        }
    };
    {
        nint i = bytealg.IndexByteString(goDebug, (rune)'+'); if (i != -1) {
            parsePart(goDebug[..(int)(i)]);
            parsePart(goDebug[(int)(i + 1)..]);
            return (dnsMode, debugLevel);
        }
    }
    parsePart(goDebug);
    return (dnsMode, debugLevel);
}

// isLocalhost reports whether h should be considered a "localhost"
// name for the myhostname NSS module.
internal static bool isLocalhost(@string h) {
    return stringsEqualFold(h, "localhost"u8) || stringsEqualFold(h, "localhost.localdomain"u8) || stringsHasSuffixFold(h, ".localhost"u8) || stringsHasSuffixFold(h, ".localhost.localdomain"u8);
}

// isGateway reports whether h should be considered a "gateway"
// name for the myhostname NSS module.
internal static bool isGateway(@string h) {
    return stringsEqualFold(h, "_gateway"u8);
}

// isOutbound reports whether h should be considered an "outbound"
// name for the myhostname NSS module.
internal static bool isOutbound(@string h) {
    return stringsEqualFold(h, "_outbound"u8);
}

} // end net_package
