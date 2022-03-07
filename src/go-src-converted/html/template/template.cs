// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 06 22:25:38 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Program Files\Go\src\html\template\template.go
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using sync = go.sync_package;
using template = go.text.template_package;
using parse = go.text.template.parse_package;
using System;


namespace go.html;

public static partial class template_package {

    // Template is a specialized Template from "text/template" that produces a safe
    // HTML document fragment.
public partial struct Template {
    public error escapeErr; // We could embed the text/template field, but it's safer not to because
// we need to keep our version of the name space and the underlying
// template's in sync.
    public ptr<template.Template> text; // The underlying template's parse tree, updated to be HTML-safe.
    public ptr<parse.Tree> Tree;
    public ref ptr<nameSpace> ptr<nameSpace> => ref ptr<nameSpace>_ptr; // common to all associated templates
}

// escapeOK is a sentinel value used to indicate valid escaping.
private static var escapeOK = fmt.Errorf("template escaped correctly");

// nameSpace is the data structure shared by all templates in an association.
private partial struct nameSpace {
    public sync.Mutex mu;
    public map<@string, ptr<Template>> set;
    public bool escaped;
    public escaper esc;
}

// Templates returns a slice of the templates associated with t, including t
// itself.
private static slice<ptr<Template>> Templates(this ptr<Template> _addr_t) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    var ns = t.nameSpace;
    ns.mu.Lock();
    defer(ns.mu.Unlock()); 
    // Return a slice so we don't expose the map.
    var m = make_slice<ptr<Template>>(0, len(ns.set));
    foreach (var (_, v) in ns.set) {
        m = append(m, v);
    }    return m;

});

// Option sets options for the template. Options are described by
// strings, either a simple string or "key=value". There can be at
// most one equals sign in an option string. If the option string
// is unrecognized or otherwise invalid, Option panics.
//
// Known options:
//
// missingkey: Control the behavior during execution if a map is
// indexed with a key that is not present in the map.
//    "missingkey=default" or "missingkey=invalid"
//        The default behavior: Do nothing and continue execution.
//        If printed, the result of the index operation is the string
//        "<no value>".
//    "missingkey=zero"
//        The operation returns the zero value for the map type's element.
//    "missingkey=error"
//        Execution stops immediately with an error.
//
private static ptr<Template> Option(this ptr<Template> _addr_t, params @string[] opt) {
    opt = opt.Clone();
    ref Template t = ref _addr_t.val;

    t.text.Option(opt);
    return _addr_t!;
}

// checkCanParse checks whether it is OK to parse templates.
// If not, it returns an error.
private static error checkCanParse(this ptr<Template> _addr_t) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    if (t == null) {
        return error.As(null!)!;
    }
    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    if (t.nameSpace.escaped) {
        return error.As(fmt.Errorf("html/template: cannot Parse after Execute"))!;
    }
    return error.As(null!)!;

});

// escape escapes all associated templates.
private static error escape(this ptr<Template> _addr_t) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    t.nameSpace.escaped = true;
    if (t.escapeErr == null) {
        if (t.Tree == null) {
            return error.As(fmt.Errorf("template: %q is an incomplete or empty template", t.Name()))!;
        }
        {
            var err = escapeTemplate(t, t.text.Root, t.Name());

            if (err != null) {
                return error.As(err)!;
            }

        }

    }
    else if (t.escapeErr != escapeOK) {
        return error.As(t.escapeErr)!;
    }
    return error.As(null!)!;

});

