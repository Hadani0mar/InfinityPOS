#!/bin/bash

# InfinityPOS Check Updates Script
echo "=== Checking for Updates ==="

# الانتقال إلى مجلد المشروع
cd /opt/infinitypos

# جلب آخر المعلومات من GitHub
echo "Fetching latest information from GitHub..."
git fetch origin

# فحص التحديثات
LOCAL_COMMIT=$(git rev-parse HEAD)
REMOTE_COMMIT=$(git rev-parse origin/master)

if [ "$LOCAL_COMMIT" != "$REMOTE_COMMIT" ]; then
    echo "✅ New updates available!"
    echo "Local commit:  $LOCAL_COMMIT"
    echo "Remote commit: $REMOTE_COMMIT"
    
    # عرض آخر التغييرات
    echo ""
    echo "Latest changes:"
    git log --oneline $LOCAL_COMMIT..$REMOTE_COMMIT
else
    echo "✅ No new updates available."
    echo "You are up to date!"
fi

echo "=== Check Complete ==="
