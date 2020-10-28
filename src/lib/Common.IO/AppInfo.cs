using System;
using System.Reflection;

namespace Common.IO
{
    public static class PiscesSuiteAppInfo
    {

        #region members

        public static readonly string Copyright;
        public static readonly string Title;
        public static readonly string InformationalVersion;
        public static readonly string Version;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        static PiscesSuiteAppInfo()
        {
            var assembly = Assembly.GetEntryAssembly();
            //watch out, these  get filled in with "TestHost" when running unit tests b/c thats the entry assembly.

            Copyright = GetCopyright(assembly);
            Title = GetTitle(assembly);
            Version = GetVersion(assembly);
            InformationalVersion = GetInformationalVersion(assembly);
        }

        private static string GetCopyright(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyCopyrightAttribute>(entryAssembly);

             if (attr == null)
              return ("Update Me");

            else
                return  attr.ToString();
        }

        private static string GetVersion(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyFileVersionAttribute>(entryAssembly);
            
            if (attr == null)
              return ("Update Me");
            
            return attr?.Version;
        }

        private static string GetInformationalVersion(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyInformationalVersionAttribute>(entryAssembly);
            
            if (attr == null)
              return ("Update Me");
            
            return attr?.InformationalVersion;
        }

        private static string GetTitle(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyTitleAttribute>(entryAssembly);
        
            if (attr == null)
              return ("Update Me");
            
            return attr?.Title;
        }

        private static T GetAssemblyAttributes<T>(Assembly entryAssembly)
        {
            var attrs = entryAssembly.GetCustomAttributes(typeof(T)) as T[];
            // ReSharper disable once PossibleNullReferenceException
            return attrs.Length == 0 ? default(T) : attrs[0];
        }

        public static bool TestLoaded()
        {
           return true;
        }

    }
}
