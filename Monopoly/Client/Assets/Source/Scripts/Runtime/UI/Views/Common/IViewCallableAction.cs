using System;

namespace Monopoly.Client.Runtime.UI.Views.Common
{
    internal interface IViewCallableAction
    {
        public void Show(Action callback = default);
        public void Hide();
    }
}
