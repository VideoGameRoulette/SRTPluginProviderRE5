using System;
using SRTPluginProviderRE5.Structs;

namespace SRTPluginProviderRE5
{
    public interface IGameMemoryRE5
    {
        // Chris HP
        short PlayerCurrentHealth { get; set; }
        short PlayerMaxHealth { get; set; }

        // Sheva HP
        short PlayerCurrentHealth2 { get; set; }
        short PlayerMaxHealth2 { get; set; }

        // Money
        int Money { get; set; }

        // Kills Chris
        int ChrisKills { get; set; }

        // Kills Sheva
        int ShevaKills { get; set; }

    }
}
