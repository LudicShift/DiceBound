using KCoreKit;

namespace DiceBound
{
    public abstract class PhaseDirectorBase : DirectorBase
    {
        public abstract void OnEnter();
        public abstract void OnExit();
    }
}