// Execute applies a parsed template to the specified data object,
// writing the output to wr.
// If an error occurs executing the template or writing its output,
// execution stops, but partial results may already have been written to
// the output writer.
// A template may be executed safely in parallel, although if parallel
// executions share a Writer the output may be interleaved.
private static error Execute(this ptr<Template> _addr_t, io.Writer wr, object data) {
    ref Template t = ref _addr_t.val;

    {
        var err = t.escape();

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(t.text.Execute(wr, data))!;

}

// ExecuteTemplate applies the template associated with t that has the given
// name to the specified data object and writes the output to wr.
// If an error occurs executing the template or writing its output,
// execution stops, but partial results may already have been written to
// the output writer.
// A template may be executed safely in parallel, although if parallel
// executions share a Writer the output may be interleaved.
private static error ExecuteTemplate(this ptr<Template> _addr_t, io.Writer wr, @string name, object data) {
    ref Template t = ref _addr_t.val;

    var (tmpl, err) = t.lookupAndEscapeTemplate(name);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(tmpl.text.Execute(wr, data))!;

}

// lookupAndEscapeTemplate guarantees that the template with the given name
// is escaped, or returns an error if it cannot be. It returns the named
// template.
private static (ptr<Template>, error) lookupAndEscapeTemplate(this ptr<Template> _addr_t, @string name) => func((defer, panic, _) => {
    ptr<Template> tmpl = default!;
    error err = default!;
    ref Template t = ref _addr_t.val;

    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    t.nameSpace.escaped = true;
    tmpl = t.set[name];
    if (tmpl == null) {
        return (_addr_null!, error.As(fmt.Errorf("html/template: %q is undefined", name))!);
    }
    if (tmpl.escapeErr != null && tmpl.escapeErr != escapeOK) {
        return (_addr_null!, error.As(tmpl.escapeErr)!);
    }
    if (tmpl.text.Tree == null || tmpl.text.Root == null) {
        return (_addr_null!, error.As(fmt.Errorf("html/template: %q is an incomplete template", name))!);
    }
    if (t.text.Lookup(name) == null) {
        panic("html/template internal error: template escaping out of sync");
    }
    if (tmpl.escapeErr == null) {
        err = escapeTemplate(tmpl, tmpl.text.Root, name);
    }
    return (_addr_tmpl!, error.As(err)!);

});

// DefinedTemplates returns a string listing the defined templates,
// prefixed by the string "; defined templates are: ". If there are none,
// it returns the empty string. Used to generate an error message.
private static @string DefinedTemplates(this ptr<Template> _addr_t) {
    ref Template t = ref _addr_t.val;

    return t.text.DefinedTemplates();
}

// Parse parses text as a template body for t.
// Named template definitions ({{define ...}} or {{block ...}} statements) in text
// define additional templates associated with t and are removed from the
// definition of t itself.
//
// Templates can be redefined in successive calls to Parse,
// before the first use of Execute on t or any associated template.
// A template definition with a body containing only white space and comments
// is considered empty and will not replace an existing template's body.
// This allows using Parse to add new named template definitions without
// overwriting the main template body.
private static (ptr<Template>, error) Parse(this ptr<Template> _addr_t, @string text) => func((defer, _, _) => {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    {
        var err = t.checkCanParse();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }


    var (ret, err) = t.text.Parse(text);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    foreach (var (_, v) in ret.Templates()) {
        var name = v.Name();
        var tmpl = t.set[name];
        if (tmpl == null) {
            tmpl = t.@new(name);
        }
        tmpl.text = v;
        tmpl.Tree = v.Tree;

    }    return (_addr_t!, error.As(null!)!);

});

// AddParseTree creates a new template with the name and parse tree
// and associates it with t.
//
// It returns an error if t or any associated template has already been executed.
private static (ptr<Template>, error) AddParseTree(this ptr<Template> _addr_t, @string name, ptr<parse.Tree> _addr_tree) => func((defer, _, _) => {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;
    ref parse.Tree tree = ref _addr_tree.val;

    {
        var err = t.checkCanParse();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }


    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    var (text, err) = t.text.AddParseTree(name, tree);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ptr<Template> ret = addr(new Template(nil,text,text.Tree,t.nameSpace,));
    t.set[name] = ret;
    return (_addr_ret!, error.As(null!)!);

});

// Clone returns a duplicate of the template, including all associated
// templates. The actual representation is not copied, but the name space of
// associated templates is, so further calls to Parse in the copy will add
// templates to the copy but not to the original. Clone can be used to prepare
// common templates and use them with variant definitions for other templates
// by adding the variants after the clone is made.
//
// It returns an error if t has already been executed.
private static (ptr<Template>, error) Clone(this ptr<Template> _addr_t) => func((defer, _, _) => {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    if (t.escapeErr != null) {
        return (_addr_null!, error.As(fmt.Errorf("html/template: cannot Clone %q after it has executed", t.Name()))!);
    }
    var (textClone, err) = t.text.Clone();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ptr<nameSpace> ns = addr(new nameSpace(set:make(map[string]*Template)));
    ns.esc = makeEscaper(ns);
    ptr<Template> ret = addr(new Template(nil,textClone,textClone.Tree,ns,));
    ret.set[ret.Name()] = ret;
    foreach (var (_, x) in textClone.Templates()) {
        var name = x.Name();
        var src = t.set[name];
        if (src == null || src.escapeErr != null) {
            return (_addr_null!, error.As(fmt.Errorf("html/template: cannot Clone %q after it has executed", t.Name()))!);
        }
        x.Tree = x.Tree.Copy();
        ret.set[name] = addr(new Template(nil,x,x.Tree,ret.nameSpace,));

    }    return (_addr_ret.set[ret.Name()]!, error.As(null!)!);

});

// New allocates a new HTML template with the given name.
public static ptr<Template> New(@string name) {
    ptr<nameSpace> ns = addr(new nameSpace(set:make(map[string]*Template)));
    ns.esc = makeEscaper(ns);
    ptr<Template> tmpl = addr(new Template(nil,template.New(name),nil,ns,));
    tmpl.set[name] = tmpl;
    return _addr_tmpl!;
}

// New allocates a new HTML template associated with the given one
// and with the same delimiters. The association, which is transitive,
// allows one template to invoke another with a {{template}} action.
//
// If a template with the given name already exists, the new HTML template
// will replace it. The existing template will be reset and disassociated with
// t.
private static ptr<Template> New(this ptr<Template> _addr_t, @string name) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    return _addr_t.@new(name)!;
});

