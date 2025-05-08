namespace Monopoly.Client.Runtime.Game.Machines.Common
{
    internal abstract class State<TState> where TState : System.Enum
    {
        internal State(TState identifier)
        {
            this.Identifier = identifier;
        }

        internal TState Identifier { get; private set; }

        internal abstract void Exit();
        internal abstract void Enter();
        internal abstract void Update();
        internal abstract TState GetNext();
    }
}
