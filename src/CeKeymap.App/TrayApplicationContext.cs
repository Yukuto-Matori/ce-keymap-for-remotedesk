using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CeKeymap.App.Forms;
using CeKeymap.App.Infrastructure;
using CeKeymap.Core.Models;

namespace CeKeymap.App
{
    internal sealed class TrayApplicationContext : ApplicationContext
    {
        private static readonly (FeatureId Id, string Label)[] FeatureLabels =
        {
            (FeatureId.AppWindowSwitch, "アプリウィンドウ切り替え"),
            (FeatureId.ZoomDesktop, "拡大率変更 デスクトップ用"),
            (FeatureId.ZoomMobile, "拡大率変更 モバイル用"),
            (FeatureId.PressWinKey, "Winキー押下"),
        };

        private readonly NotifyIcon _notifyIcon;
        private readonly AppSettings _settings;
        private readonly SettingsFileRepository _settingsRepository;
        private readonly AutoStartService _autoStartService;
        private readonly KeyboardHookService _hookService;
        private readonly FileLogger _logger;
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private readonly DisplayScalingService _displayScalingService;

        public TrayApplicationContext(
            AppSettings settings,
            SettingsFileRepository settingsRepository,
            AutoStartService autoStartService,
            KeyboardHookService hookService,
            FileLogger logger,
            Icon icon)
        {
            _settings = settings;
            _settingsRepository = settingsRepository;
            _autoStartService = autoStartService;
            _hookService = hookService;
            _logger = logger;
            _displayScalingService = new DisplayScalingService(logger);

            _hookService.FeatureTriggered += OnFeatureTriggered;

            _notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Visible = true,
                Text = "CeKeymapForRemotedesk",
                ContextMenuStrip = BuildContextMenu(),
            };
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var featureMenu = new ToolStripMenuItem("機能設定");
            foreach (var (featureId, label) in FeatureLabels)
            {
                featureMenu.DropDownItems.Add(BuildFeatureSubmenu(featureId, label));
            }
            menu.Items.Add(featureMenu);

            var autoStartItem = new ToolStripMenuItem("PC起動後に自動起動")
            {
                CheckOnClick = true,
                Checked = _settings.AutoStart,
            };
            autoStartItem.CheckedChanged += (s, e) =>
            {
                _settings.AutoStart = autoStartItem.Checked;
                _autoStartService.SetEnabled(autoStartItem.Checked);
                _settingsRepository.Save(_settings);
            };
            menu.Items.Add(autoStartItem);

            var developerMenu = new ToolStripMenuItem("開発者向け");
            developerMenu.DropDownItems.Add(new ToolStripMenuItem("アプリログを開く", null, (s, e) => OpenLogFile()));
            menu.Items.Add(developerMenu);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("終了", null, (s, e) => ExitApplication()));

            return menu;
        }

        private void OpenLogFile()
        {
            if (!File.Exists(_logger.FilePath))
            {
                MessageBox.Show(
                    "ログファイルはまだ作成されていません。",
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(_logger.FilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to open log.txt with the system default app.", ex);
                MessageBox.Show(
                    "ログファイルを開けませんでした。",
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private ToolStripMenuItem BuildFeatureSubmenu(FeatureId featureId, string label)
        {
            var binding = _settings.Features[featureId];
            var root = new ToolStripMenuItem(label);

            var enabledItem = new ToolStripMenuItem("有効")
            {
                CheckOnClick = true,
                Checked = binding.Enabled,
            };
            enabledItem.CheckedChanged += (s, e) =>
            {
                binding.Enabled = enabledItem.Checked;
                _settingsRepository.Save(_settings);
            };
            root.DropDownItems.Add(enabledItem);

            var editItem = new ToolStripMenuItem("キーバインド変更...");
            editItem.Click += (s, e) =>
            {
                using (var form = new KeybindingEditForm(_settings, featureId))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _settingsRepository.Save(_settings);
                        enabledItem.Checked = binding.Enabled;
                        RefreshMenu();
                    }
                }
            };
            root.DropDownItems.Add(editItem);

            if (featureId == FeatureId.ZoomDesktop || featureId == FeatureId.ZoomMobile)
            {
                var zoomItem = new ToolStripMenuItem("拡大率の指定...");
                zoomItem.Click += (s, e) => EditZoomPercent(featureId);
                root.DropDownItems.Add(zoomItem);
            }

            return root;
        }

        private void EditZoomPercent(FeatureId featureId)
        {
            var binding = _settings.Features[featureId];
            using (var form = new ZoomPercentEditForm(binding.ZoomPercent ?? 100))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    binding.ZoomPercent = form.SelectedPercent;
                    _settingsRepository.Save(_settings);
                }
            }
        }

        private void RefreshMenu()
        {
            _notifyIcon.ContextMenuStrip = BuildContextMenu();
        }

        private void OnFeatureTriggered(FeatureId featureId)
        {
            var binding = _settings.Features[featureId];
            if (!binding.Enabled)
            {
                _logger.Log($"Feature {featureId} matched but is disabled; skipping execution.");
                return;
            }

            switch (featureId)
            {
                case FeatureId.AppWindowSwitch:
                    _logger.Log("Executing AppWindowSwitch (Alt+Tab emulation).");
                    _inputSimulator.SimulateAppWindowSwitch();
                    break;
                case FeatureId.ZoomDesktop:
                case FeatureId.ZoomMobile:
                    var zoomPercent = binding.ZoomPercent ?? 100;
                    _logger.Log($"Executing {featureId} (ZoomPercent={zoomPercent}).");
                    _displayScalingService.ApplyZoomPercent(zoomPercent);
                    break;
                case FeatureId.PressWinKey:
                    _logger.Log("Executing PressWinKey (left Win key emulation).");
                    _inputSimulator.SimulateWinKeyPress();
                    break;
            }
        }

        private void ExitApplication()
        {
            _hookService.Stop();
            _notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
