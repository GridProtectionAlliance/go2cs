// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package get -- go2cs converted at 2020 October 08 04:33:39 UTC
// import "cmd/go/internal/get" ==> using get = go.cmd.go.@internal.get_package
// Original source: C:\Go\src\cmd\go\internal\get\discovery.go
using xml = go.encoding.xml_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class get_package
    {
        // charsetReader returns a reader that converts from the given charset to UTF-8.
        // Currently it only supports UTF-8 and ASCII. Otherwise, it returns a meaningful
        // error which is printed by go get, so the user can find why the package
        // wasn't downloaded if the encoding is not supported. Note that, in
        // order to reduce potential errors, ASCII is treated as UTF-8 (i.e. characters
        // greater than 0x7f are not rejected).
        private static (io.Reader, error) charsetReader(@string charset, io.Reader input)
        {
            io.Reader _p0 = default;
            error _p0 = default!;

            switch (strings.ToLower(charset))
            {
                case "utf-8": 

                case "ascii": 
                    return (input, error.As(null!)!);
                    break;
                default: 
                    return (null, error.As(fmt.Errorf("can't decode XML document using charset %q", charset))!);
                    break;
            }

        }

        // parseMetaGoImports returns meta imports from the HTML in r.
        // Parsing ends at the end of the <head> section or the beginning of the <body>.
        private static (slice<metaImport>, error) parseMetaGoImports(io.Reader r, ModuleMode mod)
        {
            slice<metaImport> _p0 = default;
            error _p0 = default!;

            var d = xml.NewDecoder(r);
            d.CharsetReader = charsetReader;
            d.Strict = false;
            slice<metaImport> imports = default;
            while (true)
            {
                var (t, err) = d.RawToken();
                if (err != null)
                {
                    if (err != io.EOF && len(imports) == 0L)
                    {
                        return (null, error.As(err)!);
                    }

                    break;

                }

                {
                    xml.StartElement e__prev1 = e;

                    xml.StartElement (e, ok) = t._<xml.StartElement>();

                    if (ok && strings.EqualFold(e.Name.Local, "body"))
                    {
                        break;
                    }

                    e = e__prev1;

                }

                {
                    xml.StartElement e__prev1 = e;

                    (e, ok) = t._<xml.EndElement>();

                    if (ok && strings.EqualFold(e.Name.Local, "head"))
                    {
                        break;
                    }

                    e = e__prev1;

                }

                (e, ok) = t._<xml.StartElement>();
                if (!ok || !strings.EqualFold(e.Name.Local, "meta"))
                {
                    continue;
                }

                if (attrValue(e.Attr, "name") != "go-import")
                {
                    continue;
                }

                {
                    var f = strings.Fields(attrValue(e.Attr, "content"));

                    if (len(f) == 3L)
                    {
                        imports = append(imports, new metaImport(Prefix:f[0],VCS:f[1],RepoRoot:f[2],));
                    }

                }

            } 

            // Extract mod entries if we are paying attention to them.
 

            // Extract mod entries if we are paying attention to them.
            slice<metaImport> list = default;
            map<@string, bool> have = default;
            if (mod == PreferMod)
            {
                have = make_map<@string, bool>();
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in imports)
                    {
                        m = __m;
                        if (m.VCS == "mod")
                        {
                            have[m.Prefix] = true;
                            list = append(list, m);
                        }

                    }

                    m = m__prev1;
                }
            } 

            // Append non-mod entries, ignoring those superseded by a mod entry.
            {
                var m__prev1 = m;

                foreach (var (_, __m) in imports)
                {
                    m = __m;
                    if (m.VCS != "mod" && !have[m.Prefix])
                    {
                        list = append(list, m);
                    }

                }

                m = m__prev1;
            }

            return (list, error.As(null!)!);

        }

        // attrValue returns the attribute value for the case-insensitive key
        // `name', or the empty string if nothing is found.
        private static @string attrValue(slice<xml.Attr> attrs, @string name)
        {
            foreach (var (_, a) in attrs)
            {
                if (strings.EqualFold(a.Name.Local, name))
                {
                    return a.Value;
                }

            }
            return "";

        }
    }
}}}}
