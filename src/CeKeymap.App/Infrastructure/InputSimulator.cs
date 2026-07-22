using System.Linq;
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
        private const uint KeyEventFExtendedKey = 0x0001;
        private const uint KeyEventFKeyUp = 0x0002;
        private const uint KeyEventFScanCode = 0x0008;
        private const ushort VkMenu = 0x12;   // Alt (generic)
        private const ushort VkTab = 0x09;
        private const ushort VkLWin = 0x5B;

        // The six modifier VKs the low-level hook itself tracks; must match KeyboardHookService.
        private static readonly ushort[] ModifierVks = { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 };

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
            // Windows only opens the Start Menu for a "clean" Win press/release with no other
            // key involved. Since this feature is triggered by a modifier+key hotkey (e.g.
            // RAlt+W), that modifier is still physically held at this point, so Windows would
            // see "RAlt+Win" (which has no default action) unless we temporarily fake-release
            // it around the Win keystroke and restore it afterward.
            var heldModifiers = ModifierVks.Where(vk => (GetAsyncKeyState(vk) & 0x8000) != 0).ToArray();

            foreach (var vk in heldModifiers) SendKeyUp(vk);

            SendKeyDown(VkLWin, extended: true);
            Thread.Sleep(50);
            SendKeyUp(VkLWin, extended: true);

            foreach (var vk in heldModifiers) SendKeyDown(vk);
        }

        private static void SendKeyDown(ushort vk, bool extended = false) => SendKeyInput(vk, keyUp: false, extended: extended);

        private static void SendKeyUp(ushort vk, bool extended = false) => SendKeyInput(vk, keyUp: true, extended: extended);

        private static void SendKeyInput(ushort vk, bool keyUp, bool extended)
        {
            var flags = keyUp ? KeyEventFKeyUp : 0;

            var input = new INPUT
            {
                type = InputKeyboard,
                U = new InputUnion
                {
                    ki = extended
                        ? new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = (ushort)MapVirtualKey(vk, 0),
                            dwFlags = flags | KeyEventFScanCode | KeyEventFExtendedKey,
                            time = 0,
                            dwExtraInfo = System.IntPtr.Zero,
                        }
                        : new KEYBDINPUT
                        {
                            wVk = vk,
                            wScan = 0,
                            dwFlags = flags,
                            time = 0,
                            dwExtraInfo = System.IntPtr.Zero,
                        }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

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
