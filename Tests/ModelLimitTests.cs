using System.Collections.Generic;
using System.Linq;
using UnitRosterGenerator;
using Xunit;

namespace UnitRosterGenerator.Tests;

public class ModelLimitTests
{
    [Fact]
    public void MaxQuantity_AppliesToTotalModels_NotUnitInstances()
    {
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

        var limits = new UnitsLimits();
        limits.AddOrUpdateLimit("TestUnit", 2);

        // Simulate adding two 1-model configs
        var rosterConfigs = new List<UnitConfiguration>
        {
            new UnitConfiguration(unit, null, 1, unit.Experience.First(), new Dictionary<string,int>(), new Dictionary<string,int>(), false, null),
            new UnitConfiguration(unit, null, 1, unit.Experience.First(), new Dictionary<string,int>(), new Dictionary<string,int>(), false, null)
        };

        int currentModels = rosterConfigs.Where(c => c.Unit.Name == unit.Name).Sum(c => c.ModelCount);
        Assert.Equal(2, currentModels);

        // Attempt to add one more model would exceed
        bool wouldExceed = currentModels + 1 > limits.GetMaxLimit(unit.Name);
        Assert.True(wouldExceed);

        // Directly adding a 2-model block to empty roster should be allowed
        rosterConfigs.Clear();
    var full = new UnitConfiguration(unit, null, 2, unit.Experience.First(), new Dictionary<string,int>(), new Dictionary<string,int>(), false, null);
        rosterConfigs.Add(full);
        Assert.Equal(2, rosterConfigs.Sum(c => c.ModelCount));
    }
}
