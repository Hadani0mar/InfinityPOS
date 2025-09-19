#!/bin/bash

# InfinityPOS Auto Update Script
echo "=== InfinityPOS Update Script ==="

# الانتقال إلى مجلد المشروع
cd /opt/infinitypos

# حفظ التغييرات الحالية
echo "Saving current changes..."
git stash

# سحب آخر التحديثات
echo "Pulling latest updates..."
git pull origin master

# فحص التحديثات
echo "Checking for updates..."
LATEST_COMMIT=$(git log -1 --format="%H")
CURRENT_COMMIT=$(git log -1 --format="%H")

if [ "$LATEST_COMMIT" != "$CURRENT_COMMIT" ]; then
    echo "New updates found!"
    echo "Latest commit: $LATEST_COMMIT"
    echo "Update completed successfully!"
else
    echo "No new updates available."
fi

echo "=== Update Script Finished ==="
