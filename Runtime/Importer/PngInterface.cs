using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Eden.Resource
{
    public class PngInterface
    {
        private static PngInterface singleton;
        public static PngInterface active
        {
            get
            {
                if (singleton == null)
                    singleton = new PngInterface();
                return singleton;
            }
        }
        private PngInterface()
        {
            textureCache = new Dictionary<string, Texture2D>();
        }

        private Dictionary<string, Texture2D> textureCache;
        public Texture2D Import(string path)
        {
            if (textureCache.ContainsKey(path)) 
                return textureCache[path];

            var texture = new Texture2D(2, 2);

            if (File.Exists(path))
                texture.LoadImage(File.ReadAllBytes(path));

            textureCache.Add(path, texture);

            return texture;
        }

        public void Export(string path, Texture2D texture)
        {
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
    }
}
