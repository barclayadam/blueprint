using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Blueprint - Core")]
[assembly: AssemblyCompany("Red Sequence")]
[assembly: AssemblyProduct("Blueprint")]
[assembly: AssemblyCopyright("Copyright (c) 2013 Red Sequence")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: Guid("D0BBC6DA-A979-46BA-916D-58F5D32BFECD")]
[assembly: CLSCompliant(false)]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("3.6.7")]
[assembly: AssemblyFileVersion("3.6.7")]
[assembly: NeutralResourcesLanguage("en-GB")]
