// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package term -- go2cs converted at 2022 March 06 23:31:03 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\terminal.go
using bytes = go.bytes_package;
using io = go.io_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.cmd.vendor.golang.org.x;

public static partial class term_package {

    // EscapeCodes contains escape sequences that can be written to the terminal in
    // order to achieve different styles of text.
public partial struct EscapeCodes {
    public slice<byte> Black; // Reset all attributes
    public slice<byte> Red; // Reset all attributes
    public slice<byte> Green; // Reset all attributes
    public slice<byte> Yellow; // Reset all attributes
    public slice<byte> Blue; // Reset all attributes
    public slice<byte> Magenta; // Reset all attributes
    public slice<byte> Cyan; // Reset all attributes
    public slice<byte> White; // Reset all attributes
    public slice<byte> Reset;
}

private static EscapeCodes vt100EscapeCodes = new EscapeCodes(Black:[]byte{keyEscape,'[','3','0','m'},Red:[]byte{keyEscape,'[','3','1','m'},Green:[]byte{keyEscape,'[','3','2','m'},Yellow:[]byte{keyEscape,'[','3','3','m'},Blue:[]byte{keyEscape,'[','3','4','m'},Magenta:[]byte{keyEscape,'[','3','5','m'},Cyan:[]byte{keyEscape,'[','3','6','m'},White:[]byte{keyEscape,'[','3','7','m'},Reset:[]byte{keyEscape,'[','0','m'},);

// Terminal contains the state for running a VT100 terminal that is capable of
// reading lines of input.
public partial struct Terminal {
    public Func<@string, nint, int, (@string, nint, bool)> AutoCompleteCallback; // Escape contains a pointer to the escape codes for this terminal.
// It's always a valid pointer, although the escape codes themselves
// may be empty if the terminal doesn't support them.
    public ptr<EscapeCodes> Escape; // lock protects the terminal and the state in this object from
// concurrent processing of a key press and a Write() call.
    public sync.Mutex @lock;
    public io.ReadWriter c;
    public slice<int> prompt; // line is the current line being entered.
    public slice<int> line; // pos is the logical position of the cursor in line
    public nint pos; // echo is true if local echo is enabled
    public bool echo; // pasteActive is true iff there is a bracketed paste operation in
// progress.
    public bool pasteActive; // cursorX contains the current X value of the cursor where the left
// edge is 0. cursorY contains the row number where the first row of
// the current line is 0.
    public nint cursorX; // maxLine is the greatest value of cursorY so far.
    public nint cursorY; // maxLine is the greatest value of cursorY so far.
    public nint maxLine;
    public nint termWidth; // outBuf contains the terminal data to be sent.
    public nint termHeight; // outBuf contains the terminal data to be sent.
    public slice<byte> outBuf; // remainder contains the remainder of any partial key sequences after
// a read. It aliases into inBuf.
    public slice<byte> remainder;
    public array<byte> inBuf; // history contains previously entered commands so that they can be
// accessed with the up and down keys.
    public stRingBuffer history; // historyIndex stores the currently accessed history entry, where zero
// means the immediately previous entry.
    public nint historyIndex; // When navigating up and down the history it's possible to return to
// the incomplete, initial line. That value is stored in
// historyPending.
    public @string historyPending;
}

// NewTerminal runs a VT100 terminal on the given ReadWriter. If the ReadWriter is
// a local terminal, that terminal must first have been put into raw mode.
// prompt is a string that is written at the start of each input line (i.e.
// "> ").
public static ptr<Terminal> NewTerminal(io.ReadWriter c, @string prompt) {
    return addr(new Terminal(Escape:&vt100EscapeCodes,c:c,prompt:[]rune(prompt),termWidth:80,termHeight:24,echo:true,historyIndex:-1,));
}

private static readonly nint keyCtrlC = 3;
private static readonly nint keyCtrlD = 4;
private static readonly nint keyCtrlU = 21;
private static readonly char keyEnter = '\r';
private static readonly nint keyEscape = 27;
private static readonly nint keyBackspace = 127;
private static readonly nuint keyUnknown = 0xd800 + iota;
private static readonly var keyUp = 0;
private static readonly var keyDown = 1;
private static readonly var keyLeft = 2;
private static readonly var keyRight = 3;
private static readonly var keyAltLeft = 4;
private static readonly var keyAltRight = 5;
private static readonly var keyHome = 6;
private static readonly var keyEnd = 7;
private static readonly var keyDeleteWord = 8;
private static readonly var keyDeleteLine = 9;
private static readonly var keyClearScreen = 10;
private static readonly var keyPasteStart = 11;
private static readonly var keyPasteEnd = 12;


private static byte crlf = new slice<byte>(new byte[] { '\r', '\n' });private static byte pasteStart = new slice<byte>(new byte[] { keyEscape, '[', '2', '0', '0', '~' });private static byte pasteEnd = new slice<byte>(new byte[] { keyEscape, '[', '2', '0', '1', '~' });

// bytesToKey tries to parse a key sequence from b. If successful, it returns
// the key and the remainder of the input. Otherwise it returns utf8.RuneError.
private static (int, slice<byte>) bytesToKey(slice<byte> b, bool pasteActive) {
    int _p0 = default;
    slice<byte> _p0 = default;

    if (len(b) == 0) {
        return (utf8.RuneError, null);
    }
    if (!pasteActive) {
        switch (b[0]) {
            case 1: // ^A
                return (keyHome, b[(int)1..]);
                break;
            case 2: // ^B
                return (keyLeft, b[(int)1..]);
                break;
            case 5: // ^E
                return (keyEnd, b[(int)1..]);
                break;
            case 6: // ^F
                return (keyRight, b[(int)1..]);
                break;
            case 8: // ^H
                return (keyBackspace, b[(int)1..]);
                break;
            case 11: // ^K
                return (keyDeleteLine, b[(int)1..]);
                break;
            case 12: // ^L
                return (keyClearScreen, b[(int)1..]);
                break;
            case 23: // ^W
                return (keyDeleteWord, b[(int)1..]);
                break;
            case 14: // ^N
                return (keyDown, b[(int)1..]);
                break;
            case 16: // ^P
                return (keyUp, b[(int)1..]);
                break;
        }

    }
    if (b[0] != keyEscape) {
        if (!utf8.FullRune(b)) {
            return (utf8.RuneError, b);
        }
        var (r, l) = utf8.DecodeRune(b);
        return (r, b[(int)l..]);

    }
    if (!pasteActive && len(b) >= 3 && b[0] == keyEscape && b[1] == '[') {
        switch (b[2]) {
            case 'A': 
                return (keyUp, b[(int)3..]);
                break;
            case 'B': 
                return (keyDown, b[(int)3..]);
                break;
            case 'C': 
                return (keyRight, b[(int)3..]);
                break;
            case 'D': 
                return (keyLeft, b[(int)3..]);
                break;
            case 'H': 
                return (keyHome, b[(int)3..]);
                break;
            case 'F': 
                return (keyEnd, b[(int)3..]);
                break;
        }

    }
    if (!pasteActive && len(b) >= 6 && b[0] == keyEscape && b[1] == '[' && b[2] == '1' && b[3] == ';' && b[4] == '3') {
        switch (b[5]) {
            case 'C': 
                return (keyAltRight, b[(int)6..]);
                break;
            case 'D': 
                return (keyAltLeft, b[(int)6..]);
                break;
        }

    }
    if (!pasteActive && len(b) >= 6 && bytes.Equal(b[..(int)6], pasteStart)) {
        return (keyPasteStart, b[(int)6..]);
    }
    if (pasteActive && len(b) >= 6 && bytes.Equal(b[..(int)6], pasteEnd)) {
        return (keyPasteEnd, b[(int)6..]);
    }
    foreach (var (i, c) in b[(int)0..]) {
        if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '~') {
            return (keyUnknown, b[(int)i + 1..]);
        }
    }    return (utf8.RuneError, b);

}

