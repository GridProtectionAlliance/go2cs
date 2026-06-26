// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

partial class token_package {

[GoType] partial struct serializedFile {
    // fields correspond 1:1 to fields with same (lower-case) name in File
    public @string Name;
    public nint Base;
    public nint Size;
    public slice<nint> Lines;
    public slice<lineInfo> Infos;
}

[GoType] partial struct serializedFileSet {
    public nint Base;
    public slice<serializedFile> Files;
}

// Read calls decode to deserialize a file set into s; s must not be nil.
[GoRecv] public static error Read(this ref FileSet s, Func<any, error> decode) {
    ref var ss = ref heap(new serializedFileSet(), out var Ꮡss);
    {
        var err = decode(Ꮡss); if (err != default!) {
            return err;
        }
    }
    s.mutex.Lock();
    s.@base = ss.Base;
    var files = new slice<ж<ΔFile>>(len(ss.Files));
    for (nint i = 0; i < len(ss.Files); i++) {
        var f = Ꮡ(ss.Files, i);
        files[i] = Ꮡ(new ΔFile(
            name: (~f).Name,
            @base: (~f).Base,
            size: (~f).Size,
            lines: (~f).Lines,
            infos: (~f).Infos
        ));
    }
    s.files = files;
    s.last.Store(nil);
    s.mutex.Unlock();
    return default!;
}

// Write calls encode to serialize the file set s.
[GoRecv] public static error Write(this ref FileSet s, Func<any, error> encode) {
    serializedFileSet ss = default!;
    s.mutex.Lock();
    ss.Base = s.@base;
    var files = new slice<serializedFile>(len(s.files));
    foreach (var (i, f) in s.files) {
        (~f).mutex.Lock();
        files[i] = new serializedFile(
            Name: (~f).name,
            Base: (~f).@base,
            Size: (~f).size,
            Lines: append(slice<nint>(default!), (~f).lines.ꓸꓸꓸ),
            Infos: append(slice<lineInfo>(default!), (~f).infos.ꓸꓸꓸ)
        );
        (~f).mutex.Unlock();
    }
    ss.Files = files;
    s.mutex.Unlock();
    return encode(ss);
}

} // end token_package
