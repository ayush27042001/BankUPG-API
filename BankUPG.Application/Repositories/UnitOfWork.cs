using BankUPG.Infrastructure.Data;
using BankUPG.Application.Interfaces;

namespace BankUPG.Application.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDBContext _context;
        private bool _disposed = false;

        public UnitOfWork(AppDBContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
