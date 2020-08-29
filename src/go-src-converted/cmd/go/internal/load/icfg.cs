// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2020 August 29 10:00:52 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Go\src\cmd\go\internal\load\icfg.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using ioutil = go.io.ioutil_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        // DebugDeprecatedImportcfg is installed as the undocumented -debug-deprecated-importcfg build flag.
        // It is useful for debugging subtle problems in the go command logic but not something
        // we want users to depend on. The hope is that the "deprecated" will make that clear.
        // We intend to remove this flag in Go 1.11.
        public static debugDeprecatedImportcfgFlag DebugDeprecatedImportcfg = default;

        private partial struct debugDeprecatedImportcfgFlag
        {
            public bool enabled;
            public map<@string, @string> Import;
            public map<@string, ref debugDeprecatedImportcfgPkg> Pkg;
        }

        private partial struct debugDeprecatedImportcfgPkg
        {
            public @string Dir;
            public map<@string, @string> Import;
        }

        private static slice<byte> debugDeprecatedImportcfgMagic = (slice<byte>)"# debug-deprecated-importcfg\n";        private static var errImportcfgSyntax = errors.New("malformed syntax");

        private static @string String(this ref debugDeprecatedImportcfgFlag f)
        {
            return "";
        }

        private static error Set(this ref debugDeprecatedImportcfgFlag f, @string x)
        {
            if (x == "")
            {
                f.Value = new debugDeprecatedImportcfgFlag();
                return error.As(null);
            }
            var (data, err) = ioutil.ReadFile(x);
            if (err != null)
            {
                return error.As(err);
            }
            if (!bytes.HasPrefix(data, debugDeprecatedImportcfgMagic))
            {
                return error.As(errImportcfgSyntax);
            }
            data = data[len(debugDeprecatedImportcfgMagic)..];

            f.Import = null;
            f.Pkg = null;
            {
                var err = json.Unmarshal(data, ref f);

                if (err != null)
                {
                    return error.As(errImportcfgSyntax);
                }

            }
            f.enabled = true;
            return error.As(null);
        }

        private static (@string, @string) lookup(this ref debugDeprecatedImportcfgFlag f, ref Package parent, @string path)
        {
            newPath = path;
            {
                var p__prev1 = p;

                var p = f.Import[path];

                if (p != "")
                {
                    newPath = p;
                }

                p = p__prev1;

            }
            if (parent != null)
            {
                {
                    var p1 = f.Pkg[parent.ImportPath];

                    if (p1 != null)
                    {
                        {
                            var p__prev3 = p;

                            p = p1.Import[path];

                            if (p != "")
                            {
                                newPath = p;
                            }

                            p = p__prev3;

                        }
                    }

                }
            }
            {
                var p2 = f.Pkg[newPath];

                if (p2 != null)
                {
                    return (p2.Dir, newPath);
                }

            }
            return ("", "");
        }
    }
}}}}
