﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conreign.Core.AI.Events;
using Conreign.Core.Contracts.Client;
using Serilog;

namespace Conreign.Core.AI
{
    public class BotFarm
    {
        private readonly string _id;
        private readonly IClient _client;
        private readonly IBotFactory _botFactory;
        private readonly BotFarmOptions _options;
        private readonly ILogger _logger;

        public BotFarm(string id, IClient client, IBotFactory botFactory, BotFarmOptions options)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (botFactory == null)
            {
                throw new ArgumentNullException(nameof(botFactory));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));
            }
            _id = id;
            _client = client;
            _botFactory = botFactory;
            _options = options;
            _logger = Log.Logger.ForContext("BotFarmId", id);
        }

        public async Task Run(CancellationToken? cancellationToken = null)
        {
            IDisposable logSubscription = null;
            var bots = new List<Bot>();
            try
            {
                cancellationToken = cancellationToken ?? CancellationToken.None;
                _logger.Information("[BotFarm:{BotFarmId}] Farm is starting...");
                while (_botFactory.CanCreate)
                {
                    var connectionId = Guid.NewGuid();
                    var connection = await _client.Connect(connectionId);
                    var bot = _botFactory.Create(connection);
                    bots.Add(bot);
                    _logger.Information(
                        "[BotFarm:{BotFarmId}] Bot {BotId} is connected. Connection id is {ConnectionId}.",
                        _id,
                        bot.Id,
                        connectionId);
                }
                logSubscription = bots
                    .Select(x => x.Events.Catch<IBotEvent, Exception>(e =>
                    {
                        OnError(e);
                        return Observable.Empty<IBotEvent>();
                    }))
                    .Merge()
                    .Subscribe(OnNext, OnError, OnCompleted);
                var tasks = new List<Task>();
                foreach (var bot in bots)
                {
                    tasks.Add(bot.Run(cancellationToken));
                    await Task.Delay(_options.StartupDelay, cancellationToken.Value);
                }
                _logger.Information("[BotFarm:{BotFarmId}] Farm is started.");
                var id = DiagnosticsConstants.BotFarmRunOperationId(_id);
                using (_logger.BeginTimedOperation(DiagnosticsConstants.BotFarmRunOperationDescription, id))
                {
                    await Task.WhenAll(tasks);
                }
            }
            finally
            {
                foreach (var bot in bots)
                {
                    bot.Dispose();
                    bot.Connection.Dispose();
                }
                logSubscription?.Dispose();
            }
        }

        private void OnCompleted()
        {
            _logger.Information("[BotFarm:{BotFarmId}] Event stream completed.", _id);
        }

        private void OnNext(IBotEvent @event)
        {
            _logger.Debug("[BotFarm:{BotFarmId}]. Event: {@Event}.", _id, @event);
        }

        private void OnError(Exception ex)
        {
            _logger.Error(ex, "[BotFarm:{BotFarmId}] Error: {Message}", _id, ex.Message);
        }
    }
}
