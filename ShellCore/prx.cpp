#include "stdafx.h"
#include <FileUtils.h>
#include <FileSystem.h>
#include <7zExtractor.h>

#define DECI_FILES "/mnt/usb0/DeciFiles.7z"

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
		ScePthread thr;
		scePthreadCreate(&thr, 0, [](void* arg) -> void*
		{
			Logger::Init(true, Logger::LogLevelAll);

			// Mount system as R/W.
			RemountReadWrite("/dev/da0x4.crypt", "/system");
			RemountReadWrite("/dev/da0x5.crypt", "/system_ex");

			// Disable automatic updates.
			DisableUpdates();

			// Extract DECI files if present.
			if (FileSystem::Exists(DECI_FILES))
			{
				auto res = Extract7zFile(DECI_FILES, "/");
				if (res != 0)
				{
					Logger::Error("[Fusion] Failed to extract DECI files from '%s' for reason '%d'", DECI_FILES, res);
				}
				else
				{
					Logger::Info("[Fusion] Extracted DECI files from '%s'", DECI_FILES);
				}
			}

			// Initialize offsets by firmware.
			if (!Offsets::Init())
			{
				Logger::Error("[Fusion] Unsupported firmware version.");
				scePthreadExit(0);
				return 0;
			}

			// Install patches for fpkg and QOL.
			InstallPatches();

			// Start the decid server
			int (*StartDecidServer)() = (decltype(StartDecidServer))ResolveAddress(Offsets::StartDecidServer);
			StartDecidServer();

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
