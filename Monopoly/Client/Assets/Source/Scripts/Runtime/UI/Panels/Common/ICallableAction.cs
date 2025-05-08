using System;

namespace Monopoly.Client.Runtime.UI.Panels.Common
{
    internal interface ICallableAction
    {
        public void Show(Action callback = default);
        public void Hide();
    }
}
