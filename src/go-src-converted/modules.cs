// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package packagestest -- go2cs converted at 2020 October 08 04:55:55 UTC
// import "golang.org/x/tools/go/packages/packagestest" ==> using packagestest = go.golang.org.x.tools.go.packages.packagestest_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\packagestest\modules.go
using context = go.context_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strings = go.strings_package;

using gocommand = go.golang.org.x.tools.@internal.gocommand_package;
using packagesinternal = go.golang.org.x.tools.@internal.packagesinternal_package;
using proxydir = go.golang.org.x.tools.@internal.proxydir_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace packages
{
    public static partial class packagestest_package
    {
        // Modules is the exporter that produces module layouts.
        // Each "repository" is put in it's own module, and the module file generated
        // will have replace directives for all other modules.
        // Given the two files
        //     golang.org/repoa#a/a.go
        //     golang.org/repob#b/b.go
        // You would get the directory layout
        //     /sometemporarydirectory
        //     ├── repoa
        //     │   ├── a
        //     │   │   └── a.go
        //     │   └── go.mod
        //     └── repob
        //         ├── b
        //         │   └── b.go
        //         └── go.mod
        // and the working directory would be
        //     /sometemporarydirectory/repoa
        public static modules Modules = new modules();

        private partial struct modules
        {
        }

        private partial struct moduleAtVersion
        {
            public @string module;
            public @string version;
        }

        private static @string Name(this modules _p0)
        {
            return "Modules";
        }

        private static @string Filename(this modules _p0, ptr<Exported> _addr_exported, @string module, @string fragment)
        {
            ref Exported exported = ref _addr_exported.val;

            if (module == exported.primary)
            {
                return filepath.Join(primaryDir(_addr_exported), fragment);
            }

            return filepath.Join(moduleDir(_addr_exported, module), fragment);

        }

        private static error Finalize(this modules _p0, ptr<Exported> _addr_exported)
        {
            ref Exported exported = ref _addr_exported.val;
 
            // Write out the primary module. This module can use symlinks and
            // other weird stuff, and will be the working dir for the go command.
            // It depends on all the other modules.
            var primaryDir = primaryDir(_addr_exported);
            {
                var err__prev1 = err;

                var err = os.MkdirAll(primaryDir, 0755L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            exported.Config.Dir = primaryDir;
            if (exported.written[exported.primary] == null)
            {
                exported.written[exported.primary] = make_map<@string, @string>();
            } 

            // Create a map of modulepath -> {module, version} for modulepaths
            // that are of the form `repoa/mod1@v1.1.0`.
            var versions = make_map<@string, moduleAtVersion>();
            {
                var module__prev1 = module;

                foreach (var (__module) in exported.written)
                {
                    module = __module;
                    {
                        var splt = strings.Split(module, "@");

                        if (len(splt) > 1L)
                        {
                            versions[module] = new moduleAtVersion(module:splt[0],version:splt[1],);
                        }

                    }

                } 

                // If the primary module already has a go.mod, write the contents to a temp
                // go.mod for now and then we will reset it when we are getting all the markers.

                module = module__prev1;
            }

            {
                var gomod = exported.written[exported.primary]["go.mod"];

                if (gomod != "")
                {
                    var (contents, err) = ioutil.ReadFile(gomod);
                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    {
                        var err__prev2 = err;

                        err = ioutil.WriteFile(gomod + ".temp", contents, 0644L);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                }

            }


            exported.written[exported.primary]["go.mod"] = filepath.Join(primaryDir, "go.mod");
            @string primaryGomod = "module " + exported.primary + "\nrequire (\n";
            foreach (var (other) in exported.written)
            {
                if (other == exported.primary)
                {
                    continue;
                }

                var version = moduleVersion(other); 
                // If other is of the form `repo1/mod1@v1.1.0`,
                // then we need to extract the module and the version.
                {
                    var v__prev1 = v;

                    var (v, ok) = versions[other];

                    if (ok)
                    {
                        other = v.module;
                        version = v.version;
                    }

                    v = v__prev1;

                }

                primaryGomod += fmt.Sprintf("\t%v %v\n", other, version);

            }
            primaryGomod += ")\n";
            {
                var err__prev1 = err;

                err = ioutil.WriteFile(filepath.Join(primaryDir, "go.mod"), (slice<byte>)primaryGomod, 0644L);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Create the mod cache so we can rename it later, even if we don't need it.

                err = err__prev1;

            } 

            // Create the mod cache so we can rename it later, even if we don't need it.
            {
                var err__prev1 = err;

                err = os.MkdirAll(modCache(_addr_exported), 0755L);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Write out the go.mod files for the other modules.

                err = err__prev1;

            } 

            // Write out the go.mod files for the other modules.
            {
                var module__prev1 = module;
                var files__prev1 = files;

                foreach (var (__module, __files) in exported.written)
                {
                    module = __module;
                    files = __files;
                    if (module == exported.primary)
                    {
                        continue;
                    }

                    var dir = moduleDir(_addr_exported, module);
                    var modfile = filepath.Join(dir, "go.mod"); 
                    // If other is of the form `repo1/mod1@v1.1.0`,
                    // then we need to extract the module name without the version.
                    {
                        var v__prev1 = v;

                        (v, ok) = versions[module];

                        if (ok)
                        {
                            module = v.module;
                        }

                        v = v__prev1;

                    }

                    {
                        var err__prev1 = err;

                        err = ioutil.WriteFile(modfile, (slice<byte>)"module " + module + "\n", 0644L);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev1;

                    }

                    files["go.mod"] = modfile;

                } 

                // Zip up all the secondary modules into the proxy dir.

                module = module__prev1;
                files = files__prev1;
            }

            var modProxyDir = filepath.Join(exported.temp, "modproxy");
            {
                var module__prev1 = module;
                var files__prev1 = files;

                foreach (var (__module, __files) in exported.written)
                {
                    module = __module;
                    files = __files;
                    if (module == exported.primary)
                    {
                        continue;
                    }

                    version = moduleVersion(module); 
                    // If other is of the form `repo1/mod1@v1.1.0`,
                    // then we need to extract the module and the version.
                    {
                        var v__prev1 = v;

                        (v, ok) = versions[module];

                        if (ok)
                        {
                            module = v.module;
                            version = v.version;
                        }

                        v = v__prev1;

                    }

                    {
                        var err__prev1 = err;

                        err = writeModuleFiles(modProxyDir, module, version, files);

                        if (err != null)
                        {
                            return error.As(fmt.Errorf("creating module proxy dir for %v: %v", module, err))!;
                        }

                        err = err__prev1;

                    }

                } 

                // Discard the original mod cache dir, which contained the files written
                // for us by Export.

                module = module__prev1;
                files = files__prev1;
            }

            {
                var err__prev1 = err;

                err = os.Rename(modCache(_addr_exported), modCache(_addr_exported) + ".orig");

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            exported.Config.Env = append(exported.Config.Env, "GO111MODULE=on", "GOPATH=" + filepath.Join(exported.temp, "modcache"), "GOPROXY=" + proxydir.ToURL(modProxyDir), "GOSUMDB=off");
            ptr<gocommand.Runner> gocmdRunner = addr(new gocommand.Runner());
            packagesinternal.SetGoCmdRunner(exported.Config, gocmdRunner); 

            // Run go mod download to recreate the mod cache dir with all the extra
            // stuff in cache. All the files created by Export should be recreated.
            gocommand.Invocation inv = new gocommand.Invocation(Verb:"mod",Args:[]string{"download"},Env:exported.Config.Env,BuildFlags:exported.Config.BuildFlags,WorkingDir:exported.Config.Dir,);
            {
                var err__prev1 = err;

                var (_, err) = gocmdRunner.Run(context.Background(), inv);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            return error.As(null!)!;

        }

        private static error writeModuleFiles(@string rootDir, @string module, @string ver, map<@string, @string> filePaths)
        {
            var fileData = make_map<@string, slice<byte>>();
            foreach (var (name, path) in filePaths)
            {
                var (contents, err) = ioutil.ReadFile(path);
                if (err != null)
                {
                    return error.As(err)!;
                }

                fileData[name] = contents;

            }
            return error.As(proxydir.WriteModuleVersion(rootDir, module, ver, fileData))!;

        }

        private static @string modCache(ptr<Exported> _addr_exported)
        {
            ref Exported exported = ref _addr_exported.val;

            return filepath.Join(exported.temp, "modcache/pkg/mod");
        }

        private static @string primaryDir(ptr<Exported> _addr_exported)
        {
            ref Exported exported = ref _addr_exported.val;

            return filepath.Join(exported.temp, path.Base(exported.primary));
        }

        private static @string moduleDir(ptr<Exported> _addr_exported, @string module)
        {
            ref Exported exported = ref _addr_exported.val;

            if (strings.Contains(module, "@"))
            {
                return filepath.Join(modCache(_addr_exported), module);
            }

            return filepath.Join(modCache(_addr_exported), path.Dir(module), path.Base(module) + "@" + moduleVersion(module));

        }

        private static var versionSuffixRE = regexp.MustCompile("v\\d+");

        private static @string moduleVersion(@string module)
        {
            if (versionSuffixRE.MatchString(path.Base(module)))
            {
                return path.Base(module) + ".0.0";
            }

            return "v1.0.0";

        }
    }
}}}}}}
