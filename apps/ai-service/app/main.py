"""Optional AI service bootstrap for Accreditation and Reporting."""

from datetime import datetime, timezone

from fastapi import FastAPI

from app.module_registry import MODULE_IDS


app = FastAPI(
    title="OBE & AUN-QA AI Service",
    version="0.1.0",
    description="Optional AI capabilities for Accreditation and Reporting.",
)


@app.get("/health")
def health() -> dict[str, str]:
    return {
        "status": "healthy",
        "checkedAtUtc": datetime.now(timezone.utc).isoformat(),
    }


@app.get("/capabilities")
def capabilities() -> dict[str, object]:
    return {
        "status": "placeholder",
        "modules": list(MODULE_IDS),
        "capabilities": {
            "accreditation": [
                "document-ingestion",
                "evidence-classification",
                "evidence-retrieval",
                "gap-analysis",
            ],
            "reporting": [
                "grounded-chat",
                "citation",
                "report-generation",
            ],
        },
    }
