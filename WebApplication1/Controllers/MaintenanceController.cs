using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace WebApplication1.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;
        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Leader Maintenance")]
        [HttpGet]
        public async Task<IActionResult> PendingApprovals()
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized("Anda tidak terhubung dengan Unit Bisnis manapun.");
            }

            var pendingSchedules = await _context.RepairSchedules
                .Include(rs => rs.DamageReport)
                    .ThenInclude(dr => dr.Machine)
                        .ThenInclude(m => m.ProductionLine)
                .Include(rs => rs.DamageReport)
                    .ThenInclude(dr => dr.Reporter)
                .Where(rs => rs.ApprovalStatus == false &&
                             rs.DamageReport.Machine.ProductionLine.BusinessUnitId == currentUser.BusinessUnitId)
                .OrderByDescending(rs => rs.DamageReport.ReportedAt)
                .ToListAsync();

            var viewModelList = pendingSchedules.Select(rs => new PendingApprovalViewModel
            {
                ScheduleId = rs.ScheduleId,
                ScheduleDate = rs.ScheduleDate,
                MachineName = rs.DamageReport.Machine?.Name ?? "N/A",
                ProductionLineName = rs.DamageReport.Machine?.ProductionLine?.Name ?? "N/A",
                ReporterName = rs.DamageReport.Reporter?.Name ?? "N/A",
                DamageDescription = rs.DamageReport.Description,
                ScheduleDescription = rs.Description,
                ReportDate = rs.DamageReport.ReportedAt
            }).ToList();

            return View(viewModelList);
        }

        [Authorize(Roles = "Leader Maintenance")]
        [HttpGet]
        public async Task<IActionResult> GetKytFormData(int scheduleId)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized(new { message = "Anda tidak terhubung dengan Unit Bisnis." });
            }

            var schedule = await _context.RepairSchedules
                .Include(rs => rs.DamageReport)
                    .ThenInclude(dr => dr.Machine)
                        .ThenInclude(m => m.ProductionLine)
                .Include(rs => rs.DamageReport)
                    .ThenInclude(dr => dr.Reporter)
                .FirstOrDefaultAsync(rs => rs.ScheduleId == scheduleId && rs.ApprovalStatus == false);

            if (schedule == null)
            {
                return NotFound(new { message = "Jadwal tidak ditemukan atau sudah diproses." });
            }

            if (schedule.DamageReport.Machine.ProductionLine.BusinessUnitId != currentUser.BusinessUnitId)
            {
                return Unauthorized(new { message = "Anda tidak berwenang mengakses jadwal ini." });
            }

            var technicians = await _context.Users
                .Where(u => u.Role == "Member Maintenance" && u.BusinessUnitId == currentUser.BusinessUnitId && u.IsVerified)
                .OrderBy(u => u.Name)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Name
                })
                .ToListAsync();

            var kytViewModel = new KYTFormViewModel
            {
                ScheduleId = schedule.ScheduleId,
                MachineName = schedule.DamageReport.Machine?.Name,
                ProductionLineName = schedule.DamageReport.Machine?.ProductionLine?.Name,
                DamageDescription = schedule.DamageReport.Description,
                ReporterName = schedule.DamageReport.Reporter?.Name,
                FilledByName = currentUser.Name,
                AvailableTechnicians = technicians,
                DangerousMode = 0
            };

            return Json(kytViewModel);
        }

        [Authorize(Roles = "Leader Maintenance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAndSubmitKyt(KYTFormViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized("Anda tidak terhubung dengan Unit Bisnis.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var schedule = await _context.RepairSchedules
                    .Include(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                            .ThenInclude(m => m.ProductionLine)
                    .FirstOrDefaultAsync(rs => rs.ScheduleId == model.ScheduleId);

                if (schedule == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Jadwal tidak ditemukan.");
                }
                if (schedule.ApprovalStatus == true)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Jadwal ini sudah disetujui sebelumnya.");
                }
                if (schedule.DamageReport.Machine.ProductionLine.BusinessUnitId != currentUser.BusinessUnitId)
                {
                    await transaction.RollbackAsync();
                    return Unauthorized("Anda tidak berwenang menyetujui jadwal ini.");
                }

                var kytReport = new KYTReport
                {
                    RepairSchedule = schedule,
                    CreatedById = userId,
                    CreatedAt = DateTime.Now,
                    ApprovalStatus = false,
                    ReviewedById = null,
                    Analysis = model.Analysis!,
                    Action = model.Action!,
                    Description = model.DamageDescription ?? schedule.DamageReport.Description,
                    DangerousMode = model.DangerousMode,
                    PrepareProcess = model.PrepareProcess!,
                    PreparePrediction = model.PreparePrediction!,
                    PrepareControl = model.PrepareControl!,
                    MainProcess = model.MainProcess!,
                    MainPrediction = model.MainPrediction!,
                    MainControl = model.MainControl!,
                    ConfirmProcess = model.ConfirmProcess!,
                    ConfirmPrediction = model.ConfirmPrediction!,
                    ConfirmControl = model.ConfirmControl!,
                    Technicians = new List<User>()
                };

                foreach (var techId in model.TechnicianIds)
                {
                    var technician = await _context.Users.FindAsync(techId);
                    if (technician != null && technician.Role == "Member Maintenance" && technician.IsVerified)
                    {
                        kytReport.Technicians.Add(technician);
                    }
                }

                _context.KYTReports.Add(kytReport);

                schedule.ApprovalStatus = true;

                schedule.DamageReport.Machine.Status = "pemeriksaan";

                var managers = await _context.Users
                    .Where(u => u.Role == "Manager Maintenance" && u.BusinessUnitId == currentUser.BusinessUnitId && u.IsVerified)
                    .ToListAsync();

                foreach (var manager in managers)
                {
                    var notification = new Notification
                    {
                        UserId = manager.UserId,
                        Title = "Persetujuan KYT Diperlukan",
                        Message = $"KYT baru dari {currentUser.Name} untuk mesin '{schedule.DamageReport.Machine.Name}' memerlukan persetujuan Anda.",
                        Type = "KytApprovalRequest",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                        ActionUrl = Url.Action("PendingKytReviews", "Maintenance")
                    };
                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Jadwal berhasil disetujui dan KYT tersimpan." });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan database saat menyimpan data. ({dbEx.InnerException?.Message ?? dbEx.Message})" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Terjadi kesalahan internal saat memproses permintaan Anda." });
            }
        }

        [Authorize(Roles = "Leader Maintenance")]
        [HttpGet]
        public async Task<IActionResult> GetRejectReasonForm(int scheduleId)
        {
            var schedule = await _context.RepairSchedules
                .Include(rs => rs.Creator)
                .FirstOrDefaultAsync(rs => rs.ScheduleId == scheduleId);

            if (schedule == null)
            {
                return NotFound();
            }

            return PartialView("_RejectScheduleModal", new RejectScheduleInputModel
            {
                ScheduleId = scheduleId,
                CreatorName = schedule.Creator.Name
            });
        }

        [Authorize(Roles = "Leader Maintenance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSchedule(RejectScheduleInputModel model)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized("Anda tidak memiliki unit bisnis.");
            }

            var schedule = await _context.RepairSchedules
                .Include(rs => rs.DamageReport)
                    .ThenInclude(dr => dr.Machine)
                        .ThenInclude(m => m.ProductionLine)
                .Include(rs => rs.Creator)
                .FirstOrDefaultAsync(rs => rs.ScheduleId == model.ScheduleId);

            if (schedule == null)
            {
                return NotFound("Jadwal tidak ditemukan.");
            }

            if (schedule.ApprovalStatus)
            {
                return BadRequest("Jadwal ini sudah disetujui dan tidak bisa ditolak.");
            }

            if (schedule.DamageReport.Machine.ProductionLine.BusinessUnitId != currentUser.BusinessUnitId)
            {
                return Unauthorized("Anda tidak memiliki hak untuk menolak jadwal ini.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                schedule.DamageReport.Machine.Status = "rusak";
                schedule.DamageReport.Status = false;

                var notification = new Notification
                {
                    UserId = schedule.CreatedById,
                    Title = "Jadwal Perbaikan Ditolak",
                    Message = $"Jadwal perbaikan untuk mesin '{schedule.DamageReport.Machine.Name}' ({schedule.DamageReport.Machine.ProductionLine.Name}) tgl {schedule.ScheduleDate:dd/MM/yy} DITOLAK. Alasan: {model.RejectionReason}",
                    Type = "ScheduleRejection",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                _context.RepairSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Jadwal berhasil ditolak dan notifikasi telah dikirim.";
                return RedirectToAction(nameof(PendingApprovals));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Terjadi kesalahan saat menolak jadwal.";
                return RedirectToAction(nameof(PendingApprovals));
            }
        }

        [Authorize(Roles = "Manager Maintenance")]
        [HttpGet]
        public async Task<IActionResult> PendingKytReviews()
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized("Anda tidak terhubung dengan Unit Bisnis manapun.");
            }

            var pendingKytReports = await _context.KYTReports
                .Include(kyt => kyt.Creator)
                .Include(kyt => kyt.RepairSchedule)
                    .ThenInclude(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                            .ThenInclude(m => m.ProductionLine)
                .Include(kyt => kyt.RepairSchedule)
                    .ThenInclude(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Reporter)
                .Where(kyt => kyt.ApprovalStatus == false &&
                              kyt.Creator.BusinessUnitId == currentUser.BusinessUnitId)
                .OrderByDescending(kyt => kyt.CreatedAt)
                .ToListAsync();

            var viewModelList = pendingKytReports.Select(kyt => new PendingKYTApprovalViewModel
            {
                KytId = kyt.KytId,
                CreatorName = kyt.Creator?.Name ?? "N/A",
                MachineName = kyt.RepairSchedule?.DamageReport?.Machine?.Name ?? "N/A",
                CreatedAt = kyt.CreatedAt,
                ReporterName = kyt.RepairSchedule?.DamageReport?.Reporter?.Name ?? "N/A"
            }).ToList();

            return View(viewModelList);
        }

        [Authorize(Roles = "Manager Maintenance")]
        [HttpGet]
        public async Task<IActionResult> GetKytReviewData(int kytId)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized(new { message = "Anda tidak terhubung dengan Unit Bisnis." });
            }

            var kytReport = await _context.KYTReports
                .Include(kyt => kyt.Creator)
                .Include(kyt => kyt.RepairSchedule)
                    .ThenInclude(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                            .ThenInclude(m => m.ProductionLine)
                .Include(kyt => kyt.RepairSchedule)
                    .ThenInclude(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Reporter)
                .Include(kyt => kyt.Technicians)
                .FirstOrDefaultAsync(kyt => kyt.KytId == kytId && kyt.ApprovalStatus == false);

            if (kytReport == null)
            {
                return NotFound(new { message = "KYT tidak ditemukan atau sudah diproses." });
            }

            if (kytReport.Creator.BusinessUnitId != currentUser.BusinessUnitId ||
                kytReport.RepairSchedule.DamageReport.Reporter.BusinessUnitId != currentUser.BusinessUnitId)
            {
                return Unauthorized(new { message = "Anda tidak berwenang mengakses KYT ini." });
            }

            var viewModel = new KYTFormViewModel
            {
                KytId = kytReport.KytId,
                ScheduleId = kytReport.ScheduleId,
                MachineName = kytReport.RepairSchedule.DamageReport.Machine?.Name,
                ProductionLineName = kytReport.RepairSchedule.DamageReport.Machine?.ProductionLine?.Name,
                DamageDescription = kytReport.Description,
                ReporterName = kytReport.RepairSchedule.DamageReport.Reporter?.Name,
                FilledByName = kytReport.Creator?.Name,
                TechnicianIds = kytReport.Technicians.Select(t => t.UserId).ToList(),
                Analysis = kytReport.Analysis,
                Action = kytReport.Action,
                DangerousMode = kytReport.DangerousMode,
                PrepareProcess = kytReport.PrepareProcess,
                PreparePrediction = kytReport.PreparePrediction,
                PrepareControl = kytReport.PrepareControl,
                MainProcess = kytReport.MainProcess,
                MainPrediction = kytReport.MainPrediction,
                MainControl = kytReport.MainControl,
                ConfirmProcess = kytReport.ConfirmProcess,
                ConfirmPrediction = kytReport.ConfirmPrediction,
                ConfirmControl = kytReport.ConfirmControl,
                TechnicianNames = kytReport.Technicians.Select(t => t.Name).ToList()
            };
            return PartialView("_KytFormModalReadOnly", viewModel);
        }

        [Authorize(Roles = "Manager Maintenance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectKyt(RejectKYTInputModel model)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized("Anda tidak terhubung dengan Unit Bisnis.");
            }

            var kytReport = await _context.KYTReports
                .Include(kyt => kyt.Creator)
                .Include(kyt => kyt.RepairSchedule)
                    .ThenInclude(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Reporter)
                .Include(kyt => kyt.RepairSchedule)
                    .ThenInclude(rs => rs.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                .FirstOrDefaultAsync(kyt => kyt.KytId == model.KytId);

            if (kytReport == null)
            {
                return NotFound("KYT tidak ditemukan.");
            }

            if (kytReport.ApprovalStatus != false)
            {
                return BadRequest("KYT ini sudah diproses.");
            }

            if (kytReport.Creator.BusinessUnitId != currentUser.BusinessUnitId)
            {
                return Unauthorized("Anda tidak berwenang menolak KYT ini.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                kytReport.RepairSchedule.ApprovalStatus = false;

                var notification = new Notification
                {
                    UserId = kytReport.CreatedById,
                    Title = "KYT Form Ditolak",
                    Message = $"KYTForm Anda untuk mesin '{kytReport.RepairSchedule.DamageReport.Machine.Name}' ditolak oleh Manager Maintenance. Alasan: {model.RejectionReason}",
                    Type = "KytRejection",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);

                _context.KYTReports.Remove(kytReport);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "KYT berhasil ditolak dan data terkait telah diperbarui.";
                return RedirectToAction(nameof(PendingKytReviews));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Terjadi kesalahan saat menolak KYT.";
                return RedirectToAction(nameof(PendingKytReviews));
            }
        }

        [Authorize(Roles = "Manager Maintenance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveKyt(int kytId)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                return Unauthorized("Anda tidak terhubung dengan Unit Bisnis.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var kytReport = await _context.KYTReports
                    .Include(kyt => kyt.RepairSchedule)
                        .ThenInclude(rs => rs.DamageReport)
                            .ThenInclude(dr => dr.Machine)
                    .Include(kyt => kyt.Creator)
                    .FirstOrDefaultAsync(kyt => kyt.KytId == kytId);

                if (kytReport == null)
                {
                    return NotFound("KYT tidak ditemukan.");
                }

                if (kytReport.ApprovalStatus != false)
                {
                    return BadRequest("KYT ini sudah diproses.");
                }

                if (kytReport.Creator.BusinessUnitId != currentUser.BusinessUnitId)
                {
                    return Unauthorized("Anda tidak berwenang menyetujui KYT ini.");
                }

                kytReport.ApprovalStatus = true;
                kytReport.ReviewedById = userId;

                var machine = kytReport.RepairSchedule.DamageReport.Machine;
                machine.Status = "perbaikan";

                var repairLog = await _context.RepairLogs
                    .FirstOrDefaultAsync(rl => rl.ReportId == kytReport.RepairSchedule.DamageReport.ReportId);

                if (repairLog == null)
                {
                    repairLog = new RepairLog
                    {
                        ReportId = kytReport.RepairSchedule.DamageReport.ReportId,
                        KytApprovalTime = DateTime.Now,
                        ApprovalStatus = false,
                        DamageReport = kytReport.RepairSchedule.DamageReport
                    };
                    _context.RepairLogs.Add(repairLog);
                }
                else
                {
                    repairLog.KytApprovalTime = DateTime.Now;
                }

                var notificationForLeader = new Notification
                {
                    UserId = kytReport.CreatedById,
                    Title = "KYT Telah Disetujui",
                    Message = $"KYT Anda untuk mesin '{machine.Name}' telah disetujui oleh {currentUser.Name}. Perbaikan dapat dimulai.",
                    Type = "KytApproved",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    ActionUrl = Url.Action("CompletedRepairs", "Maintenance")
                };
                _context.Notifications.Add(notificationForLeader);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Persetujuan KYT berhasil dicatat!";
                return RedirectToAction(nameof(PendingKytReviews));
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Terjadi kesalahan database saat memproses persetujuan.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Terjadi kesalahan sistem saat memproses permintaan.";
            }
            return RedirectToAction(nameof(PendingKytReviews));
        }

        [Authorize(Roles = "Leader Maintenance")]
        [HttpGet]
        public async Task<IActionResult> CompletedRepairs()
        {
            var userIdString = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Informasi pengguna tidak valid.");
            }

            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                ViewBag.ErrorMessage = "Anda tidak terhubung dengan Unit Bisnis manapun atau data Unit Bisnis tidak ditemukan.";
                return View(new List<PendingLeaderCompletionViewModel>());
            }

            try
            {
                var pendingCompletionLogs = await _context.RepairLogs
                    .Include(rl => rl.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                            .ThenInclude(m => m.ProductionLine)
                    .Include(rl => rl.DamageReport)
                        .ThenInclude(dr => dr.Reporter)
                    .Where(rl => rl.ApprovalStatus == false &&
                                 rl.KytApprovalTime != null &&
                                 rl.DamageReport.Machine.ProductionLine.BusinessUnitId == currentUser.BusinessUnitId)
                    .OrderByDescending(rl => rl.KytApprovalTime)
                    .ToListAsync();

                var viewModelList = pendingCompletionLogs.Select(rl => new PendingLeaderCompletionViewModel
                {
                    LogId = rl.LogId,
                    MachineName = rl.DamageReport?.Machine?.Name ?? "N/A",
                    ProductionLineName = rl.DamageReport?.Machine?.ProductionLine?.Name ?? "N/A",
                    ReporterName = rl.DamageReport?.Reporter?.Name ?? "N/A",
                    KytApprovalDate = rl.KytApprovalTime
                }).ToList();

                return View(viewModelList);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Terjadi kesalahan saat memuat data. Silakan coba lagi.";
                return View(new List<PendingLeaderCompletionViewModel>());
            }
        }


        [Authorize(Roles = "Leader Maintenance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRepairAsComplete(int logId)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                TempData["Error"] = "Gagal memproses permintaan: Informasi pengguna tidak valid.";
                return RedirectToAction(nameof(CompletedRepairs));
            }

            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser?.BusinessUnitId == null)
            {
                TempData["Error"] = "Gagal memproses permintaan: Anda tidak terhubung dengan Unit Bisnis.";
                return RedirectToAction(nameof(CompletedRepairs));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var repairLog = await _context.RepairLogs
                    .Include(rl => rl.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                            .ThenInclude(m => m.ProductionLine)
                    .FirstOrDefaultAsync(rl => rl.LogId == logId);

                if (repairLog == null)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Data perbaikan tidak ditemukan.";
                    return RedirectToAction(nameof(CompletedRepairs));
                }

                if (repairLog.DamageReport?.Machine?.ProductionLine?.BusinessUnitId != currentUser.BusinessUnitId)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Anda tidak berwenang menyelesaikan perbaikan ini.";
                    return RedirectToAction(nameof(CompletedRepairs));
                }

                if (repairLog.ApprovalStatus == true)
                {
                    await transaction.RollbackAsync();
                    TempData["Warning"] = "Perbaikan ini sudah ditandai selesai sebelumnya.";
                    return RedirectToAction(nameof(CompletedRepairs));
                }

                repairLog.ApprovalStatus = true;
                repairLog.RepairCompletionTime = DateTime.Now;

                var machine = repairLog.DamageReport?.Machine;
                if (machine != null)
                {
                    machine.Status = "operasional";
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Gagal memperbarui status mesin: Data mesin tidak ditemukan.";
                    return RedirectToAction(nameof(CompletedRepairs));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Perbaikan berhasil ditandai selesai.";
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Terjadi kesalahan database saat menyimpan data. ({dbEx.InnerException?.Message ?? dbEx.Message})";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Terjadi kesalahan sistem saat memproses permintaan Anda.";
            }

            return RedirectToAction(nameof(CompletedRepairs));
        }
    }
}