// queue appends data to the end of t.outBuf
private static void queue(this ptr<Terminal> _addr_t, slice<int> data) {
    ref Terminal t = ref _addr_t.val;

    t.outBuf = append(t.outBuf, (slice<byte>)string(data));
}

private static int eraseUnderCursor = new slice<int>(new int[] { ' ', keyEscape, '[', 'D' });
private static int space = new slice<int>(new int[] { ' ' });

private static bool isPrintable(int key) {
    var isInSurrogateArea = key >= 0xd800 && key <= 0xdbff;
    return key >= 32 && !isInSurrogateArea;
}

// moveCursorToPos appends data to t.outBuf which will move the cursor to the
// given, logical position in the text.
private static void moveCursorToPos(this ptr<Terminal> _addr_t, nint pos) {
    ref Terminal t = ref _addr_t.val;

    if (!t.echo) {
        return ;
    }
    var x = visualLength(t.prompt) + pos;
    var y = x / t.termWidth;
    x = x % t.termWidth;

    nint up = 0;
    if (y < t.cursorY) {
        up = t.cursorY - y;
    }
    nint down = 0;
    if (y > t.cursorY) {
        down = y - t.cursorY;
    }
    nint left = 0;
    if (x < t.cursorX) {
        left = t.cursorX - x;
    }
    nint right = 0;
    if (x > t.cursorX) {
        right = x - t.cursorX;
    }
    t.cursorX = x;
    t.cursorY = y;
    t.move(up, down, left, right);

}

