using System;
using System.Collections.Generic;
using UnitRosterGenerator;

class RandomRosterBuilder
{
    private static Random random = new Random();

    // Метод для генерации случайного ростера
    public static void BuildRandomRoster(
        List<Unit> availableUnits, // Доступные юниты
        int maxPoints, // Максимальное количество очков
        List<UnitConfiguration> currentRoster, // Текущий ростер (список конфигураций)
        List<List<UnitConfiguration>> allRosters) // Все ростеры
    {
        int currentPoints = 0;

        // Продолжаем заполнять ростер, пока есть достаточно очков для хотя бы одного юнита
        while (true)
        {
            // Получаем случайного юнита
            var unit = GetRandomUnit(availableUnits);
            if (unit == null) break; // Если нет доступных юнитов

            // Выбираем случайный уровень опыта
            var experienceLevel = GetRandomExperienceLevel(unit);
            if (experienceLevel == null) continue;

            // Случайное количество моделей для этого юнита
            int modelCount = random.Next(unit.MinModels, unit.MaxModels + 1);

            // Случайное вооружение и апгрейды
            var selectedWeapons = GetRandomWeapons(unit.Weapons);

            // Определяем, выбраны ли улучшения
            bool upgradeSelected = unit.Upgrades != null && random.Next(0, 2) == 1;

            // Создаем конфигурацию юнита
            var unitConfig = new UnitConfiguration(unit, modelCount, experienceLevel, selectedWeapons, upgradeSelected, false);

            // Если добавление этого юнита не превышает доступные очки
            if (currentPoints + unitConfig.TotalCost > maxPoints)
            {
                // Если текущих очков уже недостаточно для добавления юнита, заканчиваем
                break;
            }

            // Добавляем конфигурацию юнита в текущий ростер
            currentRoster.Add(unitConfig);

            // Увеличиваем текущие очки
            currentPoints += unitConfig.TotalCost;
        }

        // Добавляем текущий ростер в список всех возможных ростеров
        allRosters.Add(new List<UnitConfiguration>(currentRoster));
    }

    // Метод для получения случайного юнита
    private static Unit GetRandomUnit(List<Unit> availableUnits)
    {
        if (availableUnits == null || availableUnits.Count == 0)
        {
            return null;
        }

        return availableUnits[random.Next(availableUnits.Count)];
    }

    // Метод для получения случайного уровня опыта для юнита
    private static ExperienceLevelData GetRandomExperienceLevel(Unit unit)
    {
        if (unit.Experience == null || unit.Experience.Count == 0)
        {
            return null;
        }

        return unit.Experience[random.Next(unit.Experience.Count)];
    }

    // Метод для получения случайного набора оружия
    private static Dictionary<string, int> GetRandomWeapons(List<Weapon> weapons)
    {
        var selectedWeapons = new Dictionary<string, int>();

        if (weapons != null && weapons.Count > 0)
        {
            foreach (var weapon in weapons)
            {
                // Случайно решаем, будет ли выбрано оружие
                if (random.Next(0, 2) == 1)
                {
                    // Выбираем случайное количество оружия в допустимом диапазоне
                    int weaponCount = random.Next(weapon.MinCount, weapon.MaxCount + 1);
                    selectedWeapons[weapon.Name] = weaponCount;

                    // Если у оружия есть апгрейды, случайно выбираем их
                    if (weapon.Upgrades != null)
                    {
                        foreach (var upgrade in weapon.Upgrades)
                        {
                            // Случайно решаем, будет ли выбрано улучшение
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
}
