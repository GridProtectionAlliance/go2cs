// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 August 29 08:36:14 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Go\src\html\template\template.go
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using filepath = go.path.filepath_package;
using sync = go.sync_package;
using template = go.text.template_package;
using parse = go.text.template.parse_package;
using static go.builtin;

namespace go {
namespace html
{
    public static partial class template_package
    {
        // Template is a specialized Template from "text/template" that produces a safe
        // HTML document fragment.
        public partial struct Template
        {
            public error escapeErr; // We could embed the text/template field, but it's safer not to because
// we need to keep our version of the name space and the underlying
// template's in sync.
            public ptr<template.Template> text; // The underlying template's parse tree, updated to be HTML-safe.
            public ptr<parse.Tree> Tree;
            public ref nameSpace nameSpace => ref nameSpace_ptr; // common to all associated templates
        }

        // escapeOK is a sentinel value used to indicate valid escaping.
        private static var escapeOK = fmt.Errorf("template escaped correctly");

        // nameSpace is the data structure shared by all templates in an association.
        private partial struct nameSpace
        {
            public sync.Mutex mu;
            public map<@string, ref Template> set;
            public bool escaped;
            public escaper esc;
        }

        // Templates returns a slice of the templates associated with t, including t
        // itself.
        private static slice<ref Template> Templates(this ref Template _t) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            var ns = t.nameSpace;
            ns.mu.Lock();
            defer(ns.mu.Unlock()); 
            // Return a slice so we don't expose the map.
            var m = make_slice<ref Template>(0L, len(ns.set));
            foreach (var (_, v) in ns.set)
            {
                m = append(m, v);
            }
            return m;
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
        private static ref Template Option(this ref Template t, params @string[] opt)
        {
            t.text.Option(opt);
            return t;
        }

        // checkCanParse checks whether it is OK to parse templates.
        // If not, it returns an error.
        private static error checkCanParse(this ref Template _t) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            if (t == null)
            {
                return error.As(null);
            }
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            if (t.nameSpace.escaped)
            {
                return error.As(fmt.Errorf("html/template: cannot Parse after Execute"));
            }
            return error.As(null);
        });

        // escape escapes all associated templates.
        private static error escape(this ref Template _t) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            t.nameSpace.escaped = true;
            if (t.escapeErr == null)
            {
                if (t.Tree == null)
                {
                    return error.As(fmt.Errorf("template: %q is an incomplete or empty template", t.Name()));
                }
                {
                    var err = escapeTemplate(t, t.text.Root, t.Name());

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            else if (t.escapeErr != escapeOK)
            {
                return error.As(t.escapeErr);
            }
            return error.As(null);
        });

