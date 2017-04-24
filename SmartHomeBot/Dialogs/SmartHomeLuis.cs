using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SmartHomeBot.Dialogs
{
    // [LuisModel("Add-your-app-id-here", "Add-your-app-key-here")]
    [LuisModel("cf8ead5a-6950-4c13-8443-ab68219cfe26", "b5c8543d1ff64261b47f1885f213ca54")]

    [Serializable]
    public class SmartHomeLuis : LuisDialog<object>
    {
        [NonSerialized]
        private HttpClient client;
        [NonSerialized]
        private HttpResponseMessage responsemain = null;
        //Add your Smart Things API endpoint here
        private const string SmartThingApiEndPoint = "/api/smartapps/installations/87d36a4b-7a83-43a2-9994-b0487a5ad3a3";
        private HttpClient InitClient()
        {
            HttpClient client = new HttpClient();
            //Add-your-Smartthings-Access-Token-Here
            string accessToken = "48de903e-0088-4f77-bda0-117139d42eb9";
            client.BaseAddress = new Uri($"https://graph.api.smartthings.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            return client;


        }

        /// <summary>
        /// No intent
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// Get status LUIS intent
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("GetStatus")]
        public async Task GetStatus(IDialogContext context, LuisResult result)
        {
            if (client == null)
            {
                client = InitClient();
            }
            responsemain = await client.GetAsync($"{SmartThingApiEndPoint}/switches/");
            responsemain.EnsureSuccessStatusCode();
            string responseBody = await responsemain.Content.ReadAsStringAsync();
            List<Switch> switches = null;
            switches = JsonConvert.DeserializeObject<List<Switch>>(responseBody);
            string responseString = "Hello user,this is the current status - ";
            foreach(Switch s in switches)
            {
                responseString += $"<br/>{s.name} is currently {s.value}";
            }
            responseString += $"<br/><br/>What do you want to do?";
            await context.PostAsync(responseString);
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// Method to  perform vaious actions on lights based on luis intent
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("SwitchEvent")]
        public async Task LightsOn(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            var location = entities.Where((entity) => entity.Type == "location").FirstOrDefault();
            var locationEntity = location == null ? "all" : location.Entity;
            var actionEntity = entities.Where(entity => entity.Type == "action").FirstOrDefault().Entity;
            
            // Just in case if same object is used. use cached client obj.
            if (client == null)
            {
                client = InitClient();
            }
            responsemain = await client.PutAsync($"{SmartThingApiEndPoint}/switches/{actionEntity}?location={locationEntity}", null);
            responsemain.EnsureSuccessStatusCode();
            await context.PostAsync($"Lights {actionEntity} done.");
            context.Wait(MessageReceived);
        }
    }

    /// <summary>
    /// DAO for switch
    /// </summary>
    public class Switch
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}