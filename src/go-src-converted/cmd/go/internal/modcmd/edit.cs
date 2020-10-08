// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod edit

// package modcmd -- go2cs converted at 2020 October 08 04:36:43 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Go\src\cmd\go\internal\modcmd\edit.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modload = go.cmd.go.@internal.modload_package;
using work = go.cmd.go.@internal.work_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;
using System.ComponentModel;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modcmd_package
    {
        private static ptr<base.Command> cmdEdit = addr(new base.Command(UsageLine:"go mod edit [editing flags] [go.mod]",Short:"edit go.mod from tools or scripts",Long:`
Edit provides a command-line interface for editing go.mod,
for use primarily by tools or scripts. It reads only go.mod;
it does not look up information about the modules involved.
By default, edit reads and writes the go.mod file of the main module,
but a different target file can be specified after the editing flags.

The editing flags specify a sequence of editing operations.

The -fmt flag reformats the go.mod file without making other changes.
This reformatting is also implied by any other modifications that use or
rewrite the go.mod file. The only time this flag is needed is if no other
flags are specified, as in 'go mod edit -fmt'.

The -module flag changes the module's path (the go.mod file's module line).

The -require=path@version and -droprequire=path flags
add and drop a requirement on the given module path and version.
Note that -require overrides any existing requirements on path.
These flags are mainly for tools that understand the module graph.
Users should prefer 'go get path@version' or 'go get path@none',
which make other go.mod adjustments as needed to satisfy
constraints imposed by other modules.

The -exclude=path@version and -dropexclude=path@version flags
add and drop an exclusion for the given module path and version.
Note that -exclude=path@version is a no-op if that exclusion already exists.

The -replace=old[@v]=new[@v] flag adds a replacement of the given
module path and version pair. If the @v in old@v is omitted, a
replacement without a version on the left side is added, which applies
to all versions of the old module path. If the @v in new@v is omitted,
the new path should be a local module root directory, not a module
path. Note that -replace overrides any redundant replacements for old[@v],
so omitting @v will drop existing replacements for specific versions.

The -dropreplace=old[@v] flag drops a replacement of the given
module path and version pair. If the @v is omitted, a replacement without
a version on the left side is dropped.

The -require, -droprequire, -exclude, -dropexclude, -replace,
and -dropreplace editing flags may be repeated, and the changes
are applied in the order given.

The -go=version flag sets the expected Go language version.

The -print flag prints the final go.mod in its text format instead of
writing it back to go.mod.

The -json flag prints the final go.mod file in JSON format instead of
writing it back to go.mod. The JSON output corresponds to these Go types:

	type Module struct {
		Path string
		Version string
	}

	type GoMod struct {
		Module  Module
		Go      string
		Require []Require
		Exclude []Module
		Replace []Replace
	}

	type Require struct {
		Path string
		Version string
		Indirect bool
	}

	type Replace struct {
		Old Module
		New Module
	}

Note that this only describes the go.mod file itself, not other modules
referred to indirectly. For the full set of modules available to a build,
use 'go list -m -json all'.

For example, a tool can obtain the go.mod as a data structure by
parsing the output of 'go mod edit -json' and can then make changes
by invoking 'go mod edit' with -require, -exclude, and so on.
	`,));

        private static var editFmt = cmdEdit.Flag.Bool("fmt", false, "");        private static var editGo = cmdEdit.Flag.String("go", "", "");        private static var editJSON = cmdEdit.Flag.Bool("json", false, "");        private static var editPrint = cmdEdit.Flag.Bool("print", false, "");        private static var editModule = cmdEdit.Flag.String("module", "", "");        private static slice<Action<ptr<modfile.File>>> edits = default;

        public delegate void flagFunc(@string);

        private static @string String(this flagFunc f)
        {
            return "";
        }
        private static error Set(this flagFunc f, @string s)
        {
            f(s);

            return error.As(null!)!;
        }

        private static void init()
        {
            cmdEdit.Run = runEdit; // break init cycle

            cmdEdit.Flag.Var(flagFunc(flagRequire), "require", "");
            cmdEdit.Flag.Var(flagFunc(flagDropRequire), "droprequire", "");
            cmdEdit.Flag.Var(flagFunc(flagExclude), "exclude", "");
            cmdEdit.Flag.Var(flagFunc(flagDropReplace), "dropreplace", "");
            cmdEdit.Flag.Var(flagFunc(flagReplace), "replace", "");
            cmdEdit.Flag.Var(flagFunc(flagDropExclude), "dropexclude", "");

            work.AddModCommonFlags(cmdEdit);
            @base.AddBuildFlagsNX(_addr_cmdEdit.Flag);

        }

        private static void runEdit(ptr<base.Command> _addr_cmd, slice<@string> args) => func((defer, _, __) =>
        {
            ref base.Command cmd = ref _addr_cmd.val;

            var anyFlags = editModule != "" || editGo != "" || editJSON || editPrint || editFmt || len(edits) > 0L.val;

            if (!anyFlags)
            {
                @base.Fatalf("go mod edit: no flags specified (see 'go help mod edit').");
            }

            if (editJSON && editPrint.val)
            {
                @base.Fatalf("go mod edit: cannot use both -json and -print");
            }

            if (len(args) > 1L)
            {
                @base.Fatalf("go mod edit: too many arguments");
            }

            @string gomod = default;
            if (len(args) == 1L)
            {
                gomod = args[0L];
            }
            else
            {
                gomod = modload.ModFilePath();
            }

            if (editModule != "".val)
            {
                {
                    var err__prev2 = err;

                    var err = module.CheckImportPath(editModule.val);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: invalid -module: %v", err);
                    }

                    err = err__prev2;

                }

            }

            if (editGo != "".val)
            {
                if (!modfile.GoVersionRE.MatchString(editGo.val))
                {
                    @base.Fatalf("go mod: invalid -go option; expecting something like \"-go 1.12\"");
                }

            }

            var (data, err) = lockedfile.Read(gomod);
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            var (modFile, err) = modfile.Parse(gomod, data, null);
            if (err != null)
            {
                @base.Fatalf("go: errors parsing %s:\n%s", @base.ShortPath(gomod), err);
            }

            if (editModule != "".val)
            {
                modFile.AddModuleStmt(editModule.val);
            }

            if (editGo != "".val)
            {
                {
                    var err__prev2 = err;

                    err = modFile.AddGoStmt(editGo.val);

                    if (err != null)
                    {
                        @base.Fatalf("go: internal error: %v", err);
                    }

                    err = err__prev2;

                }

            }

            if (len(edits) > 0L)
            {
                foreach (var (_, edit) in edits)
                {
                    edit(modFile);
                }

            }

            modFile.SortBlocks();
            modFile.Cleanup(); // clean file after edits

            if (editJSON.val)
            {
                editPrintJSON(_addr_modFile);
                return ;
            }

            var (out, err) = modFile.Format();
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            if (editPrint.val)
            {
                os.Stdout.Write(out);
                return ;
            } 

            // Make a best-effort attempt to acquire the side lock, only to exclude
            // previous versions of the 'go' command from making simultaneous edits.
            {
                var err__prev1 = err;

                var (unlock, err) = modfetch.SideLock();

                if (err == null)
                {
                    defer(unlock());
                }

                err = err__prev1;

            }


            err = lockedfile.Transform(gomod, lockedData =>
            {
                if (!bytes.Equal(lockedData, data))
                {
                    return (null, errors.New("go.mod changed during editing; not overwriting"));
                }

                return (out, null);

            });
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

        });

        // parsePathVersion parses -flag=arg expecting arg to be path@version.
        private static (@string, @string) parsePathVersion(@string flag, @string arg)
        {
            @string path = default;
            @string version = default;

            var i = strings.Index(arg, "@");
            if (i < 0L)
            {
                @base.Fatalf("go mod: -%s=%s: need path@version", flag, arg);
            }

            path = strings.TrimSpace(arg[..i]);
            version = strings.TrimSpace(arg[i + 1L..]);
            {
                var err = module.CheckImportPath(path);

                if (err != null)
                {
                    @base.Fatalf("go mod: -%s=%s: invalid path: %v", flag, arg, err);
                } 

                // We don't call modfile.CheckPathVersion, because that insists
                // on versions being in semver form, but here we want to allow
                // versions like "master" or "1234abcdef", which the go command will resolve
                // the next time it runs (or during -fix).
                // Even so, we need to make sure the version is a valid token.

            } 

            // We don't call modfile.CheckPathVersion, because that insists
            // on versions being in semver form, but here we want to allow
            // versions like "master" or "1234abcdef", which the go command will resolve
            // the next time it runs (or during -fix).
            // Even so, we need to make sure the version is a valid token.
            if (modfile.MustQuote(version))
            {
                @base.Fatalf("go mod: -%s=%s: invalid version %q", flag, arg, version);
            }

            return (path, version);

        }

        // parsePath parses -flag=arg expecting arg to be path (not path@version).
        private static @string parsePath(@string flag, @string arg)
        {
            @string path = default;

            if (strings.Contains(arg, "@"))
            {
                @base.Fatalf("go mod: -%s=%s: need just path, not path@version", flag, arg);
            }

            path = arg;
            {
                var err = module.CheckImportPath(path);

                if (err != null)
                {
                    @base.Fatalf("go mod: -%s=%s: invalid path: %v", flag, arg, err);
                }

            }

            return path;

        }

        // parsePathVersionOptional parses path[@version], using adj to
        // describe any errors.
        private static (@string, @string, error) parsePathVersionOptional(@string adj, @string arg, bool allowDirPath)
        {
            @string path = default;
            @string version = default;
            error err = default!;

            {
                var i = strings.Index(arg, "@");

                if (i < 0L)
                {
                    path = arg;
                }
                else
                {
                    path = strings.TrimSpace(arg[..i]);
                    version = strings.TrimSpace(arg[i + 1L..]);

                }

            }

            {
                var err = module.CheckImportPath(path);

                if (err != null)
                {
                    if (!allowDirPath || !modfile.IsDirectoryPath(path))
                    {
                        return (path, version, error.As(fmt.Errorf("invalid %s path: %v", adj, err))!);
                    }

                }

            }

            if (path != arg && modfile.MustQuote(version))
            {
                return (path, version, error.As(fmt.Errorf("invalid %s version: %q", adj, version))!);
            }

            return (path, version, error.As(null!)!);

        }

        // flagRequire implements the -require flag.
        private static void flagRequire(@string arg)
        {
            var (path, version) = parsePathVersion("require", arg);
            edits = append(edits, f =>
            {
                {
                    var err = f.AddRequire(path, version);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: -require=%s: %v", arg, err);
                    }

                }

            });

        }

        // flagDropRequire implements the -droprequire flag.
        private static void flagDropRequire(@string arg)
        {
            var path = parsePath("droprequire", arg);
            edits = append(edits, f =>
            {
                {
                    var err = f.DropRequire(path);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: -droprequire=%s: %v", arg, err);
                    }

                }

            });

        }

        // flagExclude implements the -exclude flag.
        private static void flagExclude(@string arg)
        {
            var (path, version) = parsePathVersion("exclude", arg);
            edits = append(edits, f =>
            {
                {
                    var err = f.AddExclude(path, version);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: -exclude=%s: %v", arg, err);
                    }

                }

            });

        }

        // flagDropExclude implements the -dropexclude flag.
        private static void flagDropExclude(@string arg)
        {
            var (path, version) = parsePathVersion("dropexclude", arg);
            edits = append(edits, f =>
            {
                {
                    var err = f.DropExclude(path, version);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: -dropexclude=%s: %v", arg, err);
                    }

                }

            });

        }

        // flagReplace implements the -replace flag.
        private static void flagReplace(@string arg)
        {
            long i = default;
            i = strings.Index(arg, "=");

            if (i < 0L)
            {
                @base.Fatalf("go mod: -replace=%s: need old[@v]=new[@w] (missing =)", arg);
            }

            var old = strings.TrimSpace(arg[..i]);
            var @new = strings.TrimSpace(arg[i + 1L..]);
            if (strings.HasPrefix(new, ">"))
            {
                @base.Fatalf("go mod: -replace=%s: separator between old and new is =, not =>", arg);
            }

            var (oldPath, oldVersion, err) = parsePathVersionOptional("old", old, false);
            if (err != null)
            {
                @base.Fatalf("go mod: -replace=%s: %v", arg, err);
            }

            var (newPath, newVersion, err) = parsePathVersionOptional("new", new, true);
            if (err != null)
            {
                @base.Fatalf("go mod: -replace=%s: %v", arg, err);
            }

            if (newPath == new && !modfile.IsDirectoryPath(new))
            {
                @base.Fatalf("go mod: -replace=%s: unversioned new path must be local directory", arg);
            }

            edits = append(edits, f =>
            {
                {
                    var err = f.AddReplace(oldPath, oldVersion, newPath, newVersion);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: -replace=%s: %v", arg, err);
                    }

                }

            });

        }

        // flagDropReplace implements the -dropreplace flag.
        private static void flagDropReplace(@string arg)
        {
            var (path, version, err) = parsePathVersionOptional("old", arg, true);
            if (err != null)
            {
                @base.Fatalf("go mod: -dropreplace=%s: %v", arg, err);
            }

            edits = append(edits, f =>
            {
                {
                    var err = f.DropReplace(path, version);

                    if (err != null)
                    {
                        @base.Fatalf("go mod: -dropreplace=%s: %v", arg, err);
                    }

                }

            });

        }

        // fileJSON is the -json output data structure.
        private partial struct fileJSON
        {
            public module.Version Module;
            [Description("json:\",omitempty\"")]
            public @string Go;
            public slice<requireJSON> Require;
            public slice<module.Version> Exclude;
            public slice<replaceJSON> Replace;
        }

        private partial struct requireJSON
        {
            public @string Path;
            [Description("json:\",omitempty\"")]
            public @string Version;
            [Description("json:\",omitempty\"")]
            public bool Indirect;
        }

        private partial struct replaceJSON
        {
            public module.Version Old;
            public module.Version New;
        }

        // editPrintJSON prints the -json output.
        private static void editPrintJSON(ptr<modfile.File> _addr_modFile)
        {
            ref modfile.File modFile = ref _addr_modFile.val;

            ref fileJSON f = ref heap(out ptr<fileJSON> _addr_f);
            if (modFile.Module != null)
            {
                f.Module = modFile.Module.Mod;
            }

            if (modFile.Go != null)
            {
                f.Go = modFile.Go.Version;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in modFile.Require)
                {
                    r = __r;
                    f.Require = append(f.Require, new requireJSON(Path:r.Mod.Path,Version:r.Mod.Version,Indirect:r.Indirect));
                }

                r = r__prev1;
            }

            foreach (var (_, x) in modFile.Exclude)
            {
                f.Exclude = append(f.Exclude, x.Mod);
            }
            {
                var r__prev1 = r;

                foreach (var (_, __r) in modFile.Replace)
                {
                    r = __r;
                    f.Replace = append(f.Replace, new replaceJSON(r.Old,r.New));
                }

                r = r__prev1;
            }

            var (data, err) = json.MarshalIndent(_addr_f, "", "\t");
            if (err != null)
            {
                @base.Fatalf("go: internal error: %v", err);
            }

            data = append(data, '\n');
            os.Stdout.Write(data);

        }
    }
}}}}
