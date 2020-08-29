// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd !android,linux nacl netbsd openbsd solaris
// +build !cgo

// package user -- go2cs converted at 2020 August 29 08:31:52 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup_unix.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static readonly @string groupFile = "/etc/group";

        private static readonly @string userFile = "/etc/passwd";



        private static byte colon = new slice<byte>(new byte[] { ':' });

        private static void init()
        {
            groupImplemented = false;
        }

        // lineFunc returns a value, an error, or (nil, nil) to skip the row.
        public delegate  error) lineFunc(slice<byte>,  (object);

        // readColonFile parses r as an /etc/group or /etc/passwd style file, running
        // fn for each row. readColonFile returns a value, an error, or (nil, nil) if
        // the end of the file is reached without a match.
        private static (object, error) readColonFile(io.Reader r, lineFunc fn)
        {
            var bs = bufio.NewScanner(r);
            while (bs.Scan())
            {
                var line = bs.Bytes(); 
                // There's no spec for /etc/passwd or /etc/group, but we try to follow
                // the same rules as the glibc parser, which allows comments and blank
                // space at the beginning of a line.
                line = bytes.TrimSpace(line);
                if (len(line) == 0L || line[0L] == '#')
                {
                    continue;
                }
                v, err = fn(line);
                if (v != null || err != null)
                {
                    return;
                }
            }

            return (null, bs.Err());
        }

        private static lineFunc matchGroupIndexValue(@string value, long idx)
        {
            @string leadColon = default;
            if (idx > 0L)
            {
                leadColon = ":";
            }
            slice<byte> substr = (slice<byte>)leadColon + value + ":";
            return line =>
            {
                if (!bytes.Contains(line, substr) || bytes.Count(line, colon) < 3L)
                {
                    return;
                } 
                // wheel:*:0:root
                var parts = strings.SplitN(string(line), ":", 4L);
                if (len(parts) < 4L || parts[0L] == "" || parts[idx] != value || parts[0L][0L] == '+' || parts[0L][0L] == '-')
                {
                    return;
                }
                {
                    var (_, err) = strconv.Atoi(parts[2L]);

                    if (err != null)
                    {
                        return (null, null);
                    }

                }
                return (ref new Group(Name:parts[0],Gid:parts[2]), null);
            }
;
        }

        private static (ref Group, error) findGroupId(@string id, io.Reader r)
        {
            {
                var (v, err) = readColonFile(r, matchGroupIndexValue(id, 2L));

                if (err != null)
                {
                    return (null, err);
                }
                else if (v != null)
                {
                    return (v._<ref Group>(), null);
                }

            }
            return (null, UnknownGroupIdError(id));
        }

        private static (ref Group, error) findGroupName(@string name, io.Reader r)
        {
            {
                var (v, err) = readColonFile(r, matchGroupIndexValue(name, 0L));

                if (err != null)
                {
                    return (null, err);
                }
                else if (v != null)
                {
                    return (v._<ref Group>(), null);
                }

            }
            return (null, UnknownGroupError(name));
        }

        // returns a *User for a row if that row's has the given value at the
        // given index.
        private static lineFunc matchUserIndexValue(@string value, long idx)
        {
            @string leadColon = default;
            if (idx > 0L)
            {
                leadColon = ":";
            }
            slice<byte> substr = (slice<byte>)leadColon + value + ":";
            return line =>
            {
                if (!bytes.Contains(line, substr) || bytes.Count(line, colon) < 6L)
                {
                    return;
                } 
                // kevin:x:1005:1006::/home/kevin:/usr/bin/zsh
                var parts = strings.SplitN(string(line), ":", 7L);
                if (len(parts) < 6L || parts[idx] != value || parts[0L] == "" || parts[0L][0L] == '+' || parts[0L][0L] == '-')
                {
                    return;
                }
                {
                    var (_, err) = strconv.Atoi(parts[2L]);

                    if (err != null)
                    {
                        return (null, null);
                    }

                }
                {
                    (_, err) = strconv.Atoi(parts[3L]);

                    if (err != null)
                    {
                        return (null, null);
                    }

                }
                User u = ref new User(Username:parts[0],Uid:parts[2],Gid:parts[3],Name:parts[4],HomeDir:parts[5],); 
                // The pw_gecos field isn't quite standardized. Some docs
                // say: "It is expected to be a comma separated list of
                // personal data where the first item is the full name of the
                // user."
                {
                    var i = strings.Index(u.Name, ",");

                    if (i >= 0L)
                    {
                        u.Name = u.Name[..i];
                    }

                }
                return (u, null);
            }
;
        }

        private static (ref User, error) findUserId(@string uid, io.Reader r)
        {
            var (i, e) = strconv.Atoi(uid);
            if (e != null)
            {
                return (null, errors.New("user: invalid userid " + uid));
            }
            {
                var (v, err) = readColonFile(r, matchUserIndexValue(uid, 2L));

                if (err != null)
                {
                    return (null, err);
                }
                else if (v != null)
                {
                    return (v._<ref User>(), null);
                }

            }
            return (null, UnknownUserIdError(i));
        }

        private static (ref User, error) findUsername(@string name, io.Reader r)
        {
            {
                var (v, err) = readColonFile(r, matchUserIndexValue(name, 0L));

                if (err != null)
                {
                    return (null, err);
                }
                else if (v != null)
                {
                    return (v._<ref User>(), null);
                }

            }
            return (null, UnknownUserError(name));
        }

        private static (ref Group, error) lookupGroup(@string groupname) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(groupFile);
            if (err != null)
            {
                return (null, err);
            }
            defer(f.Close());
            return findGroupName(groupname, f);
        });

        private static (ref Group, error) lookupGroupId(@string id) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(groupFile);
            if (err != null)
            {
                return (null, err);
            }
            defer(f.Close());
            return findGroupId(id, f);
        });

        private static (ref User, error) lookupUser(@string username) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(userFile);
            if (err != null)
            {
                return (null, err);
            }
            defer(f.Close());
            return findUsername(username, f);
        });

        private static (ref User, error) lookupUserId(@string uid) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(userFile);
            if (err != null)
            {
                return (null, err);
            }
            defer(f.Close());
            return findUserId(uid, f);
        });
    }
}}
