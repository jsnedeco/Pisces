using System;
using System.Reflection;

namespace Common.IO
{
    public static class PiscesSuiteAppInfo
    {

        #region members

        public static readonly string Copyright;
        public static readonly string Title;
        public static readonly string Reference;
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
            Reference = GetReference(assembly);
            Version = GetVersion(assembly);
            InformationalVersion = GetInformationalVersion(assembly);
        }

        private static string GetCopyright(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyCopyrightAttribute>(entryAssembly);

             if (attr == null)
              return ("GNU GENERAL PUBLIC LICENSE");

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
              return ("5.2.11.0");
            
            return attr?.InformationalVersion;
        }

        private static string GetReference(Assembly entryAssembly)
        {
            string publication = "Tamsen Dunn, Gwenn Berry, Dorothea Emig-Agius, Yu Jiang, Serena Lei, Anita Iyer, Nitin Udar, Han-Yu Chuang, Jeff Hegarty, Michael Dickover, Brandy Klotzle, Justin Robbins, Marina Bibikova, Marc Peeters, Michael Strömberg, " +
            "Pisces: an accurate and versatile variant caller for somatic and germline next-generation sequencing data, " + 
            "Bioinformatics, Volume 35, Issue 9, 1 May 2019, " + 
            "Pages 1579–1581, https://doi.org/10.1093/bioinformatics/bty849";
            return publication;
        }

        private static string GetTitle(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyTitleAttribute>(entryAssembly);
        
            if (attr == null)
              return ("Pisces Software");
            
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
