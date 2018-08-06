namespace RabbitMQUtility.Model
{
    public class UpdatePurchaseOrderStatusByBillIdMqContract
    {
        public int UpdatePurchaseOrderStatusType;
        public int RelationBillType;
        public int RelationBillId;
        public int UpdateStatus;
        public int ModifiedBy;
    }
}
