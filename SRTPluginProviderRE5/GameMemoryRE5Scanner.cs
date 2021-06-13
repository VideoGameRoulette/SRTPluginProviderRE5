using ProcessMemory;
using static ProcessMemory.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SRTPluginProviderRE5.Structs;
using System.Text;
using SRTPluginProviderRE5.Structs.GameStructs;

namespace SRTPluginProviderRE5
{
    internal class GameMemoryRE5Scanner : IDisposable
    {
        private static readonly int MAX_ENTITIES = 64;
        private static readonly int MAX_ITEMS = 256;

        // Variables
        private ProcessMemoryHandler memoryAccess;
        private GameMemoryRE5 gameMemoryValues;
        public bool HasScanned;
        public bool ProcessRunning => memoryAccess != null && memoryAccess.ProcessRunning;
        public int ProcessExitCode => (memoryAccess != null) ? memoryAccess.ProcessExitCode : 0;

        // Pointer Address Variables
        private int pointerAddressHP;
        private int pointerAddressEnemyHP;
        private int pointerAddressMoney;
        private int pointerAddressKills;
        private int pointerAddressDA;
        private int pointerAddressChapter;
        private int pointerAddressEndOfChapter;
        private int pointerAddressIGT;
        private int pointerAddressItemSlots;

        // Pointer Classes
        private IntPtr BaseAddress { get; set; }
        private MultilevelPointer PointerPlayerHP { get; set; }
        private MultilevelPointer PointerPlayerHP2 { get; set; }
        private MultilevelPointer PointerMoney { get; set; }
        private MultilevelPointer PointerKillsChris { get; set; }
        private MultilevelPointer PointerKillsSheva { get; set; }
        private MultilevelPointer PointerChrisDA { get; set; }
        private MultilevelPointer PointerChrisDARank { get; set; }
        private MultilevelPointer PointerShevaDA { get; set; }
        private MultilevelPointer PointerShevaDARank { get; set; }
        private MultilevelPointer PointerChapter { get; set; }
        private MultilevelPointer PointerShotsFired { get; set; }
        private MultilevelPointer PointerEnemiesHit { get; set; }
        private MultilevelPointer PointerDeaths { get; set; }
        private MultilevelPointer PointerIGT { get; set; }
        private MultilevelPointer[] PointerItemSlots { get; set; }

        private MultilevelPointer[] PointerEnemyHP { get; set; }
        
        internal GameMemoryRE5Scanner(Process process = null)
        {
            gameMemoryValues = new GameMemoryRE5();
            if (process != null)
                Initialize(process);
        }

