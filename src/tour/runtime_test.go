package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestPackageIDForProjectReference(t *testing.T) {
	tests := map[string]string{
		`$(go2csPath)gen\go2cs-gen\go2cs-gen.csproj`:            "go.gen",
		`$(go2csPath)core\golib\golib.csproj`:                   "go.lib",
		`$(go2csPath)core\fmt\fmt.csproj`:                       "go.fmt",
		`$(go2csPath)go-src-converted\unicode\utf8\utf8.csproj`: "go.unicode.utf8",
	}
	for reference, want := range tests {
		got, ok := packageIDForProjectReference(reference)
		if !ok || got != want {
			t.Errorf("packageIDForProjectReference(%q) = %q, %v; want %q, true", reference, got, ok, want)
		}
	}
}

func TestResolveRuntimeUsesConfiguredDefault(t *testing.T) {
	runner := newPipelineRunner(t.TempDir(), pipelineOptions{
		defaultRuntime: "NuGet",
		nugetSource:    t.TempDir(),
		nugetVersion:   "1.23.1.1",
	})
	defer runner.close()

	resolved, err := runner.resolveRuntime("")
	if err != nil {
		t.Fatal(err)
	}
	if resolved.mode != runtimeNuGet {
		t.Fatalf("default runtime = %q, want %q", resolved.mode, runtimeNuGet)
	}
}

func TestResolvePipelineOptionsSelectsAvailableDeployedDefault(t *testing.T) {
	deployedRoot := t.TempDir()
	writeRuntimeRoot(t, deployedRoot)

	options := resolvePipelineOptions(t.TempDir(), pipelineOptions{deployedRoot: deployedRoot})
	if options.defaultRuntime != runtimeDeployed {
		t.Fatalf("default runtime = %q, want %q", options.defaultRuntime, runtimeDeployed)
	}
}

func TestResolvePipelineOptionsFallsBackToCore(t *testing.T) {
	options := resolvePipelineOptions(t.TempDir(), pipelineOptions{deployedRoot: t.TempDir()})
	if options.defaultRuntime != runtimeCore {
		t.Fatalf("default runtime = %q, want %q", options.defaultRuntime, runtimeCore)
	}
}

func TestResolvePipelineOptionsHonorsExplicitCore(t *testing.T) {
	deployedRoot := t.TempDir()
	writeRuntimeRoot(t, deployedRoot)

	options := resolvePipelineOptions(t.TempDir(), pipelineOptions{
		defaultRuntime: runtimeCore,
		deployedRoot:   deployedRoot,
	})
	if options.defaultRuntime != runtimeCore {
		t.Fatalf("default runtime = %q, want explicit %q", options.defaultRuntime, runtimeCore)
	}
}

func TestResolveRuntimeRejectsUnknownDefault(t *testing.T) {
	runner := newPipelineRunner(t.TempDir(), pipelineOptions{defaultRuntime: "unknown"})
	defer runner.close()

	if _, err := runner.resolveRuntime(""); err == nil {
		t.Fatal("unknown default runtime was accepted")
	}
}

func TestPrepareRuntimeProjectUsesNuGetPackages(t *testing.T) {
	root := t.TempDir()
	project := filepath.Join(root, "tour.local.csproj")
	content := `<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="$(go2csPath)gen\go2cs-gen\go2cs-gen.csproj" OutputItemType="Analyzer" />
    <ProjectReference Include="$(go2csPath)core\golib\golib.csproj" />
    <ProjectReference Include="$(go2csPath)core\fmt\fmt.csproj" />
  </ItemGroup>
</Project>`
	if err := os.WriteFile(project, []byte(content), 0o600); err != nil {
		t.Fatal(err)
	}

	runtime := runtimeConfiguration{
		mode:         runtimeNuGet,
		nugetSource:  filepath.Join(root, "packages"),
		nugetVersion: "1.23.1.1",
	}
	if err := prepareRuntimeProject(project, root, runtime); err != nil {
		t.Fatal(err)
	}

	updated, err := os.ReadFile(project)
	if err != nil {
		t.Fatal(err)
	}
	text := string(updated)
	for _, want := range []string{
		`<PackageReference Include="go.gen" Version="1.23.1.1" PrivateAssets="all" />`,
		`<PackageReference Include="go.lib" Version="1.23.1.1" />`,
		`<PackageReference Include="go.fmt" Version="1.23.1.1" />`,
	} {
		if !strings.Contains(text, want) {
			t.Errorf("updated project missing %q:\n%s", want, text)
		}
	}
	if strings.Contains(text, "ProjectReference") {
		t.Errorf("updated project still contains project references:\n%s", text)
	}
	if _, err := os.Stat(filepath.Join(root, "NuGet.config")); err != nil {
		t.Fatalf("NuGet.config was not generated: %v", err)
	}
}

func TestPrepareRuntimeProjectAcceptsRecursiveNuGetOutput(t *testing.T) {
	root := t.TempDir()
	projectDir := filepath.Join(root, "src", "tour.local", "session")
	if err := os.MkdirAll(projectDir, 0o755); err != nil {
		t.Fatal(err)
	}
	project := filepath.Join(projectDir, "tour.local.session.csproj")
	content := `<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="go.gen" Version="$(GoStdLibVersion)" PrivateAssets="all" />
    <PackageReference Include="go.lib" Version="$(GoStdLibVersion)" />
    <PackageReference Include="go.fmt" Version="$(GoStdLibVersion)" />
    <ProjectReference Include="..\..\..\pkg\golang.org\x\tour\wc\golang.org.x.tour.wc.csproj" />
  </ItemGroup>
</Project>`
	if err := os.WriteFile(project, []byte(content), 0o600); err != nil {
		t.Fatal(err)
	}

	runtime := runtimeConfiguration{
		mode:         runtimeNuGet,
		nugetSource:  filepath.Join(root, "packages"),
		nugetVersion: "1.23.1.1",
	}
	if err := prepareRuntimeProject(project, root, runtime); err != nil {
		t.Fatal(err)
	}
	if _, err := os.Stat(filepath.Join(root, "NuGet.config")); err != nil {
		t.Fatalf("root NuGet.config was not generated: %v", err)
	}
}

func TestRuntimeMSBuildArgs(t *testing.T) {
	local := runtimeConfiguration{mode: runtimeCore, projectRoot: filepath.FromSlash("C:/go2cs")}
	localArgs := runtimeMSBuildArgs(local)
	if len(localArgs) != 1 || !strings.HasPrefix(localArgs[0], "-p:go2csPath=") {
		t.Fatalf("local runtime args = %v", localArgs)
	}

	nuget := runtimeConfiguration{mode: runtimeNuGet, nugetVersion: "1.23.1.7"}
	nugetArgs := runtimeMSBuildArgs(nuget)
	if len(nugetArgs) != 1 || nugetArgs[0] != "-p:GoStdLibVersion=1.23.1.7" {
		t.Fatalf("NuGet runtime args = %v", nugetArgs)
	}
}

func writeRuntimeRoot(t *testing.T, root string) {
	t.Helper()
	for _, name := range []string{
		filepath.Join("core", "VERSION"),
		filepath.Join("core", "golib", "golib.csproj"),
		filepath.Join("gen", "go2cs-gen", "go2cs-gen.csproj"),
	} {
		path := filepath.Join(root, name)
		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(path, nil, 0o600); err != nil {
			t.Fatal(err)
		}
	}
}
