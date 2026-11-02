using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using UnitRosterGenerator;

namespace ArmyGeneratorMaui.ViewModels
{
    public partial class UnitsViewModel : ObservableObject
    {
        private const int CandidateRosterCount = 100;
        private readonly string importedArmiesDirectory =
            Path.Combine(FileSystem.Current.AppDataDirectory, "ImportedArmies");

        [ObservableProperty]
        private ObservableCollection<ImportedArmyViewModel> importedArmies = new();

        [ObservableProperty]
        private ImportedArmyViewModel? selectedArmy;

        [ObservableProperty]
        private ObservableCollection<UnitQuantityViewModel> unitLimits = new();

        [ObservableProperty]
        private ObservableCollection<GeneratedRosterViewModel> generatedRosters = new();

        [ObservableProperty]
        private int maxSizeOfRoster = 2000;

        [ObservableProperty]
        private bool isGenerating;

        [ObservableProperty]
        private string generationProgress = string.Empty;

        public UnitsViewModel()
        {
            Directory.CreateDirectory(importedArmiesDirectory);
            LoadImportedArmies();
            SelectedArmy = ImportedArmies.FirstOrDefault();
        }

        partial void OnSelectedArmyChanged(ImportedArmyViewModel? value)
        {
            UnitLimits.Clear();
            GeneratedRosters.Clear();

            if (value == null)
            {
                return;
            }

            foreach (var unitName in value.Data.Units
                         .Select(unit => unit.Name)
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var maxQuantity = value.MaxUnitQuantities.GetValueOrDefault(unitName, 1);
                var unitLimit = new UnitQuantityViewModel(unitName, maxQuantity);
                unitLimit.PropertyChanged += UnitLimit_PropertyChanged;
                UnitLimits.Add(unitLimit);
            }
        }

