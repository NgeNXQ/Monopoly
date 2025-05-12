using Monopoly.Client.Utilities.Trackers.Loading;

namespace Monopoly.Client.Utilities.Trackers.Loading
{
    internal static class LoadingTrackerUtility
    {
        internal static LoadingGroup CurrentLoadingGroup { get; private set; }

        internal static void Initialize(LoadingGroup loadingGroup)
        {
            LoadingTrackerUtility.CurrentLoadingGroup = loadingGroup;
            LoadingTrackerUtility.CurrentLoadingGroup.ObjectsLoadedEvent += () => LoadingTrackerUtility.CurrentLoadingGroup = null;
        }

        internal static void Finalize()
        {

        }
    }
}
