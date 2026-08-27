using System.Collections;
using System.Collections.Generic;
using KCoreKit;

namespace DiceBound
{
    public class UnitFusionDataTableRow : DataTableRowBase
    {
        public string inputUnit1;
        public string inputUnit2;
        public string outputUnit;

        public IEnumerable<string> GetInputs()
        {
            yield return inputUnit1;
            yield return inputUnit2;
        }
    }
}