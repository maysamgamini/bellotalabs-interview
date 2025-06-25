using BellotaLabInterview.UI.Console.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BellotaLabInterview.UI.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceProvider = ServiceConfiguration.ConfigureServices();
            var demo = new BlackjackDemo(serviceProvider);
            await demo.RunDemo();
        }
    }
}
