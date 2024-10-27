﻿namespace Services.LoraAutoSort.Classes
{
    public class ModelClass
    {
        public string DiffusionBaseModel { get; set; }
        public List<FileInfo> AssociatedFilesInfo { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public CivitaiBaseCategories CivitaiCategory { get; set; } = CivitaiBaseCategories.UNASSIGNED;
    }
}