private static void move(this ptr<Terminal> _addr_t, nint up, nint down, nint left, nint right) {
    ref Terminal t = ref _addr_t.val;

    int m = new slice<int>(new int[] {  }); 

    // 1 unit up can be expressed as ^[[A or ^[A
    // 5 units up can be expressed as ^[[5A

    if (up == 1) {
        m = append(m, keyEscape, '[', 'A');
    }
    else if (up > 1) {
        m = append(m, keyEscape, '[');
        m = append(m, (slice<int>)strconv.Itoa(up));
        m = append(m, 'A');
    }
    if (down == 1) {
        m = append(m, keyEscape, '[', 'B');
    }
    else if (down > 1) {
        m = append(m, keyEscape, '[');
        m = append(m, (slice<int>)strconv.Itoa(down));
        m = append(m, 'B');
    }
    if (right == 1) {
        m = append(m, keyEscape, '[', 'C');
    }
    else if (right > 1) {
        m = append(m, keyEscape, '[');
        m = append(m, (slice<int>)strconv.Itoa(right));
        m = append(m, 'C');
    }
    if (left == 1) {
        m = append(m, keyEscape, '[', 'D');
    }
    else if (left > 1) {
        m = append(m, keyEscape, '[');
        m = append(m, (slice<int>)strconv.Itoa(left));
        m = append(m, 'D');
    }
    t.queue(m);

}

private static void clearLineToRight(this ptr<Terminal> _addr_t) {
    ref Terminal t = ref _addr_t.val;

    int op = new slice<int>(new int[] { keyEscape, '[', 'K' });
    t.queue(op);
}

private static readonly nint maxLineLength = 4096;



private static void setLine(this ptr<Terminal> _addr_t, slice<int> newLine, nint newPos) {
    ref Terminal t = ref _addr_t.val;

    if (t.echo) {
        t.moveCursorToPos(0);
        t.writeLine(newLine);
        for (var i = len(newLine); i < len(t.line); i++) {
            t.writeLine(space);
        }
        t.moveCursorToPos(newPos);
    }
    t.line = newLine;
    t.pos = newPos;

}

private static void advanceCursor(this ptr<Terminal> _addr_t, nint places) {
    ref Terminal t = ref _addr_t.val;

    t.cursorX += places;
    t.cursorY += t.cursorX / t.termWidth;
    if (t.cursorY > t.maxLine) {
        t.maxLine = t.cursorY;
    }
    t.cursorX = t.cursorX % t.termWidth;

    if (places > 0 && t.cursorX == 0) { 
        // Normally terminals will advance the current position
        // when writing a character. But that doesn't happen
        // for the last character in a line. However, when
        // writing a character (except a new line) that causes
        // a line wrap, the position will be advanced two
        // places.
        //
        // So, if we are stopping at the end of a line, we
        // need to write a newline so that our cursor can be
        // advanced to the next line.
        t.outBuf = append(t.outBuf, '\r', '\n');

    }
}

