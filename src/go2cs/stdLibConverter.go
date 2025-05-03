// This file contains extensions to the go2cs project for handling standard library conversion
package main

import (
	"fmt"
	"log"
	"os"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"

	"golang.org/x/tools/go/packages"
)

// Package represents a Go package with its dependencies
type Package struct {
	Path         string   // Import path
	Dir          string   // Directory path
	Dependencies []string // Dependencies (import paths)
	Dependents   []string // Packages that depend on this one
	Processed    bool     // Whether package has been processed
}

// StdLibConverter manages the conversion of the Go standard library
type StdLibConverter struct {
	goRoot      string              // Path to Go root directory
	goPath      string              // Path to Go path directory
	go2csPath   string              // Path to output directory
	options     Options             // Conversion options
	packages    map[string]*Package // Map of package paths to Package objects
	sortedQueue []string            // Topologically sorted queue
	mutex       sync.Mutex          // Mutex to protect concurrent map access
	startTime   time.Time           // Start time for reporting progress
	totalCount  int                 // Total number of packages
}

// NewStdLibConverter creates a new standard library converter
func NewStdLibConverter(options Options) *StdLibConverter {
	return &StdLibConverter{
		goRoot:    options.goRoot,
		goPath:    options.goPath,
		go2csPath: options.go2csPath,
		options:   options,
		packages:  make(map[string]*Package),
		startTime: time.Now(),
	}
}

// ScanAndConvert scans all standard library packages and converts them
func (c *StdLibConverter) ScanAndConvert() error {
	return c.ScanAndConvertFiltered(nil)
}

// ScanAndConvertFiltered scans and converts only specific packages if a filter is provided
func (c *StdLibConverter) ScanAndConvertFiltered(packageFilter []string) error {
	// Step 1: Scan all standard library packages
	fmt.Println("Scanning Go standard library packages...")
	if err := c.scanStdLib(); err != nil {
		return fmt.Errorf("failed to scan standard library: %w", err)
	}

	// Step 2: Build dependency graph and sort
	fmt.Println("Building dependency graph...")
	if err := c.buildDependencyGraph(); err != nil {
		return fmt.Errorf("failed to build dependency graph: %w", err)
	}

	// Generate dependency graph visualization
	if err := c.GenerateDependencyGraph(); err != nil {
		fmt.Printf("WARNING: Failed to generate dependency graph: %v\n", err)
	}

	// Step 3: Perform topological sort
	fmt.Println("Sorting packages by dependencies...")
	if err := c.topologicalSort(); err != nil {
		return fmt.Errorf("failed to sort packages: %w", err)
	}

	// Generate conversion report
	if err := c.GenerateConversionReport(); err != nil {
		fmt.Printf("WARNING: Failed to generate conversion report: %v\n", err)
	}

	// Apply filter if provided
	if len(packageFilter) > 0 {
		// Create a map for O(1) lookups
		filterMap := make(map[string]bool)
		for _, pkg := range packageFilter {
			filterMap[pkg] = true
		}

		// Filter the queue
		filteredQueue := make([]string, 0)
		for _, pkg := range c.sortedQueue {
			if filterMap[pkg] {
				filteredQueue = append(filteredQueue, pkg)
			}
		}

		if len(filteredQueue) == 0 {
			return fmt.Errorf("no matching packages found after filtering")
		}

		fmt.Printf("Filtered from %d to %d packages based on filter list\n",
			len(c.sortedQueue), len(filteredQueue))
		c.sortedQueue = filteredQueue
	}

	// Check for previously converted packages
	if err := c.checkForPreviousConversion(); err != nil {
		fmt.Printf("WARNING: Failed to check for previously converted packages: %v\n", err)
	}

	// Step 4: Convert all packages
	c.totalCount = len(c.sortedQueue)
	fmt.Printf("Beginning conversion of %d packages...\n", c.totalCount)
	if err := c.convertAllPackages(); err != nil {
		return fmt.Errorf("error during package conversion: %w", err)
	}

	return nil
}

