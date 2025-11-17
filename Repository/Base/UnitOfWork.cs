using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repository.Data;
using Repository.Repositories;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Base
{
    public class UnitOfWork
    {
        private readonly CRA_DbContext _context;
        private IDbContextTransaction? _transaction = null;

        //only allow external code to consume, not change
        public IUserRepository _userRepo { get; }
        public IBookingRepository _bookingRepo { get; }
        public IInvoiceRepository _invoiceRepo { get; }
        public IParkingLotRepository _lotRepo { get; }
        public ICarRepository _carRepo { get; }
        public ICarRegRepository _carRegRepo { get; }
        public IFeedbackRepository _feedbackRepo { get; }
        public IFeedbackImageRepository _feedbackImageRepo { get; }

        //no need to construct _transaction

        public UnitOfWork(CRA_DbContext context, 
            IUserRepository userRepo, 
            IParkingLotRepository lotRepo, 
            ICarRepository carRepo, 
            ICarRegRepository carRegRepo, 
            IBookingRepository bookingRepo,
            IInvoiceRepository invoiceRepo,
            IFeedbackRepository feedbackRepo,
            IFeedbackImageRepository feedbackImgRepo)
        {
            _context = context;
            _userRepo = userRepo;
            _bookingRepo = bookingRepo;
            _invoiceRepo = invoiceRepo;
            _feedbackRepo = feedbackRepo;
            _feedbackImageRepo = feedbackImgRepo;
            _lotRepo = lotRepo;
            _carRepo = carRepo;
            _carRegRepo = carRegRepo;
        }

        //no need to construct _transaction

        #region How to construct Unit of Work:
        /*
        public UnitOfWork(YuuZoneDbContext context,
           IUserRepository userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }
         */

        //Can also construct UoW like this
        //Dont have to declare each repo in DepInj, only need to declare UoW and any other base class
        //Guaranteed shared DbContext

        /*
        public UnitOfWork(YuuZoneDbContext context)
        {
            _authRepo    = new AuthenRepository(_context);
            _userRepo = new UserRepository(_context);
            _postRepo  = new PostRepository(_context);
        }
         */
        #endregion

        public void BeginTransaction()
        {
           _transaction = _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if(_transaction!=null)
            {
                _transaction.Commit();
                _transaction.Dispose();
            }
        }

        public void RollbackTransaction()
        {
            if(_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
            }   
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            _context.Dispose();
            //Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        public virtual void Dispose(bool disposing)
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

        /*protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UnitOfWork()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }*/
    }
}
