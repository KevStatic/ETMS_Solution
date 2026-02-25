using Dapper;
using ETMS.Domain.Entities;
using ETMS.Domain.Interfaces;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly DapperContext _context;   // ← uses DapperContext like your other repos

        public OtpRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(OtpRequest otp)
        {
            const string sql = @"
                INSERT INTO OtpRequests 
                    (Email, PhoneNumber, OtpCode, Channel, IsVerified, IsUsed, ExpiresAt, CreatedAt)
                VALUES 
                    (@Email, @PhoneNumber, @OtpCode, @Channel, 0, 0, @ExpiresAt, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, otp);
        }

        public async Task<OtpRequest?> GetLatestByEmailAsync(string email)
        {
            const string sql = @"
                SELECT TOP 1 * FROM OtpRequests
                WHERE Email = @Email AND IsUsed = 0
                ORDER BY CreatedAt DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<OtpRequest>(sql, new { Email = email });
        }

        public async Task<OtpRequest?> GetLatestByPhoneAsync(string phone)
        {
            const string sql = @"
                SELECT TOP 1 * FROM OtpRequests
                WHERE PhoneNumber = @PhoneNumber AND IsUsed = 0
                ORDER BY CreatedAt DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<OtpRequest>(sql, new { PhoneNumber = phone });
        }

        public async Task MarkVerifiedAsync(int id)
        {
            const string sql = "UPDATE OtpRequests SET IsVerified = 1 WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task MarkUsedAsync(int id)
        {
            const string sql = "UPDATE OtpRequests SET IsUsed = 1 WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task InvalidatePreviousAsync(string contact)
        {
            const string sql = @"
                UPDATE OtpRequests SET IsUsed = 1
                WHERE (Email = @Contact OR PhoneNumber = @Contact) AND IsUsed = 0";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Contact = contact });
        }
    }
}