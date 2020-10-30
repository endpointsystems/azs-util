using System;
using System.Threading.Tasks;
using ats_util.Commands;
using azs_util.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace ats_util
{
    class Program
    {
        static async Task Main(string[] args) => await CommandLineApplication.ExecuteAsync<AzsUtil>(args);
    }
}
