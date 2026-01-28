using Fusion.Internal;
using Sce.PlayStation.PUI;
using Sce.PlayStation.PUI.UI2;
using Sce.Vsh.ShellUI.TopMenu;
using System;
using System.Runtime.InteropServices;

namespace Fusion.Features.Devkit
{
    public static class DevkitPanel
    {
        private static bool _showPanel = false;
        private static bool _rainbowBackground = false;
        private static float _hue = 0f;
        private static DateTime _lastOriginalUpdate = DateTime.MinValue;
        private static readonly TimeSpan OriginalUpdateInterval = TimeSpan.FromSeconds(2);

        private static IntPtr _AreaManager_ctor_stub;
        private static IntPtr _UpdateDevKitPanel_stub;

        public static bool ShowPanel
        {
            get => _showPanel;
            set
            {
                _showPanel = value;
                Update();
            }
        }

        public static bool RainbowBackground
        {
            get => _rainbowBackground;
            set
            {
                _rainbowBackground = value;
                UpdateTimerInterval();
            }
        }

        [MethodOverride(typeof(AreaManager), ".ctor", DelegateFieldName = nameof(_AreaManager_ctor_stub))]
        public static unsafe void AreaManager_Constructor(AreaManager instance)
        {
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)_AreaManager_ctor_stub)(*(IntPtr*)&instance);
            if (ShowPanel)
            {
                Create(instance);
            }
        }

        [MethodOverride(typeof(AreaManager), "updateDevKitPanel", DelegateFieldName = nameof(_UpdateDevKitPanel_stub))]
        public static unsafe void UpdateDevKitPanel(AreaManager instance)
        {
            try
            {
                UpdateTimerInterval();
                if (RainbowBackground)
                {
                    _hue += .5f;
                    if (_hue >= 360f)
                        _hue = 0f;

                    var color = HSVToRGB(_hue, 1.0f, 1.0f, 0.8f);
                    var devKitPanel = instance.m_devKitPanel;
                    if (devKitPanel != null)
                    {
                        devKitPanel.BackgroundColor = color;
                    }

                    var now = DateTime.Now;
                    if (now - _lastOriginalUpdate >= OriginalUpdateInterval)
                    {
                        ((delegate* unmanaged[Cdecl]<IntPtr, void>)_UpdateDevKitPanel_stub)(*(IntPtr*)&instance);
                        _lastOriginalUpdate = now;
                    }
                }
                else
                {
                    ((delegate* unmanaged[Cdecl]<IntPtr, void>)_UpdateDevKitPanel_stub)(*(IntPtr*)&instance);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DevkitPanel] UpdateDevKitPanel_Hook failed: {ex.Message}");
                ((delegate* unmanaged[Cdecl]<IntPtr, void>)_UpdateDevKitPanel_stub)(*(IntPtr*)&instance);
            }
        }

        public static void Create(AreaManager instance)
        {
            instance.createDevKitPanel();
            UpdateTimerInterval();
            SetColour(instance, 0.0f, 0.0f, 0.0f, 0.5f);
        }

        public static void SetColour(AreaManager instance, float r, float g, float b, float a)
        {
            try
            {
                var devKitPanel = instance.m_devKitPanel;

                if (devKitPanel != null)
                {
                    devKitPanel.BackgroundColor = new UIColor(r, g, b, a);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DevkitPanel] SetColour failed: {ex.Message}");
            }
        }

        public static void Update()
        {
            if (ShowPanel)
                Show();
            else
                Hide();
        }

        public static void Show()
        {
            try
            {
                if (AreaManager.Instance == null)
                {
                    System.Console.WriteLine("[DevkitPanel] AreaManager.Instance is null");
                    return;
                }

                var devKitPanel = AreaManager.Instance.m_devKitPanel;

                // If m_devKitPanel is null we must create the panel first
                if (devKitPanel == null)
                {
                    Create(AreaManager.Instance);
                }
                else
                {
                    var updatePanelTimer = AreaManager.Instance.m_updatePanelTimer;

                    // If the m_updatePanelTimer is initialized start the timer
                    if (updatePanelTimer != null)
                    {
                        updatePanelTimer.Start();
                    }

                    devKitPanel.Show();
                }

                _showPanel = true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DevkitPanel] Show failed: {ex.Message}");
            }
        }

        public static void Hide()
        {
            try
            {
                if (AreaManager.Instance == null)
                    return;

                var updatePanelTimer = AreaManager.Instance.m_updatePanelTimer;

                // If the m_updatePanelTimer is initialized stop the timer
                if (updatePanelTimer != null)
                {
                    updatePanelTimer.Stop();
                }

                // Hide the panel
                var devKitPanel = AreaManager.Instance.m_devKitPanel;
                if (devKitPanel != null)
                {
                    devKitPanel.Hide();
                }

                _showPanel = false;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DevkitPanel] Hide failed: {ex.Message}");
            }
        }

        public static bool GetState()
        {
            try
            {
                if (AreaManager.Instance == null)
                    return false;

                var devKitPanel = AreaManager.Instance.m_devKitPanel;
                var updatePanelTimer = AreaManager.Instance.m_updatePanelTimer;

                if (devKitPanel != null && updatePanelTimer != null)
                {
                    return !updatePanelTimer.IsStopped;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DevkitPanel] GetState failed: {ex.Message}");
                return false;
            }
        }

        private static void UpdateTimerInterval()
        {
            try
            {
                if (AreaManager.Instance == null)
                    return;

                var updatePanelTimer = AreaManager.Instance.m_updatePanelTimer;

                if (updatePanelTimer != null)
                {
                    updatePanelTimer.Interval = RainbowBackground ? 0.01f : 2.0f;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DevkitPanel] UpdateTimerInterval failed: {ex.Message}");
            }
        }

        private static UIColor HSVToRGB(float hue, float saturation, float value, float alpha)
        {
            float c = value * saturation;
            float x = c * (1 - Math.Abs((hue / 60f) % 2 - 1));
            float m = value - c;

            float r = 0, g = 0, b = 0;

            if (hue >= 0 && hue < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (hue >= 60 && hue < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (hue >= 120 && hue < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (hue >= 180 && hue < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (hue >= 240 && hue < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (hue >= 300 && hue < 360)
            {
                r = c; g = 0; b = x;
            }

            return new UIColor(r + m, g + m, b + m, alpha);
        }
    }
}