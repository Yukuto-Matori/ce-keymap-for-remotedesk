using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CeKeymap.Core.Hotkeys;
using CeKeymap.Core.Models;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Installs a WH_KEYBOARD_LL hook, tracks which modifier keys are currently held, and
    /// delegates the "does this match a registered feature" decision to <see cref="HotkeyMatcher"/>.
    /// Matching happens edge-triggered (on the transition into a new key state) so holding a
    /// combo down does not repeatedly re-trigger the feature.
    /// </summary>
    internal sealed class KeyboardHookService
    {
        private const int WhKeyboardLl = 13;
        private const int WmKeyDown = 0x0100;
        private const int WmSysKeyDown = 0x0104;
        private const int WmKeyUp = 0x0101;
        private const int WmSysKeyUp = 0x0105;

        private readonly HotkeyMatcher _matcher = new HotkeyMatcher();
        private readonly Func<AppSettings> _settingsProvider;
        private readonly FileLogger _logger;
        private readonly LowLevelKeyboardProc _proc;
        private readonly HashSet<ModifierKey> _pressedModifiers = new HashSet<ModifierKey>();
        private IntPtr _hookHandle = IntPtr.Zero;
        private int? _activeMainKeyVk;

        public event Action<FeatureId> FeatureTriggered;

        public KeyboardHookService(Func<AppSettings> settingsProvider, FileLogger logger)
        {
            _settingsProvider = settingsProvider;
            _logger = logger;
            _proc = HookCallback;
        }

        public void Start()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)
            {
                _hookHandle = SetWindowsHookEx(WhKeyboardLl, _proc, GetModuleHandle(currentModule.ModuleName), 0);
            }

            if (_hookHandle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogError($"Failed to install the keyboard hook (Win32 error {error}).");
                MessageBox.Show(
                    "キーボードフックの設置に失敗しました。アプリを終了します。",
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        public void Stop()
        {
            if (_hookHandle == IntPtr.Zero) return;

            var handle = _hookHandle;
            _hookHandle = IntPtr.Zero;
            var success = UnhookWindowsHookEx(handle);

            if (!success)
            {
                _logger.LogError("Failed to uninstall the keyboard hook.");
                MessageBox.Show(
                    "キーボードフックの解除に失敗しました。アプリを終了します。",
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var message = wParam.ToInt32();

                if (message == WmKeyDown || message == WmSysKeyDown)
                {
                    HandleKeyDown(vkCode);
                }
                else if (message == WmKeyUp || message == WmSysKeyUp)
                {
                    HandleKeyUp(vkCode);
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private void HandleKeyDown(int vkCode)
        {
            var modifier = MapModifier(vkCode);
            if (modifier.HasValue)
            {
                if (_pressedModifiers.Add(modifier.Value))
                {
                    TryMatchAndTrigger(null);
                }
                return;
            }

            if (_activeMainKeyVk == vkCode) return; // OS auto-repeat while held; already handled

            _activeMainKeyVk = vkCode;
            TryMatchAndTrigger(VkCodeToKeyName(vkCode));
        }

        private void HandleKeyUp(int vkCode)
        {
            var modifier = MapModifier(vkCode);
            if (modifier.HasValue)
            {
                _pressedModifiers.Remove(modifier.Value);
                return;
            }

            if (_activeMainKeyVk == vkCode)
            {
                _activeMainKeyVk = null;
            }
        }

        private void TryMatchAndTrigger(string mainKey)
        {
            var settings = _settingsProvider();
            var matched = _matcher.Match(_pressedModifiers, mainKey, settings);
            if (matched.HasValue)
            {
                FeatureTriggered?.Invoke(matched.Value);
            }
        }

        private static ModifierKey? MapModifier(int vkCode)
        {
            switch (vkCode)
            {
                case 0xA0: return ModifierKey.LShift;
                case 0xA1: return ModifierKey.RShift;
                case 0xA2: return ModifierKey.LCtrl;
                case 0xA3: return ModifierKey.RCtrl;
                case 0xA4: return ModifierKey.LAlt;
                case 0xA5: return ModifierKey.RAlt;
                default: return null;
            }
        }

        private static string VkCodeToKeyName(int vkCode) => ((Keys)vkCode).ToString();

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
