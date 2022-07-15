# mcc
A mini (Subset of) C compiler written in C#

## About

Based on [this blog series by Nora Sandler: Write a Compiler](https://norasandler.com/2017/11/29/Write-a-Compiler.html)

Successfully passes all* [test cases](https://github.com/nlsandler/write_a_c_compiler) (including some extra) provided by the same Author.

### Note: Only the following subset of C is currently supported:
- int type
- Unary Operators ('-', '~', '!')
- Binary Operators ('+', '-', '*', '/', '%', '<', '>', '&', '|', '^', "&&", "||", "==", "!=", "<=", ">=", "<<", ">>")
- Local and Global variables (only of type int)
- Conditionals (if/else, "?:")
- Compound Statements / Code Blocks / Scopes
- Loops (for, do, while)
- Function Declaration and Definition (returning int value, optional arguments), Calling

### Not (yet) implemented:
Optionally from [Part5](https://norasandler.com/2018/01/08/Write-a-Compiler-5.html):
- Compound Assignment Operators ("+=", "-=", "/=", "*=", "%=", "<<=", ">>=", "&=", "|=", "^=")
- Comma Operator (e1, e2)
- Increment/Decrement Operators (Prefix and postfix ++, Prefix and postfix --)

Correct usage of [Calling Conventions](https://en.wikipedia.org/wiki/X86_calling_conventions) (SysV ABI for *Nix 64-Bit or MS x64 ABI for Windows 64-Bit)

### Other notes:
- Compiles source into x86_64 assembly (currently 64-bit)
- Generated *.s files get assembled using gcc
- Running Tests removes output files

## Tested on
- Windows 10 64-Bit
- Visual Studio 2022
- .Net 6.0
- gcc installed via cygwin 3.3.5

\* There is a workaround for the following test: stage_9/valid/hello_world.c\
  The int putchar(int); function used in this program adheres to the Microsoft x64 ABI Calling Convention\
  (at least on Windows), which we don't support (yet).
