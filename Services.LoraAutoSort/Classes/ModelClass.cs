namespace Services.LoraAutoSort.Classes
{
    public class ModelClass
    {
        public string DiffusionBaseModel { get; set; }
        public List<FileInfo> fileInfo { get; set; }
        public CivitaiBaseCategories CivitaiCategory { get; set; }
    }
}
