using System;
using RabbitMQUtility.Config;
using RabbitMQUtility.Event;
using RabbitMQUtility.Model;
using RabbitMQUtility.RabbitMqClient;

namespace RabbitMQUtilityClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SendEventMessage();
            Console.ReadLine();
        }

        private static void SendEventMessage()
        {
            var originObject = new UpdatePurchaseOrderStatusByBillIdMqContract()
            {
                UpdatePurchaseOrderStatusType = 1,
                RelationBillType = 10,
                RelationBillId = 10016779,
                UpdateStatus = 30,
                ModifiedBy = 11
            };

            var sendMessage = EventMessageFactory.CreateEventMessageInstance(originObject, MessageTypeConst.ZgUpdatePurchaseStatus);
            var mqConfigDomFactory = MqConfigDomFactory.CreateConfigDomInstance();
            RabbitMqClient.Instance.TriggerEventMessage(sendMessage, mqConfigDomFactory.MqExchangeName, mqConfigDomFactory.MqListenQueueName);
            Console.WriteLine(11);
        }
    }
}