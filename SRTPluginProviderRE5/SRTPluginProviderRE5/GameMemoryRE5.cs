using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SRTPluginProviderRE5.Structs;
using SRTPluginProviderRE5.Structs.GameStructs;

namespace SRTPluginProviderRE5
{
    public class GameMemoryRE5 : IGameMemoryRE5
    {
        public short PlayerCurrentHealth { get => _playerCurrentHealth; set => _playerCurrentHealth = value; }
        internal short _playerCurrentHealth;

        public short PlayerMaxHealth { get => _playerMaxHealth; set => _playerMaxHealth = value; }
        internal short _playerMaxHealth;

        public EnemyHP[] EnemyHealth { get => _enemyHealth; set => _enemyHealth = value; }
        internal EnemyHP[] _enemyHealth;
    }
}