// scanStdLib scans all standard library packages
func (c *StdLibConverter) scanStdLib() error {
	// Scan the src directory in GOROOT
	srcPath := filepath.Join(c.goRoot, "src")

	// Check if the directory exists
	if _, err := os.Stat(srcPath); os.IsNotExist(err) {
		return fmt.Errorf("standard library source not found at %s", srcPath)
	}

	// First, get the list of standard library packages
	loadConfig := &packages.Config{
		Mode: packages.NeedName | packages.NeedImports | packages.NeedDeps | packages.NeedFiles,
		Dir:  srcPath,
		Env:  append(os.Environ(), "GO111MODULE=off"), // Disable module mode
	}

	// Set the target platform in the environment if specified
	if c.options.targetPlatform != "" {
		targetParts := strings.Split(c.options.targetPlatform, "/")
		if len(targetParts) == 2 {
			loadConfig.Env = append(loadConfig.Env, fmt.Sprintf("GOOS=%s", targetParts[0]))
			loadConfig.Env = append(loadConfig.Env, fmt.Sprintf("GOARCH=%s", targetParts[1]))
		}
	}

	fmt.Println("Loading standard library packages (this may take a while)...")
	// Load "std" pattern to get all standard library packages
	pkgs, err := packages.Load(loadConfig, "std")
	if err != nil {
		return fmt.Errorf("failed to load standard library packages: %w", err)
	}

	fmt.Printf("Found %d standard library packages\n", len(pkgs))

	// Process each package
	for _, pkg := range pkgs {
		// Skip test packages
		if strings.HasSuffix(pkg.PkgPath, "_test") {
			continue
		}

		// Skip certain special packages that don't need conversion
		if pkg.PkgPath == "unsafe" || pkg.PkgPath == "builtin" ||
			pkg.PkgPath == "cmd" || strings.HasPrefix(pkg.PkgPath, "cmd/") {
			continue
		}

		// Create new package object
		c.mutex.Lock()
		c.packages[pkg.PkgPath] = &Package{
			Path:         pkg.PkgPath,
			Dir:          pkg.Dir,
			Dependencies: make([]string, 0),
			Dependents:   make([]string, 0),
			Processed:    false,
		}
		c.mutex.Unlock()
	}

	return nil
}

// buildDependencyGraph builds the dependency graph of packages
func (c *StdLibConverter) buildDependencyGraph() error {
	fmt.Println("Building dependency graph for all packages...")

	// Count to track progress
	total := len(c.packages)
	count := 0

	// For each package, find all dependencies
	for pkgPath, pkg := range c.packages {
		count++
		if count%20 == 0 || count == total {
			fmt.Printf("\nAnalyzing dependencies: %d/%d packages (%.1f%%)...", count, total, float64(count)/float64(total)*100)
		}

		// Load the package with dependencies
		loadConfig := &packages.Config{
			Mode: packages.NeedImports,
			Dir:  pkg.Dir,
			Env:  append(os.Environ(), "GO111MODULE=off"), // Disable module mode
		}

		// Set the target platform in the environment if specified
		if c.options.targetPlatform != "" {
			targetParts := strings.Split(c.options.targetPlatform, "/")
			if len(targetParts) == 2 {
				loadConfig.Env = append(loadConfig.Env, fmt.Sprintf("GOOS=%s", targetParts[0]))
				loadConfig.Env = append(loadConfig.Env, fmt.Sprintf("GOARCH=%s", targetParts[1]))
			}
		}

		pkgs, err := packages.Load(loadConfig, pkgPath)
		if err != nil {
			// Some packages might fail to load due to build constraints
			// Just log the error and continue
			log.Printf("WARNING: Failed to load package %s: %v", pkgPath, err)
			continue
		}

		if len(pkgs) == 0 {
			// Some packages might not be found due to build constraints
			// Just log the warning and continue
			log.Printf("WARNING: Failed to find package %s", pkgPath)
			continue
		}

		// Get imports
		for importPath := range pkgs[0].Imports {
			// Only include standard library packages
			if c.isStdLib(importPath) {
				c.mutex.Lock()
				// Add dependency if not already added
				if !containsString(pkg.Dependencies, importPath) {
					pkg.Dependencies = append(pkg.Dependencies, importPath)
				}

				// Add this package as a dependent to the dependency
				if depPkg, exists := c.packages[importPath]; exists {
					if !containsString(depPkg.Dependents, pkgPath) {
						depPkg.Dependents = append(depPkg.Dependents, pkgPath)
					}
				}
				c.mutex.Unlock()
			}
		}
	}

	fmt.Println("\nDependency analysis complete!")

	// Sort dependencies and dependents for deterministic behavior
	for _, pkg := range c.packages {
		sort.Strings(pkg.Dependencies)
		sort.Strings(pkg.Dependents)
	}

	return nil
}

