namespace TG_Bot_Microcontrollers
{
    public class Esp32Client
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string esp32Address = "-";

        private async Task<string> SendCommand(string path)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{esp32Address}{path}");
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public Task<string> ToggleGreenLed()
        {
            return SendCommand("/led/green/toggle");
        }

        public Task<string> ToggleRedLed()
        {
            return SendCommand("/led/red/toggle");
        }

        public Task<string> ToggleYellowLed()
        {
            return SendCommand("/led/yellow/toggle");
        }

        public Task<string> StartServo()
        {
            return SendCommand("/servo/start");
        }

        public Task<string> StopServo()
        {
            return SendCommand("/servo/stop");
        }

        public Task<string> ResetServo()
        {
            return SendCommand("/servo/reset");
        }

        public Task<string> TurnLeft25Servo()
        {
            return SendCommand("/servo/left25");
        }

        public Task<string> TurnRight25Servo()
        {
            return SendCommand("/servo/right25");
        }
    }
}
