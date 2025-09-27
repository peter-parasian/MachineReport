using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Security.Claims;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class ProductionController : Controller
    {
        private readonly AppDbContext _context;
        public ProductionController(AppDbContext context)
        {
            _context = context;
        }
        private async Task LoadAvailableMachines(int productionLineId)
        {
            var machines = await _context.Machines
                .Where(m => m.ProductionLineId == productionLineId && m.Status == "operasional")
                .ToListAsync();
            ViewBag.MachineList = new SelectList(machines, "MachineId", "Name");
        }

        [Authorize(Roles = "Leader Production")]
        [HttpGet]
        public async Task<IActionResult> Report()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users
                .Include(u => u.ProductionLine)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.ProductionLineId == null)
            {
                return Unauthorized();
            }

            await LoadAvailableMachines(user.ProductionLineId.Value);

            var viewModel = new DamageReportViewModel();

            return View(viewModel);
        }

        [Authorize(Roles = "Leader Production")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(DamageReportViewModel model)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.ProductionLineId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableMachines(user.ProductionLineId.Value);
                return View(model);
            }

            var machine = await _context.Machines
                .Include(m => m.ProductionLine)
                .FirstOrDefaultAsync(m => m.MachineId == model.MachineId);

            if (machine == null || machine.ProductionLineId != user.ProductionLineId)
            {
                ModelState.AddModelError(nameof(model.MachineId), "Mesin tidak valid.");
                await LoadAvailableMachines(user.ProductionLineId.Value);
                return View(model);
            }

            if (machine.Status?.ToLower() != "operasional")
            {
                ModelState.AddModelError(nameof(model.MachineId), "Mesin ini sudah dilaporkan rusak.");
                await LoadAvailableMachines(user.ProductionLineId.Value);
                return View(model);
            }

            var report = new DamageReport
            {
                MachineId = model.MachineId,
                Description = model.Description,
                ReportedById = userId,
                Status = false,
                ReportedAt = DateTime.Now
            };

            try
            {
                machine.Status = "rusak";
                _context.DamageReports.Add(report);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kerusakan mesin berhasil dilaporkan.";
                return RedirectToAction("Report");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data.");
            }

            await LoadAvailableMachines(user.ProductionLineId.Value);
            return View(model);
        }

        [Authorize(Roles = "Manager Production")]
        [HttpGet]
        public async Task<IActionResult> RepairSchedules()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users.Include(u => u.BusinessUnit).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.BusinessUnitId == null)
            {
                return Unauthorized();
            }

            var reports = await _context.DamageReports
                .Include(r => r.Machine).ThenInclude(m => m.ProductionLine)
                .Include(r => r.Reporter)
                .Where(r => r.Machine.ProductionLine.BusinessUnitId == user.BusinessUnitId && r.Status == false)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            var viewModel = reports.Select(r => new DamageReportRejectionViewModel
            {
                ReportId = r.ReportId,
                MachineName = r.Machine?.Name ?? "N/A",
                ProductionLineName = r.Machine?.ProductionLine?.Name ?? "N/A",
                Description = r.Description,
                ReporterName = r.Reporter?.Name ?? "N/A",
                ReportDate = r.ReportedAt
            }).ToList();

            return View("RepairSchedules", viewModel);
        }

        [Authorize(Roles = "Manager Production")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleRepair(ScheduleRepairViewModel model)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users.Include(u => u.BusinessUnit).FirstOrDefaultAsync(u => u.UserId == userId);

            if (!ModelState.IsValid || user == null || user.BusinessUnitId == null)
            {
                return BadRequest();
            }

            var report = await _context.DamageReports
                .Include(r => r.Machine).ThenInclude(m => m.ProductionLine)
                .FirstOrDefaultAsync(r => r.ReportId == model.ReportId);

            if (report == null || report.Machine.ProductionLine.BusinessUnitId != user.BusinessUnitId)
            {
                return Unauthorized();
            }

            if (report.Status == true)
            {
                TempData["Warning"] = "Laporan ini sudah dijadwalkan sebelumnya.";
                return RedirectToAction("RepairSchedules");
            }

            try
            {
                var schedule = new RepairSchedule
                {
                    ReportId = report.ReportId,
                    CreatedById = userId,
                    ScheduleDate = model.ScheduleDate,
                    Description = model.Description,
                    ApprovalStatus = false
                };

                report.Status = true;
                report.Machine.Status = "pending";

                _context.RepairSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Jadwal perbaikan berhasil disimpan.";
                return RedirectToAction("RepairSchedules");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan jadwal.");

                var reports = await _context.DamageReports
                    .Include(r => r.Machine).ThenInclude(m => m.ProductionLine)
                    .Include(r => r.Reporter)
                    .Where(r => r.Machine.ProductionLine.BusinessUnitId == user.BusinessUnitId && r.Status == false)
                    .OrderByDescending(r => r.ReportedAt)
                    .ToListAsync();
                var viewModel = reports.Select(r => new DamageReportRejectionViewModel
                {
                    ReportId = r.ReportId,
                    MachineName = r.Machine?.Name ?? "N/A",
                    ProductionLineName = r.Machine?.ProductionLine?.Name ?? "N/A",
                    Description = r.Description,
                    ReporterName = r.Reporter?.Name ?? "N/A",
                    ReportDate = r.ReportedAt
                }).ToList();

                return View("RepairSchedules", viewModel);
            }
        }

        [Authorize(Roles = "Manager Production")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDamageReport(RejectDamageReportInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Alasan penolakan wajib diisi.";
                return RedirectToAction(nameof(RepairSchedules));
            }

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var manager = await _context.Users
                .Include(u => u.BusinessUnit)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (manager == null || manager.BusinessUnitId == null)
            {
                return Unauthorized("Informasi manajer tidak valid.");
            }

            var report = await _context.DamageReports
                .Include(r => r.Machine)
                    .ThenInclude(m => m.ProductionLine)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.ReportId == model.ReportId);

            if (report == null)
            {
                TempData["Error"] = "Laporan kerusakan tidak ditemukan.";
                return RedirectToAction(nameof(RepairSchedules));
            }

            if (report.Machine.ProductionLine.BusinessUnitId != manager.BusinessUnitId)
            {
                TempData["Error"] = "Anda tidak berwenang menolak laporan dari unit bisnis ini.";
                return RedirectToAction(nameof(RepairSchedules));
            }

            if (report.Status == true)
            {
                TempData["Error"] = "Laporan ini sudah diproses/dijadwalkan sebelumnya.";
                return RedirectToAction(nameof(RepairSchedules));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                report.Machine.Status = "operasional";

                var notification = new Notification
                {
                    UserId = report.ReportedById,
                    Title = "Laporan Kerusakan Ditolak",
                    Message = $"Laporan kerusakan Anda untuk mesin '{report.Machine.Name}' ({report.Machine.ProductionLine.Name}) tgl {report.ReportedAt:dd/MM/yy HH:mm} DITOLAK. Alasan: {model.RejectionReason}",
                    Type = "DamageReportRejection",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                _context.DamageReports.Remove(report);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Laporan kerusakan berhasil ditolak & notifikasi dikirim ke pelapor.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Terjadi kesalahan sistem saat menolak laporan.";
            }
            return RedirectToAction(nameof(RepairSchedules));
        }
    }
}