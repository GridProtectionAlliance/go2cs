package main

import (
	"bufio"
	"errors"
	"fmt"
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
)

// PackageInfo represents information about a package
type PackageInfo struct {
	IsStdLib         bool
	PackageName      string
	RootPackageName  string
	SourceDir        string
	TargetDir        string
	ProjectReference string
	Err              error
}

func getProjectName(importPath string, options Options) (string, string) {
	if strings.HasPrefix(importPath, options.goRoot) {
		importPath = pathReplace(importPath, filepath.Join(options.goRoot, "src"), "")
	} else {
		// Check if current folder has go.mod or main.go
		if _, err := os.Stat(filepath.Join(importPath, "go.mod")); err == nil {
			// If we have a go.mod, try to read the module name from it
			if moduleName := readModuleFromGoMod(filepath.Join(importPath, "go.mod")); moduleName != "" {
				// Append remaining path segments if importPath has subdirectories
				relPath := ""
				if filepath.Base(importPath) != importPath {
					// Get the relative path from the directory containing go.mod to the importPath
					relPath = getRelativePath(importPath, importPath)
					if relPath != "" {
						moduleName = filepath.Join(moduleName, relPath)
					}
				}
				importPath = moduleName
			} else {
				importPath = filepath.Base(importPath)
			}
		} else if _, err := os.Stat(filepath.Join(importPath, "main.go")); err == nil {
			importPath = filepath.Base(importPath)
		} else {
			// Check if current folder has no go files
			if !hasGoFiles(importPath) {
				// If user provided path has no go files, we will assume current path
				// for project name and let parser fail since it is not a valid package
				importPath = filepath.Base(importPath)
			} else {
				// At this point, current folder has go files, but no go.mod or main.go
				// Keep traversing up the directory tree until we find go.mod or main.go
				// or no go files or we reach the root directory
				currentPath := importPath
				lastGoFilePath := currentPath // Keep track of the last path with Go files

				for {
					parentDir := filepath.Dir(currentPath)

					if parentDir == currentPath {
						// Reached the root directory
						importPath = filepath.Base(importPath)
						break
					}

					currentPath = parentDir

					if _, err := os.Stat(filepath.Join(currentPath, "go.mod")); err == nil {
						// Found go.mod, use module name and append relative path
						if moduleName := readModuleFromGoMod(filepath.Join(currentPath, "go.mod")); moduleName != "" {
							// Get relative path from module root to import path
							relPath := getRelativePath(importPath, currentPath)
							if relPath != "" {
								importPath = filepath.Join(moduleName, relPath)
							} else {
								importPath = moduleName
							}
						} else {
							// Fallback if module name can't be read
							importPath = filepath.Base(currentPath) + "." + getRelativePath(importPath, currentPath)
						}

						break
					} else if _, err := os.Stat(filepath.Join(currentPath, "main.go")); err == nil {
						// Found main.go, get relative path from main.go directory to import path
						relPath := getRelativePath(importPath, currentPath)

						if relPath != "" {
							importPath = filepath.Base(currentPath) + "." + relPath
						} else {
							importPath = filepath.Base(currentPath)
						}

						break
					} else if !hasGoFiles(currentPath) {
						// No Go files in this directory, use the last directory with Go files
						relPath := getRelativePath(importPath, lastGoFilePath)

						if relPath != "" {
							importPath = filepath.Base(lastGoFilePath) + "." + relPath
						} else {
							importPath = filepath.Base(lastGoFilePath)
						}

						break
					}

					// Update last path with Go files if current directory has Go files
					if hasGoFiles(currentPath) {
						lastGoFilePath = currentPath
					}
				}
			}
		}
	}

	importPath = strings.ReplaceAll(importPath, "\\", "/")
	importPath = strings.TrimPrefix(importPath, "/")
	importPath = strings.TrimPrefix(importPath, "go2cs/")

	// Replace path separators with dots
	parts := strings.Split(importPath, "/")

	projectName := strings.Join(parts, ".")
	namespace := RootNamespace

	if len(parts) > 1 {

		for i := 0; i < len(parts)-1; i++ {
			namespace += "." + getCoreSanitizedIdentifier(parts[i])
		}
	}

	return projectName, namespace
}

// readModuleFromGoMod reads the module name from a go.mod file
func readModuleFromGoMod(goModPath string) string {
	data, err := os.ReadFile(goModPath)

	if err != nil {
		return ""
	}

	re := regexp.MustCompile(`module\s+(.+)`)
	matches := re.FindSubmatch(data)

	if len(matches) > 1 {
		return strings.TrimSpace(string(matches[1]))
	}

	return ""
}

// getRelativePath returns the relative path from basePath to targetPath
func getRelativePath(targetPath, basePath string) string {
	rel, err := filepath.Rel(basePath, targetPath)

	if err != nil {
		return ""
	}

	// If the paths are the same, return empty string
	if rel == "." {
		return ""
	}

	return rel
}

