﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Atlas.MatchPrediction.Test.Verification.Functions
{
    public class HealthCheckFunctions
    {
        [FunctionName(nameof(HealthCheck))]
        public static OkObjectResult HealthCheck([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            const string responseMessage = "This HTTP triggered function executed successfully";
            return new OkObjectResult(responseMessage);
        }
    }
}
