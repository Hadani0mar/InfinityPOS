using System;
using System.Drawing;
using System.Windows.Forms;
using SmartInventoryPro.Data;
using Guna.UI2.WinForms;

namespace SmartInventoryPro.Forms.Reports
{
    public partial class SimpleReportForm : Form
    {
        private readonly InfinityPOSDbContext _dbContext;
        private Guna2DataGridView dgvData = null!;

        public SimpleReportForm(InfinityPOSDbContext dbContext, string title)
        {
            _dbContext = dbContext;
            this.Text = title;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(250, 250, 250);

            dgvData = new Guna2DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(950, 520),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            dgvData.RightToLeft = RightToLeft.Yes;

            this.Controls.Add(dgvData);
        }
    }

    // Placeholder forms for other reports
    public class RequiredItemsReportForm : SimpleReportForm
    {
        public RequiredItemsReportForm(InfinityPOSDbContext dbContext) 
            : base(dbContext, "تقرير الأصناف المطلوبة") { }
    }

    public class LowStockReportForm : SimpleReportForm
    {
        public LowStockReportForm(InfinityPOSDbContext dbContext) 
            : base(dbContext, "تقرير الأصناف قليلة المخزون") { }
    }

    public class ExpiryAlertReportForm : SimpleReportForm
    {
        public ExpiryAlertReportForm(InfinityPOSDbContext dbContext) 
            : base(dbContext, "تقرير انتهاء الصلاحية") { }
    }

    public class EmployeePerformanceForm : SimpleReportForm
    {
        public EmployeePerformanceForm(InfinityPOSDbContext dbContext) 
            : base(dbContext, "تقرير أداء الموظفين") { }
    }
}
