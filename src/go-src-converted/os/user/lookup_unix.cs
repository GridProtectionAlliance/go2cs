// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (aix || darwin || dragonfly || freebsd || (js && wasm) || (!android && linux) || netbsd || openbsd || solaris) && (!cgo || osusergo)
// +build aix darwin dragonfly freebsd js,wasm !android,linux netbsd openbsd solaris
// +build !cgo osusergo

// package user -- go2cs converted at 2022 March 06 22:14:30 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\lookup_unix.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using System;


namespace go.os;

public static partial class user_package {

private static readonly @string groupFile = "/etc/group";

private static readonly @string userFile = "/etc/passwd";



private static byte colon = new slice<byte>(new byte[] { ':' });

private static void init() {
    groupImplemented = false;
}

// lineFunc returns a value, an error, or (nil, nil) to skip the row.
public delegate  error) lineFunc(slice<byte>,  (object);

// readColonFile parses r as an /etc/group or /etc/passwd style file, running
// fn for each row. readColonFile returns a value, an error, or (nil, nil) if
// the end of the file is reached without a match.
//
// readCols is the minimum number of colon-separated fields that will be passed
// to fn; in a long line additional fields may be silently discarded.
private static (object, error) readColonFile(io.Reader r, lineFunc fn, nint readCols) {
    object v = default;
    error err = default!;

    var rd = bufio.NewReader(r); 

    // Read the file line-by-line.
    while (true) {
        bool isPrefix = default;
        slice<byte> wholeLine = default; 

        // Read the next line. We do so in chunks (as much as reader's
        // buffer is able to keep), check if we read enough columns
        // already on each step and store final result in wholeLine.
        while (true) {
            slice<byte> line = default;
            line, isPrefix, err = rd.ReadLine();

            if (err != null) { 
                // We should return (nil, nil) if EOF is reached
                // without a match.
                if (err == io.EOF) {
                    err = null;
                }

                return (null, error.As(err)!);

            } 

            // Simple common case: line is short enough to fit in a
            // single reader's buffer.
            if (!isPrefix && len(wholeLine) == 0) {
                wholeLine = line;
                break;
            }

            wholeLine = append(wholeLine, line); 

            // Check if we read the whole line (or enough columns)
            // already.
            if (!isPrefix || bytes.Count(wholeLine, new slice<byte>(new byte[] { ':' })) >= readCols) {
                break;
            }

        } 

        // There's no spec for /etc/passwd or /etc/group, but we try to follow
        // the same rules as the glibc parser, which allows comments and blank
        // space at the beginning of a line.
        wholeLine = bytes.TrimSpace(wholeLine);
        if (len(wholeLine) == 0 || wholeLine[0] == '#') {
            continue;
        }
        v, err = fn(wholeLine);
        if (v != null || err != null) {
            return ;
        }
        while (isPrefix) {
            if (err != null) { 
                // We should return (nil, nil) if EOF is reached without a match.
                if (err == io.EOF) {
                    err = null;
            _, isPrefix, err = rd.ReadLine();
                }

                return (null, error.As(err)!);

            }

        }

    }

}

private static lineFunc matchGroupIndexValue(@string value, nint idx) {
    @string leadColon = default;
    if (idx > 0) {
        leadColon = ":";
    }
    slice<byte> substr = (slice<byte>)leadColon + value + ":";
    return line => {
        if (!bytes.Contains(line, substr) || bytes.Count(line, colon) < 3) {
            return ;
        }
        var parts = strings.SplitN(string(line), ":", 4);
        if (len(parts) < 4 || parts[0] == "" || parts[idx] != value || parts[0][0] == '+' || parts[0][0] == '-') {
            return ;
        }
        {
            var (_, err) = strconv.Atoi(parts[2]);

            if (err != null) {
                return (null, null);
            }

        }

        return (addr(new Group(Name:parts[0],Gid:parts[2])), null);

    };

}

private static (ptr<Group>, error) findGroupId(@string id, io.Reader r) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    {
        var (v, err) = readColonFile(r, matchGroupIndexValue(id, 2), 3);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        else if (v != null) {
            return (v._<ptr<Group>>(), error.As(null!)!);
        }

    }

    return (_addr_null!, error.As(UnknownGroupIdError(id))!);

}

private static (ptr<Group>, error) findGroupName(@string name, io.Reader r) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    {
        var (v, err) = readColonFile(r, matchGroupIndexValue(name, 0), 3);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        else if (v != null) {
            return (v._<ptr<Group>>(), error.As(null!)!);
        }

    }

    return (_addr_null!, error.As(UnknownGroupError(name))!);

}

// returns a *User for a row if that row's has the given value at the
// given index.
private static lineFunc matchUserIndexValue(@string value, nint idx) {
    @string leadColon = default;
    if (idx > 0) {
        leadColon = ":";
    }
    slice<byte> substr = (slice<byte>)leadColon + value + ":";
    return line => {
        if (!bytes.Contains(line, substr) || bytes.Count(line, colon) < 6) {
            return ;
        }
        var parts = strings.SplitN(string(line), ":", 7);
        if (len(parts) < 6 || parts[idx] != value || parts[0] == "" || parts[0][0] == '+' || parts[0][0] == '-') {
            return ;
        }
        {
            var (_, err) = strconv.Atoi(parts[2]);

            if (err != null) {
                return (null, null);
            }

        }

        {
            (_, err) = strconv.Atoi(parts[3]);

            if (err != null) {
                return (null, null);
            }

        }

        ptr<User> u = addr(new User(Username:parts[0],Uid:parts[2],Gid:parts[3],Name:parts[4],HomeDir:parts[5],)); 
        // The pw_gecos field isn't quite standardized. Some docs
        // say: "It is expected to be a comma separated list of
        // personal data where the first item is the full name of the
        // user."
        {
            var i = strings.Index(u.Name, ",");

            if (i >= 0) {
                u.Name = u.Name[..(int)i];
            }

        }

        return (u, null);

    };

}

private static (ptr<User>, error) findUserId(@string uid, io.Reader r) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    var (i, e) = strconv.Atoi(uid);
    if (e != null) {
        return (_addr_null!, error.As(errors.New("user: invalid userid " + uid))!);
    }
    {
        var (v, err) = readColonFile(r, matchUserIndexValue(uid, 2), 6);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        else if (v != null) {
            return (v._<ptr<User>>(), error.As(null!)!);
        }

    }

    return (_addr_null!, error.As(UnknownUserIdError(i))!);

}

private static (ptr<User>, error) findUsername(@string name, io.Reader r) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    {
        var (v, err) = readColonFile(r, matchUserIndexValue(name, 0), 6);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        else if (v != null) {
            return (v._<ptr<User>>(), error.As(null!)!);
        }

    }

    return (_addr_null!, error.As(UnknownUserError(name))!);

}

private static (ptr<Group>, error) lookupGroup(@string groupname) => func((defer, _, _) => {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(groupFile);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(f.Close());
    return _addr_findGroupName(groupname, f)!;

});

private static (ptr<Group>, error) lookupGroupId(@string id) => func((defer, _, _) => {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(groupFile);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(f.Close());
    return _addr_findGroupId(id, f)!;

});

private static (ptr<User>, error) lookupUser(@string username) => func((defer, _, _) => {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(userFile);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(f.Close());
    return _addr_findUsername(username, f)!;

});

private static (ptr<User>, error) lookupUserId(@string uid) => func((defer, _, _) => {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(userFile);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(f.Close());
    return _addr_findUserId(uid, f)!;

});

} // end user_package
