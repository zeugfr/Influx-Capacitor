﻿using System.Threading.Tasks;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Service
{
    class ServiceStopCommand : ActionCommandBase
    {
        public ServiceStopCommand()
            : base("Stop", "Stops the service if it is running.")
        {
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result == null)
            {
                OutputWarning("Service is not installed on this machine.");
                return true;
            }

            await ServiceCommands.StopServiceAsync();

            return true;
        }
    }
}