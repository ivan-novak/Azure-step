using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Azure.Cosmos;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string path = "./opinion.txt";

        private List<Models.Opinion> opinions;

        #region config
        String EndpointUri = "https://novak-cosmos-databases.documents.azure.com:443/";
        String PrimaryKey = "ug83sLqrXCadSLNKDaGVJhT4Kb72HIvBnLLKBGL404CDAaTklr1YDBGKKxV2S6KMEnLYVqocPLEKEf9Bf3bNvA==";
        #endregion

        private CosmosClient cosmosClient;
        private Database database;
        private Container container;
        private String databaseId = "Opinion";
        private String containerId = "OpinionContainer";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            opinions = null;
        }

        public IActionResult Index()
        {
            if (opinions == null)
            {
                try
                {
                    using var reader = new StreamReader(path);
                    opinions = JsonSerializer
                        .Deserialize<List<Models.Opinion>>
                        (reader.ReadToEnd());

                    ViewData["Mode"] = "Read";
                }
                catch
                {
                    opinions = null;
                }
            }
            else
            {
                ViewData["Mode"] = "Repeat";
            }
            ViewData["Comments"] = opinions?.ToArray();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Translate()
        {
            String lan_1 = Request.Query["len_1"].ToString();
            if (lan_1.Length == 0)
                lan_1 = "en";
            ViewData["lan_1"] = lan_1;
            String lan_2 = Request.Query["len_2"].ToString();
            if (lan_2.Length == 0)
                lan_2 = "ru";

            String endpoint = @"https://api.cognitive.microsofttranslator.com";
            String key = "38604c918a324a0b9379c2ed43cf6588";
            String path = $"/translate?api-version=3.0&from={lan_1}&to={lan_2}";
            String region = "centralus";

            String txt = Request.Query["txt"].ToString();
            if (txt.Length == 0)
                txt = "Hello";
            String body = JsonSerializer.Serialize(
                new object[] { new { Text = txt } });
            String resp;

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + path);
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                //request.Headers.Add("Content-Type", "application/json; charset=utf-8");
                //request.Headers.Add("Content-Lenght", body.Length.ToString());

                request.Content = new StringContent(
                    body, System.Text.Encoding.UTF8, "application/json");

                resp = client
                    .SendAsync(request).Result
                    .Content.ReadAsStringAsync().Result;


            }
            ViewData["txt"] = txt;
            ViewData["resp"] = resp;
            ViewData["key"] = key;
            var json = JsonSerializer
                .Deserialize<Models.Translations[]>(resp);
            ViewData["ResultTranslation"] = "";

            if (json.Length > 0)
            {
                ViewData["AllTranslate"] = json[0].translations;
                foreach (var trans in json[0].translations)
                {
                    if (trans.to.Equals(lan_2))
                        ViewData["ResultTranslation"] = trans.text;
                }
            }

            return View();
        }
        public IActionResult ApiKey()
        {
            String body = JsonSerializer.Serialize(
                new object[] { new { Key = "38604c918a324a0b9379c2ed43cf6588" }, new { Location = "centralus" } });

            return Content(body);
        }
        [HttpPost]
        public async Task<IActionResult> ComparativeAnalysisDB(Models.AnalysisDB analysis)
        {
            String errorMessage = String.Empty;

            analysis.id = Guid.NewGuid().ToString();
            analysis.date = DateTime.Now;
            analysis.type = analysis.GetType().Name;
            try
            {
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                database = cosmosClient.GetDatabase(databaseId);
                container = database.GetContainer(containerId);
                ItemResponse<Models.AnalysisDB> result = await container.CreateItemAsync(analysis);
                //return Content(result.StatusCode.ToString() + " " + result.Resource);
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                errorMessage = String.Format("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                errorMessage = String.Format("Error: {0}", e);
            }
            finally
            {
                errorMessage += String.Format("End of demo, press any key to exit.");
            }
            return Redirect("/Home/Analysis");
            //return Content(analysis.ToString());
        }

        public async Task<IActionResult> Analysis()
		{
            String content = String.Empty;
            try
            {
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                database = cosmosClient.GetDatabase(databaseId);
                container = database.GetContainer(containerId);
                content = "OK ";

                var sqlQueryText = "SELECT * FROM c WHERE c.type=\"AnalysisDB\"";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Models.AnalysisDB> queryResultSetIterator = container.GetItemQueryIterator<Models.AnalysisDB>(queryDefinition);

                List<Models.AnalysisDB> analysisDBs = new List<Models.AnalysisDB>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Models.AnalysisDB> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Models.AnalysisDB analysis in currentResultSet)
                    {
                        analysisDBs.Add(analysis);
                        content += analysis;
                    }
                    ViewData["Analysis"] = analysisDBs?.ToArray();
                }


            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                content = String.Format("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                content = String.Format("Error: {0}", e);
            }
            finally
            {
                content += String.Format("End of demo, press any key to exit.");
            }


            return View();
        }

        public async Task<IActionResult> Cosmos()
        {
            String content = String.Empty;
            try
            {
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                database = cosmosClient.GetDatabase(databaseId);
                container = database.GetContainer(containerId);
                content = "OK ";

                var sqlQueryText = "SELECT * FROM c";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Models.UserOpinion> queryResultSetIterator = container.GetItemQueryIterator<Models.UserOpinion>(queryDefinition);

                List<Models.UserOpinion> opinions = new List<Models.UserOpinion>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Models.UserOpinion> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Models.UserOpinion opinion in currentResultSet)
                    {
                        opinions.Add(opinion);
                        content += opinion;
                    }
                    ViewData["User"] = opinions?.ToArray();
                }


            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                content = String.Format("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                content = String.Format("Error: {0}", e);
            }
            finally
            {
                content += String.Format("End of demo, press any key to exit.");
            }
            

            return View();
            //return Content(content);
        }
        [HttpPost]
        public async Task<IActionResult> CosmosOpinion(Models.UserOpinion opinion)
        {
            String errorMessage = String.Empty;
            opinion.id = Guid.NewGuid().ToString();
            try
            {
                cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                database = cosmosClient.GetDatabase(databaseId);
                container = database.GetContainer(containerId);
                ItemResponse<Models.UserOpinion> result = await container.CreateItemAsync(opinion);
                //return Content(result.StatusCode.ToString() + " " + result.Resource);
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                errorMessage = String.Format("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                errorMessage = String.Format("Error: {0}", e);
            }
            finally
            {
                errorMessage += String.Format("End of demo, press any key to exit.");
            }
            //return Content(opinion.ToString());
            return Redirect("/Home/Cosmos");
        }

        [HttpPost]
        public IActionResult TranslateWord(String text)
        {
            return Content(text);
        }

        [HttpPost]
        public IActionResult UserOpinion(Models.Opinion opinion)
        {
            opinion.Date = DateTime.Now;
            if (opinions == null)
            {
                ViewData["Mode"] = "Read";
                try
                {
                    using var reader = new StreamReader(path);
                    opinions = JsonSerializer
                        .Deserialize<List<Models.Opinion>>
                        (reader.ReadToEnd());
                }
                catch
                {
                    opinions = new List<Models.Opinion>();
                }
            }
            else
            {
                ViewData["Mode"] = "Repeat";
            }
            opinions.Add(opinion);
            // Serialization
            string comment = JsonSerializer.Serialize(opinions);
            using (StreamWriter writetext = new StreamWriter(path))
            {
                writetext.WriteLine(comment);
            }
            return Redirect("/");
            // return Content(comment);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }



 //   public class Translation
	//{
 //       public String text { get; set; }

 //       public String to { get; set; }
 //   }
 //   public class Translations
 //   {
 //       public Translation[] translations { get; set; }
 //   }




}
/*
 * XML  <data>10</data>
 *      <x>10</x>
 *      <item prop1=10 prop2=20 />
 *      <item prop1=10 prop2=20>100</item>
 * 
 * JSON {"data":"10"}
 *      {"x":"10"} 
 *      { "item":{} }
 * 
 */