using System;

namespace Monopoly.Client.Runtime.UI.Panels.Common
{
    internal interface ICallableState
    {
        public void Show(Func<bool> callback = default);
        public void Hide();
    }
}
