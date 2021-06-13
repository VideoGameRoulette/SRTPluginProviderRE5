﻿using System;
using SRTPluginProviderRE5.Structs;
using SRTPluginProviderRE5.Structs.GameStructs;

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

        // Chris DA
        short ChrisDA { get; set; }

        // Chris DA Rank
        short ChrisDARank { get; set; }

        // Sheva DA
        short ShevaDA { get; set; }

        // Sheva DA Rank
        short ShevaDARank { get; set; }

        // Chapter
        public int Chapter { get; set; }

        // Shots Fired
        int ShotsFired { get; set; }

        // Enemies Hit
        int EnemiesHit { get; set; }

        // Deaths
        int Deaths { get; set; }

        // IGT
        float IGT { get; set; }

        // Shots Fired 2
        int ShotsFired2 { get; set; }

        // Enemies Hit 2
        int EnemiesHit2 { get; set; }

        // Deaths 2
        int Deaths2 { get; set; }

        // IGT 2
        float IGT2 { get; set; }

        // Versioninfo
        string VersionInfo { get; }

        // GameInfo
        string GameInfo { get; set; }
    }
}