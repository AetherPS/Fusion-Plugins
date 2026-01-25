#include "stdafx.h"
#include "Patches.h"

#include <Patcher.h>
#include <PatchManager.h>

PatchManager* Manager;

void InstallPatches()
{
	Manager = new PatchManager();

	Manager->AddPatch("sceKernelIsGenuineCEX1", ResolveAddress(Offsets::IsGenuineCEX1), (void*)"\x31\xC0\xEB\x01", 4);
	Manager->AddPatch("sceKernelIsGenuineCEX2", ResolveAddress(Offsets::IsGenuineCEX2), (void*)"\x31\xC0\xEB\x01", 4);
	Manager->AddPatch("sceKernelIsGenuineCEX3", ResolveAddress(Offsets::IsGenuineCEX3), (void*)"\x31\xC0\xEB\x01", 4);
	Manager->AddPatch("sceKernelIsGenuineCEX4", ResolveAddress(Offsets::IsGenuineCEX4), (void*)"\x31\xC0\xEB\x01", 4);

	Manager->AddPatch("sceKernelIsAssistMode1", ResolveAddress(Offsets::IsAssistMode1), (void*)"\x31\xC0\xEB\x01", 4);
	Manager->AddPatch("sceKernelIsAssistMode2", ResolveAddress(Offsets::IsAssistMode2), (void*)"\x31\xC0\xEB\x01", 4);
	Manager->AddPatch("sceKernelIsAssistMode3", ResolveAddress(Offsets::IsAssistMode3), (void*)"\x31\xC0\xEB\x01", 4);
	Manager->AddPatch("sceKernelIsAssistMode4", ResolveAddress(Offsets::IsAssistMode4), (void*)"\x31\xC0\xEB\x01", 4);

	// Enable fake pkg.
	Manager->AddPatch("EnableFakePkg", ResolveAddress(Offsets::EnableFakePkg), (void*)"\xE9\x98\x00\x00\x00", 8);

	// fake to free.
	Manager->AddPatch("FakeText", ResolveAddress(Offsets::FakeText), (void*)"free", 4);

	// Enable mounting data into sandboxes.
	Manager->AddPatch("MountDataIntoSandbox", ResolveAddress(Offsets::MountDataIntoSandbox), (void*)"\x31\xC0\xFF\xC0\x90", 5);

	// Patch Pkg Update Checks
	Manager->AddPatch("DisablePkgPatchCheck1", ResolveAddress(Offsets::DisablePkgPatchCheck1), (void*)"\xEB", 1);
	Manager->AddPatch("DisablePkgPatchCheck2", ResolveAddress(Offsets::DisablePkgPatchCheck2), (void*)"\xEB", 1);
	Manager->AddPatch("DisablePkgPatchCheck3", ResolveAddress(Offsets::DisablePkgPatchCheck3), (void*)"\x48\x31\xC0\xC3", 4);
}

void RemovePatches()
{
	delete Manager;
}