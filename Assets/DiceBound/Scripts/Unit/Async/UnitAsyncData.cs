using System;
using System.Collections.Generic;
using KCoreKit;

namespace DiceBound
{
    [Serializable]
    public class UnitAsyncData
    {
        public string unitId;
        public int tier;
        public int cellIndex;
    }

    [Serializable]
    public class UnitAsyncBoardData : ISerializeData
    {
        public string ownerId;
        public int waveIndex;
        public long capturedAtUnixSeconds;
        public List<UnitAsyncData> units = new List<UnitAsyncData>();
    }
}
