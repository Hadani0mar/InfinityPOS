# إنشاء أيقونة صحيحة للتطبيق
Add-Type -AssemblyName System.Drawing

# إنشاء صورة 32x32
$bitmap = New-Object System.Drawing.Bitmap(32, 32)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# خلفية زرقاء
$graphics.Clear([System.Drawing.Color]::FromArgb(52, 152, 219))

# كتابة حرف S
$font = New-Object System.Drawing.Font('Arial', 18, [System.Drawing.FontStyle]::Bold)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$graphics.DrawString('S', $font, $brush, 6, 2)

# حفظ كملف PNG أولاً
$bitmap.Save('app.png', [System.Drawing.Imaging.ImageFormat]::Png)

# تحويل PNG إلى ICO باستخدام PowerShell
$pngPath = "app.png"
$icoPath = "app.ico"

# إنشاء ملف ICO بسيط
$icoBytes = @(
    0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x20, 0x20, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x68, 0x05,
    0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x40, 0x00,
    0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
)

# إضافة بيانات الصورة (32x32 بكسل)
for ($i = 0; $i -lt 1024; $i++) {
    $icoBytes += 0x34, 0x98, 0xDB, 0xFF  # لون أزرق
}

# كتابة ملف ICO
[System.IO.File]::WriteAllBytes($icoPath, $icoBytes)

# تنظيف الذاكرة
$graphics.Dispose()
$bitmap.Dispose()
$font.Dispose()
$brush.Dispose()

# حذف ملف PNG المؤقت
Remove-Item $pngPath -ErrorAction SilentlyContinue

Write-Host "Icon created successfully: app.ico"
