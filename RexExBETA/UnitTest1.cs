using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace RexExBETA
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("antrg", "e9e6751e");
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }



        
        [Test,Order(1)]
        public void CreateStoryWithRequiredFieldsMustReturnOK()
        {
            var story = new
            {
                Title = "First Title",
                Description = "This is a description",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdStoryId = json.GetProperty("storyId").GetString();
            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty, "storyId is null or empty.");
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]
        public void EditTheCreatedStorySholdReturnOk()
        {
            var changes = new
            {
                Title = "Edited Title", 
                Description = "Edited description", 
                Url = ""
            };


            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(changes);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));

        }

        [Test, Order(3)]
        public void GetAllStories()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            var stories = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateNewStoryWithInvalidCredentials()
        {
            var story = new
            {
                Title = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Test, Order(6)]
        public void EditNonExistingStory()
        {
            string fakeId = "000000";
            var changes = new
            {
                Title = "Edited Title",
                Description = "Edited description",
                Url = ""
            };


            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(changes);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory()
        {
            string fakeId = "000000";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            client?.Dispose();
        }
    }
}