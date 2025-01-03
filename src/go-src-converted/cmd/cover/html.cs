// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:28:39 UTC
// Original source: C:\Program Files\Go\src\cmd\cover\html.go
namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using browser = cmd.@internal.browser_package;
using fmt = fmt_package;
using template = html.template_package;
using io = io_package;
using math = math_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;

using cover = golang.org.x.tools.cover_package;


// htmlOutput reads the profile data from profile and generates an HTML
// coverage report, writing it to outfile. If outfile is empty,
// it writes the report to a temporary file and opens it in a web browser.

public static partial class main_package {

private static error htmlOutput(@string profile, @string outfile) {
    var (profiles, err) = cover.ParseProfiles(profile);
    if (err != null) {
        return error.As(err)!;
    }
    templateData d = default;

    var (dirs, err) = findPkgs(profiles);
    if (err != null) {
        return error.As(err)!;
    }
    foreach (var (_, profile) in profiles) {
        var fn = profile.FileName;
        if (profile.Mode == "set") {
            d.Set = true;
        }
        var (file, err) = findFile(dirs, fn);
        if (err != null) {
            return error.As(err)!;
        }
        var (src, err) = os.ReadFile(file);
        if (err != null) {
            return error.As(fmt.Errorf("can't read %q: %v", fn, err))!;
        }
        ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
        err = htmlGen(_addr_buf, src, profile.Boundaries(src));
        if (err != null) {
            return error.As(err)!;
        }
        d.Files = append(d.Files, addr(new templateFile(Name:fn,Body:template.HTML(buf.String()),Coverage:percentCovered(profile),)));
    }    ptr<os.File> @out;
    if (outfile == "") {
        @string dir = default;
        dir, err = os.MkdirTemp("", "cover");
        if (err != null) {
            return error.As(err)!;
        }
        out, err = os.Create(filepath.Join(dir, "coverage.html"));
    }
    else
 {
        out, err = os.Create(outfile);
    }
    if (err != null) {
        return error.As(err)!;
    }
    err = htmlTemplate.Execute(out, d);
    {
        var err2 = @out.Close();

        if (err == null) {
            err = err2;
        }
    }
    if (err != null) {
        return error.As(err)!;
    }
    if (outfile == "") {
        if (!browser.Open("file://" + @out.Name())) {
            fmt.Fprintf(os.Stderr, "HTML output written to %s\n", @out.Name());
        }
    }
    return error.As(null!)!;
}

// percentCovered returns, as a percentage, the fraction of the statements in
// the profile covered by the test run.
// In effect, it reports the coverage of a given source file.
private static double percentCovered(ptr<cover.Profile> _addr_p) {
    ref cover.Profile p = ref _addr_p.val;

    long total = default;    long covered = default;

    foreach (var (_, b) in p.Blocks) {
        total += int64(b.NumStmt);
        if (b.Count > 0) {
            covered += int64(b.NumStmt);
        }
    }    if (total == 0) {
        return 0;
    }
    return float64(covered) / float64(total) * 100;
}

// htmlGen generates an HTML coverage report with the provided filename,
// source code, and tokens, and writes it to the given Writer.
private static error htmlGen(io.Writer w, slice<byte> src, slice<cover.Boundary> boundaries) {
    var dst = bufio.NewWriter(w);
    foreach (var (i) in src) {
        while (len(boundaries) > 0 && boundaries[0].Offset == i) {
            var b = boundaries[0];
            if (b.Start) {
                nint n = 0;
                if (b.Count > 0) {
                    n = int(math.Floor(b.Norm * 9)) + 1;
                }
                fmt.Fprintf(dst, "<span class=\"cov%v\" title=\"%v\">", n, b.Count);
            }
            else
 {
                dst.WriteString("</span>");
            }
            boundaries = boundaries[(int)1..];
        }
        {
            var b__prev1 = b;

            b = src[i];

            switch (b) {
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
    }    return error.As(dst.Flush())!;
}

// rgb returns an rgb value for the specified coverage value
// between 0 (no coverage) and 10 (max coverage).
private static @string rgb(nint n) {
    if (n == 0) {
        return "rgb(192, 0, 0)"; // Red
    }
    nint r = 128 - 12 * (n - 1);
    nint g = 128 + 12 * (n - 1);
    nint b = 128 + 3 * (n - 1);
    return fmt.Sprintf("rgb(%v, %v, %v)", r, g, b);
}

// colors generates the CSS rules for coverage colors.
private static template.CSS colors() {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    for (nint i = 0; i < 11; i++) {
        fmt.Fprintf(_addr_buf, ".cov%v { color: %v }\n", i, rgb(i));
    }
    return template.CSS(buf.String());
}

private static var htmlTemplate = template.Must(template.New("html").Funcs(new template.FuncMap("colors":colors,)).Parse(tmplHTML));

private partial struct templateData {
    public slice<ptr<templateFile>> Files;
    public bool Set;
}

// PackageName returns a name for the package being shown.
// It does this by choosing the penultimate element of the path
// name, so foo.bar/baz/foo.go chooses 'baz'. This is cheap
// and easy, avoids parsing the Go file, and gets a better answer
// for package main. It returns the empty string if there is
// a problem.
private static @string PackageName(this templateData td) {
    if (len(td.Files) == 0) {
        return "";
    }
    var fileName = td.Files[0].Name;
    var elems = strings.Split(fileName, "/"); // Package path is always slash-separated.
    // Return the penultimate non-empty element.
    for (var i = len(elems) - 2; i >= 0; i--) {
        if (elems[i] != "") {
            return elems[i];
        }
    }
    return "";
}

private partial struct templateFile {
    public @string Name;
    public template.HTML Body;
    public double Coverage;
}

private static readonly @string tmplHTML = "\n<!DOCTYPE html>\n<html>\n\t<head>\n\t\t<meta http-equiv=\"Content-Type\" content=\"text/h" +
    "tml; charset=utf-8\">\n\t\t<title>{{$pkg := .PackageName}}{{if $pkg}}{{$pkg}}: {{end" +
    "}}Go Coverage Report</title>\n\t\t<style>\n\t\t\tbody {\n\t\t\t\tbackground: black;\n\t\t\t\tcolo" +
    "r: rgb(80, 80, 80);\n\t\t\t}\n\t\t\tbody, pre, #legend span {\n\t\t\t\tfont-family: Menlo, mo" +
    "nospace;\n\t\t\t\tfont-weight: bold;\n\t\t\t}\n\t\t\t#topbar {\n\t\t\t\tbackground: black;\n\t\t\t\tpos" +
    "ition: fixed;\n\t\t\t\ttop: 0; left: 0; right: 0;\n\t\t\t\theight: 42px;\n\t\t\t\tborder-bottom" +
    ": 1px solid rgb(80, 80, 80);\n\t\t\t}\n\t\t\t#content {\n\t\t\t\tmargin-top: 50px;\n\t\t\t}\n\t\t\t#n" +
    "av, #legend {\n\t\t\t\tfloat: left;\n\t\t\t\tmargin-left: 10px;\n\t\t\t}\n\t\t\t#legend {\n\t\t\t\tmarg" +
    "in-top: 12px;\n\t\t\t}\n\t\t\t#nav {\n\t\t\t\tmargin-top: 10px;\n\t\t\t}\n\t\t\t#legend span {\n\t\t\t\tma" +
    "rgin: 0 5px;\n\t\t\t}\n\t\t\t{{colors}}\n\t\t</style>\n\t</head>\n\t<body>\n\t\t<div id=\"topbar\">\n" +
    "\t\t\t<div id=\"nav\">\n\t\t\t\t<select id=\"files\">\n\t\t\t\t{{range $i, $f := .Files}}\n\t\t\t\t<op" +
    "tion value=\"file{{$i}}\">{{$f.Name}} ({{printf \"%.1f\" $f.Coverage}}%)</option>\n\t\t" +
    "\t\t{{end}}\n\t\t\t\t</select>\n\t\t\t</div>\n\t\t\t<div id=\"legend\">\n\t\t\t\t<span>not tracked</sp" +
    "an>\n\t\t\t{{if .Set}}\n\t\t\t\t<span class=\"cov0\">not covered</span>\n\t\t\t\t<span class=\"co" +
    "v8\">covered</span>\n\t\t\t{{else}}\n\t\t\t\t<span class=\"cov0\">no coverage</span>\n\t\t\t\t<sp" +
    "an class=\"cov1\">low coverage</span>\n\t\t\t\t<span class=\"cov2\">*</span>\n\t\t\t\t<span cl" +
    "ass=\"cov3\">*</span>\n\t\t\t\t<span class=\"cov4\">*</span>\n\t\t\t\t<span class=\"cov5\">*</sp" +
    "an>\n\t\t\t\t<span class=\"cov6\">*</span>\n\t\t\t\t<span class=\"cov7\">*</span>\n\t\t\t\t<span cl" +
    "ass=\"cov8\">*</span>\n\t\t\t\t<span class=\"cov9\">*</span>\n\t\t\t\t<span class=\"cov10\">high" +
    " coverage</span>\n\t\t\t{{end}}\n\t\t\t</div>\n\t\t</div>\n\t\t<div id=\"content\">\n\t\t{{range $i" +
    ", $f := .Files}}\n\t\t<pre class=\"file\" id=\"file{{$i}}\" style=\"display: none\">{{$f." +
    "Body}}</pre>\n\t\t{{end}}\n\t\t</div>\n\t</body>\n\t<script>\n\t(function() {\n\t\tvar files = " +
    "document.getElementById(\'files\');\n\t\tvar visible;\n\t\tfiles.addEventListener(\'chang" +
    "e\', onChange, false);\n\t\tfunction select(part) {\n\t\t\tif (visible)\n\t\t\t\tvisible.styl" +
    "e.display = \'none\';\n\t\t\tvisible = document.getElementById(part);\n\t\t\tif (!visible)" +
    "\n\t\t\t\treturn;\n\t\t\tfiles.value = part;\n\t\t\tvisible.style.display = \'block\';\n\t\t\tlocat" +
    "ion.hash = part;\n\t\t}\n\t\tfunction onChange() {\n\t\t\tselect(files.value);\n\t\t\twindow.s" +
    "crollTo(0, 0);\n\t\t}\n\t\tif (location.hash != \"\") {\n\t\t\tselect(location.hash.substr(1" +
    "));\n\t\t}\n\t\tif (!visible) {\n\t\t\tselect(\"file0\");\n\t\t}\n\t})();\n\t</script>\n</html>\n";


} // end main_package
