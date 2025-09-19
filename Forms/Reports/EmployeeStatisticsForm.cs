using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Guna.UI2.WinForms;

namespace SmartInventoryPro.Forms.Reports
{
    public partial class EmployeeStatisticsForm : Form
    {
        private readonly string _connectionString;
        private Guna2DataGridView dgvEmployees = null!;
        private Label lblStatus = null!;

        public EmployeeStatisticsForm(string connectionString)
        {
            _connectionString = connectionString;
            InitializeComponent();
            LoadEmployeeStatistics();
        }

        private void InitializeComponent()
        {
            this.Text = "إحصائيات الموظفين - InfinityPOS";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Header Panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.ClientSize.Width, 60),
                BackColor = Color.FromArgb(52, 152, 219),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblTitle = new Label
            {
                Text = "👥 إحصائيات الموظفين",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd، dd MMMM yyyy - HH:mm"),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(headerPanel.Width - 300, 20),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblDateTime });

            // Main Panel
            var mainPanel = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 60),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Status Label
            lblStatus = new Label
            {
                Text = "⏳ جاري تحميل إحصائيات الموظفين...",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(52, 152, 219),
                Location = new Point(20, 20),
                Size = new Size(400, 25)
            };

            // Employees DataGridView
            dgvEmployees = new Guna2DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(mainPanel.Width - 40, mainPanel.Height - 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 40,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowTemplate = { Height = 45 },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(50, 50, 50),
                    SelectionBackColor = Color.FromArgb(52, 152, 219),
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 11, FontStyle.Regular),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(15, 8, 15, 8)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(8, 5, 8, 5)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 249, 250)
                },
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(230, 230, 230),
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            mainPanel.Controls.AddRange(new Control[] { lblStatus, dgvEmployees });
            this.Controls.AddRange(new Control[] { headerPanel, mainPanel });
        }

        private async void LoadEmployeeStatistics()
        {
            try
            {
                lblStatus.Text = "⏳ جاري تحميل إحصائيات الموظفين...";
                lblStatus.ForeColor = Color.FromArgb(52, 152, 219);

                var employees = await GetEmployeeStatisticsAsync();

                dgvEmployees.DataSource = employees;
                dgvEmployees.RightToLeft = RightToLeft.Yes;

                // Configure column widths and formatting
                ConfigureDataGridViewColumns();

                // Add cell formatting for performance indicators
                dgvEmployees.CellFormatting += DgvEmployees_CellFormatting;

                lblStatus.Text = $"✅ تم تحميل إحصائيات {employees.Count} موظف بنجاح";
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ خطأ في تحميل إحصائيات الموظفين";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show($"خطأ في تحميل إحصائيات الموظفين: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridViewColumns()
        {
            if (dgvEmployees.Columns.Count > 0)
            {
                // Set column widths based on content importance
                dgvEmployees.Columns["المعرف"].Width = 70;
                dgvEmployees.Columns["الاسم"].Width = 180;
                dgvEmployees.Columns["البريد"].Width = 200;
                dgvEmployees.Columns["معتمد"].Width = 70;
                dgvEmployees.Columns["مبيعات"].Width = 80;
                dgvEmployees.Columns["مشتريات"].Width = 80;
                dgvEmployees.Columns["عدد_المبيعات"].Width = 100;
                dgvEmployees.Columns["إجمالي_المبلغ"].Width = 130;
                dgvEmployees.Columns["آخر_بيع"].Width = 100;
                dgvEmployees.Columns["أيام_منذ_آخر_بيع"].Width = 100;
                dgvEmployees.Columns["الأداء"].Width = 90;

                // Set minimum widths
                foreach (DataGridViewColumn column in dgvEmployees.Columns)
                {
                    column.MinimumWidth = 70;
                }
            }
        }

        private void DgvEmployees_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvEmployees.Columns[e.ColumnIndex].Name == "الأداء" && e.Value != null)
            {
                string performance = e.Value.ToString() ?? "";
                if (performance == "ممتاز")
                {
                    e.CellStyle.ForeColor = Color.Green;
                    e.CellStyle.Font = new Font(dgvEmployees.Font, FontStyle.Bold);
                }
                else if (performance == "جيد")
                {
                    e.CellStyle.ForeColor = Color.Blue;
                    e.CellStyle.Font = new Font(dgvEmployees.Font, FontStyle.Bold);
                }
                else if (performance == "متوسط")
                {
                    e.CellStyle.ForeColor = Color.Orange;
                    e.CellStyle.Font = new Font(dgvEmployees.Font, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Red;
                    e.CellStyle.Font = new Font(dgvEmployees.Font, FontStyle.Bold);
                }
            }
        }

        private async Task<List<object>> GetEmployeeStatisticsAsync()
        {
            var employees = new List<object>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    u.UserID_PK,
                    u.FullName as UserName,
                    u.username,
                    u.Email,
                    u.Phone,
                    u.IsAproved,
                    sp.SalePersonName,
                    sp.SalePersonPhone,
                    sp.Is_SalesPerson,
                    sp.Is_PurchasePerson,
                    COUNT(si.SalesInvoiceID_PK) as TotalSales,
                    ISNULL(SUM(si.InvoiceNetTotal), 0) as TotalAmount,
                    MIN(si.SalesInvoiceDate) as FirstSale,
                    MAX(si.SalesInvoiceDate) as LastSale,
                    DATEDIFF(day, MAX(si.SalesInvoiceDate), GETDATE()) as DaysSinceLastSale
                FROM SysPermissions.Data_Users u
                LEFT JOIN SALES.Config_SalePersons sp ON u.UserID_PK = sp.SalePersonID_PK
                LEFT JOIN SALES.Data_SalesInvoices si ON u.UserID_PK = si.CreatedByUserID
                GROUP BY 
                    u.UserID_PK, u.FullName, u.username, u.Email, u.Phone, u.IsAproved,
                    sp.SalePersonName, sp.SalePersonPhone, sp.Is_SalesPerson, sp.Is_PurchasePerson
                ORDER BY 
                    ISNULL(SUM(si.InvoiceNetTotal), 0) DESC,
                    COUNT(si.SalesInvoiceID_PK) DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var totalAmount = reader.GetDecimal("TotalAmount");
                var totalSales = reader.GetInt32("TotalSales");
                var daysSinceLastSale = reader.IsDBNull("DaysSinceLastSale") ? 0 : reader.GetInt32("DaysSinceLastSale");
                
                // Calculate performance rating
                string performance;
                if (totalSales == 0)
                    performance = "غير نشط";
                else if (totalAmount >= 5000 && daysSinceLastSale <= 1)
                    performance = "ممتاز";
                else if (totalAmount >= 2000 && daysSinceLastSale <= 7)
                    performance = "جيد";
                else if (totalAmount >= 500 && daysSinceLastSale <= 30)
                    performance = "متوسط";
                else
                    performance = "ضعيف";

                var employee = new
                {
                    المعرف = reader.GetInt32("UserID_PK"),
                    الاسم = reader.IsDBNull("UserName") ? "غير محدد" : reader.GetString("UserName"),
                    البريد = reader.IsDBNull("Email") ? "غير محدد" : reader.GetString("Email"),
                    معتمد = reader.GetBoolean("IsAproved") ? "نعم" : "لا",
                    مبيعات = reader.IsDBNull("Is_SalesPerson") ? "لا" : (reader.GetBoolean("Is_SalesPerson") ? "نعم" : "لا"),
                    مشتريات = reader.IsDBNull("Is_PurchasePerson") ? "لا" : (reader.GetBoolean("Is_PurchasePerson") ? "نعم" : "لا"),
                    عدد_المبيعات = totalSales.ToString(),
                    إجمالي_المبلغ = $"{totalAmount:N0} د.ل",
                    آخر_بيع = reader.IsDBNull("LastSale") ? "لا يوجد" : reader.GetDateTime("LastSale").ToString("yyyy-MM-dd"),
                    أيام_منذ_آخر_بيع = daysSinceLastSale.ToString(),
                    الأداء = performance
                };

                employees.Add(employee);
            }

            return employees;
        }

    }
}
