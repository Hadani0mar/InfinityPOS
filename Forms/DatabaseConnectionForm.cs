using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace InfinityPOS.Forms
{
    public partial class DatabaseConnectionForm : Form
    {
        public string? ConnectionString { get; private set; }

        private TextBox txtServer = null!;
        private TextBox txtDatabase = null!;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnTest = null!;
        private Button btnConnect = null!;
        private Button btnCancel = null!;

        public DatabaseConnectionForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "إعدادات الاتصال بقاعدة البيانات - InfinityPOS";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Server
            var lblServer = new Label
            {
                Text = "الخادم:",
                Location = new Point(400, 30),
                Size = new Size(80, 25)
            };
            txtServer = new TextBox
            {
                Text = "localhost\\SQLEXPRESS",
                Location = new Point(150, 30),
                Size = new Size(240, 25)
            };

            // Database
            var lblDatabase = new Label
            {
                Text = "قاعدة البيانات:",
                Location = new Point(400, 70),
                Size = new Size(80, 25)
            };
            txtDatabase = new TextBox
            {
                Text = "pharmacy1",
                Location = new Point(150, 70),
                Size = new Size(240, 25)
            };

            // Username
            var lblUsername = new Label
            {
                Text = "المستخدم:",
                Location = new Point(400, 110),
                Size = new Size(80, 25)
            };
            txtUsername = new TextBox
            {
                Text = "sa",
                Location = new Point(150, 110),
                Size = new Size(240, 25)
            };

            // Password
            var lblPassword = new Label
            {
                Text = "كلمة المرور:",
                Location = new Point(400, 150),
                Size = new Size(80, 25)
            };
            txtPassword = new TextBox
            {
                Text = "123",
                Location = new Point(150, 150),
                Size = new Size(240, 25),
                UseSystemPasswordChar = true
            };

            // Buttons
            btnTest = new Button
            {
                Text = "اختبار الاتصال",
                Location = new Point(300, 200),
                Size = new Size(100, 30)
            };
            btnTest.Click += BtnTest_Click;

            btnConnect = new Button
            {
                Text = "اتصال",
                Location = new Point(190, 200),
                Size = new Size(100, 30)
            };
            btnConnect.Click += BtnConnect_Click;

            btnCancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(80, 200),
                Size = new Size(100, 30)
            };
            btnCancel.Click += BtnCancel_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblServer, txtServer,
                lblDatabase, txtDatabase,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnTest, btnConnect, btnCancel
            });
        }

        private void BtnTest_Click(object? sender, EventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                MessageBox.Show("تم الاتصال بنجاح!", "نجح الاختبار", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الاتصال: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnConnect_Click(object? sender, EventArgs e)
        {
            try
            {
                ConnectionString = BuildConnectionString();
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الاتصال: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private string BuildConnectionString()
        {
            return $"Server={txtServer.Text};Database={txtDatabase.Text};User Id={txtUsername.Text};Password={txtPassword.Text};TrustServerCertificate=true;MultipleActiveResultSets=true";
        }
    }
}