using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using SmartInventoryPro.Services;

namespace SmartInventoryPro.Forms
{
    public partial class UpdateForm : Form
    {
        private readonly UpdateService _updateService;
        private Guna2Button btnCheckUpdates;
        private Guna2Button btnApplyUpdate;
        private Guna2Button btnClose;
        private Label lblStatus;
        private Label lblUpdateInfo;
        private Panel pnlUpdateInfo;

        public UpdateForm()
        {
            _updateService = new UpdateService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "نظام التحديثات - InfinityPOS";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.TopMost = true;
            this.MinimumSize = new Size(450, 350);

            // Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            var lblTitle = new Label
            {
                Text = "🔄 نظام التحديثات",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            headerPanel.Controls.Add(lblTitle);

            // Main Panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Status Label
            lblStatus = new Label
            {
                Text = "اضغط على 'فحص التحديثات' للتحقق من وجود تحديثات جديدة",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // Update Info Panel with scrollable text
            pnlUpdateInfo = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                Padding = new Padding(15),
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblUpdateInfo = new Label
            {
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                MaximumSize = new Size(450, 0), // إزالة الحد الأقصى للارتفاع
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10)
            };

            pnlUpdateInfo.Controls.Add(lblUpdateInfo);

            // Buttons Panel
            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(10)
            };

            btnCheckUpdates = new Guna2Button
            {
                Text = "🔍 فحص التحديثات",
                Size = new Size(160, 45),
                Location = new Point(15, 12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(46, 204, 113),
                BorderRadius = 8,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            btnCheckUpdates.Click += BtnCheckUpdates_Click;

            btnApplyUpdate = new Guna2Button
            {
                Text = "⬇️ تحميل التحديث",
                Size = new Size(160, 45),
                Location = new Point(185, 12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(52, 152, 219),
                BorderRadius = 8,
                Enabled = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            btnApplyUpdate.Click += BtnApplyUpdate_Click;

            btnClose = new Guna2Button
            {
                Text = "❌ إغلاق",
                Size = new Size(110, 45),
                Location = new Point(355, 12),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(231, 76, 60),
                BorderRadius = 8,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnClose.Click += (s, e) => this.Close();

            buttonsPanel.Controls.AddRange(new Control[] { btnCheckUpdates, btnApplyUpdate, btnClose });

            mainPanel.Controls.AddRange(new Control[] { pnlUpdateInfo, lblStatus });
            this.Controls.AddRange(new Control[] { mainPanel, buttonsPanel, headerPanel });
        }

        private async void BtnCheckUpdates_Click(object sender, EventArgs e)
        {
            btnCheckUpdates.Enabled = false;
            btnCheckUpdates.Text = "⏳ جاري الفحص...";
            lblStatus.Text = "جاري التحقق من التحديثات...";
            lblStatus.ForeColor = Color.FromArgb(52, 152, 219);

            try
            {
                var updateInfo = await _updateService.CheckForUpdatesAsync();

                if (!string.IsNullOrEmpty(updateInfo.Error))
                {
                    lblStatus.Text = $"خطأ: {updateInfo.Error}";
                    lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                }
                else if (updateInfo.HasUpdates)
                {
                    lblStatus.Text = "✅ توجد تحديثات جديدة متاحة!";
                    lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
                    
                    var hashDisplay = "unknown";
                    if (!string.IsNullOrEmpty(updateInfo.LastHash) && updateInfo.LastHash.Length >= 8)
                    {
                        hashDisplay = updateInfo.LastHash.Substring(0, 8);
                    }
                    
                    // عرض النص كاملاً مع إمكانية التمرير
                    lblUpdateInfo.Text = $"📝 محتوى التحديث:\n{updateInfo.LastMessage}\n\n" +
                                       $"🕒 تاريخ النشر: {updateInfo.LastDate}\n" +
                                       $"🔗 معرف التحديث: {hashDisplay}...";
                    
                    pnlUpdateInfo.Visible = true;
                    btnApplyUpdate.Enabled = true;
                }
                else
                {
                    lblStatus.Text = "✅ التطبيق محدث إلى آخر إصدار";
                    lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
                    pnlUpdateInfo.Visible = false;
                    btnApplyUpdate.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"خطأ في الاتصال: {ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
                btnCheckUpdates.Text = "🔍 فحص التحديثات";
            }
        }

        private async void BtnApplyUpdate_Click(object sender, EventArgs e)
        {
            btnApplyUpdate.Enabled = false;
            btnApplyUpdate.Text = "⏳ جاري التحديث...";
            lblStatus.Text = "جاري تحميل وتطبيق التحديثات...";
            lblStatus.ForeColor = Color.FromArgb(52, 152, 219);

            try
            {
                var result = await _updateService.ApplyUpdateAsync();

                if (result.Success)
                {
                    lblStatus.Text = "✅ تم تطبيق التحديث بنجاح!";
                    lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
                    
                    var newCommitDisplay = "unknown";
                    if (!string.IsNullOrEmpty(result.NewCommit) && result.NewCommit.Length >= 8)
                    {
                        newCommitDisplay = result.NewCommit.Substring(0, 8);
                    }
                    
                    // عرض النص كاملاً مع إمكانية التمرير
                    lblUpdateInfo.Text = $"📝 التحديث الجديد: {result.NewMessage}\n" +
                                       $"🕒 تاريخ التحديث: {result.NewDate}\n" +
                                       $"🔗 معرف التحديث: {newCommitDisplay}...";
                    
                    btnApplyUpdate.Enabled = false;
                    btnApplyUpdate.Text = "✅ تم التحديث";
                }
                else
                {
                    lblStatus.Text = $"خطأ في التحديث: {result.Error}";
                    lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                    btnApplyUpdate.Enabled = true;
                    btnApplyUpdate.Text = "⬇️ تحميل التحديث";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"خطأ في الاتصال: {ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                btnApplyUpdate.Enabled = true;
                btnApplyUpdate.Text = "⬇️ تحميل التحديث";
            }
        }
    }
}
