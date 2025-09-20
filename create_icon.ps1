# إنشاء أيقونة بسيطة للتطبيق
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

# حفظ كملف ICO
$bitmap.Save('app.ico', [System.Drawing.Imaging.ImageFormat]::Icon)

# تنظيف الذاكرة
$graphics.Dispose()
$bitmap.Dispose()
$font.Dispose()
$brush.Dispose()

Write-Host "Icon created successfully: app.ico"
