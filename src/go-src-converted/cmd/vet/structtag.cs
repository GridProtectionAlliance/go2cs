// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for canonical struct tags.

// package main -- go2cs converted at 2020 August 29 10:09:30 UTC
// Original source: C:\Go\src\cmd\vet\structtag.go
using errors = go.errors_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("structtags", "check that struct field tags have canonical format and apply to exported fields as needed", checkStructFieldTags, structType);
        }

        // checkStructFieldTags checks all the field tags of a struct, including checking for duplicates.
        private static void checkStructFieldTags(ref File f, ast.Node node)
        {
            map<array<@string>, token.Pos> seen = default;
            foreach (var (_, field) in node._<ref ast.StructType>().Fields.List)
            {
                checkCanonicalFieldTag(f, field, ref seen);
            }
        }

        private static @string checkTagDups = new slice<@string>(new @string[] { "json", "xml" });
        private static map checkTagSpaces = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"json":true,"xml":true,"asn1":true};

        // checkCanonicalFieldTag checks a single struct field tag.
        private static void checkCanonicalFieldTag(ref File f, ref ast.Field field, ref map<array<@string>, token.Pos> seen)
        {
            if (field.Tag == null)
            {
                return;
            }
            var (tag, err) = strconv.Unquote(field.Tag.Value);
            if (err != null)
            {
                f.Badf(field.Pos(), "unable to read struct tag %s", field.Tag.Value);
                return;
            }
            {
                var err = validateStructTag(tag);

                if (err != null)
                {
                    var (raw, _) = strconv.Unquote(field.Tag.Value); // field.Tag.Value is known to be a quoted string
                    f.Badf(field.Pos(), "struct field tag %#q not compatible with reflect.StructTag.Get: %s", raw, err);
                }

            }

            foreach (var (_, key) in checkTagDups)
            {
                var val = reflect.StructTag(tag).Get(key);
                if (val == "" || val == "-" || val[0L] == ',')
                {
                    continue;
                }
                if (key == "xml" && len(field.Names) > 0L && field.Names[0L].Name == "XMLName")
                { 
                    // XMLName defines the XML element name of the struct being
                    // checked. That name cannot collide with element or attribute
                    // names defined on other fields of the struct. Vet does not have a
                    // check for untagged fields of type struct defining their own name
                    // by containing a field named XMLName; see issue 18256.
                    continue;
                }
                {
                    var i = strings.Index(val, ",");

                    if (i >= 0L)
                    {
                        if (key == "xml")
                        { 
                            // Use a separate namespace for XML attributes.
                            foreach (var (_, opt) in strings.Split(val[i..], ","))
                            {
                                if (opt == "attr")
                                {
                                    key += " attribute"; // Key is part of the error message.
                                    break;
                                }
                            }
                        }
                        val = val[..i];
                    }

                }
                if (seen == null.Value)
                {
                    seen.Value = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<array<@string>, token.Pos>{};
                }
                {
                    var (pos, ok) = (seen.Value)[new array<@string>(new @string[] { key, val })];

                    if (ok)
                    {
                        @string name = default;
                        if (len(field.Names) > 0L)
                        {
                            name = field.Names[0L].Name;
                        }
                        else
                        {
                            name = field.Type._<ref ast.Ident>().Name;
                        }
                        f.Badf(field.Pos(), "struct field %s repeats %s tag %q also at %s", name, key, val, f.loc(pos));
                    }
                    else
                    {
                        (seen.Value)[new array<@string>(new @string[] { key, val })] = field.Pos();
                    }

                }
            } 

            // Check for use of json or xml tags with unexported fields.

            // Embedded struct. Nothing to do for now, but that
            // may change, depending on what happens with issue 7363.
            if (len(field.Names) == 0L)
            {
                return;
            }
            if (field.Names[0L].IsExported())
            {
                return;
            }
            foreach (var (_, enc) in new array<@string>(new @string[] { "json", "xml" }))
            {
                if (reflect.StructTag(tag).Get(enc) != "")
                {
                    f.Badf(field.Pos(), "struct field %s has %s tag but is not exported", field.Names[0L].Name, enc);
                    return;
                }
            }
        }

        private static var errTagSyntax = errors.New("bad syntax for struct tag pair");        private static var errTagKeySyntax = errors.New("bad syntax for struct tag key");        private static var errTagValueSyntax = errors.New("bad syntax for struct tag value");        private static var errTagValueSpace = errors.New("suspicious space in struct tag value");        private static var errTagSpace = errors.New("key:\"value\" pairs not separated by spaces");

        // validateStructTag parses the struct tag and returns an error if it is not
        // in the canonical format, which is a space-separated list of key:"value"
        // settings. The value may contain spaces.
        private static error validateStructTag(@string tag)
        { 
            // This code is based on the StructTag.Get code in package reflect.

            long n = 0L;
            while (tag != "")
            {
                if (n > 0L && tag != "" && tag[0L] != ' ')
                { 
                    // More restrictive than reflect, but catches likely mistakes
                    // like `x:"foo",y:"bar"`, which parses as `x:"foo" ,y:"bar"` with second key ",y".
                    return error.As(errTagSpace);
                n++;
                } 
                // Skip leading space.
                long i = 0L;
                while (i < len(tag) && tag[i] == ' ')
                {
                    i++;
                }

                tag = tag[i..];
                if (tag == "")
                {
                    break;
                } 

                // Scan to colon. A space, a quote or a control character is a syntax error.
                // Strictly speaking, control chars include the range [0x7f, 0x9f], not just
                // [0x00, 0x1f], but in practice, we ignore the multi-byte control characters
                // as it is simpler to inspect the tag's bytes than the tag's runes.
                i = 0L;
                while (i < len(tag) && tag[i] > ' ' && tag[i] != ':' && tag[i] != '"' && tag[i] != 0x7fUL)
                {
                    i++;
                }

                if (i == 0L)
                {
                    return error.As(errTagKeySyntax);
                }
                if (i + 1L >= len(tag) || tag[i] != ':')
                {
                    return error.As(errTagSyntax);
                }
                if (tag[i + 1L] != '"')
                {
                    return error.As(errTagValueSyntax);
                }
                var key = tag[..i];
                tag = tag[i + 1L..]; 

                // Scan quoted string to find value.
                i = 1L;
                while (i < len(tag) && tag[i] != '"')
                {
                    if (tag[i] == '\\')
                    {
                        i++;
                    }
                    i++;
                }

                if (i >= len(tag))
                {
                    return error.As(errTagValueSyntax);
                }
                var qvalue = tag[..i + 1L];
                tag = tag[i + 1L..];

                var (value, err) = strconv.Unquote(qvalue);
                if (err != null)
                {
                    return error.As(errTagValueSyntax);
                }
                if (!checkTagSpaces[key])
                {
                    continue;
                }
                switch (key)
                {
                    case "xml": 
                        // If the first or last character in the XML tag is a space, it is
                        // suspicious.
                        if (strings.Trim(value, " ") != value)
                        {
                            return error.As(errTagValueSpace);
                        } 

                        // If there are multiple spaces, they are suspicious.
                        if (strings.Count(value, " ") > 1L)
                        {
                            return error.As(errTagValueSpace);
                        } 

                        // If there is no comma, skip the rest of the checks.
                        var comma = strings.IndexRune(value, ',');
                        if (comma < 0L)
                        {
                            continue;
                        } 

                        // If the character before a comma is a space, this is suspicious.
                        if (comma > 0L && value[comma - 1L] == ' ')
                        {
                            return error.As(errTagValueSpace);
                        }
                        value = value[comma + 1L..];
                        break;
                    case "json": 
                        // JSON allows using spaces in the name, so skip it.
                        comma = strings.IndexRune(value, ',');
                        if (comma < 0L)
                        {
                            continue;
                        }
                        value = value[comma + 1L..];
                        break;
                }

                if (strings.IndexByte(value, ' ') >= 0L)
                {
                    return error.As(errTagValueSpace);
                }
            }

            return error.As(null);
        }
    }
}
