using System.Collections.Generic;

namespace UnitRosterGenerator
{
    class RandomRosterBuilder
    {
        private static Random random = new Random();

        // Метод для генерации случайного ростера
        public static void BuildRandomRoster(
            List<Unit> availableUnits, // Доступные юниты
            List<Detach> availableDetaches, // Доступные детачи
            int maxPoints, // Максимальное количество очков
            List<UnitConfiguration> currentRoster, // Текущий ростер (список конфигураций)
            List<List<UnitConfiguration>> allRosters) // Все ростеры
        {
            int currentPoints = 0;

            // Выбираем случайный детач для всего ростера
            Detach selectedDetach = ChooseRandomDetach(availableDetaches);

            while (true)
            {
                // Выбор случайного юнита
                var unit = GetRandomUnit(availableUnits);
                if (unit == null) break;

                // Выбор случайного уровня опыта
                var experienceLevel = GetRandomExperienceLevel(unit);
                if (experienceLevel == null) continue;

                // Случайное количество моделей
                int modelCount = random.Next(unit.MinModels, unit.MaxModels + 1);

                // Случайное вооружение
                var selectedWeapons = GetRandomWeapons(unit.Weapons);

                // Случайные апгрейды юнита
                var selectedUnitUpgrades = GetRandomUnitUpgrades(unit.Upgrade);

                // Если детач выбран, добавляем случайные апгрейды из него
                if (selectedDetach != null)
                {
                    var selectedDetachUpgrades = GetRandomDetachUpgrades(selectedDetach);

                    // Объединяем апгрейды юнита и детача
                    foreach (var upgrade in selectedDetachUpgrades)
                    {
                        if (selectedUnitUpgrades.ContainsKey(upgrade.Key))
                        {
                            selectedUnitUpgrades[upgrade.Key] += upgrade.Value;
                        }
                        else
                        {
                            selectedUnitUpgrades[upgrade.Key] = upgrade.Value;
                        }
                    }
                }

                // Создаем конфигурацию юнита, передавая детач в качестве аргумента
                var unitConfig = new UnitConfiguration(
                    unit, modelCount, experienceLevel, selectedWeapons, selectedUnitUpgrades, false, selectedDetach);

                // Проверка превышения максимальной стоимости
                if (currentPoints + unitConfig.TotalCost > maxPoints)
                {
                    break;
                }

                // Добавляем конфигурацию юнита в ростер
                currentRoster.Add(unitConfig);
                currentPoints += unitConfig.TotalCost;
            }

            // Добавляем текущий ростер в список всех возможных ростеров
            allRosters.Add(new List<UnitConfiguration>(currentRoster));
        }

        // Выбор случайного юнита
        private static Unit GetRandomUnit(List<Unit> availableUnits)
        {
            return availableUnits.Count > 0 ? availableUnits[random.Next(availableUnits.Count)] : null;
        }

        // Выбор случайного уровня опыта
        private static ExperienceLevelData GetRandomExperienceLevel(Unit unit)
        {
            return unit.Experience?.Count > 0 ? unit.Experience[random.Next(unit.Experience.Count)] : null;
        }

        // Выбор случайного набора оружия
        private static Dictionary<string, int> GetRandomWeapons(List<Weapon> weapons)
        {
            var selectedWeapons = new Dictionary<string, int>();

            if (weapons != null && weapons.Count > 0)
            {
                foreach (var weapon in weapons)
                {
                    if (random.Next(0, 2) == 1)
                    {
                        int weaponCount = random.Next(weapon.MinCount, weapon.MaxCount + 1);
                        selectedWeapons[weapon.Name] = weaponCount;

                        if (weapon.Upgrades != null)
                        {
                            foreach (var upgrade in weapon.Upgrades)
                            {
                                if (random.Next(0, 2) == 1)
                                {
                                    selectedWeapons[upgrade.Name] = 1;
                                }
                            }
                        }
                    }
                }
            }

            return selectedWeapons;
        }

        // Выбор случайных апгрейдов для юнита
        private static Dictionary<string, int> GetRandomUnitUpgrades(List<Upgrade> upgrades)
        {
            var selectedUpgrades = new Dictionary<string, int>();

            if (upgrades != null)
            {
                foreach (var upgrade in upgrades)
                {
                    int upgradeCount = random.Next(upgrade.MinCount, upgrade.MaxCount + 1);
                    if (upgradeCount > 0)
                    {
                        selectedUpgrades[upgrade.Name] = upgradeCount;
                    }
                }
            }

            return selectedUpgrades;
        }

        // Выбор случайных апгрейдов из детача
        private static Dictionary<string, int> GetRandomDetachUpgrades(Detach detach)
        {
            var selectedUpgrades = new Dictionary<string, int>();

            foreach (var upgrade in detach.Upgrades)
            {
                if (random.Next(0, 2) == 1)
                {
                    int upgradeCount = random.Next(upgrade.MinCount, upgrade.MaxCount + 1);
                    if (upgradeCount > 0)
                    {
                        selectedUpgrades[upgrade.Name] = upgradeCount;
                    }
                }
            }

            return selectedUpgrades;
        }

        // Выбор случайного детача
        private static Detach ChooseRandomDetach(List<Detach> detaches)
        {
            return detaches.Count > 0 ? detaches[random.Next(detaches.Count)] : null;
        }
    }
}
