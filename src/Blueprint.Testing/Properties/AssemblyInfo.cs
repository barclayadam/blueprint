using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Blueprint - Testing")]
[assembly: AssemblyCompany("Red Sequence")]
[assembly: AssemblyProduct("Blueprint")]
[assembly: AssemblyCopyright("Copyright (c) 2013 Red Sequence")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: Guid("235901E3-02BD-46E3-8A59-9FF2F6EB691D")]
[assembly: CLSCompliant(false)]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("3.0.2")]
[assembly: AssemblyFileVersion("3.0.2")]
[assembly: NeutralResourcesLanguage("en-GB")]
