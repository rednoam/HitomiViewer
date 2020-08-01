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
    }
}
