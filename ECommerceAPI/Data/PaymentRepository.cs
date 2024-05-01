namespace ECommerceAPI.Data
{
    public class PaymentRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public PaymentRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
    }
}
