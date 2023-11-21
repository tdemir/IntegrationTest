using System.Collections.Generic;
using Integration.Service;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var itemOperationBackend = new Backend.ItemOperationBackend();

        var serviceList = new List<ItemIntegrationService>
        {
            new ItemIntegrationService(itemOperationBackend),//service1
            new ItemIntegrationService(itemOperationBackend)//service2
        };

        foreach (var service in serviceList)
        {
            ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
            ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
            ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

            Thread.Sleep(500);
        }

        while (serviceList.Any(x => x.HasActiveSave()))
        {
            Thread.Sleep(100);
        }

        Console.WriteLine("Everything recorded:");

        serviceList.First().GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
    }
}