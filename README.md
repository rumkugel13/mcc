# mcc
A mini (Subset of) C compiler written in C#, just for fun (and learning purposes)

## About

Based on [this blog series by Nora Sandler: Write a Compiler](https://norasandler.com/2017/11/29/Write-a-Compiler.html)

Successfully passes all [test cases](https://github.com/nlsandler/write_a_c_compiler) (including some additional) provided by the same Author.

### Note: Only the following subset of C is currently supported:
- int Datatype
- Unary Operators ('-', '~', '!')
- Binary Operators ('+', '-', '*', '/', '%', '<', '>', '&', '|', '^', "&&", "||", "==", "!=", "<=", ">=", "<<", ">>")
- Local and Global variables (only of type int)
- Conditionals (if/else, Ternary "?:")
- Compound Statements / Code Blocks / Scopes
- Loops (for, do, while)
- Function Declaration and Definition (returning int value, optional arguments), Calling

### Not (yet) implemented:
Optionally from [Part5](https://norasandler.com/2018/01/08/Write-a-Compiler-5.html):
- Compound Assignment Operators ("+=", "-=", "/=", "*=", "%=", "<<=", ">>=", "&=", "|=", "^=")
- Comma Operator (e1, e2)
- Increment/Decrement Operators (Prefix and postfix ++, Prefix and postfix --)

Correct* usage of [Calling Conventions for x86_64](https://en.wikipedia.org/wiki/X86_calling_conventions) (System V ABI for Linux 64-Bit and others, or MS x64 ABI for Windows 64-Bit) and [Calling Conventions for Arm64](https://en.wikipedia.org/wiki/Calling_convention#ARM_(A64))

\* The first n arguments are now passed via the correct registers (n depending on Calling Convention), 
the rest are put on the stack (but reading them is not yet implemented)

### Other notes:
- Compiles source into x86_64 or Arm64 assembly (depends on OS arch)
- Generated *.s files get assembled using gcc
- There is an option to interpret a source file (for supported subset)
- Running Tests removes output files

## Tested on
- Windows 10 64-Bit (Visual Studio 2022, .Net 6.0, gcc installed via cygwin 3.3.5)
- Debian 11 (Bullseye) via WSL (.Net 6.0, gcc 10.2.1)
- Raspberry Pi OS 64 Bit (Debian 11 Bullseye) on Raspberry Pi 3B+ (.Net 6.0, gcc 10.2.1)