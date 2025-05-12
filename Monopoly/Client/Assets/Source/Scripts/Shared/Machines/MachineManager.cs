using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.Client.Shared.Machines
{
    internal abstract class MachineManager<TState> : MonoBehaviour where TState : System.Enum
    {
        private protected readonly Dictionary<TState, State<TState>> states = new Dictionary<TState, State<TState>>();

        private protected State<TState> currentState;

        private void Start()
        {
            this.currentState.Enter();
        }

        private void Update()
        {
            TState nextState = this.currentState.GetNext();

            if (nextState.Equals(this.currentState.Identifier))
                this.currentState.Update();
            else
                this.Transition(nextState);
        }

        internal void Transition(TState state)
        {
            this.currentState.Exit();
            this.currentState = this.states[state];
            this.currentState.Enter();
        }
    }
}
