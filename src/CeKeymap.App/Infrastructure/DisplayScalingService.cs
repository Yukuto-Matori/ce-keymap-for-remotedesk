using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Changes the current display's scaling ("拡大率") by driving the Settings app's Display
    /// page (ms-settings:display) via UI Automation - the same action a user takes manually,
    /// selecting an item in the scaling ComboBox - rather than relying on undocumented registry
    /// values/APIs, both of which were confirmed not to actually change the effective scaling.
    /// </summary>
    internal sealed class DisplayScalingService
    {
        private const string ScalingComboAutomationId = "SystemSettings_Display_Scaling_ItemSizeOverride_ComboBox";
        private static readonly TimeSpan SearchTimeout = TimeSpan.FromSeconds(6);

        private readonly FileLogger _logger;

        public DisplayScalingService(FileLogger logger)
        {
            _logger = logger;
        }

        public void ApplyZoomPercent(int zoomPercent)
        {
            Process.Start(new ProcessStartInfo("ms-settings:display") { UseShellExecute = true });

            var combo = FindScalingComboBox();
            if (combo == null)
            {
                _logger.LogError("Could not locate the Display scaling combo box via UI Automation.");
                return;
            }

            try
            {
                SelectClosestOption(combo, zoomPercent);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to change display scaling via UI Automation.", ex);
            }
            finally
            {
                CloseSettingsWindow(combo);
            }
        }

        private AutomationElement FindScalingComboBox()
        {
            var idCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, ScalingComboAutomationId);
            var frameCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "ApplicationFrameWindow");
            var deadline = DateTime.UtcNow + SearchTimeout;

            while (DateTime.UtcNow < deadline)
            {
                var frames = AutomationElement.RootElement.FindAll(TreeScope.Children, frameCondition);
                foreach (AutomationElement frame in frames)
                {
                    var combo = frame.FindFirst(TreeScope.Descendants, idCondition);
                    if (combo != null) return combo;
                }

                Thread.Sleep(200);
            }

            return null;
        }

        private void SelectClosestOption(AutomationElement combo, int zoomPercent)
        {
            if (!combo.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPatternObj))
            {
                throw new InvalidOperationException("Scaling combo box does not support ExpandCollapsePattern.");
            }
            var expandPattern = (ExpandCollapsePattern)expandPatternObj;

            expandPattern.Expand();
            Thread.Sleep(300);

            var itemCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem);
            var items = combo.FindAll(TreeScope.Descendants, itemCondition);

            AutomationElement closest = null;
            var closestDiff = int.MaxValue;

            foreach (AutomationElement item in items)
            {
                var match = Regex.Match(item.Current.Name, @"^(\d+)%");
                if (!match.Success) continue;

                var percent = int.Parse(match.Groups[1].Value);
                var diff = Math.Abs(percent - zoomPercent);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closest = item;
                }
            }

            if (closest == null)
            {
                expandPattern.Collapse();
                throw new InvalidOperationException("No scaling options with a parseable percentage were found.");
            }

            _logger.Log($"Selecting display scaling option '{closest.Current.Name}' (requested {zoomPercent}%).");

            if (closest.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectPatternObj))
            {
                ((SelectionItemPattern)selectPatternObj).Select();
            }
            else
            {
                expandPattern.Collapse();
                throw new InvalidOperationException("Scaling option item does not support SelectionItemPattern.");
            }
        }

        private void CloseSettingsWindow(AutomationElement elementInWindow)
        {
            try
            {
                var top = elementInWindow;
                while (top != null && top.Current.ClassName != "ApplicationFrameWindow")
                {
                    top = TreeWalker.ControlViewWalker.GetParent(top);
                }

                if (top != null && top.TryGetCurrentPattern(WindowPattern.Pattern, out var windowPatternObj))
                {
                    ((WindowPattern)windowPatternObj).Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to close the Display settings window.", ex);
            }
        }
    }
}
