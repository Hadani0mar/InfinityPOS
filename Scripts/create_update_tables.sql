-- SmartInventory Pro - Update System Database Tables
-- جداول نظام التحديثات

-- جدول تحديثات التطبيق
CREATE TABLE IF NOT EXISTS app_updates (
    id INT AUTO_INCREMENT PRIMARY KEY,
    version VARCHAR(20) NOT NULL UNIQUE,
    release_date DATETIME NOT NULL,
    release_notes TEXT,
    download_url VARCHAR(500),
    file_size BIGINT,
    checksum VARCHAR(64), -- للتأكد من سلامة الملف
    is_active BOOLEAN DEFAULT TRUE,
    is_mandatory BOOLEAN DEFAULT FALSE, -- تحديث إجباري
    min_supported_version VARCHAR(20), -- أقل إصدار مدعوم
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- جدول طلبات فحص التحديثات
CREATE TABLE IF NOT EXISTS update_requests (
    id INT AUTO_INCREMENT PRIMARY KEY,
    client_version VARCHAR(20),
    client_ip VARCHAR(45),
    user_agent TEXT,
    request_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_client_ip (client_ip),
    INDEX idx_request_time (request_time)
);

-- جدول تحميلات التحديثات
CREATE TABLE IF NOT EXISTS update_downloads (
    id INT AUTO_INCREMENT PRIMARY KEY,
    version VARCHAR(20),
    client_ip VARCHAR(45),
    download_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    download_size BIGINT,
    success BOOLEAN DEFAULT TRUE,
    INDEX idx_version (version),
    INDEX idx_download_time (download_time)
);

-- جدول أخطاء التحديثات
CREATE TABLE IF NOT EXISTS update_errors (
    id INT AUTO_INCREMENT PRIMARY KEY,
    client_version VARCHAR(20),
    client_ip VARCHAR(45),
    error_message TEXT,
    error_code VARCHAR(50),
    error_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_error_time (error_time)
);

-- جدول إحصائيات التحديثات
CREATE TABLE IF NOT EXISTS update_statistics (
    id INT AUTO_INCREMENT PRIMARY KEY,
    date DATE NOT NULL,
    total_requests INT DEFAULT 0,
    unique_clients INT DEFAULT 0,
    successful_downloads INT DEFAULT 0,
    failed_downloads INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_date (date)
);

-- إدراج بيانات تجريبية
INSERT INTO app_updates (version, release_date, release_notes, download_url, file_size, is_active, is_mandatory) VALUES
('1.3.0', '2024-01-15 10:00:00', 'إصدار أولي مع نظام التحديثات التلقائية', 'https://github.com/yourusername/SmartInventoryPro/releases/download/v1.3.0/SmartInventoryPro_Update.zip', 15728640, TRUE, FALSE),
('1.2.5', '2024-01-10 15:30:00', 'إصلاح أخطاء في نظام الإحصائيات', 'https://github.com/yourusername/SmartInventoryPro/releases/download/v1.2.5/SmartInventoryPro_Update.zip', 15204352, TRUE, FALSE),
('1.2.0', '2024-01-05 09:15:00', 'إضافة ميزات جديدة وتحسينات في الأداء', 'https://github.com/yourusername/SmartInventoryPro/releases/download/v1.2.0/SmartInventoryPro_Update.zip', 14680064, TRUE, FALSE);

-- إنشاء فهارس إضافية للأداء
CREATE INDEX idx_app_updates_version ON app_updates(version);
CREATE INDEX idx_app_updates_active ON app_updates(is_active);
CREATE INDEX idx_update_requests_version ON update_requests(client_version);
CREATE INDEX idx_update_downloads_success ON update_downloads(success);

-- إنشاء view للإحصائيات اليومية
CREATE VIEW daily_update_stats AS
SELECT 
    DATE(request_time) as date,
    COUNT(*) as total_requests,
    COUNT(DISTINCT client_ip) as unique_clients,
    COUNT(DISTINCT CASE WHEN client_version IS NOT NULL THEN client_version END) as unique_versions
FROM update_requests 
GROUP BY DATE(request_time)
ORDER BY date DESC;

-- إنشاء view لإحصائيات التحميلات
CREATE VIEW download_stats AS
SELECT 
    version,
    COUNT(*) as total_downloads,
    COUNT(DISTINCT client_ip) as unique_downloaders,
    AVG(download_size) as avg_download_size,
    SUM(CASE WHEN success = TRUE THEN 1 ELSE 0 END) as successful_downloads,
    SUM(CASE WHEN success = FALSE THEN 1 ELSE 0 END) as failed_downloads
FROM update_downloads 
GROUP BY version
ORDER BY version DESC;

-- إنشاء stored procedure لتحديث الإحصائيات اليومية
DELIMITER //
CREATE PROCEDURE UpdateDailyStats()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE stat_date DATE;
    DECLARE total_reqs INT;
    DECLARE unique_clients INT;
    DECLARE successful_dls INT;
    DECLARE failed_dls INT;
    
    DECLARE date_cursor CURSOR FOR 
        SELECT DISTINCT DATE(request_time) as date FROM update_requests 
        WHERE DATE(request_time) >= DATE_SUB(CURDATE(), INTERVAL 30 DAY);
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    OPEN date_cursor;
    
    read_loop: LOOP
        FETCH date_cursor INTO stat_date;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- حساب الإحصائيات للتاريخ
        SELECT COUNT(*) INTO total_reqs 
        FROM update_requests 
        WHERE DATE(request_time) = stat_date;
        
        SELECT COUNT(DISTINCT client_ip) INTO unique_clients 
        FROM update_requests 
        WHERE DATE(request_time) = stat_date;
        
        SELECT COUNT(*) INTO successful_dls 
        FROM update_downloads 
        WHERE DATE(download_time) = stat_date AND success = TRUE;
        
        SELECT COUNT(*) INTO failed_dls 
        FROM update_downloads 
        WHERE DATE(download_time) = stat_date AND success = FALSE;
        
        -- إدراج أو تحديث الإحصائيات
        INSERT INTO update_statistics (date, total_requests, unique_clients, successful_downloads, failed_downloads)
        VALUES (stat_date, total_reqs, unique_clients, successful_dls, failed_dls)
        ON DUPLICATE KEY UPDATE
            total_requests = total_reqs,
            unique_clients = unique_clients,
            successful_downloads = successful_dls,
            failed_downloads = failed_dls;
    END LOOP;
    
    CLOSE date_cursor;
END //
DELIMITER ;

-- إنشاء event لتحديث الإحصائيات يومياً
CREATE EVENT IF NOT EXISTS daily_stats_update
ON SCHEDULE EVERY 1 DAY
STARTS '2024-01-01 00:00:00'
DO
  CALL UpdateDailyStats();

-- تفعيل event scheduler
SET GLOBAL event_scheduler = ON;
