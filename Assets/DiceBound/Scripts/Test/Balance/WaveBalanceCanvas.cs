using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    /// <summary>
    /// 웨이브 밸런스 테스트용 UI 홀더. 게임 로직은 갖지 않고 입력을 이벤트로만 전달한다.
    /// </summary>
    public class WaveBalanceCanvas : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField] private TMP_Dropdown waveDropdown;

        [Header("Result")]
        [SerializeField] private TextMeshProUGUI allyDpsLabel;
        [SerializeField] private TextMeshProUGUI enemyDpsLabel;
        [SerializeField] private TextMeshProUGUI winRateLabel;
        [SerializeField] private TextMeshProUGUI statusLabel;

        [Header("Buttons")]
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button simulateButton;

        public event Action<int> onWaveSelected;
        public event Action onRefreshClicked;
        public event Action onSimulateClicked;

        public void Awake()
        {
            if (waveDropdown) waveDropdown.onValueChanged.AddListener(i => { if (onWaveSelected != null) onWaveSelected(i); });
            if (refreshButton) refreshButton.onClick.AddListener(() => { if (onRefreshClicked != null) onRefreshClicked(); });
            if (simulateButton) simulateButton.onClick.AddListener(() => { if (onSimulateClicked != null) onSimulateClicked(); });
        }

        public void SetWaveOptions(List<string> options, int index)
        {
            if (!waveDropdown) return;
            waveDropdown.ClearOptions();
            waveDropdown.AddOptions(options);
            if (options.Count > 0)
            {
                waveDropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, options.Count - 1));
            }
            waveDropdown.RefreshShownValue();
        }

        public void SetAllyDps(float value)
        {
            if (allyDpsLabel) allyDpsLabel.text = $"아군 총 DPS: {value:0.0}";
        }

        public void SetEnemyDps(float value)
        {
            if (enemyDpsLabel) enemyDpsLabel.text = $"적군 예상 총 DPS: {value:0.0}";
        }

        public void SetWinRate(float percent)
        {
            if (winRateLabel) winRateLabel.text = $"예상 승률: {percent:0.0}%";
        }

        public void SetStatus(string text)
        {
            if (statusLabel) statusLabel.text = text;
        }
    }
}
