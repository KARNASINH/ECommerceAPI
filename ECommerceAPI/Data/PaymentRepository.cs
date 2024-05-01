using ECommerceAPI.DTO;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Data
{
    public class PaymentRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public PaymentRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PaymentResponseDTO> MakePaymentAsync(PaymentDTO paymentDto)
        {
            //T-SQL query to fetch the Total Order Amount from the Order table.
            var orderValidationQuery = "SELECT TotalAmount FROM Orders WHERE OrderId = @OrderId AND Status = 'Pending'";
     
            var insertPaymentQuery = "INSERT INTO Payments (OrderId, Amount, Status, PaymentType, PaymentDate) OUTPUT INSERTED.PaymentId VALUES (@OrderId, @Amount, 'Pending', @PaymentType, @PaymentDate)";
            
            var updatePaymentStatusQuery = "UPDATE Payments SET Status = @Status WHERE PaymentId = @PaymentId";

            //Creates an instance of PaymentResponseDTO to hold the information how is the Payment Process going on irrespective to failed or succeed payment.
            PaymentResponseDTO paymentResponseDTO = new PaymentResponseDTO();
            
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
            
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {                        
                        decimal orderAmount = 0m;
                
                        using (var validationCommand = new SqlCommand(orderValidationQuery, connection, transaction))
                        {
                            validationCommand.Parameters.AddWithValue("@OrderId", paymentDto.OrderId);
                     
                            //Fetches the Order Amount from the Database
                            var result = await validationCommand.ExecuteScalarAsync();

                            //Return reponse if there is no Order Amount found
                            if (result == null)
                            {
                                paymentResponseDTO.Message = "Order either does not exist or is not in a pending state.";
                                return paymentResponseDTO;
                            }
                            
                            orderAmount = (decimal)result;
                        }

                        //Return the respons if Payment Amount and the Order Amount do not match
                        if (orderAmount != paymentDto.Amount)
                        {
                            paymentResponseDTO.Message = "Payment amount does not match the order total.";
                            return paymentResponseDTO;
                        }
                       
                        //Insert the 1st Payment record with 'Pending' status
                        int paymentId;
                     
                        using (var insertCommand = new SqlCommand(insertPaymentQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@OrderId", paymentDto.OrderId);
                            insertCommand.Parameters.AddWithValue("@Amount", paymentDto.Amount);
                            insertCommand.Parameters.AddWithValue("@PaymentType", paymentDto.PaymentType);
                            insertCommand.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                            
                            paymentId = (int)await insertCommand.ExecuteScalarAsync();
                        }
                        
                        //Simulate interaction with a 3rd party payment gateway
                        string paymentStatus = SimulatePaymentGatewayInteraction(paymentDto);
                        
                        //Update the payment status after receiving the gateway response
                        using (var updateCommand = new SqlCommand(updatePaymentStatusQuery, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@Status", paymentStatus);
                            updateCommand.Parameters.AddWithValue("@PaymentId", paymentId);
                            
                            //Updates the Payment Status in the Database
                            await updateCommand.ExecuteNonQueryAsync();
                            
                            //Updates the PaymentResponseDTO object if the payment is successfull
                            paymentResponseDTO.IsCreated = true;
                            paymentResponseDTO.Status = paymentStatus;
                            paymentResponseDTO.PaymentId = paymentId;
                            paymentResponseDTO.Message = $"Payment Processed with Status {paymentStatus}";
                        }
                        
                        //Committing the Transaction to make the permanent changes into the Database
                        transaction.Commit();
                        
                        return paymentResponseDTO;
                    }
                    catch (Exception)
                    {
                        //If the Transaction is failed then rolling back all the changes made into the Database
                        transaction.Rollback();
                        
                        //Again throwing the Exception to handle it further
                        throw;
                    }
                }
            }
        }

        //This method checks the Payment type to generate the response
        private string SimulatePaymentGatewayInteraction(PaymentDTO paymentDto)
        {
           //Generate the response based on the Payment Type
            switch (paymentDto.PaymentType)
            {
                case "COD":
                    return "Completed"; //If the Payment Type is COD then accept it immediately
                case "CC":
                    return "Completed"; //If the Payment Type is Credit Card then accept it immediately
                case "DC":
                    return "Failed"; //If the Payment Type is Debit Card then reject it.
                default:
                    return "Failed"; //If the Payment Type is other than COD, CC or DC then reject it.
            }
        }
    }
}
