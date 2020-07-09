using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Elsa.Activities.Console.Extensions;
using Elsa.Extensions;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.EntityFrameworkCore.DbContexts;
using Elsa.Persistence.EntityFrameworkCore.Extensions;
using Elsa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ElsaWFConsoleApp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var services = BuildServices();

            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ElsaContext>();

            // Ensure DB exists.
            await dbContext.Database.EnsureCreatedAsync();

            Console.WriteLine("Please choose what you want to do:");
            Console.WriteLine("1. Create instance");
            Console.WriteLine("2. Complete instance");
            var chosenOption = int.Parse(Console.ReadLine());
            if(chosenOption == 1)
                await CreateWorkflowInstanceAsync(services.CreateScope());
            else
                await CompleteWorkflowInstance(services.CreateScope());           
        }

        private static async Task CompleteWorkflowInstance(IServiceScope services)
        {
            var instanceStore = services.ServiceProvider.GetService<IWorkflowInstanceStore>();
            var allPendingInstances = await instanceStore.ListByStatusAsync(WorkflowStatus.Executing);
            var workflowInstance = await instanceStore.GetByCorrelationIdAsync(allPendingInstances.First().CorrelationId);
            var activitiesToStart = workflowInstance.BlockingActivities.Select(z => z.ActivityId);
            
            var invoker = services.ServiceProvider.GetService<IWorkflowInvoker>();
            await invoker.ResumeAsync(workflowInstance, null, activitiesToStart);
        }

        private static async Task CreateWorkflowInstanceAsync(IServiceScope services)
        {
            var registry = services.ServiceProvider.GetService<IWorkflowRegistry>();
            var workflowDefinition = await registry.GetWorkflowDefinitionAsync(nameof(MultiStepApproval), VersionOptions.Latest);

            // Execute the workflow.
            IDictionary<string, object> inputs = new Dictionary<string, object>();            
            inputs.Add("GlobalVariable", 1);
            var workflowInstanceId = Guid.NewGuid();
            var invoker = services.ServiceProvider.GetService<IWorkflowInvoker>();
            var executionContext = await invoker.StartAsync(workflowDefinition, new Variables(inputs), null, workflowInstanceId.ToString());            
        }

        private static IServiceProvider BuildServices()
        {
            return new ServiceCollection()
                .AddElsa(
                    x => x.AddEntityFrameworkStores<SqlServerContext>(
                        options => options
                            .UseSqlServer(@"Server=localhost; Database=ElsaTestDb; Trusted_Connection=True;MultipleActiveResultSets=True")))
                .AddConsoleActivities()
                .AddWorkflow<MultiStepApproval>()                                
                .AddActivity<SimpleApproveReject>()               
                .AddActivity<IntializeForeachList>()
                .AddActivity<RaiseWorkflowInstanceCompletedEvent>()                                   
                .BuildServiceProvider();
        }
    }
}
