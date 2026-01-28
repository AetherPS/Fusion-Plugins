using Fusion.Internal;
using Sce.Vsh.ShellUI.Settings.PkgInstaller;
using System;

namespace Fusion
{
    public static class PackageInstaller
    {
        private static readonly string[] _searchPaths =
        {
            "/user/data/pkg",
            "/user/data/Fusion/pkg",
        };

        [MethodOverride(typeof(SearchJob))]
        public static void SearchDisc(SearchJob instance)
        {
            System.Console.WriteLine("[PackageInstaller] SearchDisc -> custom paths");

            foreach (var path in _searchPaths)
            {
                try
                {
                    System.Console.WriteLine($"[PackageInstaller] Searching: {path}");
                    instance.SearchDir(path);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[PackageInstaller] Error: {ex.Message}");
                }
            }
        }
    }
}