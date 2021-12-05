using System;
using System.IO;
using System.Xml;

namespace Scrape
{
    public static class ProjectFile
    {
        public static void GetProjectFile()
        {
            bool projectFile = false;
            foreach (string file in Directory.GetFiles($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..")}\\"))
            {
                if (Path.GetExtension(file) == ".srpproj")
                {
                    if (projectFile)
                    {
                        Error.CLIError("More than one project file in directory");
                    }
                    projectFile = true;
                }
            }
            if (!projectFile)
            {
                Error.CLIError("No project file in directory");
            }
        }
        public static void ReadTemplates()
        {
            using (XmlReader r = new XmlTextReader(Global.ProjectFile))
            {
                while (r.Read())
                {
                    switch (r.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (r.IsEmptyElement)
                            {
                                break;
                            }
                            //reading element contents (r.Value)
                            break;
                    }
                }
            }
        }
    }
}