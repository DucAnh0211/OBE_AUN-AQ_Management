-- Generated seed SQL: replace __...__ tokens using scripts/generate_plo_clo_seed.py.
-- This template is not executable by itself. The generated file is UTF-8.
BEGIN;
SET client_encoding = 'UTF8';

DO $import$
DECLARE
    v_data jsonb := $obe_json_0${
  "metadata": {
    "schema_version": "1.0",
    "created_date": "2026-09-15",
    "dataset_status": "finalized_at_user_request",
    "source_workbook": "1. BM - Doi Sanh Curriculum Map and Blackbox - CTĐT V1 hiện hành.xlsx",
    "source_sheet": "2. Curriculum Map (24042025)",
    "source_course_syllabus": "1. Đề cương HP-TTTN-V2.docx",
    "scope_note": "Chỉ CLO Thực tập tốt nghiệp có trong đề cương được cung cấp; các CLO khác suy luận từ tên môn và PLO đã chọn. Không suy diễn rằng liên kết/CLO sinh mới đã được cơ sở đào tạo ban hành.",
    "mapping_rule": "Môn trống PLO: tối thiểu min(số tín chỉ, 5); môn đã có PLO giữ nguyên dù dưới ngưỡng.",
    "link_codes": {
      "X": 2,
      "Y": 1,
      "I": 1,
      "R": 2,
      "E": 3
    },
    "program_version": "V1",
    "program_version_role": "baseline_for_future_comparison"
  },
  "program_plos_original": [
    {
      "id": "PLO1",
      "text": "PLO1. Phân tích một cách có hệ thống các vấn đề trong lĩnh vực Khoa học Máy tính bằng cách sử dụng thành thạo các kiến thức toán học, khoa học tự nhiên và chuyên ngành.",
      "level": "C4P3",
      "source_cell": "J1"
    },
    {
      "id": "PLO2",
      "text": "PLO2. Thiết kế các giải pháp kỹ thuật trong lĩnh vực khoa học máy tính nhằm giải quyết các bài toán thực tế cụ thể, bảo đảm đáp ứng yêu cầu đề bài và cân nhắc đến các yếu tố về tính khả thi, hiệu quả, đạo đức nghề nghiệp và bối cảnh kỹ thuật – xã hội.",
      "level": "C6 P3",
      "source_cell": "L1"
    },
    {
      "id": "PLO3",
      "text": "PLO3. Áp dụng hiệu quả các kiến thức về trí tuệ nhân tạo để giải quyết các vấn đề trong lĩnh vực Khoa học máy tính, nhằm nâng cao tính thông minh của hệ thống.",
      "level": "C3P3",
      "source_cell": "N1"
    },
    {
      "id": "PLO4",
      "text": "PLO4. Giao tiếp hiệu quả bằng văn nói và văn viết trong các tình huống chuyên môn khoa học máy tính thông qua sử dụng công cụ giao tiếp phù hợp.",
      "level": "P3",
      "source_cell": "P1"
    },
    {
      "id": "PLO5",
      "text": "PLO5. Làm việc hiệu quả trong nhóm, có khả năng dẫn dắt, lập kế hoạch và đạt được mục tiêu chung phù hợp với lĩnh vực khoa học máy tính",
      "level": "P3A3",
      "source_cell": "R1"
    }
  ],
  "curriculum_map_source_data": [
    {
      "source_row": 6,
      "name": "Đại số tuyến tính kỹ thuật",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J6",
            "progression": "K6"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "source_cells": {
            "weight": "P6",
            "progression": "Q6"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 7,
      "name": "Nhập môn giải tích kỹ thuật",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J7",
            "progression": "K7"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "source_cells": {
            "weight": "P7",
            "progression": "Q7"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 8,
      "name": "Giải tích ứng dụng kỹ thuật",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J8",
            "progression": "K8"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "source_cells": {
            "weight": "P8",
            "progression": "Q8"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 9,
      "name": "Vật lý kỹ thuật 1",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 10,
      "name": "Thí nghiệm vật lý kỹ thuật 1",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 1,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 11,
      "name": "Vật lý kỹ thuật 2",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 12,
      "name": "Thí nghiệm vật lý kỹ thuật 2",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 1,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 13,
      "name": "Toán rời rạc",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J13",
            "progression": "K13"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "P13",
            "progression": "Q13"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 14,
      "name": "Xác suất thống kê kỹ thuật",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 2,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J14",
            "progression": "K14"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "P14",
            "progression": "Q14"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 15,
      "name": "Đại số hiện đại ứng dụng",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 2,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "J15",
            "progression": "K15"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "P15",
            "progression": "Q15"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 16,
      "name": "Kinh tế kỹ thuật",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 17,
      "name": "Triết học Mác _ Lênin",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 18,
      "name": "Kinh tế chính trị Mác - Lênin",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 19,
      "name": "Tư tưởng Hồ Chí Minh",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 6",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 20,
      "name": "Chủ nghĩa xã hội khoa học",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 21,
      "name": "Lịch sử Đảng cộng sản Việt Nam",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 22,
      "name": "Pháp luật đại cương",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 23,
      "name": "Khoa học quản lý và quản trị",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 8",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 24,
      "name": "Tiếng Anh cơ bản 1",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 25,
      "name": "Tiếng Anh cơ bản 2",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 26,
      "name": "Tiếng Anh 1",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 27,
      "name": "Tiếng Anh 2",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 28,
      "name": "Nhập ngành Khoa học máy tính",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 1,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 29,
      "name": "Tham quan thực tập",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 1,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 30,
      "name": "Nhập môn lập trình",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ  1",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J30",
            "progression": "K30"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "N30",
            "progression": "O30"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "P30",
            "progression": "Q30"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 31,
      "name": "Lập trình dành cho kỹ thuật",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 32,
      "name": "Lập trình nâng cao",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J32",
            "progression": "K32"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "N32",
            "progression": "O32"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "P32",
            "progression": "Q32"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 33,
      "name": "Lập trình C++",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 2",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 34,
      "name": "Các mô hình tính toán",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 2,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "J34",
            "progression": "K34"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "P34",
            "progression": "Q34"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 35,
      "name": "Kiến trúc máy tính",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 36,
      "name": "Cấu trúc dữ liệu và thuật toán",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "J36",
            "progression": "K36"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "N36",
            "progression": "O36"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "source_cells": {
            "weight": "P36",
            "progression": "Q36"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 37,
      "name": "Lập trình Web",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 3",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 38,
      "name": "Nhập môn Trí tuệ nhân tạo",
      "course_code_cell_B": "tốt",
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "N38",
            "progression": "O38"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "P38",
            "progression": "Q38"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "R38",
            "progression": "S38"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 39,
      "name": "Hệ cơ sở dữ liệu",
      "course_code_cell_B": "v2",
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "J39",
            "progression": "K39"
          }
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "L39",
            "progression": "M39"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "P39",
            "progression": "Q39"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 40,
      "name": "Đồ án hệ cơ sở dữ liệu",
      "course_code_cell_B": "v2",
      "semester_cell_D": "Kỳ 4",
      "credits_cell_F": 1,
      "original_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "L40",
            "progression": "M40"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "source_cells": {
            "weight": "R40",
            "progression": "S40"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 41,
      "name": "Công nghệ phần mềm",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "J41",
            "progression": "K41"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "P41",
            "progression": "Q41"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "R41",
            "progression": "S41"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 42,
      "name": "Đồ án Công nghệ phần mềm",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 1,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "J42",
            "progression": "K42"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "source_cells": {
            "weight": "R42",
            "progression": "S42"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 43,
      "name": "Công nghệ chuyển đổi số",
      "course_code_cell_B": null,
      "semester_cell_D": null,
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "semester_missing",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 44,
      "name": "Nguyên lý ngôn ngữ lập trình",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 45,
      "name": "Học máy",
      "course_code_cell_B": "v2",
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "L45",
            "progression": "M45"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "N45",
            "progression": "O45"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "P45",
            "progression": "Q45"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 46,
      "name": "Thị giác máy tính",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 6",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "L46",
            "progression": "M46"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "N46",
            "progression": "O46"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "R46",
            "progression": "S46"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 47,
      "name": "Đồ án thị giác máy tính",
      "course_code_cell_B": "tốt",
      "semester_cell_D": "Kỳ 6",
      "credits_cell_F": 1,
      "original_links": [
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "source_cells": {
            "weight": "N47",
            "progression": "O47"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "source_cells": {
            "weight": "R47",
            "progression": "S47"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 48,
      "name": "Nhập môn dữ liệu lớn",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 5",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 49,
      "name": "Khai phá dữ liệu",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 6",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 50,
      "name": "Xử lý ngôn ngữ tự nhiên",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 6",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 51,
      "name": "Phát triển ứng dụng đa nền tảng",
      "course_code_cell_B": "v1",
      "semester_cell_D": "Kỳ 7",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "J51",
            "progression": "K51"
          }
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "L51",
            "progression": "M51"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "R51",
            "progression": "S51"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 52,
      "name": "Đồ án phát triển ứng dụng đa nền tảng",
      "course_code_cell_B": "v2",
      "semester_cell_D": "Kỳ 7",
      "credits_cell_F": 1,
      "original_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "L52",
            "progression": "M52"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "E",
          "source_cells": {
            "weight": "R52",
            "progression": "S52"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 53,
      "name": "Phát triển hệ thống phía server",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 6",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 54,
      "name": "Lập trình Linux",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 7",
      "credits_cell_F": 2,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "J54",
            "progression": "K54"
          }
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "L54",
            "progression": "M54"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 55,
      "name": "An toàn bảo mật thông tin",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 7",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 56,
      "name": "An ninh mạng",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 8",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 57,
      "name": "Thiết kế gaio diện và tương tác người máy",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 8",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 58,
      "name": "Quản lý dự án Công nghệ thông tin",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 7",
      "credits_cell_F": 2,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 59,
      "name": "Phát triển hệ thống phía server nâng cao",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 9",
      "credits_cell_F": 3,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 60,
      "name": "Đồ án phát triển hệ thống phía server nâng cao",
      "course_code_cell_B": null,
      "semester_cell_D": "Kỳ 9",
      "credits_cell_F": 1,
      "original_links": [],
      "source_quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ]
    },
    {
      "source_row": 61,
      "name": "Học máy nâng cao",
      "course_code_cell_B": "v1",
      "semester_cell_D": "Kỳ 8",
      "credits_cell_F": 3,
      "original_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "L61",
            "progression": "M61"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "N61",
            "progression": "O61"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "P61",
            "progression": "Q61"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 62,
      "name": "Đồ án học máy nâng cao",
      "course_code_cell_B": "v1",
      "semester_cell_D": "Kỳ 8",
      "credits_cell_F": 1,
      "original_links": [
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "N62",
            "progression": "O62"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "E",
          "source_cells": {
            "weight": "R62",
            "progression": "S62"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 63,
      "name": "Thực tập tốt nghiệp",
      "course_code_cell_B": "v1",
      "semester_cell_D": "Kỳ 9",
      "credits_cell_F": 11,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "J63",
            "progression": "K63"
          }
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "E",
          "source_cells": {
            "weight": "L63",
            "progression": "M63"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "Y",
          "progression_code": "E",
          "source_cells": {
            "weight": "N63",
            "progression": "O63"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "P63",
            "progression": "Q63"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "R63",
            "progression": "S63"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    },
    {
      "source_row": 64,
      "name": "Đồ án tốt nghiệp",
      "course_code_cell_B": "v1",
      "semester_cell_D": "Kỳ 9",
      "credits_cell_F": 14,
      "original_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "J64",
            "progression": "K64"
          }
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "L64",
            "progression": "M64"
          }
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "N64",
            "progression": "O64"
          }
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "P64",
            "progression": "Q64"
          }
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "source_cells": {
            "weight": "R64",
            "progression": "S64"
          }
        }
      ],
      "source_quality_flags": [
        "course_code_missing_or_placeholder"
      ]
    }
  ],
  "courses_finalized": [
    {
      "course_id": "MAP_ROW_06",
      "name": "Đại số tuyến tính kỹ thuật",
      "source_row": 6,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Vận dụng phép toán ma trận và hệ phương trình để phân tích một bài toán tính toán kỹ thuật.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 6,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản các bước giải và diễn giải kết quả của một bài toán đại số tuyến tính.",
          "bloom_level": "P2",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 6,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "original_mapping_below_credit_rule_preserved"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_07",
      "name": "Nhập môn giải tích kỹ thuật",
      "source_row": 7,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích đạo hàm và tích phân và xác định các yêu cầu chính của bài toán biến thiên kỹ thuật.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 7,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình giải tích và giải thích lựa chọn trong bài toán biến thiên kỹ thuật.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 7,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "original_mapping_below_credit_rule_preserved"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_08",
      "name": "Giải tích ứng dụng kỹ thuật",
      "source_row": 8,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích phương pháp giải tích ứng dụng và xác định các yêu cầu chính của bài toán tối ưu kỹ thuật.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 8,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói lời giải bằng giải tích và giải thích lựa chọn trong bài toán tối ưu kỹ thuật.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 8,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "original_mapping_below_credit_rule_preserved"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_09",
      "name": "Vật lý kỹ thuật 1",
      "source_row": 9,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Nội dung thiết kế giải pháp công nghệ cần được xác nhận bằng đề cương môn."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần hình thức báo cáo/thuyết trình để đo năng lực giao tiếp."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích một hiện tượng vật lý kỹ thuật bằng các định luật và mô hình phù hợp.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 9,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Đề xuất một mô hình hoặc phương án kỹ thuật đơn giản dựa trên kết quả phân tích vật lý.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 9,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận nội dung môn có bài toán thiết kế."
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản lời giải và ý nghĩa kỹ thuật của kết quả vật lý.",
          "bloom_level": "P2",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 9,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận hình thức báo cáo/đánh giá."
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_10",
      "name": "Thí nghiệm vật lý kỹ thuật 1",
      "source_row": 10,
      "course_code_source_value": null,
      "credits": 1,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 1,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thu thập, xử lý số liệu thí nghiệm và phân tích sai số để giải thích hiện tượng vật lý.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 10,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_11",
      "name": "Vật lý kỹ thuật 2",
      "source_row": 11,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Nội dung thiết kế giải pháp công nghệ cần được xác nhận bằng đề cương môn."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích hiện tượng vật lý kỹ thuật nâng cao và xác định các yêu cầu chính của bài toán vật lý kỹ thuật.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 11,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế mô hình vật lý đáp ứng yêu cầu và ràng buộc của bài toán vật lý kỹ thuật.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 11,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_12",
      "name": "Thí nghiệm vật lý kỹ thuật 2",
      "source_row": 12,
      "course_code_source_value": null,
      "credits": 1,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 1,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thu thập, xử lý và phân tích số liệu thí nghiệm vật lý kỹ thuật 2 để giải thích kết quả của bài toán đo lường vật lý.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 12,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_13",
      "name": "Toán rời rạc",
      "source_row": 13,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích quan hệ, đồ thị và logic và xác định các yêu cầu chính của bài toán mô hình hóa rời rạc.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 13,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói lời giải toán rời rạc và giải thích lựa chọn trong bài toán mô hình hóa rời rạc.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 13,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "original_mapping_below_credit_rule_preserved"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_14",
      "name": "Xác suất thống kê kỹ thuật",
      "source_row": 14,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích phân phối xác suất và thống kê và xác định các yêu cầu chính của bài toán dữ liệu kỹ thuật.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 14,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói kết quả phân tích thống kê và giải thích lựa chọn trong bài toán dữ liệu kỹ thuật.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 14,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_15",
      "name": "Đại số hiện đại ứng dụng",
      "source_row": 15,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích cấu trúc đại số hiện đại và xác định các yêu cầu chính của bài toán ứng dụng đại số.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 15,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình đại số và giải thích lựa chọn trong bài toán ứng dụng đại số.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 15,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_16",
      "name": "Kinh tế kỹ thuật",
      "source_row": 16,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Nội dung thiết kế giải pháp công nghệ cần được xác nhận bằng đề cương môn."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần học phần có bài tập nhóm để đo kết quả làm việc nhóm."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Đề xuất và đánh giá phương án kinh tế kỹ thuật theo các tiêu chí khả thi, hiệu quả và trách nhiệm nghề nghiệp.",
          "bloom_level": "C5",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 16,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành phương án kinh tế kỹ thuật cho bài toán lựa chọn phương án công nghệ.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 16,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_17",
      "name": "Triết học Mác _ Lênin",
      "source_row": 17,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "PLO chuyên ngành chỉ liên hệ gián tiếp với nội dung môn đại cương."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích có hệ thống luận điểm triết học Mác - Lênin và liên hệ với tình huống xã hội liên quan công nghệ.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 17,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày và bảo vệ một lập luận về luận điểm triết học Mác - Lênin có liên hệ với tình huống xã hội liên quan công nghệ.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 17,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công và phối hợp nhóm để chuẩn bị bài phân tích luận điểm có liên hệ với tình huống xã hội liên quan công nghệ.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 17,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO1",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_18",
      "name": "Kinh tế chính trị Mác - Lênin",
      "source_row": 18,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "PLO chuyên ngành chỉ liên hệ gián tiếp với nội dung môn đại cương."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích có hệ thống luận điểm kinh tế chính trị Mác - Lênin và liên hệ với tình huống kinh tế liên quan công nghệ.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 18,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày và bảo vệ một lập luận về luận điểm kinh tế chính trị Mác - Lênin có liên hệ với tình huống kinh tế liên quan công nghệ.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 18,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO1",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_19",
      "name": "Tư tưởng Hồ Chí Minh",
      "source_row": 19,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 6",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Trình bày và bảo vệ một lập luận về tư tưởng Hồ Chí Minh có liên hệ với tình huống về trách nhiệm nghề nghiệp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 19,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công và phối hợp nhóm để chuẩn bị bài trình bày quan điểm có liên hệ với tình huống về trách nhiệm nghề nghiệp.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 19,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_20",
      "name": "Chủ nghĩa xã hội khoa học",
      "source_row": 20,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "PLO chuyên ngành chỉ liên hệ gián tiếp với nội dung môn đại cương."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích có hệ thống luận điểm chủ nghĩa xã hội khoa học và liên hệ với tình huống xã hội liên quan công nghệ.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 20,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày và bảo vệ một lập luận về luận điểm chủ nghĩa xã hội khoa học có liên hệ với tình huống xã hội liên quan công nghệ.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 20,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO1",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_21",
      "name": "Lịch sử Đảng cộng sản Việt Nam",
      "source_row": 21,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Trình bày và bảo vệ một lập luận về sự kiện lịch sử Đảng Cộng sản Việt Nam có liên hệ với tình huống về trách nhiệm công dân.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 21,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công và phối hợp nhóm để chuẩn bị bài trình bày sự kiện có liên hệ với tình huống về trách nhiệm công dân.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 21,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_22",
      "name": "Pháp luật đại cương",
      "source_row": 22,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "PLO chuyên ngành chỉ liên hệ gián tiếp với nội dung môn đại cương."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích một tình huống kỹ thuật và lựa chọn giải pháp có xét đến yêu cầu pháp lý, đạo đức.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 22,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận có tình huống gắn với ngành Khoa học máy tính."
        },
        {
          "id": "CLO2",
          "text": "Lập luận bằng văn bản về lựa chọn và trách nhiệm pháp lý trong một tình huống nghề nghiệp.",
          "bloom_level": "P2",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 22,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận hình thức đánh giá."
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO2",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_23",
      "name": "Khoa học quản lý và quản trị",
      "source_row": 23,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 8",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Nội dung thiết kế giải pháp công nghệ cần được xác nhận bằng đề cương môn."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Đề xuất và đánh giá kế hoạch quản trị theo các tiêu chí khả thi, hiệu quả và trách nhiệm nghề nghiệp.",
          "bloom_level": "C5",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 23,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói kế hoạch quản trị và giải thích lựa chọn trong tình huống quản lý dự án công nghệ.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 23,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành kế hoạch quản trị cho tình huống quản lý dự án công nghệ.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 23,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_24",
      "name": "Tiếng Anh cơ bản 1",
      "source_row": 24,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Trao đổi bằng tiếng Anh về tình huống trao đổi thông tin công nghệ bằng từ vựng và cấu trúc phù hợp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 24,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phối hợp trong nhóm để chuẩn bị và thực hiện một hội thoại tiếng Anh về tình huống trao đổi thông tin công nghệ.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 24,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_25",
      "name": "Tiếng Anh cơ bản 2",
      "source_row": 25,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Trao đổi bằng tiếng Anh về tình huống trao đổi thông tin công nghệ bằng từ vựng và cấu trúc phù hợp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 25,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phối hợp trong nhóm để chuẩn bị và thực hiện một hội thoại tiếng Anh về tình huống trao đổi thông tin công nghệ.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 25,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_26",
      "name": "Tiếng Anh 1",
      "source_row": 26,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Trao đổi bằng tiếng Anh về tình huống trao đổi chuyên môn máy tính bằng từ vựng và cấu trúc phù hợp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 26,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phối hợp trong nhóm để chuẩn bị và thực hiện một hội thoại tiếng Anh về tình huống trao đổi chuyên môn máy tính.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 26,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_27",
      "name": "Tiếng Anh 2",
      "source_row": 27,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần bài đánh giá giao tiếp gắn với tình huống chuyên môn máy tính."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "thap",
          "fit_reason": "Chưa có đề cương xác nhận hoạt động nhóm gắn với lĩnh vực máy tính."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Trao đổi bằng tiếng Anh về tình huống trao đổi chuyên môn máy tính bằng từ vựng và cấu trúc phù hợp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 27,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phối hợp trong nhóm để chuẩn bị và thực hiện một hội thoại tiếng Anh về tình huống trao đổi chuyên môn máy tính.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 27,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO5",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_28",
      "name": "Nhập ngành Khoa học máy tính",
      "source_row": 28,
      "course_code_source_value": null,
      "credits": 1,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 1,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 1,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích khái niệm và hướng nghề nghiệp khoa học máy tính và xác định các yêu cầu chính của bài toán nhập ngành khoa học máy tính.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 28,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_29",
      "name": "Tham quan thực tập",
      "source_row": 29,
      "course_code_source_value": null,
      "credits": 1,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 1,
      "plo_links": [
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 1,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành kế hoạch và nhật ký tham quan cho nhiệm vụ khảo sát môi trường làm việc.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 29,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_30",
      "name": "Nhập môn lập trình",
      "source_row": 30,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ  1",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "thap",
          "fit_reason": "Liên kết PLO3 có trong nguồn nhưng tên môn chưa xác nhận nội dung AI."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích yêu cầu bài toán và xây dựng thuật toán giải bằng các cấu trúc lập trình cơ bản.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 30,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Cài đặt một thuật toán lập trình cơ bản có thể làm thành phần cho ứng dụng trí tuệ nhân tạo đơn giản.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 30,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": "Liên kết PLO3 có sẵn nhưng cần thầy kiểm tra nội dung học phần."
        },
        {
          "id": "CLO3",
          "text": "Giải thích bằng văn bản cấu trúc và kết quả chạy của chương trình đã xây dựng.",
          "bloom_level": "P2",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 30,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO3"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_31",
      "name": "Lập trình dành cho kỹ thuật",
      "source_row": 31,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích cấu trúc lập trình kỹ thuật và xác định các yêu cầu chính của bài toán tính toán kỹ thuật.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 31,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế chương trình tính toán kỹ thuật đáp ứng yêu cầu và ràng buộc của bài toán tính toán kỹ thuật.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 31,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói chương trình tính toán kỹ thuật và giải thích lựa chọn trong bài toán tính toán kỹ thuật.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 31,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_32",
      "name": "Lập trình nâng cao",
      "source_row": 32,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "thap",
          "fit_reason": "Liên kết PLO3 có trong nguồn nhưng tên môn chưa xác nhận nội dung AI."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích kỹ thuật lập trình nâng cao và xác định các yêu cầu chính của bài toán lập trình nâng cao.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 32,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Cài đặt thành phần xử lý kỹ thuật lập trình nâng cao để hỗ trợ một ứng dụng AI đơn giản cho bài toán lập trình nâng cao.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 32,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói chương trình có cấu trúc và giải thích lựa chọn trong bài toán lập trình nâng cao.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 32,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO3"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_33",
      "name": "Lập trình C++",
      "source_row": 33,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 2",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích cú pháp và thư viện C++ và xác định các yêu cầu chính của bài toán xử lý dữ liệu bằng C++.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 33,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế chương trình C++ đáp ứng yêu cầu và ràng buộc của bài toán xử lý dữ liệu bằng C++.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 33,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói chương trình C++ và giải thích lựa chọn trong bài toán xử lý dữ liệu bằng C++.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 33,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_34",
      "name": "Các mô hình tính toán",
      "source_row": 34,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích mô hình tính toán và xác định các yêu cầu chính của bài toán xác định khả năng tính toán.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 34,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình và lời giải và giải thích lựa chọn trong bài toán xác định khả năng tính toán.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 34,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_35",
      "name": "Kiến trúc máy tính",
      "source_row": 35,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần hình thức báo cáo/thuyết trình để đo năng lực giao tiếp."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích thành phần kiến trúc máy tính và xác định các yêu cầu chính của bài toán đánh giá hiệu năng hệ thống.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 35,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế mô hình tổ chức máy tính đáp ứng yêu cầu và ràng buộc của bài toán đánh giá hiệu năng hệ thống.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 35,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình tổ chức máy tính và giải thích lựa chọn trong bài toán đánh giá hiệu năng hệ thống.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 35,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_36",
      "name": "Cấu trúc dữ liệu và thuật toán",
      "source_row": 36,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "thap",
          "fit_reason": "Liên kết PLO3 có trong nguồn nhưng tên môn chưa xác nhận nội dung AI."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích cấu trúc dữ liệu và thuật toán và xác định các yêu cầu chính của bài toán xử lý dữ liệu.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 36,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Cài đặt thành phần xử lý cấu trúc dữ liệu và thuật toán để hỗ trợ một ứng dụng AI đơn giản cho bài toán xử lý dữ liệu.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 36,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói thuật toán và chương trình và giải thích lựa chọn trong bài toán xử lý dữ liệu.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 36,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "low_fit_PLO3"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_37",
      "name": "Lập trình Web",
      "source_row": 37,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 3",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích cấu trúc ứng dụng Web và xác định các yêu cầu chính của bài toán cung cấp dịch vụ trên Web.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 37,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế ứng dụng Web đáp ứng yêu cầu và ràng buộc của bài toán cung cấp dịch vụ trên Web.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 37,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói ứng dụng Web và giải thích lựa chọn trong bài toán cung cấp dịch vụ trên Web.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 37,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_38",
      "name": "Nhập môn Trí tuệ nhân tạo",
      "source_row": 38,
      "course_code_source_value": "tốt",
      "credits": 3,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "trung_binh",
          "fit_reason": "Cần học phần có bài tập nhóm để đo kết quả làm việc nhóm."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Áp dụng kỹ thuật AI phù hợp vào mô hình AI cơ bản và đánh giá kết quả trên bài toán ứng dụng AI.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 38,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình AI cơ bản và giải thích lựa chọn trong bài toán ứng dụng AI.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 38,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành mô hình AI cơ bản cho bài toán ứng dụng AI.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 38,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_39",
      "name": "Hệ cơ sở dữ liệu",
      "source_row": 39,
      "course_code_source_value": "v2",
      "credits": 3,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích yêu cầu dữ liệu và ràng buộc của một bài toán quản lý.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 39,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế lược đồ cơ sở dữ liệu đáp ứng yêu cầu và ràng buộc của bài toán.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 39,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày và bảo vệ lựa chọn thiết kế cơ sở dữ liệu bằng tài liệu kỹ thuật.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 39,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_40",
      "name": "Đồ án hệ cơ sở dữ liệu",
      "source_row": 40,
      "course_code_source_value": "v2",
      "credits": 1,
      "semester_source_value": "Kỳ 4",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 2,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 1,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Xây dựng và kiểm thử một giải pháp cơ sở dữ liệu cho bài toán được giao.",
          "bloom_level": "C5",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 40,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công công việc và phối hợp nhóm để hoàn thành đồ án cơ sở dữ liệu.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 40,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận đồ án tổ chức theo nhóm."
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_41",
      "name": "Công nghệ phần mềm",
      "source_row": 41,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "trung_binh",
          "fit_reason": "Cần học phần có bài tập nhóm để đo kết quả làm việc nhóm."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích quy trình công nghệ phần mềm và xác định các yêu cầu chính của bài toán phát triển phần mềm.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 41,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Trình bày bằng văn bản hoặc lời nói tài liệu và sản phẩm phần mềm và giải thích lựa chọn trong bài toán phát triển phần mềm.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 41,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành tài liệu và sản phẩm phần mềm cho bài toán phát triển phần mềm.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 41,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_42",
      "name": "Đồ án Công nghệ phần mềm",
      "source_row": 42,
      "course_code_source_value": null,
      "credits": 1,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 2,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 1,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích quy trình triển khai đồ án phần mềm và xác định các yêu cầu chính của bài toán phát triển phần mềm.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 42,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành sản phẩm đồ án phần mềm cho bài toán phát triển phần mềm.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 42,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_43",
      "name": "Công nghệ chuyển đổi số",
      "source_row": 43,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": null,
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Nội dung thiết kế giải pháp công nghệ cần được xác nhận bằng đề cương môn."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích quy trình chuyển đổi số và xác định các yêu cầu chính của bài toán số hóa hoạt động.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 43,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế phương án chuyển đổi số đáp ứng yêu cầu và ràng buộc của bài toán số hóa hoạt động.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 43,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành phương án chuyển đổi số cho bài toán số hóa hoạt động.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 43,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank",
        "semester_missing"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_44",
      "name": "Nguyên lý ngôn ngữ lập trình",
      "source_row": 44,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "I",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "I",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần hình thức báo cáo/thuyết trình để đo năng lực giao tiếp."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích cú pháp và ngữ nghĩa ngôn ngữ lập trình và xác định các yêu cầu chính của bài toán thiết kế hoặc lựa chọn ngôn ngữ.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 44,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế mô hình ngôn ngữ lập trình đáp ứng yêu cầu và ràng buộc của bài toán thiết kế hoặc lựa chọn ngôn ngữ.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 44,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình ngôn ngữ lập trình và giải thích lựa chọn trong bài toán thiết kế hoặc lựa chọn ngôn ngữ.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 44,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_45",
      "name": "Học máy",
      "source_row": 45,
      "course_code_source_value": "v2",
      "credits": 3,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thiết kế mô hình học máy đáp ứng yêu cầu và ràng buộc của bài toán dự đoán từ dữ liệu.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 45,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Áp dụng kỹ thuật AI phù hợp vào mô hình học máy và đánh giá kết quả trên bài toán dự đoán từ dữ liệu.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 45,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình học máy và giải thích lựa chọn trong bài toán dự đoán từ dữ liệu.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 45,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_46",
      "name": "Thị giác máy tính",
      "source_row": 46,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 6",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "trung_binh",
          "fit_reason": "Cần học phần có bài tập nhóm để đo kết quả làm việc nhóm."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thiết kế mô hình xử lý ảnh đáp ứng yêu cầu và ràng buộc của bài toán nhận dạng hình ảnh.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 46,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Áp dụng kỹ thuật AI phù hợp vào mô hình xử lý ảnh và đánh giá kết quả trên bài toán nhận dạng hình ảnh.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 46,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành mô hình xử lý ảnh cho bài toán nhận dạng hình ảnh.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 46,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_47",
      "name": "Đồ án thị giác máy tính",
      "source_row": 47,
      "course_code_source_value": "tốt",
      "credits": 1,
      "semester_source_value": "Kỳ 6",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 2,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 1,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Áp dụng kỹ thuật AI phù hợp vào sản phẩm đồ án thị giác máy tính và đánh giá kết quả trên bài toán nhận dạng hình ảnh.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 47,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành sản phẩm đồ án thị giác máy tính cho bài toán nhận dạng hình ảnh.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 47,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_48",
      "name": "Nhập môn dữ liệu lớn",
      "source_row": 48,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 5",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích đặc điểm và yêu cầu xử lý của một tập dữ liệu quy mô lớn.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 48,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế quy trình lưu trữ và xử lý dữ liệu lớn đáp ứng yêu cầu bài toán.",
          "bloom_level": "C5",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 48,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Giải thích cách dữ liệu lớn hỗ trợ một ứng dụng trí tuệ nhân tạo.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 48,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận nội dung môn có ứng dụng AI."
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_49",
      "name": "Khai phá dữ liệu",
      "source_row": 49,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 6",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích mẫu và quy luật trong dữ liệu và xác định các yêu cầu chính của bài toán tìm tri thức từ dữ liệu.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 49,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế mô hình khai phá dữ liệu đáp ứng yêu cầu và ràng buộc của bài toán tìm tri thức từ dữ liệu.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 49,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Áp dụng kỹ thuật AI phù hợp vào mô hình khai phá dữ liệu và đánh giá kết quả trên bài toán tìm tri thức từ dữ liệu.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 49,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_50",
      "name": "Xử lý ngôn ngữ tự nhiên",
      "source_row": 50,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 6",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích đặc trưng ngôn ngữ tự nhiên và xác định các yêu cầu chính của bài toán phân tích văn bản.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 50,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế mô hình xử lý văn bản đáp ứng yêu cầu và ràng buộc của bài toán phân tích văn bản.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 50,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Áp dụng kỹ thuật AI phù hợp vào mô hình xử lý văn bản và đánh giá kết quả trên bài toán phân tích văn bản.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 50,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_51",
      "name": "Phát triển ứng dụng đa nền tảng",
      "source_row": 51,
      "course_code_source_value": "v1",
      "credits": 3,
      "semester_source_value": "Kỳ 7",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "trung_binh",
          "fit_reason": "Cần học phần có bài tập nhóm để đo kết quả làm việc nhóm."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích yêu cầu ứng dụng đa nền tảng và xác định các yêu cầu chính của bài toán cung cấp dịch vụ trên nhiều thiết bị.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 51,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế ứng dụng đa nền tảng đáp ứng yêu cầu và ràng buộc của bài toán cung cấp dịch vụ trên nhiều thiết bị.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 51,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành ứng dụng đa nền tảng cho bài toán cung cấp dịch vụ trên nhiều thiết bị.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 51,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_52",
      "name": "Đồ án phát triển ứng dụng đa nền tảng",
      "source_row": 52,
      "course_code_source_value": "v2",
      "credits": 1,
      "semester_source_value": "Kỳ 7",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 2,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 1,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thiết kế sản phẩm đồ án đa nền tảng đáp ứng yêu cầu và ràng buộc của bài toán ứng dụng trên nhiều thiết bị.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 52,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành sản phẩm đồ án đa nền tảng cho bài toán ứng dụng trên nhiều thiết bị.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 52,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_53",
      "name": "Phát triển hệ thống phía server",
      "source_row": 53,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 6",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "R",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "R",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích kiến trúc hệ thống phía server và xác định các yêu cầu chính của bài toán cung cấp dịch vụ backend.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 53,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế dịch vụ phía server đáp ứng yêu cầu và ràng buộc của bài toán cung cấp dịch vụ backend.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 53,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành dịch vụ phía server cho bài toán cung cấp dịch vụ backend.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 53,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_54",
      "name": "Lập trình Linux",
      "source_row": 54,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 7",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 4,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích thành phần hệ điều hành Linux và xác định các yêu cầu chính của bài toán tự động hóa trên Linux.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 54,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế chương trình hoặc tập lệnh Linux đáp ứng yêu cầu và ràng buộc của bài toán tự động hóa trên Linux.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 54,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_55",
      "name": "An toàn bảo mật thông tin",
      "source_row": 55,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 7",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích nguy cơ và điểm yếu bảo mật trong một hệ thống thông tin.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 55,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế biện pháp bảo vệ dữ liệu và đánh giá tính khả thi của biện pháp đó.",
          "bloom_level": "C5",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 55,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_56",
      "name": "An ninh mạng",
      "source_row": 56,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 8",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần hình thức báo cáo/thuyết trình để đo năng lực giao tiếp."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích nguy cơ an ninh mạng và xác định các yêu cầu chính của bài toán phòng chống tấn công mạng.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 56,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế phương án bảo vệ mạng đáp ứng yêu cầu và ràng buộc của bài toán phòng chống tấn công mạng.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 56,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói phương án bảo vệ mạng và giải thích lựa chọn trong bài toán phòng chống tấn công mạng.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 56,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_57",
      "name": "Thiết kế gaio diện và tương tác người máy",
      "source_row": 57,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 8",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Nội dung thiết kế giải pháp công nghệ cần được xác nhận bằng đề cương môn."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "trung_binh",
          "fit_reason": "Cần hình thức báo cáo/thuyết trình để đo năng lực giao tiếp."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích nhu cầu và hành vi người dùng và xác định các yêu cầu chính của bài toán nâng cao khả dụng giao diện.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 57,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế giao diện và kịch bản tương tác đáp ứng yêu cầu và ràng buộc của bài toán nâng cao khả dụng giao diện.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 57,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói giao diện và kịch bản tương tác và giải thích lựa chọn trong bài toán nâng cao khả dụng giao diện.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 57,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_58",
      "name": "Quản lý dự án Công nghệ thông tin",
      "source_row": 58,
      "course_code_source_value": null,
      "credits": 2,
      "semester_source_value": "Kỳ 7",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 2,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 4,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Lập kế hoạch triển khai giải pháp công nghệ có xét tiến độ, nguồn lực và rủi ro.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 58,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công, theo dõi và điều phối công việc của nhóm trong một dự án công nghệ thông tin.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 58,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_59",
      "name": "Phát triển hệ thống phía server nâng cao",
      "source_row": 59,
      "course_code_source_value": null,
      "credits": 3,
      "semester_source_value": "Kỳ 9",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 3,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 3,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích kiến trúc server nâng cao và xác định các yêu cầu chính của bài toán cung cấp dịch vụ backend quy mô lớn.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 59,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế dịch vụ backend nâng cao đáp ứng yêu cầu và ràng buộc của bài toán cung cấp dịch vụ backend quy mô lớn.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 59,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành dịch vụ backend nâng cao cho bài toán cung cấp dịch vụ backend quy mô lớn.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 59,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_60",
      "name": "Đồ án phát triển hệ thống phía server nâng cao",
      "source_row": 60,
      "course_code_source_value": null,
      "credits": 1,
      "semester_source_value": "Kỳ 9",
      "mapping_origin": "completed_by_selection",
      "required_plo_count_if_new": 1,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 2,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 1,
          "provenance": "selected_from_existing_plos",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thiết kế sản phẩm đồ án backend đáp ứng yêu cầu và ràng buộc của bài toán triển khai dịch vụ backend.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 60,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành sản phẩm đồ án backend cho bài toán triển khai dịch vụ backend.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 60,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder",
        "plo_mapping_blank"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_61",
      "name": "Học máy nâng cao",
      "source_row": 61,
      "course_code_source_value": "v1",
      "credits": 3,
      "semester_source_value": "Kỳ 8",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 6,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Thiết kế mô hình học máy nâng cao đáp ứng yêu cầu và ràng buộc của bài toán dự đoán phức tạp.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 61,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Áp dụng kỹ thuật AI phù hợp vào mô hình học máy nâng cao và đánh giá kết quả trên bài toán dự đoán phức tạp.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 61,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Trình bày bằng văn bản hoặc lời nói mô hình học máy nâng cao và giải thích lựa chọn trong bài toán dự đoán phức tạp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 61,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_62",
      "name": "Đồ án học máy nâng cao",
      "source_row": 62,
      "course_code_source_value": "v1",
      "credits": 1,
      "semester_source_value": "Kỳ 8",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 2,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 1,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Áp dụng kỹ thuật AI phù hợp vào sản phẩm đồ án học máy và đánh giá kết quả trên bài toán dự đoán từ dữ liệu.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 62,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Phân công nhiệm vụ, phối hợp và theo dõi tiến độ nhóm để hoàn thành sản phẩm đồ án học máy cho bài toán dự đoán từ dữ liệu.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 62,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_63",
      "name": "Thực tập tốt nghiệp",
      "source_row": 63,
      "course_code_source_value": "v1",
      "credits": 11,
      "semester_source_value": "Kỳ 9",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 22,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 11,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "Y",
          "progression_code": "E",
          "credit_weighted_score": 11,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 22,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 22,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích nội dung, đặc điểm và phạm vi ứng dụng của giải pháp công nghệ đã tiếp cận trong giai đoạn thực tập",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "original_course_syllabus",
          "source_reference": {
            "file": "1. Đề cương HP-TTTN-V2.docx",
            "section": "3.1"
          },
          "dataset_status": "finalized"
        },
        {
          "id": "CLO2",
          "text": "Thiết kế được giải pháp kỹ thuật có tính khả thi và ứng dụng thực tế, giải quyết được trọn vẹn hoặc một phần của bài toán trong lĩnh vực Khoa học Máy tính",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "original_course_syllabus",
          "source_reference": {
            "file": "1. Đề cương HP-TTTN-V2.docx",
            "section": "3.1"
          },
          "dataset_status": "finalized"
        },
        {
          "id": "CLO3",
          "text": "Vận dụng kiến thức về trí tuệ nhân tạo để triển khai một hoặc một bài toán Khoa học Máy tính trong môi trường thực tế.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "original_course_syllabus",
          "source_reference": {
            "file": "1. Đề cương HP-TTTN-V2.docx",
            "section": "3.1"
          },
          "dataset_status": "finalized"
        },
        {
          "id": "CLO4",
          "text": "Giao tiếp hiệu quả bằng hình thức viết và nói trong môi trường công việc",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "original_course_syllabus",
          "source_reference": {
            "file": "1. Đề cương HP-TTTN-V2.docx",
            "section": "3.1"
          },
          "dataset_status": "finalized"
        },
        {
          "id": "CLO5",
          "text": "Làm việc nhóm hiệu quả trong môi trường thực tế, chuyên nghiệp.",
          "bloom_level": "P3/A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "original_course_syllabus",
          "source_reference": {
            "file": "1. Đề cương HP-TTTN-V2.docx",
            "section": "3.1"
          },
          "dataset_status": "finalized"
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    },
    {
      "course_id": "MAP_ROW_64",
      "name": "Đồ án tốt nghiệp",
      "source_row": 64,
      "course_code_source_value": "v1",
      "credits": 14,
      "semester_source_value": "Kỳ 9",
      "mapping_origin": "original_curriculum_map",
      "required_plo_count_if_new": null,
      "plo_links": [
        {
          "plo_id": "PLO1",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 28,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO2",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 28,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO3",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 28,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO4",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 28,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        },
        {
          "plo_id": "PLO5",
          "weight_code": "X",
          "progression_code": "E",
          "credit_weighted_score": 28,
          "provenance": "original_curriculum_map",
          "fit": "cao",
          "fit_reason": "Nội dung tên môn hoặc liên kết nguồn tương thích trực tiếp với PLO."
        }
      ],
      "clos": [
        {
          "id": "CLO1",
          "text": "Phân tích yêu cầu, bối cảnh và ràng buộc của đề tài tốt nghiệp.",
          "bloom_level": "C4",
          "supports_plo": [
            "PLO1"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 64,
            "plo_id": "PLO1"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO2",
          "text": "Thiết kế, hiện thực và đánh giá giải pháp kỹ thuật cho đề tài tốt nghiệp.",
          "bloom_level": "C6",
          "supports_plo": [
            "PLO2"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 64,
            "plo_id": "PLO2"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO3",
          "text": "Đánh giá khả năng áp dụng AI và triển khai thành phần AI phù hợp với đề tài.",
          "bloom_level": "C3",
          "supports_plo": [
            "PLO3"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 64,
            "plo_id": "PLO3"
          },
          "dataset_status": "finalized",
          "assumption_note": "Chỉ phù hợp nếu đề tài có nội dung AI; cần thầy duyệt."
        },
        {
          "id": "CLO4",
          "text": "Viết báo cáo kỹ thuật và bảo vệ kết quả đồ án tốt nghiệp.",
          "bloom_level": "P3",
          "supports_plo": [
            "PLO4"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 64,
            "plo_id": "PLO4"
          },
          "dataset_status": "finalized",
          "assumption_note": null
        },
        {
          "id": "CLO5",
          "text": "Phối hợp với nhóm và các bên liên quan để lập kế hoạch, hoàn thành mục tiêu đề tài.",
          "bloom_level": "P3A3",
          "supports_plo": [
            "PLO5"
          ],
          "provenance": "generated_from_course_name_and_plo",
          "source_reference": {
            "source_course_row": 64,
            "plo_id": "PLO5"
          },
          "dataset_status": "finalized",
          "assumption_note": "Cần xác nhận đồ án có hoạt động nhóm thực sự."
        }
      ],
      "quality_flags": [
        "course_code_missing_or_placeholder"
      ],
      "dataset_status": "finalized"
    }
  ],
  "plo_balance": {
    "formula": "sum(course_credits * (2 if X else 1 if Y else 0))",
    "original_scores": {
      "PLO1": 128,
      "PLO2": 77,
      "PLO3": 85,
      "PLO4": 125,
      "PLO5": 79
    },
    "final_scores": {
      "PLO1": 203,
      "PLO2": 157,
      "PLO3": 100,
      "PLO4": 181,
      "PLO5": 117
    },
    "final_mean": 151.6,
    "deviation_from_mean": {
      "PLO1": 0.3391,
      "PLO2": 0.0356,
      "PLO3": -0.3404,
      "PLO4": 0.1939,
      "PLO5": -0.2282
    },
    "alert_threshold": 0.2,
    "alerts": [
      "PLO1",
      "PLO3",
      "PLO5"
    ],
    "balance_status": "above_20_percent",
    "balance_note": "Đã ưu tiên nội dung môn và giữ liên kết gốc. Ngưỡng 20% chưa đạt khi chỉ có tên môn và một đề cương; không gắn PLO3 (AI) hoặc PLO5 (nhóm) vào môn không có căn cứ chỉ để giảm chênh lệch."
  },
  "validation_summary": {
    "source_course_count": 59,
    "originally_mapped_courses": 25,
    "newly_completed_courses": 34,
    "total_credits": 159,
    "source_clo_count": 5,
    "generated_clo_count": 141,
    "total_clo_count": 146,
    "original_courses_below_credit_rule": [
      6,
      7,
      8,
      13
    ],
    "low_fit_link_count": 14
  }
}
$obe_json_0$::jsonb;
    v_json_sha text := 'dbb46c8ede0ed1757b714a4ad083ab6b111d22e477250c18c11713fa60f05227';
    v_workbook_sha text := '7e890cba9c4fcb955c9022e0cb2263ab2e9f1e1a256767969f8cf10ef709aa00';
    v_syllabus_sha text := 'cd88180d734ecf60239f9a4aa26f345f8e1f349fe158cf922439fcb96d6b6f07';
    v_program_id bigint;
    v_version_id bigint;
    v_json_document_id bigint;
    v_workbook_document_id bigint;
    v_syllabus_document_id bigint;
    v_batch_id bigint;
    v_course_id bigint;
    v_program_course_id bigint;
    v_plo_id bigint;
    v_course_plo_id bigint;
    v_clo_id bigint;
    v_raw jsonb;
    v_plo jsonb;
    v_course jsonb;
    v_link jsonb;
    v_clo jsonb;
    v_support text;
    v_actual integer;
    v_expected integer;
BEGIN
    PERFORM pg_advisory_xact_lock(
        hashtext('curriculum_CS_' || (v_data->'metadata'->>'program_version') || '_import')
    );

    INSERT INTO curriculum.programs (code, name)
    VALUES ('CS', 'Khoa học máy tính')
    ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name
    RETURNING id INTO v_program_id;

    INSERT INTO curriculum.program_versions (program_id, version_code, dataset_status)
    VALUES (v_program_id, v_data->'metadata'->>'program_version', 'finalized')
    ON CONFLICT (program_id, version_code)
        DO UPDATE SET dataset_status = EXCLUDED.dataset_status
    RETURNING id INTO v_version_id;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES ('PLO_CLO_59_mon.json', 'json', v_json_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_json_document_id;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES (v_data->'metadata'->>'source_workbook', 'xlsx', v_workbook_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_workbook_document_id;

    INSERT INTO curriculum.source_documents (file_name, file_kind, sha256)
    VALUES (v_data->'metadata'->>'source_course_syllabus', 'docx', v_syllabus_sha)
    ON CONFLICT (file_kind, sha256) DO UPDATE SET file_name = EXCLUDED.file_name
    RETURNING id INTO v_syllabus_document_id;

    INSERT INTO curriculum.import_batches
        (program_version_id, dataset_document_id, workbook_document_id,
         syllabus_document_id, dataset_sha256)
    VALUES (v_version_id, v_json_document_id, v_workbook_document_id,
            v_syllabus_document_id, v_json_sha)
    ON CONFLICT (program_version_id, dataset_sha256)
        DO UPDATE SET imported_at = now(),
                      dataset_document_id = EXCLUDED.dataset_document_id,
                      workbook_document_id = EXCLUDED.workbook_document_id,
                      syllabus_document_id = EXCLUDED.syllabus_document_id
    RETURNING id INTO v_batch_id;

    FOR v_raw IN SELECT value FROM jsonb_array_elements(v_data->'curriculum_map_source_data')
    LOOP
        INSERT INTO curriculum.source_course_rows (import_batch_id, source_row, raw_data)
        VALUES (v_batch_id, (v_raw->>'source_row')::integer, v_raw)
        ON CONFLICT (import_batch_id, source_row)
            DO UPDATE SET raw_data = EXCLUDED.raw_data;
    END LOOP;

    FOR v_plo IN SELECT value FROM jsonb_array_elements(v_data->'program_plos_original')
    LOOP
        INSERT INTO curriculum.plos
            (program_version_id, code, statement, level_code, source_cell, provenance)
        VALUES (v_version_id, v_plo->>'id', v_plo->>'text', v_plo->>'level',
                v_plo->>'source_cell', 'original_curriculum_map')
        ON CONFLICT (program_version_id, code)
            DO UPDATE SET statement = EXCLUDED.statement,
                          level_code = EXCLUDED.level_code,
                          source_cell = EXCLUDED.source_cell
        RETURNING id INTO v_plo_id;
    END LOOP;

    FOR v_course IN SELECT value FROM jsonb_array_elements(v_data->'courses_finalized')
    LOOP
        v_program_course_id := NULL;
        v_course_id := NULL;
        SELECT id, course_id INTO v_program_course_id, v_course_id
        FROM curriculum.program_courses
        WHERE program_version_id = v_version_id
          AND source_row = (v_course->>'source_row')::integer;

        IF v_program_course_id IS NULL THEN
            INSERT INTO curriculum.courses (name)
            VALUES (v_course->>'name') RETURNING id INTO v_course_id;
            INSERT INTO curriculum.program_courses
                (program_version_id, course_id, source_row, course_code_source_value,
                 credits, semester_source_value, dataset_status, quality_flags)
            VALUES (v_version_id, v_course_id, (v_course->>'source_row')::integer,
                    v_course->>'course_code_source_value', (v_course->>'credits')::integer,
                    v_course->>'semester_source_value', v_course->>'dataset_status',
                    v_course->'quality_flags')
            RETURNING id INTO v_program_course_id;
        ELSE
            UPDATE curriculum.courses SET name = v_course->>'name' WHERE id = v_course_id;
            UPDATE curriculum.program_courses
            SET course_code_source_value = v_course->>'course_code_source_value',
                credits = (v_course->>'credits')::integer,
                semester_source_value = v_course->>'semester_source_value',
                dataset_status = v_course->>'dataset_status',
                quality_flags = v_course->'quality_flags'
            WHERE id = v_program_course_id;
        END IF;

        FOR v_link IN SELECT value FROM jsonb_array_elements(v_course->'plo_links')
        LOOP
            SELECT id INTO STRICT v_plo_id FROM curriculum.plos
            WHERE program_version_id = v_version_id AND code = v_link->>'plo_id';
            INSERT INTO curriculum.course_plos
                (program_version_id, program_course_id, plo_id, weight_code,
                 progression_code, provenance, fit, fit_reason)
            VALUES (v_version_id, v_program_course_id, v_plo_id,
                    v_link->>'weight_code', v_link->>'progression_code',
                    v_link->>'provenance', v_link->>'fit', v_link->>'fit_reason')
            ON CONFLICT (program_course_id, plo_id)
                DO UPDATE SET weight_code = EXCLUDED.weight_code,
                              progression_code = EXCLUDED.progression_code,
                              provenance = EXCLUDED.provenance,
                              fit = EXCLUDED.fit,
                              fit_reason = EXCLUDED.fit_reason
            RETURNING id INTO v_course_plo_id;
        END LOOP;

        FOR v_clo IN SELECT value FROM jsonb_array_elements(v_course->'clos')
        LOOP
            INSERT INTO curriculum.clos
                (program_course_id, code, statement, level_code, provenance,
                 source_reference, assumption_note, dataset_status)
            VALUES (v_program_course_id, v_clo->>'id', v_clo->>'text',
                    v_clo->>'bloom_level', v_clo->>'provenance',
                    v_clo->'source_reference', v_clo->>'assumption_note',
                    v_clo->>'dataset_status')
            ON CONFLICT (program_course_id, code)
                DO UPDATE SET statement = EXCLUDED.statement,
                              level_code = EXCLUDED.level_code,
                              provenance = EXCLUDED.provenance,
                              source_reference = EXCLUDED.source_reference,
                              assumption_note = EXCLUDED.assumption_note,
                              dataset_status = EXCLUDED.dataset_status
            RETURNING id INTO v_clo_id;

            FOR v_support IN SELECT value FROM jsonb_array_elements_text(v_clo->'supports_plo')
            LOOP
                SELECT cp.id INTO STRICT v_course_plo_id
                FROM curriculum.course_plos cp
                JOIN curriculum.plos p ON p.id = cp.plo_id
                WHERE cp.program_course_id = v_program_course_id
                  AND p.code = v_support;
                INSERT INTO curriculum.clo_plos (clo_id, program_course_id, course_plo_id)
                VALUES (v_clo_id, v_program_course_id, v_course_plo_id)
                ON CONFLICT (clo_id, course_plo_id) DO NOTHING;
            END LOOP;
        END LOOP;
    END LOOP;

    SELECT COUNT(*) INTO v_actual FROM curriculum.plos WHERE program_version_id = v_version_id;
    v_expected := jsonb_array_length(v_data->'program_plos_original');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'PLO count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.program_courses WHERE program_version_id = v_version_id;
    v_expected := jsonb_array_length(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'Course count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.source_course_rows WHERE import_batch_id = v_batch_id;
    v_expected := jsonb_array_length(v_data->'curriculum_map_source_data');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'Raw row count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.course_plos cp
    JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
    WHERE pc.program_version_id = v_version_id;
    SELECT SUM(jsonb_array_length(value->'plo_links')) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'Course-PLO count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.clos c
    JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
    WHERE pc.program_version_id = v_version_id;
    SELECT SUM(jsonb_array_length(value->'clos')) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized');
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'CLO count: % instead of %', v_actual, v_expected; END IF;

    SELECT COUNT(*) INTO v_actual FROM curriculum.clo_plos x
    JOIN curriculum.program_courses pc ON pc.id = x.program_course_id
    WHERE pc.program_version_id = v_version_id;
    SELECT COUNT(*) INTO v_expected
    FROM jsonb_array_elements(v_data->'courses_finalized') AS c(course_data)
    CROSS JOIN LATERAL jsonb_array_elements(c.course_data->'clos') AS o(clo_data)
    CROSS JOIN LATERAL jsonb_array_elements(o.clo_data->'supports_plo') AS p(plo_data);
    IF v_actual <> v_expected THEN RAISE EXCEPTION 'CLO-PLO count: % instead of %', v_actual, v_expected; END IF;
END
$import$;
COMMIT;
