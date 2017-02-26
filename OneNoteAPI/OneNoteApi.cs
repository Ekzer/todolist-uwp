using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Todolist.Authentication;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using HtmlAgilityPack;
using Todolist.DataModel;
using System.Linq;
using Todolist;

namespace Todolist.API
{

    public class OneNote
    {
        private const string PagesEndPoint = "https://www.onenote.com/api/v1.0/me/notes/pages";

        private WebService _client;

        public OneNote()
        {

        }

        public async void Init()
        {
            _client = new WebService(new O365Auth());
            _client.SetRequestHeaders();
        }
        /// <summary>
        /// Get meta data for ALL pages under ALL of the user's notebooks/sections
        /// </summary>
        /// <returns>The Url Content</returns>

        public async Task<String> CreateNote(String htmlToDo)
        {
            StringContent toPost = new StringContent(htmlToDo,
                    System.Text.Encoding.UTF8, "text/html");
            HttpResponseMessage response = await _client.PostRequest(PagesEndPoint, toPost, "application/json");
            return JObject.Parse(await response.Content.ReadAsStringAsync())["contentUrl"].ToString();
        }

        public async Task<HttpResponseMessage> CreateNoteWithHttpResponseMessage(String htmlToDo)
        {
            StringContent toPost = new StringContent(htmlToDo,
                    System.Text.Encoding.UTF8, "text/html");
            return await _client.PostRequest(PagesEndPoint, toPost, "application/json");
        }

        public async Task<Tasklist> GetNotes()
        {
            HttpResponseMessage response = await _client.GetRequest(PagesEndPoint, "application/json");
            return await this.TranslateHttpResponse(response);
        }

        public async Task<HtmlDocument> GetNoteByUrl(String url)
        {
            HttpResponseMessage response = await _client.GetRequest(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(await response.Content.ReadAsStringAsync());
            return doc;
        }

        public async Task<string> UpdateNote(String url, String HtmlToUpdate)
        {
            HttpResponseMessage ret = await _client.DeleteRequest(url.Substring(0, url.IndexOf("content")), "application/json");
            if (ret.StatusCode == HttpStatusCode.NoContent)
            {
                ret = await this.CreateNoteWithHttpResponseMessage(HtmlToUpdate);
                if (ret.StatusCode == HttpStatusCode.Created)
                {
                    String body = await ret.Content.ReadAsStringAsync();
                    return JObject.Parse(body)["contentUrl"].ToString();
                }
            }
            return null;
        }

        public async Task<HttpResponseMessage> DeleteNote(String url)
        {
            return await _client.DeleteRequest(url.Substring(0, url.IndexOf("content")), "application/json");
        }

        private async Task<Tasklist> TranslateHttpResponse(HttpResponseMessage response)
        {
            string body = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(body);
            Tasklist tasks = new Tasklist();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JObject.Parse(body);
                HTMLParser parser = new HTMLParser();
                foreach (var item in content["value"])
                {
                    HtmlDocument noteHTML = await this.GetNoteByUrl(item["contentUrl"].ToString());
                    ToDo task = parser.Parse(noteHTML);
                    task.Url = item["contentUrl"].ToString();
                    tasks.Add(task);
                }
            }
            return tasks;
        }
    }

    partial class WebService
    {
        private HttpClient  _client;
        private IAuth       _authenticator;

        public WebService(IAuth Authenticator)
        {
            _client = new HttpClient();
            _authenticator = Authenticator;
        }

        public async void SetRequestHeaders()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authenticator.GetAuthToken());
        }

        public async Task<HttpResponseMessage> PostRequest(String route, StringContent content, String MIME_TYPE = "text/html")
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_TYPE));
            var createMessage = new HttpRequestMessage(HttpMethod.Post, route)
            {
                Content = content
            };
            return await _client.SendAsync(createMessage);
        }

        public async Task<HttpResponseMessage> GetRequest(String route, String MIME_TYPE = "text/html")
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_TYPE));
            var getMessage = new HttpRequestMessage(HttpMethod.Get, route);
            return await _client.SendAsync(getMessage);
        }

        public async Task<HttpResponseMessage> UpdateRequest(String route, StringContent content, String MIME_TYPE = "text/html")
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_TYPE));
            var patchMessage = new HttpRequestMessage(new HttpMethod("PATCH"), route)
            {
                Content = content
            };
            return await _client.SendAsync(patchMessage);
        }

        public async Task<HttpResponseMessage> DeleteRequest(String route, String MIME_TYPE = "text/html")
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_TYPE));
            var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, route);
            return await _client.SendAsync(deleteMessage);
        }
    }

    partial class HTMLParser
    {
        public ToDo Parse(HtmlDocument document)
        {
            ToDo task = new ToDo();
            HtmlNode root = document.DocumentNode.Element("html");
            try
            {
                var title = root.Descendants("title");
                var desc = root.Descendants("p");
                HtmlNode due = (desc.Count() > 1) ? desc.ElementAt(1) : null;
                if (Utils.IsAny<HtmlNode>(title) && Utils.IsAny<HtmlNode>(desc) )
                {
                    task.Done = title.First().HasAttributes ? (title.First().Attributes.First().Value.IndexOf(":completed") > -1 ? true : false) : false;
                    task.Subject = title.First().InnerText;
                    task.Description = desc.First().InnerText;
                    task.Due = (due == null || String.IsNullOrEmpty(due.InnerText) ? DateTimeOffset.Now : DateTimeOffset.Parse(due.InnerText)) ;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            catch (FormatException e)
            {
                task.Due = DateTime.Now;
            }
            return task;
        }
    }
}
