# ONI String Assembler

A quick & dirty C# project that recreates in-game text using the data files.

## How to use

Before using the assembler, you need files from the game. Navigate to your ONI directory and, using a C# disassembler of your choice, extract STRINGS.cs and COLONY_ACHIVEMENTS.cs from the . You will need to add the directive `using LocString = System.String` at the beginning of the file, and also delete all mentions of the UI class.

Then, extract the `OxygenNotIncluded_Data/StreamingAssets/codex/` directory from your ONI directory and place it under `bin/Debug/` in the directory with `.csproj`.

Finally, open up Visual Studio 2019, install the required packages with NuGet, and press F5. The code should run, and all outputs will be placed in `../Debug/output`.

`var type` in `Program.cs` determines which type of codex will be extracted. `Utility.RewriteHtmlNode` defines the output syntax and can be modified for other formats, e.g. Markdown.