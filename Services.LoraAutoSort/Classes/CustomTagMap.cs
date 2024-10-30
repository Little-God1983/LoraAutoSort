/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

namespace Services.LoraAutoSort.Classes
{
    public class CustomTagMap
    {
        public List<string> LookForTag { get; set; } = new List<string>();
        public string MapToFolder { get; set; }
        public int Priority { get; set; }

    }

}
