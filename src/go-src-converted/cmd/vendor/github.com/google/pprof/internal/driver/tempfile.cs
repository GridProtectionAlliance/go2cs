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

// package driver -- go2cs converted at 2020 October 09 05:53:32 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\tempfile.go
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        // newTempFile returns a new output file in dir with the provided prefix and suffix.
        private static (ptr<os.File>, error) newTempFile(@string dir, @string prefix, @string suffix)
        {
            ptr<os.File> _p0 = default!;
            error _p0 = default!;

            for (long index = 1L; index < 10000L; index++)
            {
                var path = filepath.Join(dir, fmt.Sprintf("%s%03d%s", prefix, index, suffix));
                {
                    var (_, err) = os.Stat(path);

                    if (err != null)
                    {
                        return _addr_os.Create(path)!;
                    }
                }

            } 
            // Give up
            return (_addr_null!, error.As(fmt.Errorf("could not create file of the form %s%03d%s", prefix, 1L, suffix))!);

        }

        private static slice<@string> tempFiles = default;
        private static sync.Mutex tempFilesMu = new sync.Mutex();

        // deferDeleteTempFile marks a file to be deleted by next call to Cleanup()
        private static void deferDeleteTempFile(@string path)
        {
            tempFilesMu.Lock();
            tempFiles = append(tempFiles, path);
            tempFilesMu.Unlock();
        }

        // cleanupTempFiles removes any temporary files selected for deferred cleaning.
        private static void cleanupTempFiles()
        {
            tempFilesMu.Lock();
            foreach (var (_, f) in tempFiles)
            {
                os.Remove(f);
            }
            tempFiles = null;
            tempFilesMu.Unlock();

        }
    }
}}}}}}}