// hasGoFiles checks if the specified directory contains any .go files
func hasGoFiles(dirPath string) bool {
	// Pattern to match .go files in the specified directory
	pattern := filepath.Join(dirPath, "*.go")

	// Find all files matching the pattern
	matches, err := filepath.Glob(pattern)

	if err != nil {
		return false
	}

	// If we found at least one match, return true
	return len(matches) > 0
}

// ImportInfo returns information about whether the packages are from the standard
// library and their physical directories
func getImportPackageInfo(importPaths []string, options Options) map[string]PackageInfo {
	result := make(map[string]PackageInfo, len(importPaths))

	for _, importPath := range importPaths {
		pkg, err := build.Import(importPath, "", build.FindOnly)

		// go/build (GOPATH-based) cannot resolve a LOCAL/USER module reached via a `replace`
		// directive or otherwise outside GOPATH/GOROOT. Fall back to the module-aware go/packages
		// dir captured at load time, treating its converted output as in-place (co-located with the
		// Go source) — the common layout when converting a whole module tree.
		if err != nil {
			if info, ok := getLocalModulePackageInfo(importPath, options); ok {
				result[importPath] = info
			} else {
				result[importPath] = PackageInfo{Err: err}
			}
			continue
		}

		// Standard library packages are located in GOROOT
		isStdLib := pkg.Goroot && !build.IsLocalImport(importPath)

		sourceDir := pkg.Dir
		var targetDir string

		if isStdLib {
			targetDir = pathReplace(sourceDir, filepath.Join(options.goRoot, "src"), "$(go2csPath)core")
		} else {
			targetDir = pathReplace(sourceDir, filepath.Join(options.goPath, "pkg"), "$(go2csPath)pkg")
		}

		importPathParts := strings.Split(importPath, "/")
		packageName := strings.Join(importPathParts, ".")
		projectReference := filepath.Join(strings.ReplaceAll(targetDir, "/", "\\"), "\\"+packageName+".csproj")
		targetDir = strings.ReplaceAll(targetDir, "$(go2csPath)", options.go2csPath+string(os.PathSeparator))
		packageNameParts := strings.Split(packageName, ".")

		result[importPath] = PackageInfo{
			IsStdLib:         isStdLib,
			PackageName:      packageName,
			RootPackageName:  packageNameParts[len(packageNameParts)-1],
			SourceDir:        sourceDir,
			TargetDir:        targetDir,
			ProjectReference: projectReference,
		}
	}

	return result
}

// getLocalModulePackageInfo resolves cross-package reference info for a LOCAL/USER module import that
// go/build could not find, using the module-aware go/packages dir captured in importPackageDirs. The
// converted output is treated as in-place (co-located with the Go source). The returned
// ProjectReference is an ABSOLUTE path to the imported package's generated .csproj; writeProjectFile
// rewrites it relative to the referencing project. RootPackageName/PackageName are the Go package name
// (the identifier used to qualify references in code, and — for a single-segment module — the C# class
// base `<name>_package`). Returns ok=false when the import is unknown to the loaded graph.
func getLocalModulePackageInfo(importPath string, options Options) (PackageInfo, bool) {
	meta, ok := importPackageDirs[importPath]

	if !ok || meta.Dir == "" {
		return PackageInfo{}, false
	}

	// Defense in depth: build.Import is pinned to the resolved GOROOT (see main), but if it still
	// fails for a STDLIB package, the dir is under GOROOT/src — apply the stdlib `$(go2csPath)core`
	// mapping rather than emitting a machine-specific absolute reference as if it were a user module.
	goRootSrc := filepath.Join(options.goRoot, "src")

	if isPathUnder(meta.Dir, goRootSrc) {
		targetDir := pathReplace(meta.Dir, goRootSrc, "$(go2csPath)core")
		packageName := strings.Join(strings.Split(importPath, "/"), ".")
		projectReference := filepath.Join(strings.ReplaceAll(targetDir, "/", "\\"), "\\"+packageName+".csproj")
		packageNameParts := strings.Split(packageName, ".")

		return PackageInfo{
			IsStdLib:         true,
			PackageName:      packageName,
			RootPackageName:  packageNameParts[len(packageNameParts)-1],
			SourceDir:        meta.Dir,
			TargetDir:        strings.ReplaceAll(targetDir, "$(go2csPath)", options.go2csPath+string(os.PathSeparator)),
			ProjectReference: projectReference,
		}, true
	}

	// A genuine LOCAL/USER module: its converted output is in-place (co-located with its Go source),
	// and it generates `<projectName>.csproj` in its own directory. The absolute ProjectReference is
	// rewritten relative to the referencing project by writeProjectFile.
	libProjectName, _ := getProjectName(meta.Dir, options)
	projectReference := filepath.Join(meta.Dir, libProjectName+".csproj")

	return PackageInfo{
		IsStdLib:         false,
		PackageName:      meta.Name,
		RootPackageName:  meta.Name,
		SourceDir:        meta.Dir,
		TargetDir:        meta.Dir,
		ProjectReference: projectReference,
	}, true
}

