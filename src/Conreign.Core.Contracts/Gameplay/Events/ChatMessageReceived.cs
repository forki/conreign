﻿using System;
using Conreign.Core.Contracts.Communication;

namespace Conreign.Core.Contracts.Gameplay.Events
{
    [Serializable]
    public class ChatMessageReceived : IClientEvent
    {
        public Guid SenderId { get; set; }
        public string Text { get; set; }
    }
}