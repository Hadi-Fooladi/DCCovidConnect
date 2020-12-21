using DCCovidConnect.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DCCovidConnect.ViewModels
{
    class HomeViewModel : BaseViewModel
    {
        string region = "Virginia";
        string local = "Fairfax County";
        int regionCases = 123668;
        int localCases = 830;

        int phase = 3;
        public string Region
        {
            get => region;
            set => SetProperty(ref region, value);
        }
        public string Local
        {
            get => local;
            set => SetProperty(ref local, value);
        }
        public int RegionCases
        {
            get => regionCases;
            set => SetProperty(ref regionCases, value);
        }
        public int LocalCases
        {
            get => localCases;
            set => SetProperty(ref localCases, value);
        }

        public int Phase
        {
            get => phase;
            set => SetProperty(ref phase, value);
        }

        public List<InstagramItem> Posts
        {
            get; set;
        }

        public HomeViewModel()
        {
            Posts = new List<InstagramItem>();
            GetInstagramPosts();
        }

        public void GetInstagramPosts()
        {
            HttpClient client = new HttpClient();
            string response = "";
            Task task = new Task(() =>
            {
                response = client.GetStringAsync("https://www.instagram.com/dccovidconnect/?__a=1").Result;
            });
            task.Start();
            task.Wait();
            JObject responseObject = JObject.Parse(response);
            JObject user = responseObject["graphql"]["user"].Value<JObject>();
            string ProfilePicture = user["profile_pic_url"].Value<string>();
            string FullName = user["full_name"].Value<string>();
            JArray edges = user["edge_owner_to_timeline_media"]["edges"].Value<JArray>();
            foreach (JObject edge in edges)
            {
                JObject node = edge["node"].Value<JObject>();
                int likes = node["edge_liked_by"]["count"].Value<int>();
                int comments = node["edge_media_to_comment"]["count"].Value<int>();
                JArray textNode = node["edge_media_to_caption"]["edges"].Value<JArray>();
                string text = textNode.Count != 0 ? textNode[0]["node"]["text"].Value<string>() : "";
                List<string> images = new List<string>();
                if (node.ContainsKey("edge_sidecar_to_children"))
                {
                    foreach (JObject imageEdge in node["edge_sidecar_to_children"]["edges"].Value<JArray>())
                    {
                        images.Add(imageEdge["node"]["display_url"].Value<string>());
                    }
                }
                else
                {
                    images.Add(node["display_url"].Value<string>());
                }
                Posts.Add(new InstagramItem
                {
                    FullName = FullName,
                    ProfileImage = ProfilePicture,
                    LikesCount = likes,
                    CommentsCount = comments,
                    Images = images,
                    Text = text
                });
            }
        }
    }
}
