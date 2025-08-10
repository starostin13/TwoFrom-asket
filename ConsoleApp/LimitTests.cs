using System;
using System.Collections.Generic;
using System.Linq;
using UnitRosterGenerator;

// Simple ad-hoc test harness (no external test framework) to verify model-based limits.
// Run manually by invoking its Main from Program if desired.
namespace UnitRosterGeneratorTests
{
    class LimitTests
    {
        public static void Run()
        {
            // Arrange: unit with MinModels=1 MaxModels=2, limit MaxQuantity=2 (models total)
            var unit = new Unit
            {
                Name = "TestUnit",
                MinModels = 1,
                MaxModels = 2,
                Experience = new List<ExperienceLevelData>
                {
                    new ExperienceLevelData { Level = "Regular", BaseCost = 10, AdditionalModelCost = 5 }
                },
                DetachUpgrade = false
            };

            // Prepare limits: allow only 2 models total of TestUnit
            var limits = new UnitsLimits();
            limits.AddOrUpdateLimit("TestUnit", 2);

            // Simulate roster builder internal state via reflection / simplified logic
            var roster = new List<UnitConfiguration>();

            // First add 1-model config
            var cfg1 = new UnitConfiguration(unit, 1, unit.Experience.First(), new Dictionary<string,int>(), new Dictionary<string,int>(), false, null);
            roster.Add(cfg1);

            // Second attempt: add 1-model config (allowed, total 2 models)
            var cfg2 = new UnitConfiguration(unit, 1, unit.Experience.First(), new Dictionary<string,int>(), new Dictionary<string,int>(), false, null);
            roster.Add(cfg2);

            // Third attempt: try adding another (would exceed 2 models even with 1 model)
            int currentModels = roster.Where(c => c.Unit.Name == unit.Name).Sum(c => c.ModelCount);
            bool wouldExceed = currentModels + 1 > limits.GetMaxLimit(unit.Name);

            if (!wouldExceed)
            {
                throw new Exception("Limit test failed: third 1-model addition should exceed max models=2.");
            }

            // Also test adding a 2-model block directly after empty roster
            roster.Clear();
            var cfgFull = new UnitConfiguration(unit, 2, unit.Experience.First(), new Dictionary<string,int>(), new Dictionary<string,int>(), false, null);
            roster.Add(cfgFull);
            currentModels = roster.Where(c => c.Unit.Name == unit.Name).Sum(c => c.ModelCount);
            if (currentModels != 2)
            {
                throw new Exception("Limit test failed: two-model block should count as 2 models.");
            }

            Console.WriteLine("[LimitTests] Passed model-based MaxQuantity checks.");
        }
    }
}