private static void eraseNPreviousChars(this ptr<Terminal> _addr_t, nint n) {
    ref Terminal t = ref _addr_t.val;

    if (n == 0) {
        return ;
    }
    if (t.pos < n) {
        n = t.pos;
    }
    t.pos -= n;
    t.moveCursorToPos(t.pos);

    copy(t.line[(int)t.pos..], t.line[(int)n + t.pos..]);
    t.line = t.line[..(int)len(t.line) - n];
    if (t.echo) {
        t.writeLine(t.line[(int)t.pos..]);
        for (nint i = 0; i < n; i++) {
            t.queue(space);
        }
        t.advanceCursor(n);
        t.moveCursorToPos(t.pos);
    }
}

// countToLeftWord returns then number of characters from the cursor to the
// start of the previous word.
private static nint countToLeftWord(this ptr<Terminal> _addr_t) {
    ref Terminal t = ref _addr_t.val;

    if (t.pos == 0) {
        return 0;
    }
    var pos = t.pos - 1;
    while (pos > 0) {
        if (t.line[pos] != ' ') {
            break;
        }
        pos--;

    }
    while (pos > 0) {
        if (t.line[pos] == ' ') {
            pos++;
            break;
        }
        pos--;

    }

    return t.pos - pos;

}

// countToRightWord returns then number of characters from the cursor to the
// start of the next word.
private static nint countToRightWord(this ptr<Terminal> _addr_t) {
    ref Terminal t = ref _addr_t.val;

    var pos = t.pos;
    while (pos < len(t.line)) {
        if (t.line[pos] == ' ') {
            break;
        }
        pos++;

    }
    while (pos < len(t.line)) {
        if (t.line[pos] != ' ') {
            break;
        }
        pos++;

    }
    return pos - t.pos;

}

// visualLength returns the number of visible glyphs in s.
private static nint visualLength(slice<int> runes) {
    var inEscapeSeq = false;
    nint length = 0;

    foreach (var (_, r) in runes) {

        if (inEscapeSeq) 
            if ((r >= 'a' && r <= 'z') || (r >= 'A' && r <= 'Z')) {
                inEscapeSeq = false;
            }
        else if (r == '\x1b') 
            inEscapeSeq = true;
        else 
            length++;
        
    }    return length;

}

