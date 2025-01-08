using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class CheckUpService
    {
        private readonly IDbContextFactory<HmsAContext> _contextFactory;
        private readonly IMapper _mapper;

        public CheckUpService(IDbContextFactory<HmsAContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<MedicalRecordDTO> PerformCheckUpAsync(Guid patientId, Guid doctorId, string diagnosis, Guid diseaseId, List<PrescriptionDetailDTO> prescriptions, Guid appointmentId)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Create a new medical record
                var medicalRecord = new MedicalRecord
                {
                    MedicalRecordId = Guid.NewGuid(),
                    PatientId = patientId,
                    DoctorId = doctorId,
                    DiseaseId = diseaseId,
                    Diagnosis = diagnosis,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.MedicalRecords.Add(medicalRecord);
                await context.SaveChangesAsync();

                // Add prescription details
                foreach (var prescription in prescriptions)
                {
                    var prescriptionDetail = _mapper.Map<PrescriptionDetail>(prescription);
                    prescriptionDetail.PrescriptionDetailId = Guid.NewGuid();
                    prescriptionDetail.MedicalRecordId = medicalRecord.MedicalRecordId;

                    context.PrescriptionDetails.Add(prescriptionDetail);
                }

                await context.SaveChangesAsync();

                // Mark the appointment as complete
                var appointmentService = new AppointmentService(_contextFactory, null, _mapper);
                await appointmentService.MarkAppointmentAsCompleteAsync(appointmentId);

                // Commit the transaction
                await transaction.CommitAsync();

                // Map the medical record to DTO
                var medicalRecordDto = _mapper.Map<MedicalRecordDTO>(medicalRecord);
                return medicalRecordDto;
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                // Log the exception
                Log.Error(ex, "Error occurred while performing check-up");
                throw;
            }
        }

        public async Task<OrderDTO> CreateOrderAsync(Guid patientId, List<OrderDetailDTO> orderDetails, decimal doctorFee, Guid prescriptionId, Guid appointmentId)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Create a new order
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    PatientId = patientId,
                    OrderDate = DateTime.Now,
                    TotalAmount = 0, // Will be calculated later
                    DoctorFee = doctorFee,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PrescriptionId = prescriptionId
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                // Add order details and update product stock
                decimal totalAmount = 0;
                foreach (var orderDetailDto in orderDetails)
                {
                    var orderDetail = _mapper.Map<OrderDetail>(orderDetailDto);
                    orderDetail.OrderDetailId = Guid.NewGuid();
                    orderDetail.OrderId = order.OrderId;

                    totalAmount += orderDetail.Quantity * orderDetail.Price;
                    context.OrderDetails.Add(orderDetail);

                    // Update product stock
                    var product = await context.PharmacyProducts.FirstOrDefaultAsync(p => p.ProductId == orderDetail.ProductId);
                    if (product != null)
                    {
                        product.Stock -= orderDetail.Quantity;
                        context.PharmacyProducts.Update(product);
                    }
                }

                // Update the total amount of the order
                order.TotalAmount = totalAmount + doctorFee;
                context.Orders.Update(order);
                await context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                // Map the order to DTO
                var orderDto = _mapper.Map<OrderDTO>(order);
                return orderDto;
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                // Log the exception
                Log.Error(ex, "Error occurred while creating order");
                throw;
            }
        }

        public async Task<PaymentDTO> CreatePaymentAsync(Guid userId, Guid orderId, string paymentMethod, Guid appointmentId)
        {
            using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Retrieve the order
                var order = await context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    throw new Exception("Order not found");
                }

                // Create a new payment
                var payment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    UserId = userId,
                    OrderId = orderId,
                    AppointmentId = appointmentId, // Add this property
                    PaymentDate = DateTime.Now,
                    Amount = order.TotalAmount,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.Now
                };

                context.Payments.Add(payment);
                await context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                // Map the payment to DTO
                var paymentDto = _mapper.Map<PaymentDTO>(payment);
                return paymentDto;
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                // Log the exception
                Log.Error(ex, "Error occurred while creating payment");
                throw;
            }
        }
    }
}
