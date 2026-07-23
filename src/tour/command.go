package main

import (
	"os/exec"
	"runtime"
)

var newCommand = exec.CommandContext

func hideCommandWindow(command *exec.Cmd) {
	if runtime.GOOS == "windows" {
		setHiddenWindow(command)
	}
}
