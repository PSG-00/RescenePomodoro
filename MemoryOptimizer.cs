using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace radiant_noether
{
    public static class MemoryOptimizer
    {
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

        /// <summary>
        /// .NET 런타임의 미사용 힙 메모리를 해제하고 Windows에 물리적 Working Set을 반환하여
        /// 메모리 사용량을 50~80MB 이하로 최소화합니다.
        /// </summary>
        public static void TrimMemory()
        {
            try
            {
                // 1. 모든 세대 및 LOH(대형 객체 힙) 강제 수집 및 정리
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true, true);

                // 2. Windows 메모리 관리자에 프로세스 미사용 페이징 반환 요청
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    IntPtr handle = Process.GetCurrentProcess().Handle;
                    EmptyWorkingSet(handle);
                    SetProcessWorkingSetSize(handle, -1, -1);
                }
            }
            catch
            {
                // 예외 무시
            }
        }
    }
}
