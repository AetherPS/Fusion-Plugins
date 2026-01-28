#include "stdafx.h"

DetourManager* Manager;

void LoadManagedDll()
{
    MonoDomain* rootDomain = mono_get_root_domain();
    mono_thread_attach(rootDomain);

    MonoAssembly* assembly = mono_domain_assembly_open(mono_get_root_domain(), "/data/Fusion/Plugins/ShellUIManaged.dll");
    // MonoAssembly* assembly = mono_domain_assembly_open(mono_get_root_domain(), "/hostapp/mono/ShellUIManaged.dll");
    if (!assembly)
    {
        Logger::Error("Failed to load assembly");
        return;
    }

    MonoImage* image = mono_assembly_get_image(assembly);
    if (!image)
    {
        Logger::Error("Failed to get Image.");
        return;
    }

    MonoClass* klass = mono_class_from_name(image, "Fusion", "ModuleMain");
    if (!klass)
    {
        Logger::Error("Failed to get Class.");
        return;
    }

    MonoMethod* onLoad = mono_class_get_method_from_name(klass, "OnLoad", 0);
    if (!onLoad)
    {
        Logger::Error("Failed to get start method.");
        return;
    }

    mono_runtime_invoke(onLoad, nullptr, nullptr, nullptr);
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

            LoadManagedDll();

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