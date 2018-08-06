using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Clinet
{
    class Program
    {

        /// <summary>
        /// 连接配置
        /// </summary>
        private static readonly ConnectionFactory RabbitMqFactory = new
            ConnectionFactory
        {
            UserName = ConnectionFactory.DefaultUser,
            Password = ConnectionFactory.DefaultPass,
            Port = 5672,
            VirtualHost = ConnectionFactory.DefaultVHost,
            HostName = "192.168.21.199", //"localhost"
            Protocol = Protocols.DefaultProtocol
        };


        /// <summary>
        /// 路由名称
        /// </summary>
        const string ExchangeName = "cyw.exchange";

        //队列名称
        const string QueueName = "cyw.queue";

        /// <summary>
        /// 路由名称
        /// </summary>
        const string TopExchangeName = "topic.cyw.exchange";

        //队列名称
        const string TopQueueName = "topic.cyw.queue";

        static void Main(string[] args)
        {
            //DirectExchangeSendMsg();
            TopicExchangeSendMsg();
            Console.WriteLine("按任意值，退出程序");
            Console.ReadKey();
        }

        /// <summary>
        ///  单点精确路由模式
        /// </summary>
        private static void DirectExchangeSendMsg()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "direct", true, false, null);
                    channel.QueueDeclare(QueueName, true, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(QueueName, ExchangeName, QueueName);
                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    string vadata = Console.ReadLine();
                    while (vadata != null && !string.Equals(vadata, "exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var msgBody = Encoding.UTF8.GetBytes(vadata);
                        channel.BasicPublish(exchange: ExchangeName, routingKey: QueueName, basicProperties: props, body: msgBody);
                        vadata = Console.ReadLine();
                    }

                }
            }
        }


        public static void TopicExchangeSendMsg()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(TopExchangeName, "topic", durable: false, autoDelete: false, arguments: null);
                    channel.QueueDeclare(TopQueueName, durable: false, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(TopQueueName, TopExchangeName, routingKey: TopQueueName);
                    //var props = channel.CreateBasicProperties();
                    //props.Persistent = true;
                    string vadata = Console.ReadLine();
                    while (vadata != "exit")
                    {
                        var msgBody = Encoding.UTF8.GetBytes(vadata);
                        channel.BasicPublish(exchange: TopExchangeName, routingKey: TopQueueName, basicProperties: null, body: msgBody);
                        Console.WriteLine(string.Format("***发送时间:{0}，发送完成，输入exit退出消息发送", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        vadata = Console.ReadLine();
                    }
                }
            }
        }
    }
}
