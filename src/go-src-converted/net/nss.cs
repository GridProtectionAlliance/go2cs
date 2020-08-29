// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 August 29 08:27:10 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\nss.go
using errors = go.errors_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        // nssConf represents the state of the machine's /etc/nsswitch.conf file.
        private partial struct nssConf
        {
            public error err; // any error encountered opening or parsing the file
            public map<@string, slice<nssSource>> sources; // keyed by database (e.g. "hosts")
        }

        private partial struct nssSource
        {
            public @string source; // e.g. "compat", "files", "mdns4_minimal"
            public slice<nssCriterion> criteria;
        }

        // standardCriteria reports all specified criteria have the default
        // status actions.
        private static bool standardCriteria(this nssSource s)
        {
            foreach (var (i, crit) in s.criteria)
            {
                if (!crit.standardStatusAction(i == len(s.criteria) - 1L))
                {
                    return false;
                }
            }
            return true;
        }

        // nssCriterion is the parsed structure of one of the criteria in brackets
        // after an NSS source name.
        private partial struct nssCriterion
        {
            public bool negate; // if "!" was present
            public @string status; // e.g. "success", "unavail" (lowercase)
            public @string action; // e.g. "return", "continue" (lowercase)
        }

        // standardStatusAction reports whether c is equivalent to not
        // specifying the criterion at all. last is whether this criteria is the
        // last in the list.
        private static bool standardStatusAction(this nssCriterion c, bool last)
        {
            if (c.negate)
            {
                return false;
            }
            @string def = default;
            switch (c.status)
            {
                case "success": 
                    def = "return";
                    break;
                case "notfound": 

                case "unavail": 

                case "tryagain": 
                    def = "continue";
                    break;
                default: 
                    // Unknown status
                    return false;
                    break;
            }
            if (last && c.action == "return")
            {
                return true;
            }
            return c.action == def;
        }

        private static ref nssConf parseNSSConfFile(@string file) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(file);
            if (err != null)
            {
                return ref new nssConf(err:err);
            }
            defer(f.Close());
            return parseNSSConf(f);
        });

        private static ref nssConf parseNSSConf(io.Reader r)
        {
            var (slurp, err) = readFull(r);
            if (err != null)
            {
                return ref new nssConf(err:err);
            }
            ptr<nssConf> conf = @new<nssConf>();
            conf.err = foreachLine(slurp, line =>
            {
                line = trimSpace(removeComment(line));
                if (len(line) == 0L)
                {
                    return null;
                }
                var colon = bytesIndexByte(line, ':');
                if (colon == -1L)
                {
                    return errors.New("no colon on line");
                }
                var db = string(trimSpace(line[..colon]));
                var srcs = line[colon + 1L..];
                while (true)
                {
                    srcs = trimSpace(srcs);
                    if (len(srcs) == 0L)
                    {
                        break;
                    }
                    var sp = bytesIndexByte(srcs, ' ');
                    @string src = default;
                    if (sp == -1L)
                    {
                        src = string(srcs);
                        srcs = null; // done
                    }
                    else
                    {
                        src = string(srcs[..sp]);
                        srcs = trimSpace(srcs[sp + 1L..]);
                    }
                    slice<nssCriterion> criteria = default; 
                    // See if there's a criteria block in brackets.
                    if (len(srcs) > 0L && srcs[0L] == '[')
                    {
                        var bclose = bytesIndexByte(srcs, ']');
                        if (bclose == -1L)
                        {
                            return errors.New("unclosed criterion bracket");
                        }
                        error err = default;
                        criteria, err = parseCriteria(srcs[1L..bclose]);
                        if (err != null)
                        {
                            return errors.New("invalid criteria: " + string(srcs[1L..bclose]));
                        }
                        srcs = srcs[bclose + 1L..];
                    }
                    if (conf.sources == null)
                    {
                        conf.sources = make_map<@string, slice<nssSource>>();
                    }
                    conf.sources[db] = append(conf.sources[db], new nssSource(source:src,criteria:criteria,));
                }

                return null;
            });
            return conf;
        }

        // parses "foo=bar !foo=bar"
        private static (slice<nssCriterion>, error) parseCriteria(slice<byte> x)
        {
            err = foreachField(x, f =>
            {
                var not = false;
                if (len(f) > 0L && f[0L] == '!')
                {
                    not = true;
                    f = f[1L..];
                }
                if (len(f) < 3L)
                {
                    return errors.New("criterion too short");
                }
                var eq = bytesIndexByte(f, '=');
                if (eq == -1L)
                {
                    return errors.New("criterion lacks equal sign");
                }
                lowerASCIIBytes(f);
                c = append(c, new nssCriterion(negate:not,status:string(f[:eq]),action:string(f[eq+1:]),));
                return null;
            });
            return;
        }
    }
}
