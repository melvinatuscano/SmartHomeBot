
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
    [LuisModel("Add-your-app-id-here", "Add-your-app-key-here")]
    [Serializable]
    public class SmartHomeLuis : LuisDialog<object>
    {
        [NonSerialized]
        private HttpClient client;
        [NonSerialized]
        private HttpResponseMessage responsemain = null;
        //Add your Smart Things API endpoint here
        private const string SmartThingApiEndPoint = "";
        private HttpClient InitClient()
        {
            HttpClient client = new HttpClient();
            //Add-your-Smartthings-Access-Token-Here
            string accessToken = "";
            client.BaseAddress = new Uri($"https://graph.api.smartthings.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            return client;


        }
        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

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

        [LuisIntent("Lights-on")]
        public async Task LightsOn(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            var locationEntity = entities.Where((entity) => entity.Type == "location").FirstOrDefault()?.Entity;
            var actionEntity = entities.Where(entity => entity.Type == "action").FirstOrDefault().Entity;
            //check if previous clent object is cached
            if (client == null)
            {
                client = InitClient();
            }
            responsemain = await client.PutAsync($"{SmartThingApiEndPoint}/switches/{actionEntity}", null);
            responsemain.EnsureSuccessStatusCode();
            await context.PostAsync($"Lights {actionEntity} done.");
            context.Wait(MessageReceived);
        }
    }


    public class Switch
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}