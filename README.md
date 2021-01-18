# LuaBlitz
 The LuaU intelligence platform

### Scope
LuaBlitz's primary goal is to enable efficient linting and observation of LuaU source code.

### Organization

 - **LuaBlitz**: Primary project
 - **LuaBlitz.Bench**: Bench for testing LuaBlitz (will be removed upon release)

### Status
LuaBlitz is currently in primary development. It is considered not suitable for use.

| Subproject | Status |
| :----: | :----: |
| Tokenizer | Finished *?* |
| Parser | Working On |
| High-level AST | Not started |
| API | Not started |

*API may be assimilated into "High level AST"

### Building
LuaBlitz is aiming to be compiled to Lua using [CSharp.lua](https://github.com/yanghuan/CSharp.lua)

Otherwise, it should run as a standard .NET Core (or .NET Standard?) class library.

### About
LuaBlitz is just an attempt to make a decent open-source Intelli-sense like platform for Lua, especially for Roblox's LuaU variant.
LuaBlitz's end goal is to perform code-analysis tasks on par with industry-leading IDEs.

LuaBlitz is primarily authored by Mooshua (TheFluffyCat#1857) and is released under GNU GPL v3 (LICENSE.md)

Please do not ask me for help using this project. The C# typing should document it plenty.
