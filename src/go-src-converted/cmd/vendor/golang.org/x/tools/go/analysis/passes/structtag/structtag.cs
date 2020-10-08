// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package structtag defines an Analyzer that checks struct field tags
// are well formed.
// package structtag -- go2cs converted at 2020 October 08 04:58:11 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/structtag" ==> using structtag = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.structtag_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\structtag\structtag.go
using errors = go.errors_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class structtag_package
    {
        public static readonly @string Doc = (@string)"check that struct field tags conform to reflect.StructTag.Get\n\nAlso report certai" +
    "n struct tags (json, xml) used with unexported fields.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"structtag",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},RunDespiteErrors:true,Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.StructType)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<types.Struct> (styp, ok) = pass.TypesInfo.Types[n._<ptr<ast.StructType>>()].Type._<ptr<types.Struct>>(); 
                // Type information may be incomplete.
                if (!ok)
                {
                    return ;
                }

                ref namesSeen seen = ref heap(out ptr<namesSeen> _addr_seen);
                for (long i = 0L; i < styp.NumFields(); i++)
                {
                    var field = styp.Field(i);
                    var tag = styp.Tag(i);
                    checkCanonicalFieldTag(_addr_pass, _addr_field, tag, _addr_seen);
                }


            });
            return (null, error.As(null!)!);

        }

        // namesSeen keeps track of encoding tags by their key, name, and nested level
        // from the initial struct. The level is taken into account because equal
        // encoding key names only conflict when at the same level; otherwise, the lower
        // level shadows the higher level.
        private partial struct namesSeen // : map<uniqueName, token.Pos>
        {
        }

        private partial struct uniqueName
        {
            public @string key; // "xml" or "json"
            public @string name; // the encoding name
            public long level; // anonymous struct nesting level
        }

        private static (token.Pos, bool) Get(this ptr<namesSeen> _addr_s, @string key, @string name, long level)
        {
            token.Pos _p0 = default;
            bool _p0 = default;
            ref namesSeen s = ref _addr_s.val;

            if (s == null.val)
            {
                s.val = make_map<uniqueName, token.Pos>();
            }

            var (pos, ok) = (s.val)[new uniqueName(key,name,level)];
            return (pos, ok);

        }

        private static void Set(this ptr<namesSeen> _addr_s, @string key, @string name, long level, token.Pos pos)
        {
            ref namesSeen s = ref _addr_s.val;

            if (s == null.val)
            {
                s.val = make_map<uniqueName, token.Pos>();
            }

            (s.val)[new uniqueName(key,name,level)] = pos;

        }

        private static @string checkTagDups = new slice<@string>(new @string[] { "json", "xml" });
        private static map checkTagSpaces = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"json":true,"xml":true,"asn1":true};

        // checkCanonicalFieldTag checks a single struct field tag.
        private static void checkCanonicalFieldTag(ptr<analysis.Pass> _addr_pass, ptr<types.Var> _addr_field, @string tag, ptr<namesSeen> _addr_seen)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref types.Var field = ref _addr_field.val;
            ref namesSeen seen = ref _addr_seen.val;

            switch (pass.Pkg.Path())
            {
                case "encoding/json": 
                    // These packages know how to use their own APIs.
                    // Sometimes they are testing what happens to incorrect programs.

                case "encoding/xml": 
                    // These packages know how to use their own APIs.
                    // Sometimes they are testing what happens to incorrect programs.
                    return ;
                    break;
            }

            foreach (var (_, key) in checkTagDups)
            {
                checkTagDuplicates(_addr_pass, tag, key, _addr_field, _addr_field, _addr_seen, 1L);
            }
            {
                var err = validateStructTag(tag);

                if (err != null)
                {
                    pass.Reportf(field.Pos(), "struct field tag %#q not compatible with reflect.StructTag.Get: %s", tag, err);
                } 

                // Check for use of json or xml tags with unexported fields.

                // Embedded struct. Nothing to do for now, but that
                // may change, depending on what happens with issue 7363.
                // TODO(adonovan): investigate, now that that issue is fixed.

            } 

            // Check for use of json or xml tags with unexported fields.

            // Embedded struct. Nothing to do for now, but that
            // may change, depending on what happens with issue 7363.
            // TODO(adonovan): investigate, now that that issue is fixed.
            if (field.Anonymous())
            {
                return ;
            }

            if (field.Exported())
            {
                return ;
            }

            foreach (var (_, enc) in new array<@string>(new @string[] { "json", "xml" }))
            {
                if (reflect.StructTag(tag).Get(enc) != "")
                {
                    pass.Reportf(field.Pos(), "struct field %s has %s tag but is not exported", field.Name(), enc);
                    return ;
                }

            }

        }

        // checkTagDuplicates checks a single struct field tag to see if any tags are
        // duplicated. nearest is the field that's closest to the field being checked,
        // while still being part of the top-level struct type.
        private static void checkTagDuplicates(ptr<analysis.Pass> _addr_pass, @string tag, @string key, ptr<types.Var> _addr_nearest, ptr<types.Var> _addr_field, ptr<namesSeen> _addr_seen, long level)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref types.Var nearest = ref _addr_nearest.val;
            ref types.Var field = ref _addr_field.val;
            ref namesSeen seen = ref _addr_seen.val;

            var val = reflect.StructTag(tag).Get(key);
            if (val == "-")
            { 
                // Ignored, even if the field is anonymous.
                return ;

            }

            if (val == "" || val[0L] == ',')
            {
                if (!field.Anonymous())
                { 
                    // Ignored if the field isn't anonymous.
                    return ;

                }

                ptr<types.Struct> (typ, ok) = field.Type().Underlying()._<ptr<types.Struct>>();
                if (!ok)
                {
                    return ;
                }

                {
                    long i__prev1 = i;

                    for (long i = 0L; i < typ.NumFields(); i++)
                    {
                        var field = typ.Field(i);
                        if (!field.Exported())
                        {
                            continue;
                        }

                        var tag = typ.Tag(i);
                        checkTagDuplicates(_addr_pass, tag, key, _addr_nearest, _addr_field, _addr_seen, level + 1L);

                    }


                    i = i__prev1;
                }
                return ;

            }

            if (key == "xml" && field.Name() == "XMLName")
            { 
                // XMLName defines the XML element name of the struct being
                // checked. That name cannot collide with element or attribute
                // names defined on other fields of the struct. Vet does not have a
                // check for untagged fields of type struct defining their own name
                // by containing a field named XMLName; see issue 18256.
                return ;

            }

            {
                long i__prev1 = i;

                i = strings.Index(val, ",");

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

                i = i__prev1;

            }

            {
                var (pos, ok) = seen.Get(key, val, level);

                if (ok)
                {
                    var alsoPos = pass.Fset.Position(pos);
                    alsoPos.Column = 0L; 

                    // Make the "also at" position relative to the current position,
                    // to ensure that all warnings are unambiguous and correct. For
                    // example, via anonymous struct fields, it's possible for the
                    // two fields to be in different packages and directories.
                    var thisPos = pass.Fset.Position(field.Pos());
                    var (rel, err) = filepath.Rel(filepath.Dir(thisPos.Filename), alsoPos.Filename);
                    if (err != null)
                    { 
                        // Possibly because the paths are relative; leave the
                        // filename alone.
                    }
                    else
                    {
                        alsoPos.Filename = rel;
                    }

                    pass.Reportf(nearest.Pos(), "struct field %s repeats %s tag %q also at %s", field.Name(), key, val, alsoPos);

                }
                else
                {
                    seen.Set(key, val, level, field.Pos());
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
                    return error.As(errTagSpace)!;
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
                    return error.As(errTagKeySyntax)!;
                }

                if (i + 1L >= len(tag) || tag[i] != ':')
                {
                    return error.As(errTagSyntax)!;
                }

                if (tag[i + 1L] != '"')
                {
                    return error.As(errTagValueSyntax)!;
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
                    return error.As(errTagValueSyntax)!;
                }

                var qvalue = tag[..i + 1L];
                tag = tag[i + 1L..];

                var (value, err) = strconv.Unquote(qvalue);
                if (err != null)
                {
                    return error.As(errTagValueSyntax)!;
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
                            return error.As(errTagValueSpace)!;
                        } 

                        // If there are multiple spaces, they are suspicious.
                        if (strings.Count(value, " ") > 1L)
                        {
                            return error.As(errTagValueSpace)!;
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
                            return error.As(errTagValueSpace)!;
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
                    return error.As(errTagValueSpace)!;
                }

            }

            return error.As(null!)!;

        }
    }
}}}}}}}}}
