﻿using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class InitiateDefaultCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public InitiateDefaultCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Initiate", "Create counter configurations to get started.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public async override Task<bool> InvokeAsync(string paramList)
        {
            var initaiteBusiness = new InitaiteBusiness(_configBusiness, _counterBusiness);
            var messages = initaiteBusiness.CreateAll();
            foreach (var message in messages)
                OutputInformation(message);

            return true;
        }
    }
}