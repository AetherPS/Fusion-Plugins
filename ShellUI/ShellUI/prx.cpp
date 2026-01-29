#include "stdafx.h"
#include <Notify.h>

DetourManager* Manager;

bool LoadManagedDll()
{
    MonoDomain* rootDomain = mono_get_root_domain();
    mono_thread_attach(rootDomain);

    MonoAssembly* assembly = mono_domain_assembly_open(mono_get_root_domain(), "/user/data/Fusion/Plugins/ShellUIManaged.dll");
    // MonoAssembly* assembly = mono_domain_assembly_open(mono_get_root_domain(), "/hostapp/mono/ShellUIManaged.dll");
    if (!assembly)
    {
        Logger::Error("Failed to load assembly");
        return false;
    }

    MonoImage* image = mono_assembly_get_image(assembly);
    if (!image)
    {
        Logger::Error("Failed to get Image.");
        return false;
    }

    MonoClass* klass = mono_class_from_name(image, "Fusion", "ModuleMain");
    if (!klass)
    {
        Logger::Error("Failed to get Class.");
        return false;
    }

    MonoMethod* onLoad = mono_class_get_method_from_name(klass, "OnLoad", 0);
    if (!onLoad)
    {
        Logger::Error("Failed to get start method.");
        return false;
    }

    mono_runtime_invoke(onLoad, nullptr, nullptr, nullptr);

    return true;
}

extern "C"
{
    int __cdecl module_start(size_t argc, const void* args)
    {
        ScePthread thr;
        scePthreadCreate(&thr, 0, [](void* arg) -> void*
        {
            Logger::Init(true, Logger::LogLevelAll);
            Manager = new DetourManager();

            if (arg != nullptr && *(uint32_t*)arg == 1)
                sceKernelSleep(3);

            if (!LoadManagedDll())
            {
                char Version[64];
                size_t VersionLength = 64;
                sysctlbyname("Fusion.Version", Version, &VersionLength, nullptr, 0);

                char buffer[0x200];
                snprintf(buffer, sizeof(buffer), "Fusion %s Loaded\nThough no UI could be loaded.", Version);
                Notify(buffer);
            }

            scePthreadExit(0);
            return 0;
        }, (void*)args, "ModInit");

        scePthreadJoin(thr, nullptr);
        return 0;
    }

    int __cdecl module_stop(size_t argc, const void* args)
    {
        delete Manager;
        return 0;
    }
}