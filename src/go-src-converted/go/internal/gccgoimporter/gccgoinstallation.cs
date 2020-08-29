// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gccgoimporter -- go2cs converted at 2020 August 29 10:09:04 UTC
// import "go/internal/gccgoimporter" ==> using gccgoimporter = go.go.@internal.gccgoimporter_package
// Original source: C:\Go\src\go\internal\gccgoimporter\gccgoinstallation.go
using bufio = go.bufio_package;
using types = go.go.types_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gccgoimporter_package
    {
        // Information about a specific installation of gccgo.
        public partial struct GccgoInstallation
        {
            public @string GccVersion; // Target triple (e.g. x86_64-unknown-linux-gnu).
            public @string TargetTriple; // Built-in library paths used by this installation.
            public slice<@string> LibPaths;
        }

        // Ask the driver at the given path for information for this GccgoInstallation.
        private static error InitFromDriver(this ref GccgoInstallation inst, @string gccgoPath)
        {
            var cmd = exec.Command(gccgoPath, "-###", "-S", "-x", "go", "-");
            var (stderr, err) = cmd.StderrPipe();
            if (err != null)
            {
                return;
            }
            err = cmd.Start();
            if (err != null)
            {
                return;
            }
            var scanner = bufio.NewScanner(stderr);
            while (scanner.Scan())
            {
                var line = scanner.Text();

                if (strings.HasPrefix(line, "Target: ")) 
                    inst.TargetTriple = line[8L..];
                else if (line[0L] == ' ') 
                    var args = strings.Fields(line);
                    foreach (var (_, arg) in args[1L..])
                    {
                        if (strings.HasPrefix(arg, "-L"))
                        {
                            inst.LibPaths = append(inst.LibPaths, arg[2L..]);
                        }
                    }
                            }


            var (stdout, err) = exec.Command(gccgoPath, "-dumpversion").Output();
            if (err != null)
            {
                return;
            }
            inst.GccVersion = strings.TrimSpace(string(stdout));

            return;
        }

        // Return the list of export search paths for this GccgoInstallation.
        private static slice<@string> SearchPaths(this ref GccgoInstallation inst)
        {
            foreach (var (_, lpath) in inst.LibPaths)
            {
                var spath = filepath.Join(lpath, "go", inst.GccVersion);
                var (fi, err) = os.Stat(spath);
                if (err != null || !fi.IsDir())
                {
                    continue;
                }
                paths = append(paths, spath);

                spath = filepath.Join(spath, inst.TargetTriple);
                fi, err = os.Stat(spath);
                if (err != null || !fi.IsDir())
                {
                    continue;
                }
                paths = append(paths, spath);
            }
            paths = append(paths, inst.LibPaths);

            return;
        }

        // Return an importer that searches incpaths followed by the gcc installation's
        // built-in search paths and the current directory.
        private static Importer GetImporter(this ref GccgoInstallation inst, slice<@string> incpaths, map<ref types.Package, InitData> initmap)
        {
            return GetImporter(append(append(incpaths, inst.SearchPaths()), "."), initmap);
        }
    }
}}}
