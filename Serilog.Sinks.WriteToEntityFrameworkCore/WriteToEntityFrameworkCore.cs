﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Serilog.Core;
using global::Serilog.Events;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.WriteToEntityFrameworkCore
{
    public class WriteToEntityFrameworkCore : ILogEventSink
    {
        public readonly Func<DbContext> dbContextProvider;
        private readonly IFormatProvider _formatProvider;
        private readonly JsonFormatter _jsonFormatter;
        private readonly static object _look_object = new object(); 
        public WriteToEntityFrameworkCore(Func<DbContext> DbContextProvider, IFormatProvider formatProvider)
        {
            this._formatProvider = formatProvider;
            this._jsonFormatter = new JsonFormatter(formatProvider: formatProvider);
            dbContextProvider = DbContextProvider;
        }

        void ILogEventSink.Emit(LogEvent logEvent)
        {
            lock (_look_object)
            {
                if (logEvent is null)
                {
                    return;
                }
                try
                {
                    DbContext db = dbContextProvider.Invoke();
                    string json = this.ConvertLogEventToJson(logEvent);
                    JsonNode rootNode = JsonNode.Parse(json);
                    var properties = rootNode["Properties"];
                    var logData = new LogRecord
                    {
                        Exception = logEvent.Exception?.ToString(),
                        Level = logEvent.Level.ToString(),
                        LogEvent = json,
                        Message = this._formatProvider == null ? null : logEvent.RenderMessage(this._formatProvider),
                        MessageTemplate = logEvent.MessageTemplate?.ToString(),
                        TimeStamp = logEvent.Timestamp.DateTime.ToUniversalTime(),
                        EventId = (int?)properties?["EventId"]?["Id"],
                        SourceContext = (string?)properties?["SourceContext"] ?? string.Empty,
                        ActionId = (string?)properties?["ActionId"] ?? string.Empty,
                        ActionName = (string?)properties?["ActionName"] ?? string.Empty,
                        RequestId = (string?)properties?["RequestId"] ?? string.Empty,
                        RequestPath = (string?)properties?["RequestPath"] ?? string.Empty
                    };
                    if (db is not null)
                    {
                        db.Set<LogRecord>().Add(logData);
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            throw new NotImplementedException();
        }
        private string ConvertLogEventToJson(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                this._jsonFormatter.Format(logEvent, writer);
            }

            return sb.ToString();
        }
    }
}
