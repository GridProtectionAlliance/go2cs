// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 13 05:39:15 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Program Files\Go\src\text\template\template.go
namespace go.text;

using reflect = reflect_package;
using sync = sync_package;
using parse = text.template.parse_package;


// common holds the information shared by related templates.

public static partial class template_package {

private partial struct common {
    public map<@string, ptr<Template>> tmpl; // Map from name to defined templates.
    public sync.RWMutex muTmpl; // protects tmpl
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
public partial struct Template {
    public @string name;
    public ref ptr<parse.Tree> Tree> => ref Tree>_ptr;
    public ref ptr<common> ptr<common> => ref ptr<common>_ptr;
    public @string leftDelim;
    public @string rightDelim;
}

// New allocates a new, undefined template with the given name.
public static ptr<Template> New(@string name) {
    ptr<Template> t = addr(new Template(name:name,));
    t.init();
    return _addr_t!;
}

// Name returns the name of the template.
private static @string Name(this ptr<Template> _addr_t) {
    ref Template t = ref _addr_t.val;

    return t.name;
}

// New allocates a new, undefined template associated with the given one and with the same
// delimiters. The association, which is transitive, allows one template to
// invoke another with a {{template}} action.
//
// Because associated templates share underlying data, template construction
// cannot be done safely in parallel. Once the templates are constructed, they
// can be executed in parallel.
private static ptr<Template> New(this ptr<Template> _addr_t, @string name) {
    ref Template t = ref _addr_t.val;

    t.init();
    ptr<Template> nt = addr(new Template(name:name,common:t.common,leftDelim:t.leftDelim,rightDelim:t.rightDelim,));
    return _addr_nt!;
}

// init guarantees that t has a valid common structure.
private static void init(this ptr<Template> _addr_t) {
    ref Template t = ref _addr_t.val;

    if (t.common == null) {
        ptr<common> c = @new<common>();
        c.tmpl = make_map<@string, ptr<Template>>();
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
private static (ptr<Template>, error) Clone(this ptr<Template> _addr_t) => func((defer, _, _) => {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    var nt = t.copy(null);
    nt.init();
    if (t.common == null) {
        return (_addr_nt!, error.As(null!)!);
    }
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock());
    {
        var k__prev1 = k;
        var v__prev1 = v;

        foreach (var (__k, __v) in t.tmpl) {
            k = __k;
            v = __v;
            if (k == t.name) {
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

        foreach (var (__k, __v) in t.parseFuncs) {
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

        foreach (var (__k, __v) in t.execFuncs) {
            k = __k;
            v = __v;
            nt.execFuncs[k] = v;
        }
        k = k__prev1;
        v = v__prev1;
    }

    return (_addr_nt!, error.As(null!)!);
});

// copy returns a shallow copy of t, with common set to the argument.
private static ptr<Template> copy(this ptr<Template> _addr_t, ptr<common> _addr_c) {
    ref Template t = ref _addr_t.val;
    ref common c = ref _addr_c.val;

    return addr(new Template(name:t.name,Tree:t.Tree,common:c,leftDelim:t.leftDelim,rightDelim:t.rightDelim,));
}

// AddParseTree associates the argument parse tree with the template t, giving
// it the specified name. If the template has not been defined, this tree becomes
// its definition. If it has been defined and already has that name, the existing
// definition is replaced; otherwise a new template is created, defined, and returned.
private static (ptr<Template>, error) AddParseTree(this ptr<Template> _addr_t, @string name, ptr<parse.Tree> _addr_tree) => func((defer, _, _) => {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;
    ref parse.Tree tree = ref _addr_tree.val;

    t.init();
    t.muTmpl.Lock();
    defer(t.muTmpl.Unlock());
    var nt = t;
    if (name != t.name) {
        nt = t.New(name);
    }
    if (t.associate(nt, tree) || nt.Tree == null) {
        nt.Tree = tree;
    }
    return (_addr_nt!, error.As(null!)!);
});

// Templates returns a slice of defined templates associated with t.
private static slice<ptr<Template>> Templates(this ptr<Template> _addr_t) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    if (t.common == null) {
        return null;
    }
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock());
    var m = make_slice<ptr<Template>>(0, len(t.tmpl));
    foreach (var (_, v) in t.tmpl) {
        m = append(m, v);
    }    return m;
});

// Delims sets the action delimiters to the specified strings, to be used in
// subsequent calls to Parse, ParseFiles, or ParseGlob. Nested template
// definitions will inherit the settings. An empty delimiter stands for the
// corresponding default: {{ or }}.
// The return value is the template, so calls can be chained.
private static ptr<Template> Delims(this ptr<Template> _addr_t, @string left, @string right) {
    ref Template t = ref _addr_t.val;

    t.init();
    t.leftDelim = left;
    t.rightDelim = right;
    return _addr_t!;
}

// Funcs adds the elements of the argument map to the template's function map.
// It must be called before the template is parsed.
// It panics if a value in the map is not a function with appropriate return
// type or if the name cannot be used syntactically as a function in a template.
// It is legal to overwrite elements of the map. The return value is the template,
// so calls can be chained.
private static ptr<Template> Funcs(this ptr<Template> _addr_t, FuncMap funcMap) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    t.init();
    t.muFuncs.Lock();
    defer(t.muFuncs.Unlock());
    addValueFuncs(t.execFuncs, funcMap);
    addFuncs(t.parseFuncs, funcMap);
    return _addr_t!;
});

// Lookup returns the template with the given name that is associated with t.
// It returns nil if there is no such template or the template has no definition.
private static ptr<Template> Lookup(this ptr<Template> _addr_t, @string name) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    if (t.common == null) {
        return _addr_null!;
    }
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock());
    return _addr_t.tmpl[name]!;
});

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
private static (ptr<Template>, error) Parse(this ptr<Template> _addr_t, @string text) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    t.init();
    t.muFuncs.RLock();
    var (trees, err) = parse.Parse(t.name, text, t.leftDelim, t.rightDelim, t.parseFuncs, builtins());
    t.muFuncs.RUnlock();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    foreach (var (name, tree) in trees) {
        {
            var (_, err) = t.AddParseTree(name, tree);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

        }
    }    return (_addr_t!, error.As(null!)!);
}

// associate installs the new template into the group of templates associated
// with t. The two are already known to share the common structure.
// The boolean return value reports whether to store this tree as t.Tree.
private static bool associate(this ptr<Template> _addr_t, ptr<Template> _addr_@new, ptr<parse.Tree> _addr_tree) => func((_, panic, _) => {
    ref Template t = ref _addr_t.val;
    ref Template @new = ref _addr_@new.val;
    ref parse.Tree tree = ref _addr_tree.val;

    if (@new.common != t.common) {
        panic("internal error: associate not common");
    }
    {
        var old = t.tmpl[@new.name];

        if (old != null && parse.IsEmptyTree(tree.Root) && old.Tree != null) { 
            // If a template by that name exists,
            // don't replace it with an empty template.
            return false;
        }
    }
    t.tmpl[@new.name] = new;
    return true;
});

} // end template_package
