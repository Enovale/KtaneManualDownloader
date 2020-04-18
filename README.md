# Ktane Manual Downloader
This program allows you to download updated manuals for your installed KTANE mods.

The manuals are grabbed from the [Repository of Manual Pages](https://ktane.timwi.de/).

# Prerequisites:
If the app won't launch, make sure you've installed the [.NET Framework 4.7.2 Runtime or higher.](https://dotnet.microsoft.com/download/dotnet-framework/net472)
Make sure not to install the developer pack unless you actually want it.

# Usage
To use it, simply open it up, and type/browser your game's mod folder.

For workshop mods, this path will be something like:
`C:\Program Files(x86)\Steam\steamapps\workshop\content\341800`

For local mods, this path will be something like:
`C:\Program Files(x86)\Steam\steamapps\common\Keep Talking and Nobody Explodes\mods`

Make sure to press enter at the end to submit your input.

After you've done that, configure your settings and press Download, and wait for a little bit.

# Notes
Please note that this in in-dev software made by an inexperienced dev, so bugs and oddities are to be expected.

# Settings
Here I will explain what each of the different options do.

## Merge PDFs:
If this option is checked, after downloading all of the manuals, they will be all combined into one PDF, according to the [sorting rules](#sorting:) and [grouping rules](#grouping:) specified.

## Reverse Order:
Checking this option will simply reverse the final output of whatever other rules have been selected.

## Vanilla Merge:
This option will put the vanilla KTaNE manual pages into your final merged manual (excluding vanilla modules). So for example, the cover page will be added to the beginning of your PDF, then the few intro pages, and the vanilla appendixes at the end of the document.

## Sorting:
These options determine what order the merged PDF pages will be in.

### Sort by mod name
This sort mode will sort all of the modules alphabetically by the mod they're a part of.

### Sort by module name
This sort mode will sort every module alphabetically by the name of the module itself.

### Sort by difficulty
This sort mode will sort every module by it's difficulty, starting from Very Easy (this is probably actually alphabetical, and also not working lmao)

## Grouping:
The **Group by module type** option allows you to group all of the modules in order of their type, so Regular modules will come first, then Needy modules. Within these groups, they will be sorted by the aforementioned [sorting rules](#sorting:).

# TODO:
1. Make append(ixes) work
2. Get experting difficulty on the KtaneModule object for sorting.
3. More bug catching
4. Make better code so bugs don't happen
5. Stop being bad
6. Write a tutorial for building (it should work out of box, right?)
7. Find a way to properly package this app without using the stupid OneClick wizard
8. Stop adding to the readme
