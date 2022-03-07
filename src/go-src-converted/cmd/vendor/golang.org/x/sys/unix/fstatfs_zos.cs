// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build zos && s390x
// +build zos,s390x

// package unix -- go2cs converted at 2022 March 06 23:26:34 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\fstatfs_zos.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // This file simulates fstatfs on z/OS using fstatvfs and w_getmntent.
public static error Fstatfs(nint fd, ptr<Statfs_t> _addr_stat) {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    ref Statvfs_t stat_v = ref heap(out ptr<Statvfs_t> _addr_stat_v);
    err = Fstatvfs(fd, _addr_stat_v);
    if (err == null) { 
        // populate stat
        stat.Type = 0;
        stat.Bsize = stat_v.Bsize;
        stat.Blocks = stat_v.Blocks;
        stat.Bfree = stat_v.Bfree;
        stat.Bavail = stat_v.Bavail;
        stat.Files = stat_v.Files;
        stat.Ffree = stat_v.Ffree;
        stat.Fsid = stat_v.Fsid;
        stat.Namelen = stat_v.Namemax;
        stat.Frsize = stat_v.Frsize;
        stat.Flags = stat_v.Flag;
        for (nint passn = 0; passn < 5; passn++) {
            switch (passn) {
                case 0: 
                    err = tryGetmntent64(_addr_stat);
                    break;
                    break;
                case 1: 
                    err = tryGetmntent128(_addr_stat);
                    break;
                    break;
                case 2: 
                    err = tryGetmntent256(_addr_stat);
                    break;
                    break;
                case 3: 
                    err = tryGetmntent512(_addr_stat);
                    break;
                    break;
                case 4: 
                    err = tryGetmntent1024(_addr_stat);
                    break;
                    break;
                default: 
                    break;
                    break;
            } 
            //proceed to return if: err is nil (found), err is nonnil but not ERANGE (another error occurred)
            if (err == null || err != null && err != ERANGE) {
                break;
            }
        }

    }
    return error.As(err)!;

}

private static error tryGetmntent64(ptr<Statfs_t> _addr_stat) {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    ref var mnt_ent_buffer = ref heap(out ptr<var> _addr_mnt_ent_buffer);
    nint buffer_size = int(@unsafe.Sizeof(mnt_ent_buffer));
    var (fs_count, err) = W_Getmntent((byte.val)(@unsafe.Pointer(_addr_mnt_ent_buffer)), buffer_size);
    if (err != null) {
        return error.As(err)!;
    }
    err = ERANGE; //return ERANGE if no match is found in this batch
    for (nint i = 0; i < fs_count; i++) {
        if (stat.Fsid == uint64(mnt_ent_buffer.filesys_info[i].Dev)) {
            stat.Type = uint32(mnt_ent_buffer.filesys_info[i].Fstname[0]);
            err = null;
            break;
        }
    }
    return error.As(err)!;

}

private static error tryGetmntent128(ptr<Statfs_t> _addr_stat) {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    ref var mnt_ent_buffer = ref heap(out ptr<var> _addr_mnt_ent_buffer);
    nint buffer_size = int(@unsafe.Sizeof(mnt_ent_buffer));
    var (fs_count, err) = W_Getmntent((byte.val)(@unsafe.Pointer(_addr_mnt_ent_buffer)), buffer_size);
    if (err != null) {
        return error.As(err)!;
    }
    err = ERANGE; //return ERANGE if no match is found in this batch
    for (nint i = 0; i < fs_count; i++) {
        if (stat.Fsid == uint64(mnt_ent_buffer.filesys_info[i].Dev)) {
            stat.Type = uint32(mnt_ent_buffer.filesys_info[i].Fstname[0]);
            err = null;
            break;
        }
    }
    return error.As(err)!;

}

private static error tryGetmntent256(ptr<Statfs_t> _addr_stat) {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    ref var mnt_ent_buffer = ref heap(out ptr<var> _addr_mnt_ent_buffer);
    nint buffer_size = int(@unsafe.Sizeof(mnt_ent_buffer));
    var (fs_count, err) = W_Getmntent((byte.val)(@unsafe.Pointer(_addr_mnt_ent_buffer)), buffer_size);
    if (err != null) {
        return error.As(err)!;
    }
    err = ERANGE; //return ERANGE if no match is found in this batch
    for (nint i = 0; i < fs_count; i++) {
        if (stat.Fsid == uint64(mnt_ent_buffer.filesys_info[i].Dev)) {
            stat.Type = uint32(mnt_ent_buffer.filesys_info[i].Fstname[0]);
            err = null;
            break;
        }
    }
    return error.As(err)!;

}

private static error tryGetmntent512(ptr<Statfs_t> _addr_stat) {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    ref var mnt_ent_buffer = ref heap(out ptr<var> _addr_mnt_ent_buffer);
    nint buffer_size = int(@unsafe.Sizeof(mnt_ent_buffer));
    var (fs_count, err) = W_Getmntent((byte.val)(@unsafe.Pointer(_addr_mnt_ent_buffer)), buffer_size);
    if (err != null) {
        return error.As(err)!;
    }
    err = ERANGE; //return ERANGE if no match is found in this batch
    for (nint i = 0; i < fs_count; i++) {
        if (stat.Fsid == uint64(mnt_ent_buffer.filesys_info[i].Dev)) {
            stat.Type = uint32(mnt_ent_buffer.filesys_info[i].Fstname[0]);
            err = null;
            break;
        }
    }
    return error.As(err)!;

}

private static error tryGetmntent1024(ptr<Statfs_t> _addr_stat) {
    error err = default!;
    ref Statfs_t stat = ref _addr_stat.val;

    ref var mnt_ent_buffer = ref heap(out ptr<var> _addr_mnt_ent_buffer);
    nint buffer_size = int(@unsafe.Sizeof(mnt_ent_buffer));
    var (fs_count, err) = W_Getmntent((byte.val)(@unsafe.Pointer(_addr_mnt_ent_buffer)), buffer_size);
    if (err != null) {
        return error.As(err)!;
    }
    err = ERANGE; //return ERANGE if no match is found in this batch
    for (nint i = 0; i < fs_count; i++) {
        if (stat.Fsid == uint64(mnt_ent_buffer.filesys_info[i].Dev)) {
            stat.Type = uint32(mnt_ent_buffer.filesys_info[i].Fstname[0]);
            err = null;
            break;
        }
    }
    return error.As(err)!;

}

} // end unix_package
