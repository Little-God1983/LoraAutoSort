/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

namespace Services.LoraAutoSort.Classes
{
    public class ModelClass
    {
        public string DiffusionBaseModel { get; set; }
        public string ModelName { get; set; }
        public List<FileInfo> AssociatedFilesInfo { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public CivitaiBaseCategories CivitaiCategory { get; set; } = CivitaiBaseCategories.UNASSIGNED;
        public bool SkipFile { get; set; }
    }
}
