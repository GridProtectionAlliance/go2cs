// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 August 29 08:35:01 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Go\src\text\template\template.go
using reflect = go.reflect_package;
using sync = go.sync_package;
using parse = go.text.template.parse_package;
using static go.builtin;

namespace go {
namespace text
{
    public static partial class template_package
    {
        // common holds the information shared by related templates.
        private partial struct common
        {
            public map<@string, ref Template> tmpl; // Map from name to defined templates.
            public option option; // We use two maps, one for parsing and one for execution.
// This separation makes the API cleaner since it doesn't
// expose reflection to the client.
            public sync.RWMutex muFuncs; // protects parseFuncs and execFuncs
            public FuncMap parseFuncs;
            public map<@string, reflect.Value> execFuncs;
        }

        // Template is the representation of a parsed template. The *parse.Tree
        // field is exported only for use by html/template and should be treated
        // as unexported by all other clients.
        public partial struct Template
        {
            public @string name;
            public ref parse.Tree Tree => ref Tree_ptr;
            public ref common common => ref common_ptr;
            public @string leftDelim;
            public @string rightDelim;
        }

        // New allocates a new, undefined template with the given name.
        public static ref Template New(@string name)
        {
            Template t = ref new Template(name:name,);
            t.init();
            return t;
        }

        // Name returns the name of the template.
        private static @string Name(this ref Template t)
        {
            return t.name;
        }

        // New allocates a new, undefined template associated with the given one and with the same
        // delimiters. The association, which is transitive, allows one template to
        // invoke another with a {{template}} action.
        private static ref Template New(this ref Template t, @string name)
        {
            t.init();
            Template nt = ref new Template(name:name,common:t.common,leftDelim:t.leftDelim,rightDelim:t.rightDelim,);
            return nt;
        }

        // init guarantees that t has a valid common structure.
        private static void init(this ref Template t)
        {
            if (t.common == null)
            {
                ptr<common> c = @new<common>();
                c.tmpl = make_map<@string, ref Template>();
                c.parseFuncs = make(FuncMap);
                c.execFuncs = make_map<@string, reflect.Value>();
                t.common = c;
            }
        }

        // Clone returns a duplicate of the template, including all associated
        // templates. The actual representation is not copied, but the name space of
        // associated templates is, so further calls to Parse in the copy will add
        // templates to the copy but not to the original. Clone can be used to prepare
        // common templates and use them with variant definitions for other templates
        // by adding the variants after the clone is made.
        private static (ref Template, error) Clone(this ref Template _t) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            var nt = t.copy(null);
            nt.init();
            if (t.common == null)
            {
                return (nt, null);
            }
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in t.tmpl)
                {
                    k = __k;
                    v = __v;
                    if (k == t.name)
                    {
                        nt.tmpl[t.name] = nt;
                        continue;
                    } 
                    // The associated templates share nt's common structure.
                    var tmpl = v.copy(nt.common);
                    nt.tmpl[k] = tmpl;
                }

                k = k__prev1;
                v = v__prev1;
            }

