using CodeChallenger.Models;
using CommonLibrary.Handlers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeChallenger.Handlers
{
    public static class DownloadHandler
    {
        public static async Task DownloadProgramFiles(string fileName)
        {
            await Task.Run(() =>
            {
                Dictionary<int, Challenge> challenges = WebcrawlerHandler.GetChallenges();
                for (int index = 1; index <= challenges.Count; index++)
                {
                    CreateChallengeClass(fileName, challenges[index], index);
                }
                GenerateChallengeHandler(fileName, challenges);
            });
        }

        private static void GenerateChallengeHandler(string folderSelected,  Dictionary<int, Challenge> challenges)
        {
            using (StreamWriter writer = new StreamWriter($"{folderSelected}/ChallengeHandler.cs", false, Encoding.Unicode))
            {
                string handler = "using System;\nusing System.Collections.Generic;\nusing Challenges;\n\nnamespace ChallengeHandler\n{\n";
                handler += $"\tpublic static class ChallengeHandler \n\t{{\n";
                handler += $"\t\tpublic static List<string> GetSolutions()\n\t\t{{\n";

                handler += $"\t\t\tList<string> results = new List<string>();\n\n";
                for (int index = 1; index <= challenges.Count; index++)
                {
                    if (challenges.ContainsKey(index) && challenges[index]!= null)
                    {
                        handler += $"\t\t\tresults.Add({challenges[index].title.Replace(" ", String.Empty)}.Solution());\n";
                    } else
                    {
                        handler += $"\t\t\tresults.Add(\"\");\n";
                    }
                }
                handler += $"\n\t\t\treturn results;\n";

                handler += $"\t\t}}\n";
                handler += $"\t}}\n";
                handler += $"}}\n";
                writer.Write(handler);
            }
        }

        private static void CreateChallengeClass(string folderSelected, Challenge challenge, int index)
        {
            int hundredth = (int)((Math.Ceiling((decimal)((index - 1) / 100)) + 1) * 100);
            int tenth = (int)((Math.Ceiling((decimal)((index - 1) / 10)) + 1) * 10) + 100;

            string hundredthFolder = $"{(hundredth - 99).ToString().PadLeft(3, '0')}-{hundredth.ToString().PadLeft(3, '0')}";
            string tenthFolder = $"{(tenth - 109).ToString().PadLeft(3, '0')}-{(tenth - 100).ToString().PadLeft(3, '0')}";

            string fileLocation = @$"{folderSelected}/Challenges/{hundredthFolder}/{tenthFolder}/";
            FileHandler.CreateDirectory(fileLocation);

            string filename = $"{fileLocation}Challenge-{index.ToString().PadLeft(3, '0')}-{challenge.title.Replace(" ", String.Empty)}.cs";

            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.Unicode))
            {
                writer.Write(GenerateChallengeFunction(challenge));
            }
        }

        private static string GenerateChallengeFunction(Challenge challenge)
        {
            string description = string.Join("\n\t\t///\t\t", challenge.description.Split('\n'));

            string model = $"using System;\nusing System.Collections.Generic;\n\nnamespace Challenges\n{{\n";
            model += $"\tpublic static class { challenge.title.Replace(" ", String.Empty) } \n\t{{\n";
            model += $"\t\t/// <summary>\n";
            model += $"\t\t/// Title: {challenge.title}\n";
            model += $"\t\t/// Description: \n";
            model += $"\t\t///\t\t{description}\n";
            model += $"\t\t/// </summary>\n";
            model += $"\t\t/// <returns>Your result in string format.</returns>\n";
            model += $"\t\tpublic static string Solution()\n\t\t{{\n";
            model += $"\t\t\treturn \"\";\n";
            model += $"\t\t}}\n";
            model += $"\t}}\n";
            model += $"}}\n";
            return model;
        }
    }
}
