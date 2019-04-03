using Amazon.Lambda.Core;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace YouTubeVideoSearch
{
    public class Function
    {
        private static readonly string YOUTUBE_VIDEO_URL_BASE = "https://www.youtube.com/watch?v=";
        private static string _apiKey;
        private static YouTubeService _service;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<IList<Video>> FunctionHandler(string input, ILambdaContext context)
        {
            _apiKey = Environment.GetEnvironmentVariable("YoutubeApiKey");
            _service = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApplicationName = "SearchYoutube",
                    ApiKey = _apiKey
                }
            );
            return await GetSearchResults(input);
        }

        private static async Task<IList<Video>> GetSearchResults(string input)
        {
            var listRequest = _service.Search.List("snippet");
            listRequest.MaxResults = 5;
            listRequest.PrettyPrint = true;
            listRequest.Q = input;
            listRequest.Type = "video";
            listRequest.Fields = "items(id/videoId,snippet(description,title))";

            var listResponse = await listRequest.ExecuteAsync();
            return FormatSearchResults(listResponse);
        }

        private static IList<Video> FormatSearchResults(SearchListResponse listResponse)
        {
            var videos = new List<Video>();
            foreach (var item in listResponse.Items)
            {
                videos.Add(new Video()
                {
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    Link = YOUTUBE_VIDEO_URL_BASE + item.Id.VideoId
                });
            }

            return videos;
        }
    }
}