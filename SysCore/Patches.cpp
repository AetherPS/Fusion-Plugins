#include "stdafx.h"
#include "Patches.h"

#include <Patcher.h>
#include <PatchManager.h>

PatchManager* Manager;

void InstallPatches()
{
	Manager = new PatchManager();

	if (Offsets::SyscoreDECIPatch != 0)
	{
		Manager->AddPatch("SyscoreDECIPatch", ResolveAddress(Offsets::SyscoreDECIPatch), (void*)"\xC3", 1);
	}
}

void RemovePatches()
{
	delete Manager;
}