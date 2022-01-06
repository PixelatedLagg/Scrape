using System.IO;
using System.Text;

namespace Scrape.Cli
{
    public static class ProjectFile
    {
        public static void GetProjectFile()
        {
            bool projectFile = false;
            foreach (string file in Directory.GetFiles($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..")}\\"))
            {
                if (Path.GetExtension(file) == ".sproj")
                {
                    if (projectFile)
                    {
                        Error.CLIError("More than one project file in directory");
                    }
                    Global.ProjectFile = file;
                    projectFile = true;
                }
            }
            if (!projectFile)
            {
                Error.CLIError("No project file in directory");
            }
            StringBuilder property = new StringBuilder(100); 
            StringBuilder value = new StringBuilder(100);
            StringBuilder text = new StringBuilder(File.ReadAllText(Global.ProjectFile).Length);
            bool read = true; 
            using (StreamReader sr = new StreamReader(Global.ProjectFile))
            {
                while (!sr.EndOfStream)
                {
                    text.Append(sr.ReadLine());
                }
                sr.Close();
            }
            foreach (char c in text.ToString())
            {
                if (c == ' ')
                {
                    continue;
                }
                if (c == ';')
                {
                    read = true;
                    switch (property.ToString())
                    {
                        case "outputname":
                            Global.OutputFolderName = value.ToString();
                            break;
                    }
                    property.Clear();
                    value.Clear();
                    continue;
                }
                if (c == '=')
                {
                    read = false;
                    continue;
                }
                if (read)
                {
                    property.Append(c);
                }
                else
                {
                    value.Append(c);
                }
            }
        }
    }
}