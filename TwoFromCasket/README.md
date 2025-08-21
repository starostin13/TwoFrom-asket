# Two From Casket
Tired of building a roster for Warhammer / other systems again and again? Need advice which unit should be in? This application tries to "think instead of you".

The name of the app and repository refers to two characters from an old Soviet animation. They fulfilled wishes and did everything instead of the protagonist (even eating candies).

## Console App Usage

All roster source JSON files must now reside in the `ConsoleApp/Data` directory. The program looks there by default (unless you pass an absolute/relative full path).

Run the console generator specifying (optionally) the data JSON file name (relative to `Data`) and point limit.

Arguments:
- <faction.json> (опционально) – путь или имя JSON файла (по умолчанию `BlackTemplars.json`)
- --points=NUMBER или -p=NUMBER – лимит очков (по умолчанию 2000)

Examples (PowerShell):
```
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- BlackTemplars.json --points=2000
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- USA-BoltAction.json -p=750
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- -p=1000   # файл по умолчанию (Data/BlackTemplars.json)
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- C:\full\path\to\custom.json -p=1500  # полный путь допускается
```

If only a points argument is provided, the default JSON (`Data/BlackTemplars.json`) is used.

Search order (when you pass just a file name):
1. `Data/<name>` relative to current working directory
2. `ConsoleApp/Data/<name>` (if run from repository root)
3. `<bin>/Data/<name>` (when running compiled output)
4. `../ConsoleApp/Data/<name>` from bin (debug layout)

## Adding / Updating Data Files
1. Place your new `<Faction>.json` into `ConsoleApp/Data/`.
2. Ensure it matches schema: `{ "Units": [ ... ], "Detaches": [ ... ] }`.
3. (Optional) Add `Corps` arrays to units; if omitted legacy fields are auto-wrapped into a single corps variant at runtime.
4. Run with the file name: `dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- MyFaction.json -p=1250`.

## Adding Corps Variants
If you add the new `Corps` array to a unit in JSON, the program will use those variants. If absent, it auto-wraps legacy `MinModels/MaxModels/Experience` into a single default corps option.

## Tests
Unit tests (xUnit) are in the `Tests` project:
```
dotnet test .\Tests\Tests.csproj
```

## Encoding
Console output is UTF-8. If you see garbled Cyrillic on Windows, set the terminal to UTF-8 (e.g. `chcp 65001`) or use Windows Terminal / VS Code integrated terminal.

