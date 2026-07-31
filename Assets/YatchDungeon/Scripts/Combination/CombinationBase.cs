using KCoreKit;

namespace YatchDungeon
{
    public abstract class CombinationBase
    {
        public CombinationBase(CombinationDataTableRow data)
        {
            _priority =  data.priority;
            _name = LocalizationManager.GetLocalizedText(data.nameKey);
            _unitID = data.unitID;
            _additionalResourceId = data.additionalResourceId;
        }
        private readonly int _priority;
        private readonly string _name;
        private readonly string _unitID;
        private readonly string _additionalResourceId;
        public abstract bool Evaluate(CombinationContext context);

        public int GetPriority()
        {
            return _priority;
        }

        public  string GetName()
        {
            return _name;
        }

        public string GetUnitID()
        {
            return _unitID;
        }   
        
        public string GetAdditionalResourceId()
        {
            return _additionalResourceId;
        }
    }
}