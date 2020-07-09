using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Extensions;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElsaWFConsoleApp
{

    [ActivityDefinition(
      DisplayName = "Human Step",
      Category = "User Tasks",
      Outcomes = new[] { HumanApprovalOutcomeNames.Accepted, HumanApprovalOutcomeNames.Rejected }
  )]
    public class SimpleApproveReject : Activity
    {
        private readonly IWorkflowExpressionEvaluator evaluator;

        public SimpleApproveReject(IWorkflowExpressionEvaluator evaluator)            
        {
            this.evaluator = evaluator;
        }

        public WorkflowExpression<long> GlobalVariable
        {
            get => GetState<WorkflowExpression<long>>();
            set => SetState(value);
        }
       

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var globalVariable = await evaluator.EvaluateAsync(GlobalVariable, context, cancellationToken);

            return Halt(true);
        }

        protected override ActivityExecutionResult OnResume(WorkflowExecutionContext context)
        {            
            return Outcome(HumanApprovalOutcomeNames.Accepted);         
        }
    }

}
