// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package auth -- go2cs converted at 2022 March 06 23:17:19 UTC
// import "cmd/go/internal/auth" ==> using auth = go.cmd.go.@internal.auth_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\auth\netrc.go
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using sync = go.sync_package;

namespace go.cmd.go.@internal;

public static partial class auth_package {

private partial struct netrcLine {
    public @string machine;
    public @string login;
    public @string password;
}

private static sync.Once netrcOnce = default;private static slice<netrcLine> netrc = default;private static error netrcErr = default!;

private static slice<netrcLine> parseNetrc(@string data) { 
    // See https://www.gnu.org/software/inetutils/manual/html_node/The-_002enetrc-file.html
    // for documentation on the .netrc format.
    slice<netrcLine> nrc = default;
    netrcLine l = default;
    var inMacro = false;
    foreach (var (_, line) in strings.Split(data, "\n")) {
        if (inMacro) {
            if (line == "") {
                inMacro = false;
            }
            continue;
        }
        var f = strings.Fields(line);
        nint i = 0;
        while (i < len(f) - 1) { 
            // Reset at each "machine" token.
            // “The auto-login process searches the .netrc file for a machine token
            // that matches […]. Once a match is made, the subsequent .netrc tokens
            // are processed, stopping when the end of file is reached or another
            // machine or a default token is encountered.”
            switch (f[i]) {
                case "machine": 
                    l = new netrcLine(machine:f[i+1]);
                    break;
                case "default": 
                    break;
                    break;
                case "login": 
                    l.login = f[i + 1];
                    break;
                case "password": 
                    l.password = f[i + 1];
                    break;
                case "macdef": 
                    // “A macro is defined with the specified name; its contents begin with
                    // the next .netrc line and continue until a null line (consecutive
                    // new-line characters) is encountered.”
                    inMacro = true;
                    break;
            }
            if (l.machine != "" && l.login != "" && l.password != "") {
                nrc = append(nrc, l);
                l = new netrcLine();
            i += 2;
            }

        }

        if (i < len(f) && f[i] == "default") { 
            // “There can be only one default token, and it must be after all machine tokens.”
            break;

        }
    }    return nrc;

}

private static (@string, error) netrcPath() {
    @string _p0 = default;
    error _p0 = default!;

    {
        var env = os.Getenv("NETRC");

        if (env != "") {
            return (env, error.As(null!)!);
        }
    }

    var (dir, err) = os.UserHomeDir();
    if (err != null) {
        return ("", error.As(err)!);
    }
    @string @base = ".netrc";
    if (runtime.GOOS == "windows") {
        base = "_netrc";
    }
    return (filepath.Join(dir, base), error.As(null!)!);

}

private static void readNetrc() {
    var (path, err) = netrcPath();
    if (err != null) {
        netrcErr = err;
        return ;
    }
    var (data, err) = os.ReadFile(path);
    if (err != null) {
        if (!os.IsNotExist(err)) {
            netrcErr = err;
        }
        return ;

    }
    netrc = parseNetrc(string(data));

}

} // end auth_package
