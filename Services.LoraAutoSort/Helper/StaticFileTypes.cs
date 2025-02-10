/*
 * Licensed under the terms found in the LICENSE file in the root directory.
 * For non-commercial use only. See LICENSE for details.
 */

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

        public static readonly string[] GeneralExtensions = [
        ".preview.png",
        ".preview.jpeg",
        ".cm-info.json",
        ".civitai.info",
        ".safetensors",
        ".json",
        ".pt"];
    }
}
