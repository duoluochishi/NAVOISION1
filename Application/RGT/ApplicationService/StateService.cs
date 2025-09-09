using NV.CT.RGT.ApplicationService.Contract.Interfaces;

namespace NV.CT.RGT.ApplicationService.Impl;

public class StateService : IStateService
{
    public event EventHandler? AnimationFinished;

    public void AnimationComplete()
    {
        AnimationFinished?.Invoke(this, EventArgs.Empty);
    }
}