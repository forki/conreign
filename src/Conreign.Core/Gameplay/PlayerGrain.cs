using System;
using System.Threading.Tasks;
using Conreign.Core.Contracts.Communication;
using Conreign.Core.Contracts.Gameplay;
using Conreign.Core.Contracts.Gameplay.Data;
using Orleans;

namespace Conreign.Core.Gameplay
{
    public class PlayerGrain : Grain<PlayerState>, IPlayerGrain
    {
        private Player _player;
        private IObserverGrain _observer;

        public override Task OnActivateAsync()
        {
            string roomId;
            var userId = this.GetPrimaryKey(out roomId);
            _observer = GrainFactory.GetGrain<IObserverGrain>(userId, roomId, null);
            InitializeState();
            _player = new Player(
                State, 
                _observer);
            return Task.CompletedTask;
        }

        public Task UpdateGameOptions()
        {
            throw new NotImplementedException();
        }

        public Task LaunchFleet()
        {
            throw new NotImplementedException();
        }

        public Task UpdateOptions(PlayerOptionsData options)
        {
            throw new NotImplementedException();
        }

        public Task UpdateGameOptions(GameOptionsData options)
        {
            throw new NotImplementedException();
        }

        public Task StartGame()
        {
            throw new NotImplementedException();
        }

        public Task LaunchFleet(FleetData fleet)
        {
            throw new NotImplementedException();
        }

        public Task EndTurn()
        {
            throw new NotImplementedException();
        }

        public Task Write(string text)
        {
            return _player.Write(text);
        }

        public Task<IRoomState> GetState()
        {
            throw new NotImplementedException();
        }

        public async Task Connect(Guid connectionId)
        {
            await _observer.Connect(connectionId);
            await _player.Connect(connectionId);
        }

        public async Task Disconnect(Guid connectionId)
        {
            await _observer.Disconnect(connectionId);
            await _player.Disconnect(connectionId);
        }

        private void InitializeState()
        {
            string roomId;
            State.UserId = this.GetPrimaryKey(out roomId);
            State.RoomId = roomId;
            if (State.Room == null)
            {
                State.Room = GrainFactory.GetGrain<ILobbyGrain>(roomId);
            }
            foreach (var connectionId in State.ConnectionIds)
            {
                _observer.Connect(connectionId);
            }
        }
    }
}