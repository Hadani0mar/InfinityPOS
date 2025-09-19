<?php
/**
 * SmartInventory Pro - Advanced Update Server
 * نظام تحديثات متقدم للتطبيق
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type');

// إعدادات قاعدة البيانات
$db_host = 'localhost';
$db_name = 'infinitypos';
$db_user = 'root';
$db_pass = '';

// إعدادات GitHub
$github_repo = 'yourusername/SmartInventoryPro';
$github_token = 'your_github_token';

// إعدادات التطبيق
$app_name = 'SmartInventory Pro';
$current_version = '1.3.0';

/**
 * فحص التحديثات من GitHub
 */
function checkGitHubUpdates($repo, $token) {
    $url = "https://api.github.com/repos/$repo/releases/latest";
    
    $headers = [
        'User-Agent: SmartInventoryPro-Updater',
        'Accept: application/vnd.github.v3+json'
    ];
    
    if ($token) {
        $headers[] = "Authorization: token $token";
    }
    
    $context = stream_context_create([
        'http' => [
            'method' => 'GET',
            'header' => implode("\r\n", $headers),
            'timeout' => 30
        ]
    ]);
    
    $response = @file_get_contents($url, false, $context);
    
    if ($response === false) {
        return null;
    }
    
    return json_decode($response, true);
}

/**
 * فحص التحديثات من قاعدة البيانات المحلية
 */
function checkLocalUpdates($pdo) {
    try {
        $stmt = $pdo->prepare("
            SELECT version, release_date, release_notes, download_url, file_size
            FROM app_updates 
            WHERE is_active = 1 
            ORDER BY release_date DESC 
            LIMIT 1
        ");
        
        $stmt->execute();
        $update = $stmt->fetch(PDO::FETCH_ASSOC);
        
        return $update;
    } catch (PDOException $e) {
        return null;
    }
}

/**
 * تسجيل طلب التحديث
 */
function logUpdateRequest($pdo, $client_version, $client_ip) {
    try {
        $stmt = $pdo->prepare("
            INSERT INTO update_requests (client_version, client_ip, request_time)
            VALUES (?, ?, NOW())
        ");
        
        $stmt->execute([$client_version, $client_ip]);
    } catch (PDOException $e) {
        // تجاهل أخطاء السجل
    }
}

/**
 * الحصول على إحصائيات التحديثات
 */
function getUpdateStats($pdo) {
    try {
        $stmt = $pdo->prepare("
            SELECT 
                COUNT(*) as total_requests,
                COUNT(DISTINCT client_ip) as unique_clients,
                MAX(request_time) as last_request
            FROM update_requests 
            WHERE request_time >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
        ");
        
        $stmt->execute();
        return $stmt->fetch(PDO::FETCH_ASSOC);
    } catch (PDOException $e) {
        return null;
    }
}

// معالجة الطلبات
$action = $_GET['action'] ?? 'check';

try {
    // الاتصال بقاعدة البيانات
    $pdo = new PDO("mysql:host=$db_host;dbname=$db_name;charset=utf8", $db_user, $db_pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    switch ($action) {
        case 'check':
            // فحص التحديثات
            $client_version = $_GET['version'] ?? $current_version;
            $client_ip = $_SERVER['REMOTE_ADDR'] ?? 'unknown';
            
            // تسجيل الطلب
            logUpdateRequest($pdo, $client_version, $client_ip);
            
            // فحص التحديثات من GitHub أولاً
            $github_update = checkGitHubUpdates($github_repo, $github_token);
            
            if ($github_update) {
                $latest_version = str_replace('v', '', $github_update['tag_name']);
                $has_update = version_compare($latest_version, $client_version, '>');
                
                $response = [
                    'has_updates' => $has_update,
                    'current_version' => $client_version,
                    'latest_version' => $latest_version,
                    'release_notes' => $github_update['body'] ?? '',
                    'release_date' => $github_update['published_at'] ?? '',
                    'download_url' => $github_update['assets'][0]['browser_download_url'] ?? '',
                    'file_size' => $github_update['assets'][0]['size'] ?? 0,
                    'source' => 'github'
                ];
            } else {
                // فحص التحديثات المحلية
                $local_update = checkLocalUpdates($pdo);
                
                if ($local_update) {
                    $has_update = version_compare($local_update['version'], $client_version, '>');
                    
                    $response = [
                        'has_updates' => $has_update,
                        'current_version' => $client_version,
                        'latest_version' => $local_update['version'],
                        'release_notes' => $local_update['release_notes'] ?? '',
                        'release_date' => $local_update['release_date'] ?? '',
                        'download_url' => $local_update['download_url'] ?? '',
                        'file_size' => $local_update['file_size'] ?? 0,
                        'source' => 'local'
                    ];
                } else {
                    $response = [
                        'has_updates' => false,
                        'current_version' => $client_version,
                        'latest_version' => $client_version,
                        'message' => 'لا توجد تحديثات متاحة'
                    ];
                }
            }
            
            break;
            
        case 'stats':
            // إحصائيات التحديثات
            $stats = getUpdateStats($pdo);
            
            $response = [
                'success' => true,
                'stats' => $stats ?: [
                    'total_requests' => 0,
                    'unique_clients' => 0,
                    'last_request' => null
                ]
            ];
            
            break;
            
        case 'download':
            // تحميل التحديث
            $version = $_GET['version'] ?? '';
            $client_ip = $_SERVER['REMOTE_ADDR'] ?? 'unknown';
            
            if (empty($version)) {
                throw new Exception('إصدار غير محدد');
            }
            
            // تسجيل التحميل
            $stmt = $pdo->prepare("
                INSERT INTO update_downloads (version, client_ip, download_time)
                VALUES (?, ?, NOW())
            ");
            $stmt->execute([$version, $client_ip]);
            
            $response = [
                'success' => true,
                'message' => 'تم تسجيل التحميل بنجاح'
            ];
            
            break;
            
        default:
            throw new Exception('إجراء غير صحيح');
    }
    
    echo json_encode($response, JSON_UNESCAPED_UNICODE);
    
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => $e->getMessage()
    ], JSON_UNESCAPED_UNICODE);
}

// إنشاء جداول قاعدة البيانات إذا لم تكن موجودة
function createTables($pdo) {
    $sql = "
    CREATE TABLE IF NOT EXISTS app_updates (
        id INT AUTO_INCREMENT PRIMARY KEY,
        version VARCHAR(20) NOT NULL,
        release_date DATETIME NOT NULL,
        release_notes TEXT,
        download_url VARCHAR(500),
        file_size BIGINT,
        is_active BOOLEAN DEFAULT TRUE,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    
    CREATE TABLE IF NOT EXISTS update_requests (
        id INT AUTO_INCREMENT PRIMARY KEY,
        client_version VARCHAR(20),
        client_ip VARCHAR(45),
        request_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    
    CREATE TABLE IF NOT EXISTS update_downloads (
        id INT AUTO_INCREMENT PRIMARY KEY,
        version VARCHAR(20),
        client_ip VARCHAR(45),
        download_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    ";
    
    $pdo->exec($sql);
}
?>
