using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Monopoly.Client.Shared.Factories;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Factories
{
    internal sealed class BotTileStrategyFactory : ISimpleFactory<ITileStrategy, Tile>
    {
        private readonly Dictionary<Type, ITileStrategy> strategies;

        internal BotTileStrategyFactory()
        {
            this.strategies = new Dictionary<Type, ITileStrategy>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Func<Type, bool> predicate = type => typeof(ITileStrategy).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;

            IEnumerable<Type> strategyTypes = assembly.GetTypes().Where(predicate);

            foreach (Type strategyType in strategyTypes)
            {
                if (!strategyType.Namespace.Contains("Mutual") && !strategyType.Namespace.Contains("Bot"))
                    continue;

                if (Activator.CreateInstance(strategyType) is ITileStrategy strategy)
                {
                    PropertyInfo tileTypeProperty = strategyType.GetProperty("TileType", bindingFlags);

                    if (tileTypeProperty == null)
                        continue;

                    Type tileType = (Type)tileTypeProperty.GetValue(strategy);

                    if (tileType != null && !this.strategies.ContainsKey(tileType))
                        this.strategies[tileType] = strategy;
                }
            }
        }

        public ITileStrategy Create(Tile tile)
        {
            if (tile == null)
                throw new NullReferenceException($"{nameof(tile)} cannot be null.");

            Type tileType = tile.GetType();

            if (this.strategies.TryGetValue(tileType, out ITileStrategy strategy))
                return strategy;

            foreach (KeyValuePair<Type, ITileStrategy> entry in strategies)
            {
                if (entry.Key.IsAssignableFrom(tileType))
                    return entry.Value;
            }

            throw new ArgumentException($"No strategy found for tile type {tile.GetType().FullName}.");
        }
    }
}
