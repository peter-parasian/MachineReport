using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using ClosedXML.Excel;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Email dan password wajib diisi.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login gagal untuk email: {Email}", email);
                ModelState.AddModelError(string.Empty, "Email atau password salah.");
                return View();
            }


            if (!user.IsVerified)
            {
                _logger.LogWarning("Akun belum diverifikasi: {Email}", email);
                ModelState.AddModelError(string.Empty, "Silakan hubungi Admin untuk melakukan verifikasi akun Anda.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            _logger.LogInformation("User {Email} berhasil login.", email);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                BusinessUnits = _context.BusinessUnits.OrderBy(bu => bu.Name).ToList(),
                Roles = new List<string> {
                    "Leader Production",
                    "Leader Maintenance",
                    "Manager Production",
                    "Manager Maintenance",
                    "Member Maintenance"
                    },
                ProductionLines = new List<ProductionLine>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("Memulai proses pendaftaran untuk {Name}", model.Name);

            if (model.Role == "Leader Production" && !model.ProductionLineId.HasValue)
            {
                ModelState.AddModelError(nameof(model.ProductionLineId), "Lini produksi wajib dipilih untuk peran Leader Production.");
                _logger.LogWarning("Lini produksi wajib dipilih untuk peran Leader Production.");
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState valid, melanjutkan proses...");
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email sudah terdaftar.");
                    _logger.LogWarning("Pendaftaran gagal: Email {Email} sudah terdaftar.", model.Email);
                    await RepopulateRegisterViewModelDropdowns(model);
                    return View(model);
                }

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    BusinessUnitId = model.BusinessUnitId,
                    IsVerified = false,
                    ProductionLineId = model.Role == "Leader Production" ? model.ProductionLineId : null
                };

                _logger.LogInformation("Menyimpan data user ke database...");
                _context.Users.Add(user);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {Email} berhasil didaftarkan.", user.Email);
                    return RedirectToAction("Login");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Gagal menyimpan data ke database. Error: {Message}", ex.InnerException?.Message ?? ex.Message);
                    ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat menyimpan data. Silakan coba lagi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan tidak terduga saat registrasi.");
                    ModelState.AddModelError(string.Empty, "Terjadi kesalahan tidak terduga.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState tidak valid.");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    _logger.LogWarning("Validation Error: {Error}", error);
                }
            }
            _logger.LogInformation("Mengembalikan view Register karena validasi gagal.");
            await RepopulateRegisterViewModelDropdowns(model);
            return View(model);
        }

        private async Task RepopulateRegisterViewModelDropdowns(RegisterViewModel model)
        {
            model.BusinessUnits = await _context.BusinessUnits.OrderBy(bu => bu.Name).ToListAsync();
            model.Roles = new List<string> { "Leader Production", "Leader Maintenance", "Manager Production", "Manager Maintenance" };
            if (model.BusinessUnitId > 0 && model.Role == "Leader Production")
            {
                model.ProductionLines = await _context.ProductionLines
                    .Where(pl => pl.BusinessUnitId == model.BusinessUnitId)
                    .OrderBy(pl => pl.Name)
                    .ToListAsync();
            }
            else
            {
                model.ProductionLines = new List<ProductionLine>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductionLines(int businessUnitId)
        {
            if (businessUnitId <= 0)
            {
                return BadRequest("Business Unit ID tidak valid.");
            }

            var productionLines = await _context.ProductionLines
                .Where(pl => pl.BusinessUnitId == businessUnitId)
                .OrderBy(pl => pl.Name)
                .Select(pl => new {
                    productionLineId = pl.ProductionLineId,
                    name = pl.Name
                })
                .ToListAsync();
            return Json(productionLines);
        }

        [Authorize(Roles = "Leader Production, Manager Maintenance, Leader Maintenance, Manager Production")]
        [HttpGet]
        public async Task<IActionResult> MyNotifications()
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(notifications);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound(new { success = false, message = "Notifikasi tidak ditemukan." });
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound(new { success = false, message = "Notifikasi tidak ditemukan." });
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Notifikasi berhasil dihapus." });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = int.Parse(User.FindFirstValue("UserId")!);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifications.Any())
            {
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }


        [Authorize(Roles = "Leader Production, Manager Maintenance, Leader Maintenance, Manager Production, admin")]
        public IActionResult Dashboard()
        {
            return View();
        }

        //[Authorize(Roles = "Leader Production, Manager Maintenance, Leader Maintenance, Manager Production, admin")]
        //[HttpGet("Home/GetDataDashboard")]
        //public async Task<IActionResult> GetDataDashboard()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Mulai mengambil data dashboard...");

        //        // 1. Mengambil data status mesin
        //        var statusCounts = await _context.Machines
        //            .AsNoTracking()
        //            .GroupBy(m => m.Status)
        //            .Select(g => new { Status = g.Key ?? "tidak diketahui", Count = g.Count() })
        //            .ToDictionaryAsync(x => x.Status, x => x.Count);

        //        _logger.LogInformation("Data status mesin berhasil diambil: {Count} status", statusCounts.Count);

        //        var statusMachinesData = new
        //        {
        //            status_counts = statusCounts
        //        };

        //        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        //        // 2. Ambil semua damage reports dengan machine data dalam 7 hari terakhir
        //        var damageReportsWithMachine = await _context.DamageReports
        //            .AsNoTracking()
        //            .Where(dr => dr.ReportedAt >= sevenDaysAgo && dr.Machine != null)
        //            .Include(dr => dr.Machine)
        //            .Select(dr => new
        //            {
        //                MachineId = dr.MachineId,
        //                ReportedAt = dr.ReportedAt,
        //                ProductionLineId = dr.Machine.ProductionLineId,
        //                MachineName = dr.Machine.Name
        //            })
        //            .ToListAsync();

        //        _logger.LogInformation("Data damage reports berhasil diambil: {Count} records", damageReportsWithMachine.Count);

        //        // Lakukan grouping dan sorting di memory
        //        var brokenByMachines = damageReportsWithMachine
        //            .GroupBy(x => x.MachineId)
        //            .Select(g => g.OrderByDescending(x => x.ReportedAt).First())
        //            .OrderByDescending(x => x.ReportedAt)
        //            .Select(x => new
        //            {
        //                machine_id = x.MachineId,
        //                production_line_id = x.ProductionLineId,
        //                name = x.MachineName,
        //                reported_at = x.ReportedAt
        //            })
        //            .ToList();

        //        // 3. Query production line
        //        var brokenByProductionLineTemp = await _context.DamageReports
        //            .AsNoTracking()
        //            .Where(dr => dr.ReportedAt >= sevenDaysAgo && dr.Machine != null)
        //            .Include(dr => dr.Machine)
        //            .Select(dr => new
        //            {
        //                ProductionLineId = dr.Machine.ProductionLineId,
        //                MachineId = dr.MachineId,
        //                ReportedAt = dr.ReportedAt
        //            })
        //            .ToListAsync();

        //        var brokenByProductionLine = brokenByProductionLineTemp
        //            .GroupBy(dr => dr.ProductionLineId)
        //            .Select(g => new
        //            {
        //                production_line_id = g.Key,
        //                broken_machine_count = g.Select(dr => dr.MachineId).Distinct().Count(),
        //                machine_ids = g.Select(dr => dr.MachineId).Distinct().ToList(),
        //                reported_at = g.Max(dr => dr.ReportedAt)
        //            })
        //            .OrderBy(r => r.production_line_id)
        //            .ToList();

        //        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        //        // 4. PERBAIKAN: Query repair logs dengan include Machine untuk mendapatkan nama mesin
        //        var repairLogs = await _context.RepairLogs
        //            .AsNoTracking()
        //            .Where(rl => rl.RepairCompletionTime >= thirtyDaysAgo &&
        //                         rl.KytApprovalTime.HasValue &&
        //                         rl.RepairCompletionTime.HasValue &&
        //                         rl.DamageReport != null &&
        //                         rl.DamageReport.Machine != null) 
        //            .Include(rl => rl.DamageReport)
        //                .ThenInclude(dr => dr.Machine) 
        //            .OrderByDescending(rl => rl.RepairCompletionTime)
        //            .Select(rl => new
        //            {
        //                MachineId = rl.DamageReport.MachineId,
        //                MachineName = rl.DamageReport.Machine.Name, 
        //                KytApprovalTime = rl.KytApprovalTime.Value,
        //                RepairCompletionTime = rl.RepairCompletionTime.Value
        //            })
        //            .ToListAsync();

        //        _logger.LogInformation("Data repair logs berhasil diambil: {Count} records", repairLogs.Count);

        //        // PERBAIKAN: Tambahkan nama mesin pada timeRepairsData
        //        var timeRepairsData = repairLogs.Select(tr =>
        //        {
        //            var duration = tr.RepairCompletionTime - tr.KytApprovalTime;
        //            var durationReadable = $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";

        //            return new
        //            {
        //                machine_id = tr.MachineId,
        //                machine_name = tr.MachineName, 
        //                kyt_approval_time = tr.KytApprovalTime,
        //                repair_completion_time = tr.RepairCompletionTime,
        //                duration_seconds = (long)duration.TotalSeconds,
        //                duration_readable = durationReadable
        //            };
        //        });

        //        // Menggabungkan semua data menjadi satu objek
        //        var result = new
        //        {
        //            status_machines = statusMachinesData,
        //            broken_by_machines = brokenByMachines,
        //            broken_by_production_line = brokenByProductionLine,
        //            time_repairs = timeRepairsData
        //        };

        //        _logger.LogInformation("Data dashboard berhasil disiapkan dan akan dikirim ke client");
        //        return Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Terjadi kesalahan fatal saat mengambil data dashboard. Pesan: {ErrorMessage}", ex.Message);
        //        return StatusCode(500, new
        //        {
        //            message = "Terjadi kesalahan pada server saat mengambil data dashboard.",
        //            error = ex.Message,
        //            success = false
        //        });
        //    }
        //}

        [Authorize(Roles = "Leader Production, Manager Maintenance, Leader Maintenance, Manager Production, admin")]
        [HttpGet("Home/GetDataDashboard")]
        public async Task<IActionResult> GetDataDashboard()
        {
            try
            {
                _logger.LogInformation("Mulai mengambil data dashboard...");

                // 1. Dapatkan ID Pengguna dan Unit Bisnisnya
                var userIdString = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdString, out var userId))
                {
                    _logger.LogWarning("Gagal mendapatkan UserId dari claims.");
                    return Unauthorized(new { message = "Informasi pengguna tidak valid." });
                }

                var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                if (currentUser?.BusinessUnitId == null)
                {
                    _logger.LogWarning("Pengguna dengan ID {UserId} tidak memiliki BusinessUnitId.", userId);
                    return Unauthorized(new { message = "Anda tidak terhubung dengan Unit Bisnis manapun." });
                }
                var userBusinessUnitId = currentUser.BusinessUnitId;

                // --- START MODIFIKASI ---
                // 1. Mengambil data status mesin dari database (SUDAH DIFILTER)
                var dbStatusCounts = await _context.Machines
                    .AsNoTracking()
                    .Where(m => m.ProductionLine.BusinessUnitId == userBusinessUnitId && m.Status != null) // FILTER BERDASARKAN BUSINESS UNIT
                    .GroupBy(m => m.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                _logger.LogInformation("Data status mesin dari DB untuk BU {BusinessUnitId} berhasil diambil: {Count} status", userBusinessUnitId, dbStatusCounts.Count);

                // Inisialisasi dictionary dengan semua status yang diinginkan dan nilai default 0
                var allStatusCounts = new Dictionary<string, int>
                {
                    { "operasional", 0 },
                    { "rusak", 0 },
                    { "perbaikan", 0 },
                    { "pending", 0 }
                };

                // Perbarui dictionary dengan data dari database
                foreach (var status in dbStatusCounts)
                {
                    if (allStatusCounts.ContainsKey(status.Key))
                    {
                        allStatusCounts[status.Key] = status.Value;
                    }
                }

                var statusMachinesData = new
                {
                    status_counts = allStatusCounts
                };
                // --- END MODIFIKASI ---

                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

                // 2. Ambil semua damage reports dengan machine data dalam 7 hari terakhir (SUDAH DIFILTER)
                var damageReportsWithMachine = await _context.DamageReports
                    .AsNoTracking()
                    .Where(dr => dr.ReportedAt >= sevenDaysAgo &&
                                 dr.Machine != null &&
                                 dr.Machine.ProductionLine.BusinessUnitId == userBusinessUnitId) // FILTER BERDASARKAN BUSINESS UNIT
                    .Include(dr => dr.Machine)
                    .Select(dr => new
                    {
                        MachineId = dr.MachineId,
                        ReportedAt = dr.ReportedAt,
                        ProductionLineId = dr.Machine.ProductionLineId,
                        MachineName = dr.Machine.Name
                    })
                    .ToListAsync();

                _logger.LogInformation("Data damage reports untuk BU {BusinessUnitId} berhasil diambil: {Count} records", userBusinessUnitId, damageReportsWithMachine.Count);

                // Lakukan grouping dan sorting di memory
                var brokenByMachines = damageReportsWithMachine
                    .GroupBy(x => x.MachineId)
                    .Select(g => g.OrderByDescending(x => x.ReportedAt).First())
                    .OrderByDescending(x => x.ReportedAt)
                    .Select(x => new
                    {
                        machine_id = x.MachineId,
                        production_line_id = x.ProductionLineId,
                        name = x.MachineName,
                        reported_at = x.ReportedAt
                    })
                    .ToList();

                // 3. Query production line (SUDAH DIFILTER) - KODE DIMODIFIKASI
                var brokenByProductionLineTemp = await _context.DamageReports
                    .AsNoTracking()
                    .Where(dr => dr.ReportedAt >= sevenDaysAgo &&
                                 dr.Machine != null &&
                                 dr.Machine.ProductionLine != null &&
                                 dr.Machine.ProductionLine.BusinessUnitId == userBusinessUnitId) // FILTER BERDASARKAN BUSINESS UNIT
                    .Include(dr => dr.Machine)
                        .ThenInclude(m => m.ProductionLine) // Sertakan data ProductionLine
                    .Select(dr => new
                    {
                        ProductionLineId = dr.Machine.ProductionLineId,
                        ProductionLineName = dr.Machine.ProductionLine.Name, // Ambil nama ProductionLine
                        MachineId = dr.MachineId,
                        ReportedAt = dr.ReportedAt
                    })
                    .ToListAsync();

                var brokenByProductionLine = brokenByProductionLineTemp
                    .GroupBy(dr => new { dr.ProductionLineId, dr.ProductionLineName }) // Kelompokkan berdasarkan ID dan Nama
                    .Select(g => new
                    {
                        production_line_id = g.Key.ProductionLineId,
                        production_line_name = g.Key.ProductionLineName, // Tambahkan nama ke hasil akhir
                        broken_machine_count = g.Select(dr => dr.MachineId).Distinct().Count(),
                        machine_ids = g.Select(dr => dr.MachineId).Distinct().ToList(),
                        reported_at = g.Max(dr => dr.ReportedAt)
                    })
                    .OrderBy(r => r.production_line_id)
                    .ToList();

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                // 4. PERBAIKAN: Query repair logs dengan include Machine untuk mendapatkan nama mesin (SUDAH DIFILTER)
                var repairLogs = await _context.RepairLogs
                    .AsNoTracking()
                    .Where(rl => rl.RepairCompletionTime >= thirtyDaysAgo &&
                                 rl.KytApprovalTime.HasValue &&
                                 rl.RepairCompletionTime.HasValue &&
                                 rl.DamageReport != null &&
                                 rl.DamageReport.Machine != null &&
                                 rl.DamageReport.Machine.ProductionLine.BusinessUnitId == userBusinessUnitId) // FILTER BERDASARKAN BUSINESS UNIT
                    .Include(rl => rl.DamageReport)
                        .ThenInclude(dr => dr.Machine)
                    .OrderByDescending(rl => rl.RepairCompletionTime)
                    .Select(rl => new
                    {
                        MachineId = rl.DamageReport.MachineId,
                        MachineName = rl.DamageReport.Machine.Name,
                        KytApprovalTime = rl.KytApprovalTime.Value,
                        RepairCompletionTime = rl.RepairCompletionTime.Value
                    })
                    .ToListAsync();

                _logger.LogInformation("Data repair logs untuk BU {BusinessUnitId} berhasil diambil: {Count} records", userBusinessUnitId, repairLogs.Count);

                // PERBAIKAN: Tambahkan nama mesin pada timeRepairsData
                var timeRepairsData = repairLogs.Select(tr =>
                {
                    var duration = tr.RepairCompletionTime - tr.KytApprovalTime;
                    var durationReadable = $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";

                    return new
                    {
                        machine_id = tr.MachineId,
                        machine_name = tr.MachineName,
                        kyt_approval_time = tr.KytApprovalTime,
                        repair_completion_time = tr.RepairCompletionTime,
                        duration_seconds = (long)duration.TotalSeconds,
                        duration_readable = durationReadable
                    };
                });

                // Menggabungkan semua data menjadi satu objek
                var result = new
                {
                    status_machines = statusMachinesData,
                    broken_by_machines = brokenByMachines,
                    broken_by_production_line = brokenByProductionLine,
                    time_repairs = timeRepairsData
                };

                _logger.LogInformation("Data dashboard berhasil disiapkan dan akan dikirim ke client");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan fatal saat mengambil data dashboard. Pesan: {ErrorMessage}", ex.Message);
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan pada server saat mengambil data dashboard.",
                    error = ex.Message,
                    success = false
                });
            }
        }

        [Authorize(Roles = "Manager Maintenance")]
        [HttpGet("Home/GetDataExcel")]
        public async Task<IActionResult> GetDataExcel()
        {
            try
            {
                _logger.LogInformation("Mulai mengambil data untuk Excel...");

                // Dapatkan ID Pengguna dan Unit Bisnisnya untuk filtering data
                var userIdString = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdString, out var userId))
                {
                    _logger.LogWarning("Gagal mendapatkan UserId dari claims.");
                    return Unauthorized(new { message = "Informasi pengguna tidak valid." });
                }

                var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                if (currentUser?.BusinessUnitId == null)
                {
                    _logger.LogWarning("Pengguna dengan ID {UserId} tidak memiliki BusinessUnitId.", userId);
                    return Unauthorized(new { message = "Anda tidak terhubung dengan Unit Bisnis manapun." });
                }
                var userBusinessUnitId = currentUser.BusinessUnitId;

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                // Query utama untuk mengambil data log perbaikan yang sudah disetujui (approved)
                var excelDataQuery = _context.RepairLogs
                    .AsNoTracking()
                    .Where(rl => rl.ApprovalStatus == true &&
                                 rl.RepairCompletionTime.HasValue &&
                                 rl.RepairCompletionTime.Value >= thirtyDaysAgo &&
                                 rl.DamageReport.Machine.ProductionLine.BusinessUnitId == userBusinessUnitId)
                    .Select(rl => new
                    {
                        // Ambil data dasar dari RepairLog dan relasi terdekatnya
                        Log = rl,
                        Report = rl.DamageReport,
                        Reporter = rl.DamageReport.Reporter,
                        Machine = rl.DamageReport.Machine,
                        ProductionLine = rl.DamageReport.Machine.ProductionLine,
                        // Sub-query untuk menemukan data KYT Report yang relevan (yang sudah disetujui)
                        ApprovedKytData = rl.DamageReport.RepairSchedules
                            .SelectMany(rs => rs.KYTReports)
                            .Where(kyt => kyt.ApprovalStatus == true)
                            .OrderByDescending(kyt => kyt.CreatedAt)
                            .Select(kyt => new
                            {
                                KytReport = kyt,
                                Creator = kyt.Creator,
                                // technician_id tidak lagi diambil
                                TechnicianNames = kyt.Technicians.Select(t => t.Name).ToList()
                            })
                            .FirstOrDefault()
                    });

                var excelData = await excelDataQuery.ToListAsync();

                // Ubah hasil query menjadi format JSON yang diinginkan
                var formattedData = excelData
                    .Where(x => x.ApprovedKytData != null)
                    .Select(x =>
                    {
                        string duration_readable = "N/A";

                        // Lakukan pengecekan untuk memastikan kedua waktu tidak null sebelum menghitung
                        if (x.Log.KytApprovalTime.HasValue && x.Log.RepairCompletionTime.HasValue)
                        {
                            var duration = x.Log.RepairCompletionTime.Value - x.Log.KytApprovalTime.Value;
                            // duration_seconds tidak lagi dihitung dan dikembalikan
                            duration_readable = $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
                        }

                        // Kembalikan objek anonim dengan semua properti yang dibutuhkan
                        return new
                        {
                            Nama_Lini_Produksi = x.ProductionLine.Name,
                            Nama_Mesin = x.Machine.Name,
                            reported_by = x.Reporter.Name,
                            reported_at = x.Report.ReportedAt,
                            created_at = x.ApprovedKytData.KytReport.CreatedAt,
                            kyt_approval_time = x.Log.KytApprovalTime,
                            repair_completion_time = x.Log.RepairCompletionTime,
                            // Kolom duration_seconds dihapus
                            duration_readable,
                            // Kolom technician_id dihapus
                            nama_teknisi = x.ApprovedKytData.TechnicianNames,
                            created_by = x.ApprovedKytData.Creator.Name,
                            Prepare_process = x.ApprovedKytData.KytReport.PrepareProcess,
                            Prepare_prediction = x.ApprovedKytData.KytReport.PreparePrediction,
                            Prepare_control = x.ApprovedKytData.KytReport.PrepareControl,
                            Main_process = x.ApprovedKytData.KytReport.MainProcess,
                            Main_prediction = x.ApprovedKytData.KytReport.MainPrediction,
                            Main_control = x.ApprovedKytData.KytReport.MainControl,
                            Confirm_process = x.ApprovedKytData.KytReport.ConfirmProcess,
                            Confirm_prediction = x.ApprovedKytData.KytReport.ConfirmPrediction,
                            Confirm_control = x.ApprovedKytData.KytReport.ConfirmControl,
                            Analysis = x.ApprovedKytData.KytReport.Analysis,
                            Description = x.ApprovedKytData.KytReport.Description,
                            Action = x.ApprovedKytData.KytReport.Action
                        };
                    })
                    .ToList();


                // Bungkus hasil dalam parent key 'data_excel'
                var result = new
                {
                    data_excel = formattedData
                };

                _logger.LogInformation("Berhasil mengambil {Count} record data untuk Excel.", formattedData.Count);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan saat mengambil data Excel. Pesan: {ErrorMessage}", ex.Message);
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan pada server saat mengambil data Excel.",
                    error = ex.Message,
                    success = false
                });
            }
        }

        [Authorize(Roles = "Manager Maintenance")]
        [HttpGet("Home/ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            // Ambil data menggunakan logika yang sama dengan GetDataExcel
            var jsonResult = await GetDataExcel() as JsonResult;
            if (jsonResult?.Value == null)
            {
                return StatusCode(500, "Tidak dapat mengambil data untuk diekspor.");
            }

            // Ekstrak data list dari objek JSON
            var dataObject = jsonResult.Value;
            var dataProperty = dataObject.GetType().GetProperty("data_excel");
            if (dataProperty == null)
            {
                return StatusCode(500, "Struktur data tidak valid.");
            }

            // Konversi data ke tipe yang lebih spesifik
            var dataList = (dataProperty.GetValue(dataObject) as IEnumerable<object>).Cast<dynamic>().ToList();

            // Mendefinisikan mapping dari nama properti ke nama header Excel
            var headerMappings = new Dictionary<string, string>
            {
                { "Nama_Lini_Produksi", "Lini Produksi" },
                { "Nama_Mesin", "Tipe Mesin" },
                { "reported_by", "Pelapor" },
                { "reported_at", "Tanggal Pelaporan" },
                { "created_at", "Tanggal Pembuatan KYT" },
                { "kyt_approval_time", "Tanggal Persetujuan KYT" },
                { "repair_completion_time", "Tanggal Selesai Perbaikan" },
                { "duration_readable", "Durasi Perbaikan" },
                { "nama_teknisi", "Teknisi" },
                { "created_by", "Penanggung Jawab" },
                { "Prepare_process", "Persiapan Proses" },
                { "Prepare_prediction", "Persiapan Prediksi Bahaya" },
                { "Prepare_control", "Persiapan Tindakan Pengendalian" },
                { "Main_process", "Proses Utama" },
                { "Main_prediction", "Prediksi Bahaya Utama" },
                { "Main_control", "Tindakan Pengendalian Utama" },
                { "Confirm_process", "Konfirmasi Proses" },
                { "Confirm_prediction", "Konfirmasi Prediksi Bahaya" },
                { "Confirm_control", "Konfirmasi Tindakan Pengendalian" },
                { "Analysis", "Analisis Kecelakaan" },
                { "Description", "Deskripsi Kerusakan" },
                { "Action", "Tindakan Diperlukan" }
            };

            // Menyimpan urutan properti sesuai definisi di atas
            var orderedPropertyNames = headerMappings.Keys.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data Laporan Perbaikan");
                var currentRow = 1;

                // --- MEMBUAT HEADER KUSTOM ---
                for (int i = 0; i < orderedPropertyNames.Count; i++)
                {
                    // Gunakan nilai dari dictionary sebagai nama header
                    worksheet.Cell(currentRow, i + 1).Value = headerMappings[orderedPropertyNames[i]];
                }

                // --- MENGISI DATA PADA BARIS ---
                if (dataList.Any())
                {
                    foreach (var item in dataList)
                    {
                        currentRow++;
                        for (int i = 0; i < orderedPropertyNames.Count; i++)
                        {
                            var propName = orderedPropertyNames[i];
                            var value = item.GetType().GetProperty(propName)?.GetValue(item, null);

                            // Penanganan khusus untuk kolom array (misal: nama_teknisi)
                            if (value is IEnumerable<object> list && !(value is string))
                            {
                                worksheet.Cell(currentRow, i + 1).Value = string.Join(", ", list);
                            }
                            // Penanganan khusus untuk tanggal agar mudah dibaca
                            else if (value is DateTime dt)
                            {
                                worksheet.Cell(currentRow, i + 1).Value = dt;
                                // Mengatur format tanggal di Excel
                                worksheet.Cell(currentRow, i + 1).Style.DateFormat.Format = "dd-MMM-yyyy HH:mm:ss";
                            }
                            else
                            {
                                worksheet.Cell(currentRow, i + 1).Value = value?.ToString();
                            }
                        }
                    }
                }

                // Atur lebar kolom agar sesuai dengan konten
                worksheet.Columns().AdjustToContents();

                // Mengatur style untuk header
                var headerRange = worksheet.Range(1, 1, 1, headerMappings.Count);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Simpan workbook ke memory stream dan kirim sebagai file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Laporan_Perbaikan_{DateTime.Now:MMMM yyyy}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }


    }
}