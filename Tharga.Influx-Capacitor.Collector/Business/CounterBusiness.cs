﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class CounterBusiness : ICounterBusiness
    {
        public List<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config)
        {
            if (config.Groups == null) throw new NullReferenceException("No groups in config.");

            var counterGroups = new List<IPerformanceCounterGroup>();

            foreach (var group in config.Groups)
            {
                var performanceCounterInfos = new List<IPerformanceCounterInfo>();
                foreach (var counter in group.Counters)
                {
                    var performanceCounters = GetPerformanceCounters(counter.CategoryName, counter.CounterName, counter.InstanceName).ToArray();
                    if (performanceCounters.Any())
                    {
                        foreach (var performanceCounter in performanceCounters)
                        {
                            var name = counter.Name;
                            if (name == "*")
                            {
                                name = performanceCounter.InstanceName;
                            }

                            performanceCounterInfos.Add(new PerformanceCounterInfo(name, performanceCounter));
                        }
                    }
                    else
                    {
                        performanceCounterInfos.Add(new PerformanceCounterInfo(counter.Name, null));
                    }
                }

                var performanceCounterGroup = new PerformanceCounterGroup(group.Name, group.SecondsInterval, performanceCounterInfos);
                counterGroups.Add(performanceCounterGroup);
            }

            Thread.Sleep(100);

            return counterGroups;
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return PerformanceCounterCategory.GetCategories().Select(x => x.CategoryName);
        }

        public IEnumerable<string> GetCounterNames(string category)
        {
            var cat = new PerformanceCounterCategory(category);
            var instances = cat.GetInstanceNames();
            foreach (var instance in instances)
            {
                foreach (var ins in cat.GetCounters(instance).Select(x => x.CounterName))
                {
                    yield return ins;
                }
            }
        }

        public IEnumerable<string> GetInstances(string category, string counterName)
        {
            var cat = new PerformanceCounterCategory(category);
            return cat.GetInstanceNames();
        }

        private bool Match(string data, string pattern)
        {
            if (pattern == "*")
            {
                return true;
            }

            var reg = new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
            return reg.IsMatch(data);
        }

        private IEnumerable<PerformanceCounter> GetPerformanceCounters(string categoryName, string counterName, string instanceName)
        {
            var response = new List<PerformanceCounter>();
            try
            {
                //if (string.IsNullOrEmpty(instanceName))
                //{
                //    string[] counterNames;
                //    if (counterName.Contains("*"))
                //    {
                //        var cat = new PerformanceCounterCategory(categoryName);
                //        counterNames = cat.GetCounters().Where(x => Match(x.CounterName, counterName)).Select(x => x.CounterName).ToArray();
                //    }
                //    else
                //    {
                //        counterNames = new[] { counterName };
                //    }

                //    foreach (var counter in counterNames)
                //    {
                //        var processorCounter = new PerformanceCounter(categoryName, counter);
                //        processorCounter.NextValue();
                //        response.Add(processorCounter);
                //    }
                //}
                //else 
                    if ((counterName.Contains("*") || (instanceName != null && instanceName.Contains("*"))))
                {
                    var cat = new PerformanceCounterCategory(categoryName);

                    string[] instances;
                    if (instanceName.Contains("*"))
                    {
                        instances = cat.GetInstanceNames().Where(x => Match(x, instanceName)).ToArray();
                        //TODO: If this response gives no instances, this means that this counter should use null, for instance
                        if (!instances.Any())
                        {
                            instances = new[] { (string)null };
                        }
                    }
                    else
                    {
                        instances = new[] { instanceName };
                    }

                    var counterNames = new[] { counterName };
                    if (counterName.Contains("*"))
                    {
                        foreach (var instance in instances)
                        {
                            if (string.IsNullOrEmpty(instance))
                                counterNames = cat.GetCounters().Where(x => Match(x.CounterName, counterName)).Select(x => x.CounterName).ToArray();
                            else
                                counterNames = cat.GetCounters(instance).Where(x => Match(x.CounterName, counterName)).Select(x => x.CounterName).ToArray();
                        }
                    }

                    foreach(var counter in counterNames)
                    {
                        foreach (var instance in instances)
                        {
                            var processorCounter = new PerformanceCounter(categoryName, counter, instance);
                            processorCounter.NextValue();
                            response.Add(processorCounter);
                        }
                    }
                }
                else
                {
                    var processorCounter = new PerformanceCounter(categoryName, counterName, instanceName);
                    processorCounter.NextValue();
                    response.Add(processorCounter);
                }
            }
            catch (InvalidOperationException exception)
            {
                var message = exception.Message;
                message += " categoryName=" + categoryName + ", counterName=" + counterName + ", instanceName=" + (instanceName ?? "N/A");
                EventLog.WriteEntry(Constants.ServiceName, message, EventLogEntryType.Error);
            }

            return response;
        }
    }
}