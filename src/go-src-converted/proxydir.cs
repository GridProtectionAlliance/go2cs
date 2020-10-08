// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package proxydir provides functions for writing module data to a directory
// in proxy format, so that it can be used as a module proxy by setting
// GOPROXY="file://<dir>".
// package proxydir -- go2cs converted at 2020 October 08 04:55:55 UTC
// import "golang.org/x/tools/internal/proxydir" ==> using proxydir = go.golang.org.x.tools.@internal.proxydir_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\proxydir\proxydir.go
using zip = go.archive.zip_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using testenv = go.golang.org.x.tools.@internal.testenv_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class proxydir_package
    {
        // WriteModuleVersion creates a directory in the proxy dir for a module.
        public static error WriteModuleVersion(@string rootDir, @string module, @string ver, map<@string, slice<byte>> files) => func((defer, _, __) =>
        {
            error rerr = default!;

            var dir = filepath.Join(rootDir, module, "@v");
            {
                var err__prev1 = err;

                var err = os.MkdirAll(dir, 0755L);

                if (err != null)
                {
                    return error.As(err)!;
                }
                err = err__prev1;

            } 

            // The go command checks for versions by looking at the "list" file.  Since
            // we are supporting multiple versions, create this file if it does not exist
            // or append the version number to the preexisting file.
            var (f, err) = os.OpenFile(filepath.Join(dir, "list"), os.O_APPEND | os.O_CREATE | os.O_WRONLY, 0644L);
            if (err != null)
            {
                return error.As(err)!;
            }
            defer(checkClose("list file", f, _addr_rerr));
            {
                var err__prev1 = err;

                var (_, err) = f.WriteString(ver + "\n");

                if (err != null)
                {
                    return error.As(err)!;
                }
                err = err__prev1;

            } 

            // Serve the go.mod file on the <version>.mod url, if it exists. Otherwise,
            // serve a stub.
            var (modContents, ok) = files["go.mod"];
            if (!ok)
            {
                modContents = (slice<byte>)"module " + module;
            }
            {
                var err__prev1 = err;

                err = ioutil.WriteFile(filepath.Join(dir, ver + ".mod"), modContents, 0644L);

                if (err != null)
                {
                    return error.As(err)!;
                }
                err = err__prev1;

            } 

            // info file, just the bare bones.
            slice<byte> infoContents = (slice<byte>)fmt.Sprintf("{\"Version\": \"%v\", \"Time\":\"2017-12-14T13:08:43Z\"}", ver);
            {
                var err__prev1 = err;

                err = ioutil.WriteFile(filepath.Join(dir, ver + ".info"), infoContents, 0644L);

                if (err != null)
                {
                    return error.As(err)!;
                }
                err = err__prev1;

            } 

            // zip of all the source files.
            f, err = os.OpenFile(filepath.Join(dir, ver + ".zip"), os.O_CREATE | os.O_WRONLY, 0644L);
            if (err != null)
            {
                return error.As(err)!;
            }
            defer(checkClose("zip file", f, _addr_rerr));
            var z = zip.NewWriter(f);
            defer(checkClose("zip writer", z, _addr_rerr));
            foreach (var (name, contents) in files)
            {
                var (zf, err) = z.Create(module + "@" + ver + "/" + name);
                if (err != null)
                {
                    return error.As(err)!;
                }
                {
                    var err__prev1 = err;

                    (_, err) = zf.Write(contents);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }
                    err = err__prev1;

                }

            }            return error.As(null!)!;

        });

        private static void checkClose(@string name, io.Closer closer, ptr<error> _addr_err)
        {
            ref error err = ref _addr_err.val;

            {
                var cerr = closer.Close();

                if (cerr != null && err == null.val)
                {
                    err = error.As(fmt.Errorf("closing %s: %v", name, cerr))!;
                }

            }

        }

        // ToURL returns the file uri for a proxy directory.
        public static @string ToURL(@string dir)
        {
            if (testenv.Go1Point() >= 13L)
            { 
                // file URLs on Windows must start with file:///. See golang.org/issue/6027.
                var path = filepath.ToSlash(dir);
                if (!strings.HasPrefix(path, "/"))
                {
                    path = "/" + path;
                }

                return "file://" + path;

            }
            else
            { 
                // Prior to go1.13, the Go command on Windows only accepted GOPROXY file URLs
                // of the form file://C:/path/to/proxy. This was incorrect: when parsed, "C:"
                // is interpreted as the host. See golang.org/issue/6027. This has been
                // fixed in go1.13, but we emit the old format for old releases.
                return "file://" + filepath.ToSlash(dir);

            }

        }
    }
}}}}}
