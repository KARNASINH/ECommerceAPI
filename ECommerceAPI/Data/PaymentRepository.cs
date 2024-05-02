using ECommerceAPI.DTO;
using ECommerceAPI.Models;
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


        //This method Updates the Payment Status after checking several conditions
        public async Task<UpdatePaymentResponseDTO> UpdatePaymentStatusAsync(int paymentId, string newStatus)
        {
            // T-SQL query to fetch the data from Payment and Order table (by Joining) based on the Payment Id.
            var paymentDetailsQuery = "SELECT p.OrderId, p.Amount, p.Status, o.Status AS OrderStatus FROM Payments p INNER JOIN Orders o ON p.OrderId = o.OrderId WHERE p.PaymentId = @PaymentId";

            //T-SQL querty to update the data into Payment Table
            var updatePaymentStatusQuery = "UPDATE Payments SET Status = @Status WHERE PaymentId = @PaymentId";

            //Tracks and Stores the response aobut the Payment update process
            UpdatePaymentResponseDTO updatePaymentResponseDTO = new UpdatePaymentResponseDTO()
            {
                PaymentId = paymentId
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                int orderId;
                decimal paymentAmount;
                string currentPaymentStatus, orderStatus;

                //Fetches current Payment and Order details based on the Payment Id
                using (var command = new SqlCommand(paymentDetailsQuery, connection))
                {
                    command.Parameters.AddWithValue("@PaymentId", paymentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            //If no data found then it throws the Exception
                            throw new Exception("Payment record not found.");
                        }

                        //If T-SQL query found the data then it getting the data from the fetched row
                        orderId = reader.GetInt32(reader.GetOrdinal("OrderId"));
                        paymentAmount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                        currentPaymentStatus = reader.GetString(reader.GetOrdinal("Status"));
                        orderStatus = reader.GetString(reader.GetOrdinal("OrderStatus"));

                        //Sets the CurrentStatus in updatePaymentResponseDTO object
                        updatePaymentResponseDTO.CurrentStatus = currentPaymentStatus;
                    }
                }

                //Validate the new status change
                if (!IsValidStatusTransition(currentPaymentStatus, newStatus, orderStatus))
                {
                    updatePaymentResponseDTO.IsUpdated = false;

                    updatePaymentResponseDTO.Message = $"Invalid status transition from {currentPaymentStatus} to {newStatus} for order status {orderStatus}.";

                    return updatePaymentResponseDTO;
                }

                // Update the payment status
                using (var updateCommand = new SqlCommand(updatePaymentStatusQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@PaymentId", paymentId);
                    updateCommand.Parameters.AddWithValue("@Status", newStatus);

                    await updateCommand.ExecuteNonQueryAsync();

                    updatePaymentResponseDTO.IsUpdated = true;
                    updatePaymentResponseDTO.UpdatedStatus = newStatus;
                    updatePaymentResponseDTO.Message = $"Payment Status Updated from {currentPaymentStatus} to {newStatus}";

                    return updatePaymentResponseDTO;
                }
            }
        }


        //This method checks if the transition from Old Status to New Status is possible or not
        private bool IsValidStatusTransition(string currentStatus, string newStatus, string orderStatus)
        {
            //Completed payments cannot be modified unless it's a refund for a returned order
            if (currentStatus == "Completed" && newStatus != "Refund")
            {
                return false;
            }

            //Only pending payments can be cancelled
            if (currentStatus == "Pending" && newStatus == "Cancelled")
            {
                return true;
            }

            //Refunds should only be processed for returned orders
            if (currentStatus == "Completed" && newStatus == "Refund" && orderStatus != "Returned")
            {
                return false;
            }

            //Payments should only be marked as failed if they are not completed or cancelled
            if (newStatus == "Failed" && (currentStatus == "Completed" || currentStatus == "Cancelled"))
            {
                return false;
            }

            //Assuming 'Pending' payments become 'Completed' when the order is shipped or Confirmer
            if (currentStatus == "Pending" && newStatus == "Completed" && (orderStatus == "Shipped" || orderStatus == "Confirmed"))
            {
                return true;
            }

            //Can add other rules based on the Business requirements
            return true;
        }


        //This method fetches the Payment Details for the given Payment Id.
        public async Task<Payment?> GetPaymentDetailsAsync(int paymentId)
        {
            //T-SQL query to fetch the Payment details
            var query = "SELECT PaymentId, OrderId, Amount, Status, PaymentType, PaymentDate FROM Payments WHERE PaymentId = @PaymentId";
            
            Payment? payment = null;
            
            //
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
            
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PaymentId", paymentId);
                
                    //Runs the T-SQL query into the SQL Sever
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        //Checks if is there any row of data
                        if (await reader.ReadAsync())
                        {
                            //Sotres the Payment details in the Payment object
                            payment = new Payment
                            {
                                PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                PaymentType = reader.GetString(reader.GetOrdinal("PaymentType")),
                                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
                            };
                        }
                    }
                }
            }

            //Returns the Payment object
            return payment;
        }
    }
}

