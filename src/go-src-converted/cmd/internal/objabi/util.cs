// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 October 08 03:50:15 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\util.go
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        private static @string envOr(@string key, @string value)
        {
            {
                var x = os.Getenv(key);

                if (x != "")
                {
                    return x;
                }
            }

            return value;

        }

        private static @string defaultGOROOT = default;        public static var GOROOT = envOr("GOROOT", defaultGOROOT);        public static var GOARCH = envOr("GOARCH", defaultGOARCH);        public static var GOOS = envOr("GOOS", defaultGOOS);        public static var GO386 = envOr("GO386", defaultGO386);        public static var GOAMD64 = goamd64();        public static var GOARM = goarm();        public static var GOMIPS = gomips();        public static var GOMIPS64 = gomips64();        public static var GOPPC64 = goppc64();        public static var GOWASM = gowasm();        public static var GO_LDSO = defaultGO_LDSO;        public static var Version = version;

        public static readonly long ElfRelocOffset = (long)256L;
        public static readonly long MachoRelocOffset = (long)2048L; // reserve enough space for ELF relocations
        public static readonly @string Go115AMD64 = (@string)"alignedjumps"; // Should be "alignedjumps" or "normaljumps"; this replaces environment variable introduced in CL 219357.

        // TODO(1.16): assuming no issues in 1.15 release, remove this and related constant.
        private static @string goamd64()
        {
            return Go115AMD64;
        }

        private static long goarm() => func((_, panic, __) =>
        {
            {
                var v = envOr("GOARM", defaultGOARM);

                switch (v)
                {
                    case "5": 
                        return 5L;
                        break;
                    case "6": 
                        return 6L;
                        break;
                    case "7": 
                        return 7L;
                        break;
                }
            } 
            // Fail here, rather than validate at multiple call sites.
            log.Fatalf("Invalid GOARM value. Must be 5, 6, or 7.");
            panic("unreachable");

        });

        private static @string gomips() => func((_, panic, __) =>
        {
            {
                var v = envOr("GOMIPS", defaultGOMIPS);

                switch (v)
                {
                    case "hardfloat": 

                    case "softfloat": 
                        return v;
                        break;
                }
            }
            log.Fatalf("Invalid GOMIPS value. Must be hardfloat or softfloat.");
            panic("unreachable");

        });

        private static @string gomips64() => func((_, panic, __) =>
        {
            {
                var v = envOr("GOMIPS64", defaultGOMIPS64);

                switch (v)
                {
                    case "hardfloat": 

                    case "softfloat": 
                        return v;
                        break;
                }
            }
            log.Fatalf("Invalid GOMIPS64 value. Must be hardfloat or softfloat.");
            panic("unreachable");

        });

        private static long goppc64() => func((_, panic, __) =>
        {
            {
                var v = envOr("GOPPC64", defaultGOPPC64);

                switch (v)
                {
                    case "power8": 
                        return 8L;
                        break;
                    case "power9": 
                        return 9L;
                        break;
                }
            }
            log.Fatalf("Invalid GOPPC64 value. Must be power8 or power9.");
            panic("unreachable");

        });

        private partial struct gowasmFeatures
        {
            public bool SignExt;
            public bool SatConv;
        }

        private static @string String(this gowasmFeatures f)
        {
            slice<@string> flags = default;
            if (f.SatConv)
            {
                flags = append(flags, "satconv");
            }

            if (f.SignExt)
            {
                flags = append(flags, "signext");
            }

            return strings.Join(flags, ",");

        }

        private static gowasmFeatures gowasm()
        {
            gowasmFeatures f = default;

            foreach (var (_, opt) in strings.Split(envOr("GOWASM", ""), ","))
            {
                switch (opt)
                {
                    case "satconv": 
                        f.SatConv = true;
                        break;
                    case "signext": 
                        f.SignExt = true;
                        break;
                    case "": 
                        break;
                    default: 
                        log.Fatalf("Invalid GOWASM value. No such feature: " + opt);
                        break;
                }

            }
            return ;

        }

        public static @string Getgoextlinkenabled()
        {
            return envOr("GO_EXTLINK_ENABLED", defaultGO_EXTLINK_ENABLED);
        }

        private static void init()
        {
            foreach (var (_, f) in strings.Split(goexperiment, ","))
            {
                if (f != "")
                {
                    addexp(f);
                }

            }

        }

        public static bool Framepointer_enabled(@string goos, @string goarch)
        {
            return framepointer_enabled != 0L && (goarch == "amd64" || goarch == "arm64" && (goos == "linux" || goos == "darwin"));
        }

        private static void addexp(@string s)
        { 
            // Could do general integer parsing here, but the runtime copy doesn't yet.
            long v = 1L;
            var name = s;
            if (len(name) > 2L && name[..2L] == "no")
            {
                v = 0L;
                name = name[2L..];
            }

            for (long i = 0L; i < len(exper); i++)
            {
                if (exper[i].name == name)
                {
                    if (exper[i].val != null)
                    {
                        exper[i].val = v;
                    }

                    return ;

                }

            }


            fmt.Printf("unknown experiment %s\n", s);
            os.Exit(2L);

        }

        private static long framepointer_enabled = 1L;        public static long Fieldtrack_enabled = default;        public static long Preemptibleloops_enabled = default;        public static long Staticlockranking_enabled = default;

        // Toolchain experiments.
        // These are controlled by the GOEXPERIMENT environment
        // variable recorded when the toolchain is built.
        // This list is also known to cmd/gc.


        private static var defaultExpstring = Expstring();

        public static @string DefaultExpstring()
        {
            return defaultExpstring;
        }

        public static @string Expstring()
        {
            @string buf = "X";
            foreach (var (i) in exper)
            {
                if (exper[i].val != 0L.val)
                {
                    buf += "," + exper[i].name;
                }

            }
            if (buf == "X")
            {
                buf += ",none";
            }

            return "X:" + buf[2L..];

        }
    }
}}}
