using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.WriteToEntityFrameworkCore
{
    using System;

    using global::Serilog;
    using global::Serilog.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Serilog.Context;

    public static class WriteToEntityframeworkCoreExtensions
    {
        public static LoggerConfiguration EntityFrameworkSink(
          this LoggerSinkConfiguration loggerConfiguration,
          Func<DbContext> dbContextProvider,
          IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new WriteToEntityFrameworkCore(dbContextProvider, formatProvider));
        }
    }
}
