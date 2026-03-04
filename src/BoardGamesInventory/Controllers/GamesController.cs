using BoardGamesInventory.ViewModels.Games;
using Microsoft.AspNetCore.Mvc;
using U2U.BoardGames;

namespace BoardGamesInventory.Controllers
{
  public class GamesController : Controller
  {
    private readonly BoardGamesDb db;

    public GamesController(BoardGamesDb db)
      => this.db = db;

    // GET: Games
    public ActionResult Index()
    {
      List<Game> games = db.Games.ToList();
      return View(model: games);
    }

    // GET: Games/Details/5
    public ActionResult Details(int id)
    {
      Game? game = db.Games
                     .Include(g => g.Publisher)
                     .Where(g => g.Id == id)
                     .FirstOrDefault();
      if (game is null)
      {
        return NotFound();
      }
      GameDetailViewModel vm = new()
      {
        Id = game.Id,
        Name = game.Name,
        Price = game.Price.Amount,
        Publisher = game.Publisher.Name
      };
      return View(model: vm);
    }

    // GET: Games/Create
    [HttpGet]
    public ActionResult Create()
    {
      GameCreateViewModel vm = new();
      vm.Publishers = db.Publishers.ToList();
      vm.PublisherId = vm.Publishers.First().Id;
      return View(model: vm);
    }

    // POST: Games/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(GameCreateViewModel vm)
    {
      try
      {
        Game game = new(0, vm.Name);
        Publisher pub = db.Publishers
                          .Where(p => p.Id == vm.PublisherId)
                          .Single();
        game.ChangePublisher(pub);
        game.SetPrice(Money.Eur(vm.Price));

        db.Games.Add(game);
        db.SaveChanges();
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        vm.Publishers = db.Publishers.ToList();
        return View(vm);
      }
    }

    // GET: Games/Edit/5
    [HttpGet]
    public ActionResult Edit(int id)
    {
      Game? game = db.Games
                     .Include(g => g.Publisher)
                     .Where(g => g.Id == id)
                     .FirstOrDefault();
      if (game is null)
      {
        return NotFound();
      }

      GameEditViewModel vm = new()
      {
        Id = game.Id,
        Name = game.Name,
        Price = game.Price.Amount,
        PublisherId = game.Publisher.Id,
        Publishers = db.Publishers.ToList()
      };

      return View(model: vm);
    }

    // POST: Games/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, GameEditViewModel vm)
    {
      // The id of the game cannot be edited
      if (id != vm.Id)
      {
        return BadRequest();
      }
      Game? game = db.Games
               .Include(g => g.Publisher)
               .Where(g => g.Id == id)
               .FirstOrDefault();
      if (game is null)
      {
        return NotFound();
      }
      try
      {
        game.Name = vm.Name;
        game.SetPrice(Money.Eur(vm.Price));
        if (game.Publisher.Id != vm.PublisherId)
        {
          Publisher pub = db.Publishers.Where(p => p.Id == vm.PublisherId).Single();
          game.ChangePublisher(pub);
        }
        db.SaveChanges();
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        vm.Publishers = db.Publishers.ToList();
        return View(vm);
      }
    }
  }
}
