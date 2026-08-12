using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    /// <summary>
    /// 스킬 이펙트 / 유닛 애니메이션 테스트용 UI 홀더.
    /// 게임 로직은 갖지 않고 입력을 이벤트로만 전달한다.
    /// </summary>
    public class BattleTestCanvas : MonoBehaviour
    {
        [Header("Playback")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private TextMeshProUGUI pauseLabel;

        [Header("Interval")]
        [SerializeField] private Slider intervalSlider;
        [SerializeField] private TextMeshProUGUI intervalValueLabel;

        [Header("StartUp Delay")]
        [SerializeField] private Slider startUpDelaySlider;
        [SerializeField] private TextMeshProUGUI startUpDelayValueLabel;

        [Header("Speed")]
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TextMeshProUGUI speedValueLabel;

        [Header("Selection")]
        [SerializeField] private TMP_Dropdown allyDropdown;
        [SerializeField] private TMP_Dropdown enemyDropdown;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private TMP_Dropdown attackClipDropdown;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusLabel;

        public event Action onPlay;
        public event Action onPause;
        public event Action onStop;
        public event Action<float> onIntervalChanged;
        public event Action<float> onStartUpDelayChanged;
        public event Action<float> onSpeedChanged;
        public event Action<int> onAllySelected;
        public event Action<int> onEnemySelected;
        public event Action<int> onEffectSelected;
        public event Action<int> onAttackClipSelected;

        public float Interval
        {
            get { return intervalSlider ? intervalSlider.value : 1f; }
        }

        public float StartUpDelay
        {
            get { return startUpDelaySlider ? startUpDelaySlider.value : 0f; }
        }

        public float Speed
        {
            get { return speedSlider ? speedSlider.value : 1f; }
        }

        public void Awake()
        {
            if (playButton) playButton.onClick.AddListener(() => { if (onPlay != null) onPlay(); });
            if (pauseButton) pauseButton.onClick.AddListener(() => { if (onPause != null) onPause(); });
            if (stopButton) stopButton.onClick.AddListener(() => { if (onStop != null) onStop(); });

            if (intervalSlider)
            {
                intervalSlider.onValueChanged.AddListener(value =>
                {
                    UpdateIntervalLabel(value);
                    if (onIntervalChanged != null) onIntervalChanged(value);
                });
                UpdateIntervalLabel(intervalSlider.value);
            }

            if (startUpDelaySlider)
            {
                startUpDelaySlider.onValueChanged.AddListener(value =>
                {
                    UpdateStartUpDelayLabel(value);
                    if (onStartUpDelayChanged != null) onStartUpDelayChanged(value);
                });
                UpdateStartUpDelayLabel(startUpDelaySlider.value);
            }

            if (speedSlider)
            {
                speedSlider.onValueChanged.AddListener(value =>
                {
                    UpdateSpeedLabel(value);
                    if (onSpeedChanged != null) onSpeedChanged(value);
                });
                UpdateSpeedLabel(speedSlider.value);
            }

            if (allyDropdown) allyDropdown.onValueChanged.AddListener(i => { if (onAllySelected != null) onAllySelected(i); });
            if (enemyDropdown) enemyDropdown.onValueChanged.AddListener(i => { if (onEnemySelected != null) onEnemySelected(i); });
            if (effectDropdown) effectDropdown.onValueChanged.AddListener(i => { if (onEffectSelected != null) onEffectSelected(i); });
            if (attackClipDropdown) attackClipDropdown.onValueChanged.AddListener(i => { if (onAttackClipSelected != null) onAttackClipSelected(i); });
        }

        public void SetUnitOptions(List<string> options, int allyIndex, int enemyIndex)
        {
            FillDropdown(allyDropdown, options, allyIndex);
            FillDropdown(enemyDropdown, options, enemyIndex);
        }

        public void SetEffectOptions(List<string> options, int index)
        {
            FillDropdown(effectDropdown, options, index);
        }

        public void SetAttackClipOptions(List<string> options, int index)
        {
            FillDropdown(attackClipDropdown, options, index);
        }

        public void SetInterval(float value)
        {
            if (!intervalSlider) return;
            intervalSlider.SetValueWithoutNotify(value);
            UpdateIntervalLabel(value);
        }

        public void SetStartUpDelay(float value)
        {
            if (!startUpDelaySlider) return;
            startUpDelaySlider.SetValueWithoutNotify(value);
            UpdateStartUpDelayLabel(value);
        }

        public void SetSpeed(float value)
        {
            if (!speedSlider) return;
            speedSlider.SetValueWithoutNotify(value);
            UpdateSpeedLabel(value);
        }

        public void SetPaused(bool paused)
        {
            if (pauseLabel) pauseLabel.text = paused ? "Resume" : "Pause";
        }

        public void SetStatus(string text)
        {
            if (statusLabel) statusLabel.text = text;
        }

        private void FillDropdown(TMP_Dropdown dropdown, List<string> options, int index)
        {
            if (!dropdown) return;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            if (options.Count > 0)
            {
                dropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, options.Count - 1));
            }
            dropdown.RefreshShownValue();
        }

        private void UpdateIntervalLabel(float value)
        {
            if (intervalValueLabel) intervalValueLabel.text = value.ToString("0.00") + "s";
        }

        private void UpdateStartUpDelayLabel(float value)
        {
            if (startUpDelayValueLabel) startUpDelayValueLabel.text = value.ToString("0.00") + "s";
        }

        private void UpdateSpeedLabel(float value)
        {
            if (speedValueLabel) speedValueLabel.text = value.ToString("0.00") + "x";
        }
    }
}
