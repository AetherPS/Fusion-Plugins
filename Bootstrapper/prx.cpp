#include "stdafx.h"

#define INI_PATH "/data/Fusion/PluginList.ini"

void Exit()
{
	Logger::Info("[Bootstrapper] Shutting down...");
	scePthreadExit(0);
}

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
		Logger::Init(true, Logger::LogLevelAll);
		Logger::Info("[Bootstrapper] Starting up...");

		ScePthread thr;
		scePthreadCreate(&thr, 0, [](void* arg) -> void*
		{
			// Get the app info for the title Id.
			SceAppInfo info{};
			if (sceKernelGetAppInfo(getpid(), &info) != 0)
			{
				Logger::Error("[Bootstrapper] Failed to get app info.");
				Exit();
				return 0;
			}

			// Fetch the INI file.
			IniParser ini;
			if (!ini.Load(INI_PATH))
			{
				Logger::Error("[Bootstrapper] Failed to load INI from path '%s'", INI_PATH);
				Exit();
				return 0;
			}

			// Keep track of loaded plugin paths to avoid duplicates.
			std::set<std::string> loadedPaths;

			// Load plugins that apply to all titles.
			if (auto* defaults = ini.GetSection("default"))
			{
				for (const auto& path : *defaults)
				{
					if (loadedPaths.insert(path).second)
					{
						sceKernelLoadStartModule(path.c_str(), 0, 0, 0, 0, 0);
					}
				}
			}

			// Load title-specific plugins.
			std::string titleId(info.TitleId, strnlen(info.TitleId, 10));
			if (auto* titlePlugins = ini.GetSection(titleId))
			{
				for (const auto& path : *titlePlugins)
				{
					if (loadedPaths.insert(path).second)
					{
						sceKernelLoadStartModule(path.c_str(), 0, 0, 0, 0, 0);
					}
				}
			}

			Exit();
			return 0;
		}, 0, "BootStrapper");
		scePthreadJoin(thr, nullptr);

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{
		Logger::Info("[Bootstrapper] Stopping...");
		return 0;
	}
}
