// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Internal.Runtime.InteropServices
{
    /// <summary>
    /// This class enables the .NET IJW host to load an in-memory module as a .NET assembly
    /// </summary>
    public static class InMemoryAssemblyLoader
    {
#if TARGET_WINDOWS
        private static bool IsSupported { get; } = InitializeIsSupported();

        private static bool InitializeIsSupported() => AppContext.TryGetSwitch("System.Runtime.InteropServices.EnableCppCLIHostActivation", out bool isSupported) ? isSupported : true;
#endif

        /// <summary>
        /// Loads into an isolated AssemblyLoadContext an assembly that has already been loaded into memory by the OS loader as a native module.
        /// </summary>
        /// <param name="moduleHandle">The native module handle for the assembly.</param>
        /// <param name="assemblyPath">The path to the assembly (as a pointer to a UTF-16 C string).</param>
        public static unsafe void LoadInMemoryAssembly(IntPtr moduleHandle, IntPtr assemblyPath)
        {
#if TARGET_WINDOWS
            if (!IsSupported)
                throw new NotSupportedException("This API is not enabled in trimmed scenarios. see https://aka.ms/dotnet-illink/nativehost for more details");

            string? assemblyPathString = Marshal.PtrToStringUni(assemblyPath);
            if (assemblyPathString == null)
            {
                throw new ArgumentOutOfRangeException(nameof(assemblyPath));
            }

            // We don't cache the ALCs here since each IJW assembly will call this method at most once
            // (the load process rewrites the stubs that call here to call the actual methods they're supposed to)
#pragma warning disable IL2026 // suppressed in ILLink.Suppressions.LibraryBuild.xml
            AssemblyLoadContext context = new IsolatedComponentLoadContext(assemblyPathString);
#pragma warning restore IL2026
            context.LoadFromInMemoryModule(moduleHandle);
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
