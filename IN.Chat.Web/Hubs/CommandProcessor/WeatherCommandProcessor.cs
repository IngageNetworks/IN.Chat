using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;
using Newtonsoft.Json;

namespace IN.Chat.Web.Hubs.CommandProcessor
{
    public class WeatherCommandProcessor : ICommandProcessor
    {
        private const string url = "http://www.google.com/ig/api?weather={0}";
        private static Regex REGEX = new Regex("(?i)^(/weather )(me |at |for |in )?(.+)$");
        public string Name { get { return "Weather"; } }
        public string Description { get { return "Weather information, current & forcasts"; } }
        public string[] Usage { get { return new string[] { "/weather <city,state>" }; } }

        public bool CanProcess(Message message)
        {
            return REGEX.IsMatch(message.ProcessedContent);
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                var location = REGEX.Match(message.ProcessedContent).Groups[3].Value;
                message.ProcessedContent = GetWeather(location);
            }
        }

        private string GetWeather(string location)
        {
            const string url = "http://openweathermap.org/data/2.1/find/name?q={0}";
            var formattedInformation = string.Empty;

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us"));
                client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
                var result = client.GetAsync(String.Format(url, Uri.EscapeDataString(location))).Result;
                var content = result.Content.ReadAsStringAsync().Result;

                if (!result.IsSuccessStatusCode)
                {
                    formattedInformation = "Sorry, weather is unavaliable at the moment.";
                }
                else
                {
                    dynamic json = JsonConvert.DeserializeObject(content);
                    var city = json.list[0];
                    var name = (string)city.name;
                    var temp_current = (double)city.main.temp;
                    var temp_min = (double)city.main.temp_min;
                    var temp_max = (double)city.main.temp_max;
                    var humidity = (double)city.main.humidity;
                    var date = (DateTime)city.date;
                    var wind_speed = (double)city.wind.speed;
                    var wind_degrees = (double)city.wind.deg;
                    var description = (string)city.weather[0].description;

                    var temp_current_C = temp_current - 273.15;
                    var temp_current_F = 9.0 / 5.0 * temp_current_C + 32;

                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(name);
                    stringBuilder.AppendLine("---");
                    stringBuilder.AppendLine(description);
                    stringBuilder.AppendLine(string.Format("{0:F0}°F {1:F0}°C", temp_current_F, temp_current_C));
                    stringBuilder.AppendLine(string.Format("{0:F0}% humidity", humidity));

                    formattedInformation = stringBuilder.ToString();
                }
            }
            catch
            {
                formattedInformation = "Sorry, weather is unavaliable at the moment.";
            }

            return formattedInformation;
        }
    }
}