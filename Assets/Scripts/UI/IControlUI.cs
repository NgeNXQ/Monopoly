internal interface IControlUI
{
    public delegate void ButtonClickedCallback();

    public void Show(ButtonClickedCallback callback);

    public void Hide();
}