// handleKey processes the given key and, optionally, returns a line of text
// that the user has entered.
private static (@string, bool) handleKey(this ptr<Terminal> _addr_t, int key) {
    @string line = default;
    bool ok = default;
    ref Terminal t = ref _addr_t.val;

    if (t.pasteActive && key != keyEnter) {
        t.addKeyToLine(key);
        return ;
    }

    if (key == keyBackspace) 
        if (t.pos == 0) {
            return ;
        }
        t.eraseNPreviousChars(1);
    else if (key == keyAltLeft) 
        // move left by a word.
        t.pos -= t.countToLeftWord();
        t.moveCursorToPos(t.pos);
    else if (key == keyAltRight) 
        // move right by a word.
        t.pos += t.countToRightWord();
        t.moveCursorToPos(t.pos);
    else if (key == keyLeft) 
        if (t.pos == 0) {
            return ;
        }
        t.pos--;
        t.moveCursorToPos(t.pos);
    else if (key == keyRight) 
        if (t.pos == len(t.line)) {
            return ;
        }
        t.pos++;
        t.moveCursorToPos(t.pos);
    else if (key == keyHome) 
        if (t.pos == 0) {
            return ;
        }
        t.pos = 0;
        t.moveCursorToPos(t.pos);
    else if (key == keyEnd) 
        if (t.pos == len(t.line)) {
            return ;
        }
        t.pos = len(t.line);
        t.moveCursorToPos(t.pos);
    else if (key == keyUp) 
        var (entry, ok) = t.history.NthPreviousEntry(t.historyIndex + 1);
        if (!ok) {
            return ("", false);
        }
        if (t.historyIndex == -1) {
            t.historyPending = string(t.line);
        }
        t.historyIndex++;
        slice<int> runes = (slice<int>)entry;
        t.setLine(runes, len(runes));
    else if (key == keyDown) 
        switch (t.historyIndex) {
            case -1: 
                return ;
                break;
            case 0: 
                runes = (slice<int>)t.historyPending;
                t.setLine(runes, len(runes));
                t.historyIndex--;
                break;
            default: 
                (entry, ok) = t.history.NthPreviousEntry(t.historyIndex - 1);
                if (ok) {
                    t.historyIndex--;
                    runes = (slice<int>)entry;
                    t.setLine(runes, len(runes));
                }
                break;
        }
    else if (key == keyEnter) 
        t.moveCursorToPos(len(t.line));
        t.queue((slice<int>)"\r\n");
        line = string(t.line);
        ok = true;
        t.line = t.line[..(int)0];
        t.pos = 0;
        t.cursorX = 0;
        t.cursorY = 0;
        t.maxLine = 0;
    else if (key == keyDeleteWord) 
        // Delete zero or more spaces and then one or more characters.
        t.eraseNPreviousChars(t.countToLeftWord());
    else if (key == keyDeleteLine) 
        // Delete everything from the current cursor position to the
        // end of line.
        for (var i = t.pos; i < len(t.line); i++) {
            t.queue(space);
            t.advanceCursor(1);
        }
        t.line = t.line[..(int)t.pos];
        t.moveCursorToPos(t.pos);
    else if (key == keyCtrlD) 
        // Erase the character under the current position.
        // The EOF case when the line is empty is handled in
        // readLine().
        if (t.pos < len(t.line)) {
            t.pos++;
            t.eraseNPreviousChars(1);
        }
    else if (key == keyCtrlU) 
        t.eraseNPreviousChars(t.pos);
    else if (key == keyClearScreen) 
        // Erases the screen and moves the cursor to the home position.
        t.queue((slice<int>)"\x1b[2J\x1b[H");
        t.queue(t.prompt);
        (t.cursorX, t.cursorY) = (0, 0);        t.advanceCursor(visualLength(t.prompt));
        t.setLine(t.line, t.pos);
    else 
        if (t.AutoCompleteCallback != null) {
            var prefix = string(t.line[..(int)t.pos]);
            var suffix = string(t.line[(int)t.pos..]);

            t.@lock.Unlock();
            var (newLine, newPos, completeOk) = t.AutoCompleteCallback(prefix + suffix, len(prefix), key);
            t.@lock.Lock();

            if (completeOk) {
                t.setLine((slice<int>)newLine, utf8.RuneCount((slice<byte>)newLine[..(int)newPos]));
                return ;
            }
        }
        if (!isPrintable(key)) {
            return ;
        }
        if (len(t.line) == maxLineLength) {
            return ;
        }
        t.addKeyToLine(key);
        return ;

}

// addKeyToLine inserts the given key at the current position in the current
// line.
private static void addKeyToLine(this ptr<Terminal> _addr_t, int key) {
    ref Terminal t = ref _addr_t.val;

    if (len(t.line) == cap(t.line)) {
        var newLine = make_slice<int>(len(t.line), 2 * (1 + len(t.line)));
        copy(newLine, t.line);
        t.line = newLine;
    }
    t.line = t.line[..(int)len(t.line) + 1];
    copy(t.line[(int)t.pos + 1..], t.line[(int)t.pos..]);
    t.line[t.pos] = key;
    if (t.echo) {
        t.writeLine(t.line[(int)t.pos..]);
    }
    t.pos++;
    t.moveCursorToPos(t.pos);

}

private static void writeLine(this ptr<Terminal> _addr_t, slice<int> line) {
    ref Terminal t = ref _addr_t.val;

    while (len(line) != 0) {
        var remainingOnLine = t.termWidth - t.cursorX;
        var todo = len(line);
        if (todo > remainingOnLine) {
            todo = remainingOnLine;
        }
        t.queue(line[..(int)todo]);
        t.advanceCursor(visualLength(line[..(int)todo]));
        line = line[(int)todo..];

    }

}

