﻿using MediatR;

namespace Conreign.Core.Client.Messages
{
    public class StartGameCommand : IAsyncRequest
    {
        public string RoomId { get; set; }
    }
}
