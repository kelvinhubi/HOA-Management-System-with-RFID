namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Infrastructure
{
    public class ConnectivityChecker
    {
        public static async Task<bool> IsOnline()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Set a short timeout to avoid long delays if offline
                    httpClient.Timeout = TimeSpan.FromSeconds(5);

                    // Make a request to a known online endpoint
                    HttpResponseMessage response = await httpClient.GetAsync("https://google.com");
                   await Task.Delay(3000);
                    return response.IsSuccessStatusCode; // Returns true if status is 200-299
                }
            }
            catch
            {
                // If any exception occurs, assume offline
                return false;
            }
        }
    }
}
