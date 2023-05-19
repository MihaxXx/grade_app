using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;



#if DEBUG
[assembly: Application(UsesCleartextTraffic = true, Debuggable=true)]
#else
[assembly: Application(UsesCleartextTraffic = false, Debuggable = false)]
#endif