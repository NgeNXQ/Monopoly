using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Monopoly.Client.Shared.Factories;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Factories
{
    internal sealed class BotCardStrategyFactory : ISimpleFactory<ICardStrategy, CardScriptableObject>
    {
        private readonly Dictionary<Type, ICardStrategy> strategies;

        internal BotCardStrategyFactory()
        {
            this.strategies = new Dictionary<Type, ICardStrategy>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Func<Type, bool> predicate = type => typeof(ICardStrategy).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;

            IEnumerable<Type> strategyTypes = assembly.GetTypes().Where(predicate);

            foreach (Type strategyType in strategyTypes)
            {
                if (!strategyType.Namespace.Contains("Mutual") && !strategyType.Namespace.Contains("Bot"))
                    continue;

                if (Activator.CreateInstance(strategyType) is ICardStrategy strategy)
                {
                    PropertyInfo cardTypeProperty = strategyType.GetProperty("CardType", bindingFlags);

                    if (cardTypeProperty == null)
                        continue;

                    Type cardType = (Type)cardTypeProperty.GetValue(strategy);

                    if (cardType != null && !this.strategies.ContainsKey(cardType))
                        this.strategies[cardType] = strategy;
                }
            }
        }

        public ICardStrategy Create(CardScriptableObject card)
        {
            if (card == null)
                throw new NullReferenceException($"{nameof(card)} cannot be null.");

            Type cardType = card.GetType();

            if (this.strategies.TryGetValue(cardType, out ICardStrategy strategy))
                return strategy;

            foreach (KeyValuePair<Type, ICardStrategy> entry in this.strategies)
            {
                if (entry.Key.IsAssignableFrom(cardType))
                    return entry.Value;
            }

            throw new ArgumentException($"No strategy found for tile type {card.GetType().FullName}.");
        }
    }
}
