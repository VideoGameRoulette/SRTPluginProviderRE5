using System;
using SRTPluginProviderRE5.Structs;

namespace SRTPluginProviderRE5
{
    public interface IGameMemoryRE5
    {
        short PlayerCurrentHealth { get; set; }
        short PlayerMaxHealth { get; set; }
    }
}