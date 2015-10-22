// RAMAccess.cs
// Copyright 2015
// 
// Project Lead: Need
// Contact:      
//     Mail:     mailto:needdragon@gmail.com 
//     Twitter: https://twitter.com/NeedDragon

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Need_Utilities.Util.RAM {
    public static class RAMAccess {
        public static IntPtr OpenProcess(Process process) {
            IntPtr processPointer = Kernel32Import.OpenProcess(Kernel32Import.PROCESS_QUERY_INFORMATION | 
                Kernel32Import.PROCESS_VM_OPERATION | 
                Kernel32Import.PROCESS_WM_READ | 
                Kernel32Import.PROCESS_VM_WRITE, false, process.Id
            );
            return processPointer;
        }

        /// <summary>
        /// Reserves amountbytes bytes in process pointed by processPointer
        /// </summary>
        /// <param name="processPointer">A pointer to a process, in which the memory region should be reserved</param>
        /// <param name="amountBytes">The amount of bytes to reserve</param>
        /// <returns>The pointer to the start of the reserved memory region, or IntPtr.Zero if memory region could not be reserved</returns>
        public static IntPtr Memory_ReserveMemoryRegion(IntPtr processPointer, int amountBytes) {
            IntPtr reservedCodeCaveMemory = Kernel32Import.VirtualAllocEx(processPointer, IntPtr.Zero, amountBytes, Kernel32Import.MEM_RESERVE | Kernel32Import.MEM_COMMIT, Kernel32Import.PAGE_EXECUTE_READWRITE);
            
            return reservedCodeCaveMemory;
        }
        /// <summary>
        /// Tries to free the specified memory region in the specified process (pointed by the pointer)
        /// </summary>
        /// <param name="processPointer">The pointer to the process</param>
        /// <param name="memRegionPointer">The pointer to the memory region</param>
        /// <returns>A error message if function fails to free the memory region or an empty string, if function sucessfully released the region.</returns>
        public static string Memory_FreeMemory(IntPtr processPointer, IntPtr memRegionPointer) {
            if(memRegionPointer == IntPtr.Zero)  return "Could not free memory region! (Memory region is invalid)";
            if(processPointer == IntPtr.Zero) return "Could not free memory region! (Invalid process)";

            int success = Kernel32Import.VirtualFreeEx(processPointer, memRegionPointer, 0, Kernel32Import.MEM_RELEASE);
            if(success==0) return "Could not release reserved memory in target process. Probable Memory-Leak is occuring! (Error: " + new Win32Exception(Kernel32Import.GetLastError()).Message + ")";
            return "";
        }

        /// <summary>
        /// Writes the specified hex string to the process pointed by processPointer at location memoryLocation
        /// </summary>
        /// <param name="processPointer">The pointer to the process</param>
        /// <param name="memoryLocation">The pointer to the memory location</param>
        /// <param name="hexString">The bytes to write</param>
        /// <returns>An error message if function fails, else an empty string</returns>
        public static string Memory_WriteProcessMemory(IntPtr processPointer, IntPtr memoryLocation, string hexString) {
            byte[] byteArray = ByteTransformHelper.Transform_HexStringTOByteArray(hexString);
            return Memory_WriteProcessMemory(processPointer, memoryLocation, byteArray);
        }

        /// <summary>
        /// Writes the specified bytes to the process pointed by processPointer at location memoryLocation
        /// </summary>
        /// <param name="processPointer">The pointer to the process to write into</param>
        /// <param name="memoryLocation">The pointer to the memory location</param>
        /// <param name="bytes">The bytes to write</param>
        /// <returns>An error message if function fails, else an empty string</returns>
        public static string Memory_WriteProcessMemory(IntPtr processPointer, IntPtr memoryLocation, byte[] bytes) {
            if (memoryLocation == IntPtr.Zero) return "Could not write to memory region! (Memory region is invalid)";
            if (processPointer == IntPtr.Zero) return "Could not write to memory region! (Invalid process)";

            IntPtr bytesWritten = new IntPtr(0);
            bool n = Kernel32Import.WriteProcessMemory(processPointer, memoryLocation, bytes, bytes.Length, out bytesWritten);
            if (!n) {
                Memory_FreeMemory(processPointer, memoryLocation);
                return "Failed to write \"" + ByteTransformHelper.Transform_ByteArrayTOHexString(bytes) + "\" to memory location \"" + memoryLocation + "\"!";
            }
            return "";
        }

        /// <summary>
        /// Reads and returns "size" amount of bytes in process pointed by processPointer at location memoryLocation
        /// </summary>
        /// <param name="processPointer">The pointer to the process to read from</param>
        /// <param name="memoryLocation">The pointer to the memory location to read from</param>
        /// <param name="size">The amount of bytes to read. The returned array will have this size</param>
        /// <returns>The bytes read from the process or null, if function fails</returns>
        public static byte[] Memory_ReadProcessMemory(IntPtr processPointer, IntPtr memoryLocation, int size) {
            if (size <= 0) return null;
            byte[] bytes = new byte[size];
            int bytesRead = 0;
            int n = Kernel32Import.ReadProcessMemory(processPointer, memoryLocation, bytes, size, ref bytesRead);
            if(n == 0 || bytesRead == 0) return null;
            return bytes;
        } 
    }
}