        [RelayCommand]
        private async Task ImportArmy()
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Import army JSON"
            });

            if (file == null)
            {
                return;
            }

            if (!Path.GetExtension(file.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                GenerationProgress = "Please select a JSON file.";
                return;
            }

            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var data = JsonSerializer.Deserialize<GameData>(json);

                if (data?.Units == null || data.Units.Count == 0)
                {
                    GenerationProgress = "The JSON file does not contain any units.";
                    return;
                }

                var record = new ImportedArmyRecord
                {
                    Name = Path.GetFileNameWithoutExtension(file.FileName),
                    ImportedAt = DateTime.Now,
                    Json = json,
                    MaxUnitQuantities = data.Units
                        .Where(unit => !string.IsNullOrWhiteSpace(unit.Name))
                        .Select(unit => unit.Name)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(name => name, _ => 1, StringComparer.OrdinalIgnoreCase)
                };
                var storagePath = Path.Combine(importedArmiesDirectory, $"{Guid.NewGuid():N}.json");
                SaveRecord(storagePath, record);

                var importedArmy = CreateArmyViewModel(record, data, storagePath);
                ImportedArmies.Insert(0, importedArmy);
                SelectedArmy = importedArmy;
                GenerationProgress = $"Imported {record.Name}.";
            }
            catch (JsonException)
            {
                GenerationProgress = "The selected file is not a valid army JSON file.";
            }
            catch (IOException)
            {
                GenerationProgress = "The selected file could not be read.";
            }
        }

        [RelayCommand]
        private async Task GenerateRosters()
        {
            var selectedArmy = SelectedArmy;
            if (selectedArmy == null)
            {
                GenerationProgress = "Import or select an army first.";
                return;
            }

            IsGenerating = true;
            GeneratedRosters.Clear();
            GenerationProgress = $"Generating rosters: 0/{CandidateRosterCount}";
            var unitQuantities = UnitLimits.ToDictionary(
                limit => limit.Name,
                limit => limit.MaxQuantity,
                StringComparer.OrdinalIgnoreCase);
            var maxPoints = MaxSizeOfRoster;
            var progress = new Progress<int>(count =>
                GenerationProgress = $"Generating rosters: {count}/{CandidateRosterCount}");

            try
            {
                var rosters = await Task.Run(() => RosterGenerationService.GenerateTopRosters(
                    selectedArmy.Data,
                    maxPoints,
                    unitQuantities,
                    CandidateRosterCount,
                    progress));

                for (var index = 0; index < rosters.Count; index++)
                {
                    GeneratedRosters.Add(new GeneratedRosterViewModel(index + 1, rosters[index]));
                }

                GenerationProgress = $"Generated {rosters.Count} rosters.";
            }
            catch (Exception exception)
            {
                GenerationProgress = $"Generation failed: {exception.Message}";
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private void LoadImportedArmies()
        {
            foreach (var storagePath in Directory.EnumerateFiles(importedArmiesDirectory, "*.json"))
            {
                try
                {
                    var record = JsonSerializer.Deserialize<ImportedArmyRecord>(File.ReadAllText(storagePath));
                    if (record == null || string.IsNullOrWhiteSpace(record.Json))
                    {
                        continue;
                    }

                    var data = JsonSerializer.Deserialize<GameData>(record.Json);
                    if (data?.Units?.Count > 0)
                    {
                        ImportedArmies.Add(CreateArmyViewModel(record, data, storagePath));
                    }
                }
                catch (JsonException)
                {
                }
                catch (IOException)
                {
                }
            }

            foreach (var army in ImportedArmies.OrderBy(army => army.ImportedAt).ToList())
            {
                ImportedArmies.Move(ImportedArmies.IndexOf(army), 0);
            }
        }

        private ImportedArmyViewModel CreateArmyViewModel(
            ImportedArmyRecord record,
            GameData data,
            string storagePath)
        {
            return new ImportedArmyViewModel(
                record.Name,
                record.ImportedAt,
                record.Json,
                data,
                record.MaxUnitQuantities ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                storagePath);
        }

        private void UnitLimit_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(UnitQuantityViewModel.MaxQuantity) ||
                sender is not UnitQuantityViewModel changedLimit ||
                SelectedArmy == null)
            {
                return;
            }

            SelectedArmy.MaxUnitQuantities[changedLimit.Name] = changedLimit.MaxQuantity;
            SaveRecord(SelectedArmy.StoragePath, new ImportedArmyRecord
            {
                Name = SelectedArmy.Name,
                ImportedAt = SelectedArmy.ImportedAt,
                Json = SelectedArmy.SourceJson,
                MaxUnitQuantities = SelectedArmy.MaxUnitQuantities
            });
        }

        private static void SaveRecord(string path, ImportedArmyRecord record)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(record));
        }
    }

    public sealed class ImportedArmyViewModel
    {
        public string Name { get; }
        public DateTime ImportedAt { get; }
        public string SourceJson { get; }
        public GameData Data { get; }
        public Dictionary<string, int> MaxUnitQuantities { get; }
        public string StoragePath { get; }
        public string DisplayName => $"{Name} — {ImportedAt:dd.MM.yyyy HH:mm}";

        public ImportedArmyViewModel(
            string name,
            DateTime importedAt,
            string sourceJson,
            GameData data,
            Dictionary<string, int> maxUnitQuantities,
            string storagePath)
        {
            Name = name;
            ImportedAt = importedAt;
            SourceJson = sourceJson;
            Data = data;
            MaxUnitQuantities = maxUnitQuantities;
            StoragePath = storagePath;
        }
    }

    public partial class UnitQuantityViewModel : ObservableObject
    {
        public string Name { get; }

        [ObservableProperty]
        private int maxQuantity;

        public UnitQuantityViewModel(string name, int maxQuantity)
        {
            Name = name;
            this.maxQuantity = Math.Max(0, maxQuantity);
        }

        [RelayCommand]
        private void DecreaseQuantity()
        {
            if (MaxQuantity > 0)
            {
                MaxQuantity--;
            }
        }

        [RelayCommand]
        private void IncreaseQuantity()
        {
            MaxQuantity++;
        }
    }

    public sealed class GeneratedRosterViewModel
    {
        public string Title { get; }
        public string CopyText { get; }

        public GeneratedRosterViewModel(int index, Roster roster)
        {
            Title = $"Roster {index} — {roster.CalculateTotalCost():N0} pts";
            var lines = roster.UnitConfigurations
                .Select(unit => $"{unit.DisplayName} ×{unit.ModelCount} — {unit.TotalCost} pts")
                .ToList();

            if (roster.SelectedDetaches.Count > 0)
            {
                lines.Insert(0, $"Detachments: {string.Join(", ", roster.SelectedDetaches.Select(detach => detach.Name))}");
            }

            lines.Insert(0, Title);
            CopyText = string.Join(Environment.NewLine, lines);
        }
    }

    internal sealed class ImportedArmyRecord
    {
        public ImportedArmyRecord()
        {
        }

        public string Name { get; set; } = string.Empty;
        public DateTime ImportedAt { get; set; }
        public string Json { get; set; } = string.Empty;
        public Dictionary<string, int> MaxUnitQuantities { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