// writeWithCRLF writes buf to w but replaces all occurrences of \n with \r\n.
private static (nint, error) writeWithCRLF(io.Writer w, slice<byte> buf) {
    nint n = default;
    error err = default!;

    while (len(buf) > 0) {
        var i = bytes.IndexByte(buf, '\n');
        var todo = len(buf);
        if (i >= 0) {
            todo = i;
        }
        nint nn = default;
        nn, err = w.Write(buf[..(int)todo]);
        n += nn;
        if (err != null) {
            return (n, error.As(err)!);
        }
        buf = buf[(int)todo..];

        if (i >= 0) {
            _, err = w.Write(crlf);

            if (err != null) {
                return (n, error.As(err)!);
            }

            n++;
            buf = buf[(int)1..];

        }
    }

    return (n, error.As(null!)!);

}

private static (nint, error) Write(this ptr<Terminal> _addr_t, slice<byte> buf) => func((defer, _, _) => {
    nint n = default;
    error err = default!;
    ref Terminal t = ref _addr_t.val;

    t.@lock.Lock();
    defer(t.@lock.Unlock());

    if (t.cursorX == 0 && t.cursorY == 0) { 
        // This is the easy case: there's nothing on the screen that we
        // have to move out of the way.
        return writeWithCRLF(t.c, buf);

    }
    t.move(0, 0, t.cursorX, 0);
    t.cursorX = 0;
    t.clearLineToRight();

    while (t.cursorY > 0) {
        t.move(1, 0, 0, 0);
        t.cursorY--;
        t.clearLineToRight();
    }

    _, err = t.c.Write(t.outBuf);

    if (err != null) {
        return ;
    }
    t.outBuf = t.outBuf[..(int)0];

    n, err = writeWithCRLF(t.c, buf);

    if (err != null) {
        return ;
    }
    t.writeLine(t.prompt);
    if (t.echo) {
        t.writeLine(t.line);
    }
    t.moveCursorToPos(t.pos);

    _, err = t.c.Write(t.outBuf);

    if (err != null) {
        return ;
    }
    t.outBuf = t.outBuf[..(int)0];
    return ;

});

// ReadPassword temporarily changes the prompt and reads a password, without
// echo, from the terminal.
private static (@string, error) ReadPassword(this ptr<Terminal> _addr_t, @string prompt) => func((defer, _, _) => {
    @string line = default;
    error err = default!;
    ref Terminal t = ref _addr_t.val;

    t.@lock.Lock();
    defer(t.@lock.Unlock());

    var oldPrompt = t.prompt;
    t.prompt = (slice<int>)prompt;
    t.echo = false;

    line, err = t.readLine();

    t.prompt = oldPrompt;
    t.echo = true;

    return ;
});

// ReadLine returns a line of input from the terminal.
private static (@string, error) ReadLine(this ptr<Terminal> _addr_t) => func((defer, _, _) => {
    @string line = default;
    error err = default!;
    ref Terminal t = ref _addr_t.val;

    t.@lock.Lock();
    defer(t.@lock.Unlock());

    return t.readLine();
});

