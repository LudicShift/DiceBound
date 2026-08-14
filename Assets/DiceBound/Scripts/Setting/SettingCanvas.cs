using KCoreKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    /// <summary>
    /// 타이틀에서 열리는 설정 창. 슬라이더 조작을 <see cref="SettingManager"/> 로 전달한다.
    /// </summary>
    public class SettingCanvas : WidgetBase
    {
        [Header("Volume")] [SerializeField] private SliderWidget masterSlider;
        [SerializeField] private TextMeshProUGUI masterValueLabel;
        [SerializeField] private SliderWidget sfxSlider;
        [SerializeField] private TextMeshProUGUI sfxValueLabel;
        [SerializeField] private SliderWidget musicSlider;
        [SerializeField] private TextMeshProUGUI musicValueLabel;

        [Header("Close")] [SerializeField] private ButtonWidget closeButton;
        [SerializeField] private ButtonWidget dimButton;

        public void Awake()
        {
            masterSlider.onValueChanged += OnMasterVolumeChanged;

            sfxSlider.onValueChanged += OnSFXVolumeChanged;


            musicSlider.onValueChanged += OnMusicVolumeChanged;

             closeButton.onClickAction = Hide; 
             dimButton.onClickAction = Hide;

            Hide();
        }

        private void OnMusicVolumeChanged(float value)
        {
            UpdateLabel(musicValueLabel, value);
            var manager = SettingManager.GetInstance();
            if (manager) manager.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            UpdateLabel(sfxValueLabel, value);
            var manager = SettingManager.GetInstance();
            if (manager) manager.SetSFXVolume(value);
        }

        private void OnMasterVolumeChanged(float value)
        {
            UpdateLabel(masterValueLabel, value);
            var manager = SettingManager.GetInstance();
            if (manager) manager.SetMasterVolume(value);
        }

        public override void Show()
        {
            base.Show();
            SyncFromManager();
        }

        /// <summary>현재 설정값을 슬라이더에 반영한다. (콜백을 거치지 않아 볼륨을 다시 쓰지 않는다)</summary>
        private void SyncFromManager()
        {
            var manager = SettingManager.GetInstance();
            if (!manager) return;

            ApplyWithoutNotify(masterSlider, masterValueLabel, manager.GetMasterVolume());
            ApplyWithoutNotify(sfxSlider, sfxValueLabel, manager.GetSFXVolume());
            ApplyWithoutNotify(musicSlider, musicValueLabel, manager.GetMusicVolume());
        }

        private void ApplyWithoutNotify(SliderWidget widget, TextMeshProUGUI label, float value)
        {
            widget.slider.SetValueWithoutNotify(value);
            UpdateLabel(label, value);
        }

        private void UpdateLabel(TextMeshProUGUI label, float value)
        {
            if (label) label.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }
}