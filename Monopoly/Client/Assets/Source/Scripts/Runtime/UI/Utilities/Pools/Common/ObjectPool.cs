using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.Client.Runtime.UI.Utilities.Pools.Common
{
    internal abstract class ObjectPool<TComponent> : MonoBehaviour where TComponent : Component
    {
        private readonly LinkedList<TComponent> pooledComponents = new LinkedList<TComponent>();

        internal int Count => this.pooledComponents.Count;

        internal TComponent GetInactiveObject()
        {
            foreach (TComponent component in this.pooledComponents)
            {
                if (component.gameObject.activeInHierarchy)
                    continue;

                component.gameObject.SetActive(true);
                return component;
            }

            throw new OverflowException($"Unauthorized access to {GetType().FullName}!");
        }

        internal void Clear()
        {
            this.pooledComponents.Clear();
        }

        internal void Append(TComponent component)
        {
            if (this.pooledComponents.Contains(component))
                throw new ArgumentException($"Object {component.name} is already in pool {base.GetType().FullName}!");

            this.pooledComponents.AddLast(component);
        }

        internal void Remove(TComponent component)
        {
            if (!this.pooledComponents.Contains(component))
                throw new ArgumentException($"Object {component.name} is not in pool {base.GetType().FullName}!");

            this.pooledComponents.Remove(component);
        }
    }
}
