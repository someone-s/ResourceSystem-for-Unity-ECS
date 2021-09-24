using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Eden.Resource
{
    public class WavInterface
    {
        private static WavInterface singleton;
        public static WavInterface active
        {
            get
            {
                if (singleton == null)
                    singleton = new WavInterface();
                return singleton;
            }
        }
        private WavInterface() 
        {
            wavCache = new Dictionary<string, AudioClip>();
        }

        private Dictionary<string, AudioClip> wavCache;
        public AudioClip Import(string path)
        {
            if (wavCache.ContainsKey(path))
                return wavCache[path];

            AudioClip audioClip;
            if (!File.Exists(path))
                audioClip = AudioClip.Create("", 0, 0, 1, false);
            else
            {
                byte[] wav = File.ReadAllBytes(path);

                int channelCount = wav[22];
                int frequency = BitConverter.ToInt32(wav, 24);

                int pos = 12;
                while (wav[pos] != 100
                    || wav[pos + 1] != 97
                    || wav[pos + 2] != 116
                    || wav[pos + 3] != 97)
                {
                    pos += 4;
                    uint chunkSize = BitConverter.ToUInt32(wav, pos);
                    pos += 4 + (int)chunkSize;
                }
                pos += 8;

                int sampleCount = (wav.Length - pos) / 2 / channelCount;

                float[] channelCombined = new float[sampleCount * channelCount];

                int i = 0;
                while (pos < wav.Length)
                {
                    for (int k = 0; k < channelCount; k++)
                    {
                        channelCombined[i] = (short)((wav[pos + 1] << 8) | wav[pos]) / 32768f;

                        pos += 2;
                        i++;
                    }
                }

                audioClip = AudioClip.Create("", sampleCount, channelCount, frequency, false);
                audioClip.SetData(channelCombined, 0);
            }

            wavCache.Add(path, audioClip);
            return audioClip;
        }
    }
}
