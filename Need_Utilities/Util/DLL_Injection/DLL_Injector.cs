// DLL_Injector.cs
// Copyright 2015
// 
// Project Lead: Need
// Contact:      
//     Mail:     mailto:needdragon@gmail.com 
//     Twitter: https://twitter.com/NeedDragon

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Need_Utilities.Util.RAM;

namespace Need_Utilities.Util.DLL_Injection {
    public static class DLL_Injector {
        public delegate int LoadLibA(IntPtr param);
        static int MyThreadProc(IntPtr param) {
            int pid = Process.GetCurrentProcess().Id;
            Console.WriteLine("Pid {0}: Inside my new thread!. Param={1}", pid, param.ToInt32());
            return 1;
        }

        public static string InjectDLL(string pExe_name, string pDLL_location) {
            Process[] processes = Process.GetProcessesByName(pExe_name);
            if(processes.Length == 1) {
                IntPtr processPointer = Kernel32Import.OpenProcess(Kernel32Import.PROCESS_QUERY_INFORMATION | 
                    Kernel32Import.PROCESS_VM_OPERATION | 
                    Kernel32Import.PROCESS_WM_READ | 
                    Kernel32Import.PROCESS_VM_WRITE, false, processes[0].Id
                );
                if(processPointer == IntPtr.Zero) return "Could not access process. Please try again as admin!";
                return InjectDLL(processPointer, pDLL_location);
            }
            return "There are "+processes.Length+" different processes named \""+pExe_name+"\"!";
        }
        private static string InjectDLL(IntPtr processPointer, string pDLL_location) {
            IntPtr resMemRegion = RAMAccess.Memory_ReserveMemoryRegion(processPointer, pDLL_location.Length);
            if (resMemRegion == IntPtr.Zero) {
                return "Injector| Failed to reserve memory in "+processPointer+" for \""+pDLL_location+"\"";
            }


            IntPtr loadLibAddr = Kernel32Import.GetProcAddress(Kernel32Import.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if(loadLibAddr == IntPtr.Zero) {
                string ret = "Injector| Failed to get address of LoadLibraryA!";
                string freeSuccess = RAMAccess.Memory_FreeMemory(processPointer, resMemRegion);
                if(!"".Equals(freeSuccess)) ret+=" Failed to free reserved memory. (MEMORY LEAK!)";
                return ret;
            }
            
            string writeSuccess = RAMAccess.Memory_WriteProcessMemory(processPointer, resMemRegion, ByteTransformHelper.Transform_StringTOByteArray(pDLL_location));
            if (!"".Equals(writeSuccess)) {
                string ret = "Injector| Failed to write dll location \"" + pDLL_location + "\" to process " + processPointer + ". Trying to free reserved memory...";
                string freeSuccess = RAMAccess.Memory_FreeMemory(processPointer, resMemRegion);
                if (!"".Equals(freeSuccess)) ret += " Failed to free reserved memory. (MEMORY LEAK!)";
                return ret;
            }

            InjectDLLIntoProcess(processPointer, loadLibAddr, resMemRegion);

            RAMAccess.Memory_FreeMemory(processPointer, resMemRegion);
            Kernel32Import.CloseHandle(processPointer);
            return "";
        }

        private static string InjectDLLIntoProcess(IntPtr processPointer, IntPtr loadLibraryAddr, IntPtr arg) {
	        //                                 process  security    stacksize   start address                   parameter   creationFlags   ThreadId
            LoadLibA libA = MyThreadProc;
            IntPtr fpProc = Marshal.GetFunctionPointerForDelegate(libA);

            IntPtr thread = Kernel32Import.CreateRemoteThread(processPointer, IntPtr.Zero, 0, loadLibraryAddr, arg, 0, IntPtr.Zero);
            if(thread == IntPtr.Zero) {
                string ret = "Injector| Failed to create remote thread!";
                string freeSuccess = RAMAccess.Memory_FreeMemory(processPointer, arg);
                if(!"".Equals(freeSuccess)) ret += " Failed to free reserved memory. (MEMORY LEAK!)";
                return ret;
	        }
            Kernel32Import.WaitForSingleObject(thread, Kernel32Import.INFINITE);
            return "";
        }
    }
}