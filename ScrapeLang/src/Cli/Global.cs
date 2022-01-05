using System.Collections.Generic;

namespace Scrape.Cli
{
    public static class Global
    {
        public static string ProjectFile;
        public static string OutputFolderName = "output";
        public static bool EntryPoint;
        public static string CurrentPath;
        public static List<Template> Templates;
    }
}