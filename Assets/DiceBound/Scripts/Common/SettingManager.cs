using Ami.BroAudio;
using KCoreKit;

namespace DiceBound
{
    public class SettingManager : Singleton<SettingManager>
    {
        private float _masterVolume;
        private float _sfxVolume;
        private float _musicVolume;
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
            BroAudio.SetVolume(_masterVolume);
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = volume;
            BroAudio.SetVolume(BroAudioType.SFX,volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = volume;
            BroAudio.SetVolume(BroAudioType.Music,volume);
        }
    }
}