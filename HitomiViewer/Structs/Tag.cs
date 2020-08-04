using System;

namespace HitomiViewer.Structs
{
    public class Tag
    {
        public Types types { get; set; }
        public string name { get; set; }
        public enum Types
        {
            female,
            male,
            tag,
            type
        }

        public static Types ParseTypes(string value)
        {
            if (value.Contains(":"))
                return (Types)Enum.Parse(typeof(Tag.Types), value.Split(':')[0]);
            else
                return Types.tag;
        }
    }
}
