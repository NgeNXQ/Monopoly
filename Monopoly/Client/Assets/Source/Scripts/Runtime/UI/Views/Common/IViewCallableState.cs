using System;

namespace Monopoly.Client.Runtime.UI.Views.Common
{
    internal interface IViewCallableState
    {
        public void Show(Func<bool> callback = default);
        public void Hide();
    }
}
