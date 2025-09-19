using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPro.Data;
using SmartInventoryPro.Models;
using SmartInventoryPro.Services;
using SmartInventoryPro.Forms.Reports;
using Guna.UI2.WinForms;

namespace SmartInventoryPro.Forms
{
    public partial class MainForm : Form
    {
        private readonly InfinityPOSDbContext _dbContext;
        private readonly StatisticsService _statisticsService;
        private Guna2DataGridView dgvProducts = null!;
        private Label lblStatus = null!;
        private Guna2Button btnRequiredItems = null!;
        private Guna2Button btnLowStock = null!;
        private Guna2Button btnExpiry = null!;
        private Guna2Button btnPerformance = null!;
        private Guna2Button btnUpdates = null!;
        
        // Statistics Cards
        private Panel cardTotalProducts = null!;
        private Panel cardLowStock = null!;
        private Panel cardProductsWithStock = null!;
        private Panel cardNearExpiry = null!;
        private Panel cardTotalSales = null!;
        private Panel cardTotalCash = null!;
        private Panel cardTopProduct = null!;
        private Panel cardTopGroup = null!;

        public MainForm(InfinityPOSDbContext dbContext, StatisticsService statisticsService)
        {
            _dbContext = dbContext;
            _statisticsService = statisticsService;
            InitializeComponent();
            LoadStatistics();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "SmartInventory Pro - Advanced Business Management System v1.3";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.MinimumSize = new Size(1200, 700);

            // Create statistics cards and get the panel
            var statsPanel = CreateStatisticsCards();

            // Create main panel that responds to form resize
            var mainPanel = new Panel
            {
                Location = new Point(0, 60), // Start below the smaller header
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            // Buttons panel with modern styling (responsive)
            var btnPanel = new Panel
            {
                Location = new Point(20, 140), // تعديل الموضع ليكون تحت البطاقات مباشرة
                Size = new Size(mainPanel.Width - 40, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            
            // Add shadow effect
            btnPanel.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, btnPanel.Width, btnPanel.Height);
                using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(0, btnPanel.Height - 2, btnPanel.Width, 2));
                }
            };

            btnRequiredItems = CreateModernButton("📋 تقرير الأصناف المطلوبة", Color.FromArgb(52, 152, 219), 20, 15);
            btnRequiredItems.Click += BtnRequiredItems_Click;

            btnLowStock = CreateModernButton("📉 الأصناف قليلة المخزون", Color.FromArgb(230, 126, 34), 230, 15);
            btnLowStock.Click += BtnLowStock_Click;

            btnExpiry = CreateModernButton("⏰ تنبيهات انتهاء الصلاحية", Color.FromArgb(231, 76, 60), 440, 15);
            btnExpiry.Click += BtnExpiry_Click;

            btnPerformance = CreateModernButton("👥 إحصائيات الموظفين", Color.FromArgb(155, 89, 182), 650, 15);
            btnUpdates = CreateModernButton("🔄 التحديثات", Color.FromArgb(52, 152, 219), 800, 15);
            btnPerformance.Click += BtnPerformance_Click;
            btnUpdates.Click += BtnUpdates_Click;

            btnPanel.Controls.AddRange(new Control[] { btnRequiredItems, btnLowStock, btnExpiry, btnPerformance, btnUpdates });

            // Search and controls panel
            var searchPanel = new Panel
            {
                Location = new Point(20, 220),
                Size = new Size(mainPanel.Width - 40, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Search textbox
            var txtSearch = new Guna2TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(300, 30),
                PlaceholderText = "🔍 البحث بالكود أو ID المنتج...",
                Font = new Font("Segoe UI", 10),
                BorderRadius = 8
            };

            // Show all products checkbox
            var chkShowAll = new CheckBox
            {
                Text = "عرض جميع المنتجات",
                Location = new Point(330, 15),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9),
                Checked = false
            };

