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
	if err := prepareRuntimeProject(project, runtime); err != nil {
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
