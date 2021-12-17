# ONI String Assembler

A quick & dirty C# project that recreates in-game text using the data files.

## How to use

Use dotPeek to extract all the STRINGS related files into the STRINGS directory.

Then, extract the `OxygenNotIncluded_Data/StreamingAssets/codex/` directory from your ONI directory and place it under `bin/Debug/` in the directory with `.csproj`.

Finally, open up Visual Studio 2019 (or other IDE),
 install the required packages with NuGet, and press F5. The code should run, 
 and all outputs will be placed in `../Debug/output`.

`var type` in `Program.cs` determines which type of codex will be extracted.
 `Utility.RewriteHtmlNode` defines the output syntax and can be modified for 
 other formats, e.g. Markdown.
