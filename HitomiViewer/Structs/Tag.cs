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
            type,
            artist,
            character,
            group,
            series,
            none
        }

        public static Types ParseTypes(string value)
        {
            if (value.Contains(":"))
            {
                Types type;
                bool err = Enum.TryParse<Types> (value.Split(':')[0], out type);
                if (err)
                    return type;
                else
                {
                    switch (value.Split(':')[0])
                    {
                        case "남":
                            return Types.male;
                        case "여":
                            return Types.female;
                        case "캐릭":
                            return Types.character;
                        case "태그":
                            return Types.tag;
                        case "종류":
                            return Types.type;
                        default:
                            return Types.none;
                    }
                }
            }
            else
                return Types.tag;
        }
    }
}
