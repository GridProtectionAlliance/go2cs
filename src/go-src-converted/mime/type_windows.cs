// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2020 October 09 04:56:15 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Go\src\mime\type_windows.go
using registry = go.@internal.syscall.windows.registry_package;
using static go.builtin;

namespace go
{
    public static partial class mime_package
    {
        private static void init()
        {
            osInitMime = initMimeWindows;
        }

        private static void initMimeWindows()
        {
            var (names, err) = registry.CLASSES_ROOT.ReadSubKeyNames(-1L);
            if (err != null)
            {
                return ;
            }

            foreach (var (_, name) in names)
            {
                if (len(name) < 2L || name[0L] != '.')
                { // looking for extensions only
                    continue;

                }

                var (k, err) = registry.OpenKey(registry.CLASSES_ROOT, name, registry.READ);
                if (err != null)
                {
                    continue;
                }

                var (v, _, err) = k.GetStringValue("Content Type");
                k.Close();
                if (err != null)
                {
                    continue;
                }

                setExtensionType(name, v);

            }

        }

        private static map<@string, @string> initMimeForTests()
        {
            return /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{".PnG":"image/png",};
        }
    }
}
