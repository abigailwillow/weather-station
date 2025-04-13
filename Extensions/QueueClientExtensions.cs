namespace WeatherStation.Extensions;

using System.Text;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

public static class QueueClientExtensions {
    public static Response<SendReceipt> SendMessageString(this QueueClient queueClient, string message) {
        string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
        return queueClient.SendMessage(base64Message);
    }
}