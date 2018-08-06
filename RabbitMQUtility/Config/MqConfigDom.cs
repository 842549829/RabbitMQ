namespace RabbitMQUtility.Config
{
    /// <summary>
    /// 消息队列相关配置的DOM。
    /// </summary>
    public class MqConfigDom
    {
        /// <summary>
        /// 消息队列的地址。
        /// </summary>
        public string MqHost { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string MqUserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string MqPassword { get; set; }

        /// <summary>
        /// 客户端默认监听的路由名称
        /// </summary>
        public string MqExchangeName { get; set; }

        /// <summary>
        /// 客户端默认监听的队列名称
        /// </summary>
        public string MqListenQueueName { get; set; }

        /// <summary>
        /// VirtualHost名称
        /// </summary>
        public string MqVirtualHost { get; internal set; }
    }
}