// new is the implementation of New, without the lock.
private static ptr<Template> @new(this ptr<Template> _addr_t, @string name) {
    ref Template t = ref _addr_t.val;

    ptr<Template> tmpl = addr(new Template(nil,t.text.New(name),nil,t.nameSpace,));
    {
        var (existing, ok) = tmpl.set[name];

        if (ok) {
            var emptyTmpl = New(existing.Name());
            existing.val = emptyTmpl.val;
        }
    }

    tmpl.set[name] = tmpl;
    return _addr_tmpl!;

}

// Name returns the name of the template.
private static @string Name(this ptr<Template> _addr_t) {
    ref Template t = ref _addr_t.val;

    return t.text.Name();
}

// FuncMap is the type of the map defining the mapping from names to
// functions. Each function must have either a single return value, or two
// return values of which the second has type error. In that case, if the
// second (error) argument evaluates to non-nil during execution, execution
// terminates and Execute returns that error. FuncMap has the same base type
// as FuncMap in "text/template", copied here so clients need not import
// "text/template".
private static ptr<Template> Funcs(this ptr<Template> _addr_t, FuncMap funcMap) {
    ref Template t = ref _addr_t.val;

    t.text.Funcs(template.FuncMap(funcMap));
    return _addr_t!;
}

// Delims sets the action delimiters to the specified strings, to be used in
// subsequent calls to Parse, ParseFiles, or ParseGlob. Nested template
// definitions will inherit the settings. An empty delimiter stands for the
// corresponding default: {{ or }}.
// The return value is the template, so calls can be chained.
private static ptr<Template> Delims(this ptr<Template> _addr_t, @string left, @string right) {
    ref Template t = ref _addr_t.val;

    t.text.Delims(left, right);
    return _addr_t!;
}

// Lookup returns the template with the given name that is associated with t,
// or nil if there is no such template.
private static ptr<Template> Lookup(this ptr<Template> _addr_t, @string name) => func((defer, _, _) => {
    ref Template t = ref _addr_t.val;

    t.nameSpace.mu.Lock();
    defer(t.nameSpace.mu.Unlock());
    return _addr_t.set[name]!;
});

// Must is a helper that wraps a call to a function returning (*Template, error)
// and panics if the error is non-nil. It is intended for use in variable initializations
// such as
//    var t = template.Must(template.New("name").Parse("html"))
public static ptr<Template> Must(ptr<Template> _addr_t, error err) => func((_, panic, _) => {
    ref Template t = ref _addr_t.val;

    if (err != null) {
        panic(err);
    }
    return _addr_t!;

});

// ParseFiles creates a new Template and parses the template definitions from
// the named files. The returned template's name will have the (base) name and
// (parsed) contents of the first file. There must be at least one file.
// If an error occurs, parsing stops and the returned *Template is nil.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
// For instance, ParseFiles("a/foo", "b/foo") stores "b/foo" as the template
// named "foo", while "a/foo" is unavailable.
public static (ptr<Template>, error) ParseFiles(params @string[] filenames) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    filenames = filenames.Clone();

    return _addr_parseFiles(_addr_null, readFileOS, filenames)!;
}

// ParseFiles parses the named files and associates the resulting templates with
// t. If an error occurs, parsing stops and the returned template is nil;
// otherwise it is t. There must be at least one file.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
//
// ParseFiles returns an error if t or any associated template has already been executed.
private static (ptr<Template>, error) ParseFiles(this ptr<Template> _addr_t, params @string[] filenames) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    filenames = filenames.Clone();
    ref Template t = ref _addr_t.val;

    return _addr_parseFiles(_addr_t, readFileOS, filenames)!;
}

