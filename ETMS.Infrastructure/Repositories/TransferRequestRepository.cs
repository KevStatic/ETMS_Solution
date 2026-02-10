using Dapper;
using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETMS.Infrastructure.Repositories
{
    public class TransferRequestRepository : ITransferRequestRepository
    {
        private readonly DapperContext _context;

        public TransferRequestRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransferRequestDto>> GetByEmployeeIdAsync(int employeeId)
        {
            var sql = @"SELECT tr.TransferRequestId, tr.EmployeeId, tr.RequestDate, 
                               tr.Status, tr.TransferType,
                               CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName
                        FROM TransferRequests tr
                        JOIN Employees e ON tr.EmployeeId = e.EmployeeId
                        WHERE tr.EmployeeId = @EmployeeId";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TransferRequestDto>(sql, new { EmployeeId = employeeId });
        }
    }
}
