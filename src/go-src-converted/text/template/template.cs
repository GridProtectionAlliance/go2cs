// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using reflect = reflect_package;
using sync = sync_package;
using parse = text.template.parse_package;
using text.template;

partial class template_package {

// common holds the information shared by related templates.
[GoType] partial struct common {
    internal map<@string, ж<Template>> tmpl; // Map from name to defined templates.
    internal sync_package.RWMutex muTmpl;         // protects tmpl
    internal option option;
    // We use two maps, one for parsing and one for execution.
    // This separation makes the API cleaner since it doesn't
    // expose reflection to the client.
    internal sync_package.RWMutex muFuncs; // protects parseFuncs and execFuncs
    internal FuncMap parseFuncs;
    internal map<@string, reflectꓸValue> execFuncs;
}

// Template is the representation of a parsed template. The *parse.Tree
// field is exported only for use by [html/template] and should be treated
// as unexported by all other clients.
[GoType] partial struct Template {
    internal @string name;
    public partial ref ж<text.template.parse_package.Tree> Tree { get; }
    public partial ref ж<common> common { get; }
    internal @string leftDelim;
    internal @string rightDelim;
}

// New allocates a new, undefined template with the given name.
public static ж<Template> New(@string name) {
    var t = Ꮡ(new Template(
        name: name
    ));
    t.init();
    return t;
}

// Name returns the name of the template.
[GoRecv] public static @string Name(this ref Template t) {
    return t.name;
}

// New allocates a new, undefined template associated with the given one and with the same
// delimiters. The association, which is transitive, allows one template to
// invoke another with a {{template}} action.
//
// Because associated templates share underlying data, template construction
// cannot be done safely in parallel. Once the templates are constructed, they
// can be executed in parallel.
[GoRecv] public static ж<Template> New(this ref Template t, @string name) {
    t.init();
    var nt = Ꮡ(new Template(
        name: name,
        common: t.common,
        leftDelim: t.leftDelim,
        rightDelim: t.rightDelim
    ));
    return nt;
}

// init guarantees that t has a valid common structure.
[GoRecv] internal static void init(this ref Template t) {
    if (t.common == nil) {
        var c = @new<common>();
        c.val.tmpl = new map<@string, ж<Template>>();
        c.val.parseFuncs = new FuncMap();
        c.val.execFuncs = new map<@string, reflectꓸValue>();
        t.common = c;
    }
}

// Clone returns a duplicate of the template, including all associated
// templates. The actual representation is not copied, but the name space of
// associated templates is, so further calls to [Template.Parse] in the copy will add
// templates to the copy but not to the original. Clone can be used to prepare
// common templates and use them with variant definitions for other templates
// by adding the variants after the clone is made.
[GoRecv] public static (ж<Template>, error) Clone(this ref Template t) => func((defer, _) => {
    var nt = t.copy(nil);
    nt.init();
    if (t.common == nil) {
        return (nt, default!);
    }
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock);
    foreach (var (k, v) in t.tmpl) {
        if (k == t.name) {
            nt.tmpl[t.name] = nt;
            continue;
        }
        // The associated templates share nt's common structure.
        var tmpl = v.copy((~nt).common);
        nt.tmpl[k] = tmpl;
    }
    t.muFuncs.RLock();
    defer(t.muFuncs.RUnlock);
    foreach (var (k, v) in t.parseFuncs) {
        nt.parseFuncs[k] = v;
    }
    foreach (var (k, v) in t.execFuncs) {
        nt.execFuncs[k] = v;
    }
    return (nt, default!);
});

// copy returns a shallow copy of t, with common set to the argument.
[GoRecv] public static ж<Template> copy(this ref Template t, ж<common> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return Ꮡ(new Template(
        name: t.name,
        Tree: t.Tree,
        common: c,
        leftDelim: t.leftDelim,
        rightDelim: t.rightDelim
    ));
}

