using System;
using RabbitMQUtility.Event;
using RabbitMQUtility.Model;
using RabbitMQUtility.RabbitMqClient;
using RabbitMQUtility.Serializer;

namespace RabbitMQUtilityService
{
    class Program
    {
        static void Main(string[] args)
        {
            Listening();

            Console.ReadLine();
        }
        private static void Listening()
        {
            RabbitMqClient.Instance.ActionEventMessage += mqClient_ActionEventMessage;
            RabbitMqClient.Instance.OnListening();
        }

        private static void mqClient_ActionEventMessage(EventMessageResult result)
        {
            if (result.EventMessageBytes.EventMessageMarkcode == MessageTypeConst.ZgUpdatePurchaseStatus)
            {
                var message = MessageSerializerFactory.CreateMessageSerializerInstance().Deserialize<UpdatePurchaseOrderStatusByBillIdMqContract>(result.MessageBytes);
                result.IsOperationOk = true; //处理成功
                Console.WriteLine(message.ModifiedBy);
            }
        }
    }
}
