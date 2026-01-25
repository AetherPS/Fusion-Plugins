#pragma once

class Offsets
{
public:
	static bool Init()
	{
		SceKernelSwVersion SoftwareVersion;
		SoftwareVersion.Size = sizeof(SceKernelSwVersion);
		if (sceKernelGetSystemSwVersion(&SoftwareVersion) != 0)
		{
			Logger::Error("Failed to get the software version.");
			return false;
		}

		switch (SoftwareVersion.Version)
		{
		case 0x9008031:

			// DECI
			MountFuse = 0x12C0;
			DevPortThread = 0x1480;
			SyscoreDECIPatch = 0x0;

			break;

		case 0x12020011:

			// DECI
			MountFuse = 0x12A0;
			DevPortThread = 0x1480;
			SyscoreDECIPatch = 0x3A3D0;

			break;

		case 0x12508001:

			// DECI
			MountFuse = 0x12A0;
			DevPortThread = 0x1480;
			SyscoreDECIPatch = 0x3A3D0;

			break;

		case 0x13008001:

			// DECI
			MountFuse = 0x12A0;
			DevPortThread = 0x1480;
			SyscoreDECIPatch = 0x3A3D0;

			break;

		default:
			Logger::Info("Unknown Firmware: %X\n", SoftwareVersion.Version);
			return false;
		}

		return true;
	}

	// DECI
	static inline uint64_t MountFuse;
	static inline uint64_t DevPortThread;
	static inline uint64_t SyscoreDECIPatch;

private:

};
