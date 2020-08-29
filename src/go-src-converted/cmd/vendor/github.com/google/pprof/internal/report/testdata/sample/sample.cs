// sample program that is used to produce some of the files in
// pprof/internal/report/testdata.
// package main -- go2cs converted at 2020 August 29 10:06:12 UTC
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\report\testdata\sample\sample.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using math = go.math_package;
using os = go.os_package;
using pprof = go.runtime.pprof_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var cpuProfile = flag.String("cpuprofile", "", "where to write cpu profile");

        private static void Main() => func((defer, _, __) =>
        {
            flag.Parse();
            var (f, err) = os.Create(cpuProfile.Value);
            if (err != null)
            {
                log.Fatal("could not create CPU profile: ", err);
            }
            {
                var err = pprof.StartCPUProfile(f);

                if (err != null)
                {
                    log.Fatal("could not start CPU profile: ", err);
                }

            }
            defer(pprof.StopCPUProfile());
            busyLoop();
        });

        private static void busyLoop()
        {
            var m = make_map<long, long>();
            {
                long i__prev1 = i;

                for (long i = 0L; i < 1000000L; i++)
                {
                    m[i] = i + 10L;
                }


                i = i__prev1;
            }
            double sum = default;
            {
                long i__prev1 = i;

                for (i = 0L; i < 100L; i++)
                {
                    foreach (var (_, v) in m)
                    {
                        sum += math.Abs(float64(v));
                    }
                }


                i = i__prev1;
            }
            fmt.Println("Sum", sum);
        }
    }
}
