# C# Coding Style

We follow a similar coding style as [dotnet/runtime](https://github.com/dotnet/runtime).

For non code files (xml, etc), our current best guidance is consistency. When editing files, keep new code and changes consistent with the style in the files. For new files, it should conform to the style for that component. If there is a completely new component, anything that is reasonably broadly accepted is fine.

The general rule we follow is "_use Visual Studio defaults_".

1. We use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces, where each brace begins on a new line. A single line statement block can go without braces but the block must be properly indented on its own line and must not be nested in other statement blocks that use braces. One exception is that a `using` statement is permitted to be nested within another `using` statement by starting on the following line at the same indentation level, even if the nested `using` contains a controlled block.
2. We use four spaces of indentation (no tabs).
3. We use `m_camelCase` for internal and private fields and use `readonly` where possible. Prefix internal and private member (instance) fields with `m_`, static fields with `s_` and thread static fields with `t_`. When used on static fields, `readonly` should come after `static` (e.g. `static readonly` not `readonly static`).  Public fields should be used sparingly and should use PascalCasing with no prefix when used.
4. We avoid `this.` unless absolutely necessary. 
5. We always specify the visibility, even if it's the default (e.g. `private string m_foo` not `string m_foo`). Visibility should be the first modifier (e.g. `public abstract` not `abstract public`).
6. Namespace imports should be specified at the top of the file, *outside* of `namespace` declarations, and should be sorted alphabetically, with the exception of `System.*` namespaces, which are to be placed on top of all others.
7. Avoid more than one empty line at any time. For example, do not have two blank lines between members of a type.
8. Avoid spurious free spaces. For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces. Consider enabling "View White Space (Ctrl+E, S)" if using Visual Studio to aid detection.
9. If a file happens to differ in style from these guidelines (e.g. private members are named `_member` rather than `m_member`), the existing style in that file takes precedence.
10. Alway use actual type names, we only use `var` when it's required because the actual type is unknown or is run-time deferred.
11. We use language keywords instead of BCL types (e.g. `int, string, float` instead of `Int32, String, Single`, etc) for both type references as well as method calls (e.g. `int.Parse` instead of `Int32.Parse`).
12. We use PascalCasing to name all our constant local variables and fields. The only exception is for interop code where the constant value should exactly match the name and value of the code you are calling via interop.
13. We use ```nameof(...)``` instead of ```"..."``` whenever possible and relevant.
14. Fields should be specified at the top within type declarations.
15. When including non-ASCII characters in the source code use Unicode escape sequences (\uXXXX) instead of literal characters. Literal non-ASCII characters occasionally get garbled by a tool or editor.
16. When using #region sections or labels (for goto), indent the item one less than the current indentation.

An [EditorConfig](https://editorconfig.org "EditorConfig homepage") file (`.editorconfig`) has been provided at the root of the repository, enabling C# auto-formatting.
