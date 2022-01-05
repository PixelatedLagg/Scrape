using Scrape.Code.Generation;

namespace Scrape.Cli
{
    public struct Template
    {
        public string Name;
        public string Desc;
        public string ID;
        public string Display;
        public Template(string name, string desc, string id)
        {
            if (name.Length > 15)
            {
                Error.CLIError("Template name must be below 15 characters");
            }
            if (desc.Length > 25)
            {
                Error.CLIError("Template description must be below 25 characters");
            }
            if (id.Length > 10)
            {
                Error.CLIError("Template ID must be below 10 characters");
            }
            Name = name;
            Desc = desc;
            ID = id;
            var display1 = Name;
            for (int x = 0; x < 15 - Name.Length; x++)
            {
                display1 += " ";
            }
            var display2 = Desc;
            for (int x = 0; x < 25 - Desc.Length; x++)
            {
                display2 += " ";
            }
            var display3 = ID;
            for (int x = 0; x < 10 - ID.Length; x++)
            {
                display3 += " ";
            }
            Display = $"{display1} {display2} {display3}";
        }
    }
    public class DefaultTemplates
    {
        public static void Add()
        {
            Global.Templates.Add(new Template("Terminal App", "A terminal application", "terminal"));
        }
    }
}