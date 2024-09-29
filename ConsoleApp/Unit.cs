namespace UnitRosterGenerator
{
    // Класс юнита
    class Unit
    {
        public required string Name { get; set; }
        public int MinModels { get; set; }
        public int MaxModels { get; set; }
        public List<ExperienceLevelData>? Experience { get; set; }
        public List<Weapon>? Weapons { get; set; }
        public List<Upgrade>? Upgrade { get; set; }
        public List<IncompatibleWeaponGroup>? IncompatibleWeapons { get; set; }

        // Метод для расчета стоимости юнита с учетом количества моделей, уровня опыта, выбранного вооружения и улучшений
        public int CalculateCost(
    int modelCount,
    Dictionary<string, int> selectedWeapons,
    ExperienceLevelData experienceLevel,
    Dictionary<string, int> selectedUnitUpgrades,  // Изменено
    bool weaponUpgradeSelected)
        {
            // Базовая стоимость за минимальное количество моделей
            int totalCost = experienceLevel.BaseCost;

            // Добавляем стоимость за дополнительные модели
            if (modelCount > MinModels)
            {
                totalCost += (modelCount - MinModels) * experienceLevel.AdditionalModelCost;
            }

            // Добавляем стоимость за дополнительное вооружение, если оно есть
            if (Weapons != null) // Проверяем на null перед циклом
            {
                foreach (var weapon in Weapons)
                {
                    if (selectedWeapons.ContainsKey(weapon.Name))
                    {
                        int weaponCount = selectedWeapons[weapon.Name];
                        totalCost += weaponCount * weapon.Cost;

                        // Добавляем стоимость за улучшения оружия, если они выбраны
                        if (weaponUpgradeSelected)
                        {
                            if (weapon.Upgrades != null) // Проверяем на null для Upgrades
                            {
                                foreach (var upgrade in weapon.Upgrades)
                                {
                                    totalCost += weaponCount * upgrade.Cost; // Предполагаем, что улучшение применяется ко всему количеству оружия
                                }
                            }
                        }
                    }
                }
            }

            // Добавляем стоимость за улучшения юнита, если они выбраны
            if (selectedUnitUpgrades.Count != 0)  // Проверяем на null
            {
                foreach (var upgrade in Upgrade)  // Проходим по апгрейдам юнита
                {
                    if (selectedUnitUpgrades.ContainsKey(upgrade.Name))
                    {
                        int upgradeCount = selectedUnitUpgrades[upgrade.Name];  // Количество выбранных апгрейдов
                        totalCost += upgradeCount * upgrade.Cost;
                    }
                }
            }

            // Добавляем стоимость за правило Tough Fighters, если уровень опыта - Veteran и правило активно
            if (experienceLevel.Level == "Veteran" && experienceLevel.Upgrades != null)
            {
                totalCost += modelCount; // Увеличение стоимости на 1 за каждую модель
            }

            return totalCost;
        }



        public override string ToString()
        {
            return $"{Name}, Модели: {MinModels}-{MaxModels}, Опыт: {string.Join(", ", Experience.Select(e => $"{e.Level} (Base: {e.BaseCost}, Extra: {e.AdditionalModelCost})"))}";
        }
    }
}
