using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP6_WebApi.Models;
using TP6_WebApi.Repositories;

namespace TP6_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompteController : ControllerBase
    {
        // Code pour injecter la dépendance au service ICompteRepository
        private readonly ICompteRepository _repository;
        // Contexte utilisé pour vérifier l'existence d'un client / trouver un compte par numéro
        private readonly AppDbContext _context;

        public CompteController(ICompteRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compte>>> GetComptes()
        {
            return Ok(await _repository.GetAll());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Compte>> GetCompte(int id)
        {
            var compte = await _repository.GetById(id);
            if (compte == null)
                return NotFound();
            return Ok(compte);
        }

        [HttpPost]
        public async Task<ActionResult<Compte>> CreateCompte(Compte c)
        {
            // Ajouter un nouveau compte
            // vérifier que c.ClientId concerne un client existant dans la base de données
            if (!await _context.Clients.AnyAsync(cl => cl.Id == c.ClientID))
                return BadRequest("Client inexistant");

            c.Client = null;      // ne pas créer de client à partir du corps de la requête
            var compte = await _repository.Add(c);
            return CreatedAtAction(nameof(GetCompte), new { id = compte.Id }, compte);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Compte>> DeleteCompte(int id)
        {
            //vérifier si le compte a des opérations, si oui interdire la suppression
            var compte = await _repository.GetById(id);
            if (compte == null)
                return NotFound();

            if (await _repository.Nombre_op(compte.NuméroCompte) > 0)
                return BadRequest("Compte avec opérations : suppression interdite");

            return Ok(await _repository.Delete(id));
        }

        [HttpGet("search/{ClientId:int}")]
        public async Task<ActionResult<IEnumerable<Compte>>> Search(int ClientId)
        {
            return Ok(await _repository.Search(ClientId));
        }

        [HttpPost("operation")]
        public async Task<ActionResult<Operation>> CreateOperation(Operation p)
        {
            //Ajoute une opération de retrait ou versement à un compte, si le type de
            //l'opération est "Retrait" et le solde du compte est insuffisant retourne un
            //badrequest avec un message d'erreur, sinon mise à jour du solde du compte
            if (p.TypeOperation != "Retrait" && p.TypeOperation != "Versement")
                return BadRequest("L'opération doit être un Retrait ou un Versement");

            var compte = await _repository.GetById(p.CompteID);
            if (compte == null)
                return NotFound("Compte inexistant");

            if (p.TypeOperation == "Retrait" && compte.Solde < p.Montant)
                return BadRequest("Solde insuffisant");

            var operation = await _repository.Add(p);
            await _repository.Maj_solde(compte.NuméroCompte, p);
            return Ok(operation);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Compte>> UpdateCompte(int id, Compte c)
        {
            //mettre à jour un compte
            if (id != c.Id)
                return BadRequest("L'id ne correspond pas au compte");

            c.Client = null;
            return Ok(await _repository.update(c));
        }

        // OBTENIR NOMBRE D'OPERATIONS
        [HttpGet("{id:int}/nombre-operations")]
        public async Task<ActionResult<int>> NombreOperations(int id)
        {
            var compte = await _repository.GetById(id);
            if (compte == null)
                return NotFound();
            return Ok(await _repository.Nombre_op(compte.NuméroCompte));
        }

        // EXTRAIT D'UN COMPTE
        [HttpGet("{num:int}/extrait")]
        public async Task<ActionResult<IEnumerable<Operation>>> Extrait(int num, DateTime date)
        {
            return Ok(await _repository.Extrait_Compte(num, date));
        }

        // SOLDE TOTAL D'UN CLIENT
        [HttpGet("solde-total/{clientId:int}")]
        public async Task<ActionResult<double>> SoldeTotal(int clientId)
        {
            return Ok(await _repository.Solde_Total(clientId));
        }

        // MAJ PAR VIREMENT
        [HttpPost("virement")]
        public async Task<ActionResult> MAJVirement(int num1, int num2, double montant)
        {
            var compte1 = await _context.Comptes.FirstOrDefaultAsync(c => c.NuméroCompte == num1);
            var compte2 = await _context.Comptes.FirstOrDefaultAsync(c => c.NuméroCompte == num2);
            if (compte1 == null || compte2 == null)
                return NotFound("Compte inexistant");
            if (montant <= 0)
                return BadRequest("Montant invalide");
            if (compte1.Solde < montant)
                return BadRequest("Solde insuffisant");

            await _repository.MAJVirement(num1, num2, montant);
            return Ok("Virement effectué");
        }

        // TOTAL RETRAITS D'UN CLIENT
        [HttpGet("total-retraits/{clientId:int}")]
        public async Task<ActionResult<double>> TotalRetraits(int clientId)
        {
            return Ok(await _repository.Total_Retraits(clientId));
        }
    }
}
