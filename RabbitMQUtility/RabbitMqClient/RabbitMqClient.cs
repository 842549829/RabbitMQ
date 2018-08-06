using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQUtility.Event;
using RabbitMQUtility.Serializer;

namespace RabbitMQUtility.RabbitMqClient
{
    /// <summary>
    /// 表示RabbitMQ客户端组件。
    /// </summary>
    public class RabbitMqClient : IRabbitMqClient
    {
        /// <summary>
        /// 客户端实例私有字段。
        /// </summary>
        private static IRabbitMqClient _instanceClient;

        /// <summary>
        /// 返回全局唯一的RabbitMqClient实例，此。
        /// </summary>
        public static IRabbitMqClient Instance
        {
            get
            {
                if (_instanceClient == null)
                {
                    RabbitMqClientFactory.CreateRabbitMqClientInstance();
                }
                return _instanceClient;
            }

            internal set => _instanceClient = value;
        }

        /// <summary>
        /// RabbitMqClient 数据上下文。
        /// </summary>
        public RabbitMqClientContext Context { get; set; }

        /// <summary>
        /// 事件激活委托实例。
        /// </summary>
        private ActionEvent _actionMessage;

        /// <summary>
        /// 当侦听的队列中有消息到达时触发的执行事件。
        /// </summary>
        public event ActionEvent ActionEventMessage
        {
            add
            {
                if (_actionMessage == null)
                {
                    _actionMessage += value;
                }
            }
            remove
            {
                if (_actionMessage != null)
                {
                    // ReSharper disable once DelegateSubtraction
                    _actionMessage -= value;
                }
            }
        }

        /// <summary>
        /// 触发一个事件且将事件打包成消息发送到远程队列中。
        /// </summary>
        /// <param name="eventMessage">发送的消息实例。</param>
        /// <param name="exChange">RabbitMq的Exchange名称。</param>
        /// <param name="queue">队列名称。</param>
        public void TriggerEventMessage(EventMessage eventMessage, string exChange, string queue)
        {
            Context.SendConnection = RabbitMqClientFactory.CreateConnection(); //获取连接
            using (Context.SendConnection)
            {
                Context.SendChannel = RabbitMqClientFactory.CreateModel(Context.SendConnection); //获取通道

                // 设置消息有效期为60秒
                //var argus = new Dictionary<string, object> { { "x-message-ttl", 6000 } };
                //Context.SendChannel.ExchangeDeclare(exChange, "topic", durable: false, autoDelete: false, arguments: argus);

                Context.SendChannel.ExchangeDeclare(exChange, "topic", false, false, null);
                Context.SendChannel.QueueDeclare(queue, false, autoDelete: false, exclusive: false, arguments: null);
                Context.SendChannel.QueueBind(queue, exChange, queue);

                const byte deliveryMode = 2;
                using (Context.SendChannel)
                {
                    var messageSerializer = MessageSerializerFactory.CreateMessageSerializerInstance(); //反序列化消息

                    var properties = Context.SendChannel.CreateBasicProperties();
                    properties.DeliveryMode = deliveryMode; //表示持久化消息
                    //properties.Expiration = "6000";

                    //推送消息
                    Context.SendChannel.BasicPublish(exChange, queue, properties, messageSerializer.SerializerBytes(eventMessage));
                }
            }
        }

        /// <summary>
        /// 侦听初始化。
        /// </summary>
        public void OnListening()
        {
            Context.ListenConnection = RabbitMqClientFactory.CreateConnection(); //获取连接

            Context.ListenConnection.ConnectionShutdown += (o, e) =>
            {
                // 日志
            };
            Context.ListenChannel = RabbitMqClientFactory.CreateModel(Context.ListenConnection); //获取通道
            Context.ListenChannel.ExchangeDeclare(Context.ListenQueueName, "topic", false, false, null);
            Context.ListenChannel.QueueDeclare(Context.ListenQueueName, false, autoDelete: false, exclusive: false, arguments: null);
            Context.ListenChannel.BasicQos(0, 1, false);
            Context.ListenChannel.QueueBind(Context.ListenQueueName, Context.ListenQueueName, Context.ListenQueueName);
            var consumer = new EventingBasicConsumer(Context.ListenChannel); //创建事件驱动的消费者类型
            consumer.Received += (s, e) =>
            {

                try
                {
                    var result = EventMessage.BuildEventMessageResult(e.Body); //获取消息返回对象

                    _actionMessage?.Invoke(result); //触发外部侦听事件

                    if (result.IsOperationOk == false)
                    {
                        //未能消费此消息，重新放入队列头
                        Context.ListenChannel.BasicReject(e.DeliveryTag, true);
                    }
                    else if (Context.ListenChannel.IsClosed == false)
                    {
                        Context.ListenChannel.BasicAck(e.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    // 日志
                }
            };
            Context.ListenChannel.BasicQos(0, 1, false); //一次只获取一个消息进行消费
            Context.ListenChannel.BasicConsume(Context.ListenQueueName, false, consumer);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            if (Context.SendConnection == null)
            {

                return;
            }
            if (Context.SendConnection.IsOpen)
            {
                Context.SendConnection.Close();
            }
            Context.SendConnection.Dispose();
        }
    }
}