// isPathUnder reports whether path is the directory dir or nested within it (case-insensitive on
// Windows), used to recognize a stdlib package by its location under GOROOT/src.
func isPathUnder(path, dir string) bool {
	rel, err := filepath.Rel(dir, path)
	if err != nil {
		return false
	}

	return rel != ".." && !strings.HasPrefix(rel, ".."+string(os.PathSeparator))
}

func pathReplace(subject string, search string, replace string) string {
	// Execute case insensitive replacement on Windows, otherwise use the standard replace function
	if runtime.GOOS == "windows" {
		searchEscaped := regexp.QuoteMeta(search)
		searchRE := regexp.MustCompile("(?i)" + searchEscaped)
		return searchRE.ReplaceAllString(subject, replace)
	} else {
		return strings.ReplaceAll(subject, search, replace)
	}
}

func (v *Visitor) loadImportedTypeAliases(projectImport string) {
	packageInfoMap := getImportPackageInfo([]string{projectImport}, v.options)

	for _, info := range packageInfoMap {
		// Load imported type aliases for the target project import, if not already loaded
		loadImportedTypeAliases(info)
	}
}

func loadImportedTypeAliases(info PackageInfo) {
	packageInfoFile := filepath.Join(info.TargetDir, PackageInfoFileName)

	packageLock.Lock()

	// Check if this package info file has already been parsed
	if _, ok := parsedPackageInfoFiles[packageInfoFile]; ok {
		packageLock.Unlock()
		return
	}

	parsedPackageInfoFiles.Add(packageInfoFile)
	packageLock.Unlock()

	// Ignore imports if the package info file does not exist
	if _, err := os.Stat(packageInfoFile); os.IsNotExist(err) {
		return
	}

	// Parse package info file for exported type aliases, these are used
	// as the imported type aliases in the current package
	results, err := parseExportedTypeAliases(packageInfoFile)

	if err == nil {
		rootPackageName := getSanitizedIdentifier(info.RootPackageName)
		packageName := getCoreSanitizedIdentifier(info.PackageName)

		for _, result := range results {
			// Add the exported type alias to the imported type aliases map
			alias := fmt.Sprintf("%s.%s", rootPackageName, getCoreSanitizedIdentifier(result[0]))
			typeName := getCoreSanitizedIdentifier(result[1])

			if strings.HasPrefix(typeName, "const:") {
				typeName = strings.TrimPrefix(typeName, "const:")

				packageLock.Lock()
				constImportedTypeAliases.Add(alias)
				packageLock.Unlock()
			} else if !strings.HasPrefix(typeName, RootNamespace) {
				typeName = fmt.Sprintf("%s.%s%s.%s", RootNamespace, packageName, PackageSuffix, typeName)
			}

			packageLock.Lock()
			importedTypeAliases[alias] = typeName
			packageLock.Unlock()
		}

		// Record the imported package's POINTER-sourced GoImplement pairs: their generated
		// adapter classes (TжIface) are public members of the foreign package class, so a
		// cross-package pointer-to-interface conversion here can reference them by qualified
		// name (io/fs's PathErrorжerror consumed by os - CS0029 x38).
		if pairs, err := parseExportedPointerImplements(packageInfoFile); err == nil {
			rootPackageName := getSanitizedIdentifier(info.RootPackageName)

			packageLock.Lock()

			for _, pair := range pairs {
				importedPointerImplements.Add(fmt.Sprintf("%s|%s|%s", rootPackageName, pair[0], canonicalRecordIfaceName(pair[1], info.RootPackageName)))
			}

			packageLock.Unlock()
		}

		// Record the imported package's VALUE-form GoImplement pairs (plain or Promoted): the
		// foreign struct's own assembly implements the interface, so a value cast here converts
		// implicitly and needs no local adapter (see the both-foreign value arm in
		// convertToInterfaceType).
		if pairs, err := parseExportedValueImplements(packageInfoFile); err == nil {
			rootPackageName := getSanitizedIdentifier(info.RootPackageName)

			packageLock.Lock()

			for _, pair := range pairs {
				importedValueImplements.Add(fmt.Sprintf("%s|%s|%s", rootPackageName, pair[0], canonicalRecordIfaceName(pair[1], info.RootPackageName)))
			}

			packageLock.Unlock()
		}
	} else {
		showWarning("Failed to parse exported type aliases from package info file \"%s\": %s", packageInfoFile, err)
	}
}

