// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Helper functions to make constructing templates easier.

// package template -- go2cs converted at 2020 October 08 03:42:15 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Go\src\text\template\helper.go
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace text
{
    public static partial class template_package
    {
        // Functions and methods to parse templates.

        // Must is a helper that wraps a call to a function returning (*Template, error)
        // and panics if the error is non-nil. It is intended for use in variable
        // initializations such as
        //    var t = template.Must(template.New("name").Parse("text"))
        public static ptr<Template> Must(ptr<Template> _addr_t, error err) => func((_, panic, __) =>
        {
            ref Template t = ref _addr_t.val;

            if (err != null)
            {
                panic(err);
            }
            return _addr_t!;

        });

        // ParseFiles creates a new Template and parses the template definitions from
        // the named files. The returned template's name will have the base name and
        // parsed contents of the first file. There must be at least one file.
        // If an error occurs, parsing stops and the returned *Template is nil.
        //
        // When parsing multiple files with the same name in different directories,
        // the last one mentioned will be the one that results.
        // For instance, ParseFiles("a/foo", "b/foo") stores "b/foo" as the template
        // named "foo", while "a/foo" is unavailable.
        public static (ptr<Template>, error) ParseFiles(params @string[] filenames)
        {
            ptr<Template> _p0 = default!;
            error _p0 = default!;
            filenames = filenames.Clone();

            return _addr_parseFiles(_addr_null, filenames)!;
        }

        // ParseFiles parses the named files and associates the resulting templates with
        // t. If an error occurs, parsing stops and the returned template is nil;
        // otherwise it is t. There must be at least one file.
        // Since the templates created by ParseFiles are named by the base
        // names of the argument files, t should usually have the name of one
        // of the (base) names of the files. If it does not, depending on t's
        // contents before calling ParseFiles, t.Execute may fail. In that
        // case use t.ExecuteTemplate to execute a valid template.
        //
        // When parsing multiple files with the same name in different directories,
        // the last one mentioned will be the one that results.
        private static (ptr<Template>, error) ParseFiles(this ptr<Template> _addr_t, params @string[] filenames)
        {
            ptr<Template> _p0 = default!;
            error _p0 = default!;
            filenames = filenames.Clone();
            ref Template t = ref _addr_t.val;

            t.init();
            return _addr_parseFiles(_addr_t, filenames)!;
        }

        // parseFiles is the helper for the method and function. If the argument
        // template is nil, it is created from the first file.
        private static (ptr<Template>, error) parseFiles(ptr<Template> _addr_t, params @string[] filenames)
        {
            ptr<Template> _p0 = default!;
            error _p0 = default!;
            filenames = filenames.Clone();
            ref Template t = ref _addr_t.val;

            if (len(filenames) == 0L)
            { 
                // Not really a problem, but be consistent.
                return (_addr_null!, error.As(fmt.Errorf("template: no files named in call to ParseFiles"))!);

            }

            foreach (var (_, filename) in filenames)
            {
                var (b, err) = ioutil.ReadFile(filename);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                var s = string(b);
                var name = filepath.Base(filename); 
                // First template becomes return value if not already defined,
                // and we use that one for subsequent New calls to associate
                // all the templates together. Also, if this file has the same name
                // as t, this file becomes the contents of t, so
                //  t, err := New(name).Funcs(xxx).ParseFiles(name)
                // works. Otherwise we create a new template associated with t.
                ptr<Template> tmpl;
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
                    return (_addr_null!, error.As(err)!);
                }

            }
            return (_addr_t!, error.As(null!)!);

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
        public static (ptr<Template>, error) ParseGlob(@string pattern)
        {
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
        private static (ptr<Template>, error) ParseGlob(this ptr<Template> _addr_t, @string pattern)
        {
            ptr<Template> _p0 = default!;
            error _p0 = default!;
            ref Template t = ref _addr_t.val;

            t.init();
            return _addr_parseGlob(_addr_t, pattern)!;
        }

        // parseGlob is the implementation of the function and method ParseGlob.
        private static (ptr<Template>, error) parseGlob(ptr<Template> _addr_t, @string pattern)
        {
            ptr<Template> _p0 = default!;
            error _p0 = default!;
            ref Template t = ref _addr_t.val;

            var (filenames, err) = filepath.Glob(pattern);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            if (len(filenames) == 0L)
            {
                return (_addr_null!, error.As(fmt.Errorf("template: pattern matches no files: %#q", pattern))!);
            }

            return _addr_parseFiles(_addr_t, filenames)!;

        }
    }
}}
