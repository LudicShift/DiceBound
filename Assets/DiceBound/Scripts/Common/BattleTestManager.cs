using System.Collections;
using System.Collections.Generic;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    /// <summary>
    /// 스킬 이펙트 / 유닛 애니메이션 확인용 테스트 하네스.
    /// 데미지·사망 등 전투 로직은 일절 다루지 않고 연출만 재생한다.
    /// </summary>
    public class BattleTestManager : MonoBehaviour
    {
        private List<UnitDataTableRow> _unitDataTableRow;
        private List<SkillEffectDataTableRow> _skillEffectDataList;

        [SerializeField] private BattleTestCanvas canvas;

        [Header("Spawn")]
        [SerializeField] private UnitCore allyPrefab;
        [SerializeField] private UnitCore enemyPrefab;
        [SerializeField] private Transform allySpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;

        [Header("Effects")]
        [Tooltip("Assets/DiceBound/Contents/SkillEffect 프리팹 목록")]
        [SerializeField] private List<SkillEffect> effectPrefabs = new List<SkillEffect>();

        [Header("Playback")]
        [SerializeField] private float interval = 1f;

        private UnitCore _ally;
        private UnitCore _enemy;
        private readonly List<SkillEffect> _liveEffects = new List<SkillEffect>();
        private Coroutine _loop;
        private bool _isPaused;
        private int _allyIndex;
        private int _enemyIndex;
        private int _effectIndex;

        public void Start()
        {
            _unitDataTableRow = DataTableManager.FindAllRows<UnitDataTableRow>();
            _skillEffectDataList = DataTableManager.FindAllRows<SkillEffectDataTableRow>();

            MergeRegisteredEffects();
            SpawnUnits();
            BindCanvas();
            ApplyAppearance();
            SetStatus("Ready");
        }

        public void OnDisable()
        {
            Time.timeScale = 1f;
        }

        /// <summary>데이터테이블에 등록된 이펙트 중 목록에 없는 것을 합친다.</summary>
        private void MergeRegisteredEffects()
        {
            if (_skillEffectDataList == null) return;
            foreach (var row in _skillEffectDataList)
            {
                if (row == null || row.prefab == null) continue;
                if (!effectPrefabs.Contains(row.prefab)) effectPrefabs.Add(row.prefab);
            }
        }

        private void SpawnUnits()
        {
            if (allyPrefab && allySpawnPoint)
            {
                _ally = Instantiate(allyPrefab, allySpawnPoint.position, Quaternion.identity, allySpawnPoint);
                _ally.name = "TestAlly";
                _ally.FlipSprite(false);
            }

            if (enemyPrefab && enemySpawnPoint)
            {
                _enemy = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity, enemySpawnPoint);
                _enemy.name = "TestEnemy";
                _enemy.FlipSprite(true);
            }
        }

        private void BindCanvas()
        {
            if (!canvas) return;

            var unitNames = new List<string>();
            if (_unitDataTableRow != null)
            {
                foreach (var row in _unitDataTableRow) unitNames.Add(row.id);
            }

            var effectNames = new List<string>();
            foreach (var fx in effectPrefabs) effectNames.Add(fx ? fx.name : "(none)");

            _enemyIndex = unitNames.Count > 1 ? 1 : 0;
            canvas.SetUnitOptions(unitNames, _allyIndex, _enemyIndex);
            canvas.SetEffectOptions(effectNames, _effectIndex);
            canvas.SetInterval(interval);
            canvas.SetPaused(false);

            canvas.onPlay += OnPlay;
            canvas.onPause += OnPause;
            canvas.onStop += OnStop;
            canvas.onIntervalChanged += value => interval = value;
            canvas.onAllySelected += index => { _allyIndex = index; ApplyAppearance(); };
            canvas.onEnemySelected += index => { _enemyIndex = index; ApplyAppearance(); };
            canvas.onEffectSelected += index => _effectIndex = index;
        }

        private void ApplyAppearance()
        {
            var allyRow = GetUnitRow(_allyIndex);
            if (_ally && allyRow)
            {
                _ally.SetupAppearanceOnly(allyRow);
                _ally.ResetVisualState();
                _ally.FlipSprite(false);
                _ally.Animate("Idle");
            }

            var enemyRow = GetUnitRow(_enemyIndex);
            if (_enemy && enemyRow)
            {
                _enemy.SetupAppearanceOnly(enemyRow);
                _enemy.ResetVisualState();
                _enemy.FlipSprite(true);
                _enemy.Animate("Idle");
            }
        }

        private UnitDataTableRow GetUnitRow(int index)
        {
            if (_unitDataTableRow == null || _unitDataTableRow.Count == 0) return null;
            return _unitDataTableRow[Mathf.Clamp(index, 0, _unitDataTableRow.Count - 1)];
        }

        private SkillEffect GetEffectPrefab()
        {
            if (effectPrefabs.Count == 0) return null;
            return effectPrefabs[Mathf.Clamp(_effectIndex, 0, effectPrefabs.Count - 1)];
        }

        // ---------- 재생 제어 ----------

        private void OnPlay()
        {
            Time.timeScale = 1f;
            _isPaused = false;
            if (canvas) canvas.SetPaused(false);

            if (_loop == null) _loop = StartCoroutine(AttackLoop());
            SetStatus("Playing");
        }

        private void OnPause()
        {
            if (_loop == null) return;
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            if (canvas) canvas.SetPaused(_isPaused);
            SetStatus(_isPaused ? "Paused" : "Playing");
        }

        private void OnStop()
        {
            if (_loop != null)
            {
                StopCoroutine(_loop);
                _loop = null;
            }

            _isPaused = false;
            Time.timeScale = 1f;
            if (canvas) canvas.SetPaused(false);

            ClearEffects();

            if (_ally) _ally.Animate("Idle");
            if (_enemy) _enemy.Animate("Idle");
            SetStatus("Stopped");
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                FireOnce();
                yield return new WaitForSeconds(Mathf.Max(0.05f, interval));
            }
        }

        private void FireOnce()
        {
            if (!_ally || !_enemy) return;
            _ally.ShowAttackAnimation(LaunchEffect);
        }

        private void LaunchEffect()
        {
            var prefab = GetEffectPrefab();
            if (!prefab)
            {
                StartCoroutine(PlayHitAfter(0f));
                return;
            }

            var effect = Instantiate(prefab, _ally.transform.position, Quaternion.identity);
            effect.SetPosition(_ally.transform.position);
            _liveEffects.Add(effect);

            effect.Play(_enemy, spent =>
            {
                _liveEffects.Remove(spent);
                if (spent) Destroy(spent.gameObject);
            });

            StartCoroutine(PlayHitAfter(effect.GetImpactDelay()));
        }

        private IEnumerator PlayHitAfter(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (_enemy) _enemy.PlayHitAnimation();
        }

        private void ClearEffects()
        {
            foreach (var effect in _liveEffects)
            {
                if (effect) Destroy(effect.gameObject);
            }
            _liveEffects.Clear();
        }

        private void SetStatus(string text)
        {
            if (canvas) canvas.SetStatus(text);
        }
    }
}
