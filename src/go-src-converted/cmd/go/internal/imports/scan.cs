// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package imports -- go2cs converted at 2020 October 09 05:45:46 UTC
// import "cmd/go/internal/imports" ==> using imports = go.cmd.go.@internal.imports_package
// Original source: C:\Go\src\cmd\go\internal\imports\scan.go
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class imports_package
    {
        public static (slice<@string>, slice<@string>, error) ScanDir(@string dir, map<@string, bool> tags)
        {
            slice<@string> _p0 = default;
            slice<@string> _p0 = default;
            error _p0 = default!;

            var (infos, err) = ioutil.ReadDir(dir);
            if (err != null)
            {
                return (null, null, error.As(err)!);
            }
            slice<@string> files = default;
            foreach (var (_, info) in infos)
            {
                var name = info.Name(); 

                // If the directory entry is a symlink, stat it to obtain the info for the
                // link target instead of the link itself.
                if (info.Mode() & os.ModeSymlink != 0L)
                {
                    info, err = os.Stat(filepath.Join(dir, name));
                    if (err != null)
                    {
                        continue; // Ignore broken symlinks.
                    }
                }
                if (info.Mode().IsRegular() && !strings.HasPrefix(name, "_") && strings.HasSuffix(name, ".go") && MatchFile(name, tags))
                {
                    files = append(files, filepath.Join(dir, name));
                }
            }            return scanFiles(files, tags, false);

        }

        public static (slice<@string>, slice<@string>, error) ScanFiles(slice<@string> files, map<@string, bool> tags)
        {
            slice<@string> _p0 = default;
            slice<@string> _p0 = default;
            error _p0 = default!;

            return scanFiles(files, tags, true);
        }

        private static (slice<@string>, slice<@string>, error) scanFiles(slice<@string> files, map<@string, bool> tags, bool explicitFiles)
        {
            slice<@string> _p0 = default;
            slice<@string> _p0 = default;
            error _p0 = default!;

            var imports = make_map<@string, bool>();
            var testImports = make_map<@string, bool>();
            long numFiles = 0L;
Files:
            foreach (var (_, name) in files)
            {
                var (r, err) = os.Open(name);
                if (err != null)
                {
                    return (null, null, error.As(err)!);
                }

                ref slice<@string> list = ref heap(out ptr<slice<@string>> _addr_list);
                var (data, err) = ReadImports(r, false, _addr_list);
                r.Close();
                if (err != null)
                {
                    return (null, null, error.As(fmt.Errorf("reading %s: %v", name, err))!);
                } 

                // import "C" is implicit requirement of cgo tag.
                // When listing files on the command line (explicitFiles=true)
                // we do not apply build tag filtering but we still do apply
                // cgo filtering, so no explicitFiles check here.
                // Why? Because we always have, and it's not worth breaking
                // that behavior now.
                foreach (var (_, path) in list)
                {
                    if (path == "\"C\"" && !tags["cgo"] && !tags["*"])
                    {
                        _continueFiles = true;
                        break;
                    }

                }
                if (!explicitFiles && !ShouldBuild(data, tags))
                {
                    continue;
                }

                numFiles++;
                var m = imports;
                if (strings.HasSuffix(name, "_test.go"))
                {
                    m = testImports;
                }

                foreach (var (_, p) in list)
                {
                    var (q, err) = strconv.Unquote(p);
                    if (err != null)
                    {
                        continue;
                    }

                    m[q] = true;

                }

            }
            if (numFiles == 0L)
            {
                return (null, null, error.As(ErrNoGo)!);
            }

            return (keys(imports), keys(testImports), error.As(null!)!);

        }

        public static var ErrNoGo = fmt.Errorf("no Go source files");

        private static slice<@string> keys(map<@string, bool> m)
        {
            slice<@string> list = default;
            foreach (var (k) in m)
            {
                list = append(list, k);
            }
            sort.Strings(list);
            return list;

        }
    }
}}}}
