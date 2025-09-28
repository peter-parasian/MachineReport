# Sistem Manajemen Laporan Kerusakan Mesin (Machine Damage Report Management System)

Aplikasi Web ASP.NET MVC untuk Pelaporan dan Penjadwalan Perawatan Mesin di Lingkungan Manufaktur.

---

## 📖 Tentang Proyek

Proyek ini adalah aplikasi web yang dibangun menggunakan **ASP.NET MVC (.NET 9)** untuk mengelola alur kerja pelaporan kerusakan, penjadwalan perbaikan, dan analisis keselamatan (KYT - Kiken Yochi Training) untuk mesin produksi. Sistem ini dirancang untuk mendigitalisasi dan mengefisiensikan komunikasi antara departemen Produksi dan Maintenance.

Aplikasi ini memiliki sistem otentikasi berbasis peran (Role-Based Access Control) untuk memastikan setiap pengguna memiliki akses dan fungsionalitas yang sesuai dengan tanggung jawabnya.

---

## ✨ Fitur Utama

-   **Otentikasi & Manajemen Pengguna**: Sistem login, registrasi, dan verifikasi pengguna oleh admin.
-   **Akses Berbasis Peran (RBAC)**: Fungsionalitas yang berbeda untuk setiap peran (Admin, Leader Produksi, Manajer Produksi, Leader Maintenance, Manajer Maintenance).
-   **Dashboard Monitoring**: Visualisasi data real-time mengenai status mesin, tren kerusakan, dan durasi perbaikan menggunakan Chart.js.
-   **Alur Kerja Pelaporan**: Leader Produksi dapat melaporkan kerusakan mesin secara spesifik.
-   **Alur Kerja Penjadwalan**: Manajer Produksi dapat meninjau laporan, menolak laporan yang tidak valid, atau menjadwalkan perbaikan.
-   **Formulir KYT (Kiken Yochi Training)**: Leader Maintenance mengisi form analisis risiko dan keselamatan sebelum melakukan perbaikan.
-   **Alur Kerja Persetujuan**: Manajer Maintenance mereview dan menyetujui atau menolak formulir KYT yang diajukan.
-   **Manajemen Master Data**: Halaman admin untuk menambah Unit Bisnis, Lini Produksi, dan Mesin.
-   **Sistem Notifikasi**: Pengguna mendapatkan notifikasi real-time untuk setiap progres yang relevan.
-   **Ekspor ke Excel**: Manajer Maintenance dapat mengekspor data laporan perbaikan lengkap ke dalam format `.xlsx`.

---

## ⚙️ Alur Kerja & Peran Pengguna

Sistem ini memiliki beberapa peran dengan alur kerja yang terdefinisi:

1.  **Leader Produksi**:
    -   Melaporkan kerusakan pada mesin di lini produksinya.
    -   Menerima notifikasi jika laporannya ditolak atau perbaikan telah selesai.

2.  **Manajer Produksi**:
    -   Melihat semua laporan kerusakan yang belum diproses di dalam unit bisnisnya.
    -   Menjadwalkan perbaikan untuk laporan yang valid.
    -   Menolak laporan kerusakan dengan memberikan alasan.

3.  **Leader Maintenance**:
    -   Melihat jadwal perbaikan yang telah dibuat oleh Manajer Produksi.
    -   Menyetujui jadwal dengan mengisi formulir KYT (analisis bahaya) dan menugaskan teknisi.
    -   Menolak jadwal perbaikan dengan memberikan alasan.
    -   Menandai perbaikan sebagai "selesai" setelah teknisi menyelesaikan pekerjaan.

4.  **Manajer Maintenance**:
    -   Melihat formulir KYT yang diajukan oleh Leader Maintenance.
    -   Menyetujui KYT, yang secara otomatis mengubah status mesin menjadi "dalam perbaikan" dan memulai pencatatan durasi perbaikan.
    -   Menolak KYT dengan memberikan alasan.
    -   Mengakses dashboard monitoring dan fitur ekspor data ke Excel.

5.  **Admin**:
    -   Memverifikasi akun pengguna baru.
    -   Mengelola data master: menambah Unit Bisnis, Lini Produksi, dan Mesin baru ke dalam sistem.

---

## 🚀 Teknologi yang Digunakan

-   **Framework**: ASP.NET Core MVC (.NET 9)
-   **Bahasa**: C#
-   **Database**: SQL Server
-   **ORM**: Entity Framework Core 9 (Code-First)
-   **Frontend**:
    -   HTML5 & CSS3
    -   JavaScript (ES6+)
    -   Bootstrap 5
    -   jQuery
-   **Charting/Visualisasi**: Chart.js
-   **Otentikasi**: ASP.NET Core Identity (Cookie-based)
-   **Library Tambahan**:
    -   **BCrypt.Net**: Untuk hashing password.
    -   **ClosedXML**: Untuk fungsionalitas ekspor ke Excel.

---

## 🔧 Panduan Instalasi dan Konfigurasi

Untuk menjalankan proyek ini di lingkungan lokal, ikuti langkah-langkah berikut:

### Prasyarat

-   .NET 9 SDK atau versi yang lebih baru.
-   SQL Server
-   IDE seperti Visual Studio 2022 atau Visual Studio Code.

### Langkah-langkah

1.  **Clone Repositori**
    ```bash
    git clone [https://github.com/peter-parasian/MachineReport.git]
    cd nama-repo-anda
    ```

2.  **Konfigurasi Database**
    -   Buka file `appsettings.json`.
    -   Ubah `DefaultConnectionString` agar sesuai dengan konfigurasi SQL Server Anda. Pastikan nama database yang Anda inginkan sudah dicantumkan.
    ```json
    "ConnectionStrings": {
      "DefaultConnectionString": "Server=NAMA_SERVER_ANDA;Database=NamaDbAnda;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```

3.  **Terapkan Migrasi Database**
    -   Buka terminal atau command prompt di root direktori proyek.
    -   Jalankan perintah berikut untuk membuat database dan skemanya berdasarkan model EF Core.
    ```bash
    dotnet ef database update
    ```
    Perintah ini akan secara otomatis membuat database dan semua tabel yang diperlukan.

4.  **Jalankan Aplikasi**
    -   Jalankan proyek melalui IDE Anda (klik tombol Run di Visual Studio) atau gunakan perintah berikut di terminal:
    ```bash
    dotnet run
    ```
    -   Aplikasi akan berjalan di `https://localhost:xxxx` dan `http://localhost:xxxx`.

5.  **Login Awal**
    -   Sistem secara otomatis membuat akun admin saat pertama kali dijalankan.
    -   **Email**: `admin@gmail.com`
    -   **Password**: `admin`
    -   Gunakan akun ini untuk login dan mulai memverifikasi pengguna lain atau menambahkan data master.

---

## 🖼️ Tangkapan Layar Aplikasi

<img width="1919" height="950" alt="Screenshot 2025-09-27 160926" src="https://github.com/user-attachments/assets/c34e42b6-37c0-456d-9fd9-3d52f9d55809" />


<img width="1563" height="633" alt="Screenshot 2025-09-27 014735" src="https://github.com/user-attachments/assets/4ead864c-6551-4a35-9941-5fed5660bd41" />


Untuk informasi lebih lanjut dan visualisasi fitur, lihat dokumentasi lengkap di [Dokumentasi Website SPMR](https://my-project-3.gitbook.io/dokumentasi_website/).
