// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

partial class fs_package {

// A StatFS is a file system with a Stat method.
[GoType] partial interface StatFS :
    FS
{
    // Stat returns a FileInfo describing the file.
    // If there is an error, it should be of type *PathError.
    (FileInfo, error) Stat(@string name);
}

// Stat returns a [FileInfo] describing the named file from the file system.
//
// If fs implements [StatFS], Stat calls fs.Stat.
// Otherwise, Stat opens the [File] to stat it.
public static (FileInfo, error) Stat(FS fsys, @string name) => func((defer, _) => {
    {
        var (fsysΔ1, ok) = fsys._<StatFS>(ᐧ); if (ok) {
            return fsysΔ1.Stat(name);
        }
    }
    (file, err) = fsys.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    var fileʗ1 = file;
    defer(fileʗ1.Close);
    return file.Stat();
});

} // end fs_package
