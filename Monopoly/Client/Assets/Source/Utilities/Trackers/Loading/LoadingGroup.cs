using System;
using System.Collections.Generic;

namespace Monopoly.Client.Utilities.Trackers.Loading
{
    internal abstract class LoadingGroup
    {
        private protected Dictionary<Type, int> Objects { get; private set;}

        internal event Action ObjectsLoadedEvent;

        private protected LoadingGroup()
        {
            this.Objects = new Dictionary<Type, int>();
        }

        internal void RegisterInstance(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException($"{nameof(instance)} cannot be null.");

            if (!this.Objects.ContainsKey(instance.GetType()))
                throw new ArgumentException($"{instance.GetType().Name} is not registered in {this.GetType().Name}.");

            if (this.Objects[instance.GetType()] > 1)
                --this.Objects[instance.GetType()];
            else
                this.Objects.Remove(instance.GetType());

            if (this.Objects.Count == 0)
                this.ObjectsLoadedEvent?.Invoke();
        }
    }
}