private static (@string, error) readLine(this ptr<Terminal> _addr_t) {
    @string line = default;
    error err = default!;
    ref Terminal t = ref _addr_t.val;
 
    // t.lock must be held at this point

    if (t.cursorX == 0 && t.cursorY == 0) {
        t.writeLine(t.prompt);
        t.c.Write(t.outBuf);
        t.outBuf = t.outBuf[..(int)0];
    }
    var lineIsPasted = t.pasteActive;

    while (true) {
        var rest = t.remainder;
        var lineOk = false;
        while (!lineOk) {
            int key = default;
            key, rest = bytesToKey(rest, t.pasteActive);
            if (key == utf8.RuneError) {
                break;
            }
            if (!t.pasteActive) {
                if (key == keyCtrlD) {
                    if (len(t.line) == 0) {
                        return ("", error.As(io.EOF)!);
                    }
                }
                if (key == keyCtrlC) {
                    return ("", error.As(io.EOF)!);
                }
                if (key == keyPasteStart) {
                    t.pasteActive = true;
                    if (len(t.line) == 0) {
                        lineIsPasted = true;
                    }
                    continue;
                }
            }
            else if (key == keyPasteEnd) {
                t.pasteActive = false;
                continue;
            }

            if (!t.pasteActive) {
                lineIsPasted = false;
            }

            line, lineOk = t.handleKey(key);

        }
        if (len(rest) > 0) {
            var n = copy(t.inBuf[..], rest);
            t.remainder = t.inBuf[..(int)n];
        }
        else
 {
            t.remainder = null;
        }
        t.c.Write(t.outBuf);
        t.outBuf = t.outBuf[..(int)0];
        if (lineOk) {
            if (t.echo) {
                t.historyIndex = -1;
                t.history.Add(line);
            }
            if (lineIsPasted) {
                err = ErrPasteIndicator;
            }
            return ;
        }
        var readBuf = t.inBuf[(int)len(t.remainder)..];
        n = default;

        t.@lock.Unlock();
        n, err = t.c.Read(readBuf);
        t.@lock.Lock();

        if (err != null) {
            return ;
        }
        t.remainder = t.inBuf[..(int)n + len(t.remainder)];

    }

}

// SetPrompt sets the prompt to be used when reading subsequent lines.
private static void SetPrompt(this ptr<Terminal> _addr_t, @string prompt) => func((defer, _, _) => {
    ref Terminal t = ref _addr_t.val;

    t.@lock.Lock();
    defer(t.@lock.Unlock());

    t.prompt = (slice<int>)prompt;
});

private static void clearAndRepaintLinePlusNPrevious(this ptr<Terminal> _addr_t, nint numPrevLines) {
    ref Terminal t = ref _addr_t.val;
 
    // Move cursor to column zero at the start of the line.
    t.move(t.cursorY, 0, t.cursorX, 0);
    (t.cursorX, t.cursorY) = (0, 0);    t.clearLineToRight();
    while (t.cursorY < numPrevLines) { 
        // Move down a line
        t.move(0, 1, 0, 0);
        t.cursorY++;
        t.clearLineToRight();

    } 
    // Move back to beginning.
    t.move(t.cursorY, 0, 0, 0);
    (t.cursorX, t.cursorY) = (0, 0);    t.queue(t.prompt);
    t.advanceCursor(visualLength(t.prompt));
    t.writeLine(t.line);
    t.moveCursorToPos(t.pos);

}

private static error SetSize(this ptr<Terminal> _addr_t, nint width, nint height) => func((defer, _, _) => {
    ref Terminal t = ref _addr_t.val;

    t.@lock.Lock();
    defer(t.@lock.Unlock());

    if (width == 0) {
        width = 1;
    }
    var oldWidth = t.termWidth;
    (t.termWidth, t.termHeight) = (width, height);
    if (width == oldWidth) 
        // If the width didn't change then nothing else needs to be
        // done.
        return error.As(null!)!;
    else if (len(t.line) == 0 && t.cursorX == 0 && t.cursorY == 0) 
        // If there is nothing on current line and no prompt printed,
        // just do nothing
        return error.As(null!)!;
    else if (width < oldWidth) 
        // Some terminals (e.g. xterm) will truncate lines that were
        // too long when shinking. Others, (e.g. gnome-terminal) will
        // attempt to wrap them. For the former, repainting t.maxLine
        // works great, but that behaviour goes badly wrong in the case
        // of the latter because they have doubled every full line.

        // We assume that we are working on a terminal that wraps lines
        // and adjust the cursor position based on every previous line
        // wrapping and turning into two. This causes the prompt on
        // xterms to move upwards, which isn't great, but it avoids a
        // huge mess with gnome-terminal.
        if (t.cursorX >= t.termWidth) {
            t.cursorX = t.termWidth - 1;
        }
        t.cursorY *= 2;
        t.clearAndRepaintLinePlusNPrevious(t.maxLine * 2);
    else if (width > oldWidth) 
        // If the terminal expands then our position calculations will
        // be wrong in the future because we think the cursor is
        // |t.pos| chars into the string, but there will be a gap at
        // the end of any wrapped line.
        //
        // But the position will actually be correct until we move, so
        // we can move back to the beginning and repaint everything.
        t.clearAndRepaintLinePlusNPrevious(t.maxLine);
        var (_, err) = t.c.Write(t.outBuf);
    t.outBuf = t.outBuf[..(int)0];
    return error.As(err)!;

});

