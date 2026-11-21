using Microsoft.EntityFrameworkCore;
using TP6_WebApi.Models;

namespace TP6_WebApi.Repositories
{
    public class CompteRepository : ICompteRepository
    {
        //code pour injecter la dépendance de la classe de contexte
        private readonly AppDbContext _context;

        public CompteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Compte>> GetAll()
        {
            // retourne la liste des comptes existants triés par leurs dates de création,
            // chaque objet compte doit inclure l'objet Client.
            return await _context.Comptes.Include(c => c.Client)
                                         .OrderBy(c => c.DateCréation)
                                         .ToListAsync();
        }

        public async Task<Compte?> GetById(int CompteId)
        {
            //retourne l'objet Compte ayant l'ID passé en paramètre.
            return await _context.Comptes.Include(c => c.Client)
                                         .FirstOrDefaultAsync(c => c.Id == CompteId);
        }

        public async Task<Compte> Add(Compte c)
        {
            // enregistre un nouveau compte dans la base de données
            _context.Comptes.Add(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<int> Nombre_op(int NuméroCompte)
        {
            // retourne le nombre d'opérations réalisés sur un compte donné
            return await _context.Operations.CountAsync(o => o.Compte!.NuméroCompte == NuméroCompte);
        }

        public async Task<Compte> update(Compte c)
        {
            // met à jour les données d'un compte donné
            _context.Comptes.Update(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<Compte?> Delete(int CompteId)
        {
            // supprime un compte
            var compte = await _context.Comptes.FindAsync(CompteId);
            if (compte != null)
            {
                _context.Comptes.Remove(compte);
                await _context.SaveChangesAsync();
            }
            return compte;
        }

        public async Task<IEnumerable<Compte>> Search(int ClientId)
        {
            // recherche les comptes d'un client donné.
            return await _context.Comptes.Where(c => c.ClientID == ClientId).ToListAsync();
        }

        public async Task<Operation> Add(Operation o)
        {
            // enregistre une opération pour un compte.
            _context.Operations.Add(o);
            await _context.SaveChangesAsync();
            return o;
        }

        public async Task<Compte?> Maj_solde(int numcompte, Operation o)
        {
            // met à jour le solde d'un compte suite à une opération de type retrait ou versement.
            var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.NuméroCompte == numcompte);
            if (compte != null)
            {
                if (o.TypeOperation == "Retrait")
                    compte.Solde -= o.Montant;
                else if (o.TypeOperation == "Versement")
                    compte.Solde += o.Montant;
                await _context.SaveChangesAsync();
            }
            return compte;
        }

        public async Task<IEnumerable<Operation>> Extrait_Compte(int numcompte, DateTime d)
        {
            // retourne la liste des opérations réalisés sur le compte ayant le numéro numcompte
            // à partir de la date d triés sur leur dates d'opération.
            return await _context.Operations
                                 .Where(o => o.Compte!.NuméroCompte == numcompte && o.DateOperation >= d)
                                 .OrderBy(o => o.DateOperation)
                                 .ToListAsync();
        }

        public async Task<double> Solde_Total(int ClientId)
        {
            // retourne la somme des soldes des comptes d'un client donné.
            return await _context.Comptes.Where(c => c.ClientID == ClientId).SumAsync(c => c.Solde);
        }

        public async Task MAJVirement(int numcompte1, int numcompte2, double m)
        {
            // met à jour les soldes des comptes suite à un virement du Compte1 vers le compte2.
            var compte1 = await _context.Comptes.FirstOrDefaultAsync(c => c.NuméroCompte == numcompte1);
            var compte2 = await _context.Comptes.FirstOrDefaultAsync(c => c.NuméroCompte == numcompte2);
            if (compte1 != null && compte2 != null)
            {
                compte1.Solde -= m;
                compte2.Solde += m;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<double> Total_Retraits(int ClientId)
        {
            // retourne la somme des montants retirés par un client donné de ses comptes.
            return await _context.Operations
                                 .Where(o => o.Compte!.ClientID == ClientId && o.TypeOperation == "Retrait")
                                 .SumAsync(o => o.Montant);
        }
    }
}
