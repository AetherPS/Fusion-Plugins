using Fusion.Internal;
using Sce.PlayStation.PUI.UI2;
using Sce.Vsh.ShellUI.TopMenu;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fusion.TopMenu
{
    internal class SystemAreaPanelHooks
    {
        private static IntPtr _SysItemInit_stub;

        public static void AddFusionMenu(SystemAreaPanel instance)
        {
            var m_baseWidget = instance.m_baseWidget;
            var m_systemAreaIconList = instance.m_systemAreaIconList;

            // Create the panel
            var fusionPanel = new Panel()
            {
                Name = "FusionPanel",
                Width = 128,
                Height = 128,
                X = 56,
                Y = 38,
            };

            m_baseWidget.AppendChild(fusionPanel);

            // Create the icon
            var fusionIcon = new SystemAreaIconFusion(fusionPanel);

            // Insert at the beginning (leftmost position)
            m_systemAreaIconList.Insert(0, fusionIcon);

            instance.createVoiceGuide("Fusion");
        }

        [MethodOverride(typeof(SystemAreaPanel))]
        public static unsafe void SysItemInit(SystemAreaPanel instance)
        {
            AddFusionMenu(instance);
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)_SysItemInit_stub)(*(IntPtr*)&instance);
        }
    }
}