﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conreign.Core.Communication;
using Conreign.Core.Contracts.Communication;
using Conreign.Core.Contracts.Gameplay;
using Conreign.Core.Contracts.Gameplay.Data;
using Conreign.Core.Contracts.Presence;
using Conreign.Core.Presence;

namespace Conreign.Core.Gameplay
{
    public class Game : IGame
    {
        private readonly GameState _state;
        private readonly Hub _hub;

        public Game(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            _hub = new Hub(state.Hub);
            _state = state;
        }

        public Task<IRoomData> GetState(Guid userId)
        {
            throw new NotImplementedException();
        }


        public Task LaunchFleet(Guid userId, FleetData fleet)
        {
            throw new NotImplementedException();
        }

        public Task EndTurn(Guid userId)
        {
            throw new NotImplementedException();
        }

        internal Task CalculateTurn()
        {
            return Task.CompletedTask;
        }

        public Task Notify(ISet<Guid> users, params IClientEvent[] events)
        {
            return _hub.Notify(users, events);
        }

        public Task NotifyEverybody(params IClientEvent[] events)
        {
            return _hub.NotifyEverybody(events);
        }

        public Task NotifyEverybodyExcept(ISet<Guid> users, params IClientEvent[] events)
        {
            return _hub.NotifyEverybodyExcept(users, events);
        }

        public Task Join(Guid userId, IClientObserver observer)
        {
            return _hub.Join(userId, observer);
        }

        public Task Leave(Guid userId)
        {
            return _hub.Leave(userId);
        }
    }
}
