#include "stdafx.h"
#include "Exports.h"

void* get_compiled_method(MonoMethod* method)
{
    if (!method) return nullptr;

    // Try AOT first, fall back to JIT
    void* aot_code = mono_aot_get_method(mono_domain_get(), method);
    if (aot_code)
        return aot_code;

    return mono_compile_method(method);
}

extern "C"
{
    __declspec(dllexport) void* AddMethodDetour(const char* hookKey, MonoMethod* from, MonoMethod* to)
    {
        void* compiled_from = get_compiled_method(from);
        if (!compiled_from)
        {
            Logger::Error("[Detour] Failed to compile to method: %s", from->name);
            return nullptr;
        }

        void* compiled_to = get_compiled_method(to);
        if (!compiled_to)
        {
            Logger::Error("[Detour] Failed to compile to method: %s", to->name);
            return nullptr;
        }

        Manager->AddDetour<Detour64>(hookKey, (uintptr_t)compiled_from, compiled_to);
        Logger::Success("[Detour] Installed: %s (key: %s)", from->name, hookKey);

        return Manager->GetStub(hookKey);
    }
}