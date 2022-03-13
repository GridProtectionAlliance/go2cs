// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package driver -- go2cs converted at 2022 March 13 06:36:35 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\tempfile.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using sync = sync_package;


// newTempFile returns a new output file in dir with the provided prefix and suffix.

public static partial class driver_package {

private static (ptr<os.File>, error) newTempFile(@string dir, @string prefix, @string suffix) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;

    for (nint index = 1; index < 10000; index++) {
        {
            var (f, err) = os.OpenFile(filepath.Join(dir, fmt.Sprintf("%s%03d%s", prefix, index, suffix)), os.O_RDWR | os.O_CREATE | os.O_EXCL, 0666);


            if (err == null) 
                return (_addr_f!, error.As(null!)!);
            else if (!os.IsExist(err)) 
                return (_addr_null!, error.As(err)!);

        }
    } 
    // Give up
    return (_addr_null!, error.As(fmt.Errorf("could not create file of the form %s%03d%s", prefix, 1, suffix))!);
}

private static slice<@string> tempFiles = default;
private static sync.Mutex tempFilesMu = new sync.Mutex();

// deferDeleteTempFile marks a file to be deleted by next call to Cleanup()
private static void deferDeleteTempFile(@string path) {
    tempFilesMu.Lock();
    tempFiles = append(tempFiles, path);
    tempFilesMu.Unlock();
}

// cleanupTempFiles removes any temporary files selected for deferred cleaning.
private static error cleanupTempFiles() => func((defer, _, _) => {
    tempFilesMu.Lock();
    defer(tempFilesMu.Unlock());
    error lastErr = default!;
    foreach (var (_, f) in tempFiles) {
        {
            var err = os.Remove(f);

            if (err != null) {
                lastErr = error.As(err)!;
            }

        }
    }    tempFiles = null;
    return error.As(lastErr)!;
});

} // end driver_package
