using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Monopoly.Client.Runtime.Core.Models
{
    internal sealed class MonopolyApplication : MonoBehaviour
    {
        [field: SerializeField]
        internal AssetReference SceneAssetMainMenu { get; private set; }

        [field: SerializeField]
        internal AssetReference SceneAssetMonopolyGame { get; private set; }

        [field: SerializeField]
        internal AssetReference SceneAssetLobbyUnranked { get; private set; }

        internal static MonopolyApplication Instance { get; private set; }

        private void Awake()
        {
            if (MonopolyApplication.Instance != null)
                throw new TypeInitializationException(nameof(MonopolyApplication), new ApplicationException($"Singleton has already been initialized."));

            GameObject.DontDestroyOnLoad(this);
            MonopolyApplication.Instance = this;
        }
    }
}
