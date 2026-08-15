using System.Collections.Generic;
using System.Linq;
using KCoreKit;

namespace DiceBound
{
    public class MasteryManager : Singleton<MasteryManager>
    {
        private const string SaveFileName = "SkillTree.sav";
        private const string SaveDirectory = "MetaProgression";

        private Dictionary<string, MasteryDataTableRow> _nodeDataDictionary;
        private HashSet<string> _unlockedNodeIds = new HashSet<string>();
        private int _diamond;

        private void Awake()
        {
            DataTableManager.AddOnLoadAction(OnDataTableLoaded);
        }

        private void OnDataTableLoaded()
        {
            _nodeDataDictionary = DataTableManager.FindAllRows<MasteryDataTableRow>().ToDictionary(x => x.id);
            LoadOrDefault();
        }

        private void LoadOrDefault()
        {
            if (SaveSystem.Exist(SaveFileName, SaveDirectory))
            {
                SaveSystem.Load<SkillTreeSaveData>(SaveFileName, SaveDirectory, out var saved);
                _unlockedNodeIds = new HashSet<string>(saved.unlockedNodeIds);
                _diamond = saved.diamond;
            }
            else
            {
                _unlockedNodeIds = new HashSet<string>();
                _diamond = 0;
            }
        }

        private void Persist()
        {
            var data = new SkillTreeSaveData
            {
                unlockedNodeIds = new List<string>(_unlockedNodeIds),
                diamond = _diamond
            };
            SaveSystem.Save(data, SaveFileName, SaveDirectory, true);
        }

        public float GetModifierTotal(string effectKey)
        {
            float total = 0f;
            foreach (var id in _unlockedNodeIds)
            {
                if (_nodeDataDictionary.TryGetValue(id, out var row) && row.effectKey == effectKey)
                {
                    total += row.effectValue;
                }
            }

            return total;
        }

        public bool IsNodeUnlocked(string nodeId)
        {
            return _unlockedNodeIds.Contains(nodeId);
        }

        public bool ArePrerequisitesMet(string nodeId)
        {
            if (!_nodeDataDictionary.TryGetValue(nodeId, out var row) || row.prerequisiteIds == null)
            {
                return true;
            }

            foreach (var prerequisiteId in row.prerequisiteIds)
            {
                if (!_unlockedNodeIds.Contains(prerequisiteId))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanUnlock(string nodeId)
        {
            if (IsNodeUnlocked(nodeId) || !_nodeDataDictionary.TryGetValue(nodeId, out var row))
            {
                return false;
            }

            return ArePrerequisitesMet(nodeId) && _diamond >= row.cost;
        }

        public bool TryUnlock(string nodeId)
        {
            if (!CanUnlock(nodeId))
            {
                return false;
            }

            var row = _nodeDataDictionary[nodeId];
            _diamond -= row.cost;
            _unlockedNodeIds.Add(nodeId);
            Persist();
            return true;
        }

        public void AddDiamond(int amount)
        {
            _diamond += amount;
            Persist();
        }

        public int GetDiamond()
        {
            return _diamond;
        }

        public MasteryDataTableRow GetNode(string id)
        {
            return _nodeDataDictionary[id];
        }

        public IReadOnlyDictionary<string, MasteryDataTableRow> GetAllNodeData()
        {
            return _nodeDataDictionary;
        }
    }
}