private partial struct pasteIndicatorError {
}

private static @string Error(this pasteIndicatorError _p0) {
    return "terminal: ErrPasteIndicator not correctly handled";
}

// ErrPasteIndicator may be returned from ReadLine as the error, in addition
// to valid line data. It indicates that bracketed paste mode is enabled and
// that the returned line consists only of pasted data. Programs may wish to
// interpret pasted data more literally than typed data.
public static pasteIndicatorError ErrPasteIndicator = new pasteIndicatorError();

// SetBracketedPasteMode requests that the terminal bracket paste operations
// with markers. Not all terminals support this but, if it is supported, then
// enabling this mode will stop any autocomplete callback from running due to
// pastes. Additionally, any lines that are completely pasted will be returned
// from ReadLine with the error set to ErrPasteIndicator.
private static void SetBracketedPasteMode(this ptr<Terminal> _addr_t, bool on) {
    ref Terminal t = ref _addr_t.val;

    if (on) {
        io.WriteString(t.c, "\x1b[?2004h");
    }
    else
 {
        io.WriteString(t.c, "\x1b[?2004l");
    }
}

// stRingBuffer is a ring buffer of strings.
private partial struct stRingBuffer {
    public slice<@string> entries;
    public nint max; // head contains the index of the element most recently added to the ring.
    public nint head; // size contains the number of elements in the ring.
    public nint size;
}

private static void Add(this ptr<stRingBuffer> _addr_s, @string a) {
    ref stRingBuffer s = ref _addr_s.val;

    if (s.entries == null) {
        const nint defaultNumEntries = 100;

        s.entries = make_slice<@string>(defaultNumEntries);
        s.max = defaultNumEntries;
    }
    s.head = (s.head + 1) % s.max;
    s.entries[s.head] = a;
    if (s.size < s.max) {
        s.size++;
    }
}

// NthPreviousEntry returns the value passed to the nth previous call to Add.
// If n is zero then the immediately prior value is returned, if one, then the
// next most recent, and so on. If such an element doesn't exist then ok is
// false.
private static (@string, bool) NthPreviousEntry(this ptr<stRingBuffer> _addr_s, nint n) {
    @string value = default;
    bool ok = default;
    ref stRingBuffer s = ref _addr_s.val;

    if (n >= s.size) {
        return ("", false);
    }
    var index = s.head - n;
    if (index < 0) {
        index += s.max;
    }
    return (s.entries[index], true);

}

// readPasswordLine reads from reader until it finds \n or io.EOF.
// The slice returned does not include the \n.
// readPasswordLine also ignores any \r it finds.
// Windows uses \r as end of line. So, on Windows, readPasswordLine
// reads until it finds \r and ignores any \n it finds during processing.
private static (slice<byte>, error) readPasswordLine(io.Reader reader) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    array<byte> buf = new array<byte>(1);
    slice<byte> ret = default;

    while (true) {
        var (n, err) = reader.Read(buf[..]);
        if (n > 0) {
            switch (buf[0]) {
                case '\b': 
                    if (len(ret) > 0) {
                        ret = ret[..(int)len(ret) - 1];
                    }
                    break;
                case '\n': 
                    if (runtime.GOOS != "windows") {
                        return (ret, error.As(null!)!);
                    } 
                    // otherwise ignore \n
                    break;
                case '\r': 
                    if (runtime.GOOS == "windows") {
                        return (ret, error.As(null!)!);
                    } 
                    // otherwise ignore \r
                    break;
                default: 
                    ret = append(ret, buf[0]);
                    break;
            }
            continue;

        }
        if (err != null) {
            if (err == io.EOF && len(ret) > 0) {
                return (ret, error.As(null!)!);
            }
            return (ret, error.As(err)!);
        }
    }

}

} // end term_package
