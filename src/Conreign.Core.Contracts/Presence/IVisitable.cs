﻿using System;
using System.Threading.Tasks;
using Conreign.Core.Contracts.Communication;

namespace Conreign.Core.Contracts.Presence
{
    public interface IVisitable
    {
        Task Join(Guid userId, IObserver observer);
        Task Leave(Guid userId);
    }
}