// containsString checks if a string slice contains a specific string
func containsString(slice []string, str string) bool {
	for _, s := range slice {
		if s == str {
			return true
		}
	}
	return false
}

// isStdLib checks if a package is part of the standard library
func (c *StdLibConverter) isStdLib(pkgPath string) bool {
	// Standard library packages don't have a domain prefix
	if strings.Contains(pkgPath, ".") {
		return false
	}

	// Check if it's in our package map
	_, exists := c.packages[pkgPath]
	return exists
}

// topologicalSort performs a topological sort of the packages
func (c *StdLibConverter) topologicalSort() error {
	fmt.Println("Sorting packages in dependency order...")

	// Create a copy of the package map for sorting
	unprocessed := make(map[string]*Package)
	for path, pkg := range c.packages {
		unprocessed[path] = &Package{
			Path:         pkg.Path,
			Dependencies: pkg.Dependencies,
			Processed:    false,
		}
	}

	// Create the sorted queue
	c.sortedQueue = make([]string, 0, len(c.packages))

	// First handle packages with no dependencies
	var noDeps []string
	for path, pkg := range unprocessed {
		if len(pkg.Dependencies) == 0 && !pkg.Processed {
			noDeps = append(noDeps, path)
		}
	}

	// Sort for deterministic order
	sort.Strings(noDeps)

	// Process packages with no dependencies first
	for _, path := range noDeps {
		if !unprocessed[path].Processed {
			unprocessed[path].Processed = true
			c.sortedQueue = append(c.sortedQueue, path)
		}
	}

	// Keep track of packages being processed (to detect cycles)
	processing := make(map[string]bool)

	// Then recursively process remaining packages
	for path := range unprocessed {
		if !unprocessed[path].Processed {
			if err := c.visitPackage(path, unprocessed, processing); err != nil {
				return err
			}
		}
	}

	// Print some statistics
	fmt.Printf("Sorted %d packages in dependency order\n", len(c.sortedQueue))

	// For debugging: print first 5 and last 5 packages in the sorted queue
	if len(c.sortedQueue) > 10 {
		fmt.Println("First packages to convert:")
		for i := 0; i < 5; i++ {
			fmt.Printf("  %s\n", c.sortedQueue[i])
		}
		fmt.Println("Last packages to convert:")
		for i := len(c.sortedQueue) - 5; i < len(c.sortedQueue); i++ {
			fmt.Printf("  %s\n", c.sortedQueue[i])
		}
	}

	return nil
}

