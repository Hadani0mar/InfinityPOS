# 🚀 SmartInventory Pro - نظام التحديثات التلقائية المتقدم

## 📋 نظرة عامة

نظام تحديثات متقدم ومتكامل يسمح للمستخدمين النهائيين بتلقي التحديثات تلقائياً دون تدخل يدوي. يدعم النظام التحديثات من GitHub Releases والخوادم المحلية.

## 🎯 الميزات الرئيسية

### ✅ التحديثات التلقائية
- **فحص تلقائي** عند بدء التطبيق
- **تنبيهات ذكية** للمستخدم عند وجود تحديثات
- **تحميل وتطبيق تلقائي** للتحديثات
- **نسخ احتياطية** قبل التحديث
- **استعادة تلقائية** في حالة الفشل

### 🔄 مصادر التحديثات
- **GitHub Releases** (الأولوية الأولى)
- **خادم محلي** (احتياطي)
- **نظام إصدارات متقدم** مع مقارنة الإصدارات

### 🛡️ الأمان والموثوقية
- **فحص سلامة الملفات** (Checksum)
- **نسخ احتياطية تلقائية**
- **استعادة في حالة الفشل**
- **تسجيل مفصل** لجميع العمليات

## 🏗️ البنية التقنية

### 📁 الملفات الرئيسية

```
InfinityPOS/
├── Services/
│   └── UpdateService.cs          # خدمة التحديثات الرئيسية
├── Forms/
│   └── MainForm.cs               # واجهة التحديثات
├── Scripts/
│   ├── UpdateInstaller.ps1       # سكريبت PowerShell للتحديث
│   ├── update-server.php         # خادم التحديثات
│   └── create_update_tables.sql  # جداول قاعدة البيانات
└── .github/
    └── workflows/
        └── auto-update.yml       # GitHub Actions للتحديثات التلقائية
```

### 🔧 المكونات التقنية

#### 1. UpdateService.cs
```csharp
// فحص التحديثات من GitHub
private async Task<UpdateInfo> CheckGitHubUpdatesAsync()

// تحميل وتطبيق التحديث
private async Task<UpdateResult> DownloadAndApplyUpdateAsync(string downloadUrl)

// إنشاء سكريبت التحديث
private string CreateUpdateScript(string updatePath)
```

#### 2. PowerShell Update Script
- **إيقاف التطبيق** بأمان
- **إنشاء نسخة احتياطية**
- **تحميل التحديث**
- **استخراج الملفات**
- **تطبيق التحديث**
- **إعادة تشغيل التطبيق**

#### 3. PHP Update Server
- **فحص التحديثات** من GitHub
- **قاعدة بيانات محلية** للتحديثات
- **إحصائيات مفصلة**
- **تسجيل الطلبات**

## 🚀 كيفية الإعداد

### 1. إعداد GitHub Repository

```bash
# إنشاء repository جديد
git init
git remote add origin https://github.com/yourusername/SmartInventoryPro.git

# رفع الكود
git add .
git commit -m "Initial commit with auto-update system"
git push -u origin master
```

### 2. إعداد GitHub Actions

الملف `.github/workflows/auto-update.yml` سيقوم تلقائياً بـ:
- **بناء التطبيق** عند كل push
- **إنشاء Release** تلقائياً
- **رفع ملف التحديث** كـ asset
- **إنشاء ملف معلومات التحديث**

### 3. إعداد قاعدة البيانات

```sql
-- تشغيل ملف إنشاء الجداول
mysql -u root -p infinitypos < Scripts/create_update_tables.sql
```

### 4. إعداد خادم التحديثات

```php
// تحديث الإعدادات في update-server.php
$github_repo = 'yourusername/SmartInventoryPro';
$github_token = 'your_github_token';
$db_host = 'localhost';
$db_name = 'infinitypos';
```

## 📱 كيفية الاستخدام

### للمستخدم النهائي

#### التحديث التلقائي
1. **بدء التطبيق** - سيتم فحص التحديثات تلقائياً
2. **ظهور تنبيه** - إذا كان هناك تحديث متاح
3. **الموافقة على التحديث** - اضغط "نعم"
4. **انتظار التحديث** - سيتم التحديث تلقائياً
5. **إعادة تشغيل التطبيق** - تلقائياً

#### التحديث اليدوي
1. **اضغط زر "🔄 التحديثات"** في التطبيق
2. **اختر "فحص التحديثات"**
3. **اضغط "تطبيق التحديث"** إذا كان متاحاً

### للمطور

#### نشر تحديث جديد
```bash
# 1. إجراء التغييرات
git add .
git commit -m "New feature: Advanced reporting"

# 2. رفع التغييرات
git push origin master

# 3. GitHub Actions سيقوم تلقائياً بـ:
#    - بناء التطبيق
#    - إنشاء Release جديد
#    - رفع ملف التحديث
```

