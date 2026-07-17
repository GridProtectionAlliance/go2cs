# Converted package-test fixtures

`ConvertedTestHarness` is the end-to-end fixture for the opt-in converted Go test path (`-tests`).
It covers same-package access to an unexported declaration, an external `package p_test` with both
the named and the DOT self-import form (`. "go2cs/convertedtestharness"` — the shape the
`unicode/utf8` first-proof package requires), typed discovery, `TestMain`, duplicate parallel
subtests, cleanup, `TempDir`, and a `testdata` fixture read from the isolated working directory. (Invalid
test names like `Testlower` cannot live in the fixture — `go test`'s default vet `tests` analyzer
refuses to build them — so non-registration is guarded by the converter's `TestIsGoTestName`.)

From the repository root, build the converter (`go build -o bin/go2cs.exe .` in `src/go2cs`) and run:

```text
go2cs -tests -test-action all -go2cspath <repository>/src <repository>/src/Tests/PackageTests/ConvertedTestHarness
```

The command converts production and test sources, builds and runs the generated C# host in an
isolated process, captures a clean `go test -json -count=1` baseline, and compares terminal
results by full Go test name. Artifacts (converted `.cs`, the `.tests.csproj`, the manifest, and
the comparison/results files) are regenerated in place and are gitignored.