// visitPackage visits a package during topological sort
func (c *StdLibConverter) visitPackage(path string, unprocessed map[string]*Package, processing map[string]bool) error {
	// Check for cycles
	if processing[path] {
		// We found a cycle, but it might be okay for Go packages
		// Just log it and continue
		log.Printf("WARNING: Cycle detected involving package %s", path)
		return nil
	}

	// Mark as being processed
	processing[path] = true

	// Process dependencies first
	for _, depPath := range unprocessed[path].Dependencies {
		if dep, exists := unprocessed[depPath]; exists && !dep.Processed {
			if err := c.visitPackage(depPath, unprocessed, processing); err != nil {
				return err
			}
		}
	}

	// Mark as processed and add to the queue
	unprocessed[path].Processed = true
	c.sortedQueue = append(c.sortedQueue, path)

	// Unmark as being processed
	processing[path] = false

	return nil
}

// checkForPreviousConversion checks if a previous conversion attempt was made
func (c *StdLibConverter) checkForPreviousConversion() error {
	// Look for the progress file
	progressFile := filepath.Join(c.go2csPath, "stdlib_conversion_progress.txt")

	if _, err := os.Stat(progressFile); os.IsNotExist(err) {
		// No previous conversion
		return nil
	}

	// Look for the specific failed packages file
	failedFile := filepath.Join(c.go2csPath, "failed_packages.txt")

	if _, err := os.Stat(failedFile); os.IsNotExist(err) {
		// No failed packages file
		fmt.Println("Found previous conversion attempt, but no failed packages file.")
		return nil
	}

	// Read the failed packages file
	data, err := os.ReadFile(failedFile)
	if err != nil {
		return fmt.Errorf("failed to read previous conversion file: %w", err)
	}

	// Parse the failed packages
	lines := strings.Split(string(data), "\n")
	var failedPackages []string

	for _, line := range lines {
		line = strings.TrimSpace(line)
		if line != "" {
			failedPackages = append(failedPackages, line)
		}
	}

	if len(failedPackages) == 0 {
		fmt.Println("Found previous conversion attempt, but no failed packages.")
		return nil
	}

	fmt.Printf("Found previous conversion attempt with %d failed packages.\n", len(failedPackages))

	// Ask the user if they want to resume
	fmt.Print("Do you want to resume converting only the failed packages? (y/n): ")
	var answer string
	fmt.Scanln(&answer)

	if strings.ToLower(answer) == "y" || strings.ToLower(answer) == "yes" {
		// Filter the queue to only include the failed packages
		fmt.Printf("Resuming conversion of %d previously failed packages.\n", len(failedPackages))

		// Create a map for O(1) lookups
		filterMap := make(map[string]bool)
		for _, pkg := range failedPackages {
			filterMap[pkg] = true
		}

		// Filter the queue
		filteredQueue := make([]string, 0)
		for _, pkg := range c.sortedQueue {
			if filterMap[pkg] {
				filteredQueue = append(filteredQueue, pkg)
			}
		}

		if len(filteredQueue) == 0 {
			return fmt.Errorf("no matching packages found after filtering for failed packages")
		}

		fmt.Printf("Filtered from %d to %d packages based on previously failed packages\n",
			len(c.sortedQueue), len(filteredQueue))
		c.sortedQueue = filteredQueue
	} else {
		fmt.Println("Proceeding with full conversion...")
	}

	return nil
}