            // Filter by stock checkbox
            var chkFilterByStock = new CheckBox
            {
                Text = "المنتجات التي لها مخزون فقط",
                Location = new Point(500, 15),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9),
                Checked = false
            };

            // Search button
            var btnSearch = new Guna2Button
            {
                Text = "بحث",
                Location = new Point(700, 8),
                Size = new Size(80, 34),
                FillColor = Color.FromArgb(52, 152, 219),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BorderRadius = 8
            };

            searchPanel.Controls.AddRange(new Control[] { txtSearch, chkShowAll, chkFilterByStock, btnSearch });

            // Products DataGridView with modern styling (responsive)
            dgvProducts = new Guna2DataGridView
            {
                Location = new Point(20, 280),
                Size = new Size(mainPanel.Width - 40, mainPanel.Height - 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default,
                ColumnHeadersHeight = 35,
                RowTemplate = { Height = 30 }
            };
            
            // Style the DataGridView
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 81, 137);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProducts.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            // Status label with modern styling (responsive position)
            lblStatus = new Label
            {
                Text = "🟢 جاهز - تم تحميل النظام بنجاح",
                Location = new Point(20, mainPanel.Height - 40),
                Size = new Size(500, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = Color.FromArgb(39, 174, 96),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            // Add event handlers for search functionality
            btnSearch.Click += (s, e) => SearchProducts(txtSearch.Text, chkShowAll.Checked, chkFilterByStock.Checked);
            chkShowAll.CheckedChanged += (s, e) => LoadProducts(chkShowAll.Checked, chkFilterByStock.Checked);
            chkFilterByStock.CheckedChanged += (s, e) => LoadProducts(chkShowAll.Checked, chkFilterByStock.Checked);
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)13) SearchProducts(txtSearch.Text, chkShowAll.Checked, chkFilterByStock.Checked); };

            mainPanel.Controls.AddRange(new Control[] { statsPanel, btnPanel, searchPanel, dgvProducts, lblStatus });
            this.Controls.Add(mainPanel);
            
            // Add resize event handler to maintain proper layout
            this.Resize += MainForm_Resize;
        }
        
        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // Update header panel width
            if (this.Controls.Count > 0)
            {
                var headerPanel = this.Controls[0]; // Should be the header panel
                if (headerPanel != null)
                {
                    headerPanel.Width = this.ClientSize.Width;
                }
            }
        }

        private async void LoadProducts(bool showAll = false, bool filterByStock = false)
        {
            try
            {
                lblStatus.Text = "⏳ جاري تحميل قائمة المنتجات...";
                lblStatus.ForeColor = Color.FromArgb(52, 152, 219);
                
                var query = _dbContext.Products
                    .Include(p => p.ProductGroup)
                    .Include(p => p.ProductTrademark)
                    .Where(p => !p.IsInActive); // IsInActive = false means active

                // فلترة حسب المخزون
                if (filterByStock)
                {
                    query = query.Where(p => p.ProductInventories.Any(pi => pi.CurrentStockLevel > 0));
                }

                if (!showAll)
                {
                    query = query.Take(100); // عرض 100 منتج فقط
                }

                var products = await query
                    .Select(p => new
                    {
                        المعرف = p.ProductId,
                        الكود = p.ProductCode,
                        اسم_المنتج = p.ProductDescription,
                        المجموعة = p.ProductGroup != null ? p.ProductGroup.ProductGroupDescription : "غير محدد",
                        العلامة_التجارية = p.ProductTrademark != null ? p.ProductTrademark.ProductTrademarkDescription : "غير محدد",
                        الحالة = p.ProductInventories.Any(pi => pi.CurrentStockLevel > 0) 
                            ? p.ProductInventories.Where(pi => pi.CurrentStockLevel > 0).First().CurrentStockLevel.ToString() 
                            : "منتهي المخزون"
                    })
                    .ToListAsync();

                dgvProducts.DataSource = products;
                dgvProducts.RightToLeft = RightToLeft.Yes;

                // Add cell formatting for stock status
                dgvProducts.CellFormatting += DgvProducts_CellFormatting;

                // Add context menu for product details
                AddProductContextMenu();

                lblStatus.Text = $"📦 تم تحميل {products.Count} منتج بنجاح" + (showAll ? " (جميع المنتجات)" : " (أول 100 منتج)");
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ خطأ في تحميل المنتجات";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show($"خطأ في تحميل المنتجات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SearchProducts(string searchTerm, bool showAll, bool filterByStock = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    LoadProducts(showAll, filterByStock);
                    return;
                }

                lblStatus.Text = "🔍 جاري البحث...";
                lblStatus.ForeColor = Color.FromArgb(52, 152, 219);

                var query = _dbContext.Products
                    .Where(p => !p.IsInActive && // IsInActive = false means active
                           (p.ProductCode.Contains(searchTerm) || 
                            p.ProductId.ToString().Contains(searchTerm) ||
                            p.ProductDescription.Contains(searchTerm)));

                // فلترة حسب المخزون
                if (filterByStock)
                {
                    query = query.Where(p => p.ProductInventories.Any(pi => pi.CurrentStockLevel > 0));
                }

                var products = await query
                    .Select(p => new
                    {
                        المعرف = p.ProductId,
                        الكود = p.ProductCode,
                        اسم_المنتج = p.ProductDescription,
                        الحالة = p.ProductInventories.Any(pi => pi.CurrentStockLevel > 0) 
                            ? p.ProductInventories.Where(pi => pi.CurrentStockLevel > 0).First().CurrentStockLevel.ToString() 
                            : "منتهي المخزون"
                    })
                    .ToListAsync();

                dgvProducts.DataSource = products;
                dgvProducts.RightToLeft = RightToLeft.Yes;

                // Add cell formatting for stock status
                dgvProducts.CellFormatting += DgvProducts_CellFormatting;

                lblStatus.Text = $"🔍 تم العثور على {products.Count} منتج مطابق للبحث";
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ خطأ في البحث";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRequiredItems_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("سيتم تطوير تقرير الأصناف المطلوبة قريباً", "قيد التطوير", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnLowStock_Click(object? sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "⏳ جاري تحليل المنتجات قليلة المخزون...";
                lblStatus.ForeColor = Color.FromArgb(230, 126, 34);

                // Get products with low stock (less than 10 units)
                var lowStockProducts = await _dbContext.ProductInventories
                    .Include(pi => pi.Product)
                    .Where(pi => pi.CurrentStockLevel <= 10 && 
                                pi.Product != null && 
                                !pi.Product.IsInActive) // IsInActive = false means active
                    .Select(pi => new
                    {
                        المعرف = pi.Product!.ProductId,
                        الكود = pi.Product.ProductCode,
                        اسم_المنتج = pi.Product.ProductDescription,
                        المخزون_الحالي = pi.CurrentStockLevel,
                        الحالة = pi.CurrentStockLevel <= 5 ? "حرج" : "منخفض"
                    })
                    .OrderBy(p => p.المخزون_الحالي)
                    .ToListAsync();

                if (lowStockProducts.Any())
                {
                    dgvProducts.DataSource = lowStockProducts;
                    dgvProducts.RightToLeft = RightToLeft.Yes;
                    
                    lblStatus.Text = $"⚠️ تم العثور على {lowStockProducts.Count} منتج بمخزون منخفض";
                    lblStatus.ForeColor = Color.FromArgb(230, 126, 34);
                }
                else
                {
                    lblStatus.Text = "✅ لا توجد منتجات بمخزون منخفض حالياً";
                    lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
                    
                    // Show empty result
                    dgvProducts.DataSource = new List<object>();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ خطأ في تحليل المنتجات قليلة المخزون";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show($"خطأ في تحليل المنتجات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExpiry_Click(object? sender, EventArgs e)
        {
            var reportForm = new ExpiryAlertReportForm(_dbContext);
            reportForm.ShowDialog();
        }

        private void BtnPerformance_Click(object? sender, EventArgs e)
        {
            var connectionString = _dbContext.Database.GetConnectionString() ?? "";
            var reportForm = new EmployeeStatisticsForm(connectionString);
            reportForm.ShowDialog();
        }

        private Panel CreateStatisticsCards()
        {
            // Modern Header with gradient background (responsive)
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.ClientSize.Width, 60), // تقليل الارتفاع
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(64, 81, 137)
            };
            
            headerPanel.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, headerPanel.Width, headerPanel.Height);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(64, 81, 137), Color.FromArgb(52, 152, 219), 
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };

            var lblTitle = new Label
            {
                Text = "🏢 SmartInventory Pro - Business Dashboard",
                Font = new Font("Segoe UI", Math.Max(12, Math.Min(18, headerPanel.Width / 80)), FontStyle.Bold), // خط متكيف
                Location = new Point(20, 15),
                Size = new Size(headerPanel.Width - 400, 30),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dd/MM/yyyy - HH:mm"),
                Font = new Font("Segoe UI", Math.Max(9, Math.Min(12, headerPanel.Width / 120)), FontStyle.Regular), // خط متكيف
                Location = new Point(headerPanel.Width - 200, 20),
                Size = new Size(180, 20),
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // إضافة أيقونة معلومات التطبيق
            var btnAppInfo = new Guna2Button
            {
                Text = "ℹ️",
                Location = new Point(headerPanel.Width - 50, 15),
                Size = new Size(35, 30),
                FillColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 16, FontStyle.Regular),
                BorderRadius = 15,
                BorderThickness = 0,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnAppInfo.Click += BtnAppInfo_Click;

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblDateTime, btnAppInfo });

            // Statistics Panel with modern spacing (responsive) - خط واحد
            var statsPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(this.ClientSize.Width - 40, 100), // تقليل الارتفاع لخط واحد
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            // Calculate responsive spacing for cards - 7 cards in one row
            int cardWidth = 180;
            int cardSpacing = 6;
            int totalCardsWidth = (cardWidth * 8) + (cardSpacing * 7);
            int startX = Math.Max(0, (statsPanel.Width - totalCardsWidth) / 2);

            // All Statistics in one row (responsive spacing) - 8 cards
            cardTotalProducts = CreateStatCard("📦", "إجمالي المنتجات", "0", Color.FromArgb(52, 152, 219), startX, 0);
            cardLowStock = CreateStatCard("⚠️", "منتجات منتهية المخزون", "0", Color.FromArgb(230, 126, 34), startX + (cardWidth + cardSpacing), 0);
            cardProductsWithStock = CreateStatCard("📋", "منتجات تحتوي مخزون", "0", Color.FromArgb(52, 73, 94), startX + (cardWidth + cardSpacing) * 2, 0);
            cardNearExpiry = CreateStatCard("⏰", "قريبة الانتهاء", "0", Color.FromArgb(231, 76, 60), startX + (cardWidth + cardSpacing) * 3, 0);
            cardTotalSales = CreateStatCardWithToggle("💰", "إجمالي المبيعات", "0 د.ل", Color.FromArgb(39, 174, 96), startX + (cardWidth + cardSpacing) * 4, 0);
            cardTotalCash = CreateStatCardWithToggle("💵", "إجمالي النقد", "0 د.ل", Color.FromArgb(46, 204, 113), startX + (cardWidth + cardSpacing) * 5, 0, false);
            cardTopProduct = CreateStatCard("🏆", "أفضل منتج", "غير محدد", Color.FromArgb(155, 89, 182), startX + (cardWidth + cardSpacing) * 6, 0);
            cardTopGroup = CreateStatCard("📊", "أفضل فئة", "غير محدد", Color.FromArgb(26, 188, 156), startX + (cardWidth + cardSpacing) * 7, 0);

            statsPanel.Controls.AddRange(new Control[] { 
                cardTotalProducts, cardLowStock, cardProductsWithStock, cardNearExpiry, cardTotalSales, cardTotalCash,
                cardTopProduct, cardTopGroup
            });

            // Add headerPanel directly to form (stays at top)
            this.Controls.Add(headerPanel);
            
            // Return statsPanel to be added to mainPanel later
            return statsPanel;
        }

        private Panel CreateStatCard(string icon, string title, string value, Color color, int x, int y)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(180, 90), // تعديل الحجم للـ 7 بطاقات
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Add modern shadow and border effects
            card.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, card.Width, card.Height);
                
                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, new Rectangle(3, 3, rect.Width, rect.Height));
                }
                
                // Draw main background
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                
                // Draw colored left border
                using (var borderBrush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(borderBrush, new Rectangle(0, 0, 5, rect.Height));
                }
                
                // Draw subtle border
                using (var borderPen = new Pen(Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 22, FontStyle.Regular),
                Location = new Point(10, 25), // توسيط أكثر
                Size = new Size(60, 40),
                ForeColor = color,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7, FontStyle.Regular),
                Location = new Point(65, 12),
                Size = new Size(105, 18),
                ForeColor = Color.FromArgb(100, 100, 100),
                BackColor = Color.Transparent
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(65, 32),
                Size = new Size(105, 30),
                ForeColor = Color.FromArgb(50, 50, 50),
                BackColor = Color.Transparent,
                Tag = "value"
            };

            card.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblValue });
            return card;
        }

        private bool _salesAmountVisible = false; // مخفي افتراضياً
        private string _actualSalesAmount = "";
        private bool _cashAmountVisible = false; // مخفي افتراضياً
        private string _actualCashAmount = "";

        private Panel CreateStatCardWithToggle(string icon, string title, string value, Color color, int x, int y, bool isSalesCard = true)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(180, 90),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Add modern shadow and border effects
            card.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, card.Width, card.Height);
                
                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, new Rectangle(3, 3, rect.Width, rect.Height));
                }
                
                // Draw main background
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                
                // Draw colored left border
                using (var borderBrush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(borderBrush, new Rectangle(0, 0, 5, rect.Height));
                }
                
                // Draw subtle border
                using (var borderPen = new Pen(Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 22, FontStyle.Regular),
                Location = new Point(10, 25),
                Size = new Size(60, 40),
                ForeColor = color,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7, FontStyle.Regular),
                Location = new Point(65, 12),
                Size = new Size(75, 18),
                ForeColor = Color.FromArgb(100, 100, 100),
                BackColor = Color.Transparent
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(65, 32),
                Size = new Size(75, 30),
                ForeColor = Color.FromArgb(50, 50, 50),
                BackColor = Color.Transparent,
                Tag = "value"
            };

            // Eye toggle button
            var btnToggle = new Label
            {
                Text = "👁️",
                Font = new Font("Segoe UI Emoji", 12, FontStyle.Regular),
                Location = new Point(150, 45),
                Size = new Size(20, 20),
                ForeColor = Color.FromArgb(100, 100, 100),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Tag = "toggle"
            };

            btnToggle.Click += (s, e) =>
            {
                if (isSalesCard)
                {
                    _salesAmountVisible = !_salesAmountVisible;
                    if (_salesAmountVisible)
                    {
                        btnToggle.Text = "👁️";
                        lblValue.Text = _actualSalesAmount;
                    }
                    else
                    {
                        btnToggle.Text = "🙈";
                        lblValue.Text = "*** د.ل";
                    }
                }
                else
                {
                    _cashAmountVisible = !_cashAmountVisible;
                    if (_cashAmountVisible)
                    {
                        btnToggle.Text = "👁️";
                        lblValue.Text = _actualCashAmount;
                    }
                    else
                    {
                        btnToggle.Text = "🙈";
                        lblValue.Text = "*** د.ل";
                    }
                }
            };

            // Store actual amount for toggling
            if (isSalesCard)
            {
                _actualSalesAmount = value;
                // تطبيق الحالة المخفية افتراضياً
                if (!_salesAmountVisible)
                {
                    btnToggle.Text = "🙈";
                    lblValue.Text = "*** د.ل";
                }
            }
            else
            {
                _actualCashAmount = value;
                // تطبيق الحالة المخفية افتراضياً
                if (!_cashAmountVisible)
                {
                    btnToggle.Text = "🙈";
                    lblValue.Text = "*** د.ل";
                }
            }

            card.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblValue, btnToggle });
            return card;
        }

        private Guna2Button CreateModernButton(string text, Color color, int x, int y)
        {
            var button = new Guna2Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(200, 40),
                FillColor = color,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BorderRadius = 12,
                BorderThickness = 0,
                ShadowDecoration = { Enabled = true }
            };

            // Add hover effects
            var originalColor = color;
            var hoverColor = Color.FromArgb(Math.Max(0, color.R - 30), Math.Max(0, color.G - 30), Math.Max(0, color.B - 30));
            
            button.MouseEnter += (s, e) => button.FillColor = hoverColor;
            button.MouseLeave += (s, e) => button.FillColor = originalColor;

            return button;
        }

        private async void LoadStatistics()
        {
            try
            {
                lblStatus.Text = "⏳ جاري تحميل الإحصائيات من قاعدة البيانات...";
                lblStatus.ForeColor = Color.FromArgb(52, 152, 219);
                
                var stats = await _statisticsService.GetDashboardStatisticsAsync();
                
                UpdateStatCard(cardTotalProducts, stats.TotalProducts.ToString());
                UpdateStatCard(cardLowStock, stats.OutOfStockCount.ToString());
                UpdateStatCard(cardProductsWithStock, stats.ProductsWithStock.ToString());
                UpdateStatCard(cardNearExpiry, stats.NearExpiryProductsCount.ToString());
                
                // Update sales card with new currency and store actual amount
                var salesAmount = $"{stats.TotalSalesThisMonth:N0} د.ل";
                _actualSalesAmount = salesAmount;
                UpdateStatCard(cardTotalSales, _salesAmountVisible ? salesAmount : "*** د.ل");
                
                // Update cash card and store actual amount
                var cashAmount = $"{stats.TotalCashThisMonth:N0} د.ل";
                _actualCashAmount = cashAmount;
                UpdateStatCard(cardTotalCash, _cashAmountVisible ? cashAmount : "*** د.ل");
                
                UpdateStatCard(cardTopProduct, stats.TopSellingProduct);
                UpdateStatCard(cardTopGroup, stats.TopSellingProductGroup);

                lblStatus.Text = "✅ تم تحميل جميع الإحصائيات بنجاح";
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ خطأ في تحميل الإحصائيات - يرجى المحاولة مرة أخرى";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show($"خطأ في تحميل الإحصائيات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatCard(Panel card, string value)
        {
            var valueLabel = card.Controls.OfType<Label>().FirstOrDefault(l => l.Tag?.ToString() == "value");
            if (valueLabel != null)
                valueLabel.Text = value;
        }

        private void DgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts.Columns[e.ColumnIndex].Name == "الحالة" && e.Value != null)
            {
                string status = e.Value.ToString() ?? "";
                if (status == "منتهي المخزون")
                {
                    e.CellStyle.ForeColor = Color.Red;
                    e.CellStyle.Font = new Font(dgvProducts.Font, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Green;
                    e.CellStyle.Font = new Font(dgvProducts.Font, FontStyle.Bold);
                }
            }
        }

        private void AddProductContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            var menuItem = new ToolStripMenuItem("📊 عرض تفاصيل المنتج");
            menuItem.Click += async (s, e) => await ShowProductDetails();
            contextMenu.Items.Add(menuItem);
            
            dgvProducts.ContextMenuStrip = contextMenu;
        }

        private async Task ShowProductDetails()
        {
            if (dgvProducts.SelectedRows.Count == 0) return;

            try
            {
                var selectedRow = dgvProducts.SelectedRows[0];
                var productId = Convert.ToInt32(selectedRow.Cells["المعرف"].Value);

                // Get detailed product information
                var product = await _dbContext.Products
                    .Include(p => p.ProductInventories)
                    .Include(p => p.ProductGroup)
                    .Include(p => p.ProductTrademark)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    MessageBox.Show("لم يتم العثور على تفاصيل المنتج", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get sales statistics
                var salesStats = await GetProductSalesStats(productId);
                var inventory = product.ProductInventories?.FirstOrDefault();

                // Create details form
                var detailsForm = CreateProductDetailsForm(product, inventory, salesStats);
                detailsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض تفاصيل المنتج: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<ProductSalesStats> GetProductSalesStats(long productId)
        {
            var stats = new ProductSalesStats();

            try
            {
                var thirtyDaysAgo = DateTime.Today.AddDays(-30);

                var salesData = await _dbContext.SalesInvoiceItems
                    .Include(item => item.SalesInvoice)
                    .Where(item => item.ProductId == productId && 
                                  item.SalesInvoice!.InvoiceDate >= thirtyDaysAgo)
                    .ToListAsync();

                stats.TotalSoldQuantity = salesData.Sum(item => item.Quantity ?? 0);
                stats.TotalSalesAmount = salesData.Sum(item => item.TotalPrice ?? 0);
                stats.AveragePrice = salesData.Any() ? salesData.Average(item => item.UnitPrice ?? 0) : 0;
                stats.SalesPercentage = CalculateSalesPercentage(productId, stats.TotalSoldQuantity);
            }
            catch
            {
                // Return default values on error
            }

            return stats;
        }

        private decimal CalculateSalesPercentage(long productId, decimal soldQuantity)
        {
            try
            {
                var totalSalesThisMonth = _dbContext.SalesInvoiceItems
                    .Include(item => item.SalesInvoice)
                    .Where(item => item.SalesInvoice!.InvoiceDate >= DateTime.Today.AddDays(-30))
                    .Sum(item => item.Quantity ?? 0);

                return totalSalesThisMonth > 0 ? (soldQuantity / totalSalesThisMonth) * 100 : 0;
            }
            catch
            {
                return 0;
            }
        }

        private Form CreateProductDetailsForm(Product product, ProductInventory? inventory, ProductSalesStats stats)
        {
            var form = new Form
            {
                Text = "تفاصيل المنتج",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                BackColor = Color.White
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            var lblTitle = new Label
            {
                Text = $"📦 {product.ProductDescription}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(440, 30),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            var lblCode = new Label
            {
                Text = $"🏷️ الكود: {product.ProductCode}",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 60),
                Size = new Size(440, 25)
            };

            var lblGroup = new Label
            {
                Text = $"📂 المجموعة: {product.ProductGroup?.ProductGroupDescription ?? "غير محدد"}",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 90),
                Size = new Size(440, 25)
            };

            var lblTrademark = new Label
            {
                Text = $"🏢 العلامة التجارية: {product.ProductTrademark?.ProductTrademarkDescription ?? "غير محدد"}",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 120),
                Size = new Size(440, 25)
            };

            var lblStock = new Label
            {
                Text = $"📊 المخزون الحالي: {inventory?.CurrentStockLevel ?? 0:N2}",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 150),
                Size = new Size(440, 25),
                ForeColor = (inventory?.CurrentStockLevel ?? 0) < 10 ? Color.Red : Color.Green
            };

            var lblPrice = new Label
            {
                Text = $"💰 متوسط السعر: {stats.AveragePrice:N2} د.ل",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 180),
                Size = new Size(440, 25)
            };

            var lblSalesPercentage = new Label
            {
                Text = $"📈 نسبة البيع: {stats.SalesPercentage:N2}%",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 210),
                Size = new Size(440, 25),
                ForeColor = Color.FromArgb(39, 174, 96)
            };

            var lblTotalSales = new Label
            {
                Text = $"💵 إجمالي المبيعات (30 يوم): {stats.TotalSalesAmount:N2} د.ل",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 240),
                Size = new Size(440, 25)
            };

            var btnClose = new Button
            {
                Text = "إغلاق",
                Location = new Point(200, 300),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnClose.Click += (s, e) => form.Close();

            panel.Controls.AddRange(new Control[] { 
                lblTitle, lblCode, lblGroup, lblTrademark, lblStock, 
                lblPrice, lblSalesPercentage, lblTotalSales, btnClose 
            });

            form.Controls.Add(panel);
            return form;
        }

        private void BtnUpdates_Click(object sender, EventArgs e)
        {
            var updateForm = new UpdateForm();
            updateForm.ShowDialog();
        }

        private void BtnAppInfo_Click(object? sender, EventArgs e)
        {
            var infoForm = new Form
            {
                Text = "معلومات التطبيق - SmartInventory Pro",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                BackColor = Color.White
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            var lblTitle = new Label
            {
                Text = "🏢 SmartInventory Pro",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(540, 35),
                ForeColor = Color.FromArgb(52, 152, 219),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblVersion = new Label
            {
                Text = "الإصدار: v1.3 - Advanced Business Management System",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Location = new Point(20, 70),
                Size = new Size(540, 25),
                ForeColor = Color.FromArgb(100, 100, 100),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblFeatures = new Label
            {
                Text = "الميزات الرئيسية:",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 110),
                Size = new Size(540, 30),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            var features = new string[]
            {
                "📦 إدارة المخزون المتقدمة - تتبع المنتجات والمخزون",
                "📊 لوحة تحكم ذكية - إحصائيات شاملة ومؤشرات الأداء",
                "🔍 بحث متقدم - البحث بالكود أو اسم المنتج",
                "⚠️ تنبيهات المخزون - تحذيرات المنتجات قليلة المخزون",
                "⏰ تنبيهات انتهاء الصلاحية - إدارة تواريخ انتهاء المنتجات",
                "👥 إحصائيات الموظفين - تحليل أداء فريق المبيعات",
                "💰 تقارير المبيعات - تحليل الإيرادات والأرباح",
                "🔄 نظام التحديثات - تحديثات تلقائية من GitHub",
                "🏢 متعدد القطاعات - مناسب لجميع أنواع الأعمال",
                "🔒 أمان متقدم - حماية البيانات والمعلومات"
            };

            var yPos = 150;
            foreach (var feature in features)
            {
                var lblFeature = new Label
                {
                    Text = feature,
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(20, yPos),
                    Size = new Size(540, 25),
                    ForeColor = Color.FromArgb(60, 60, 60)
                };
                panel.Controls.Add(lblFeature);
                yPos += 25;
            }

            var lblCopyright = new Label
            {
                Text = "© 2024 SmartInventory Pro - جميع الحقوق محفوظة",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(20, yPos + 20),
                Size = new Size(540, 20),
                ForeColor = Color.FromArgb(150, 150, 150),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnClose = new Guna2Button
            {
                Text = "إغلاق",
                Location = new Point(250, yPos + 50),
                Size = new Size(100, 35),
                FillColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8
            };

            btnClose.Click += (s, e) => infoForm.Close();

            panel.Controls.AddRange(new Control[] { 
                lblTitle, lblVersion, lblFeatures, lblCopyright, btnClose 
            });

            infoForm.Controls.Add(panel);
            infoForm.ShowDialog();
        }

        public class ProductSalesStats
        {
            public decimal TotalSoldQuantity { get; set; }
            public decimal TotalSalesAmount { get; set; }
            public decimal AveragePrice { get; set; }
            public decimal SalesPercentage { get; set; }
        }
    }
}