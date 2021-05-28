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

        // Pointer Classes
        private IntPtr BaseAddress { get; set; }
        private MultilevelPointer PointerPlayerHP { get; set; }
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

            //if (!SelectPointerAddresses(GameHashes.DetectVersion(process.MainModule.FileName)))
            //    return; // Unknown version.

            int pid = GetProcessId(process).Value;
            memoryAccess = new ProcessMemoryHandler(pid);
            if (ProcessRunning)
            {
                BaseAddress = NativeWrappers.GetProcessBaseAddress(pid, PInvoke.ListModules.LIST_MODULES_64BIT); // Bypass .NET's managed solution for getting this and attempt to get this info ourselves via PInvoke since some users are getting 299 PARTIAL COPY when they seemingly shouldn't.

                // Setup the pointers.
                PointerPlayerHP = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressHP), 0x24);

                PointerEnemyHP = new MultilevelPointer[20];
                for (int i = 0; i < PointerEnemyHP.Length; ++i)
                    PointerEnemyHP[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemyHP), 0x30, 0x0 + (i * 0x04), 0x8);

                GenerateEnemyEntries();
            }
        }

        private void SelectPointerAddresses()
        {
            pointerAddressHP = 0x00DA2A5C;
            pointerAddressEnemyHP = 0x00DA224C;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        private unsafe void GenerateEnemyEntries()
        {
            if (PointerEnemyHP == null) // Enter if the pointer table is null (first run) or the size does not match.
            {
                PointerEnemyHP = new MultilevelPointer[15]; // Create a new enemy pointer table array with the detected size.
                for (int i = 0; i < PointerEnemyHP.Length; ++i) // Loop through and create all of the pointers for the table.
                    PointerEnemyHP[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemyHP), 0x30, 0x0 + (i * 0x04), 0x8);
            }
        }

        internal void UpdatePointers()
        {
            PointerPlayerHP.UpdatePointers();
            GenerateEnemyEntries(); // This has to be here for the next part.
            for (int i = 0; i < PointerEnemyHP.Length; ++i)
                PointerEnemyHP[i].UpdatePointers();
        }

        internal unsafe IGameMemoryRE5 Refresh()
        {
            bool success;

            // Player HP
            if (SafeReadByteArray(PointerPlayerHP.Address, sizeof(GamePlayerHP), out byte[] gamePlayerHpBytes))
            {
                var playerHp = GamePlayerHP.AsStruct(gamePlayerHpBytes);
                gameMemoryValues._playerMaxHealth = playerHp.Max;
                gameMemoryValues._playerCurrentHealth = playerHp.Current;
            }

            // Enemy HP
            GenerateEnemyEntries();
            if (gameMemoryValues._enemyHealth == null)
            {
                gameMemoryValues._enemyHealth = new EnemyHP[15];
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