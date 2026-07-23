//go:build !windows

package main

import "os/exec"

func setHiddenWindow(_ *exec.Cmd) {}
