using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;

        // تذكر تغيير الـ _chatId إلى معرف حسابك الشخصي أو معرف الجروب الصحيح
        private readonly string _botToken = "8618073838:AAEoQorV5RWBSwkdcRN90_Jr22lVV4-XrNY";
        private readonly string _chatId = "-5347946926";

        public TelegramService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> SendReportAsync(string message, List<IFormFile> images)
        {
            try
            {
                // رسالة فقط
                if (images == null || images.Count == 0)
                {
                    return await SendMessage(message);
                }

                // صورة واحدة
                if (images.Count == 1)
                {
                    return await SendSinglePhoto(message, images[0]);
                }

                // أكثر من صورة
                return await SendMediaGroup(message, images);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in TelegramService: {ex}");
                return false;
            }
        }

        private async Task<bool> SendMessage(string message)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            using var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", _chatId),
                new KeyValuePair<string, string>("text", message ?? "")
            });

            var response = await _httpClient.PostAsync(url, form);

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Telegram Response (SendMessage): {responseString}");

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> SendSinglePhoto(string message, IFormFile file)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendPhoto";

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_chatId), "chat_id");
            form.Add(new StringContent(message ?? ""), "caption");

            using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "photo", file.FileName);

            var response = await _httpClient.PostAsync(url, form);

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Telegram Response (SendSinglePhoto): {responseString}");

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> SendMediaGroup(string message, List<IFormFile> images)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMediaGroup";

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_chatId), "chat_id");

            var media = new List<Dictionary<string, object>>();

            // قائمة للاحتفاظ بالـ Streams والـ Contents مفتوحة لحين إتمام الـ Request
            var resourcesToDispose = new List<IDisposable>();

            try
            {
                for (int i = 0; i < images.Count; i++)
                {
                    var file = images[i];
                    var stream = file.OpenReadStream();
                    resourcesToDispose.Add(stream);

                    var fileContent = new StreamContent(stream);
                    resourcesToDispose.Add(fileContent);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                    var attachName = $"photo{i}";
                    form.Add(fileContent, attachName, file.FileName);

                    var item = new Dictionary<string, object>
                    {
                        { "type", "photo" },
                        { "media", $"attach://{attachName}" }
                    };

                    if (i == 0)
                    {
                        item.Add("caption", message ?? "");
                    }

                    media.Add(item);
                }

                var mediaJson = JsonSerializer.Serialize(media);
                form.Add(new StringContent(mediaJson), "media");

                var response = await _httpClient.PostAsync(url, form);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Telegram Response (SendMediaGroup): {responseString}");

                return response.IsSuccessStatusCode;
            }
            finally
            {
                // تنظيف الموارد بعد انتهاء الـ Request تماماً
                foreach (var resource in resourcesToDispose)
                {
                    resource.Dispose();
                }
            }
        }
    }
}