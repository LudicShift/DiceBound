using System;
using KCoreKit;

namespace DiceBound
{
    [Serializable]
    public class AsyncPvpAuthData : ISerializeData
    {
        public string refreshToken;
        public string uid;
    }
}
