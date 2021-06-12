using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Chat_Group_Console
{
    class Program
    {
        const string ConnectionString = "Endpoint=sb://chatgroup.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=EuPAd/RjQOUKcQ4yZnW1p8td5ud5A4ABUd8eV2Obtec=";
        const string TopicPath = "Chat-Group-Topic";
        static string Usr = "";
        static void Main(string[] args)
        {
            ManagementClient managementClient = new ManagementClient(ConnectionString);
            var topicExist = managementClient.TopicExistsAsync(TopicPath).GetAwaiter().GetResult();
            if (!topicExist)
            {
                try
                {
                    managementClient.CreateTopicAsync(TopicPath).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error : {ex}");
                }
            }

            var topicClient = new TopicClient(ConnectionString, TopicPath);
            Console.WriteLine("Please Type your name to start chat");

            string usr = Console.ReadLine();
            Usr = usr;
            Console.WriteLine("type exit to quite");
            var WelcomeMessageContent = new Message(Encoding.UTF8.GetBytes($"Joind the chat Group"));
            WelcomeMessageContent.Label = usr;
            topicClient.SendAsync(WelcomeMessageContent).GetAwaiter();
            var subscriptionDescription = new SubscriptionDescription(TopicPath, usr)
            {
                AutoDeleteOnIdle = new TimeSpan(0, 5, 0)
            };
            managementClient.CreateSubscriptionAsync(subscriptionDescription);
            var subscription = new SubscriptionClient(ConnectionString, TopicPath, usr);
            subscription.RegisterMessageHandler(HandleRecevedMessageAsync, ExceptionHandlerAsync);
            while (true)
            {
                var messageText = Console.ReadLine();
                if (messageText == "exit")
                {
                    subscription.CloseAsync().GetAwaiter();
                    break;
                }
                var messageContent = new Message(Encoding.UTF8.GetBytes(messageText));
                messageContent.Label = usr;
                topicClient.SendAsync(messageContent).GetAwaiter();
            }
            Console.WriteLine($"{usr} has left chat");
            //topicClient.CloseAsync().GetAwaiter();
        }

        private static Task ExceptionHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private static Task HandleRecevedMessageAsync(Message message, CancellationToken token)
        {
            if (message.Label != Usr)
            {
                Console.WriteLine($"{message.Label } : {Encoding.UTF8.GetString(message.Body)}");
            }
            return Task.CompletedTask;
        }
    }
}