// convertAllPackages converts all packages in the sorted queue
func (c *StdLibConverter) convertAllPackages() error {
	successCount := 0
	failedPackages := make([]string, 0)

	// Create a benchmark file to track conversion progress
	benchmarkFile := filepath.Join(c.go2csPath, "stdlib_conversion_progress.txt")
	benchmark, err := os.Create(benchmarkFile)
	if err != nil {
		log.Printf("WARNING: Could not create benchmark file: %v", err)
	} else {
		defer benchmark.Close()
		fmt.Fprintf(benchmark, "Go Standard Library Conversion - Started %s\n\n", c.startTime.Format(time.RFC3339))
		fmt.Fprintf(benchmark, "%-45s %-15s %-15s %s\n", "Package", "Status", "Duration", "Timestamp")
		fmt.Fprintf(benchmark, "%s\n", strings.Repeat("-", 100))
	}

	// Determine concurrency level - use 1 for sequential processing for stability,
	// but allow up to 4 packages to be processed concurrently if the environment
	// variable GO2CS_PARALLEL is set to a number.
	concurrentLimit := 1

	if concurrentEnv := os.Getenv("GO2CS_PARALLEL"); concurrentEnv != "" {
		if limit, err := strconv.Atoi(concurrentEnv); err == nil && limit > 0 {
			concurrentLimit = limit
			if concurrentLimit > 4 {
				// Cap at 4 for stability
				concurrentLimit = 4
			}
			fmt.Printf("Using parallel conversion with %d concurrent packages\n", concurrentLimit)
		}
	}

	// TODO: Move package variables in main.go into a struct to avoid global state
	if concurrentLimit > 1 {
		fmt.Printf("WARNING: Running in package conversion in parallel mode is currently not supported -- resetting concurrency limit to 1\n")
		concurrentLimit = 1
	}

	// For parallel processing we need a mutex to protect the benchmark file and counters
	var resultMutex sync.Mutex

	// Create a semaphore to limit concurrency
	semaphore := make(chan struct{}, concurrentLimit)
	var wg sync.WaitGroup

	// Process each package in the queue
	for i, pkgPath := range c.sortedQueue {
		// Acquire semaphore slot
		semaphore <- struct{}{}
		wg.Add(1)

		// Launch package conversion in a goroutine if parallel mode is enabled
		go func(i int, pkgPath string) {
			defer func() {
				// Release semaphore slot
				<-semaphore
				wg.Done()
			}()

			pkg := c.packages[pkgPath]
			pkgStartTime := time.Now()

			resultMutex.Lock()

			// Calculate progress
			progress := float64(i+1) / float64(c.totalCount) * 100
			elapsed := time.Since(c.startTime)

			var totalEstimated time.Duration
			var remaining time.Duration

			if i > 0 {
				totalEstimated = time.Duration(float64(elapsed) / float64(i) * float64(c.totalCount))
				remaining = totalEstimated - elapsed
			}

			fmt.Printf("\n[%d/%d] Converting package %s (%.1f%% complete", i+1, c.totalCount, pkgPath, progress)

			if i > 0 {
				fmt.Printf(", ~%s remaining", formatDuration(remaining))
			}

			fmt.Println(")")

			// Handle dependencies
			if len(pkg.Dependencies) > 0 {
				fmt.Printf("  Dependencies (%d): ", len(pkg.Dependencies))
				if len(pkg.Dependencies) > 5 {
					// Show just the first 5 for brevity
					fmt.Printf("%s... and %d more\n", strings.Join(pkg.Dependencies[:5], ", "),
						len(pkg.Dependencies)-5)
				} else {
					fmt.Printf("%s\n", strings.Join(pkg.Dependencies, ", "))
				}
			}
			resultMutex.Unlock()

			// Create output path
			outputPath := c.getOutputPath(pkgPath)

			// Process the conversion with error handling
			var conversionErr error

			func() {
				// Use a panic recovery to catch any unexpected errors
				defer func() {
					if r := recover(); r != nil {
						conversionErr = fmt.Errorf("panic during conversion: %v", r)
					}
				}()

				conversionErr = c.convertPackage(pkg.Dir, outputPath)
			}()

			conversionDuration := time.Since(pkgStartTime)

			resultMutex.Lock()
			defer resultMutex.Unlock()

			if conversionErr != nil {
				msg := fmt.Sprintf("failed to convert package %s: %v", pkgPath, conversionErr)
				log.Println(msg)
				failedPackages = append(failedPackages, pkgPath)

				if benchmark != nil {
					fmt.Fprintf(benchmark, "%-40s %-15s %-15s %s\n",
						pkgPath, "FAILED", formatDuration(conversionDuration),
						time.Now().Format(time.RFC3339))
				}
			} else {
				successCount++
				fmt.Printf("  Completed in %s\n", formatDuration(conversionDuration))

				if benchmark != nil {
					fmt.Fprintf(benchmark, "%-45s %-15s %-15s %s\n",
						pkgPath, "SUCCESS", formatDuration(conversionDuration),
						time.Now().Format(time.RFC3339))
				}
			}
		}(i, pkgPath)

		// If not running in parallel mode, wait for each package to complete
		// before processing the next one
		if concurrentLimit == 1 {
			wg.Wait()
		}
	}

	// Wait for all goroutines to complete
	wg.Wait()

	// Print final statistics
	elapsed := time.Since(c.startTime)
	fmt.Printf("\nConversion Summary:\n")
	fmt.Printf("Total packages: %d\n", c.totalCount)
	fmt.Printf("Successfully converted: %d (%.1f%%)\n", successCount, float64(successCount)/float64(c.totalCount)*100)
	fmt.Printf("Failed: %d (%.1f%%)\n", len(failedPackages), float64(len(failedPackages))/float64(c.totalCount)*100)
	fmt.Printf("Total time: %s\n", formatDuration(elapsed))

	if successCount > 0 {
		fmt.Printf("Average time per successful package: %s\n", formatDuration(elapsed/time.Duration(successCount)))
	}

	// Write completion info to benchmark file
	if benchmark != nil {
		fmt.Fprintf(benchmark, "\nSummary:\n")
		fmt.Fprintf(benchmark, "Total packages: %d\n", c.totalCount)
		fmt.Fprintf(benchmark, "Successfully converted: %d (%.1f%%)\n", successCount, float64(successCount)/float64(c.totalCount)*100)
		fmt.Fprintf(benchmark, "Failed: %d (%.1f%%)\n", len(failedPackages), float64(len(failedPackages))/float64(c.totalCount)*100)
		fmt.Fprintf(benchmark, "Total time: %s\n", formatDuration(elapsed))

		if len(failedPackages) > 0 {
			fmt.Fprintf(benchmark, "\nFailed packages:\n")
			for _, pkg := range failedPackages {
				fmt.Fprintf(benchmark, "  %s\n", pkg)
			}
		}
	}

	// If any packages failed, create a resume file with just the failed packages
	if len(failedPackages) > 0 {
		resumeFile := filepath.Join(c.go2csPath, "failed_packages.txt")
		if file, err := os.Create(resumeFile); err == nil {
			defer file.Close()
			for _, pkg := range failedPackages {
				fmt.Fprintln(file, pkg)
			}
			fmt.Printf("\nList of failed packages written to: %s\n", resumeFile)
		}
	}

	return nil
}

