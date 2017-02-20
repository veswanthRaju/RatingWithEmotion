using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SharedProject
{
    class Emotion_Core
    {
        private static string RateMessage = "You have rated this app as ";
        private static string AppID = "Your Azure EmotionApi here";
        private static string noRate = "No Rating";
        private static int Num = 1;

        private static async Task<Emotion[]> GetHappiness(Stream stream)
        {
            string emotionKey = AppID;

            var emotionClient = new EmotionServiceClient(emotionKey);

            var emotionResults = await emotionClient.RecognizeAsync(stream);

            if (emotionResults == null || emotionResults.Count() == 0)
            {
                throw new Exception("Can't detect face");
            }

            return emotionResults;
        }

        //Get the Emotion here
        public static async Task<float> GetAverageHappinessScore(Stream stream)
        {
            var emotionResults = await GetHappiness(stream);

            float happiness = 0;

            foreach (var emotionResult in emotionResults)
            {
                happiness = happiness + emotionResult.Scores.Happiness; //emotion score here
            }

            return happiness / emotionResults.Count(); //Average incase multiple faces
        }

        public static string GetHappinessMessage(float score)
        {
            score = score * 100;
            double result = Math.Round(score, 0);

            if (result <= 20)
                return RateMessage + Num;

            else if (result > 20 && result <= 40)
                return RateMessage + Num + 1;

            else if (result > 40 && result <= 60)
                return RateMessage + Num + 2;

            else if (result > 60 && result <= 80)
                return RateMessage + Num + 3;

            else if (result > 80 && result <= 100)
                return RateMessage + Num + 4;

            else
                return noRate;
        }
    }
}
