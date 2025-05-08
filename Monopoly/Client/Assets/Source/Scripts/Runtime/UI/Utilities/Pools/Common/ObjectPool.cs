using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.Client.Runtime.UI.Utilities.Pools.Common
{
    internal abstract class ObjectPool<TBehaviour> : MonoBehaviour where TBehaviour : MonoBehaviour
    {
        private readonly LinkedList<TBehaviour> pooledGameObjects = new LinkedList<TBehaviour>();

        internal int Count => this.pooledGameObjects.Count;

        internal TBehaviour GetInactiveObject()
        {
            foreach (TBehaviour gameObject in this.pooledGameObjects)
            {
                if (gameObject.gameObject.activeInHierarchy)
                    continue;

                gameObject.gameObject.SetActive(true);
                return gameObject;
            }

            throw new OverflowException($"Unauthorized access to {GetType().FullName}!");
        }

        internal void Clear()
        {
            this.pooledGameObjects.Clear();
        }

        internal void Append(TBehaviour gameObject)
        {
            if (this.pooledGameObjects.Contains(gameObject))
                throw new ArgumentException($"Object {gameObject.name} is already in pool {GetType().FullName}!");

            this.pooledGameObjects.AddLast(gameObject);
        }

        internal void Remove(TBehaviour gameObject)
        {
            if (!this.pooledGameObjects.Contains(gameObject))
                throw new ArgumentException($"Object {gameObject.name} is not in pool {GetType().FullName}!");

            this.pooledGameObjects.Remove(gameObject);
        }
    }
}
