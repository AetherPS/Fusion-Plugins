#include "stdafx.h"

#define INI_PATH "/data/Fusion/PluginList.ini"

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
		ScePthread thr;
		scePthreadCreate(&thr, 0, [](void* arg) -> void*
		{
			Logger::Init(true, Logger::LogLevelAll);

			// Get the app info for the title Id.
			SceAppInfo info{};
			if (sceKernelGetAppInfo(getpid(), &info) != 0)
			{
				scePthreadExit(0);
				return 0;
			}

			// Fetch the INI file.
			IniParser ini;
			if (!ini.Load(INI_PATH))
			{
				scePthreadExit(0);
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

			scePthreadExit(0);
			return 0;
		}, 0, "Init");
		scePthreadJoin(thr, nullptr);

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{

		return 0;
	}
}
