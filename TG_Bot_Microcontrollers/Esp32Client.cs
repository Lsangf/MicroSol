using System.Net.Http;

namespace TG_Bot_Microcontrollers
{
    public class Esp32Client
    {
        private readonly HttpClient httpClient = new HttpClient();

        private readonly string esp32Address = "-";

        public async Task<string> TurnOnGreenLed()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{esp32Address}/led/green/on");

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> TurnOnRedLed()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{esp32Address}/led/red/on");

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> TurnOnYellowLed()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{esp32Address}/led/yellow/on");

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
