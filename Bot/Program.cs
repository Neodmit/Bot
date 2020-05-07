using VkNet.Enums.SafetyEnums;
using ApiAiSDK;
using ApiAiSDK.Model;
//using System.Data.Entity;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model.Attachments;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Bot
{
    class Program
    {
        static HttpClient http=new HttpClient();

        static public string GetTemperatureNow(string City)
        {
            try
            {
                string url = $"https://api.weatherbit.io/v2.0/current?city="+City+"&key=ab110df0535b429ab07bcc8d3e55fe87";
                string data = http.GetStringAsync(url).Result;
                dynamic r = JObject.Parse(data);
                return $"{r.data[0].temp}";
            }
            catch(Exception)
            {
                return "ошибка запроса";
            }
        }

        static public string[] GetForecast(string City, int day)
        {
            string[] arr = new string[3];
            string url = $"https://www.metaweather.com/api/location/search/?query=" + City;
            string data = http.GetStringAsync(url).Result;
            JArray jsonArray = JArray.Parse(data);
            dynamic r = JObject.Parse(jsonArray[0].ToString());
            string url2 = $"https://www.metaweather.com/api/location/" + $"{r.woeid}" +"/" + DateTime.Today.Year + "/" + DateTime.Today.Month + "/" + day + "/";
            string data2 = http.GetStringAsync(url2).Result;
            JArray jsonArray2 = JArray.Parse(data2);
            dynamic r2 = JObject.Parse(jsonArray2[0].ToString());
            arr[0] = $"{r2.the_temp}";
            arr[1] = $"{r2.max_temp}";
            arr[2] = $"{r2.min_temp}";
            return arr;
        }


        static public float[] GetStatistic(string City)
        {
            string woeid;
            float max = -99999, max_exp = -99999, difMax = 0, allMax = 0, min = 99999, min_exp = 99999, difMin = 0, allMin = 0, temp = 0, tempAll = 0;
            float[] ff=new float[3];    
            string url = $"https://www.metaweather.com/api/location/search/?query="+City;
            string data = http.GetStringAsync(url).Result;
            JArray jsonArray = JArray.Parse(data);
            dynamic r = JObject.Parse(jsonArray[0].ToString());
            woeid = $"{r.woeid}";
            int day=1;
            while(day< DateTime.Today.Day) {
                string url2 = $"https://www.metaweather.com/api/location/"+woeid+"/"+DateTime.Today.Year+"/"+DateTime.Today.Month+"/"+day+"/";
                string data2 = http.GetStringAsync(url2).Result;
                JArray jsonArray2 = JArray.Parse(data2);
                temp = 0;
                for (int i = 0; i < 7; i++)
                {
                    dynamic r2 = JObject.Parse(jsonArray2[i].ToString());
                    if (float.Parse($"{r2.the_temp}") > max) max = float.Parse($"{r2.the_temp}");
                    if (float.Parse($"{r2.the_temp}") < min) min = float.Parse($"{r2.the_temp}");
                    if (float.Parse($"{r2.max_temp}") > max_exp) max_exp = float.Parse($"{r2.max_temp}");
                    if (float.Parse($"{r2.min_temp}") < min_exp) min_exp = float.Parse($"{r2.min_temp}");
                    temp =temp+float.Parse($"{r2.the_temp}");
                }
                temp /= 7;
                tempAll += temp;
                difMax = max_exp - max;
                if (difMax < 0) difMax *= -1;
                allMax = (allMax + difMax);
                difMin = min_exp - min;
                if (difMin < 0) difMin *= -1;
                allMin = (allMin + difMin);
                  day++;
            }

            ff[0] = tempAll / day;
            ff[1] = allMax / day;
            ff[2] = allMin / day;

            return ff;
        }

        static void Main(string[] args)
        {

            string token = "1298268226:AAFfQhgQc5Z9EimepogQqL7APX9Nre3oz4g";
           
            TelegramBotClient client = new TelegramBotClient(token);

            client.OnMessage +=
                delegate (object sender, MessageEventArgs e)
                {         
                    if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        String[] words = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (words[0] == "Погода")
                        {
                            Console.WriteLine(e.Message.Text);
                            client.SendTextMessageAsync(
                              e.Message.Chat.Id,
                              GetTemperatureNow(words[1])
                            );
                        }
                        else if (words[0] == "Прогноз" && words[2]!=null)
                        {
                            client.SendTextMessageAsync(
                            e.Message.Chat.Id,
                                "Прогноз:"+ GetForecast(words[1], Convert.ToInt32(words[2]))[0]+
                                "\n Максимальная: "+GetForecast(words[1], Convert.ToInt32(words[2]))[1]+
                                "\n Минимальная: " + GetForecast(words[1], Convert.ToInt32(words[2]))[2]
                            );
                            
                        }
                        else if (words[0] == "Статистика")
                        {
                            client.SendTextMessageAsync(
                            e.Message.Chat.Id,
                                "Средний прогноз:" + GetStatistic(words[1])[0] +
                                "\nСреднее отклонение от максимального: " + GetStatistic(words[1])[1] +
                                "\nСреднее отклонение от минимального: " + GetStatistic(words[1])[2]
                            );

                        }
                        else {
                            client.SendTextMessageAsync(
                                e.Message.Chat.Id,
                                    "IDK"
                                    );
                        }
                    }
                };

            client.StartReceiving();
            Console.ReadKey();
        }
    }
}
