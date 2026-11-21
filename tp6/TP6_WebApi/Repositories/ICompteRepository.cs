using TP6_WebApi.Models;

namespace TP6_WebApi.Repositories
{
    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetAll();
        Task<Compte?> GetById(int CompteId);
        Task<Compte> Add(Compte c);
        Task<int> Nombre_op(int NuméroCompte);
        Task<Compte> update(Compte c);
        Task<Compte?> Delete(int CompteId);
        Task<IEnumerable<Compte>> Search(int ClientId);
        Task<Operation> Add(Operation o);
        Task<Compte?> Maj_solde(int numcompte, Operation o);
        Task<IEnumerable<Operation>> Extrait_Compte(int numcompte, DateTime d);
        Task<double> Solde_Total(int ClientId);
        Task MAJVirement(int numcompte1, int numcompte2, double m);
        Task<double> Total_Retraits(int ClientId);
    }
}