// getOutputPath determines the output path for a package
func (c *StdLibConverter) getOutputPath(pkgPath string) string {
	// For standard library packages, maintain the same structure under go2cspath/core
	return filepath.Join(c.go2csPath, "core", pkgPath)
}

// convertPackage converts a single package
func (c *StdLibConverter) convertPackage(inputPath, outputPath string) error {
	// Use a channel to synchronize completion
	done := make(chan error, 1)

	go func() {
		// Run the conversion in a goroutine to allow for timeout handling
		defer func() {
			if r := recover(); r != nil {
				done <- fmt.Errorf("panic in package conversion: %v", r)
			}
		}()

		// Call the existing processConversion function
		processConversion(inputPath, true, outputPath, c.options)
		done <- nil
	}()

	// Set a timeout for package conversion (10 minutes should be plenty)
	select {
	case err := <-done:
		return err
	case <-time.After(10 * time.Minute):
		return fmt.Errorf("conversion timed out after 10 minutes")
	}
}

// GenerateDependencyGraph creates a visual representation of the dependency graph
func (c *StdLibConverter) GenerateDependencyGraph() error {
	fmt.Println("Generating dependency graph visualization...")

	// First, ensure graphviz directory exists
	graphDir := filepath.Join(c.go2csPath, "graphs")
	if err := os.MkdirAll(graphDir, 0755); err != nil {
		return fmt.Errorf("failed to create graphs directory: %w", err)
	}

	// Create a DOT file for the dependency graph
	dotFile := filepath.Join(graphDir, "package_dependencies.dot")
	f, err := os.Create(dotFile)
	if err != nil {
		return fmt.Errorf("failed to create DOT file: %w", err)
	}
	defer f.Close()

	// Write DOT file header
	fmt.Fprintln(f, "digraph G {")
	fmt.Fprintln(f, "  rankdir=LR;")
	fmt.Fprintln(f, "  node [shape=box, style=filled, fillcolor=lightblue];")

	// Write nodes for each package
	for pkgPath := range c.packages {
		// Sanitize package name for DOT
		pkgName := strings.ReplaceAll(pkgPath, "/", "_")
		pkgName = strings.ReplaceAll(pkgName, ".", "_")
		fmt.Fprintf(f, "  \"%s\" [label=\"%s\"];\n", pkgName, pkgPath)
	}

	// Write edges for dependencies
	for pkgPath, pkg := range c.packages {
		// Sanitize package name for DOT
		pkgName := strings.ReplaceAll(pkgPath, "/", "_")
		pkgName = strings.ReplaceAll(pkgName, ".", "_")

		for _, dep := range pkg.Dependencies {
			// Sanitize dependency name for DOT
			depName := strings.ReplaceAll(dep, "/", "_")
			depName = strings.ReplaceAll(depName, ".", "_")

			fmt.Fprintf(f, "  \"%s\" -> \"%s\";\n", pkgName, depName)
		}
	}

	// Write DOT file footer
	fmt.Fprintln(f, "}")

	fmt.Printf("Dependency graph generated: %s\n", dotFile)
	fmt.Println("You can visualize this with Graphviz by running:")
	fmt.Printf("  dot -Tpng %s -o %s\n", dotFile, filepath.Join(graphDir, "package_dependencies.png"))

	return nil
}

