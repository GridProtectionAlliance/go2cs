// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package user -- go2cs converted at 2020 August 29 08:31:53 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup_windows.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static void init()
        {
            groupImplemented = false;
        }

        private static (bool, error) isDomainJoined()
        {
            ref ushort domain = default;
            uint status = default;
            var err = syscall.NetGetJoinInformation(null, ref domain, ref status);
            if (err != null)
            {
                return (false, err);
            }
            syscall.NetApiBufferFree((byte.Value)(@unsafe.Pointer(domain)));
            return (status == syscall.NetSetupDomainName, null);
        }

        private static (@string, error) lookupFullNameDomain(@string domainAndUser)
        {
            return syscall.TranslateAccountName(domainAndUser, syscall.NameSamCompatible, syscall.NameDisplay, 50L);
        }

        private static (@string, error) lookupFullNameServer(@string servername, @string username) => func((defer, _, __) =>
        {
            var (s, e) = syscall.UTF16PtrFromString(servername);
            if (e != null)
            {
                return ("", e);
            }
            var (u, e) = syscall.UTF16PtrFromString(username);
            if (e != null)
            {
                return ("", e);
            }
            ref byte p = default;
            e = syscall.NetUserGetInfo(s, u, 10L, ref p);
            if (e != null)
            {
                return ("", e);
            }
            defer(syscall.NetApiBufferFree(p));
            var i = (syscall.UserInfo10.Value)(@unsafe.Pointer(p));
            if (i.FullName == null)
            {
                return ("", null);
            }
            var name = syscall.UTF16ToString(new ptr<ref array<ushort>>(@unsafe.Pointer(i.FullName))[..]);
            return (name, null);
        });

        private static (@string, error) lookupFullName(@string domain, @string username, @string domainAndUser)
        {
            var (joined, err) = isDomainJoined();
            if (err == null && joined)
            {
                var (name, err) = lookupFullNameDomain(domainAndUser);
                if (err == null)
                {
                    return (name, null);
                }
            }
            (name, err) = lookupFullNameServer(domain, username);
            if (err == null)
            {
                return (name, null);
            } 
            // domain worked neither as a domain nor as a server
            // could be domain server unavailable
            // pretend username is fullname
            return (username, null);
        }

        private static (ref User, error) newUser(ref syscall.SID usid, @string gid, @string dir)
        {
            var (username, domain, t, e) = usid.LookupAccount("");
            if (e != null)
            {
                return (null, e);
            }
            if (t != syscall.SidTypeUser)
            {
                return (null, fmt.Errorf("user: should be user account type, not %d", t));
            }
            var domainAndUser = domain + "\\" + username;
            var (uid, e) = usid.String();
            if (e != null)
            {
                return (null, e);
            }
            var (name, e) = lookupFullName(domain, username, domainAndUser);
            if (e != null)
            {
                return (null, e);
            }
            User u = ref new User(Uid:uid,Gid:gid,Username:domainAndUser,Name:name,HomeDir:dir,);
            return (u, null);
        }

        private static (ref User, error) current() => func((defer, _, __) =>
        {
            var (t, e) = syscall.OpenCurrentProcessToken();
            if (e != null)
            {
                return (null, e);
            }
            defer(t.Close());
            var (u, e) = t.GetTokenUser();
            if (e != null)
            {
                return (null, e);
            }
            var (pg, e) = t.GetTokenPrimaryGroup();
            if (e != null)
            {
                return (null, e);
            }
            var (gid, e) = pg.PrimaryGroup.String();
            if (e != null)
            {
                return (null, e);
            }
            var (dir, e) = t.GetUserProfileDirectory();
            if (e != null)
            {
                return (null, e);
            }
            return newUser(u.User.Sid, gid, dir);
        });

        // BUG(brainman): Lookup and LookupId functions do not set
        // Gid and HomeDir fields in the User struct returned on windows.

        private static (ref User, error) newUserFromSid(ref syscall.SID usid)
        { 
            // TODO(brainman): do not know where to get gid and dir fields
            @string gid = "unknown";
            @string dir = "Unknown directory";
            return newUser(usid, gid, dir);
        }

        private static (ref User, error) lookupUser(@string username)
        {
            var (sid, _, t, e) = syscall.LookupSID("", username);
            if (e != null)
            {
                return (null, e);
            }
            if (t != syscall.SidTypeUser)
            {
                return (null, fmt.Errorf("user: should be user account type, not %d", t));
            }
            return newUserFromSid(sid);
        }

        private static (ref User, error) lookupUserId(@string uid)
        {
            var (sid, e) = syscall.StringToSid(uid);
            if (e != null)
            {
                return (null, e);
            }
            return newUserFromSid(sid);
        }

        private static (ref Group, error) lookupGroup(@string groupname)
        {
            return (null, errors.New("user: LookupGroup not implemented on windows"));
        }

        private static (ref Group, error) lookupGroupId(@string _p0)
        {
            return (null, errors.New("user: LookupGroupId not implemented on windows"));
        }

        private static (slice<@string>, error) listGroups(ref User _p0)
        {
            return (null, errors.New("user: GroupIds not implemented on windows"));
        }
    }
}}
