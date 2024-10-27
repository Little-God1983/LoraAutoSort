namespace Services.LoraAutoSort.Helper
{
    internal static class StaticFileTypes
    {
        public static readonly string[] ModelExtensions =
        [
            ".ckpt",
            ".safetensors",
            ".pth",
            ".pt"
        ];

        public static readonly string[] GeneralExtensions = [".safetensors", ".json", ".preview.png", ".preview.jpeg", ".pt", ".cm-info.json"];
    }
}
