using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class AuditLogService
    {
        private readonly HmsAContext _context;
        private readonly IMapper _mapper;

        public AuditLogService(HmsAContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task LogAuditAsync(string tableName, Guid recordId, string changeType, string? oldValue, string? newValue, string changedBy)
        {
            var auditLog = new AuditLog
            {
                AuditId = Guid.NewGuid(),
                TableName = tableName,
                RecordId = recordId,
                ChangeType = changeType,
                ChangedBy = changedBy,
                ChangeDate = DateTime.UtcNow,
                OldValue = oldValue,
                NewValue = newValue
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<AuditLogDTO?> GetAuditLogByIdAsync(Guid auditId)
        {
            var auditLog = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.AuditId == auditId);
            return auditLog != null ? _mapper.Map<AuditLogDTO>(auditLog) : null;
        }

        public async Task<IEnumerable<AuditLogDTO>> GetAllAuditLogsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var auditLogs = await _context.AuditLogs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDTO>>(auditLogs);
        }
    }
}