// GenerateConversionReport creates a detailed report of the conversion process
func (c *StdLibConverter) GenerateConversionReport() error {
	fmt.Println("Generating conversion report...")

	// Ensure reports directory exists
	reportDir := filepath.Join(c.go2csPath, "reports")
	if err := os.MkdirAll(reportDir, 0755); err != nil {
		return fmt.Errorf("failed to create reports directory: %w", err)
	}

	// Create report file
	reportFile := filepath.Join(reportDir, "conversion_report.html")
	f, err := os.Create(reportFile)
	if err != nil {
		return fmt.Errorf("failed to create report file: %w", err)
	}
	defer f.Close()

	// Write HTML header
	fmt.Fprintln(f, "<!DOCTYPE html>")
	fmt.Fprintln(f, "<html>")
	fmt.Fprintln(f, "<head>")
	fmt.Fprintln(f, "  <title>Go Standard Library Conversion Report</title>")
	fmt.Fprintln(f, "  <style>")
	fmt.Fprintln(f, "    body { font-family: Arial, sans-serif; margin: 20px; }")
	fmt.Fprintln(f, "    h1 { color: #0066cc; }")
	fmt.Fprintln(f, "    table { border-collapse: collapse; width: 100%; }")
	fmt.Fprintln(f, "    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }")
	fmt.Fprintln(f, "    tr:nth-child(even) { background-color: #f2f2f2; }")
	fmt.Fprintln(f, "    th { background-color: #0066cc; color: white; }")
	fmt.Fprintln(f, "    .success { color: green; }")
	fmt.Fprintln(f, "    .failure { color: red; }")
	fmt.Fprintln(f, "  </style>")
	fmt.Fprintln(f, "</head>")
	fmt.Fprintln(f, "<body>")

	// Write report header
	fmt.Fprintln(f, "  <h1>Go Standard Library Conversion Report</h1>")
	fmt.Fprintf(f, "  <p>Generated: %s</p>\n", time.Now().Format(time.RFC1123))
	fmt.Fprintf(f, "  <p>Target platform: %s</p>\n", c.options.targetPlatform)

	// Write package summary
	fmt.Fprintln(f, "  <h2>Package Summary</h2>")
	fmt.Fprintln(f, "  <ul>")
	fmt.Fprintf(f, "    <li>Total packages: %d</li>\n", len(c.packages))
	fmt.Fprintf(f, "    <li>Packages in conversion queue: %d</li>\n", len(c.sortedQueue))
	fmt.Fprintln(f, "  </ul>")

	// Write package table
	fmt.Fprintln(f, "  <h2>Package Details</h2>")
	fmt.Fprintln(f, "  <table>")
	fmt.Fprintln(f, "    <tr>")
	fmt.Fprintln(f, "      <th>Package</th>")
	fmt.Fprintln(f, "      <th>Dependencies</th>")
	fmt.Fprintln(f, "      <th>Dependents</th>")
	fmt.Fprintln(f, "    </tr>")

	// Sort packages by name for consistent output
	sortedPackages := make([]string, 0, len(c.packages))
	for pkgPath := range c.packages {
		sortedPackages = append(sortedPackages, pkgPath)
	}
	sort.Strings(sortedPackages)

	// Add a row for each package
	for _, pkgPath := range sortedPackages {
		pkg := c.packages[pkgPath]
		fmt.Fprintln(f, "    <tr>")
		fmt.Fprintf(f, "      <td>%s</td>\n", pkgPath)
		fmt.Fprintf(f, "      <td>%d</td>\n", len(pkg.Dependencies))
		fmt.Fprintf(f, "      <td>%d</td>\n", len(pkg.Dependents))
		fmt.Fprintln(f, "    </tr>")
	}

	fmt.Fprintln(f, "  </table>")

	// Write information about the conversion order
	fmt.Fprintln(f, "  <h2>Conversion Order</h2>")
	fmt.Fprintln(f, "  <p>Packages will be converted in the following order (based on dependency analysis):</p>")
	fmt.Fprintln(f, "  <ol>")

	// List the first 20 packages and last 20 packages
	firstN := 20
	lastN := 20

	if len(c.sortedQueue) <= firstN+lastN {
		// If we have fewer than firstN+lastN packages, just list them all
		for _, pkgPath := range c.sortedQueue {
			fmt.Fprintf(f, "    <li>%s</li>\n", pkgPath)
		}
	} else {
		// List the first firstN packages
		for i := 0; i < firstN && i < len(c.sortedQueue); i++ {
			fmt.Fprintf(f, "    <li>%s</li>\n", c.sortedQueue[i])
		}

		fmt.Fprintln(f, "    <li>... (and more) ...</li>")

		// List the last lastN packages
		for i := len(c.sortedQueue) - lastN; i < len(c.sortedQueue); i++ {
			fmt.Fprintf(f, "    <li>%s</li>\n", c.sortedQueue[i])
		}
	}

	fmt.Fprintln(f, "  </ol>")

	// Write HTML footer
	fmt.Fprintln(f, "</body>")
	fmt.Fprintln(f, "</html>")

	fmt.Printf("Conversion report generated: %s\n", reportFile)

	return nil
}

// formatDuration formats a duration into a human-readable string
func formatDuration(d time.Duration) string {
	// Round to seconds
	d = d.Round(time.Second)

	if d < time.Minute {
		return fmt.Sprintf("%.3gs", d.Seconds())
	} else if d < time.Hour {
		m := d / time.Minute
		s := (d % time.Minute) / time.Second
		return fmt.Sprintf("%dm %ds", m, s)
	} else {
		h := d / time.Hour
		m := (d % time.Hour) / time.Minute
		return fmt.Sprintf("%dh %dm", h, m)
	}
}
