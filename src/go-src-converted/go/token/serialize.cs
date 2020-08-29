// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package token -- go2cs converted at 2020 August 29 08:48:09 UTC
// import "go/token" ==> using token = go.go.token_package
// Original source: C:\Go\src\go\token\serialize.go

using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class token_package
    {
        private partial struct serializedFile
        {
            public @string Name;
            public long Base;
            public long Size;
            public slice<long> Lines;
            public slice<lineInfo> Infos;
        }

        private partial struct serializedFileSet
        {
            public long Base;
            public slice<serializedFile> Files;
        }

        // Read calls decode to deserialize a file set into s; s must not be nil.
        private static error Read(this ref FileSet s, Func<object, error> decode)
        {
            serializedFileSet ss = default;
            {
                var err = decode(ref ss);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            s.mutex.Lock();
            s.@base = ss.Base;
            var files = make_slice<ref File>(len(ss.Files));
            for (long i = 0L; i < len(ss.Files); i++)
            {
                var f = ref ss.Files[i];
                files[i] = ref new File(set:s,name:f.Name,base:f.Base,size:f.Size,lines:f.Lines,infos:f.Infos,);
            }

            s.files = files;
            s.last = null;
            s.mutex.Unlock();

            return error.As(null);
        }

        // Write calls encode to serialize the file set s.
        private static error Write(this ref FileSet s, Func<object, error> encode)
        {
            serializedFileSet ss = default;

            s.mutex.Lock();
            ss.Base = s.@base;
            var files = make_slice<serializedFile>(len(s.files));
            foreach (var (i, f) in s.files)
            {
                f.mutex.Lock();
                files[i] = new serializedFile(Name:f.name,Base:f.base,Size:f.size,Lines:append([]int(nil),f.lines...),Infos:append([]lineInfo(nil),f.infos...),);
                f.mutex.Unlock();
            }
            ss.Files = files;
            s.mutex.Unlock();

            return error.As(encode(ss));
        }
    }
}}
