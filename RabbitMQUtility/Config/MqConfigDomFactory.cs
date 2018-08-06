using System;
using System.Configuration;

namespace RabbitMQUtility.Config
{
    /// <summary>
    /// 创建工厂。
    /// </summary>
    public class MqConfigDomFactory
    {
        /// <summary>
        /// 创建MqConfigDom一个实例。
        /// </summary>
        /// <returns>MqConfigDom</returns>
        public static MqConfigDom CreateConfigDomInstance()
        {
            return GetConfigFormAppStting();
        }

        /// <summary>
        /// 获取物理配置文件中的配置项目。
        /// </summary>
        /// <returns></returns>
        private static MqConfigDom GetConfigFormAppStting()
        {
            var result = new MqConfigDom();
            var mqHost = ConfigurationManager.AppSettings["MqHost"];
            if (string.IsNullOrWhiteSpace(mqHost))
            {
                throw new Exception("RabbitMQ地址配置错误");
            }
            result.MqHost = mqHost;

            var mqUserName = ConfigurationManager.AppSettings["MqUserName"];
            if (string.IsNullOrWhiteSpace(mqUserName))
            {
                throw new Exception("RabbitMQ用户名不能为NULL");
            }
            result.MqUserName = mqUserName;

            var mqPassword = ConfigurationManager.AppSettings["MqPassword"];
            if (string.IsNullOrWhiteSpace(mqPassword))
            {
                throw new Exception("RabbitMQ密码不能为NULL");
            }
            result.MqPassword = mqPassword;

            var mqExchangeName = ConfigurationManager.AppSettings["MqExchangeName"];
            if (string.IsNullOrWhiteSpace(mqExchangeName))
            {
                throw new Exception("RabbitMQ默认侦听的MQ路由名称不能为NULL");
            }
            result.MqExchangeName = mqExchangeName;

            var mqListenQueueName = ConfigurationManager.AppSettings["MqListenQueueName"];
            if (string.IsNullOrWhiteSpace(mqListenQueueName))
            {
                throw new Exception("RabbitMQClient 默认侦听的MQ队列名称不能为NULL");
            }
            result.MqListenQueueName = mqListenQueueName;

            var mqVirtualHost = ConfigurationManager.AppSettings["MqVirtualHost"];
            if (string.IsNullOrWhiteSpace(mqVirtualHost))
            {
                throw new Exception("MqDB 默认侦听的MQVirtualHost名称不能为NULL");
            }
            result.MqVirtualHost = mqVirtualHost;
            return result;
        }
    }
}