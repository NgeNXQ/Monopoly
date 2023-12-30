using System;

internal interface IStateControlUI
{
    public void Show(Func<bool> stateCallback = default);

    public void Hide();
}
