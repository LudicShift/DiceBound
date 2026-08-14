using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    /// <summary>
    /// 웨이브 밸런스 테스트 오버레이. 씬에 이미 있는 실제 디렉터(UnitDirector/WaveDirector)를 참조해서
    /// 현재 배치된 아군 팀 기준으로 예상 DPS/승률을 계산하고, 실제 웨이브 전투도 트리거할 수 있다.
    /// </summary>
    public class WaveBalanceOverlay : MonoBehaviour
    {
        [SerializeField] private WaveBalanceCanvas canvas;
        [SerializeField] private int winRateTrials = 300;
        [SerializeField] private int dpsSampleDraws = 200;

        private UnitDirector _unitDirector;
        private WaveDirector _waveDirector;

        private List<WaveDataTableRow> _waves;
        private Dictionary<int, List<WaveEnemyPoolDataTableRow>> _waveEnemyPool;
        private Dictionary<string, UnitDataTableRow> _unitDictionary;
        private Dictionary<string, SkillDataTableRow> _skillDictionary;
        private int _selectedWaveIndex;

        public void Start()
        {
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            yield return DirectorFacade.WaitUntilInitialized();

            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _waveDirector = DirectorFacade.GetDirector<WaveDirector>();

            _waves = DataTableManager.FindAllRows<WaveDataTableRow>().OrderBy(x => x.index).ToList();
            _waveEnemyPool = DataTableManager.FindAllRows<WaveEnemyPoolDataTableRow>()
                .GroupBy(x => x.index).ToDictionary(x => x.Key, x => x.ToList());
            _unitDictionary = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);
            _skillDictionary = DataTableManager.FindAllRows<SkillDataTableRow>().ToDictionary(x => x.id);

            if (canvas)
            {
                var options = _waves.Select(w => $"Wave {w.index + 1}").ToList();
                canvas.SetWaveOptions(options, 0);
                canvas.onWaveSelected += OnWaveSelected;
                canvas.onRefreshClicked += Refresh;
                canvas.onSimulateClicked += OnSimulateClicked;
                canvas.SetStatus("Ready");
            }

            _selectedWaveIndex = _waves.Count > 0 ? _waves[0].index : 0;
        }

        private void OnWaveSelected(int dropdownIndex)
        {
            if (dropdownIndex < 0 || dropdownIndex >= _waves.Count) return;
            _selectedWaveIndex = _waves[dropdownIndex].index;
        }

        private List<BalanceUnit> GetAllyTeam()
        {
            var result = new List<BalanceUnit>();
            if (_unitDirector == null) return result;

            foreach (var unit in _unitDirector.GetAllies())
            {
                result.Add(new BalanceUnit(unit.GetData(), unit.GetTier()));
            }

            return result;
        }

        private void Refresh()
        {
            if (_waves == null || _waves.Count == 0)
            {
                if (canvas) canvas.SetStatus("웨이브 데이터 없음");
                return;
            }

            var wave = _waves.Find(w => w.index == _selectedWaveIndex);
            if (wave == null || !_waveEnemyPool.TryGetValue(_selectedWaveIndex, out var pool) || pool.Count == 0)
            {
                if (canvas) canvas.SetStatus($"Wave {_selectedWaveIndex + 1}: 적 풀 데이터 없음");
                return;
            }

            var allyTeam = GetAllyTeam();
            if (allyTeam.Count == 0)
            {
                if (canvas) canvas.SetStatus("배치된 아군이 없습니다. 먼저 유닛을 배치하세요.");
                return;
            }

            float allyDpsSum = 0f;
            float enemyDpsSum = 0f;
            for (int i = 0; i < dpsSampleDraws; i++)
            {
                var enemyTeam = WaveBalanceCalculator.DrawWaveComposition(wave, pool, _unitDictionary);
                if (enemyTeam.Count == 0) continue;
                allyDpsSum += WaveBalanceCalculator.ComputeTeamDps(allyTeam, enemyTeam, _skillDictionary);
                enemyDpsSum += WaveBalanceCalculator.ComputeTeamDps(enemyTeam, allyTeam, _skillDictionary);
            }

            var allyDps = allyDpsSum / dpsSampleDraws;
            var enemyDps = enemyDpsSum / dpsSampleDraws;
            var winRate = WaveBalanceCalculator.SimulateWinRate(allyTeam, wave, pool, _unitDictionary, _skillDictionary, winRateTrials);

            if (canvas)
            {
                canvas.SetAllyDps(allyDps);
                canvas.SetEnemyDps(enemyDps);
                canvas.SetWinRate(winRate);
                canvas.SetStatus($"Wave {_selectedWaveIndex + 1} 기준 (아군 {allyTeam.Count}명)");
            }
        }

        private void OnSimulateClicked()
        {
            if (_waveDirector == null) return;
            _waveDirector.PlayWave(_selectedWaveIndex);
            if (canvas) canvas.SetStatus($"Wave {_selectedWaveIndex + 1} 모의 전투 시작");
        }
    }
}
