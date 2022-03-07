// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package token -- go2cs converted at 2022 March 06 22:25:54 UTC
// import "go/token" ==> using token = go.go.token_package
// Original source: C:\Program Files\Go\src\go\token\serialize.go

using System;


namespace go.go;

public static partial class token_package {

private partial struct serializedFile {
    public @string Name;
    public nint Base;
    public nint Size;
    public slice<nint> Lines;
    public slice<lineInfo> Infos;
}

private partial struct serializedFileSet {
    public nint Base;
    public slice<serializedFile> Files;
}

// Read calls decode to deserialize a file set into s; s must not be nil.
private static error Read(this ptr<FileSet> _addr_s, Func<object, error> decode) {
    ref FileSet s = ref _addr_s.val;

    ref serializedFileSet ss = ref heap(out ptr<serializedFileSet> _addr_ss);
    {
        var err = decode(_addr_ss);

        if (err != null) {
            return error.As(err)!;
        }
    }


    s.mutex.Lock();
    s.@base = ss.Base;
    var files = make_slice<ptr<File>>(len(ss.Files));
    for (nint i = 0; i < len(ss.Files); i++) {
        var f = _addr_ss.Files[i];
        files[i] = addr(new File(set:s,name:f.Name,base:f.Base,size:f.Size,lines:f.Lines,infos:f.Infos,));
    }
    s.files = files;
    s.last = null;
    s.mutex.Unlock();

    return error.As(null!)!;

}

// Write calls encode to serialize the file set s.
private static error Write(this ptr<FileSet> _addr_s, Func<object, error> encode) {
    ref FileSet s = ref _addr_s.val;

    serializedFileSet ss = default;

    s.mutex.Lock();
    ss.Base = s.@base;
    var files = make_slice<serializedFile>(len(s.files));
    foreach (var (i, f) in s.files) {
        f.mutex.Lock();
        files[i] = new serializedFile(Name:f.name,Base:f.base,Size:f.size,Lines:append([]int(nil),f.lines...),Infos:append([]lineInfo(nil),f.infos...),);
        f.mutex.Unlock();
    }    ss.Files = files;
    s.mutex.Unlock();

    return error.As(encode(ss))!;
}

} // end token_package