// parseFiles is the helper for the method and function. If the argument
// template is nil, it is created from the first file.
private static (ptr<Template>, error) parseFiles(ptr<Template> _addr_t, Func<@string, (@string, slice<byte>, error)> readFile, params @string[] filenames) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    filenames = filenames.Clone();
    ref Template t = ref _addr_t.val;

    {
        var err = t.checkCanParse();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }


    if (len(filenames) == 0) { 
        // Not really a problem, but be consistent.
        return (_addr_null!, error.As(fmt.Errorf("html/template: no files named in call to ParseFiles"))!);

    }
    foreach (var (_, filename) in filenames) {
        var (name, b, err) = readFile(filename);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        var s = string(b); 
        // First template becomes return value if not already defined,
        // and we use that one for subsequent New calls to associate
        // all the templates together. Also, if this file has the same name
        // as t, this file becomes the contents of t, so
        //  t, err := New(name).Funcs(xxx).ParseFiles(name)
        // works. Otherwise we create a new template associated with t.
        ptr<Template> tmpl;
        if (t == null) {
            t = New(name);
        }
        if (name == t.Name()) {
            tmpl = t;
        }
        else
 {
            tmpl = t.New(name);
        }
        _, err = tmpl.Parse(s);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }    return (_addr_t!, error.As(null!)!);

}

// ParseGlob creates a new Template and parses the template definitions from
// the files identified by the pattern. The files are matched according to the
// semantics of filepath.Match, and the pattern must match at least one file.
// The returned template will have the (base) name and (parsed) contents of the
// first file matched by the pattern. ParseGlob is equivalent to calling
// ParseFiles with the list of files matched by the pattern.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
public static (ptr<Template>, error) ParseGlob(@string pattern) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;

    return _addr_parseGlob(_addr_null, pattern)!;
}

// ParseGlob parses the template definitions in the files identified by the
// pattern and associates the resulting templates with t. The files are matched
// according to the semantics of filepath.Match, and the pattern must match at
// least one file. ParseGlob is equivalent to calling t.ParseFiles with the
// list of files matched by the pattern.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
//
// ParseGlob returns an error if t or any associated template has already been executed.
private static (ptr<Template>, error) ParseGlob(this ptr<Template> _addr_t, @string pattern) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    return _addr_parseGlob(_addr_t, pattern)!;
}

// parseGlob is the implementation of the function and method ParseGlob.
private static (ptr<Template>, error) parseGlob(ptr<Template> _addr_t, @string pattern) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    {
        var err = t.checkCanParse();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    var (filenames, err) = filepath.Glob(pattern);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (len(filenames) == 0) {
        return (_addr_null!, error.As(fmt.Errorf("html/template: pattern matches no files: %#q", pattern))!);
    }
    return _addr_parseFiles(_addr_t, readFileOS, filenames)!;

}

// IsTrue reports whether the value is 'true', in the sense of not the zero of its type,
// and whether the value has a meaningful truth value. This is the definition of
// truth used by if and other such actions.
public static (bool, bool) IsTrue(object val) {
    bool truth = default;
    bool ok = default;

    return template.IsTrue(val);
}

// ParseFS is like ParseFiles or ParseGlob but reads from the file system fs
// instead of the host operating system's file system.
// It accepts a list of glob patterns.
// (Note that most file names serve as glob patterns matching only themselves.)
public static (ptr<Template>, error) ParseFS(fs.FS fs, params @string[] patterns) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    patterns = patterns.Clone();

    return _addr_parseFS(_addr_null, fs, patterns)!;
}

// ParseFS is like ParseFiles or ParseGlob but reads from the file system fs
// instead of the host operating system's file system.
// It accepts a list of glob patterns.
// (Note that most file names serve as glob patterns matching only themselves.)
private static (ptr<Template>, error) ParseFS(this ptr<Template> _addr_t, fs.FS fs, params @string[] patterns) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    patterns = patterns.Clone();
    ref Template t = ref _addr_t.val;

    return _addr_parseFS(_addr_t, fs, patterns)!;
}

private static (ptr<Template>, error) parseFS(ptr<Template> _addr_t, fs.FS fsys, slice<@string> patterns) {
    ptr<Template> _p0 = default!;
    error _p0 = default!;
    ref Template t = ref _addr_t.val;

    slice<@string> filenames = default;
    foreach (var (_, pattern) in patterns) {
        var (list, err) = fs.Glob(fsys, pattern);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (len(list) == 0) {
            return (_addr_null!, error.As(fmt.Errorf("template: pattern matches no files: %#q", pattern))!);
        }
        filenames = append(filenames, list);

    }    return _addr_parseFiles(_addr_t, readFileFS(fsys), filenames)!;

}

private static (@string, slice<byte>, error) readFileOS(@string file) {
    @string name = default;
    slice<byte> b = default;
    error err = default!;

    name = filepath.Base(file);
    b, err = os.ReadFile(file);
    return ;
}

private static Func<@string, (@string, slice<byte>, error)> readFileFS(fs.FS fsys) {
    return file => {
        name = path.Base(file);
        b, err = fs.ReadFile(fsys, file);
        return ;
    };
}

} // end template_package
