using CodeChallenger.Models;
using CommonLibrary.Handlers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodeChallenger.Handlers
{
    public static class WebcrawlerHandler
    {
        private static string EulerChallengesBaseUrl = "https://projecteuler.net/";
        private static object padlock = new object();

        public static Dictionary<int, Challenge> GetChallenges()
        {
            Dictionary<int, Challenge> challenges = GetEulerChallenges();
            return challenges;
        }

        private static Dictionary<int, Challenge> GetEulerChallenges()
        {
            Dictionary<int, Challenge> eulerChallenges = new Dictionary<int, Challenge>();
            int eulerChallengeMax = GetLatestChallengeNumber();
#if DEBUG
            //eulerChallengeMax = 20;
#endif
            Parallel.For(1, eulerChallengeMax + 1, (index) =>
            {
                try
                {
                    Challenge obtainedChallenge = ObtainEulerChallege(index);
                    lock (padlock)
                    {
                        eulerChallenges.Add(index, obtainedChallenge);
                    }
                } catch (Exception)
                {
                    lock (padlock)
                    {
                        eulerChallenges.Add(index, new Challenge() { title = $"Challenge {index} had an error" });
                    }
                }
            });

            for (int index = 1; index <= eulerChallengeMax; index++)
            {
                if (!eulerChallenges.ContainsKey(index))
                {
                    eulerChallenges.Add(index, null);
                }
                if (eulerChallenges[index] == null)
                {
                    try
                    {
                        eulerChallenges[index] = ObtainEulerChallege(index);
                    }
                    catch (Exception)
                    {
                        eulerChallenges[index] = new Challenge() { title = $"Challenge {index} had an error" };
                    }
                }
            }
            return eulerChallenges;
        }

        private static Challenge ObtainEulerChallege(int index)
        {
            Challenge challenge = new Challenge();
            HtmlDocument html = ObtainHtml($"problem={index}");

            challenge.title = RemoveNonLetters(ComplexStringHandler.StringCheckToWords(html.DocumentNode.Descendants("h2")
                .First().InnerText));
            challenge.titleOriginal = html.DocumentNode.Descendants("h2")
                .First().InnerText;
            challenge.description = html.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("problem_content")).First().InnerHtml;

            return challenge;
        }

        private static string RemoveNonLetters(string original)
        {
            return new string(Array.FindAll<char>(original.ToCharArray(), (c => char.IsLetter(c))));
        }

        private static int GetLatestChallengeNumber()
        {
            HtmlDocument html = ObtainHtml($"recent");
            HtmlNode node = html.DocumentNode.Descendants("table").First();
            node = node.Descendants("td").First();
            return Convert.ToInt32(node.InnerText);
        }

        private static HtmlDocument ObtainHtml(string url)
        {
            HttpClient client = new HttpClient();
            string htmlString = client.GetStringAsync(EulerChallengesBaseUrl + url).Result;
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlString);
            return html;
        }
    }
}
