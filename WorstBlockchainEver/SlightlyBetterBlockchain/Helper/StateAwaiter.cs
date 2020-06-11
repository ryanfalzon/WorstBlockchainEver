using SlightlyBetterBlockchain.Models.Enums;
using System;
using System.Threading;

namespace SlightlyBetterBlockchain.Helper
{
    public static class StateAwaiter
    {
        public static void Await(States state)
        {
            while (Client.State != state)
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }
    }
}