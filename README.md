# mcc
A mini C compiler written in C#

### Based on [this blog series by Nora Sandler: Write a Compiler](https://norasandler.com/2017/11/29/Write-a-Compiler.html)

Successfully passes all (but one*) [test cases](https://github.com/nlsandler/write_a_c_compiler) (including some extra) provided by the same Author.

### Note: Only the following subset of C is currently supported:
- int type
- Unary Operators ('-', '~', '!')
- Binary Operators ('+', '-', '*', '/', '%', '<', '>', '&', '|', '^', "&&", "||", "==", "!=", "<=", ">=", "<<", ">>")
- Local and Global variables (only of type int)
- Conditionals (if/else, "?:")
- Compound Statements / Code Blocks / Scopes
- Loops (for, do, while)
- Functions (returning int value, optional arguments)

### Not yet implemented:
Optionally from [Part5](https://norasandler.com/2018/01/08/Write-a-Compiler-5.html):
- Compound Assignment Operators ("+=", "-=", "/=", "*=", "%=", "<<=", ">>=", "&=", "|=", "^=")
- Comma Operator (e1, e2)
- Increment/Decrement Operators (Prefix and postfix ++, Prefix and postfix --)

### Other notes:
- Compiles source into x86_64 assembly (currently 64-bit)
- Generated *.s files get assembled using gcc
- Running the program without any parameters prints usage

## Tested on
- Windows 10
- .Net 6.0
- gcc installed via cygwin

\* The following test doesn't work: stage_9/valid/hello_world.c\
  The int putchar(int); function used in this program doesn't print the correct output.
