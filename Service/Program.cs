﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Service
{
    internal class Program
    {
        /// <summary>
        /// 连接配置
        /// </summary>
        //private static readonly ConnectionFactory RabbitMqFactory = new
        //    ConnectionFactory
        //    {
        //        UserName = ConnectionFactory.DefaultUser,
        //        Password = ConnectionFactory.DefaultPass,
        //        Port = 5672,
        //        VirtualHost = ConnectionFactory.DefaultVHost,
        //        HostName = "192.168.21.199", //"localhost",
        //        Protocol = Protocols.DefaultProtocol
        //    };


        private static readonly ConnectionFactory RabbitMqFactory = new
            ConnectionFactory
            {
                UserName = "rootsa",
                Password = "rootsa",
                Port = 5672,
                VirtualHost = "OrderQueue",
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
            //DirectAcceptExchange();
            //DirectAcceptExchangeEvent();
            //DirectAcceptExchangeTask();
            TopicAcceptExchange();
            Console.WriteLine("按任意值，退出程序");
            Console.ReadKey();
        }


        public static void DirectAcceptExchange()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    while (true)
                    {
                        BasicGetResult msgResponse = channel.BasicGet(QueueName, true);
                        if (msgResponse != null)
                        {
                            var msgBody = Encoding.UTF8.GetString(msgResponse.Body);
                            Console.WriteLine(string.Format("***接收时间:{0}，消息内容：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        }

                        //BasicGetResult msgResponse2 = channel.BasicGet(QueueName, noAck: false);

                        ////process message ...

                        //channel.BasicAck(msgResponse2.DeliveryTag, multiple: false);
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }
        }

        public static void DirectAcceptExchangeEvent()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    //channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("***接收时间:{0}，消息内容：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                    };
                    channel.BasicConsume(QueueName, true, consumer: consumer);

                    //已过时用EventingBasicConsumer代替
                    //var consumer2 = new QueueingBasicConsumer(channel);
                    //channel.BasicConsume(QueueName, noAck: true, consumer: consumer);
                    //var msgResponse = consumer2.Queue.Dequeue(); //blocking
                    //var msgBody2 = Encoding.UTF8.GetString(msgResponse.Body);

                    Console.WriteLine("按任意值，退出程序");
                    Console.ReadKey();
                }
            }
        }

        public static void DirectAcceptExchangeTask()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//告诉broker同一时间只处理一个消息
                    //channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("***接收时间:{0}，消息内容：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        int dots = msgBody.Split('.').Length - 1;
                        System.Threading.Thread.Sleep(dots * 1000);
                        Console.WriteLine(" [x] Done");
                        //处理完成，告诉Broker可以服务端可以删除消息，分配新的消息过来
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    //noAck设置false,告诉broker，发送消息之后，消息暂时不要删除，等消费者处理完成再说
                    channel.BasicConsume(QueueName,  false, consumer: consumer);

                    Console.WriteLine("按任意值，退出程序");
                    Console.ReadKey();
                }
            }
        }

        public static void TopicAcceptExchange()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(TopExchangeName, "topic", durable: false, autoDelete: false, arguments: null);
                    channel.QueueDeclare(TopQueueName, durable: false, autoDelete: false, exclusive: false, arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    channel.QueueBind(TopQueueName, TopExchangeName, routingKey: TopQueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("***接收时间:{0}，消息内容：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        int dots = msgBody.Split('.').Length - 1;
                        System.Threading.Thread.Sleep(dots * 1000);
                        Console.WriteLine(" [x] Done");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(TopQueueName, false, consumer: consumer);

                    Console.WriteLine("按任意值，退出程序");
                    Console.ReadKey();
                }
            }
        }

    }
}