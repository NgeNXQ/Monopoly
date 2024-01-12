using System;

internal interface IActionControlUI
{
    public void Show(Action actionCallback = default);

    public void Hide();
}
