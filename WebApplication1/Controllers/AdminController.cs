using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.ViewModels;
using System.Linq;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult AddBusinessUnit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBusinessUnit(BusinessUnit businessUnit)
        {
            if (!ModelState.IsValid)
            {
                return View(businessUnit);
            }

            try
            {
                _context.BusinessUnits.Add(businessUnit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Business Unit berhasil ditambahkan.";
                return RedirectToAction("AddBusinessUnit");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat menyimpan data.");
                return View(businessUnit);
            }
        }

        public async Task<IActionResult> AddProductionLine()
        {
            var viewModel = new ProductionLineFormViewModel
            {
                BusinessUnits = await _context.BusinessUnits
                    .Select(bu => new SelectListItem
                    {
                        Value = bu.BusinessUnitId.ToString(),
                        Text = bu.Name
                    }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductionLine(ProductionLineFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                form.BusinessUnits = await _context.BusinessUnits
                    .Select(bu => new SelectListItem
                    {
                        Value = bu.BusinessUnitId.ToString(),
                        Text = bu.Name
                    }).ToListAsync();
                return View(form);
            }

            try
            {
                var productionLine = new ProductionLine
                {
                    Name = form.Name,
                    BusinessUnitId = form.BusinessUnitId
                };

                _context.ProductionLines.Add(productionLine);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lini produksi berhasil ditambahkan.";
                return RedirectToAction("AddProductionLine");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat menyimpan data.");
                form.BusinessUnits = await _context.BusinessUnits
                    .Select(bu => new SelectListItem
                    {
                        Value = bu.BusinessUnitId.ToString(),
                        Text = bu.Name
                    }).ToListAsync();
                return View(form);
            }
        }

        public async Task<IActionResult> AddMachine()
        {
            var viewModel = new MachineFormViewModel
            {
                ProductionLines = await _context.ProductionLines
                    .Include(pl => pl.BusinessUnit)
                    .Select(pl => new SelectListItem
                    {
                        Value = pl.ProductionLineId.ToString(),
                        Text = $"{pl.Name} - {pl.BusinessUnit.Name}"
                    }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMachine(MachineFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                form.ProductionLines = await _context.ProductionLines
                    .Include(pl => pl.BusinessUnit)
                    .Select(pl => new SelectListItem
                    {
                        Value = pl.ProductionLineId.ToString(),
                        Text = $"{pl.Name} - {pl.BusinessUnit.Name}"
                    }).ToListAsync();
                return View(form);
            }

            try
            {
                var machine = new Machine
                {
                    Name = form.Name,
                    ProductionLineId = form.ProductionLineId
                };

                _context.Machines.Add(machine);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mesin berhasil ditambahkan.";
                return RedirectToAction("AddMachine");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat menyimpan data.");
                form.ProductionLines = await _context.ProductionLines
                    .Include(pl => pl.BusinessUnit)
                    .Select(pl => new SelectListItem
                    {
                        Value = pl.ProductionLineId.ToString(),
                        Text = $"{pl.Name} - {pl.BusinessUnit.Name}"
                    }).ToListAsync();
                return View(form);
            }
        }

        public async Task<IActionResult> VerifyUsers()
        {
            var unverifiedUsers = await _context.Users
                .Where(u => !u.IsVerified && u.Role != "admin")
                .ToListAsync();

            return View(unverifiedUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyUserAccount(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Pengguna tidak ditemukan.";
                return RedirectToAction("VerifyUsers");
            }

            user.IsVerified = true;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Pengguna {user.Name} berhasil diverifikasi.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Terjadi kesalahan saat memverifikasi pengguna.";
            }

            return RedirectToAction("VerifyUsers");
        }
    }
}
