using ChatClient.MVVM.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatClient.Log
{
    public static class ChatLogger
    {
        private static string _logFilePath = "chat_log.json";
        public static void LogMessage(ChatMessage message)
        {
            try
            {
                List<ChatMessage> existingLogs = LoadLogs();
                existingLogs.Add(message);

                string json = JsonConvert.SerializeObject(existingLogs, Formatting.Indented);
                File.WriteAllText(_logFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging message: {ex.Message}");
            }
        }

        public static List<ChatMessage> LoadLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    string json = File.ReadAllText(_logFilePath);
                    return JsonConvert.DeserializeObject<List<ChatMessage>>(json) ?? new List<ChatMessage>();
                }
                else
                {
                    return new List<ChatMessage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading logs: {ex.Message}");
                return new List<ChatMessage>();
            }
        }
    }
}
