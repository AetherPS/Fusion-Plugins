#include "stdafx.h"

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
		ScePthread thr;
		scePthreadCreate(&thr, 0, [](void* arg) -> void*
		{
			Logger::Init(true, Logger::LogLevelAll);

			// Initialize offsets by firmware.
			if (!Offsets::Init())
			{
				Logger::Error("[Fusion] Unsupported firmware version.");
				scePthreadExit(0);
				return 0;
			}

			// Install patches for DECI
			InstallPatches();

			// Mount fuse
			int (*MountFuse)(const char* to, const char* from) = (decltype(MountFuse))ResolveAddress(Offsets::MountFuse);
			MountFuse("/hostapp", "/dev/fuse0");
			MountFuse("/host", "/dev/fuse1");

			// Kick deci start thread.
			ScePthread thr;
			scePthreadCreate(&thr, 0, (void* (*)(void*))ResolveAddress(Offsets::DevPortThread), 0, 0);
			scePthreadDetach(thr);

			scePthreadExit(0);
			return 0;
		}, 0, "Init");
		scePthreadJoin(thr, nullptr);

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{
		RemovePatches();

		return 0;
	}
}
