using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls.Editors;

namespace HitomiViewer.AutoComplete
{
    public class SuggestionProvider : ISuggestionProvider
    {
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            List<Tag> tags = HiyobiTags.Tags;
            if (tags == null) HiyobiTags.LoadTags();
            if (filter.Split(' ').Length > 1)
            {
                string filterstring = string.Join(" ", filter.Split(' ').Take(filter.Split(' ').Length - 1));
                return tags.Select(x => x.name)
                           .StartsContains(filter.Split(' ').Last())
                           .Select(x => filterstring + " " + x);
            }
            else
                return tags.Select(x => x.name).StartsContains(filter.Split(' ').Last());
                //return tags.Select(x => x.name).Where(x => x.StartsWith(filter));
        }
    }

    public class OnceSuggestionProvider : ISuggestionProvider
    {
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            List<Tag> tags = HiyobiTags.Tags;
            if (tags == null) HiyobiTags.LoadTags();
            return tags.Select(x => x.name).StartsContains(filter.Split(' ').Last());
        }
    }
}
