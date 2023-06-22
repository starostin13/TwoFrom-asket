using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmyGeneratorMaui
{
    public static class Core
    {
        private static Faction mainFaction;

        internal static Faction MainFaction { get => mainFaction; set => mainFaction = value; }

        internal static void SetCurrentFaction(Faction faction)
        {
            MainFaction = faction;
        }
    }
}
