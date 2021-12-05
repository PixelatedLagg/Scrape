namespace Scrape
{
    public struct Template
    {
        public string Name;
        public string Desc;
        public string ID;
        public Template(string name, string desc, string id)
        {
            Name = name;
            Desc = desc;
            ID = id;
        }
    }
    public class DefaultTemplates
    {
        public static void Add()
        {
            Global.Templates.Add(new Template("Console App", "An application that consists of basic input/output in a terminal.", "console"));
        }
    }
}