// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package staticdata -- go2cs converted at 2022 March 13 06:00:17 UTC
// import "cmd/compile/internal/staticdata" ==> using staticdata = go.cmd.compile.@internal.staticdata_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\staticdata\embed.go
namespace go.cmd.compile.@internal;

using path = path_package;
using sort = sort_package;
using strings = strings_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using System;

public static partial class staticdata_package {

private static readonly var embedUnknown = iota;
private static readonly var embedBytes = 0;
private static readonly var embedString = 1;
private static readonly var embedFiles = 2;

private static slice<@string> embedFileList(ptr<ir.Name> _addr_v, nint kind) {
    ref ir.Name v = ref _addr_v.val;
 
    // Build list of files to store.
    var have = make_map<@string, bool>();
    slice<@string> list = default;
    foreach (var (_, e) in v.Embed.val) {
        foreach (var (_, pattern) in e.Patterns) {
            var (files, ok) = @base.Flag.Cfg.Embed.Patterns[pattern];
            if (!ok) {
                @base.ErrorfAt(e.Pos, "invalid go:embed: build system did not map pattern: %s", pattern);
            }
            foreach (var (_, file) in files) {
                if (@base.Flag.Cfg.Embed.Files[file] == "") {
                    @base.ErrorfAt(e.Pos, "invalid go:embed: build system did not map file: %s", file);
                    continue;
                }
                if (!have[file]) {
                    have[file] = true;
                    list = append(list, file);
                }
                if (kind == embedFiles) {
                    {
                        var dir = path.Dir(file);

                        while (dir != "." && !have[dir]) {
                            have[dir] = true;
                            list = append(list, dir + "/");
                            dir = path.Dir(dir);
                        }

                    }
                }
            }
        }
    }    sort.Slice(list, (i, j) => embedFileLess(list[i], list[j]));

    if (kind == embedString || kind == embedBytes) {
        if (len(list) > 1) {
            @base.ErrorfAt(v.Pos(), "invalid go:embed: multiple files for type %v", v.Type());
            return null;
        }
    }
    return list;
}

// embedKind determines the kind of embedding variable.
private static nint embedKind(ptr<types.Type> _addr_typ) {
    ref types.Type typ = ref _addr_typ.val;

    if (typ.Sym() != null && typ.Sym().Name == "FS" && (typ.Sym().Pkg.Path == "embed" || (typ.Sym().Pkg == types.LocalPkg && @base.Ctxt.Pkgpath == "embed"))) {
        return embedFiles;
    }
    if (typ.Kind() == types.TSTRING) {
        return embedString;
    }
    if (typ.IsSlice() && typ.Elem().Kind() == types.TUINT8) {
        return embedBytes;
    }
    return embedUnknown;
}

private static (@string, @string, bool) embedFileNameSplit(@string name) {
    @string dir = default;
    @string elem = default;
    bool isDir = default;

    if (name[len(name) - 1] == '/') {
        isDir = true;
        name = name[..(int)len(name) - 1];
    }
    var i = len(name) - 1;
    while (i >= 0 && name[i] != '/') {
        i--;
    }
    if (i < 0) {
        return (".", name, isDir);
    }
    return (name[..(int)i], name[(int)i + 1..], isDir);
}

// embedFileLess implements the sort order for a list of embedded files.
// See the comment inside ../../../../embed/embed.go's Files struct for rationale.
private static bool embedFileLess(@string x, @string y) {
    var (xdir, xelem, _) = embedFileNameSplit(x);
    var (ydir, yelem, _) = embedFileNameSplit(y);
    return xdir < ydir || xdir == ydir && xelem < yelem;
}

// WriteEmbed emits the init data for a //go:embed variable,
// which is either a string, a []byte, or an embed.FS.
public static void WriteEmbed(ptr<ir.Name> _addr_v) {
    ref ir.Name v = ref _addr_v.val;
 
    // TODO(mdempsky): User errors should be reported by the frontend.

    var commentPos = (v.Embed.val)[0].Pos;
    if (!types.AllowsGoVersion(types.LocalPkg, 1, 16)) {
        var prevPos = @base.Pos;
        @base.Pos = commentPos;
        @base.ErrorfVers("go1.16", "go:embed");
        @base.Pos = prevPos;
        return ;
    }
    if (@base.Flag.Cfg.Embed.Patterns == null) {
        @base.ErrorfAt(commentPos, "invalid go:embed: build system did not supply embed configuration");
        return ;
    }
    var kind = embedKind(_addr_v.Type());
    if (kind == embedUnknown) {
        @base.ErrorfAt(v.Pos(), "go:embed cannot apply to var of type %v", v.Type());
        return ;
    }
    var files = embedFileList(_addr_v, kind);

    if (kind == embedString || kind == embedBytes) 
        var file = files[0];
        var (fsym, size, err) = fileStringSym(v.Pos(), @base.Flag.Cfg.Embed.Files[file], kind == embedString, null);
        if (err != null) {
            @base.ErrorfAt(v.Pos(), "embed %s: %v", file, err);
        }
        var sym = v.Linksym();
        nint off = 0;
        off = objw.SymPtr(sym, off, fsym, 0); // data string
        off = objw.Uintptr(sym, off, uint64(size)); // len
        if (kind == embedBytes) {
            objw.Uintptr(sym, off, uint64(size)); // cap for slice
        }
    else if (kind == embedFiles) 
        var slicedata = @base.Ctxt.Lookup("\"\"." + v.Sym().Name + ".files");
        off = 0; 
        // []files pointed at by Files
        off = objw.SymPtr(slicedata, off, slicedata, 3 * types.PtrSize); // []file, pointing just past slice
        off = objw.Uintptr(slicedata, off, uint64(len(files)));
        off = objw.Uintptr(slicedata, off, uint64(len(files))); 

        // embed/embed.go type file is:
        //    name string
        //    data string
        //    hash [16]byte
        // Emit one of these per file in the set.
        const nint hashSize = 16;

        var hash = make_slice<byte>(hashSize);
        {
            var file__prev1 = file;

            foreach (var (_, __file) in files) {
                file = __file;
                off = objw.SymPtr(slicedata, off, StringSym(v.Pos(), file), 0); // file string
                off = objw.Uintptr(slicedata, off, uint64(len(file)));
                if (strings.HasSuffix(file, "/")) { 
                    // entry for directory - no data
                    off = objw.Uintptr(slicedata, off, 0);
                    off = objw.Uintptr(slicedata, off, 0);
                    off += hashSize;
                }
                else
 {
                    (fsym, size, err) = fileStringSym(v.Pos(), @base.Flag.Cfg.Embed.Files[file], true, hash);
                    if (err != null) {
                        @base.ErrorfAt(v.Pos(), "embed %s: %v", file, err);
                    }
                    off = objw.SymPtr(slicedata, off, fsym, 0); // data string
                    off = objw.Uintptr(slicedata, off, uint64(size));
                    off = int(slicedata.WriteBytes(@base.Ctxt, int64(off), hash));
                }
            }

            file = file__prev1;
        }

        objw.Global(slicedata, int32(off), obj.RODATA | obj.LOCAL);
        sym = v.Linksym();
        objw.SymPtr(sym, 0, slicedata, 0);
    }

} // end staticdata_package
