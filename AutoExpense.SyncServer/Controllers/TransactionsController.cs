using AutoExpense.SyncServer.Data;
using AutoExpense.SyncServer.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoExpense.SyncServer.Controllers
{
    [Route("tables/transactions")]
    public class TransactionsController : TableController<Transaction>
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context) : base(new EntityTableRepository<Transaction>(context))
        {
            _context = context;
        }
    }
}
