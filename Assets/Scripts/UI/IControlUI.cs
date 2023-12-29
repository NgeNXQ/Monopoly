using System;

internal interface IControlUI
{
    public delegate void ButtonClickedCallback();

    public void Show(ButtonClickedCallback actionCallback = default, Func<bool> stateCallback = default);

    public void Hide();
}
