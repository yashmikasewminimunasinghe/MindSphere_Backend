using System.Collections.Generic;

namespace MindSphereAuthAPI.Dtos
{
    public class StripeWebhookDto
    {
        public string Type { get; set; }
        public StripeWebhookData Data { get; set; }
    }

    public class StripeWebhookData
    {
        public StripeWebhookObject Object { get; set; }
    }

    public class StripeWebhookObject
    {
        public Dictionary<string, string> Metadata { get; set; }
    }
}
