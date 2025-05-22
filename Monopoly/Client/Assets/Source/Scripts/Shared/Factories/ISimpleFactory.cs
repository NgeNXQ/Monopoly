namespace Monopoly.Client.Shared.Factories
{
    internal interface ISimpleFactory<TOutput, TInput>
    {
        TOutput Create(TInput input);
    }
}
