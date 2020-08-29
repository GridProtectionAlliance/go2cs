// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:59:28 UTC
// Original source: C:\Go\src\cmd\cover\html.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using browser = go.cmd.@internal.browser_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using math = go.math_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // htmlOutput reads the profile data from profile and generates an HTML
        // coverage report, writing it to outfile. If outfile is empty,
        // it writes the report to a temporary file and opens it in a web browser.
        private static error htmlOutput(@string profile, @string outfile)
        {
            var (profiles, err) = ParseProfiles(profile);
            if (err != null)
            {
                return error.As(err);
            }
            templateData d = default;

            foreach (var (_, profile) in profiles)
            {
                var fn = profile.FileName;
                if (profile.Mode == "set")
                {
                    d.Set = true;
                }
                var (file, err) = findFile(fn);
                if (err != null)
                {
                    return error.As(err);
                }
                var (src, err) = ioutil.ReadFile(file);
                if (err != null)
                {
                    return error.As(fmt.Errorf("can't read %q: %v", fn, err));
                }
                bytes.Buffer buf = default;
                err = htmlGen(ref buf, src, profile.Boundaries(src));
                if (err != null)
                {
                    return error.As(err);
                }
                d.Files = append(d.Files, ref new templateFile(Name:fn,Body:template.HTML(buf.String()),Coverage:percentCovered(profile),));
            }            ref os.File @out = default;
            if (outfile == "")
            {
                @string dir = default;
                dir, err = ioutil.TempDir("", "cover");
                if (err != null)
                {
                    return error.As(err);
                }
                out, err = os.Create(filepath.Join(dir, "coverage.html"));
            }
            else
            {
                out, err = os.Create(outfile);
            }
            if (err != null)
            {
                return error.As(err);
            }
            err = htmlTemplate.Execute(out, d);
            {
                var err2 = @out.Close();

                if (err == null)
                {
                    err = err2;
                }
            }
            if (err != null)
            {
                return error.As(err);
            }
            if (outfile == "")
            {
                if (!browser.Open("file://" + @out.Name()))
                {
                    fmt.Fprintf(os.Stderr, "HTML output written to %s\n", @out.Name());
                }
            }
            return error.As(null);
        }

        // percentCovered returns, as a percentage, the fraction of the statements in
        // the profile covered by the test run.
        // In effect, it reports the coverage of a given source file.
        private static double percentCovered(ref Profile p)
        {
            long total = default;            long covered = default;

            foreach (var (_, b) in p.Blocks)
            {
                total += int64(b.NumStmt);
                if (b.Count > 0L)
                {
                    covered += int64(b.NumStmt);
                }
            }
            if (total == 0L)
            {
                return 0L;
            }
            return float64(covered) / float64(total) * 100L;
        }

        // htmlGen generates an HTML coverage report with the provided filename,
        // source code, and tokens, and writes it to the given Writer.
        private static error htmlGen(io.Writer w, slice<byte> src, slice<Boundary> boundaries)
        {
            var dst = bufio.NewWriter(w);
            foreach (var (i) in src)
            {
                while (len(boundaries) > 0L && boundaries[0L].Offset == i)
                {
                    var b = boundaries[0L];
                    if (b.Start)
                    {
                        long n = 0L;
                        if (b.Count > 0L)
                        {
                            n = int(math.Floor(b.Norm * 9L)) + 1L;
                        }
                        fmt.Fprintf(dst, "<span class=\"cov%v\" title=\"%v\">", n, b.Count);
                    }
                    else
                    {
                        dst.WriteString("</span>");
                    }
                    boundaries = boundaries[1L..];
                }

                {
                    var b__prev1 = b;

                    b = src[i];

                    switch (b)
                    {
                        case '>': 
                            dst.WriteString("&gt;");
                            break;
                        case '<': 
                            dst.WriteString("&lt;");
                            break;
                        case '&': 
                            dst.WriteString("&amp;");
                            break;
                        case '\t': 
                            dst.WriteString("        ");
                            break;
                        default: 
                            dst.WriteByte(b);
                            break;
                    }

                    b = b__prev1;
                }
            }
            return error.As(dst.Flush());
        }

        // rgb returns an rgb value for the specified coverage value
        // between 0 (no coverage) and 10 (max coverage).
        private static @string rgb(long n)
        {
            if (n == 0L)
            {
                return "rgb(192, 0, 0)"; // Red
            } 
            // Gradient from gray to green.
            long r = 128L - 12L * (n - 1L);
            long g = 128L + 12L * (n - 1L);
            long b = 128L + 3L * (n - 1L);
            return fmt.Sprintf("rgb(%v, %v, %v)", r, g, b);
        }

        // colors generates the CSS rules for coverage colors.
        private static template.CSS colors()
        {
            bytes.Buffer buf = default;
            for (long i = 0L; i < 11L; i++)
            {
                fmt.Fprintf(ref buf, ".cov%v { color: %v }\n", i, rgb(i));
            }

            return template.CSS(buf.String());
        }

        private static var htmlTemplate = template.Must(template.New("html").Funcs(new template.FuncMap("colors":colors,)).Parse(tmplHTML));

        private partial struct templateData
        {
            public slice<ref templateFile> Files;
            public bool Set;
        }

        private partial struct templateFile
        {
            public @string Name;
            public template.HTML Body;
            public double Coverage;
        }

        private static readonly @string tmplHTML = "\n<!DOCTYPE html>\n<html>\n\t<head>\n\t\t<meta http-equiv=\"Content-Type\" content=\"text/h" +
    "tml; charset=utf-8\">\n\t\t<style>\n\t\t\tbody {\n\t\t\t\tbackground: black;\n\t\t\t\tcolor: rgb(8" +
    "0, 80, 80);\n\t\t\t}\n\t\t\tbody, pre, #legend span {\n\t\t\t\tfont-family: Menlo, monospace;" +
    "\n\t\t\t\tfont-weight: bold;\n\t\t\t}\n\t\t\t#topbar {\n\t\t\t\tbackground: black;\n\t\t\t\tposition: f" +
    "ixed;\n\t\t\t\ttop: 0; left: 0; right: 0;\n\t\t\t\theight: 42px;\n\t\t\t\tborder-bottom: 1px so" +
    "lid rgb(80, 80, 80);\n\t\t\t}\n\t\t\t#content {\n\t\t\t\tmargin-top: 50px;\n\t\t\t}\n\t\t\t#nav, #leg" +
    "end {\n\t\t\t\tfloat: left;\n\t\t\t\tmargin-left: 10px;\n\t\t\t}\n\t\t\t#legend {\n\t\t\t\tmargin-top: " +
    "12px;\n\t\t\t}\n\t\t\t#nav {\n\t\t\t\tmargin-top: 10px;\n\t\t\t}\n\t\t\t#legend span {\n\t\t\t\tmargin: 0 " +
    "5px;\n\t\t\t}\n\t\t\t{{colors}}\n\t\t</style>\n\t</head>\n\t<body>\n\t\t<div id=\"topbar\">\n\t\t\t<div " +
    "id=\"nav\">\n\t\t\t\t<select id=\"files\">\n\t\t\t\t{{range $i, $f := .Files}}\n\t\t\t\t<option val" +
    "ue=\"file{{$i}}\">{{$f.Name}} ({{printf \"%.1f\" $f.Coverage}}%)</option>\n\t\t\t\t{{end}" +
    "}\n\t\t\t\t</select>\n\t\t\t</div>\n\t\t\t<div id=\"legend\">\n\t\t\t\t<span>not tracked</span>\n\t\t\t{" +
    "{if .Set}}\n\t\t\t\t<span class=\"cov0\">not covered</span>\n\t\t\t\t<span class=\"cov8\">cove" +
    "red</span>\n\t\t\t{{else}}\n\t\t\t\t<span class=\"cov0\">no coverage</span>\n\t\t\t\t<span class" +
    "=\"cov1\">low coverage</span>\n\t\t\t\t<span class=\"cov2\">*</span>\n\t\t\t\t<span class=\"cov" +
    "3\">*</span>\n\t\t\t\t<span class=\"cov4\">*</span>\n\t\t\t\t<span class=\"cov5\">*</span>\n\t\t\t\t" +
    "<span class=\"cov6\">*</span>\n\t\t\t\t<span class=\"cov7\">*</span>\n\t\t\t\t<span class=\"cov" +
    "8\">*</span>\n\t\t\t\t<span class=\"cov9\">*</span>\n\t\t\t\t<span class=\"cov10\">high coverag" +
    "e</span>\n\t\t\t{{end}}\n\t\t\t</div>\n\t\t</div>\n\t\t<div id=\"content\">\n\t\t{{range $i, $f := " +
    ".Files}}\n\t\t<pre class=\"file\" id=\"file{{$i}}\" style=\"display: none\">{{$f.Body}}</" +
    "pre>\n\t\t{{end}}\n\t\t</div>\n\t</body>\n\t<script>\n\t(function() {\n\t\tvar files = document" +
    ".getElementById(\'files\');\n\t\tvar visible;\n\t\tfiles.addEventListener(\'change\', onCh" +
    "ange, false);\n\t\tfunction select(part) {\n\t\t\tif (visible)\n\t\t\t\tvisible.style.displa" +
    "y = \'none\';\n\t\t\tvisible = document.getElementById(part);\n\t\t\tif (!visible)\n\t\t\t\tret" +
    "urn;\n\t\t\tfiles.value = part;\n\t\t\tvisible.style.display = \'block\';\n\t\t\tlocation.hash" +
    " = part;\n\t\t}\n\t\tfunction onChange() {\n\t\t\tselect(files.value);\n\t\t\twindow.scrollTo(" +
    "0, 0);\n\t\t}\n\t\tif (location.hash != \"\") {\n\t\t\tselect(location.hash.substr(1));\n\t\t}\n" +
    "\t\tif (!visible) {\n\t\t\tselect(\"file0\");\n\t\t}\n\t})();\n\t</script>\n</html>\n";

    }
}
