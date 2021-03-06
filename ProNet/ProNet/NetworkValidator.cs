﻿using System;
using System.Linq;

namespace ProNet
{
    public class NetworkValidator : INetworkValidator
    {
        public void Validate(Network network)
        {
            if (network.Programmer == null)
                throw new ArgumentException("Network has no list of programmers");

            if (network.Programmer.Any(item => item.name == null))
                throw new ArgumentException("Network has a programmer with no name");

            if (network.Programmer.Any(item => item.Recommendations == null))
                throw new ArgumentException("Network has a programmer with no Recommendations");

            if (network.Programmer.Any(item => item.Skills == null))
                throw new ArgumentException("Network has a programmer with no Skills");
        }
    }
}