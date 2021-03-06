using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Counter
{
    internal class CounterStartCommand : ActionCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;
        private readonly ISendBusiness _sendBusiness;
        private readonly ITagLoader _tagLoader;

        public CounterStartCommand(IConfigBusiness configBusiness, ICounterBusiness counterBusiness, ISendBusiness sendBusiness, ITagLoader tagLoader)
            : base("Start", "Start the counter and run the collector.")
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
            _sendBusiness = sendBusiness;
            _tagLoader = tagLoader;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var config = _configBusiness.LoadFiles(new string[] { });
            var counterGroups = _counterBusiness.GetPerformanceCounterGroups(config).ToArray();

            var index = 0;
            var counterGroup = QueryParam("Group", GetParam(paramList, index++), counterGroups.Select(x => new KeyValuePair<IPerformanceCounterGroup, string>(x, x.Name)));

            var processor = new Processor(_configBusiness, _counterBusiness, _sendBusiness, _tagLoader);
            processor.EngineActionEvent += EngineActionEvent;

            if (counterGroup == null)
            {
                if (!processor.RunAsync(new string[] { }).Wait(5000))
                {
                    throw new InvalidOperationException("Unable to start processor engine.");
                }
            }
            else
            {
                var counterGroupsToRead = new[] { counterGroup };
                processor.RunAsync(counterGroupsToRead);
            }

            return true;
        }

        private void EngineActionEvent(object sender, EngineActionEventArgs e)
        {
            OutputLine(e.Message, e.OutputLevel);
        }
    }
}