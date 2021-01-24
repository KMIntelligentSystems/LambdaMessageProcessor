using Amazon.Lambda.Core;
using System;

namespace MessageProcessor
{
    public class StepFunctionTasks
    {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public StepFunctionTasks()
        {
        }


        public State HandleIncomingGraphType(State state, ILambdaContext context)
        {
            Console.WriteLine("$Message:{state.Message}");

            if (!string.IsNullOrEmpty(state.Name))
            {
                state.Message += " " + state.Name;
            }

            // Tell Step Function to wait 5 seconds before calling 
            state.WaitInSeconds = 5;

            return state;
        }

        public State ProceesGraphType(State state, ILambdaContext context)
        {
            state.Message += ", Goodbye";

            if (!string.IsNullOrEmpty(state.Name))
            {
                state.Message += " " + state.Name;
            }

            return state;
        }
    }
}
