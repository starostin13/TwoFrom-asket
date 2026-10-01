# Two From Casket
Tired of the building roster for Warhammer 40K again and again? Need advice which unit should be in? That application created for "think instead of you". The name of app and repository refer to two character from old soviet animation. They realized desires and did everything instead of the protagonist (even eat candies).

## Running the console application

The console application is in `ConsoleApp`. Run the commands below from the repository root (the directory containing `ConsoleApp`):

```powershell
dotnet build .\ConsoleApp\ConsoleApp.csproj
dotnet run --project .\ConsoleApp\ConsoleApp.csproj
```

When no arguments are provided, the app loads `ChaosDaemons - Nurgle.json` by default. To pass a faction file and the maximum roster points, put both after `--`:

```powershell
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- ".\ConsoleApp\Data\GreyKnights.json" 1000
```

Pass multiple JSON files as separate arguments to combine their units and detachments into one data set. Quote each path, especially when its filename contains spaces. The points value can follow the file names:

```powershell
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- `
	".\ConsoleApp\Data\ChaosDaemons - Nurgle.json" `
	".\ConsoleApp\Data\ChaosDaemons - Tzeentch.json" `
	1000
```

An optional `--tags` argument filters units by all listed tags (comma-separated):

```powershell
dotnet run --project .\ConsoleApp\ConsoleApp.csproj -- `
	".\ConsoleApp\Data\GreyKnights.json" 1000 --tags E,M
```

The project targets .NET 9, so the .NET 9 SDK (or a compatible newer SDK) is required. JSON files under `ConsoleApp\Data` are copied to the build output; paths to files elsewhere can also be supplied.