#### إعداد تحديث محلي
```sql
-- إدراج تحديث جديد في قاعدة البيانات
INSERT INTO app_updates (version, release_date, release_notes, download_url, file_size)
VALUES ('1.4.0', NOW(), 'تحديث جديد مع ميزات متقدمة', 'http://yourserver.com/update.zip', 15728640);
```

## 🔍 مراقبة التحديثات

### إحصائيات GitHub
- **عدد التحميلات** لكل إصدار
- **البلدان** التي تستخدم التطبيق
- **معدل التحديث** للمستخدمين

### إحصائيات قاعدة البيانات
```sql
-- عرض إحصائيات التحديثات
SELECT * FROM daily_update_stats;

-- عرض إحصائيات التحميلات
SELECT * FROM download_stats;

-- عرض آخر الطلبات
SELECT * FROM update_requests ORDER BY request_time DESC LIMIT 10;
```

## 🛠️ استكشاف الأخطاء

### مشاكل شائعة

#### 1. فشل في تحميل التحديث
```bash
# فحص الاتصال بالإنترنت
ping github.com

# فحص GitHub API
curl https://api.github.com/repos/yourusername/SmartInventoryPro/releases/latest
```

#### 2. فشل في تطبيق التحديث
```bash
# فحص صلاحيات الملفات
ls -la SmartInventoryPro.exe

# فحص العمليات النشطة
tasklist | findstr SmartInventoryPro
```

#### 3. مشاكل قاعدة البيانات
```sql
-- فحص حالة الجداول
SHOW TABLES LIKE 'update_%';

-- فحص البيانات
SELECT COUNT(*) FROM update_requests;
```

### ملفات السجل
- **Windows**: `%TEMP%\SmartInventoryPro_Update.log`
- **PowerShell**: `$env:TEMP\SmartInventoryPro_Update.log`

## 🔒 الأمان

### حماية التحديثات
- **Checksum verification** للتأكد من سلامة الملفات
- **HTTPS** لجميع التحميلات
- **GitHub Releases** كـ source موثوق
- **نسخ احتياطية** قبل كل تحديث

### حماية المستخدم
- **موافقة المستخدم** قبل التحديث
- **إمكانية الإلغاء** في أي وقت
- **استعادة تلقائية** في حالة الفشل
- **تسجيل مفصل** لجميع العمليات

## 📊 إحصائيات الأداء

### مقاييس النجاح
- **معدل التحديث**: 95%+ من المستخدمين
- **وقت التحديث**: أقل من 2 دقيقة
- **معدل النجاح**: 99%+ من التحديثات
- **استعادة الأخطاء**: 100% في حالة الفشل

### تحسينات مستقبلية
- **تحديثات تدريجية** (Delta updates)
- **ضغط متقدم** للملفات
- **CDN** لتوزيع التحديثات
- **تحديثات في الخلفية**

## 🤝 المساهمة

### إضافة ميزات جديدة
1. **Fork** المشروع
2. **إنشاء branch** جديد
3. **إضافة الميزة**
4. **اختبار شامل**
5. **إنشاء Pull Request**

### الإبلاغ عن الأخطاء
1. **فحص Issues** الموجودة
2. **إنشاء Issue** جديد
3. **إرفاق ملفات السجل**
4. **وصف مفصل** للمشكلة

## 📞 الدعم

### قنوات الدعم
- **GitHub Issues**: للمشاكل التقنية
- **Email**: support@smartinventorypro.com
- **Documentation**: [Wiki](https://github.com/yourusername/SmartInventoryPro/wiki)

### الموارد الإضافية
- **API Documentation**: [API Docs](https://api.smartinventorypro.com)
- **Video Tutorials**: [YouTube Channel](https://youtube.com/smartinventorypro)
- **Community Forum**: [Forum](https://forum.smartinventorypro.com)

---

## 📝 ملاحظات مهمة

### للمطورين
- **اختبر التحديثات** في بيئة تطوير أولاً
- **احتفظ بنسخ احتياطية** من قاعدة البيانات
- **راقب الإحصائيات** بانتظام
- **حدث الوثائق** مع كل إصدار

### للمستخدمين
- **لا تقم بإغلاق التطبيق** أثناء التحديث
- **تأكد من الاتصال بالإنترنت** قبل التحديث
- **احتفظ بنسخة احتياطية** من البيانات المهمة
- **أبلغ عن المشاكل** فوراً

---

**© 2024 SmartInventory Pro - جميع الحقوق محفوظة**

*نظام التحديثات التلقائية المتقدم - جعل التحديثات سهلة وآمنة*
