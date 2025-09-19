<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');

// الانتقال إلى مجلد المشروع
chdir('/opt/infinitypos');

// جلب آخر المعلومات من GitHub
exec('git fetch origin 2>&1', $fetch_output, $fetch_status);

// الحصول على معلومات الـ commits
exec('git rev-parse HEAD', $local_commit);
exec('git rev-parse origin/master', $remote_commit);

// الحصول على آخر commit message
exec('git log -1 --format="%s" origin/master', $last_message);
exec('git log -1 --format="%H" origin/master', $last_hash);
exec('git log -1 --format="%ci" origin/master', $last_date);

// فحص التحديثات
$has_updates = $local_commit[0] !== $remote_commit[0];

$response = [
    'has_updates' => $has_updates,
    'local_commit' => trim($local_commit[0]),
    'remote_commit' => trim($remote_commit[0]),
    'last_message' => trim($last_message[0]),
    'last_hash' => trim($last_hash[0]),
    'last_date' => trim($last_date[0]),
    'timestamp' => date('Y-m-d H:i:s')
];

echo json_encode($response, JSON_PRETTY_PRINT);
?>
