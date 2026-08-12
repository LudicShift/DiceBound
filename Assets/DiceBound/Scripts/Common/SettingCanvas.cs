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
        [Header("Volume")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private TextMeshProUGUI masterValueLabel;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TextMeshProUGUI sfxValueLabel;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private TextMeshProUGUI musicValueLabel;

        [Header("Close")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimButton;

        public void Awake()
        {
            if (masterSlider)
            {
                masterSlider.onValueChanged.AddListener(value =>
                {
                    UpdateLabel(masterValueLabel, value);
                    var manager = SettingManager.GetInstance();
                    if (manager) manager.SetMasterVolume(value);
                });
            }

            if (sfxSlider)
            {
                sfxSlider.onValueChanged.AddListener(value =>
                {
                    UpdateLabel(sfxValueLabel, value);
                    var manager = SettingManager.GetInstance();
                    if (manager) manager.SetSFXVolume(value);
                });
            }

            if (musicSlider)
            {
                musicSlider.onValueChanged.AddListener(value =>
                {
                    UpdateLabel(musicValueLabel, value);
                    var manager = SettingManager.GetInstance();
                    if (manager) manager.SetMusicVolume(value);
                });
            }

            if (closeButton) closeButton.onClick.AddListener(Hide);
            if (dimButton) dimButton.onClick.AddListener(Hide);

            Hide();
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

        private void ApplyWithoutNotify(Slider slider, TextMeshProUGUI label, float value)
        {
            if (slider) slider.SetValueWithoutNotify(value);
            UpdateLabel(label, value);
        }

        private void UpdateLabel(TextMeshProUGUI label, float value)
        {
            if (label) label.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }
}