        // Execute applies a parsed template to the specified data object,
        // writing the output to wr.
        // If an error occurs executing the template or writing its output,
        // execution stops, but partial results may already have been written to
        // the output writer.
        // A template may be executed safely in parallel, although if parallel
        // executions share a Writer the output may be interleaved.
        private static error Execute(this ref Template t, io.Writer wr, object data)
        {
            {
                var err = t.escape();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(t.text.Execute(wr, data));
        }

        // ExecuteTemplate applies the template associated with t that has the given
        // name to the specified data object and writes the output to wr.
        // If an error occurs executing the template or writing its output,
        // execution stops, but partial results may already have been written to
        // the output writer.
        // A template may be executed safely in parallel, although if parallel
        // executions share a Writer the output may be interleaved.
        private static error ExecuteTemplate(this ref Template t, io.Writer wr, @string name, object data)
        {
            var (tmpl, err) = t.lookupAndEscapeTemplate(name);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(tmpl.text.Execute(wr, data));
        }

        // lookupAndEscapeTemplate guarantees that the template with the given name
        // is escaped, or returns an error if it cannot be. It returns the named
        // template.
        private static (ref Template, error) lookupAndEscapeTemplate(this ref Template _t, @string name) => func(_t, (ref Template t, Defer defer, Panic panic, Recover _) =>
        {
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            t.nameSpace.escaped = true;
            tmpl = t.set[name];
            if (tmpl == null)
            {
                return (null, fmt.Errorf("html/template: %q is undefined", name));
            }
            if (tmpl.escapeErr != null && tmpl.escapeErr != escapeOK)
            {
                return (null, tmpl.escapeErr);
            }
            if (tmpl.text.Tree == null || tmpl.text.Root == null)
            {
                return (null, fmt.Errorf("html/template: %q is an incomplete template", name));
            }
            if (t.text.Lookup(name) == null)
            {
                panic("html/template internal error: template escaping out of sync");
            }
            if (tmpl.escapeErr == null)
            {
                err = escapeTemplate(tmpl, tmpl.text.Root, name);
            }
            return (tmpl, err);
        });

        // DefinedTemplates returns a string listing the defined templates,
        // prefixed by the string "; defined templates are: ". If there are none,
        // it returns the empty string. Used to generate an error message.
        private static @string DefinedTemplates(this ref Template t)
        {
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
        private static (ref Template, error) Parse(this ref Template _t, @string text) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = t.checkCanParse();

                if (err != null)
                {
                    return (null, err);
                }

            }

            var (ret, err) = t.text.Parse(text);
            if (err != null)
            {
                return (null, err);
            } 

            // In general, all the named templates might have changed underfoot.
            // Regardless, some new ones may have been defined.
            // The template.Template set has been updated; update ours.
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            foreach (var (_, v) in ret.Templates())
            {
                var name = v.Name();
                var tmpl = t.set[name];
                if (tmpl == null)
                {
                    tmpl = t.@new(name);
                }
                tmpl.text = v;
                tmpl.Tree = v.Tree;
            }
            return (t, null);
        });

        // AddParseTree creates a new template with the name and parse tree
        // and associates it with t.
        //
        // It returns an error if t or any associated template has already been executed.
        private static (ref Template, error) AddParseTree(this ref Template _t, @string name, ref parse.Tree _tree) => func(_t, _tree, (ref Template t, ref parse.Tree tree, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = t.checkCanParse();

                if (err != null)
                {
                    return (null, err);
                }

            }

            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            var (text, err) = t.text.AddParseTree(name, tree);
            if (err != null)
            {
                return (null, err);
            }
            Template ret = ref new Template(nil,text,text.Tree,t.nameSpace,);
            t.set[name] = ret;
            return (ret, null);
        });

        // Clone returns a duplicate of the template, including all associated
        // templates. The actual representation is not copied, but the name space of
        // associated templates is, so further calls to Parse in the copy will add
        // templates to the copy but not to the original. Clone can be used to prepare
        // common templates and use them with variant definitions for other templates
        // by adding the variants after the clone is made.
        //
        // It returns an error if t has already been executed.
        private static (ref Template, error) Clone(this ref Template _t) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            if (t.escapeErr != null)
            {
                return (null, fmt.Errorf("html/template: cannot Clone %q after it has executed", t.Name()));
            }
            var (textClone, err) = t.text.Clone();
            if (err != null)
            {
                return (null, err);
            }
            nameSpace ns = ref new nameSpace(set:make(map[string]*Template));
            ns.esc = makeEscaper(ns);
            Template ret = ref new Template(nil,textClone,textClone.Tree,ns,);
            ret.set[ret.Name()] = ret;
            foreach (var (_, x) in textClone.Templates())
            {
                var name = x.Name();
                var src = t.set[name];
                if (src == null || src.escapeErr != null)
                {
                    return (null, fmt.Errorf("html/template: cannot Clone %q after it has executed", t.Name()));
                }
                x.Tree = x.Tree.Copy();
                ret.set[name] = ref new Template(nil,x,x.Tree,ret.nameSpace,);
            } 
            // Return the template associated with the name of this template.
            return (ret.set[ret.Name()], null);
        });

        // New allocates a new HTML template with the given name.
        public static ref Template New(@string name)
        {
            nameSpace ns = ref new nameSpace(set:make(map[string]*Template));
            ns.esc = makeEscaper(ns);
            Template tmpl = ref new Template(nil,template.New(name),nil,ns,);
            tmpl.set[name] = tmpl;
            return tmpl;
        }

        // New allocates a new HTML template associated with the given one
        // and with the same delimiters. The association, which is transitive,
        // allows one template to invoke another with a {{template}} action.
        //
        // If a template with the given name already exists, the new HTML template
        // will replace it. The existing template will be reset and disassociated with
        // t.
        private static ref Template New(this ref Template _t, @string name) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            return t.@new(name);
        });

        // new is the implementation of New, without the lock.
        private static ref Template @new(this ref Template t, @string name)
        {
            Template tmpl = ref new Template(nil,t.text.New(name),nil,t.nameSpace,);
            {
                var (existing, ok) = tmpl.set[name];

                if (ok)
                {
                    var emptyTmpl = New(existing.Name());
                    existing.Value = emptyTmpl.Value;
                }

            }
            tmpl.set[name] = tmpl;
            return tmpl;
        }

        // Name returns the name of the template.
        private static @string Name(this ref Template t)
        {
            return t.text.Name();
        }

        // FuncMap is the type of the map defining the mapping from names to
        // functions. Each function must have either a single return value, or two
        // return values of which the second has type error. In that case, if the
        // second (error) argument evaluates to non-nil during execution, execution
        // terminates and Execute returns that error. FuncMap has the same base type
        // as FuncMap in "text/template", copied here so clients need not import
        // "text/template".
        private static ref Template Funcs(this ref Template t, FuncMap funcMap)
        {
            t.text.Funcs(template.FuncMap(funcMap));
            return t;
        }

        // Delims sets the action delimiters to the specified strings, to be used in
        // subsequent calls to Parse, ParseFiles, or ParseGlob. Nested template
        // definitions will inherit the settings. An empty delimiter stands for the
        // corresponding default: {{ or }}.
        // The return value is the template, so calls can be chained.
        private static ref Template Delims(this ref Template t, @string left, @string right)
        {
            t.text.Delims(left, right);
            return t;
        }

        // Lookup returns the template with the given name that is associated with t,
        // or nil if there is no such template.
        private static ref Template Lookup(this ref Template _t, @string name) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            t.nameSpace.mu.Lock();
            defer(t.nameSpace.mu.Unlock());
            return t.set[name];
        });

        // Must is a helper that wraps a call to a function returning (*Template, error)
        // and panics if the error is non-nil. It is intended for use in variable initializations
        // such as
        //    var t = template.Must(template.New("name").Parse("html"))
        public static ref Template Must(ref Template _t, error err) => func(_t, (ref Template t, Defer _, Panic panic, Recover __) =>
        {
            if (err != null)
            {
                panic(err);
            }
            return t;
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
        public static (ref Template, error) ParseFiles(params @string[] filenames)
        {
            filenames = filenames.Clone();

            return parseFiles(null, filenames);
        }

        // ParseFiles parses the named files and associates the resulting templates with
        // t. If an error occurs, parsing stops and the returned template is nil;
        // otherwise it is t. There must be at least one file.
        //
        // When parsing multiple files with the same name in different directories,
        // the last one mentioned will be the one that results.
        //
        // ParseFiles returns an error if t or any associated template has already been executed.
        private static (ref Template, error) ParseFiles(this ref Template t, params @string[] filenames)
        {
            return parseFiles(t, filenames);
        }

        // parseFiles is the helper for the method and function. If the argument
        // template is nil, it is created from the first file.
        private static (ref Template, error) parseFiles(ref Template t, params @string[] filenames)
        {
            filenames = filenames.Clone();

            {
                var err = t.checkCanParse();

                if (err != null)
                {
                    return (null, err);
                }

            }

            if (len(filenames) == 0L)
            { 
                // Not really a problem, but be consistent.
                return (null, fmt.Errorf("html/template: no files named in call to ParseFiles"));
            }
            foreach (var (_, filename) in filenames)
            {
                var (b, err) = ioutil.ReadFile(filename);
                if (err != null)
                {
                    return (null, err);
                }
                var s = string(b);
                var name = filepath.Base(filename); 
                // First template becomes return value if not already defined,
                // and we use that one for subsequent New calls to associate
                // all the templates together. Also, if this file has the same name
                // as t, this file becomes the contents of t, so
                //  t, err := New(name).Funcs(xxx).ParseFiles(name)
                // works. Otherwise we create a new template associated with t.
                ref Template tmpl = default;
                if (t == null)
                {
                    t = New(name);
                }
                if (name == t.Name())
                {
                    tmpl = t;
                }
                else
                {
                    tmpl = t.New(name);
                }
                _, err = tmpl.Parse(s);
                if (err != null)
                {
                    return (null, err);
                }
            }
            return (t, null);
        }

        // ParseGlob creates a new Template and parses the template definitions from the
        // files identified by the pattern, which must match at least one file. The
        // returned template will have the (base) name and (parsed) contents of the
        // first file matched by the pattern. ParseGlob is equivalent to calling
        // ParseFiles with the list of files matched by the pattern.
        //
        // When parsing multiple files with the same name in different directories,
        // the last one mentioned will be the one that results.
        public static (ref Template, error) ParseGlob(@string pattern)
        {
            return parseGlob(null, pattern);
        }

        // ParseGlob parses the template definitions in the files identified by the
        // pattern and associates the resulting templates with t. The pattern is
        // processed by filepath.Glob and must match at least one file. ParseGlob is
        // equivalent to calling t.ParseFiles with the list of files matched by the
        // pattern.
        //
        // When parsing multiple files with the same name in different directories,
        // the last one mentioned will be the one that results.
        //
        // ParseGlob returns an error if t or any associated template has already been executed.
        private static (ref Template, error) ParseGlob(this ref Template t, @string pattern)
        {
            return parseGlob(t, pattern);
        }

        // parseGlob is the implementation of the function and method ParseGlob.
        private static (ref Template, error) parseGlob(ref Template t, @string pattern)
        {
            {
                var err = t.checkCanParse();

                if (err != null)
                {
                    return (null, err);
                }

            }
            var (filenames, err) = filepath.Glob(pattern);
            if (err != null)
            {
                return (null, err);
            }
            if (len(filenames) == 0L)
            {
                return (null, fmt.Errorf("html/template: pattern matches no files: %#q", pattern));
            }
            return parseFiles(t, filenames);
        }

        // IsTrue reports whether the value is 'true', in the sense of not the zero of its type,
        // and whether the value has a meaningful truth value. This is the definition of
        // truth used by if and other such actions.
        public static (bool, bool) IsTrue(object val)
        {
            return template.IsTrue(val);
        }
    }
}}
