"""Service host. SV5 owns the AI implementation under modules/sv5/knowledge/ai."""
from pathlib import Path
import sys

# Native execution and Docker both preserve the repository-relative module layout.
for root in Path(__file__).resolve().parents:
    if (root / "modules/sv5/knowledge/ai").is_dir():
        if str(root) not in sys.path:
            sys.path.insert(0, str(root))
        break