            t.muFuncs.RLock();
            defer(t.muFuncs.RUnlock());
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in t.parseFuncs)
                {
                    k = __k;
                    v = __v;
                    nt.parseFuncs[k] = v;
                }

                k = k__prev1;
                v = v__prev1;
            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in t.execFuncs)
                {
                    k = __k;
                    v = __v;
                    nt.execFuncs[k] = v;
                }

                k = k__prev1;
                v = v__prev1;
            }

            return (nt, null);
        });

        // copy returns a shallow copy of t, with common set to the argument.
        private static ref Template copy(this ref Template t, ref common c)
        {
            var nt = New(t.name);
            nt.Tree = t.Tree;
            nt.common = c;
            nt.leftDelim = t.leftDelim;
            nt.rightDelim = t.rightDelim;
            return nt;
        }

        // AddParseTree adds parse tree for template with given name and associates it with t.
        // If the template does not already exist, it will create a new one.
        // If the template does exist, it will be replaced.
        private static (ref Template, error) AddParseTree(this ref Template t, @string name, ref parse.Tree tree)
        {
            t.init(); 
            // If the name is the name of this template, overwrite this template.
            var nt = t;
            if (name != t.name)
            {
                nt = t.New(name);
            } 
            // Even if nt == t, we need to install it in the common.tmpl map.
            {
                var (replace, err) = t.associate(nt, tree);

                if (err != null)
                {
                    return (null, err);
                }
                else if (replace || nt.Tree == null)
                {
                    nt.Tree = tree;
                }

            }
            return (nt, null);
        }

        // Templates returns a slice of defined templates associated with t.
        private static slice<ref Template> Templates(this ref Template t)
        {
            if (t.common == null)
            {
                return null;
            } 
            // Return a slice so we don't expose the map.
            var m = make_slice<ref Template>(0L, len(t.tmpl));
            foreach (var (_, v) in t.tmpl)
            {
                m = append(m, v);
            }
            return m;
        }

        // Delims sets the action delimiters to the specified strings, to be used in
        // subsequent calls to Parse, ParseFiles, or ParseGlob. Nested template
        // definitions will inherit the settings. An empty delimiter stands for the
        // corresponding default: {{ or }}.
        // The return value is the template, so calls can be chained.
        private static ref Template Delims(this ref Template t, @string left, @string right)
        {
            t.init();
            t.leftDelim = left;
            t.rightDelim = right;
            return t;
        }

        // Funcs adds the elements of the argument map to the template's function map.
        // It must be called before the template is parsed.
        // It panics if a value in the map is not a function with appropriate return
        // type or if the name cannot be used syntactically as a function in a template.
        // It is legal to overwrite elements of the map. The return value is the template,
        // so calls can be chained.
        private static ref Template Funcs(this ref Template _t, FuncMap funcMap) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            t.init();
            t.muFuncs.Lock();
            defer(t.muFuncs.Unlock());
            addValueFuncs(t.execFuncs, funcMap);
            addFuncs(t.parseFuncs, funcMap);
            return t;
        });

        // Lookup returns the template with the given name that is associated with t.
        // It returns nil if there is no such template or the template has no definition.
        private static ref Template Lookup(this ref Template t, @string name)
        {
            if (t.common == null)
            {
                return null;
            }
            return t.tmpl[name];
        }

        // Parse parses text as a template body for t.
        // Named template definitions ({{define ...}} or {{block ...}} statements) in text
        // define additional templates associated with t and are removed from the
        // definition of t itself.
        //
        // Templates can be redefined in successive calls to Parse.
        // A template definition with a body containing only white space and comments
        // is considered empty and will not replace an existing template's body.
        // This allows using Parse to add new named template definitions without
        // overwriting the main template body.
        private static (ref Template, error) Parse(this ref Template t, @string text)
        {
            t.init();
            t.muFuncs.RLock();
            var (trees, err) = parse.Parse(t.name, text, t.leftDelim, t.rightDelim, t.parseFuncs, builtins);
            t.muFuncs.RUnlock();
            if (err != null)
            {
                return (null, err);
            } 
            // Add the newly parsed trees, including the one for t, into our common structure.
            foreach (var (name, tree) in trees)
            {
                {
                    var (_, err) = t.AddParseTree(name, tree);

                    if (err != null)
                    {
                        return (null, err);
                    }

                }
            }
            return (t, null);
        }

        // associate installs the new template into the group of templates associated
        // with t. The two are already known to share the common structure.
        // The boolean return value reports whether to store this tree as t.Tree.
        private static (bool, error) associate(this ref Template _t, ref Template _@new, ref parse.Tree _tree) => func(_t, _@new, _tree, (ref Template t, ref Template @new, ref parse.Tree tree, Defer _, Panic panic, Recover __) =>
        {
            if (@new.common != t.common)
            {
                panic("internal error: associate not common");
            }
            {
                var old = t.tmpl[@new.name];

                if (old != null && parse.IsEmptyTree(tree.Root) && old.Tree != null)
                { 
                    // If a template by that name exists,
                    // don't replace it with an empty template.
                    return (false, null);
                }

            }
            t.tmpl[@new.name] = new;
            return (true, null);
        });
    }
}}
