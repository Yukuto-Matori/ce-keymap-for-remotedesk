using System.Runtime.InteropServices;
using System.Threading;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Emulates key presses via SendInput for the features that act like a keyboard
    /// shortcut the remote host would otherwise receive (window switch, Win key).
    /// </summary>
    internal sealed class InputSimulator
    {
        private const int InputKeyboard = 1;
        private const uint KeyEventFKeyUp = 0x0002;
        private const ushort VkMenu = 0x12;   // Alt (generic)
        private const ushort VkTab = 0x09;
        private const ushort VkLWin = 0x5B;

        public void SimulateAppWindowSwitch()
        {
            SendKeyDown(VkMenu);
            SendKeyDown(VkTab);
            SendKeyUp(VkTab);
            Thread.Sleep(50);
            SendKeyUp(VkMenu);
        }

        public void SimulateWinKeyPress()
        {
            SendKeyDown(VkLWin);
            SendKeyUp(VkLWin);
        }

        private static void SendKeyDown(ushort vk) => SendKeyInput(vk, keyUp: false);

        private static void SendKeyUp(ushort vk) => SendKeyInput(vk, keyUp: true);

        private static void SendKeyInput(ushort vk, bool keyUp)
        {
            var input = new INPUT
            {
                type = InputKeyboard,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = keyUp ? KeyEventFKeyUp : 0,
                        time = 0,
                        dwExtraInfo = System.IntPtr.Zero,
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion U;
        }

        // The union must be large enough to hold MOUSEINPUT/HARDWAREINPUT too (even though
        // only ki is used here), otherwise sizeof(INPUT) here won't match user32.dll's own
        // struct size and SendInput will silently reject every call.
        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public System.IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public System.IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
    }
}
