// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package x509 -- go2cs converted at 2022 March 06 22:19:51 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\root_unix.go
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

namespace go.crypto;

public static partial class x509_package {

 
// certFileEnv is the environment variable which identifies where to locate
// the SSL certificate file. If set this overrides the system default.
private static readonly @string certFileEnv = "SSL_CERT_FILE"; 

// certDirEnv is the environment variable which identifies which directory
// to check for SSL certificate files. If set this overrides the system default.
// It is a colon separated list of directories.
// See https://www.openssl.org/docs/man1.0.2/man1/c_rehash.html.
private static readonly @string certDirEnv = "SSL_CERT_DIR";


private static (slice<slice<ptr<Certificate>>>, error) systemVerify(this ptr<Certificate> _addr_c, ptr<VerifyOptions> _addr_opts) {
    slice<slice<ptr<Certificate>>> chains = default;
    error err = default!;
    ref Certificate c = ref _addr_c.val;
    ref VerifyOptions opts = ref _addr_opts.val;

    return (null, error.As(null!)!);
}

private static (ptr<CertPool>, error) loadSystemRoots() {
    ptr<CertPool> _p0 = default!;
    error _p0 = default!;

    var roots = NewCertPool();

    var files = certFiles;
    {
        var f = os.Getenv(certFileEnv);

        if (f != "") {
            files = new slice<@string>(new @string[] { f });
        }
    }


    error firstErr = default!;
    foreach (var (_, file) in files) {
        var (data, err) = os.ReadFile(file);
        if (err == null) {
            roots.AppendCertsFromPEM(data);
            break;
        }
        if (firstErr == null && !os.IsNotExist(err)) {
            firstErr = error.As(err)!;
        }
    }    var dirs = certDirectories;
    {
        var d = os.Getenv(certDirEnv);

        if (d != "") { 
            // OpenSSL and BoringSSL both use ":" as the SSL_CERT_DIR separator.
            // See:
            //  * https://golang.org/issue/35325
            //  * https://www.openssl.org/docs/man1.0.2/man1/c_rehash.html
            dirs = strings.Split(d, ":");

        }
    }


    foreach (var (_, directory) in dirs) {
        var (fis, err) = readUniqueDirectoryEntries(directory);
        if (err != null) {
            if (firstErr == null && !os.IsNotExist(err)) {
                firstErr = error.As(err)!;
            }
            continue;
        }
        foreach (var (_, fi) in fis) {
            (data, err) = os.ReadFile(directory + "/" + fi.Name());
            if (err == null) {
                roots.AppendCertsFromPEM(data);
            }
        }
    }    if (roots.len() > 0 || firstErr == null) {
        return (_addr_roots!, error.As(null!)!);
    }
    return (_addr_null!, error.As(firstErr)!);

}

// readUniqueDirectoryEntries is like os.ReadDir but omits
// symlinks that point within the directory.
private static (slice<fs.DirEntry>, error) readUniqueDirectoryEntries(@string dir) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;

    var (files, err) = os.ReadDir(dir);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var uniq = files[..(int)0];
    foreach (var (_, f) in files) {
        if (!isSameDirSymlink(f, dir)) {
            uniq = append(uniq, f);
        }
    }    return (uniq, error.As(null!)!);

}

// isSameDirSymlink reports whether fi in dir is a symlink with a
// target not containing a slash.
private static bool isSameDirSymlink(fs.DirEntry f, @string dir) {
    if (f.Type() & fs.ModeSymlink == 0) {
        return false;
    }
    var (target, err) = os.Readlink(filepath.Join(dir, f.Name()));
    return err == null && !strings.Contains(target, "/");

}

} // end x509_package
