using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class PaymentAndBillingService
    {
        private readonly IDbContextFactory<HmsAContext> _contextFactory;
        private readonly IMapper _mapper;

        public PaymentAndBillingService(IDbContextFactory<HmsAContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        private static readonly HashSet<string> ValidPaymentMethods = new HashSet<string>
            {
                "CreditCard",
                "Cash",
                "Internet Banking"
            };

        private void ValidatePaymentMethod(string? paymentMethod)
        {
            if (paymentMethod == null || !ValidPaymentMethods.Contains(paymentMethod))
            {
                throw new ArgumentException($"Invalid payment method: {paymentMethod}");
            }
        }

        private void ValidateOrderOrAppointment(Guid? orderId, Guid? appointmentId)
        {
            if (orderId == null && appointmentId == null)
            {
                throw new ArgumentException("Either OrderID or AppointmentID must be provided.");
            }
        }

        // Payment Methods
        public async Task<List<PaymentDTO>> GetPaymentsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Payments
                .Where(p => p.IsDeleted == false)
                .Select(p => _mapper.Map<PaymentDTO>(p))
                .ToListAsync();
        }

        public async Task<PaymentDTO?> GetPaymentByIdAsync(Guid paymentId)
        {
            using var context = _contextFactory.CreateDbContext();
            var payment = await context.Payments.FindAsync(paymentId);
            if (payment == null || payment.IsDeleted == true)
            {
                return null;
            }

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO> CreatePaymentAsync(PaymentDTO paymentDto)
        {
            ValidatePaymentMethod(paymentDto.PaymentMethod);
            ValidateOrderOrAppointment(paymentDto.OrderId, paymentDto.AppointmentId);

            using var context = _contextFactory.CreateDbContext();
            var order = await context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Prescription)
                .ThenInclude(p => p.PrescriptionDetails)
                .ThenInclude(pd => pd.Product)
                .FirstOrDefaultAsync(o => o.OrderId == paymentDto.OrderId);

            if (order == null && paymentDto.OrderId != null)
            {
                throw new Exception("Order not found");
            }

            var totalAmount = order != null ? order.TotalAmount + order.DoctorFee : 0;

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                UserId = paymentDto.UserId,
                PaymentDate = paymentDto.PaymentDate ?? DateTime.Now,
                Amount = totalAmount,
                PaymentMethod = paymentDto.PaymentMethod,
                PaymentStatus = paymentDto.PaymentStatus ?? "Pending",
                OrderId = paymentDto.OrderId,
                AppointmentId = paymentDto.AppointmentId,
                IsDeleted = paymentDto.IsDeleted ?? false,
                CreatedAt = paymentDto.CreatedAt ?? DateTime.Now
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            paymentDto.PaymentId = payment.PaymentId;
            paymentDto.Amount = totalAmount;
            return paymentDto;
        }

        public async Task<bool> UpdatePaymentAsync(PaymentDTO paymentDto)
        {
            ValidatePaymentMethod(paymentDto.PaymentMethod);
            ValidateOrderOrAppointment(paymentDto.OrderId, paymentDto.AppointmentId);

            using var context = _contextFactory.CreateDbContext();
            var payment = await context.Payments.FindAsync(paymentDto.PaymentId);
            if (payment == null || payment.IsDeleted == true)
            {
                return false;
            }

            _mapper.Map(paymentDto, payment);
            context.Payments.Update(payment);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePaymentAsync(Guid paymentId)
        {
            using var context = _contextFactory.CreateDbContext();
            var payment = await context.Payments.FindAsync(paymentId);
            if (payment == null || payment.IsDeleted == true)
            {
                return false;
            }

            payment.IsDeleted = true;
            context.Payments.Update(payment);
            await context.SaveChangesAsync();

            return true;
        }

        // Billing History
        public async Task<List<BillingDTO>> GetBillingsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Billings
                .Select(b => _mapper.Map<BillingDTO>(b))
                .ToListAsync();
        }

        public async Task<BillingDTO?> GetBillingByIdAsync(Guid billingId)
        {
            using var context = _contextFactory.CreateDbContext();
            var billing = await context.Billings.FindAsync(billingId);
            if (billing == null)
            {
                return null;
            }

            return _mapper.Map<BillingDTO>(billing);
        }

        public async Task<BillingDTO> CreateBillingAsync(BillingDTO billingDto)
        {
            using var context = _contextFactory.CreateDbContext();
            var billing = _mapper.Map<Billing>(billingDto);
            billing.BillId = Guid.NewGuid();
            billing.CreatedAt = DateTime.Now;

            context.Billings.Add(billing);
            await context.SaveChangesAsync();

            return _mapper.Map<BillingDTO>(billing);
        }

        public async Task<bool> UpdateBillingAsync(BillingDTO billingDto)
        {
            using var context = _contextFactory.CreateDbContext();
            var billing = await context.Billings.FindAsync(billingDto.BillId);
            if (billing == null)
            {
                return false;
            }

            _mapper.Map(billingDto, billing);
            context.Billings.Update(billing);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBillingAsync(Guid billingId)
        {
            using var context = _contextFactory.CreateDbContext();
            var billing = await context.Billings.FindAsync(billingId);
            if (billing == null)
            {
                return false;
            }

            context.Billings.Remove(billing);
            await context.SaveChangesAsync();

            return true;
        }

        // Order Methods
        public async Task<OrderDTO?> GetOrderByIdAsync(Guid orderId)
        {
            using var context = _contextFactory.CreateDbContext();
            var order = await context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Prescription)
                .ThenInclude(p => p.PrescriptionDetails)
                .ThenInclude(pd => pd.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return null;
            }

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<List<OrderDTO>> GetOrdersAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Prescription)
                .ThenInclude(p => p.PrescriptionDetails)
                .ThenInclude(pd => pd.Product)
                .Select(o => _mapper.Map<OrderDTO>(o))
                .ToListAsync();
        }

        // Medical Record Methods
        public async Task<MedicalRecordDTO?> GetMedicalRecordByIdAsync(Guid medicalRecordId)
        {
            using var context = _contextFactory.CreateDbContext();
            var medicalRecord = await context.MedicalRecords
                .Include(mr => mr.PrescriptionDetails)
                .ThenInclude(pd => pd.Product)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == medicalRecordId);

            if (medicalRecord == null)
            {
                return null;
            }

            return _mapper.Map<MedicalRecordDTO>(medicalRecord);
        }

        public async Task<List<MedicalRecordDTO>> GetMedicalRecordsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.MedicalRecords
                .Include(mr => mr.PrescriptionDetails)
                .ThenInclude(pd => pd.Product)
                .Select(mr => _mapper.Map<MedicalRecordDTO>(mr))
                .ToListAsync();
        }

        // Prescription Detail Methods
        public async Task<PrescriptionDetailDTO?> GetPrescriptionDetailByIdAsync(Guid prescriptionDetailId)
        {
            using var context = _contextFactory.CreateDbContext();
            var prescriptionDetail = await context.PrescriptionDetails
                .Include(pd => pd.Product)
                .FirstOrDefaultAsync(pd => pd.PrescriptionDetailId == prescriptionDetailId);

            if (prescriptionDetail == null)
            {
                return null;
            }

            return _mapper.Map<PrescriptionDetailDTO>(prescriptionDetail);
        }

        public async Task<List<PrescriptionDetailDTO>> GetPrescriptionDetailsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PrescriptionDetails
                .Include(pd => pd.Product)
                .Select(pd => _mapper.Map<PrescriptionDetailDTO>(pd))
                .ToListAsync();
        }
        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);
            return user == null ? null : _mapper.Map<UserDTO>(user);
        }
        public async Task<IEnumerable<PharmacyProductDTO>> GetAllProductsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            var products = await context.PharmacyProducts.ToListAsync();
            return _mapper.Map<IEnumerable<PharmacyProductDTO>>(products);
        }
    }
}
