using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SRTPluginProviderRE5.Structs;
using SRTPluginProviderRE5.Structs.GameStructs;

namespace SRTPluginProviderRE5
{
    public class GameMemoryRE5 : IGameMemoryRE5
    {   // Chris HP
        public short PlayerCurrentHealth { get => _playerCurrentHealth; set => _playerCurrentHealth = value; }
        internal short _playerCurrentHealth;

        public short PlayerMaxHealth { get => _playerMaxHealth; set => _playerMaxHealth = value; }
        internal short _playerMaxHealth;
        // Sheva HP
        public short PlayerCurrentHealth2 { get => _playerCurrentHealth2; set => _playerCurrentHealth2 = value; }
        internal short _playerCurrentHealth2;

        public short PlayerMaxHealth2 { get => _playerMaxHealth2; set => _playerMaxHealth2 = value; }
        internal short _playerMaxHealth2;

        // Money
        public int Money { get => _money; set => _money = value; }
        internal int _money;

        // Kills Chris
        public int ChrisKills { get => _chrisKills; set => _chrisKills = value; }
        internal int _chrisKills;

        // Kills Sheva
        public int ShevaKills { get => _shevaKills; set => _shevaKills = value; }
        internal int _shevaKills;

        // Enemy HP
        public EnemyHP[] EnemyHealth { get => _enemyHealth; set => _enemyHealth = value; }
        internal EnemyHP[] _enemyHealth;
    }
}
