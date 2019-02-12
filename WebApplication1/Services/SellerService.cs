using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Services.Exceptions;

namespace WebApplication1.Services
{
    public class SellerService
    {
        private readonly WebApplication1Context _context;

        public SellerService(WebApplication1Context context)
        {
            _context = context;
        }

        public async Task<List<Seller>> FindAllAsync()
        {
            return await _context.Seller.ToListAsync();
        }

        //método para inserir um vendedor no DB
        public async Task InsertAsync(Seller obj)
        {
            _context.Add(obj);
            await _context.SaveChangesAsync();
        }

        public async Task<Seller> FindByIdAsync(int id)
        {
            //usa o include porque o Department é outro objeto associado ao objeto Seller, então o que carrega é só o Seller
            //daí para mostrar o Department também tem que incluílo na busca desse jeito.
            return await _context.Seller.Include(obj => obj.Department).FirstOrDefaultAsync(obj => obj.Id == id);
        }

        public async Task RemoveAsync(int id)
        {
            try
            {
                var obj = await _context.Seller.FindAsync(id);
                _context.Seller.Remove(obj);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new IntegrityException("Can't delete seller because he/she has sales.");
            }
        }

        
        public async Task UpdateAsync(Seller obj)
        {
            bool hasAny = await _context.Seller.AnyAsync(x => x.Id == obj.Id);
            if (!hasAny)
            {
                throw new NotFoundException("Id not found.");
            }
            try
            {
                _context.Update(obj);
                _context.SaveChanges();
            }
            //a camada de serviços não pode propagar uma exceção do nível de acesso a dados, por isso lançamos uma exceção aqui da camada
            //de serviço:
            catch(DbUpdateConcurrencyException e)
            {
                throw new DbConcurrencyException(e.Message);
            }
        }
    }
}
