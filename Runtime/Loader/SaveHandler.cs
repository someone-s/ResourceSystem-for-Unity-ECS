using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Eden.Resource
{
    public class SaveHandler
    {
        private static SaveHandler singleton;
        public static SaveHandler active
        {
            get
            {
                if (singleton == null)
                    singleton = new SaveHandler();

                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                return singleton;
            }
        }
        private SaveHandler() { }

        private static char div = Path.DirectorySeparatorChar;
        private static string documents = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        private static string root = $"{documents}{div}SandBox{div}Save";

        public List<string> avaliable => Directory.GetDirectories(root).Select(Path.GetFileName).ToList();

        public SaveFileFormat Import(string name)
        {
            GetPaths(name, out _, out _, out string savePath);

            if (!File.Exists(savePath)) return new SaveFileFormat
            {
                staticEntities = new StaticEntitySave[0]
            };

            return JsonUtility.FromJson<SaveFileFormat>(File.ReadAllText(savePath));
        }
        public void Export(string name, Texture2D thumbnail, SaveFileFormat saveFile)
        {
            GetPaths(name, out string mainPath, out string thumbnailPath, out string savePath);
            if (!Directory.Exists(mainPath)) Directory.CreateDirectory(mainPath);
            if (!File.Exists(thumbnailPath)) File.Create(thumbnailPath).Close();
            if (!File.Exists(savePath)) File.Create(savePath).Close();

            PngInterface.active.Export(thumbnailPath, thumbnail);

            File.WriteAllText(savePath, JsonUtility.ToJson(saveFile));
            File.SetLastWriteTimeUtc(savePath, DateTime.Now);
        }
        public void Info(string name, out Texture2D thumbnail, out DateTime lastWrite)
        {
            GetPaths(name, out _, out string thumbnailPath, out string savePath);

            thumbnail = PngInterface.active.Import(thumbnailPath);
            lastWrite = File.Exists(savePath) ? File.GetLastWriteTimeUtc(savePath) : DateTime.Now;
        }

        private void GetPaths(string name, out string main, out string thumbnail, out string save)
        {
            main = $"{root}{div}{name}";
            thumbnail = $"{root}{div}{name}{div}thumbnail.png";
            save = $"{root}{div}{name}{div}save.json";
        }
    }
}