// AddParseTree associates the argument parse tree with the template t, giving
// it the specified name. If the template has not been defined, this tree becomes
// its definition. If it has been defined and already has that name, the existing
// definition is replaced; otherwise a new template is created, defined, and returned.
[GoRecv] public static (ж<Template>, error) AddParseTree(this ref Template t, @string name, ж<parse.Tree> Ꮡtree) => func((defer, _) => {
    ref var tree = ref Ꮡtree.val;

    t.init();
    t.muTmpl.Lock();
    defer(t.muTmpl.Unlock);
    var nt = t;
    if (name != t.name) {
        nt = t.New(name);
    }
    // Even if nt == t, we need to install it in the common.tmpl map.
    if (t.associate(nt, Ꮡtree) || (~nt).Tree == nil) {
        nt.val.Tree = tree;
    }
    return (nt, default!);
});

// Templates returns a slice of defined templates associated with t.
[GoRecv] public static slice<ж<Template>> Templates(this ref Template t) => func((defer, _) => {
    if (t.common == nil) {
        return default!;
    }
    // Return a slice so we don't expose the map.
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock);
    var m = new slice<ж<Template>>(0, len(t.tmpl));
    foreach (var (_, v) in t.tmpl) {
        m = append(m, v);
    }
    return m;
});

// Delims sets the action delimiters to the specified strings, to be used in
// subsequent calls to [Template.Parse], [Template.ParseFiles], or [Template.ParseGlob]. Nested template
// definitions will inherit the settings. An empty delimiter stands for the
// corresponding default: {{ or }}.
// The return value is the template, so calls can be chained.
[GoRecv("capture")] public static ж<Template> Delims(this ref Template t, @string left, @string right) {
    t.init();
    t.leftDelim = left;
    t.rightDelim = right;
    return DelimsꓸᏑt;
}

// Funcs adds the elements of the argument map to the template's function map.
// It must be called before the template is parsed.
// It panics if a value in the map is not a function with appropriate return
// type or if the name cannot be used syntactically as a function in a template.
// It is legal to overwrite elements of the map. The return value is the template,
// so calls can be chained.
[GoRecv("capture")] public static ж<Template> Funcs(this ref Template t, FuncMap funcMap) => func((defer, _) => {
    t.init();
    t.muFuncs.Lock();
    defer(t.muFuncs.Unlock);
    addValueFuncs(t.execFuncs, funcMap);
    addFuncs(t.parseFuncs, funcMap);
    return FuncsꓸᏑt;
});

// Lookup returns the template with the given name that is associated with t.
// It returns nil if there is no such template or the template has no definition.
[GoRecv] public static ж<Template> Lookup(this ref Template t, @string name) => func((defer, _) => {
    if (t.common == nil) {
        return default!;
    }
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock);
    return t.tmpl[name];
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
[GoRecv("capture")] public static (ж<Template>, error) Parse(this ref Template t, @string text) {
    t.init();
    t.muFuncs.RLock();
    (trees, err) = parse.Parse(t.name, text, t.leftDelim, t.rightDelim, t.parseFuncs, builtins());
    t.muFuncs.RUnlock();
    if (err != default!) {
        return (default!, err);
    }
    // Add the newly parsed trees, including the one for t, into our common structure.
    foreach (var (name, tree) in trees) {
        {
            (_, errΔ1) = t.AddParseTree(name, tree); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    return (ParseꓸᏑt, default!);
}

// associate installs the new template into the group of templates associated
// with t. The two are already known to share the common structure.
// The boolean return value reports whether to store this tree as t.Tree.
[GoRecv] public static bool associate(this ref Template t, ж<Template> Ꮡnew, ж<parse.Tree> Ꮡtree) {
    ref var @new = ref Ꮡnew.val;
    ref var tree = ref Ꮡtree.val;

    if (@new.common != t.common) {
        throw panic("internal error: associate not common");
    }
    {
        var old = t.tmpl[@new.name]; if (old != nil && parse.IsEmptyTree(~tree.Root) && (~old).Tree != nil) {
            // If a template by that name exists,
            // don't replace it with an empty template.
            return false;
        }
    }
    t.tmpl[@new.name] = @new;
    return true;
}

} // end template_package
