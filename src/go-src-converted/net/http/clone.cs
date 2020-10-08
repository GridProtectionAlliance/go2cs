// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 08 03:38:27 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\clone.go
using multipart = go.mime.multipart_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        private static url.Values cloneURLValues(url.Values v)
        {
            if (v == null)
            {
                return null;
            }
            return url.Values(Header(v).Clone());

        }

        private static ptr<url.URL> cloneURL(ptr<url.URL> _addr_u)
        {
            ref url.URL u = ref _addr_u.val;

            if (u == null)
            {
                return _addr_null!;
            }

            ptr<url.URL> u2 = @new<url.URL>();
            u2.val = u;
            if (u.User != null)
            {
                u2.User = @new<url.Userinfo>();
                u2.User.val = u.User.val;
            }

            return _addr_u2!;

        }

        private static ptr<multipart.Form> cloneMultipartForm(ptr<multipart.Form> _addr_f)
        {
            ref multipart.Form f = ref _addr_f.val;

            if (f == null)
            {
                return _addr_null!;
            }

            ptr<multipart.Form> f2 = addr(new multipart.Form(Value:(map[string][]string)(Header(f.Value).Clone()),));
            if (f.File != null)
            {
                var m = make_map<@string, slice<ptr<multipart.FileHeader>>>();
                foreach (var (k, vv) in f.File)
                {
                    var vv2 = make_slice<ptr<multipart.FileHeader>>(len(vv));
                    foreach (var (i, v) in vv)
                    {
                        vv2[i] = cloneMultipartFileHeader(_addr_v);
                    }
                    m[k] = vv2;

                }
                f2.File = m;

            }

            return _addr_f2!;

        }

        private static ptr<multipart.FileHeader> cloneMultipartFileHeader(ptr<multipart.FileHeader> _addr_fh)
        {
            ref multipart.FileHeader fh = ref _addr_fh.val;

            if (fh == null)
            {
                return _addr_null!;
            }

            ptr<multipart.FileHeader> fh2 = @new<multipart.FileHeader>();
            fh2.val = fh;
            fh2.Header = textproto.MIMEHeader(Header(fh.Header).Clone());
            return _addr_fh2!;

        }

        // cloneOrMakeHeader invokes Header.Clone but if the
        // result is nil, it'll instead make and return a non-nil Header.
        private static Header cloneOrMakeHeader(Header hdr)
        {
            var clone = hdr.Clone();
            if (clone == null)
            {
                clone = make(Header);
            }

            return clone;

        }
    }
}}