        internal unsafe void Initialize(Process process)
        {
            if (process == null)
                return; // Do not continue if this is null.

            SelectPointerAddresses();
            gameMemoryValues._gameInfo = GameHashes.DetectVersion(process.MainModule.FileName);

            //if (!SelectPointerAddresses(GameHashes.DetectVersion(process.MainModule.FileName)))
            //    return; // Unknown version.

            int pid = GetProcessId(process).Value;
            memoryAccess = new ProcessMemoryHandler(pid);
            if (ProcessRunning)
            {
                BaseAddress = NativeWrappers.GetProcessBaseAddress(pid, PInvoke.ListModules.LIST_MODULES_64BIT); // Bypass .NET's managed solution for getting this and attempt to get this info ourselves via PInvoke since some users are getting 299 PARTIAL COPY when they seemingly shouldn't.

                // Setup the pointers.
                PointerPlayerHP = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressHP), 0x24);
                PointerPlayerHP2 = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressHP), 0x28);
                PointerMoney = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMoney));
                PointerKillsChris = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressKills));
                PointerKillsSheva = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressKills));
                PointerChrisDA = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressDA), 0x6C, 0x4C, 0x2D1C);
                PointerChrisDARank = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressDA), 0x6C, 0x4C, 0x2D1C);
                PointerShevaDA = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressDA), 0x6C, 0x4C, 0x2D1C);
                PointerShevaDARank = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressDA), 0x6C, 0x4C, 0x2D1C);
                PointerChapter = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressChapter));
                PointerShotsFired = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEndOfChapter));
                PointerEnemiesHit = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEndOfChapter));
                PointerDeaths = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEndOfChapter));
                PointerIGT = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressIGT));

                PointerEnemyHP = new MultilevelPointer[32];
                for (int i = 0; i < PointerEnemyHP.Length; ++i)
                    PointerEnemyHP[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemyHP), 0x140, 0x50, 0x0 + (i * 0x04));

                GenerateEnemyEntries();
            }
        }

        private void SelectPointerAddresses()
        {
            pointerAddressHP = 0x00DA2A5C;
            pointerAddressEnemyHP = 0x00DA224C;
            pointerAddressMoney = 0x00DA23D8;
            pointerAddressKills = 0x00DA23D8;
            pointerAddressDA = 0x00E2487C;
            pointerAddressChapter = 0xDA23D8;
            pointerAddressEndOfChapter = 0xDA23D8;
            pointerAddressIGT = 0xDA23D8;
            pointerAddressItemSlots = 0x00DA25B4;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        private unsafe void GenerateEnemyEntries()
        {
            if (PointerEnemyHP == null) // Enter if the pointer table is null (first run) or the size does not match.
            {
                PointerEnemyHP = new MultilevelPointer[32]; // Create a new enemy pointer table array with the detected size.
                for (int i = 0; i < PointerEnemyHP.Length; ++i) // Loop through and create all of the pointers for the table.
                    PointerEnemyHP[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemyHP), 0x140, 0x50, 0x0 + (i * 0x04));
            }
        }

        internal void UpdatePointers()
        {
            PointerPlayerHP.UpdatePointers();
            PointerPlayerHP2.UpdatePointers();
            PointerMoney.UpdatePointers();
            PointerKillsChris.UpdatePointers();
            PointerKillsSheva.UpdatePointers();
            PointerChrisDA.UpdatePointers();
            PointerChrisDARank.UpdatePointers();
            PointerShevaDA.UpdatePointers();
            PointerShevaDARank.UpdatePointers();
            PointerChapter.UpdatePointers();
            PointerShotsFired.UpdatePointers();
            PointerEnemiesHit.UpdatePointers();
            PointerDeaths.UpdatePointers();
            PointerIGT.UpdatePointers();


            GenerateEnemyEntries(); // This has to be here for the next part.
            for (int i = 0; i < PointerEnemyHP.Length; ++i)
                PointerEnemyHP[i].UpdatePointers();
        }

        internal unsafe IGameMemoryRE5 Refresh()
        {
            bool success;

            // Chris HP
            if (SafeReadByteArray(PointerPlayerHP.Address, sizeof(GamePlayerHP), out byte[] gamePlayerHpBytes))
            {
                var playerHp = GamePlayerHP.AsStruct(gamePlayerHpBytes);
                gameMemoryValues._playerMaxHealth = playerHp.Max;
                gameMemoryValues._playerCurrentHealth = playerHp.Current;
            }
            // Sheva HP
            if (SafeReadByteArray(PointerPlayerHP2.Address, sizeof(GamePlayerHP), out byte[] gamePlayerHpBytes2))
            {
                var playerHp2 = GamePlayerHP.AsStruct(gamePlayerHpBytes2);
                gameMemoryValues._playerMaxHealth2 = playerHp2.Max;
                gameMemoryValues._playerCurrentHealth2 = playerHp2.Current;
            }

            // Money
            fixed (int* p = &gameMemoryValues._money)
                success = PointerMoney.TryDerefInt(0x1C0, p);
                
            // Kills Chris
            fixed (int* p = &gameMemoryValues._chrisKills)
                success = PointerKillsChris.TryDerefInt(0x273EC, p);

            // Kills Sheva
            fixed (int* p = &gameMemoryValues._shevaKills)
                success = PointerKillsSheva.TryDerefInt(0x27404, p);

            // Chris DA
            fixed (short* p = &gameMemoryValues._chrisDA)
                success = PointerChrisDA.TryDerefShort(0x388, p);

            // Chris DA Rank
            fixed (short* p = &gameMemoryValues._chrisDARank)
                success = PointerChrisDARank.TryDerefShort(0x37C, p);

            // Sheva DA
            fixed (short* p = &gameMemoryValues._shevaDA)
                success = PointerShevaDA.TryDerefShort(0x3B4, p);

            // Sheva DA Rank
            fixed (short* p = &gameMemoryValues._shevaDARank)
                success = PointerShevaDARank.TryDerefShort(0x3A8, p);

            // Chapter
            fixed (int* p = &gameMemoryValues._chapter)
                success = PointerChapter.TryDerefInt(0x273D0, p);

            // Shots Fired
            fixed (int* p = &gameMemoryValues._shotsfired)
                success = PointerShotsFired.TryDerefInt(0x273F0, p);

            // Enemies Hit
            fixed (int* p = &gameMemoryValues._enemiesHit)
                success = PointerShotsFired.TryDerefInt(0x273F4, p);

            // Deaths
            fixed (int* p = &gameMemoryValues._deaths)
                success = PointerShotsFired.TryDerefInt(0x273E8, p);

            // IGT
            fixed (float* p = &gameMemoryValues._igt)
                success = PointerIGT.TryDerefFloat(0x273F8, p);

            // Shots Fired 2
            fixed (int* p = &gameMemoryValues._shotsfired2)
                success = PointerShotsFired.TryDerefInt(0x27408, p);

            // Enemies Hit 2
            fixed (int* p = &gameMemoryValues._enemiesHit2)
                success = PointerShotsFired.TryDerefInt(0x2740C, p);

            // Deaths 2
            fixed (int* p = &gameMemoryValues._deaths2)
                success = PointerShotsFired.TryDerefInt(0x27400, p);

            // IGT 2
            fixed (float* p = &gameMemoryValues._igt2)
                success = PointerShotsFired.TryDerefFloat(0x27410, p);

            // Enemy HP
            GenerateEnemyEntries();
            if (gameMemoryValues._enemyHealth == null)
            {
                gameMemoryValues._enemyHealth = new EnemyHP[32];
                for (int i = 0; i < gameMemoryValues._enemyHealth.Length; ++i)
                    gameMemoryValues._enemyHealth[i] = new EnemyHP();
            }
            for (int i = 0; i < gameMemoryValues._enemyHealth.Length; ++i)
            {
                try
                {
                    // Check to see if the pointer is currently valid. It can become invalid when rooms are changed.
                    if (PointerEnemyHP[i].Address != IntPtr.Zero)
                    {
                        if (i > 0 && PointerEnemyHP[i].Address != PointerEnemyHP[i - 1].Address)
                        {
                            fixed (short* p = &gameMemoryValues.EnemyHealth[i]._maximumHP)
                                PointerEnemyHP[i].TryDerefShort(0x1366, p);
                            fixed (short* p = &gameMemoryValues.EnemyHealth[i]._currentHP)
                                PointerEnemyHP[i].TryDerefShort(0x1364, p);


                        } else if(i == 0)
                        {
                            fixed (short* p = &gameMemoryValues.EnemyHealth[i]._maximumHP)
                                PointerEnemyHP[i].TryDerefShort(0x1366, p);
                            fixed (short* p = &gameMemoryValues.EnemyHealth[i]._currentHP)
                                PointerEnemyHP[i].TryDerefShort(0x1364, p);
                        }
                        else
                        {
                            // Clear these values out so stale data isn't left behind when the pointer address is no longer value and nothing valid gets read.
                            // This happens when the game removes pointers from the table (map/room change).
                            gameMemoryValues.EnemyHealth[i]._maximumHP = 0;
                            gameMemoryValues.EnemyHealth[i]._currentHP = 0;
                        }
                    }
                    else
                    {
                        // Clear these values out so stale data isn't left behind when the pointer address is no longer value and nothing valid gets read.
                        // This happens when the game removes pointers from the table (map/room change).
                        gameMemoryValues.EnemyHealth[i]._maximumHP = 0;
                        gameMemoryValues.EnemyHealth[i]._currentHP = 0;
                    }
                }
                catch
                {
                    gameMemoryValues.EnemyHealth[i]._maximumHP = 0;
                    gameMemoryValues.EnemyHealth[i]._currentHP = 0;
                }
            }

            HasScanned = true;
            return gameMemoryValues;
        }

        private int? GetProcessId(Process process) => process?.Id;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        private unsafe bool SafeReadByteArray(IntPtr address, int size, out byte[] readBytes)
        {
            readBytes = new byte[size];
            fixed (byte* p = readBytes)
            {
                return memoryAccess.TryGetByteArrayAt(address, size, p);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (memoryAccess != null)
                        memoryAccess.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~REmake1Memory() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}