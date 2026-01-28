using Fusion.Internal;
using Sce.Vsh.Accessor;
using Sce.Vsh.Accessor.Db;
using Sce.Vsh.ShellUI.AppSystem;
using Sce.Vsh.ShellUI.Base;
using Sce.Vsh.ShellUI.Library;
using Sce.Vsh.ShellUI.TopMenu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.Features.Devkit
{
    internal static class DevkitContent
    {
        private static IntPtr _ExecuteCountQuery_stub;
        private static IntPtr _ExecuteSelectQuery_stub;
        private static IntPtr _ExecuteSelectQueryForIndex_stub;
        private static IntPtr _GetIconPath_stub;

        private static IList<AppBrowseItem> MemoryItemList
        {
            get
            {
                List<AppBrowseItem> list = new List<AppBrowseItem>();

                if (_appHome)
                {
                    list.Add(new AppBrowseItem
                    {
                        TitleId = "NPXS29999",
                        TitleName = "★APP_HOME(host)",
                        MetaDataPath = string.Empty
                    });
                    list.Add(new AppBrowseItem
                    {
                        TitleId = "NPXS29998",
                        TitleName = "★APP_HOME(data)",
                        MetaDataPath = string.Empty
                    });
                }

                if (_debugSettingsShortcut)
                {
                    list.Add(new AppBrowseItem
                    {
                        TitleId = "NPXS20993",
                        TitleName = "★Debug Settings",
                        MetaDataPath = string.Empty
                    });
                }

                return list;
            }
        }

        private static bool _debugSettingsShortcut = false;
        public static bool DebugSettingsShortcut
        {
            get
            {
                return _debugSettingsShortcut;
            }

            set
            {
                if (_debugSettingsShortcut != value)
                {
                    _debugSettingsShortcut = value;
                    RefreshTopMenu();
                }
            }
        }

        private static bool _appHome = false;
        public static bool AppHome
        {
            get
            {
                return _appHome;
            }

            set
            {
                if (_appHome != value)
                {
                    _appHome = value;
                    RefreshTopMenu();
                }
            }
        }

        public static void RefreshTopMenu()
        {
            ContentAreaScene scene = (ContentAreaScene)AreaManager.Instance.GetContainerScene(AreaType.Contents).Find("ContentAreaScene");
            scene.m_contentsList.First().ReloadItemSource();
        }

        private static List<AppBrowseItem> GetMemoryItemList(ref int offset, ref int limit)
        {
            var list = new List<AppBrowseItem>();

            try
            {
                int num = offset;
                int num2 = limit;
                int num3;
                if (offset + limit <= MemoryItemList.Count)
                {
                    num3 = limit;
                    num = 0;
                    num2 = 0;
                }
                else if (offset < MemoryItemList.Count)
                {
                    num3 = MemoryItemList.Count - offset;
                    num = 0;
                    num2 = limit - num3;
                }
                else
                {
                    num3 = 0;
                    num = offset - MemoryItemList.Count;
                    num2 = limit;
                }
                list = new List<AppBrowseItem>();
                for (int i = offset + num3 - 1; i >= offset; i--)
                {
                    list.Insert(0, MemoryItemList[i]);
                }
                offset = num;
                limit = num2;
            }
            catch
            {
                
            }

            return list;
        }

        [MethodOverride(typeof(AppBrowseItemAccessor))]
        public static unsafe int ExecuteCountQuery(AppBrowseItemAccessor instance)
        {
            int count = ((delegate* unmanaged[Cdecl]<IntPtr, int>)_ExecuteCountQuery_stub)(*(IntPtr*)&instance);

            if (instance == null)
                return count;

            if (instance.exclusionFilterTypeAppHome != AppBrowseItemAccessor.FilterTypeAppHome.None)
                return count;

            return count + MemoryItemList.Count;
        }

        [MethodOverride(typeof(AppBrowseItemAccessor))]
        public static unsafe List<Item> ExecuteSelectQuery(AppBrowseItemAccessor instance, int offset, int limit)
        {
            IntPtr resultPtr = ((delegate* unmanaged[Cdecl]<IntPtr, int, int, IntPtr>)_ExecuteSelectQuery_stub)(
                *(IntPtr*)&instance,
                offset,
                limit
            );
            List<Item> result = *(List<Item>*)&resultPtr;

            if (instance.exclusionFilterTypeAppHome != AppBrowseItemAccessor.FilterTypeAppHome.None)
                return result;

            List<Item> list = new List<Item>();
            List<AppBrowseItem> memoryItemList = GetMemoryItemList(ref offset, ref limit);
            if (memoryItemList != null)
            {
                foreach (AppBrowseItem appBrowseItem in memoryItemList)
                {
                    list.Add(new AppBrowseItem
                    {
                        TitleId = appBrowseItem.TitleId,
                        TitleName = appBrowseItem.TitleName,
                        MetaDataPath = appBrowseItem.MetaDataPath
                    });
                }
            }
            return list.Concat(result).ToList();
        }

        [MethodOverride(typeof(AppBrowseItemAccessor))]
        public static unsafe List<Item> ExecuteSelectQueryForIndex(AppBrowseItemAccessor instance, int offset, int limit)
        {
            IntPtr resultPtr = ((delegate* unmanaged[Cdecl]<IntPtr, int, int, IntPtr>)_ExecuteSelectQueryForIndex_stub)(
                *(IntPtr*)&instance,
                offset,
                limit
            );
            List<Item> result = *(List<Item>*)&resultPtr;

            if (instance.exclusionFilterTypeAppHome != AppBrowseItemAccessor.FilterTypeAppHome.None)
                return result;

            List<Item> list = new List<Item>();
            List<AppBrowseItem> memoryItemList = GetMemoryItemList(ref offset, ref limit);
            if (memoryItemList != null)
            {
                foreach (AppBrowseItem appBrowseItem in memoryItemList)
                {
                    list.Add(new AppBrowseItemLite
                    {
                        TitleId = appBrowseItem.TitleId
                    });
                }
            }
            return list.Concat(result).ToList();
        }

        [MethodOverride(typeof(AppBrowseItemMethodExteneder))]
        public static unsafe string GetIconPath(this AppBrowseItem item, bool withTheme = false)
        {
            string titleId = item.GetTitleId();
            if (titleId == "NPXS29999")
            {
                return BasePlugin.GetTexture("tex_app_home");
            }
            if (titleId == "NPXS29998")
            {
                return BasePlugin.GetTexture("tex_app_home_data");
            }
            if (titleId == "NPXS20993")
            {
                return BasePlugin.GetTexture("tex_debug_settings");
            }

            IntPtr resultPtr = ((delegate* unmanaged[Cdecl]<IntPtr, bool, IntPtr>)_GetIconPath_stub)(
                *(IntPtr*)&item,
                withTheme
            );
            return *(string*)&resultPtr;
        }

        // This one doesn't need a stub since it doesn't call the original
        [MethodOverride(typeof(ApplicationMonitor.AppConfig))]
        public static bool IsLaunchable(string titleId)
        {
            if (titleId.IndexOf("NPXS20") == 0)
            {
                return true;
            }
            if (titleId == "NPXS21008" || titleId == "NPXS27003" || titleId == "NPXS27009" ||
                titleId == "NPXS29998" || titleId == "NPXS29999")
            {
                return true;
            }
            return false;
        }
    }
}
