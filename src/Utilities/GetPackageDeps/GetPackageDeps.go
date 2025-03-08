package main

import (
	"fmt"
	"os/exec"
	"path/filepath"
	"strings"
)

// PackageInfo represents the structure returned by "go list -json"
type PackageInfo struct {
	ImportPath string
	Dir        string
	Imports    []string
	ImportMap  map[string]string
}

// GetImportPaths returns a map where key is the import path and value is the full file system path
func GetImportPaths(packagePath string) (map[string]string, error) {
	// First, get the direct imports of the package
	cmd := exec.Command("go", "list", "-f", "{{.Imports}}", packagePath)
	output, err := cmd.Output()
	if err != nil {
		return nil, fmt.Errorf("failed to get imports: %w", err)
	}

	// Parse the import list (format: [import1 import2 ...])
	imports := string(output)
	imports = strings.TrimSpace(imports)
	imports = strings.TrimPrefix(imports, "[")
	imports = strings.TrimSuffix(imports, "]")
	
	// Split the string by spaces
	importList := []string{}
	if imports != "" {
		importList = strings.Fields(imports)
	}

	// Create result map
	result := make(map[string]string)

	// For each import, get its directory in a single batch command
	for _, imp := range importList {
		dirCmd := exec.Command("go", "list", "-f", "{{.Dir}}", imp)
		dirOutput, err := dirCmd.Output()
		if err != nil {
			// Skip standard library packages that might not have a directory
			continue
		}
		
		// Add to result map (trim newline from output)
		dir := strings.TrimSpace(string(dirOutput))
		result[imp] = filepath.Clean(dir)
	}

	return result, nil
}

// Example usage
func main() {
	// Get imports for the current directory
	importPaths, err := GetImportPaths(".")
	if err != nil {
		fmt.Printf("Error: %v\n", err)
		return
	}

	// Print result
	fmt.Println("Import paths:")
	for imp, path := range importPaths {
		fmt.Printf("%s -> %s\n", imp, path)
	}
}