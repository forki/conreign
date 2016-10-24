﻿using Conreign.Core.Contracts.Gameplay.Data;
using MediatR;

namespace Conreign.Core.Client.Messages
{
    public class UpdatePlayerOptionsCommand : IAsyncRequest
    {
        public string RoomId { get; set; }
        public PlayerOptionsData Options { get; set; }
    }
}
