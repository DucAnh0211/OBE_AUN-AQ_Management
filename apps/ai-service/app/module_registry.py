"""AI registry for the SV4 and SV5 modules; Curriculum intentionally has no AI."""

MODULES = {
    "accreditation": "modules.sv4.accreditation.ai",
    "reporting": "modules.sv5.reporting.ai",
}

MODULE_IDS = tuple(MODULES)
