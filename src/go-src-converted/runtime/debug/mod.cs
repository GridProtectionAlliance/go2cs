// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;

partial class debug_package {

// exported from runtime.
internal static partial @string modinfo();

// ReadBuildInfo returns the build information embedded
// in the running binary. The information is available only
// in binaries built with module support.
public static (ж<BuildInfo> info, bool ok) ReadBuildInfo() {
    ж<BuildInfo> info = default!;
    bool ok = default!;

    @string data = modinfo();
    if (len(data) < 32) {
        return (default!, false);
    }
    data = data[16..(int)(len(data) - 16)];
    (bi, err) = ParseBuildInfo(data);
    if (err != default!) {
        return (default!, false);
    }
    // The go version is stored separately from other build info, mostly for
    // historical reasons. It is not part of the modinfo() string, and
    // ParseBuildInfo does not recognize it. We inject it here to hide this
    // awkwardness from the user.
    bi.val.GoVersion = runtime.Version();
    return (bi, true);
}

// BuildInfo represents the build information read from a Go binary.
[GoType] partial struct BuildInfo {
    // GoVersion is the version of the Go toolchain that built the binary
    // (for example, "go1.19.2").
    public @string GoVersion;
    // Path is the package path of the main package for the binary
    // (for example, "golang.org/x/tools/cmd/stringer").
    public @string Path;
    // Main describes the module that contains the main package for the binary.
    public Module Main;
    // Deps describes all the dependency modules, both direct and indirect,
    // that contributed packages to the build of this binary.
    public slice<ж<Module>> Deps;
    // Settings describes the build settings used to build the binary.
    public slice<BuildSetting> Settings;
}

// A Module describes a single module included in a build.
[GoType] partial struct Module {
    public @string Path; // module path
    public @string Version; // module version
    public @string Sum; // checksum
    public ж<Module> Replace; // replaced by this module
}

// A BuildSetting is a key-value pair describing one setting that influenced a build.
//
// Defined keys include:
//
//   - -buildmode: the buildmode flag used (typically "exe")
//   - -compiler: the compiler toolchain flag used (typically "gc")
//   - CGO_ENABLED: the effective CGO_ENABLED environment variable
//   - CGO_CFLAGS: the effective CGO_CFLAGS environment variable
//   - CGO_CPPFLAGS: the effective CGO_CPPFLAGS environment variable
//   - CGO_CXXFLAGS:  the effective CGO_CXXFLAGS environment variable
//   - CGO_LDFLAGS: the effective CGO_LDFLAGS environment variable
//   - GOARCH: the architecture target
//   - GOAMD64/GOARM/GO386/etc: the architecture feature level for GOARCH
//   - GOOS: the operating system target
//   - vcs: the version control system for the source tree where the build ran
//   - vcs.revision: the revision identifier for the current commit or checkout
//   - vcs.time: the modification time associated with vcs.revision, in RFC3339 format
//   - vcs.modified: true or false indicating whether the source tree had local modifications
[GoType] partial struct BuildSetting {
    // Key and Value describe the build setting.
    // Key must not contain an equals sign, space, tab, or newline.
    // Value must not contain newlines ('\n').
    public @string Key;
    public @string Value;
}

// quoteKey reports whether key is required to be quoted.
internal static bool quoteKey(@string key) {
    return len(key) == 0 || strings.ContainsAny(key, "= \t\r\n\"`"u8);
}

// quoteValue reports whether value is required to be quoted.
internal static bool quoteValue(@string value) {
    return strings.ContainsAny(value, " \t\r\n\"`"u8);
}

[GoRecv] public static @string String(this ref BuildInfo bi) {
    var buf = @new<strings.Builder>();
    if (bi.GoVersion != ""u8) {
        fmt.Fprintf(~buf, "go\t%s\n"u8, bi.GoVersion);
    }
    if (bi.Path != ""u8) {
        fmt.Fprintf(~buf, "path\t%s\n"u8, bi.Path);
    }
    Action<@string, Module> formatMod = default!;
    formatMod = 
    var bufʗ1 = buf;
    var formatModʗ1 = formatMod;
    (@string word, Module m) => {
        bufʗ1.WriteString(word);
        bufʗ1.WriteByte((rune)'\t');
        bufʗ1.WriteString(m.Path);
        bufʗ1.WriteByte((rune)'\t');
        bufʗ1.WriteString(m.Version);
        if (m.Replace == nil){
            bufʗ1.WriteByte((rune)'\t');
            bufʗ1.WriteString(m.Sum);
        } else {
            bufʗ1.WriteByte((rune)'\n');
            formatModʗ1("=>"u8, m.Replace.val);
        }
        bufʗ1.WriteByte((rune)'\n');
    };
    if (bi.ΔMain != (new Module(nil))) {
        formatMod("mod"u8, bi.ΔMain);
    }
    foreach (var (_, dep) in bi.Deps) {
        formatMod("dep"u8, dep.val);
    }
    foreach (var (_, s) in bi.Settings) {
        @string key = s.Key;
        if (quoteKey(key)) {
            key = strconv.Quote(key);
        }
        @string value = s.Value;
        if (quoteValue(value)) {
            value = strconv.Quote(value);
        }
        fmt.Fprintf(~buf, "build\t%s=%s\n"u8, key, value);
    }
    return buf.String();
}

public static (ж<BuildInfo> bi, error err) ParseBuildInfo(@string data) => func((defer, _) => {
    ж<BuildInfo> bi = default!;
    error errΔ1 = default!;

    nint lineNum = 1;
    defer(() => {
        if (errΔ1 != default!) {
             = fmt.Errorf("could not parse Go build info: line %d: %w"u8, lineNum, errΔ1);
        }
    });
    @string pathLine = "path\t"u8;
    @string modLine = "mod\t"u8;
    @string depLine = "dep\t"u8;
    @string repLine = "=>\t"u8;
    @string buildLine = "build\t"u8;
    @string newline = "\n"u8;
    @string tab = "\t"u8;
    var readModuleLine = (slice<@string> elem) => {
        if (len(elem) != 2 && len(elem) != 3) {
            return (new Module(nil), fmt.Errorf("expected 2 or 3 columns; got %d"u8, len(elem)));
        }
        @string version = elem[1];
        @string sum = ""u8;
        if (len(elem) == 3) {
            sum = elem[2];
        }
        return (new Module(
            Path: elem[0],
            Version: version,
            Sum: sum
        ), default!);
    };
    bi = @new<BuildInfo>();
    ж<Module> last = default!;
    @string line = default!;
    bool okΔ1 = default!;
    // Reverse of BuildInfo.String(), except for go version.
    while (len(data) > 0) {
        (line, data, ) = strings.Cut(data, newline);
        if (!okΔ1) {
            break;
        }
        switch (ᐧ) {
        case {} when strings.HasPrefix(line, pathLine): {
            @string elem = line[(int)(len(pathLine))..];
            bi.val.Path = ((@string)elem);
            break;
        }
        case {} when strings.HasPrefix(line, modLine): {
            var elem = strings.Split(line[(int)(len(modLine))..], tab);
            last = Ꮡ((~bi).ΔMain);
            (last.val, errΔ5) = readModuleLine(elem);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            break;
        }
        case {} when strings.HasPrefix(line, depLine): {
            var elem = strings.Split(line[(int)(len(depLine))..], tab);
            last = @new<Module>();
            bi.val.Deps = append((~bi).Deps, last);
            (last.val, errΔ5) = readModuleLine(elem);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            break;
        }
        case {} when strings.HasPrefix(line, repLine): {
            var elem = strings.Split(line[(int)(len(repLine))..], tab);
            if (len(elem) != 3) {
                return (default!, fmt.Errorf("expected 3 columns for replacement; got %d"u8, len(elem)));
            }
            if (last == nil) {
                return (default!, fmt.Errorf("replacement with no module on previous line"u8));
            }
            last.val.Replace = Ꮡ(new Module(
                Path: ((@string)elem[0]),
                Version: ((@string)elem[1]),
                Sum: ((@string)elem[2])
            ));
            last = default!;
            break;
        }
        case {} when strings.HasPrefix(line, buildLine): {
            @string kv = line[(int)(len(buildLine))..];
            if (len(kv) < 1) {
                return (default!, fmt.Errorf("build line missing '='"u8));
            }
            @string key = default!;
            @string rawValue = default!;
            switch (kv[0]) {
            case (rune)'=': {
                return (default!, fmt.Errorf("build line with missing key"u8));
            }
            case (rune)'`' or (rune)'"': {
                var (rawKey, errΔ7) = strconv.QuotedPrefix(kv);
                if (errΔ7 != default!) {
                    return (default!, fmt.Errorf("invalid quoted key in build line"u8));
                }
                if (len(kv) == len(rawKey)) {
                    return (default!, fmt.Errorf("build line missing '=' after quoted key"u8));
                }
                {
                    var c = kv[len(rawKey)]; if (c != (rune)'=') {
                        return (default!, fmt.Errorf("unexpected character after quoted key: %q"u8, c));
                    }
                }
                (key, _) = strconv.Unquote(rawKey);
                rawValue = kv[(int)(len(rawKey) + 1)..];
                break;
            }
            default: {
                bool okΔ3 = default!;
                (key, rawValue, okΔ3) = strings.Cut(kv, "="u8);
                if (!okΔ3) {
                    return (default!, fmt.Errorf("build line missing '=' after key"u8));
                }
                if (quoteKey(key)) {
                    return (default!, fmt.Errorf("unquoted key %q must be quoted"u8, key));
                }
                break;
            }}

            @string value = default!;
            if (len(rawValue) > 0) {
                switch (rawValue[0]) {
                case (rune)'`' or (rune)'"': {
                    error errΔ9 = default!;
                    (value, errΔ9) = strconv.Unquote(rawValue);
                    if (errΔ9 != default!) {
                        return (default!, fmt.Errorf("invalid quoted value in build line"u8));
                    }
                    break;
                }
                default: {
                    value = rawValue;
                    if (quoteValue(value)) {
                        return (default!, fmt.Errorf("unquoted value %q must be quoted"u8, value));
                    }
                    break;
                }}

            }
            bi.val.Settings = append((~bi).Settings, new BuildSetting(Key: key, Value: value));
            break;
        }}

        lineNum++;
    }
    return (bi, default!);
});

} // end debug_package
