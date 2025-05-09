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

		// Handle error, e.g., package not found
		if err != nil {
			result[importPath] = PackageInfo{Err: err}
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

		// TODO: Check if the import path is a local package and handle it accordingly
		//if build.IsLocalImport(importPath) {

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
