using System.IO;
using System;
using Scrape.Code.Generation;
using System.Collections.Generic;

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
            ReadData();
        }
        private static void ReadData()
        {
            string text = File.ReadAllText(Global.ProjectFile);
            string value = "";
            Rules? currentRule = null;
            List<Rules?> setRules = new List<Rules?>();
            foreach (char c in text)
            {
                switch (c)
                {
                    case ' ':
                        continue;
                    case '=':
                        if (setRules.Contains(currentRule))
                        {
                            Error.CLIError("Already set sproj file property");
                        }
                        switch (value)
                        {
                            case "outputname":
                                currentRule = Rules.OutputFolderName;
                                value = "";
                                setRules.Add(currentRule);
                                break;
                            default:
                                Error.CLIError("Invalid sproj file property");
                                break;
                        }
                        continue;
                    case ';':
                        ChangeRule(currentRule, value);
                        value = "";
                        continue;
                }
                value += c;
            }
        }
        private static void ChangeRule(Rules? rule, string arg)
        {
            switch (rule)
            {
                case Rules.OutputFolderName:
                    Global.OutputFolderName = arg;
                    break;
            }
        }
    }
}