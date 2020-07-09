using Elsa.Expressions;
using Elsa.Extensions;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ElsaWFConsoleApp
{

	public class RaiseWorkflowInstanceCompletedEvent : Activity
	{
		private readonly IWorkflowExpressionEvaluator evaluator;

		public RaiseWorkflowInstanceCompletedEvent(IWorkflowExpressionEvaluator evaluator)
		{
			this.evaluator = evaluator;
		}

		public WorkflowExpression<long> GlobalVariable
		{
			get => GetState<WorkflowExpression<long>>();
			set => SetState(value);
		}

		protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context,
			CancellationToken cancellationToken)
		{			
			// this line will not work as the scope keep only the foreach variables not the global variable.
			var globalVariable = await evaluator.EvaluateAsync(GlobalVariable, context, cancellationToken);		

			return Done();
		}
	}

}