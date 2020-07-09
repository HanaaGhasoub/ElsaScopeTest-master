using Elsa;
using Elsa.Activities;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Activities.Workflows.Activities;
using Elsa.Scripting.JavaScript;
using Elsa.Services;
using Elsa.Services.Models;
using System.Collections;

namespace ElsaWFConsoleApp
{
	public class MultiStepApproval : IWorkflow
	{
		public void Build(IWorkflowBuilder builder)
		{
			builder
				.StartWith<SetVariable>(				
					setup =>
					{
						setup.VariableName = "GlobalVariable";
						setup.ValueExpression = new JavaScriptExpression<int>("input('GlobalVariable')");
					})				
				.Then<IntializeForeachList>(
					setup =>
					{						
						setup.ForeachList = "ForeachList";
					})
				.Then<ForEach>(
					setup =>
					{
						setup.CollectionExpression =
							new JavaScriptExpression<IList>("ForeachList");
					},
					forEach =>
					{
						forEach
							.When(OutcomeNames.Iterate)						
							.Then<SimpleApproveReject>(
								setup =>
								{									
									setup.GlobalVariable =
										new JavaScriptExpression<long>("GlobalVariable");									
								},
								executeHumanStep =>
								{
									executeHumanStep.When(HumanApprovalOutcomeNames.Rejected)
										.Then(nameof(RaiseWorkflowInstanceCompletedEvent));

									executeHumanStep.When(HumanApprovalOutcomeNames.Accepted)
										.Then("ActivityTypesLoop");
								}
							).Then(forEach);
					}).WithName("ActivityTypesLoop")				
				.Then<RaiseWorkflowInstanceCompletedEvent>(
					setup =>
					{
						setup.GlobalVariable =
							new JavaScriptExpression<long>("GlobalVariable");						
					}).WithName(nameof(RaiseWorkflowInstanceCompletedEvent))
				.Then<Finish>();
		}
	}
}
