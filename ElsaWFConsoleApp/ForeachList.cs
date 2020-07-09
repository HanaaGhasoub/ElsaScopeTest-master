using Elsa.Expressions;
using Elsa.Extensions;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElsaWFConsoleApp
{

	public class IntializeForeachList : Activity
	{		
		public IntializeForeachList()
		{						
		}

		public string ForeachList
		{
			get => GetState<string>();
			set => SetState(value);
		}

		protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context,
			CancellationToken cancellationToken)
		{			
			var foreachList = new List<int>();
			foreachList.Add(1);

			context.CurrentScope.SetVariable(ForeachList, foreachList);

			return Done();
		}
	}

}
