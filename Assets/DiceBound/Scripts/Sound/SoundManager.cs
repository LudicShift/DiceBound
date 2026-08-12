using System;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using KCoreKit;

namespace DiceBound
{
    public class SoundManager : Singleton<SoundManager>
    {
        private static Dictionary<string,SoundID> _soundDataMap;

        private void Start()
        {
            Setup();
        }

        public void Setup()
        {
            _soundDataMap = DataTableManager.FindAllRows<SoundDataTableRow>().ToDictionary(x=>x.id,x=>new SoundID(x.audio));
        }

        public static void PlaySound(string id)
        {
            var hasKey = _soundDataMap.ContainsKey(id);
            if (hasKey && _soundDataMap.TryGetValue(id, out var soundID))
            {
                BroAudio.Play(soundID);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Sound ID '{id}' not found.");
            }
        }
    }
}