//go:build windows

package main

import (
	"os/exec"
	"syscall"
)

func setHiddenWindow(command *exec.Cmd) {
	command.SysProcAttr = &syscall.SysProcAttr{HideWindow: true}
}
