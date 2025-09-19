<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');

// الانتقال إلى مجلد المشروع
chdir('/opt/infinitypos');

// حفظ التغييرات الحالية
exec('git stash 2>&1', $stash_output, $stash_status);

// سحب آخر التحديثات
exec('git pull origin master 2>&1', $pull_output, $pull_status);

// فحص التحديثات
exec('git log -1 --format="%H"', $new_commit);
exec('git log -1 --format="%s"', $new_message);
exec('git log -1 --format="%ci"', $new_date);

$response = [
    'success' => $pull_status === 0,
    'stash_output' => $stash_output,
    'pull_output' => $pull_output,
    'new_commit' => trim($new_commit[0]),
    'new_message' => trim($new_message[0]),
    'new_date' => trim($new_date[0]),
    'timestamp' => date('Y-m-d H:i:s')
];

echo json_encode($response, JSON_PRETTY_PRINT);
?>