// parseExportedTypeAliases parses a package info file and extracts the GoTypeAlias
// entries as tuples of (source, destination) strings
func parseExportedTypeAliases(packageInfoFile string) ([][2]string, error) {
	file, err := os.Open(packageInfoFile)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	scanner := bufio.NewScanner(file)

	// Look for the start of the ExportedTypeAliases section
	inSection := false
	var aliases [][2]string

	// Pattern to match: [assembly: GoTypeAlias("Source", "Destination")]
	pattern := regexp.MustCompile(`\[assembly: GoTypeAlias\("([^"]+)", "([^"]+)"\)\]`)

	for scanner.Scan() {
		line := scanner.Text()

		if strings.TrimSpace(line) == "// <ExportedTypeAliases>" {
			inSection = true
			continue
		}

		if strings.TrimSpace(line) == "// </ExportedTypeAliases>" {
			break // End of section reached
		}

		if inSection {
			matches := pattern.FindStringSubmatch(line)

			if len(matches) == 3 {
				// Extract the source and destination as tuple
				alias := [2]string{matches[1], matches[2]}
				aliases = append(aliases, alias)
			}
		}
	}

	if err := scanner.Err(); err != nil {
		return nil, err
	}

	if !inSection && len(aliases) == 0 {
		return nil, errors.New("exported type aliases section not found")
	}

	return aliases, nil
}

// canonicalRecordIfaceName reduces a GoImplement record's INTERFACE side to the canonical
// qualified form the cast-site keys use: the RootNamespace prefix is stripped and a DOTLESS
// name (an interface local to the recording package, or the universe `error`) is qualified
// with the recording package's class. The SIMPLE name alone is ambiguous — image's
// Paletted→image.Image record must not satisfy a Paletted→draw.Image cast (both reduce to
// "Image"; image/draw referenced the foreign adapter implementing the WRONG interface,
// CS1503).
func canonicalRecordIfaceName(ifaceName string, rootPackageName string) string {
	ifaceName = strings.TrimPrefix(ifaceName, RootNamespace+".")

	if !strings.Contains(ifaceName, ".") && ifaceName != "error" {
		ifaceName = getSanitizedIdentifier(rootPackageName) + PackageSuffix + "." + ifaceName
	}

	return ifaceName
}

// parseExportedPointerImplements parses a package info file for `GoImplement<T, Iface>(Pointer
// = true)` assembly attributes, returning (T-simple, Iface-qualified) pairs - the adapter-class
// existence records for cross-package pointer-to-interface conversions.
func parseExportedPointerImplements(packageInfoFile string) ([][2]string, error) {
	file, err := os.Open(packageInfoFile)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	scanner := bufio.NewScanner(file)

	var pairs [][2]string

	pattern := regexp.MustCompile(`\[assembly: GoImplement<(.+), (.+)>\(Pointer = true\)\]`)

	for scanner.Scan() {
		matches := pattern.FindStringSubmatch(scanner.Text())

		if matches == nil {
			continue
		}

		// The struct side reduces to its SIMPLE (last-dot-segment) name - the adapter class
		// name composes from exactly that (see adapterTypeRef / the ImplementGenerator). The
		// INTERFACE side keeps its qualifier (canonicalized at the populate site).
		tName := matches[1]

		if idx := strings.LastIndex(tName, "."); idx >= 0 {
			tName = tName[idx+1:]
		}

		pairs = append(pairs, [2]string{tName, matches[2]})
	}

	return pairs, scanner.Err()
}

// parseExportedValueImplements parses a package info file for VALUE-form `GoImplement<T, Iface>`
// assembly attributes (plain or `(Promoted = true)`), returning (T-simple, Iface-simple) pairs -
// records that the defining assembly itself implements the interface on the value type.
func parseExportedValueImplements(packageInfoFile string) ([][2]string, error) {
	file, err := os.Open(packageInfoFile)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	scanner := bufio.NewScanner(file)

	var pairs [][2]string

	pattern := regexp.MustCompile(`\[assembly: GoImplement<(.+), (.+)>(?:\(Promoted = true\))?\]`)

	for scanner.Scan() {
		matches := pattern.FindStringSubmatch(scanner.Text())

		if matches == nil {
			continue
		}

		// A Pointer-form record is the ADAPTER existence signal, not a value implementation.
		if strings.Contains(scanner.Text(), "(Pointer = true)") {
			continue
		}

		// The struct side reduces to its SIMPLE (last-dot-segment) name; the INTERFACE side
		// keeps its qualifier (canonicalized at the populate site — the simple name collides
		// across same-named interfaces, see canonicalRecordIfaceName).
		tName := matches[1]

		if idx := strings.LastIndex(tName, "."); idx >= 0 {
			tName = tName[idx+1:]
		}

		pairs = append(pairs, [2]string{tName, matches[2]})
	}

	return pairs, scanner.Err()
}
