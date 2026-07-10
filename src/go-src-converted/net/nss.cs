// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using os = os_package;
using Δsync = sync_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using go.io;

partial class net_package {

internal static readonly @string nssConfigPath = "/etc/nsswitch.conf"u8;

internal static ж<nsswitchConfig> ᏑnssConfig = new(default(nsswitchConfig));
internal static ref nsswitchConfig nssConfig => ref ᏑnssConfig.Value;

[GoType] partial struct nsswitchConfig {
    internal Δsync.Once initOnce; // guards init of nsswitchConfig
    // ch is used as a semaphore that only allows one lookup at a
    // time to recheck nsswitch.conf
    internal channel<EmptyStruct> ch; // guards lastChecked and modTime
    internal time.Time lastChecked;     // last time nsswitch.conf was checked
    internal Δsync.Mutex mu; // protects nssConf
    internal ж<nssConf> nssConf;
}

internal static ж<nssConf> getSystemNSS() {
    ᏑnssConfig.tryUpdate();
    ᏑnssConfig.of(nsswitchConfig.Ꮡmu).Lock();
    var conf = nssConfig.nssConf;
    ᏑnssConfig.of(nsswitchConfig.Ꮡmu).Unlock();
    return conf;
}

// init initializes conf and is only called via conf.initOnce.
[GoRecv] internal static void init(this ref nsswitchConfig conf) {
    conf.nssConf = parseNSSConfFile("/etc/nsswitch.conf"u8);
    conf.lastChecked = time.Now();
    conf.ch = new channel<EmptyStruct>(1);
}

// tryUpdate tries to update conf.
internal static void tryUpdate(this ж<nsswitchConfig> Ꮡconf) => func((defer, recover) => {
    ref var conf = ref Ꮡconf.Value;

    Ꮡconf.of(nsswitchConfig.ᏑinitOnce).Do(Ꮡconf.init);
    // Ensure only one update at a time checks nsswitch.conf
    if (!conf.tryAcquireSema()) {
        return;
    }
    defer(Ꮡconf.releaseSema);
    var now = time.Now();
    if (conf.lastChecked.After(now.Add(-5000000000L))) {
        return;
    }
    conf.lastChecked = now;
    time.Time mtime = default!;
    {
        var (fi, err) = os.Stat(nssConfigPath); if (err == default!) {
            mtime = fi.ModTime();
        }
    }
    if (mtime.Equal((~conf.nssConf).mtime)) {
        return;
    }
    var nssConf = parseNSSConfFile(nssConfigPath);
    Ꮡconf.of(nsswitchConfig.Ꮡmu).Lock();
    conf.nssConf = nssConf;
    Ꮡconf.of(nsswitchConfig.Ꮡmu).Unlock();
});

[GoRecv] internal static void acquireSema(this ref nsswitchConfig conf) {
    conf.ch.ᐸꟷ(new EmptyStruct());
}

[GoRecv] internal static bool tryAcquireSema(this ref nsswitchConfig conf) {
    switch (ᐧ) {
    case ᐧ: {
        return true;
    }
    default: {
        return false;
    }}
}

[GoRecv] internal static void releaseSema(this ref nsswitchConfig conf) {
    ᐸꟷ(conf.ch);
}

// nssConf represents the state of the machine's /etc/nsswitch.conf file.
[GoType] partial struct nssConf {
    internal time.Time mtime;              // time of nsswitch.conf modification
    internal error err;                  // any error encountered opening or parsing the file
    internal map<@string, slice<nssSource>> sources; // keyed by database (e.g. "hosts")
}

[GoType] partial struct nssSource {
    internal @string source; // e.g. "compat", "files", "mdns4_minimal"
    internal slice<nssCriterion> criteria;
}

// standardCriteria reports all specified criteria have the default
// status actions.
internal static bool standardCriteria(this nssSource s) {
    foreach (var (i, crit) in s.criteria) {
        if (!crit.standardStatusAction(i == len(s.criteria) - 1)) {
            return false;
        }
    }
    return true;
}

// nssCriterion is the parsed structure of one of the criteria in brackets
// after an NSS source name.
[GoType] partial struct nssCriterion {
    internal bool negate;   // if "!" was present
    internal @string status; // e.g. "success", "unavail" (lowercase)
    internal @string action; // e.g. "return", "continue" (lowercase)
}

// standardStatusAction reports whether c is equivalent to not
// specifying the criterion at all. last is whether this criteria is the
// last in the list.
internal static bool standardStatusAction(this nssCriterion c, bool last) {
    if (c.negate) {
        return false;
    }
    @string def = default!;
    var exprᴛ1 = c.status;
    if (exprᴛ1 == "success"u8) {
        def = "return"u8;
    }
    else if (exprᴛ1 == "notfound"u8 || exprᴛ1 == "unavail"u8 || exprᴛ1 == "tryagain"u8) {
        def = "continue"u8;
    }
    else { /* default: */
        return false;
    }

    // Unknown status
    if (last && c.action == "return"u8) {
        return true;
    }
    return c.action == def;
}

internal static ж<nssConf> parseNSSConfFile(@string Δfile) => func((defer, recover) => {
    var (f, err) = open(Δfile);
    if (err != default!) {
        return Ꮡ(new nssConf(err: err));
    }
    var fʗ1 = f;
    defer(fʗ1.close);
    (var mtime, _, err) = f.stat();
    if (err != default!) {
        return Ꮡ(new nssConf(err: err));
    }
    var conf = parseNSSConf(f);
    conf.Value.mtime = mtime;
    return conf;
});

internal static ж<nssConf> parseNSSConf(ж<Δfile> Ꮡf) {
    ref var f = ref Ꮡf.Value;

    var conf = @new<nssConf>();
    for (var (line, ok) = f.readLine(); ok; (line, ok) = f.readLine()) {
        line = trimSpace(removeComment(line));
        if (len(line) == 0) {
            continue;
        }
        nint colon = bytealg.IndexByteString(line, (rune)':');
        if (colon == -1) {
            conf.Value.err = errors.New("no colon on line"u8);
            return conf;
        }
        @string db = trimSpace(line[..(int)(colon)]);
        @string srcs = line[(int)(colon + 1)..];
        while (ᐧ) {
            srcs = trimSpace(srcs);
            if (len(srcs) == 0) {
                break;
            }
            nint sp = bytealg.IndexByteString(srcs, (rune)' ');
            @string src = default!;
            if (sp == -1){
                src = srcs;
                srcs = ""u8;
            } else {
                // done
                src = srcs[..(int)(sp)];
                srcs = trimSpace(srcs[(int)(sp + 1)..]);
            }
            slice<nssCriterion> criteria = default!;
            // See if there's a criteria block in brackets.
            if (len(srcs) > 0 && srcs[0] == (rune)'[') {
                nint bclose = bytealg.IndexByteString(srcs, (rune)']');
                if (bclose == -1) {
                    conf.Value.err = errors.New("unclosed criterion bracket"u8);
                    return conf;
                }
                error err = default!;
                (criteria, err) = parseCriteria(srcs[1..(int)(bclose)]);
                if (err != default!) {
                    conf.Value.err = errors.New("invalid criteria: " + srcs[1..(int)(bclose)]);
                    return conf;
                }
                srcs = srcs[(int)(bclose + 1)..];
            }
            if ((~conf).sources == default!) {
                conf.Value.sources = new map<@string, slice<nssSource>>();
            }
            conf.Value.sources[db] = append((~conf).sources[db], new nssSource(
                source: src,
                criteria: criteria
            ));
        }
    }
    return conf;
}

// parses "foo=bar !foo=bar"
internal static (slice<nssCriterion> c, error err) parseCriteria(@string x) {
    slice<nssCriterion> c = default!;
    error err = default!;

    err = foreachField(x, error (@string f) => {
        var not = false;
        if (len(f) > 0 && f[0] == (rune)'!') {
            not = true;
            f = f[1..];
        }
        if (len(f) < 3) {
            return errors.New("criterion too short"u8);
        }
        nint eq = bytealg.IndexByteString(f, (rune)'=');
        if (eq == -1) {
            return errors.New("criterion lacks equal sign"u8);
        }
        if (hasUpperCase(f)) {
            var lower = slice<byte>(f);
            lowerASCIIBytes(lower);
            f = ((@string)lower);
        }
        c = append(c, new nssCriterion(
            negate: not,
            status: f[..(int)(eq)],
            action: f[(int)(eq + 1)..]
        ));
        return default!;
    });
    return (c, err);
}

} // end net_package
