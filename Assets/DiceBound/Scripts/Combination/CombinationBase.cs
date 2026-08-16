using System.Collections.Generic;
using KCoreKit;

namespace DiceBound
{
    public abstract class CombinationBase
    {
        
        private readonly string _name;
        private readonly string _unitID;
        private readonly int _priority;
        private readonly string _additionalResourceId;
        public CombinationBase(CombinationDataTableRow data)
        {
            _name = LocalizationManager.GetLocalizedText(data.nameKey);
            _unitID = data.unitID;
            _priority =  data.priority;
            _additionalResourceId = data.additionalResourceId;
        }
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
        
        public abstract bool Evaluate(CombinationContext context);
    }
}
