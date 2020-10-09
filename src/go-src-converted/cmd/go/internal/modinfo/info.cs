// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modinfo -- go2cs converted at 2020 October 09 05:45:41 UTC
// import "cmd/go/internal/modinfo" ==> using modinfo = go.cmd.go.@internal.modinfo_package
// Original source: C:\Go\src\cmd\go\internal\modinfo\info.go
using time = go.time_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modinfo_package
    {
        // Note that these structs are publicly visible (part of go list's API)
        // and the fields are documented in the help text in ../list/list.go
        public partial struct ModulePublic
        {
            [Description("json:\",omitempty\"")]
            public @string Path; // module path
            [Description("json:\",omitempty\"")]
            public @string Version; // module version
            [Description("json:\",omitempty\"")]
            public slice<@string> Versions; // available module versions
            [Description("json:\",omitempty\"")]
            public ptr<ModulePublic> Replace; // replaced by this module
            [Description("json:\",omitempty\"")]
            public ptr<time.Time> Time; // time version was created
            [Description("json:\",omitempty\"")]
            public ptr<ModulePublic> Update; // available update (with -u)
            [Description("json:\",omitempty\"")]
            public bool Main; // is this the main module?
            [Description("json:\",omitempty\"")]
            public bool Indirect; // module is only indirectly needed by main module
            [Description("json:\",omitempty\"")]
            public @string Dir; // directory holding local copy of files, if any
            [Description("json:\",omitempty\"")]
            public @string GoMod; // path to go.mod file describing module, if any
            [Description("json:\",omitempty\"")]
            public @string GoVersion; // go version used in module
            [Description("json:\",omitempty\"")]
            public ptr<ModuleError> Error; // error loading module
        }

        public partial struct ModuleError
        {
            public @string Err; // error text
        }

        private static @string String(this ptr<ModulePublic> _addr_m)
        {
            ref ModulePublic m = ref _addr_m.val;

            var s = m.Path;
            if (m.Version != "")
            {
                s += " " + m.Version;
                if (m.Update != null)
                {
                    s += " [" + m.Update.Version + "]";
                }

            }

            if (m.Replace != null)
            {
                s += " => " + m.Replace.Path;
                if (m.Replace.Version != "")
                {
                    s += " " + m.Replace.Version;
                    if (m.Replace.Update != null)
                    {
                        s += " [" + m.Replace.Update.Version + "]";
                    }

                }

            }

            return s;

        }
    }
}}}}
