using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HabitTracker_Lamda
{
    public class Function
    {


         Dictionary<string,string> headers= new Dictionary<string, string>()
                {
                    {"Access-Control-Allow-Headers","Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
{"Access-Control-Allow-Origin","*" }

                };

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
          
            

           
            var response = input.HttpMethod switch
            {
                "GET" =>await HandleGet(input),
                "POST" =>await HandlePost(input),
                "PUT" =>await HandlePut(input, context),
                "DELETE" =>await HandleDelete(input),
                _ => null,
            };      //await HandleGet(id);




            return response;
        }

        private  async Task<APIGatewayProxyResponse>HandleGet_old(APIGatewayProxyRequest request)
        {

            
            //Dictionary<string,string>dadas=new Dictionary<string, string>() { { "asd", "asds" } }

            var id = request.QueryStringParameters["id"];
            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            var list =await dynamoDBContext.QueryAsync<UserHabit>(id).GetRemainingAsync();

            var response = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(list),
                Headers=headers
            };
            return response;
        }


        //sample.api.com/v1/users/:userId/habits
        private async Task<APIGatewayProxyResponse> HandleGet(APIGatewayProxyRequest request)
        {


            //Dictionary<string,string>dadas=new Dictionary<string, string>() { { "asd", "asds" } }


            var id = request.PathParameters["userid"];
            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            var list = await dynamoDBContext.QueryAsync<UserHabit>(id).GetRemainingAsync();

            var response = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(list),
                Headers = headers
            };
            return response;
        }
        private async Task<APIGatewayProxyResponse> HandlePost(APIGatewayProxyRequest request)
        {

            UserHabit habit = JsonSerializer.Deserialize<UserHabit>(request.Body);

            //Dictionary<string,string>dadas=new Dictionary<string, string>() { { "asd", "asds" } }
            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            await dynamoDBContext.SaveAsync<UserHabit>(habit);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Inserted successfully",
                Headers = headers
            };
            return response;
        }
        private async Task<APIGatewayProxyResponse> HandlePut(APIGatewayProxyRequest request, ILambdaContext context)
        {
           
            var userId = request.PathParameters["userid"];
            var habitId = request.PathParameters["habitid"];

            context.Logger.Log("user id" + userId + " habitId" + habitId);
            var checkInDates = JsonSerializer.Deserialize<List<string>>(request.Body);


            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
            var userHabit = await dynamoDBContext.LoadAsync<UserHabit>(userId, habitId);
            userHabit.CheckinDate = checkInDates;
            await dynamoDBContext.SaveAsync(userHabit);

            //await  Task.Delay(1000);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Updated successfully",
                Headers = headers
            };
            return response;
        }
        private async Task<APIGatewayProxyResponse> HandleDelete(APIGatewayProxyRequest request)
        {
            var userId = request.PathParameters["userid"];
            var habitId = request.PathParameters["habitid"];

            var dynamoDBContext = new DynamoDBContext(new AmazonDynamoDBClient());
           await dynamoDBContext.DeleteAsync<UserHabit>(userId,habitId);

           // await Task.Delay(1000);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Deleted successfully",
                Headers = headers
            };
            return response;
        }
    }
    public class UserHabit
    {
        public string UserId { get; set; }
        public string HabitId { get; set; }

        public string HabitName { get; set; }

        public List<string> CheckinDate { get; set; }
    }
}
